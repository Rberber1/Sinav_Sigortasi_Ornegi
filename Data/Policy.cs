using System.ComponentModel.DataAnnotations;

// Veritabanındaki 'Policies' tablosunun nasıl görüneceğini
// tanımlayan C# sınıfı (modeli)
public class Policy
{
    [Key]
    public int Id { get; set; } 

    public string PolicyHolder { get; set; } 
    public string StudentName { get; set; }
    public string StudentTC { get; set; }
    public string ExamName { get; set; }
    
    public DateTime ExamDate { get; set; } 
    
    public decimal CoverageAmount { get; set; } // Bu artık Sınav Ücreti olacak
    public decimal PremiumAmount { get; set; } // Bu artık Toplam Hesaplanan Prim olacak
    
    public DateTime DateOfBirth { get; set; }

    // YENİ EKLENEN SÜTUN
    // Poliçenin 'Tether Testnet' üzerindeki (simüle) işlem hash'i
    public string? BlockchainHash { get; set; }
}