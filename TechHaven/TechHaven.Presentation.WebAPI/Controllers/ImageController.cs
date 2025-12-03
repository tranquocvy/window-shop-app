using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TechHaven.Application.Interfaces;
using TechHaven.Shared.DTOs.Common;

namespace TechHaven.Presentation.WebAPI.Controllers;

/// <summary>
/// Controller for handling image uploads
/// </summary>
public class ImageController : BaseApiController
{
  private readonly IImageUploadService _imageUploadService;
  private readonly ILogger<ImageController> _logger;

  public ImageController(
      IImageUploadService imageUploadService,
      ILogger<ImageController> logger)
  {
    _imageUploadService = imageUploadService;
    _logger = logger;
  }

  /// <summary>
  /// Upload a single image
  /// </summary>
  /// <param name="file">Image file to upload</param>
  /// <param name="folder">Optional folder name (e.g., "products", "users")</param>
  /// <returns>Public URL of uploaded image</returns>
  [HttpPost("upload")]
  [ProducesResponseType(typeof(ResponseWrapper<ImageUploadResponse>), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ResponseWrapper<object>), StatusCodes.Status400BadRequest)]
  [RequestSizeLimit(5_242_880)] // 5MB limit
  public async Task<IActionResult> UploadImage(
      IFormFile file,
      [FromQuery] string? folder = null,
      CancellationToken cancellationToken = default)
  {
    try
    {
      if (file == null || file.Length == 0)
      {
        return BadRequest(new ResponseWrapper<object>
        {
          Success = false,
          Message = "No file provided",
          Errors = new List<string> { "File is required" }
        });
      }

      _logger.LogInformation(
          "Uploading image: {FileName} ({FileSize} bytes) to folder: {Folder}",
          file.FileName, file.Length, folder ?? "root");

      // Validate image
      var validation = _imageUploadService.ValidateImage(file.FileName, file.Length);
      if (!validation.IsValid)
      {
        _logger.LogWarning(
            "Image validation failed: {FileName} - {Error}",
            file.FileName, validation.ErrorMessage);

        return BadRequest(new ResponseWrapper<object>
        {
          Success = false,
          Message = "Invalid image file",
          Errors = new List<string> { validation.ErrorMessage }
        });
      }

      // Upload image
      using var stream = file.OpenReadStream();
      var imageUrl = await _imageUploadService.UploadImageAsync(
          stream,
          file.FileName,
          folder,
          cancellationToken);

      _logger.LogInformation(
          "Image uploaded successfully: {FileName} -> {ImageUrl}",
          file.FileName, imageUrl);

      return Ok(new ResponseWrapper<ImageUploadResponse>
      {
        Success = true,
        Message = "Image uploaded successfully",
        Data = new ImageUploadResponse
        {
          ImageUrl = imageUrl,
          FileName = file.FileName,
          FileSize = file.Length
        }
      });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to upload image: {FileName}", file?.FileName);

      return BadRequest(new ResponseWrapper<object>
      {
        Success = false,
        Message = "Failed to upload image",
        Errors = new List<string> { ex.Message }
      });
    }
  }

  /// <summary>
  /// Upload multiple images
  /// </summary>
  /// <param name="files">List of image files to upload</param>
  /// <param name="folder">Optional folder name</param>
  /// <returns>List of public URLs of uploaded images</returns>
  [HttpPost("upload-multiple")]
  [ProducesResponseType(typeof(ResponseWrapper<List<ImageUploadResponse>>), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ResponseWrapper<object>), StatusCodes.Status400BadRequest)]
  [RequestSizeLimit(20_971_520)] // 20MB total limit for multiple files
  public async Task<IActionResult> UploadMultipleImages(
      List<IFormFile> files,
      [FromQuery] string? folder = null,
      CancellationToken cancellationToken = default)
  {
    try
    {
      if (files == null || files.Count == 0)
      {
        return BadRequest(new ResponseWrapper<object>
        {
          Success = false,
          Message = "No files provided",
          Errors = new List<string> { "At least one file is required" }
        });
      }

      if (files.Count > 10)
      {
        return BadRequest(new ResponseWrapper<object>
        {
          Success = false,
          Message = "Too many files",
          Errors = new List<string> { "Maximum 10 files allowed per request" }
        });
      }

      _logger.LogInformation(
          "Uploading {FileCount} images to folder: {Folder}",
          files.Count, folder ?? "root");

      var uploadResults = new List<ImageUploadResponse>();
      var errors = new List<string>();

      foreach (var file in files)
      {
        try
        {
          // Validate each file
          var validation = _imageUploadService.ValidateImage(file.FileName, file.Length);
          if (!validation.IsValid)
          {
            errors.Add($"{file.FileName}: {validation.ErrorMessage}");
            continue;
          }

          // Upload file
          using var stream = file.OpenReadStream();
          var imageUrl = await _imageUploadService.UploadImageAsync(
              stream,
              file.FileName,
              folder,
              cancellationToken);

          uploadResults.Add(new ImageUploadResponse
          {
            ImageUrl = imageUrl,
            FileName = file.FileName,
            FileSize = file.Length
          });
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Failed to upload image: {FileName}", file.FileName);
          errors.Add($"{file.FileName}: {ex.Message}");
        }
      }

      var response = new ResponseWrapper<List<ImageUploadResponse>>
      {
        Success = uploadResults.Count > 0,
        Message = errors.Count == 0
              ? "All images uploaded successfully"
              : $"{uploadResults.Count} of {files.Count} images uploaded",
        Data = uploadResults,
        Errors = errors
      };

      _logger.LogInformation(
          "Multiple upload completed: {SuccessCount}/{TotalCount} successful",
          uploadResults.Count, files.Count);

      return Ok(response);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to upload multiple images");

      return BadRequest(new ResponseWrapper<object>
      {
        Success = false,
        Message = "Failed to upload images",
        Errors = new List<string> { ex.Message }
      });
    }
  }

  /// <summary>
  /// Delete an image
  /// </summary>
  /// <param name="imageUrl">Public URL of the image to delete</param>
  [HttpDelete]
  [ProducesResponseType(typeof(ResponseWrapper<object>), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ResponseWrapper<object>), StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> DeleteImage(
      [FromQuery] string imageUrl,
      CancellationToken cancellationToken = default)
  {
    try
    {
      if (string.IsNullOrWhiteSpace(imageUrl))
      {
        return BadRequest(new ResponseWrapper<object>
        {
          Success = false,
          Message = "Image URL is required",
          Errors = new List<string> { "imageUrl parameter is required" }
        });
      }

      _logger.LogInformation("Deleting image: {ImageUrl}", imageUrl);

      await _imageUploadService.DeleteImageAsync(imageUrl, cancellationToken);

      _logger.LogInformation("Image deleted successfully: {ImageUrl}", imageUrl);

      return Ok(new ResponseWrapper<object>
      {
        Success = true,
        Message = "Image deleted successfully"
      });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to delete image: {ImageUrl}", imageUrl);

      return BadRequest(new ResponseWrapper<object>
      {
        Success = false,
        Message = "Failed to delete image",
        Errors = new List<string> { ex.Message }
      });
    }
  }
}

/// <summary>
/// Response DTO for image upload
/// </summary>
public class ImageUploadResponse
{
  public string ImageUrl { get; set; } = string.Empty;
  public string FileName { get; set; } = string.Empty;
  public long FileSize { get; set; }
}