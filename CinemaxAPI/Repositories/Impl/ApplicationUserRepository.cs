using CinemaxAPI.Data;
using CinemaxAPI.Models.Domain;
using CinemaxAPI.Utils;
using Microsoft.EntityFrameworkCore;

namespace CinemaxAPI.Repositories.Impl
{
    public class ApplicationUserRepository : Repository<ApplicationUser>, IApplicationUserRepository
    {
        public ApplicationUserRepository(CinemaxServerDbContext context) : base(context)
        {
        }

        public async Task<List<ApplicationUser>> GetAllCustomers()
        {
            // get all user with role customer and not employee
            var users = await (
                from user in _context.ApplicationUsers
                where
                    _context.UserRoles.Any(ur => ur.UserId == user.Id && _context.Roles.Any(r => r.Id == ur.RoleId && r.Name == Constants.Role_Customer))
                    &&
                    !_context.UserRoles.Any(ur => ur.UserId == user.Id && _context.Roles.Any(r => r.Id == ur.RoleId && r.Name == Constants.Role_Employee))
                select user
            ).ToListAsync();

            return users;
        }

        public async Task<List<ApplicationUser>> GetAllEmployees(int theaterId)
        {
            // get all user with role employee and theaterId
            var users = await (
                from user in _context.ApplicationUsers
                where
                    _context.UserRoles.Any(ur => ur.UserId == user.Id && _context.Roles.Any(r => r.Id == ur.RoleId && r.Name == Constants.Role_Employee))
                    &&
                    !_context.UserRoles.Any(ur => ur.UserId == user.Id && _context.Roles.Any(r => r.Id == ur.RoleId && r.Name == Constants.Role_Manager))
                    &&
                    user.TheaterId == theaterId
                select user
                ).ToListAsync();

            return users;
        }

        public async Task<List<ApplicationUser>> GetAllManagers()
        {
            // get all user with role manager
            var users = await (
                from user in _context.ApplicationUsers
                where
                    _context.UserRoles.Any(ur => ur.UserId == user.Id && _context.Roles.Any(r => r.Id == ur.RoleId && r.Name == Constants.Role_Manager))
                    &&
                    !_context.UserRoles.Any(ur => ur.UserId == user.Id && _context.Roles.Any(r => r.Id == ur.RoleId && r.Name == Constants.Role_Admin))
                select user
            ).ToListAsync();

            return users;
        }
    }
}
