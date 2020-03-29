using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.Kestrel.Internal.System.Collections.Sequences;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

using twothor.Data;
using twothor.Models;

namespace twothor.Data
{
    public static class ApplicationDbInitializer
    {
        public static void Initialize(ApplicationDbContext db, UserManager<ApplicationUser> um, RoleManager<IdentityRole> rm, bool isDevelopment)
        {
            if (!isDevelopment)
            {
                db.Database.EnsureCreated();
                return;
            }

            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();


            //Add roles
            var adminRole = new IdentityRole("Admin");
            var userRole = new IdentityRole("User");
            rm.CreateAsync(adminRole).Wait();
            

            //Add users
            var admin = new ApplicationUser { UserName = "admin@student.uia.no", Email = "admin@student.uia.no" };
            um.CreateAsync(admin, "Password1.").Wait();
            um.AddToRoleAsync(admin, "Admin");

            var user = new ApplicationUser { UserName = "susie@student.uia.no", Email = "susie@student.uia.no" };
            um.CreateAsync(user, "Password1.");

            var user1 = new ApplicationUser { UserName = "steve@student.uia.no", Email = "steve@student.uia.no" };
            um.CreateAsync(user1, "Password1.");
            
            var user2 = new ApplicationUser { UserName = "john@student.uia.no", Email = "john@student.uia.no" };
            um.CreateAsync(user2, "Password1.");

            var user3 = new ApplicationUser { UserName = "even@student.uia.no", Email = "even@student.uia.no" };
            um.CreateAsync(user3, "Password1.");

            var user4 = new ApplicationUser { UserName = "adrian@student.uia.no", Email = "adrian@student.uia.no" };
            um.CreateAsync(user4, "Password1.");

            var user5 = new ApplicationUser { UserName = "franziska@student.uia.no", Email = "franziska@student.uia.no" };
            um.CreateAsync(user5, "Password1.");

            var user6 = new ApplicationUser { UserName = "harry@student.uia.no", Email = "harry@student.uia.no" };
            um.CreateAsync(user6, "Password1.");

            var user7 = new ApplicationUser { UserName = "lars@student.uia.no", Email = "lars@student.uia.no" };
            um.CreateAsync(user7, "Password1.");

            var user8 = new ApplicationUser { UserName = "knut@student.uia.no", Email = "knut@student.uia.no" };
            um.CreateAsync(user8, "Password1.");

            var user9 = new ApplicationUser { UserName = "christian@student.uia.no", Email = "christian@student.uia.no" };
            um.CreateAsync(user9, "Password1.");

            var user10 = new ApplicationUser { UserName = "baard@student.uia.no", Email = "baard@student.uia.no" };
            um.CreateAsync(user10, "Password1.");

            // Adding subjects to database
            db.DbSubjectList.Add(new Subject("DAT101  Programmering Grunnkurs"));
            db.DbSubjectList.Add(new Subject("DAT110  Webpublisering"));
            db.DbSubjectList.Add(new Subject("DAT111  Grunnkurs i C-programmering"));
            db.DbSubjectList.Add(new Subject("DAT112  Operativsystemer og mikroprosessorer"));
            db.DbSubjectList.Add(new Subject("DAT113  Softwareutvikling 1"));
            db.DbSubjectList.Add(new Subject("DAT202  Databaser"));
            db.DbSubjectList.Add(new Subject("DAT210  Nettverksdrift 1"));
            db.DbSubjectList.Add(new Subject("DAT211  Nettverk og sikkerhet"));
            db.DbSubjectList.Add(new Subject("DAT217  Nettverksdrift 2"));
            db.DbSubjectList.Add(new Subject("DAT219  Internettjenester"));
            db.DbSubjectList.Add(new Subject("DAT222  Visuell design"));

            db.SaveChanges();
        }
    }
}