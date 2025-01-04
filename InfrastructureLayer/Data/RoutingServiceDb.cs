using DomainLayer.EntityModels;
using IdentityLayer.IdnetityModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InfrastructureLayer.Data
{
    public class RoutingServiceDb(DbContextOptions<RoutingServiceDb> options) : IdentityDbContext<IdentityUser>(options)
    {

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

        }
        public DbSet<Node>? Nodes { get; set; }
        public DbSet<Edge>? Edges { get; set; }
        public DbSet<RoutingServiceAppUser>? ApplicationUsers { get; set; }
    }
}
