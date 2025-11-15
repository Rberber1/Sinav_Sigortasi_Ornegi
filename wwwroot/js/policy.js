document.addEventListener('DOMContentLoaded', () => {
    
    // --- 1. Elementleri Seçme ---
    const nextButton = document.getElementById('nextButton');
    const prevButton = document.getElementById('prevButton');
    const createPolicyButton = document.getElementById('createPolicyButton');
    const calculateButton = document.getElementById('calculateButton');

    const formSteps = document.querySelectorAll('.form-step');
    const stepperSteps = document.querySelectorAll('.step');
    
    const policyForm = document.getElementById('policyForm');
    const studentTCInput = document.getElementById('studentTC');
    const policyCountInput = document.getElementById('policyCount');
    
    let currentStep = 1;

    // --- 2. Adımlayıcı (Stepper) Fonksiyonları ---
    nextButton.addEventListener('click', () => {
        if (currentStep < formSteps.length) {
            currentStep++;
            updateFormStep();
        }
    });
    prevButton.addEventListener('click', () => {
        if (currentStep > 1) {
            currentStep--;
            updateFormStep();
        }
    });

    function updateFormStep() {
        formSteps.forEach(step => step.classList.remove('active'));
        formSteps[currentStep - 1].classList.add('active');
        stepperSteps.forEach((step, index) => {
            if (index < currentStep - 1) {
                step.classList.add('completed');
                step.classList.remove('active');
            } else if (index === currentStep - 1) {
                step.classList.remove('completed');
                step.classList.add('active');
            } else {
                step.classList.remove('completed');
                step.classList.remove('active');
            }
        });
        updateButtons();
    }

    function updateButtons() {
        calculateButton.style.display = 'none';
        createPolicyButton.style.display = 'none';
        if (currentStep === 1) {
            prevButton.style.display = 'none';
            nextButton.style.display = 'block';
        } else if (currentStep === formSteps.length) {
            prevButton.style.display = 'block';
            nextButton.style.display = 'none';
            calculateButton.style.display = 'block';
            createPolicyButton.style.display = 'block';
        } else {
            prevButton.style.display = 'block';
            nextButton.style.display = 'block';
        }
    }

    // --- 3. FİYATLAMA MOTORU BAĞLANTILARI ---

    // a) TCKN Alanından Çıkıldığında (blur) Geçmişi Sorgula
    studentTCInput.addEventListener('blur', async () => {
        const tc = studentTCInput.value;
        if (!tc || tc.length < 11) {
            policyCountInput.value = 0;
            return;
        }
        policyCountInput.value = '';
        policyCountInput.placeholder = "Sorgulanıyor...";
        try {
            const response = await fetch(`/api/get-policy-history/${tc}`);
            if (!response.ok) {
                policyCountInput.placeholder = "Hata oluştu";
                return;
            }
            const data = await response.json();
            policyCountInput.value = data.count;
        } catch (error) {
            console.error('Poliçe adedi sorgulama hatası:', error);
            policyCountInput.placeholder = "Sorgu hatası";
        }
    });

    // b) 'AI Prim Hesapla' Butonuna Tıklandığında
    calculateButton.addEventListener('click', async () => {
        const formData = new FormData(policyForm);
        const buttonText = calculateButton.querySelector('span');
        calculateButton.disabled = true;
        buttonText.textContent = "Hesaplanıyor...";
        try {
            const response = await fetch('/api/calculate-premium', {
                method: 'POST',
                body: formData
            });
            if (!response.ok) {
                alert("Prim hesaplanamadı. Lütfen tüm zorunlu alanları doldurduğunuzdan emin olun.");
                return;
            }
            const data = await response.json();
            document.getElementById('premiumT1').value = data.premiumT1.toFixed(2);
            document.getElementById('premiumT2').value = data.premiumT2.toFixed(2);
            document.getElementById('premiumT3').value = data.premiumT3.toFixed(2);
            document.getElementById('policyCount').value = data.policyCount; 
            document.getElementById('discountAmount').value = data.discountAmount.toFixed(2);
            document.getElementById('totalPremiumAmount').value = data.totalPremium.toFixed(2);
            document.getElementById('coverageAmount').value = data.coverageAmount.toFixed(2);
        } catch (error) {
            console.error('Prim hesaplama hatası:', error);
            alert("Sunucuyla iletişim kurulamadı. Lütfen tekrar deneyin.");
        } finally {
            calculateButton.disabled = false;
            buttonText.textContent = "AI Prim Hesapla";
        }
    });
    
    // c) 'Poliçeyi Oluştur' Butonu (GÜNCELLENDİ: ÖDEMEYE YÖNLENDİR)
    createPolicyButton.addEventListener('click', () => {
        
        // 1. Formdaki tüm verileri bir nesneye dönüştür
        const formData = new FormData(policyForm);
        const policyData = Object.fromEntries(formData.entries());

        // 2. Önemli: Hesaplanan primlerin boş olmadığından emin ol
        if (!policyData.totalPremiumAmount || parseFloat(policyData.totalPremiumAmount) <= 0) {
            alert("Lütfen önce 'AI Prim Hesapla' butonuna basarak primi hesaplatın.");
            return;
        }

        // 3. Veriyi tarayıcı hafızasına (localStorage) kaydet
        // (JSON string olarak)
        try {
            localStorage.setItem('policyData', JSON.stringify(policyData));
        } catch (e) {
            console.error("Hafızaya (localStorage) yazılamadı:", e);
            alert("Poliçe verisi hafızaya kaydedilemedi. Tarayıcı ayarlarınızı kontrol edin.");
            return;
        }

        // 4. Kullanıcıyı 'payment.html' sayfasına yönlendir
        window.location.href = 'payment.html';
    });

    // Sayfa ilk yüklendiğinde butonları ayarla
    updateButtons();
});