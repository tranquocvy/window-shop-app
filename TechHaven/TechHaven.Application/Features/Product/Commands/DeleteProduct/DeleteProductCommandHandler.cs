using TechHaven.Application.Interfaces;
using TechHaven.Domain.Interfaces;
using TechHaven.Application.Common.Exceptions;
using MediatR;

namespace TechHaven.Application.Features.Product.Commands.DeleteProduct;

public class DeleteProductCommandHandler : ICommandHandler<DeleteProductCommand>
{
  private readonly IUnitOfWork _unitOfWork;

  public DeleteProductCommandHandler(
    IUnitOfWork unitOfWork)
  {
    _unitOfWork = unitOfWork;
  }

  public async Task<Unit> Handle(
    DeleteProductCommand request,
    CancellationToken cancellationToken)
  {
    var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId, cancellationToken);

    if (product == null)
    {
      throw new NotFoundException(nameof(Domain.Entities.Product), request.ProductId);
    }

    await _unitOfWork.Products.RemoveAsync(product, cancellationToken);
    await _unitOfWork.SaveChangesAsync(cancellationToken);

    return Unit.Value;
  }
}