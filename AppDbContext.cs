using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace prriva_10
{
    class AppDbContext : DbContext
    {
        public DbSet<Tovars> Tovar { get; set; }
        public DbSet<Prodajas> Prodajas { get; set; }
        public DbSet<Kategors> Kategors { get; set; }
        public DbSet<Izmers> Izmers { get; set; }
        public DbSet<Privozs> Privozs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // для работы в классе (закомментировать дома)
            // optionsBuilder.UseSqlServer("Data Source=DESKTOP-03J9O0A;DataBase=ValeevaTovarDb; Trusted_Connection = True; TrustServerCertificate = True;");
            
            // для чилла, для расслабона (закомментировать в классе)
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=ValeevaTovarDb;Trusted_Connection=true;TrustServerCertificate=true;");
        }
    }
}
