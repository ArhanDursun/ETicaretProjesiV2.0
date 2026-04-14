using ETicaretProjesiV2._0.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Persistence.Configurations
{
    public class WalletTransactionConfiguration : IEntityTypeConfiguration<WalletTransaction>
    {
        public void Configure(EntityTypeBuilder<WalletTransaction> builder)
        {
            builder.Property(wt => wt.Amount).HasPrecision(18, 2);
            builder.Property(wt => wt.Description).HasMaxLength(500);

            builder.HasOne(wt => wt.AppUser).WithMany().HasForeignKey(wt => wt.AppUserId);
        }
    }
}
