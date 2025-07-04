using CinemaxAPI.Data;
using CinemaxAPI.Models.Domain;
using CinemaxAPI.Models.DTO.Requests;
using CinemaxAPI.Utils;
using Microsoft.EntityFrameworkCore;

namespace CinemaxAPI.Repositories.Impl
{
    public class ApplicationUserRepository : Repository<ApplicationUser>, IApplicationUserRepository
    {
        public ApplicationUserRepository(CinemaxServerDbContext context) : base(context)
        {
        }

        public async Task<(IEnumerable<ApplicationUser> customers, int totalCount)> GetAllCustomerAsync(CustomerFilterRequestDTO? filter = null, SortRequestDTO? sort = null, PagedRequestDTO? paged = null)
        {
            // only get user with customer role
            var query = from applicationUser in _context.ApplicationUsers
                        join userRole in _context.UserRoles on applicationUser.Id equals userRole.UserId
                        join role in _context.Roles on userRole.RoleId equals role.Id
                        where role.Name == Constants.Role_Customer
                        select applicationUser;

            // FILTERING
            if (filter != null)
            {
                if (!string.IsNullOrEmpty(filter.Name))
                {
                    query = query.Where(e => e.DisplayName.ToLower().Contains(filter.Name.ToLower()));
                }
                if (!string.IsNullOrEmpty(filter.Email))
                {
                    query = query.Where(e => e.Email.ToLower().Contains(filter.Email.ToLower()));
                }
            }

            // SORTING
            if (sort != null && !string.IsNullOrEmpty(sort.SortBy))
            {
                if (sort.SortBy.ToLower() == "name")
                {
                    query = sort.IsDescending ? query.OrderByDescending(e => e.DisplayName) : query.OrderBy(e => e.DisplayName);
                }
                else if (sort.SortBy.ToLower() == "email")
                {
                    query = !sort.IsDescending ? query.OrderBy(e => e.Email) : query.OrderByDescending(e => e.Email);
                }
            }
            else
            {
                query = query.OrderBy(e => e.DisplayName);
            }

            var totalCount = await query.CountAsync();

            // PAGING
            if (paged != null)
            {
                var pageSize = paged.PageSize > 0 ? paged.PageSize : 10; // Default page size
                var pageNumber = paged.PageNumber > 0 ? paged.PageNumber : 1; // Default page number
                query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            var customers = await query.ToListAsync();

            return (customers, totalCount);
        }

        public async Task<(IEnumerable<ApplicationUser> employees, int totalCount)> GetAllEmployeeAsync(EmployeeFilterRequestDTO? filter = null, SortRequestDTO? sort = null, PagedRequestDTO? paged = null, string? includeProperties = null)
        {
            //var query = dbSet.AsQueryable();

            var query = from applicationUser in _context.ApplicationUsers
                        join userRole in _context.UserRoles on applicationUser.Id equals userRole.UserId
                        join role in _context.Roles on userRole.RoleId equals role.Id
                        where role.Name == Constants.Role_Employee
                        select applicationUser;

            // only get user with employee role

            // FILTERING
            if (filter != null)
            {
                if (!string.IsNullOrEmpty(filter.Name))
                {
                    query = query.Where(e => e.DisplayName.ToLower().Contains(filter.Name.ToLower()));
                }
                if (!string.IsNullOrEmpty(filter.Email))
                {
                    query = query.Where(e => e.Email.ToLower().Contains(filter.Email.ToLower()));
                }
                if (filter.TheaterId.HasValue)
                {
                    query = query.Where(e => e.TheaterId == filter.TheaterId.Value);
                }
            }

            // SORTING
            if (sort != null && !string.IsNullOrEmpty(sort.SortBy))
            {
                if (sort.SortBy.ToLower() == "name")
                {
                    query = sort.IsDescending ? query.OrderByDescending(e => e.DisplayName) : query.OrderBy(e => e.DisplayName);
                }
                else if (sort.SortBy.ToLower() == "email")
                {
                    query = !sort.IsDescending ? query.OrderBy(e => e.Email) : query.OrderByDescending(e => e.Email);
                }
            }
            else
            {
                query = query.OrderBy(e => e.DisplayName);
            }

            var totalCount = await query.CountAsync();

            // PAGING
            if (paged != null)
            {
                var pageSize = paged.PageSize > 0 ? paged.PageSize : 10; // Default page size
                var pageNumber = paged.PageNumber > 0 ? paged.PageNumber : 1; // Default page number
                query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            // INCLUDE PROPERTIES
            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty.Trim());
                }
            }

            var employees = await query.ToListAsync();

            return (employees, totalCount);
        }
    }
}
