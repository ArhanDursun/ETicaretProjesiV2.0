using ETicaretProjesiV2._0.Common;
using ETicaretProjesiV2._0.Entities;
using ETicaretProjesiV2._0.Enums;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace ETicaretProjesiV2._0.Persistence.Context
{
    public class ApplicationDbContext : IdentityDbContext<AppUser,AppRole,Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<WalletTransaction> WalletTransactions { get; set; }
        public DbSet<PendingUser> PendingUsers { get; set; }
        public DbSet<Offer> Offers { get; set; }
        public DbSet<Basket> Baskets { get; set; }
        public DbSet<BasketItem> BasketItems { get; set; }
        public DbSet<ProductComment> ProductComments { get; set; }
        public DbSet<ProductQuestion> ProductQuestions { get; set; }
        public DbSet<UserNotification> UserNotifications { get; set; }
        public DbSet<SupportTicket> SupportTickets { get; set; }
        public DbSet<TicketMessage> TicketMessages { get; set; }
        public DbSet<DirectMessage> DirectMessages { get; set; }
        public DbSet<UserFavorite> UserFavorites { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Ignore<Enum>();
            builder.Ignore<OrderStatus>();
            builder.Ignore<TransactionType>();
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
            builder.Entity<Category>()
                .HasOne(c=>c.ParentCategory)
                .WithMany(c=>c.SubCategories)
                .HasForeignKey(c=>c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<AppUser>().HasQueryFilter(u => !u.IsDeleted);
            builder.Entity<Product>()
                .Property(p => p.Price).HasPrecision(18,2);
            builder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);
            builder.Entity<Offer>()
                .HasOne(o=>o.Seller).WithMany()
                .HasForeignKey(o=>o.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SupportTicket>().HasOne(t => t.AssignedAdmin).WithMany()
        .HasForeignKey(t => t.AssignedAdminId).OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SupportTicket>().HasOne(t => t.User).WithMany()
        .HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TicketMessage>().HasOne(m => m.SupportTicket).WithMany(t => t.Messages)
                .HasForeignKey(m => m.SupportTicketId).OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TicketMessage>().HasOne(m => m.Sender).WithMany()
                .HasForeignKey(m => m.SenderId).OnDelete(DeleteBehavior.Restrict);

            builder.Entity<DirectMessage>().HasOne(m => m.Sender).WithMany()
                .HasForeignKey(m => m.SenderId).OnDelete(DeleteBehavior.Restrict);

            builder.Entity<DirectMessage>().HasOne(m => m.Receiver).WithMany()
        .HasForeignKey(m => m.ReceiverId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<DirectMessage>().HasQueryFilter(m => !m.IsDeleted);

            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                   
                    var parameter = Expression.Parameter(entityType.ClrType, "x");
                    var property = Expression.Property(parameter, "IsDeleted");
                    var falseConstant = Expression.Constant(false);
                    var compare = Expression.Equal(property, falseConstant);
                    var lambda = Expression.Lambda(compare, parameter);    
                    builder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }
            }

        }
        public override async Task<int> SaveChangesAsync (CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries<BaseEntity>().Where(e => e.State == EntityState.Deleted);

            foreach( var entry in entries)
            {
                entry.State = EntityState.Unchanged;
                entry.Entity.IsDeleted = true;
                entry.Entity.DeletedDate = DateTime.UtcNow;
            }
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
