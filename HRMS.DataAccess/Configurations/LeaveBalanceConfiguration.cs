using HRMS.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.DataAccess.Configurations;

public class LeaveBalanceConfiguration : IEntityTypeConfiguration<LeaveBalance>
{
    public void Configure(EntityTypeBuilder<LeaveBalance> builder)
    {
        builder.ToTable("LeaveBalances");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id)
            .ValueGeneratedOnAdd();

        builder.Property(l => l.EmpNo)
            .IsRequired()
            .HasMaxLength(32);

        builder.HasIndex(l => l.EmpNo)
            .IsUnique();

        builder.Property(l => l.Annual)
            .IsRequired();

        builder.Property(l => l.Sick)
            .IsRequired();

        builder.Property(l => l.Unpaid)
            .IsRequired();
    }
}
