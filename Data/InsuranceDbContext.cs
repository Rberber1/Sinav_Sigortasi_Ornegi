using Microsoft.EntityFrameworkCore; 

// Bu bizim veritabanı ile konuşan ana "köprü" sınıfımız
public class InsuranceDbContext : DbContext
{
    // EF Core'a, "Policy" modelimizi kullanarak bir "Policies"
    // adında tablo oluşturmasını söylüyoruz.
    public DbSet<Policy> Policies { get; set; }

    // EF Core'a, "ExamFee" modelimizi kullanarak bir "ExamFees"
    // adında tablo oluşturmasını söylüyoruz.
    public DbSet<ExamFee> ExamFees { get; set; }

    // Bu, 'Program.cs' dosyasından veritabanı ayarlarını
    // (örn: "SQLite kullan") almamızı sağlayan standart koddur.
    
        // YENİ TABLOMUZ
    public DbSet<Claim> Claims { get; set; } // Bu satırı ekleyin
    public InsuranceDbContext(DbContextOptions<InsuranceDbContext> options)
        : base(options)
    {
    }
}