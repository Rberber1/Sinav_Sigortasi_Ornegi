using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Hasar kayıtlarımızı tutacağımız tablo
public class Claim
{
    [Key]
    public int Id { get; set; } // Hasar Numarası

    // Hangi poliçeye bağlı olduğunu gösteren 'Foreign Key'
    [ForeignKey("Policy")]
    public int PolicyId { get; set; }
    public Policy Policy { get; set; } // İlişkiyi kurar

    public DateTime ClaimDate { get; set; } // Hasar talebinin oluşturulduğu tarih
    public string Reason { get; set; } // Hasar nedeni (örn: "Ameliyat")
    
    // Hasar Durumu: "Onay Bekliyor", "Onaylandı", "Ödendi", "Reddedildi"
    public string Status { get; set; } 
    
    // Ödeme Bilgileri
    public decimal PaymentAmount { get; set; } // Ödenen tutar (poliçenin teminatı)
    public string PaymentAddress { get; set; } // IBAN veya Cüzdan Adresi
    public string? PaymentTxHash { get; set; } // Ödeme işlemi (Tether/IBAN) hash'i
    
    // Yüklenen belgenin adı (Güvenlik için sadece adını saklıyoruz)
    public string DocumentName { get; set; }
}