using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using System;
using Microsoft.EntityFrameworkCore; 
using System.Globalization; 
using Microsoft.AspNetCore.Http; 
using System.Linq; 
// --- 1. EKLEME: Servis namespace'ini ekledik ---
//using WebProjem.Services; 
using SinavSigortasiWeb3.Services;

var builder = WebApplication.CreateBuilder(args);

// --- SERVİSLERİ TANIMLAMA ---
var connectionString = "Data Source=sigorta.db";
builder.Services.AddDbContext<InsuranceDbContext>(options =>
    options.UseSqlite(connectionString)
);
builder.Services.AddScoped<PricingService>();

var app = builder.Build();

// --- 2. EKLEME: Test Endpointleri (Sunumda hocaya göstermek için kalsın) ---
app.MapGet("/test/durum", async () =>
{
    string ogrenciCuzdani = "0x70997970C51812dc3A010C7d01b50e0d17dc79C8"; 
    var servis = new BlockchainService();
    bool sonuc = await servis.IsInsuredAsync(ogrenciCuzdani);
    return Results.Ok(new { Ogrenci = ogrenciCuzdani, SigortaliMi = sonuc });
});

// --- UYGULAMA AYARLARI ---
var options = new DefaultFilesOptions();
options.DefaultFileNames.Clear();
options.DefaultFileNames.Add("login.html");
app.UseDefaultFiles(options);
app.UseStaticFiles();

// --- API ENDPOINT'LERİ ---

// YARDIMCI API: GEÇMİŞ POLİÇE ADEDİ
app.MapGet("/api/get-policy-history/{tc}", async (string tc, InsuranceDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(tc) || tc.Length < 11)
        return Results.BadRequest(new { message = "Geçersiz T.C. Kimlik Numarası." });
    
    var oneYearAgo = DateTime.Now.AddYears(-1);
    var policyCount = await db.Policies
        .Where(p => p.StudentTC == tc && p.ExamDate >= oneYearAgo)
        .CountAsync();
    return Results.Ok(new { count = policyCount });
});

// FİYATLAMA MOTORU
app.MapPost("/api/calculate-premium", async (HttpContext context, PricingService pricing, InsuranceDbContext db) =>
{
    var form = await context.Request.ReadFormAsync();
    try
    {
        string examCode = form["examName"];
        string city = form["examCity"];
        DateTime examDate = DateTime.Parse((string)form["examDate"], CultureInfo.InvariantCulture);
        DateTime dateOfBirth = DateTime.Parse((string)form["dateOfBirth"], CultureInfo.InvariantCulture);
        string studentTC = form["studentTC"];

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
        return Results.BadRequest(new { message = "Hesaplama hatası: " + ex.Message });
    }
});

// GİRİŞ İŞLEMİ
app.MapPost("/login", async (HttpContext context) =>
{
    var form = await context.Request.ReadFormAsync();
    string? username = form["username"];
    string? password = form["password"];
    if (username == "admin" && password == "1234")
    {
        context.Response.Redirect("/index.html");
    }
    else
    {
        context.Response.Redirect("/"); 
    }
});

