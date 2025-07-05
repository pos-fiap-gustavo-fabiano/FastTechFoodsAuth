using FastTechFoodsAuth.Domain.Entities;

namespace FastTechFoodsAuth.Application.Interfaces
{
    public interface IRoleRepository
    {
        Task<Role> GetByIdAsync(Guid id);
        Task<Role> GetByNameAsync(string name);
        Task<IEnumerable<Role>> GetAllAsync();
        Task AddAsync(Role role);
        Task SaveChangesAsync();
    }
}
