using ApplicationCore.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options)
           : base(options)
        {
        }
        public DbSet<user> users { get; set; }
      

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
             base.OnModelCreating(modelBuilder);
            // modelBuilder.Entity<user>().ToTable("users");
        }
    }
}
