using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DrawebData.Models;

public partial class DrawebDbContext : DbContext
{
    public DrawebDbContext(DbContextOptions<DrawebDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Draw> Draws { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Draw>(entity =>
        {
            entity.HasKey(e => e.DrawId).HasName("PRIMARY");

            entity.Property(e => e.CreationDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.LastUpdate)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.User).WithMany(p => p.Draws)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Draw_ibfk_1");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
