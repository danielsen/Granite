using System;
using Granite.Core;
using Microsoft.EntityFrameworkCore;

namespace Granite.Infrastructure.Common.Data.Orm
{
    public class Context : DbContext
    {
        public DbSet<Queue> Queues { get; set; }
        public DbSet<QueueItem> QueueItems { get; set; }

        public Context() 
        {
        }
        
        public Context(DbContextOptions<Context> options) : base(options){}

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Queue>()
                .HasKey("Id");

            builder.Entity<Queue>()
                .HasMany(e => e.Items)
                .WithOne(e => e.Queue)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<QueueItem>()
                .HasKey("Id");

            builder.Entity<QueueItem>()
                .HasIndex(e => e.AcknowledgeId);

            builder.Entity<QueueItem>()
                .Property(e => e.ModifiedUtc)
                .IsConcurrencyToken();
        }
    }
}