using TechHaven.Application.Interfaces;
using TechHaven.Domain.Interfaces;
using TechHaven.Domain.Common;

namespace TechHaven.Application.Features.Product.Commands.DeleteProduct;

public class DeleteProductCommandHandler : ICommandHandler<DeleteProductCommand, Result>
{
  private readonly IUnitOfWork _unitOfWork;

  public DeleteProductCommandHandler(
    IUnitOfWork unitOfWork)
  {
    _unitOfWork = unitOfWork;
  }

  public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
  {
    try
    {
      var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId, cancellationToken);

      if (product == null)
      {
        return Result.Failure(
          $"Product {request.ProductId} not found.",
          ErrorType.NotFound
        );
      }

      await _unitOfWork.Products.DeleteAsync(product);
      await _unitOfWork.SaveChangesAsync(cancellationToken);

      return Result.Success();
    }
    catch (Exception ex)
    {
      return Result.Failure(
        $"Failed to create product: {ex.Message}",
        ErrorType.InternalError);
    }
  }
}