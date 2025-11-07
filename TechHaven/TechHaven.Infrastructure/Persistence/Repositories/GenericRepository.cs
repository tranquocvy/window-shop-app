using TechHaven.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace TechHaven.Infrastructure.Persistence.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        // Implementation will be added in the future as needed.
    }
}