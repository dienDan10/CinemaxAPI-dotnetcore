﻿
using CinemaxAPI.Data;
using CinemaxAPI.Models.Domain;
using CinemaxAPI.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CinemaxAPI.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly CinemaxServerDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;


        public DbInitializer(CinemaxServerDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public void Initialize()
        {
            // migrations of they are not applied
            try
            {
                if (_context.Database.GetPendingMigrations().Count() > 0)
                    _context.Database.Migrate();
            }
            catch (Exception e)
            {

            }

            // create roles if they are not created
            if (!_roleManager.RoleExistsAsync(Constants.Role_Customer).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(Constants.Role_Customer)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(Constants.Role_Employee)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(Constants.Role_Admin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(Constants.Role_Manager)).GetAwaiter().GetResult();


                // create admin user if it does not exist
                _userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "admin@gmail.com",
                    Email = "admin@gmail.com",
                    DisplayName = "Admin",
                    PhoneNumber = "1234567890",

                }, "Test@123").GetAwaiter().GetResult();

                ApplicationUser user = _context.ApplicationUsers.FirstOrDefault(u => u.Email == "admin@gmail.com");
                _userManager.AddToRoleAsync(user, Constants.Role_Admin).GetAwaiter().GetResult();
            }

            if (!_context.Movies.Any(m => m.Title == "Inception"))
            {
                _context.Movies.Add(new Movie
                {
                    Title = "Inception",
                    Genre = "Sci-Fi",
                    Director = "Christopher Nolan",
                    Cast = "Leonardo DiCaprio",
                    Description = "A mind-bending thriller.",
                    Duration = 148,
                    ReleaseDate = new DateOnly(2010, 7, 16),
                    PosterUrl = null,
                    TrailerUrl = null
                });
            }
            if (!_context.Movies.Any(m => m.Title == "Interstellar"))
            {
                _context.Movies.Add(new Movie
                {
                    Title = "Interstellar",
                    Genre = "Sci-Fi",
                    Director = "Christopher Nolan",
                    Cast = "Matthew McConaughey",
                    Description = "Journey beyond the stars.",
                    Duration = 169,
                    ReleaseDate = new DateOnly(2014, 11, 7),
                    PosterUrl = null,
                    TrailerUrl = null
                });
            }
            _context.SaveChanges();
            return;
        }
    }
}
