using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nimble.Modulith.Reporting.Models;

namespace Nimble.Modulith.Reporting.Data.Config;

public class DimDateConfig : IEntityTypeConfiguration<DimDate>
{
    public void Configure(EntityTypeBuilder<DimDate> builder)
    {
        builder.ToTable("DimDate");
        builder.HasKey(d => d.DateKey);
        
        builder.Property(d => d.DateKey)
            .ValueGeneratedNever(); // DateKey is YYYYMMDD format

        builder.Property(d => d.DayName).HasMaxLength(20);
        builder.Property(d => d.MonthName).HasMaxLength(20);

        // Seed all dates from 2024 to 2030
        var dates = new List<DimDate>();
        for (int year = 2024; year <= 2030; year++)
        {
            dates.AddRange(GenerateDateDimension(year));
        }
        builder.HasData(dates);
    }

    private static List<DimDate> GenerateDateDimension(int year)
    {
        var dates = new List<DimDate>();
        var startDate = new DateTime(year, 1, 1);
        var endDate = new DateTime(year, 12, 31);
        
        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            dates.Add(new DimDate
            {
                DateKey = int.Parse(date.ToString("yyyyMMdd")),
                Date = date,
                Year = date.Year,
                Quarter = (date.Month - 1) / 3 + 1,
                Month = date.Month,
                Day = date.Day,
                DayOfWeek = (int)date.DayOfWeek,
                DayName = date.DayOfWeek.ToString(),
                MonthName = date.ToString("MMMM")
            });
        }
        
        return dates;
    }
}
