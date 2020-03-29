using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using twothor.Models;

namespace twothor.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Databasen som inneholder all chat. En index per melding
        public DbSet<ChatMessage> DbChatLog { get; set; } 

        // Databasen som inneholder alle 2Thors på siden. En index per 2Thor
        public DbSet<TwoThorProfile> DbTwoThorProfileList { get; set; }

        // Databasen som inneholder alle fag for alle 2Thors. En index per fag per 2Thor. Dvs en 2THor med fem fag har fem indexer i denne databasen
        public DbSet<TwoThorSubjects> DbTwoThorSubjectList { get; set; }

        // Databasen som inneholder listen over fag som tilbys hjelp i
        public DbSet<Subject> DbSubjectList { get; set;}

        // Databasen som inneholder alle jobbene som for øyeblikket er i gang. En index per job. Opptil flere per 2Thor/elev
        public DbSet<TwoThorJob> DbTwoThorJobList { get; set; }

        // Databasen som inneholder alle reviews på siden. Opptil flere per 2THor, per 2THor fag osv.
        public DbSet<TwoThorReview> DbTwoThorReviews { get; set; }
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }
    }
}
