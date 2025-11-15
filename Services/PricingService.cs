using System.Threading.Tasks; // Task için
using Microsoft.EntityFrameworkCore; // DbContext için
using System; // DateTime için

// Risk Fiyatlama Motoru (Sizin "AI" dediğiniz yer)
public class PricingService
{
    // Temel Fiyatı 5 TL'ye düşürdük
    private const decimal BASE_PREMIUM = 5.0m; 

    // TEMİNAT 1: Tarih, Yer, Zaman Risk Primi
    // Fiyatları 10'a böldük (örn: 25.0m -> 2.5m)
    public decimal CalculateTier1_LocationTimeRisk(string city, DateTime examDate)
    {
        decimal riskPremium = 0;

        // 1. Şehir Riski
        if (city == "İstanbul" || city == "Ankara" || city == "İzmir")
        {
            riskPremium += 2.5m; // 2.5 TL büyükşehir riski
        }
        else
        {
            riskPremium += 1.0m; // 1 TL diğer şehirler
        }

        // 2. Mevsimsel Risk
        int month = examDate.Month;
        if (month == 12 || month == 1 || month == 2) // Kış ayları
        {
            riskPremium += 3.0m; // 3 TL kış riski
        }

        return riskPremium;
    }

    // TEMİNAT 2: Yaş Risk Primi
    // Fiyatları 10'a böldük (örn: 40.0m -> 4.0m)
    public decimal CalculateTier2_AgeRisk(DateTime dateOfBirth)
    {
        // Yaşı hesapla
        int age = DateTime.Now.Year - dateOfBirth.Year;
        if (DateTime.Now.DayOfYear < dateOfBirth.DayOfYear)
        {
            age--; // Doğum günü henüz gelmemişse
        }

        // Yaşa göre risk belirle
        if (age < 16)
        {
            return 1.0m; // 1 TL (LGS vb.)
        }
        if (age > 40)
        {
            return 4.0m; // 4 TL (Yüksek yaş)
        }
        
        return 2.0m; // 2 TL (Standart)
    }

    // TEMİNAT 3: Sınav Ücreti Baz Primi
    public async Task<decimal> CalculateTier3_ExamFeeRisk(string examCode, InsuranceDbContext db)
    {
        // Veritabanından sınav ücretini bul
        var exam = await db.ExamFees
            .FirstOrDefaultAsync(e => e.ExamCode == examCode);

        if (exam == null)
        {
            return 1.0m; // Sınav bulunamazsa standart 1 TL
        }

        // Sizin isteğiniz: "100 tl lik bir sınav için 1 tl" (%1)
        return exam.FeeAmount * 0.01m;
    }

    // İNDİRİM: Hasarsızlık (Poliçe Adedi)
    // İndirim tutarlarını da güncelledik
    public decimal CalculateDiscount(int policyCount)
    {
        if (policyCount >= 3)
        {
            return 2.0m; // 3+ poliçe için 2 TL indirim
        }
        if (policyCount > 0)
        {
            return 1.0m; // 1-2 poliçe için 1 TL indirim
        }
        
        return 0m; // Hiç poliçesi yoksa indirim yok
    }

    // TEMEL PRİMİ GETİREN YARDIMCI FONKSİYON
    public decimal GetBasePremium()
    {
        return BASE_PREMIUM;
    }
}