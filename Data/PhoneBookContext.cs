using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PhoneBook.Data;

public partial class PhoneBookContext : DbContext
{
    public PhoneBookContext()
    {
    }

    public PhoneBookContext(DbContextOptions<PhoneBookContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Contact> Contacts { get; set; }

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//        => optionsBuilder.UseSqlServer("Server=LAPTOP-EU7O01O0\\SQLEXPRESS01;Database=PhoneBookDB_Бобков_2407СА2;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Contact>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Contacts__3214EC072ADD423E");

            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
