using FastTechFoodsAuth.Application.Interfaces;
using FastTechFoodsAuth.Domain.Entities;
using FastTechFoodsAuth.Infra.Context;
using Microsoft.EntityFrameworkCore;

namespace FastTechFoodsAuth.Infra.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public RoleRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Role> GetByIdAsync(Guid id)
        {
            return await _dbContext.Roles
                .Include(r => r.UserRoles)
                .ThenInclude(ur => ur.User)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Role> GetByNameAsync(string name)
        {
            return await _dbContext.Roles
                .Include(r => r.UserRoles)
                .ThenInclude(ur => ur.User)
                .FirstOrDefaultAsync(r => r.Name == name);
        }

        public async Task<IEnumerable<Role>> GetAllAsync()
        {
            return await _dbContext.Roles
                .Include(r => r.UserRoles)
                .ThenInclude(ur => ur.User)
                .ToListAsync();
        }

        public async Task AddAsync(Role role)
        {
            await _dbContext.Roles.AddAsync(role);
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
