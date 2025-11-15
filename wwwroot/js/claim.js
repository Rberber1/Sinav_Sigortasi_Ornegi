document.addEventListener('DOMContentLoaded', () => {

    const createClaimButton = document.getElementById('createClaimButton');
    const claimForm = document.getElementById('claimForm');

    // "Hasar Talebini Gönder" butonuna tıklandığında
    createClaimButton.addEventListener('click', async () => {

        // Formdaki tüm verileri al (PDF dosyası dahil)
        const formData = new FormData(claimForm);

        // Alanların boş olup olmadığını basitçe kontrol et
        const studentTC = formData.get('studentTC');
        const paymentAddress = formData.get('paymentAddress');
        const reason = formData.get('reason');
        const documentFile = formData.get('documentUpload');
        const aiConfirm = formData.get('aiConfirmCheckbox');

        if (!studentTC || !paymentAddress || !reason || !documentFile || !aiConfirm) {
            alert("Lütfen tüm zorunlu alanları doldurun ve AI onay kutusunu işaretleyin.");
            return;
        }

        if (documentFile.type !== "application/pdf") {
             alert("Lütfen sadece PDF formatında bir belge yükleyin.");
             return;
        }

        createClaimButton.disabled = true;
        createClaimButton.textContent = "AI Analizi Yapılıyor ve Ödeme Simüle Ediliyor...";

        try {
            // C#'ta oluşturacağımız YENİ '/api/create-claim' API'sine 'POST' isteği at
            const response = await fetch('/api/create-claim', {
                method: 'POST',
                body: formData 
                // Not: 'enctype=multipart/form-data' olduğu için 'headers' gerekmez
            });

            if (!response.ok) {
                // C#'tan bir hata dönerse (örn: Poliçe bulunamadı)
                const errorData = await response.json();
                alert(`Hasar talebi oluşturulamadı: ${errorData.message}`);
                return;
            }

            // Başarılı! C#'tan JSON cevabını al
            const data = await response.json(); 
            
            // Başarı mesajı göster (Hasar No ve Sahte Hash ile)
            alert(`Hasar Talebi Başarılı!\n\nHasar No: #${data.claimId}\nDurum: Ödendi\nÖdeme (Simüle) İşlem Hash: ${data.txHash}`);
            
            // Ana Sayfa'ya yönlendir
            window.location.href = '/index.html';

        } catch (error) {
            console.error('Hasar oluşturma hatası:', error);
            alert("Sunucuyla iletişim kurulamadı. Hasar talebi oluşturulamadı.");
        } finally {
            createClaimButton.disabled = false;
            createClaimButton.textContent = "Hasar Talebini Gönder ve AI Analizini Başlat";
        }
    });
});