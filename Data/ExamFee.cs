using System.ComponentModel.DataAnnotations;

// Sınavların temel ücretlerini tutacağımız tablo
public class ExamFee
{
    [Key]
    public int Id { get; set; }

    // Sınavın kısa adı (örn: "YKS", "LGS", "KPSS")
    // Bu, formdaki <select> ile eşleşecek
    public string ExamCode { get; set; }
    
    // Sınavın tam adı (örn: "Yükseköğretim Kurumları Sınavı")
    public string ExamFullName { get; set; }

    // Sizin Teminat 3 için baz alacağımız sınav ücreti
    public decimal FeeAmount { get; set; }
}