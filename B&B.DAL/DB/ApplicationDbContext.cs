using AutoMapper.Execution;
using B_B.DAL.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B_B.DAL.DB
{
    public class ApplicationDBcontext : DbContext

    {

        public ApplicationDBcontext(DbContextOptions<ApplicationDBcontext> options) : base(options)
        {

        }
        public DbSet<Receipt> Receipts { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Plumber> Plumbers { get; set; }
        public DbSet<ReceiptDetail> ReceiptDetails { get; set; }
        public DbSet<StockTransaction> StockTransactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //Plumber
            modelBuilder.Entity<Plumber>(entity =>
            {
                entity.HasKey(p => p.Id);

                entity.Property(p => p.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(p => p.Phone)
                      .HasMaxLength(20);

               
            });

            modelBuilder.Entity<Receipt>(entity =>
            {
                entity.HasOne(or => or.Plumber)
                      .WithMany(p => p.OutReceipts)
                      .HasForeignKey(or => or.PlumberId)
                      .OnDelete(DeleteBehavior.Cascade);
            });


            //Payment
            modelBuilder.Entity<Payment>()
              .HasOne(p => p.Receipt)
              .WithMany(r => r.Payments)
              .HasForeignKey(p => p.ReceiptId)
              .OnDelete(DeleteBehavior.Cascade);

            // Product
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Name).IsRequired().HasMaxLength(150);
            });
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Supplier)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.SupplierId)
                .OnDelete(DeleteBehavior.SetNull);


            // Supplier
            modelBuilder.Entity<Supplier>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Name).IsRequired().HasMaxLength(150);
            });

            // Client
            modelBuilder.Entity<Client>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Name).IsRequired().HasMaxLength(150);
            });

            // Receipt
            modelBuilder.Entity<Receipt>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Date).HasDefaultValueSql("GETDATE()");
                entity.Property(r => r.ReceiptType).IsRequired();

                entity.HasOne(r => r.Supplier)
                      .WithMany(s => s.Receipts)
                      .HasForeignKey(r => r.SupplierId)
                      .OnDelete(DeleteBehavior.Restrict);
        

                entity.HasOne(r => r.Client)
                      .WithMany(c => c.Receipts)
                      .HasForeignKey(r => r.ClientId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<Receipt>()
    .HasMany(r => r.ReceiptDetails)
    .WithOne(d => d.Receipt)
    .HasForeignKey(d => d.ReceiptId);


            // ReceiptDetail
            modelBuilder.Entity<ReceiptDetail>(entity =>
            {
                entity.HasKey(rd => rd.Id);

                entity.HasOne(rd => rd.Receipt)
                      .WithMany(r => r.ReceiptDetails)
                      .HasForeignKey(rd => rd.ReceiptId);

                entity.HasOne(rd => rd.Product)
                      .WithMany(p => p.ReceiptDetails)
                      .HasForeignKey(rd => rd.ProductId);
                
                    entity.Property(rd => rd.DiscountPercentage)
                        .HasDefaultValue(0)
                        .HasPrecision(5, 2); // e.g., 12.50%
               

            });

            // StockTransaction
            modelBuilder.Entity<StockTransaction>(entity =>
            {
                entity.HasKey(st => st.Id);
                entity.Property(st => st.TransactionDate)
                      .HasDefaultValueSql("GETDATE()");

                entity.HasOne(st => st.Product)
                      .WithMany()
                      .HasForeignKey(st => st.ProductId);

                entity.HasOne(st => st.Receipt)
                      .WithMany()
                      .HasForeignKey(st => st.ReceiptId);
            });
        }

    }
}
