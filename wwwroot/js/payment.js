document.addEventListener('DOMContentLoaded', () => {

    const completePaymentButton = document.getElementById('completePaymentButton');
    const totalPrimDisplay = document.getElementById('total-prim-display');
    let policyData = null; 

    // 1. Sayfa yüklendiğinde, hafızadan (localStorage) poliçe verilerini al
    function loadPolicyData() {
        const dataString = localStorage.getItem('policyData');
        
        if (!dataString) {
            alert("Poliçe verisi bulunamadı. Lütfen formu tekrar doldurun.");
            window.location.href = 'policy.html';
            return;
        }

        policyData = JSON.parse(dataString);

        if (policyData.totalPremiumAmount) {
            // DÜZELTME: toFixed(2) ile kuruşları da göster
            let amount = parseFloat(policyData.totalPremiumAmount).toFixed(2);
            totalPrimDisplay.textContent = `${amount} TL`;
        } else {
            totalPrimDisplay.textContent = "Hata!";
        }
    }

    // 2. "Ödemeyi Tamamla" butonuna tıklandığında
    completePaymentButton.addEventListener('click', async () => {
        
        if (!policyData) {
            alert("Poliçe verisi yüklenemedi. Lütfen geri dönüp tekrar deneyin.");
            return;
        }

        completePaymentButton.disabled = true;
        completePaymentButton.textContent = "Ödeme Alınıyor ve Poliçe Oluşturuluyor...";

        // C#'a göndermek için verileri bir FormData nesnesine dönüştür
        const formData = new FormData();
        for (const key in policyData) {
            formData.append(key, policyData[key]);
        }

        try {
            // Güvenli C# API'mize ('/api/create-policy') POST isteği at
            const response = await fetch('/api/create-policy', {
                method: 'POST',
                body: formData
            });

            if (!response.ok) {
                const errorData = await response.json();
                alert(`Poliçe kaydedilemedi: ${errorData.message}`);
                return;
            }

            // DÜZELTME: Başarılı! Yönlendirme yerine JSON cevabını al
            const data = await response.json(); 
            
            // Hafızayı (localStorage) temizle
            localStorage.removeItem('policyData');
            
            // YENİ BAŞARI MESAJI (Poliçe No ve Hash ile)
            alert(`Ödeme başarılı!\n\nPoliçe No: #${data.policyId}\nOluşturuldu ve Tether Test Ağına kaydedildi.\nİşlem Hash: ${data.txHash}`);
            
            // Ana Sayfa'ya yönlendir
            window.location.href = '/index.html';

        } catch (error) {
            console.error('Poliçe oluşturma hatası:', error);
            alert("Sunucuyla iletişim kurulamadı. Poliçe oluşturulamadı.");
        } finally {
            completePaymentButton.disabled = false;
            completePaymentButton.textContent = "Ödemeyi Tamamla ve Poliçeyi Oluştur";
        }
    });

    // Sayfa yüklenir yüklenmez verileri çek
    loadPolicyData();
});