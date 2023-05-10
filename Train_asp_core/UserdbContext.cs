using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Train_asp_core;

public partial class UserdbContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;

    public DbSet<UserState> UsersState { get; set; } = null!;

    public DbSet<UserGroup> UserGroups { get; set; } = null!;

    public UserdbContext()
    {
        Database.EnsureDeleted();
        Database.EnsureCreated();
    }

    public UserdbContext(DbContextOptions<UserdbContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=userdb;Username=postgres;Password=root");
        optionsBuilder.EnableSensitiveDataLogging();

    }

    //protected override void OnModelCreating(ModelBuilder modelBuilder)
    //{
    //    OnModelCreatingPartial(modelBuilder);
    //}

    //partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}


