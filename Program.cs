using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using System;
using Microsoft.EntityFrameworkCore; 
using System.Globalization; // YENİ: Kültür (nokta/virgül) hatasını çözmek için
using Microsoft.AspNetCore.Http; // PDF dosyasını (IFormFile) alabilmek için bu GEREKLİ
using System.Linq; // Raporlama (Join/Select) için bu GEREKLİ


var builder = WebApplication.CreateBuilder(args);

// --- 1. SERVİSLERİ TANIMLAMA ---
var connectionString = "Data Source=sigorta.db";
builder.Services.AddDbContext<InsuranceDbContext>(options =>
    options.UseSqlite(connectionString)
);
builder.Services.AddScoped<PricingService>();

// --- 2. UYGULAMAYI OLUŞTURMA ---
var app = builder.Build();

// --- 3. UYGULAMA AYARLARI ---
var options = new DefaultFilesOptions();
options.DefaultFileNames.Clear();
options.DefaultFileNames.Add("login.html");
app.UseDefaultFiles(options);
app.UseStaticFiles();

// --- 4. API ENDPOINT'LERİ ---

// YARDIMCI API: TCKN İLE GEÇMİŞ POLİÇE ADEDİ GETİRME
app.MapGet("/api/get-policy-history/{tc}", async (string tc, InsuranceDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(tc) || tc.Length < 11)
    {
        return Results.BadRequest(new { message = "Geçersiz T.C. Kimlik Numarası." });
    }
    var oneYearAgo = DateTime.Now.AddYears(-1);
    var policyCount = await db.Policies
        .Where(p => p.StudentTC == tc && p.ExamDate >= oneYearAgo)
        .CountAsync();
    return Results.Ok(new { count = policyCount });
});

