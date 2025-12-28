using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Supabase;
using TechHaven.Application.Interfaces;
using TechHaven.Infrastructure.Configuration;

namespace TechHaven.Infrastructure.Services;

public class SupabaseImageUploadService : IImageUploadService
{
  private readonly ILogger<SupabaseImageUploadService> _logger;
  private readonly SupabaseSettings _settings;
  private readonly Client _supabaseClient;

  public SupabaseImageUploadService(
      ILogger<SupabaseImageUploadService> logger,
      IOptions<SupabaseSettings> settings)
  {
    _logger = logger;
    _settings = settings.Value;

    // Initialize Supabase client
    var options = new Supabase.SupabaseOptions
    {
      AutoConnectRealtime = false
    };

    _supabaseClient = new Client(
        _settings.Url,
        _settings.ApiKey,
        options);
  }

  public async Task<string> UploadImageAsync(
      Stream fileStream,
      string fileName,
      string? folder = null,
      CancellationToken cancellationToken = default)
  {
    try
    {
      _logger.LogInformation(
          "Uploading image: {FileName} to folder: {Folder}",
          fileName, folder ?? "root");

      // Validate file
      var validation = ValidateImage(fileName, fileStream.Length);
      if (!validation.IsValid)
      {
        throw new InvalidOperationException(validation.ErrorMessage);
      }

      // Generate unique file name to avoid conflicts
      var fileExtension = Path.GetExtension(fileName);
      var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";

      // Build storage path
      var storagePath = string.IsNullOrEmpty(folder)
          ? uniqueFileName
          : $"{folder}/{uniqueFileName}";

      _logger.LogDebug("Storage path: {StoragePath}", storagePath);

      // Convert stream to byte array
      using var memoryStream = new MemoryStream();
      await fileStream.CopyToAsync(memoryStream, cancellationToken);
      var fileBytes = memoryStream.ToArray();

      // Upload to Supabase Storage
      var result = await _supabaseClient.Storage
          .From(_settings.BucketName)
          .Upload(fileBytes, storagePath);

      if (string.IsNullOrEmpty(result))
      {
        throw new InvalidOperationException("Upload failed: No URL returned from Supabase");
      }

      // Get public URL
      var publicUrl = _supabaseClient.Storage
          .From(_settings.BucketName)
          .GetPublicUrl(storagePath);

      _logger.LogInformation(
          "Image uploaded successfully: {FileName} -> {PublicUrl}",
          fileName, publicUrl);

      return publicUrl;
    }
    catch (Exception ex)
    {
      _logger.LogError(
          ex,
          "Failed to upload image: {FileName}",
          fileName);
      throw;
    }
  }

  public async Task DeleteImageAsync(
      string imageUrl,
      CancellationToken cancellationToken = default)
  {
    try
    {
      _logger.LogInformation("Deleting image: {ImageUrl}", imageUrl);

      // Extract file path from URL
      var uri = new Uri(imageUrl);
      var pathSegments = uri.AbsolutePath.Split('/');

      // Find the bucket name and get everything after it
      var bucketIndex = Array.IndexOf(pathSegments, _settings.BucketName);
      if (bucketIndex < 0 || bucketIndex >= pathSegments.Length - 1)
      {
        throw new InvalidOperationException("Invalid image URL format");
      }

      var filePath = string.Join("/", pathSegments.Skip(bucketIndex + 1));

      _logger.LogDebug("Extracted file path: {FilePath}", filePath);

      // Delete from Supabase Storage
      await _supabaseClient.Storage
          .From(_settings.BucketName)
          .Remove(new List<string> { filePath });

      _logger.LogInformation("Image deleted successfully: {ImageUrl}", imageUrl);
    }
    catch (Exception ex)
    {
      _logger.LogError(
          ex,
          "Failed to delete image: {ImageUrl}",
          imageUrl);
      throw;
    }
  }

  public (bool IsValid, string ErrorMessage) ValidateImage(string fileName, long fileSize)
  {
    // Check file size
    if (fileSize > _settings.MaxFileSizeBytes)
    {
      var maxSizeMB = _settings.MaxFileSizeBytes / (1024.0 * 1024.0);
      return (false, $"File size exceeds maximum allowed size of {maxSizeMB:F2}MB");
    }

    // Check file extension
    var extension = Path.GetExtension(fileName).ToLowerInvariant();
    if (!_settings.AllowedExtensions.Contains(extension))
    {
      var allowed = string.Join(", ", _settings.AllowedExtensions);
      return (false, $"File type not allowed. Allowed types: {allowed}");
    }

    return (true, string.Empty);
  }
}