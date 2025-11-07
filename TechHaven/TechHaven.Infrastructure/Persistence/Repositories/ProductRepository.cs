using TechHaven.Domain.Entities;
using TechHaven.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace TechHaven.Infrastructure.Persistence.Repositories
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        // Implementation will be added in the future as needed.
    }
}