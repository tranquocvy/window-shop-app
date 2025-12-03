namespace TechHaven.Application.Interfaces;

/// <summary>
/// Service for handling image uploads to cloud storage.
/// </summary>
public interface IImageUploadService
{
  /// <summary>
  /// Upload an image and return the public URL.
  /// </summary>
  /// <param name="file">The image file to upload</param>
  /// <param name="folder">Optional folder path in storage (e.g., "products", "users")</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>Public URL of the uploaded image</returns>
  Task<string> UploadImageAsync(
      Stream fileStream,
      string fileName,
      string? folder = null,
      CancellationToken cancellationToken = default);

  /// <summary>
  /// Delete an image from storage.
  /// </summary>
  /// <param name="imageUrl">The public URL of the image to delete</param>
  /// <param name="cancellationToken">Cancellation token</param>
  Task DeleteImageAsync(
      string imageUrl,
      CancellationToken cancellationToken = default);

  /// <summary>
  /// Validate image file before upload.
  /// </summary>
  /// <param name="fileName">Name of the file</param>
  /// <param name="fileSize">Size of the file in bytes</param>
  /// <returns>True if valid, false otherwise</returns>
  (bool IsValid, string ErrorMessage) ValidateImage(string fileName, long fileSize);
}