using ETicaretProjesiV2._0.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Persistence.Configurations
{
    public static class UserAndRoleSeeder
    {
       
        public static async Task SeedAsync(RoleManager<AppRole> roleManager, UserManager<AppUser> userManager)
        {
            
            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new AppRole { Name = "Admin" });

            if (!await roleManager.RoleExistsAsync("User"))
                await roleManager.CreateAsync(new AppRole { Name = "User" });

            
            var adminEmail = "admin@brandcorner.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var newAdmin = new AppUser
                {
                    UserName = "admin",
                    Email = adminEmail,
                    FirstName = "Sistem",
                    LastName = "Yöneticisi",
                    EmailConfirmed = true,
                    Balance = 999999
                };

                
                var result = await userManager.CreateAsync(newAdmin, "Admin123!");

                if (result.Succeeded)
                {
                   
                    await userManager.AddToRoleAsync(newAdmin, "Admin");
                }
            }

            var secondAdminEmail = "admin@arhan.com"; 
            var secondAdmin = await userManager.FindByEmailAsync(secondAdminEmail);

            if (secondAdmin == null)
            {
                var newSecondAdmin = new AppUser
                {
                    UserName = "AdminArhan", 
                    Email = secondAdminEmail,
                    FirstName = "Arhan",
                    LastName = "Admin",
                    EmailConfirmed = true,
                    Balance = 5000000 
                };

                var result = await userManager.CreateAsync(newSecondAdmin, "Admin123!"); 

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newSecondAdmin, "Admin");
                }
            }
        }
    }
}