// ANA FİYATLAMA MOTORU API'Sİ
app.MapPost("/api/calculate-premium", async (HttpContext context, PricingService pricing, InsuranceDbContext db) =>
{
    var form = await context.Request.ReadFormAsync();
    try
    {
        string examCode = form["examName"];
        string city = form["examCity"];
        // DÜZELTME: Kültür ayarı (nokta/virgül)
        DateTime examDate = DateTime.Parse((string)form["examDate"], CultureInfo.InvariantCulture);
        DateTime dateOfBirth = DateTime.Parse((string)form["dateOfBirth"], CultureInfo.InvariantCulture);
        string studentTC = form["studentTC"];

        // Fiyatlama Motoru Fonksiyonlarını Çağır
        decimal premiumT1 = pricing.CalculateTier1_LocationTimeRisk(city, examDate);
        decimal premiumT2 = pricing.CalculateTier2_AgeRisk(dateOfBirth);
        decimal premiumT3 = await pricing.CalculateTier3_ExamFeeRisk(examCode, db);

        var oneYearAgo = DateTime.Now.AddYears(-1);
        int policyCount = await db.Policies
            .Where(p => p.StudentTC == studentTC && p.ExamDate >= oneYearAgo)
            .CountAsync();
        
        decimal discountAmount = pricing.CalculateDiscount(policyCount);
        decimal basePremium = pricing.GetBasePremium(); 
        decimal totalPremium = (basePremium + premiumT1 + premiumT2 + premiumT3) - discountAmount;
        if (totalPremium < 0) totalPremium = 0;

        var exam = await db.ExamFees.FirstOrDefaultAsync(e => e.ExamCode == examCode);
        decimal coverageAmount = (exam != null) ? exam.FeeAmount : 0m; 

        return Results.Ok(new 
        {
            premiumT1, premiumT2, premiumT3,
            policyCount, discountAmount,
            totalPremium, coverageAmount
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { message = "Hesaplama hatası: Form alanları eksik veya geçersiz. " + ex.Message });
    }
});

// GİRİŞ (LOGIN) İŞLEMİ
app.MapPost("/login", async (HttpContext context) =>
{
    var form = await context.Request.ReadFormAsync();
    string? username = form["username"];
    string? password = form["password"];
    if (username == "admin" && password == "1234")
    {
        Console.WriteLine("Giriş başarılı!");
        context.Response.Redirect("/index.html");
    }
    else
    {
        Console.WriteLine("Giriş başarısız!");
        context.Response.Redirect("/"); 
    }
});

// POLİÇE OLUŞTURMA İŞLEMİ (Blockchain Simülasyonu Eklendi)
app.MapPost("/api/create-policy", async (HttpContext context, InsuranceDbContext db) =>
{
    var form = await context.Request.ReadFormAsync();
    try
    {
        string safePolicyHolder = ((string)form["policyHolder"]).Replace("<", "&lt;").Replace(">", "&gt;");
        string safeStudentName = ((string)form["studentName"]).Replace("<", "&lt;").Replace(">", "&gt;");

        // DÜZELTME: Kültür ayarı (nokta/virgül)
        var culture = CultureInfo.InvariantCulture;
        
        var newPolicy = new Policy
        {
            PolicyHolder = safePolicyHolder,
            StudentName = safeStudentName,
            StudentTC = (string)form["studentTC"],
            ExamName = (string)form["examName"],
            ExamDate = DateTime.Parse((string)form["examDate"], culture),
            DateOfBirth = DateTime.Parse((string)form["dateOfBirth"], culture),
            CoverageAmount = decimal.Parse((string)form["coverageAmount"], culture),
            PremiumAmount = decimal.Parse((string)form["totalPremiumAmount"], culture)
        };

        // 1. Veritabanına Kaydet
        db.Policies.Add(newPolicy);
        await db.SaveChangesAsync();
        
        // KAYITTAN SONRA: newPolicy.Id artık veritabanındaki ID'dir
        int policyId = newPolicy.Id; // Bu bizim Poliçe Numarası
        
        Console.WriteLine($"BAŞARILI: Poliçe #{policyId} ({newPolicy.PolicyHolder}) veritabanına eklendi.");
        
        // 2. Tether Test Ağına Kayıt Simülasyonu
        Console.WriteLine($"Tether test ağına bağlanılıyor (Poliçe No: {policyId})...");
        await Task.Delay(2000); // 2 saniye bekle (blockchain işlemini taklit et)
        string fakeTxHash = $"0x{Guid.NewGuid().ToString("N").Substring(0, 20)}...[TESTNET]";
        Console.WriteLine($"Tether test ağına kayıt başarılı. Hash: {fakeTxHash}");

        // 3. JavaScript'e Yönlendirme YERİNE, JSON ile Başarı Mesajı Döndür
        return Results.Ok(new { 
            message = "Poliçe başarıyla oluşturuldu!",
            policyId = policyId,
            txHash = fakeTxHash 
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"HATA: Poliçe kaydedilemedi! {ex.Message}");
        return Results.BadRequest(new { message = "Poliçe oluşturulamadı. Lütfen tüm alanların dolu olduğundan emin olun." });
    }
});
// === YENİ HASAR OLUŞTURMA API'Sİ (SIMÜLE AI İLE) ===
// Bu, claim.js'in bağlanacağı yerdir
app.MapPost("/api/create-claim", async (HttpContext context, InsuranceDbContext db) =>
{
    // Form verilerini ve dosyayı al
    var form = await context.Request.ReadFormAsync();
    var culture = CultureInfo.InvariantCulture;

    try
    {
        // 1. Girdileri Al
        string studentTC = form["studentTC"];
        string paymentAddress = form["paymentAddress"];
        string reason = form["reason"];
        var documentFile = form.Files["documentUpload"]; // Dosyayı al
        bool aiConfirm = (form["aiConfirmCheckbox"] == "on"); // Checkbox'ı al

        // 2. "Simüle Edilmiş AI" Doğrulaması
        if (documentFile == null || documentFile.Length == 0)
            return Results.BadRequest(new { message = "PDF belge yüklenmesi zorunludur." });
        
        if(documentFile.ContentType != "application/pdf")
            return Results.BadRequest(new { message = "Lütfen sadece PDF formatında bir belge yükleyin." });

        if (!aiConfirm)
            return Results.BadRequest(new { message = "AI Onay kutusunu işaretlemeniz zorunludur." });

        // 3. Poliçeyi Bul
        // Poliçeyi TCKN'ye göre arıyoruz.
        var policy = await db.Policies
            .FirstOrDefaultAsync(p => p.StudentTC == studentTC);

        if (policy == null)
            return Results.BadRequest(new { message = $"Bu TCKN ({studentTC}) ile eşleşen bir poliçe bulunamadı." });
            
        // 4. Hasarı Oluştur (AI onayladı varsayıyoruz)
        Console.WriteLine("AI Onayı başarılı (Checkbox işaretli ve PDF alındı).");
        
        // 5. Ödeme Simülasyonu (Tether/IBAN)
        Console.WriteLine($"Hasar ödemesi ({policy.CoverageAmount} TL) {paymentAddress} adresine gönderiliyor...");
        await Task.Delay(2000); // 2 saniye ödeme simülasyonu
        string paymentTxHash = $"0x{Guid.NewGuid().ToString("N").Substring(0, 30)}...[PAYMENT_SIM]";
        Console.WriteLine($"Ödeme başarılı. Hash: {paymentTxHash}");

        // 6. Hasarı Veritabanına Kaydet
        var newClaim = new Claim
        {
            PolicyId = policy.Id,
            ClaimDate = DateTime.UtcNow,
            Reason = reason,
            Status = "Ödendi", // Otomatik onaylandı
            PaymentAmount = policy.CoverageAmount, // Teminatı poliçeden al
            PaymentAddress = paymentAddress,
            PaymentTxHash = paymentTxHash,
            DocumentName = documentFile.FileName // Sadece dosya adını kaydet
        };

        db.Claims.Add(newClaim);
        await db.SaveChangesAsync();

        int claimId = newClaim.Id; // Yeni Hasar No
        Console.WriteLine($"BAŞARILI: Hasar #{claimId} (Poliçe #{policy.Id}) veritabanına eklendi.");

        // 7. JavaScript'e Başarı Mesajı Döndür
        return Results.Ok(new { 
            message = "Hasar talebi AI tarafından onaylandı ve ödendi!",
            claimId = claimId,
            txHash = paymentTxHash 
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"HATA: Hasar kaydedilemedi! {ex.Message}");
        return Results.BadRequest(new { message = "Hasar talebi oluşturulamadı. Sistem hatası." });
    }
});
// === YENİ RAPORLAMA API'Sİ (ÖDEME MODÜLÜ) ===
// Bu, js/reports.js'in bağlanacağı yerdir
// Lütfen bu bloğu app.Run(); satırının HEMEN ÜSTÜNE yapıştırın.
app.MapGet("/api/get-paid-claims", async (InsuranceDbContext db) =>
{
    try
    {
        // Claims tablosunu Policies tablosu ile "Join" (birleştir)
        // Sadece "Ödendi" statüsündekileri al
        // En son ödeneni en üstte göstermek için 'OrderByDescending'
        var paidClaims = await db.Claims
            .Where(c => c.Status == "Ödendi")
            .Include(c => c.Policy) // İlişkili Policy verisini yükle
            .OrderByDescending(c => c.ClaimDate) // En yeniler en üstte
            .Select(c => new {
                // Hasar tablosundan
                id = c.Id, // Hasar No
                policyId = c.PolicyId, // Poliçe No
                paymentAmount = c.PaymentAmount,
                claimDate = c.ClaimDate,
                paymentAddress = c.PaymentAddress,
                paymentTxHash = c.PaymentTxHash,
                
                // Policy tablosundan (Join ile)
                studentName = c.Policy.StudentName // İlişkili poliçedeki öğrenci adı
            })
            .ToListAsync(); // Listeyi al

        return Results.Ok(paidClaims);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"HATA: Ödeme raporu alınamadı! {ex.Message}");
        // Tarayıcıya (JavaScript'e) 500 hatası döndür
        return Results.Problem("Rapor alınırken bir sunucu hatası oluştu.");
    }
});
// --- 5. ADIM: UYGULAMAYI ÇALIŞTIRMA ---
app.Run();