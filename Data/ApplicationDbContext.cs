using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ServiceManager.Models;

namespace ServiceManager.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, AppRole, int>
    {
        
        
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
           
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {

           
            base.OnModelCreating(builder);

            builder.Entity<MetaData>();
            builder.Entity<ApplicationUser>().Ignore(e => e.FullName);
           


        builder.Entity<Profession>()
                .HasData(
                    new Profession { Id = 1, Skill = "Welder"},
                    new Profession { Id = 2, Skill = "Brick Layer"},
                    new Profession { Id = 3, Skill = "Electrician" },
                    new Profession { Id = 4, Skill = "Hardwood Floor Installer" },
                    new Profession { Id = 5, Skill = "Tile Installer" },
                    new Profession { Id = 6, Skill = "Plumber" },
                    new Profession { Id = 7, Skill = "Drywall Installer" },
                    new Profession { Id = 8, Skill = "Insulation Installer" },
                    new Profession { Id = 9, Skill = "Kitchen Cabinet Installer" },
                    new Profession { Id = 10, Skill = "Framer" }
                );







        }

        public DbSet<ServiceManager.Models.Profession> Profession { get; set; }

        public DbSet<ServiceManager.Models.WorkOrder> WorkOrder { get; set; }

        public DbSet<ServiceManager.Models.MetaData> MetaData { get; set; }
    }
}
