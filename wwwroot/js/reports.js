document.addEventListener('DOMContentLoaded', () => {

    const tableBody = document.getElementById('reports-table-body');

    // Sayfa yüklendiğinde C#'tan rapor verilerini çek
    async function fetchReports() {
        try {
            // C#'ta oluşturacağımız YENİ '/api/get-paid-claims' API'sine istek at
            const response = await fetch('/api/get-paid-claims');

            if (!response.ok) {
                tableBody.innerHTML = `<tr><td colspan="7" style="text-align: center; color: red; padding: 2rem;">Rapor yüklenemedi. Sunucu hatası.</td></tr>`;
                return;
            }

            const claims = await response.json();

            if (claims.length === 0) {
                tableBody.innerHTML = `<tr><td colspan="7" style="text-align: center; padding: 2rem;">Gösterilecek ödenmiş hasar kaydı bulunamadı.</td></tr>`;
                return;
            }

            // Tabloyu temizle
            tableBody.innerHTML = '';

            // Gelen her hasar kaydı için bir tablo satırı (<tr>) oluştur
            claims.forEach(claim => {
                const row = document.createElement('tr');
                
                // Tarihi daha güzel bir formata getir (örn: 15.11.2025)
                const claimDate = new Date(claim.claimDate).toLocaleDateString('tr-TR');

                row.innerHTML = `
                    <td>#${claim.id}</td>
                    <td>#${claim.policyId}</td>
                    <td>${claim.studentName}</td>
                    <td>${claim.paymentAmount.toFixed(2)} TL</td>
                    <td>${claimDate}</td>
                    <td>${claim.paymentAddress}</td>
                    <td><span class="hash-cell">${claim.paymentTxHash}</span></td>
                `;
                tableBody.appendChild(row);
            });

        } catch (error) {
            console.error('Rapor yükleme hatası:', error);
            tableBody.innerHTML = `<tr><td colspan="7" style="text-align: center; color: red; padding: 2rem;">Rapor yüklenirken bir hata oluştu.</td></tr>`;
        }
    }

    // Fonksiyonu hemen çağır
    fetchReports();
});