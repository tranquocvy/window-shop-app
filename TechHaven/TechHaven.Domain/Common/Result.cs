namespace TechHaven.Domain.Common;

public class Result
{
  public bool IsSuccess { get; }
  public bool IsFailure => !IsSuccess;
  public string? ErrorMessage { get; }
  public string? ErrorCode { get; }
  public List<string> Errors { get; }

  protected Result(bool isSuccess, string? errorMessage = null, string? errorCode = null)
  {
    IsSuccess = isSuccess;
    ErrorMessage = errorMessage;
    ErrorCode = errorCode;
    Errors = new List<string>();
  }

  // Factory methods
  public static Result Success() => new(true); // IsSuccess = true

  public static Result Failure(string errorMessage, string? errorCode = null)
    => new(false, errorMessage, errorCode); // IsSuccess = false

  public static Result Failure(List<string> errors)
  {
    var result = new Result(false);
    result.Errors.AddRange(errors);
    return result;
  }

  // Implicit conversion từ bool
  public static implicit operator Result(bool success)
      => success ? Success() : Failure("Operation failed");
}

// Generic Result with data
public class Result<T> : Result
{
  public T? Data { get; }

  protected Result(T? data, bool isSuccess, string? errorMessage = null, string? errorCode = null)
    : base(isSuccess, errorMessage, errorCode)
  {
    Data = data;
  }

  // Factory methods
  public static Result<T> Success(T data) => new(data, true);

  public static new Result<T> Failure(string errorMessage, string? errorCode = null)
    => new(default, false, errorMessage, errorCode);

  public static new Result<T> Failure(List<string> errors)
  {
    var result = new Result<T>(default, false);
    result.Errors.AddRange(errors);
    return result;
  }

  // Implicit conversion từ T
  public static implicit operator Result<T>(T data)
    => Success(data);
}