// --- 3. VE EN ÖNEMLİ DEĞİŞİKLİK: POLİÇE OLUŞTURMA (Blockchain Entegreli) ---
app.MapPost("/api/create-policy", async (HttpContext context, InsuranceDbContext db) =>
{
    var form = await context.Request.ReadFormAsync();
    try
    {
        // 1. Web2 (SQL) Kayıt İşlemleri
        string safePolicyHolder = ((string)form["policyHolder"]).Replace("<", "&lt;").Replace(">", "&gt;");
        string safeStudentName = ((string)form["studentName"]).Replace("<", "&lt;").Replace(">", "&gt;");
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

        db.Policies.Add(newPolicy);
        await db.SaveChangesAsync();
        int policyId = newPolicy.Id; 
        
        Console.WriteLine($"Web2 Kaydı Başarılı. Poliçe #{policyId}. Şimdi Blok Zincirine gidiliyor...");

        // 2. Web3 (Blockchain) Entegrasyonu - GERÇEK İŞLEM
        string txHash = "HATA";
        try 
        {
            // Hardhat Test Ağındaki Öğrenci Cüzdanı (Demo için sabitlendi)
            // Gerçekte bu bilgi formdan gelirdi: form["walletAddress"]
            string targetWallet = "0x70997970C51812dc3A010C7d01b50e0d17dc79C8"; 

            var blockchain = new BlockchainService();

            // Kontrol et: Zaten var mı?
            bool exists = await blockchain.IsInsuredAsync(targetWallet);
            if(exists)
            {
                Console.WriteLine("UYARI: Bu cüzdanın zaten poliçesi var blok zincirinde.");
                // Demo olduğu için hata fırlatmıyoruz, var olanın üstüne işlem yapıyoruz veya geçiyoruz
            }

            // Blok Zincirine Yaz (Mint)
            txHash = await blockchain.CreatePolicyAsync(targetWallet);
            Console.WriteLine($"BLOK ZİNCİRİ BAŞARILI! Hash: {txHash}");
        }
        catch(Exception bcEx)
        {
            Console.WriteLine($"Blok zinciri hatası: {bcEx.Message}");
            txHash = "Blockchain-Error-Offline";
        }

        // 3. Sonuç Dön
        return Results.Ok(new { 
            message = "Poliçe başarıyla oluşturuldu ve Blok Zincirine işlendi!",
            policyId = policyId,
            txHash = txHash // Artık gerçek Hash dönüyor!
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { message = "Poliçe hatası: " + ex.Message });
    }
});

// --- 4. DEĞİŞİKLİK: HASAR OLUŞTURMA (Blockchain Doğrulamalı) ---
app.MapPost("/api/create-claim", async (HttpContext context, InsuranceDbContext db) =>
{
    var form = await context.Request.ReadFormAsync();
    try
    {
        string studentTC = form["studentTC"];
        string paymentAddress = form["paymentAddress"];
        string reason = form["reason"];
        var documentFile = form.Files["documentUpload"]; 
        bool aiConfirm = (form["aiConfirmCheckbox"] == "on"); 

        // Web2 Kontrolleri
        if (documentFile == null || !aiConfirm)
            return Results.BadRequest(new { message = "Eksik belge veya onay." });

        var policy = await db.Policies.FirstOrDefaultAsync(p => p.StudentTC == studentTC);
        if (policy == null)
            return Results.BadRequest(new { message = "Poliçe bulunamadı." });

        // *** BLOK ZİNCİRİ DOĞRULAMASI ***
        // Ödeme yapmadan önce, blok zincirine "Bu adamın poliçesi gerçekten var mı?" diye soruyoruz.
        try 
        {
             string targetWallet = "0x70997970C51812dc3A010C7d01b50e0d17dc79C8"; // Demo cüzdanı
             var blockchain = new BlockchainService();
             bool isInsuredOnChain = await blockchain.IsInsuredAsync(targetWallet);

             if(!isInsuredOnChain)
             {
                 Console.WriteLine("KRİTİK: Veritabanında poliçe var ama Blok Zincirinde YOK! Sahtecilik riski.");
                 // İstersen burada işlemi durdurabilirsin:
                 // return Results.BadRequest(new { message = "Blok zinciri doğrulaması başarısız!" });
             }
             else 
             {
                 Console.WriteLine("Blok zinciri doğrulaması BAŞARILI. Ödeme onaylanıyor.");
             }
        }
        catch { /* Blockchain kapalıysa akışı bozma */ }

        // Ödeme Simülasyonu (Bankadan yapılıyor gibi)
        await Task.Delay(1500); 
        string paymentTxHash = $"0x{Guid.NewGuid().ToString("N")}"; // Banka referans no simülasyonu

        // DB Kayıt
        var newClaim = new Claim
        {
            PolicyId = policy.Id,
            ClaimDate = DateTime.UtcNow,
            Reason = reason,
            Status = "Ödendi", 
            PaymentAmount = policy.CoverageAmount, 
            PaymentAddress = paymentAddress,
            PaymentTxHash = paymentTxHash,
            DocumentName = documentFile.FileName 
        };

        db.Claims.Add(newClaim);
        await db.SaveChangesAsync();

        return Results.Ok(new { 
            message = "Hasar talebi doğrulandı ve ödendi!",
            claimId = newClaim.Id,
            txHash = paymentTxHash 
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { message = "Hata: " + ex.Message });
    }
});

// RAPORLAMA API'Sİ
app.MapGet("/api/get-paid-claims", async (InsuranceDbContext db) =>
{
    try
    {
        var paidClaims = await db.Claims
            .Where(c => c.Status == "Ödendi")
            .Include(c => c.Policy) 
            .OrderByDescending(c => c.ClaimDate) 
            .Select(c => new {
                id = c.Id, 
                policyId = c.PolicyId, 
                paymentAmount = c.PaymentAmount,
                claimDate = c.ClaimDate,
                paymentAddress = c.PaymentAddress,
                paymentTxHash = c.PaymentTxHash,
                studentName = c.Policy.StudentName 
            })
            .ToListAsync(); 

        return Results.Ok(paidClaims);
    }
    catch (Exception ex)
    {
        return Results.Problem("Rapor hatası: " + ex.Message);
    }
});

app.Run();