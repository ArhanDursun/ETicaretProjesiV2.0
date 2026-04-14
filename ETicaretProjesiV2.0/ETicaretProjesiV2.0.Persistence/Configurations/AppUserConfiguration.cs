using ETicaretProjesiV2._0.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace ETicaretProjesiV2._0.Persistence.Configurations
{
    public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
    {
        public void Configure(EntityTypeBuilder<AppUser> builder) {
            builder.Property(u => u.Balance).HasPrecision(18, 2).HasDefaultValue(0);

            builder.Property(u => u.FirstName).HasMaxLength(50);
            builder.Property(u => u.LastName).HasMaxLength(50);
        }

    }
}
