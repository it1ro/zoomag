using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace prriva_10
{
    public class AppDbContext : DbContext // <-- Сделали public
    {
        public DbSet<Tovars> Tovar { get; set; }
        public DbSet<Prodajas> Prodajas { get; set; }
        public DbSet<Kategors> Kategors { get; set; }
        public DbSet<Izmers> Izmers { get; set; }
        public DbSet<Privozs> Privozs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=ValeevaTovarDb;Trusted_Connection=true;TrustServerCertificate=true;");
        }
    }
}