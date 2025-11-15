// Three.js ile 3D animasyon için JavaScript kodu
document.addEventListener('DOMContentLoaded', () => {
const canvas = document.getElementById('backgroundCanvas');

// Canvas'ı bulamazsa, login sayfasında değiliz demektir, kodu çalıştırma.
if (!canvas) return; 

const loginContainer = document.querySelector('.login-container-3d');

// Scene, Camera, Renderer
const scene = new THREE.Scene();
const camera = new THREE.PerspectiveCamera(75, window.innerWidth / window.innerHeight, 0.1, 1000);
const renderer = new THREE.WebGLRenderer({ canvas: canvas, antialias: true, alpha: true }); // Alpha: true
renderer.setSize(window.innerWidth, window.innerHeight);
renderer.setPixelRatio(window.devicePixelRatio);
renderer.setClearColor(0x000000, 0); // Arkaplanı şeffaf yap (CSS'teki #111 görünecek)

camera.position.z = 5;

// Işıklar
renderer.setClearColor(0x000000, 0); // Arkaplanı şeffaf yap (CSS'teki #111 görünecek)

camera.position.z = 5;

// Işıklar (Daha belirgin olması için güncellendi)
const ambientLight = new THREE.AmbientLight(0xffffff, 0.5); // Ortam ışığını biraz kıstık (0.7'den 0.5'e)
scene.add(ambientLight);

// Yönlü ışık (daha net gölgeler ve vurgular için)
const directionalLight = new THREE.DirectionalLight(0xffffff, 1.0);
directionalLight.position.set(5, 10, 7.5); // Işığı daha iyi bir açıdan veriyoruz
scene.add(directionalLight);
// const pointLight = new THREE.PointLight(0xffffff, 1); // Eski ışığı kapattık
// pointLight.position.set(5, 5, 5);
// scene.add(pointLight);

// ===========================================

// ===========================================
// KALEM MODELİNİ OLUŞTURMA
// ===========================================
let pencilGroup = new THREE.Group();
scene.add(pencilGroup);

function createPencil() {
    // Kalem Ucu (Koyu Gri)
    const tipGeometry = new THREE.ConeGeometry(0.1, 0.5, 32);
    const tipMaterial = new THREE.MeshPhongMaterial({ color: 0x333333 });
    const tip = new THREE.Mesh(tipGeometry, tipMaterial);
    tip.position.y = 0.75;
    pencilGroup.add(tip);

    // Kalem Gövdesi (Sarı)
    const bodyGeometry = new THREE.CylinderGeometry(0.15, 0.15, 2, 32);
    const bodyMaterial = new THREE.MeshPhongMaterial({ color: 0xFFD700 }); // Altın sarısı
    const body = new THREE.Mesh(bodyGeometry, bodyMaterial);
    body.position.y = 0;
    pencilGroup.add(body);

    // Silgi (Kırmızı)
    const eraserGeometry = new THREE.CylinderGeometry(0.16, 0.16, 0.3, 32);
    const eraserMaterial = new THREE.MeshPhongMaterial({ color: 0xFF0000 }); // Kırmızı
    const eraser = new THREE.Mesh(eraserGeometry, eraserMaterial);
    eraser.position.y = -1.15;
    pencilGroup.add(eraser);
    
    pencilGroup.rotation.z = Math.PI / 2;
    pencilGroup.position.x = 0; // Ortada başla
    pencilGroup.position.y = 0; 
}

// ===========================================
// BİNER / SIFIRLAR OLUŞTURMA
// ===========================================
const binaryGroup = new THREE.Group();
let font = null;
const binaryDigits = [];

// Fontu yüklemek için Loader
const fontLoader = new THREE.FontLoader();
fontLoader.load('https://threejs.org/examples/fonts/helvetiker_regular.typeface.json', 
    (loadedFont) => {
        font = loadedFont;
        createBinaryDigits(); // Font yüklendikten sonra 0 ve 1'leri oluştur
    },
    undefined, // onProgress
    (err) => {
        console.error('Font yüklenirken bir hata oluştu:', err);
        // Hata olursa fallback (küp) fonksiyonunu kullan
        createBinaryDigit = createBinaryFallback; // Fonksiyonu değiştir
        createBinaryDigits();
    }
);

// 0 ve 1'leri (küp olarak) oluşturan fallback
function createBinaryFallback(value, x, y, z) {
    const geo = new THREE.BoxGeometry(0.25, 0.25, 0.05);
    const mat = new THREE.MeshPhongMaterial({ color: value === '1' ? 0x00ff00 : 0x00cc00 });
    const mesh = new THREE.Mesh(geo, mat);
    mesh.position.set(x, y, z);
    return mesh;
}

// 0 ve 1'leri (gerçek font ile) oluşturan fonksiyon
function createBinaryDigit(value, x, y, z) {
    if (!font) { 
        console.warn("Font henüz yüklenmedi, fallback kullanılıyor.");
        return createBinaryFallback(value, x, y, z);
    }
    // THREE.TextGeometry'nin FontLoader'dan gelen fontu kullanması gerekir
    const textGeo = new THREE.TextGeometry(value, {
        font: font,
        size: 0.3,
        height: 0.1,
        curveSegments: 12,
    });
    const textMat = new THREE.MeshPhongMaterial({ color: 0x00ff00 }); // Yeşil
    const textMesh = new THREE.Mesh(textGeo, textMat);
    textMesh.position.set(x, y, z);
    return textMesh;
}

function createBinaryDigits() {
    while(binaryGroup.children.length > 0){ 
        binaryGroup.remove(binaryGroup.children[0]); 
    }
    binaryDigits.length = 0;

    for (let i = 0; i < 50; i++) {
        const value = Math.random() > 0.5 ? '1' : '0';
        const x = (Math.random() - 0.5) * 10;
        const y = (Math.random() - 0.5) * 10;
        const z = (Math.random() - 0.5) * 10;
        const digit = createBinaryDigit(value, x, y, z);
        binaryGroup.add(digit);
        binaryDigits.push(digit);
    }
}

// ===========================================
// BLOKLAR OLUŞTURMA
// ===========================================
const blockGroup = new THREE.Group();
const blocks = [];
for (let i = 0; i < 30; i++) {
    const geometry = new THREE.BoxGeometry(0.5, 0.5, 0.5);
    const material = new THREE.MeshPhongMaterial({ color: 0x007bff }); // Mavi bloklar
    const block = new THREE.Mesh(geometry, material);
    block.position.set(
        (Math.random() - 0.5) * 8,
        (Math.random() - 0.5) * 8,
        (Math.random() - 0.5) * 8
    );
    blockGroup.add(block);
    blocks.push(block);
}

// ===========================================
// ANİMASYON DURUMLARI
// ===========================================
let animationState = 'pencil'; // 'pencil', 'binary', 'blocks'

// Kalemi oluştur
createPencil(); 

function animate() {
    requestAnimationFrame(animate);

    // Duruma göre animasyon
    if (animationState === 'pencil' && pencilGroup) {
        pencilGroup.rotation.y += 0.01;
    } else if (animationState === 'binary' && binaryGroup) {
        binaryGroup.rotation.x += 0.01;
        binaryGroup.rotation.y += 0.005;
    } else if (animationState === 'blocks' && blockGroup) {
        blockGroup.rotation.x += 0.005;
        blockGroup.rotation.y += 0.01;
    }

    // Kamera hareketi (hafif salınım)
    camera.position.x = Math.sin(Date.now() * 0.0001) * 0.5;
    camera.position.y = Math.cos(Date.now() * 0.0001) * 0.5;
    camera.lookAt(scene.position); // Her zaman sahnenin merkezine bak

    renderer.render(scene, camera);
}

animate(); // Animasyonu başlat

// ===========================================
// GEÇİŞ MANTIĞI (GSAP KULLANARAK)
// ===========================================

// Geçişi başlatan ana fonksiyon
function startTransition(from, to) {
    
    // Önceki objeyi sahneden kaldır
    if (from === 'pencil') {
        gsap.to(pencilGroup.scale, { duration: 1, x: 0.1, y: 0.1, z: 0.1, ease: "power2.in" });
        pencilGroup.children.forEach(child => {
            gsap.to(child.material, { duration: 1, opacity: 0, ease: "power2.in", onComplete: () => {
                if (pencilGroup.parent) {
                    pencilGroup.parent.remove(pencilGroup);
                }
            }});
        });
    } else if (from === 'binary') {
        binaryDigits.forEach(digit => {
            gsap.to(digit.position, { duration: 1, x: (Math.random() - 0.5) * 20, y: (Math.random() - 0.5) * 20, z: (Math.random() - 0.5) * 20, ease: "power2.in" });
            gsap.to(digit.material, { duration: 1, opacity: 0, ease: "power2.in" });
        });
        setTimeout(() => { if(binaryGroup.parent) binaryGroup.parent.remove(binaryGroup); }, 1000);
    } else if (from === 'blocks') {
        blocks.forEach(block => {
            gsap.to(block.scale, { duration: 1, x: 0.01, y: 0.01, z: 0.01, ease: "power2.in" });
            gsap.to(block.material, { duration: 1, opacity: 0, ease: "power2.in" });
        });
        setTimeout(() => { if(blockGroup.parent) blockGroup.parent.remove(blockGroup); }, 1000);
    }

    // Yeni objeyi sahneye ekle (1 saniye gecikmeyle)
    setTimeout(() => {
        if (to === 'binary') {
            if(font && binaryDigits.length === 0) createBinaryDigits(); 
            
            scene.add(binaryGroup);
            binaryDigits.forEach(digit => {
                const [startX, startY, startZ] = [digit.position.x, digit.position.y, digit.position.z];
                digit.position.set(0, 0, 0);
                digit.material.opacity = 0;
                gsap.to(digit.position, { duration: 1, x: startX, y: startY, z: startZ, ease: "power2.out" });
                gsap.to(digit.material, { duration: 1, opacity: 1, ease: "power2.out" });
            });
            animationState = 'binary';

        } else if (to === 'blocks') {
            scene.add(blockGroup);
            blocks.forEach(block => {
                const [startX, startY, startZ] = [block.position.x, block.position.y, block.position.z];
                block.position.set(0, 0, 0);
                block.scale.set(0.01, 0.01, 0.01);
                block.material.opacity = 0;
                gsap.to(block.position, { duration: 1, x: startX, y: startY, z: startZ, ease: "power2.out" });
                gsap.to(block.scale, { duration: 1, x: 1, y: 1, z: 1, ease: "power2.out" });
                gsap.to(block.material, { duration: 1, opacity: 1, ease: "power2.out" });
            });
            animationState = 'blocks';

        } else if (to === 'pencil') {
            pencilGroup = new THREE.Group();
            createPencil();
            pencilGroup.children.forEach(child => {
                child.material.transparent = true;
                child.material.opacity = 0;
            });
            pencilGroup.scale.set(0.1, 0.1, 0.1);
            scene.add(pencilGroup);
            gsap.to(pencilGroup.scale, { duration: 1, x: 1, y: 1, z: 1, ease: "power2.out" });
            pencilGroup.children.forEach(child => {
                gsap.to(child.material, { duration: 1, opacity: 1, ease: "power2.out" });
            });
            animationState = 'pencil';
        }
    }, 1000); // 1 saniye gecikme
}

// Animasyonları her 5 saniyede bir değiştir
let stateIndex = 0;
const states = ['pencil', 'binary', 'blocks'];

if (typeof gsap === 'undefined') {
    console.error("GSAP kütüphanesi (gsap.min.js) yüklenmemiş. Animasyon geçişleri (transitions) çalışmayacak.");
    // GSAP yoksa, sert geçiş yap
    setInterval(() => {
        stateIndex = (stateIndex + 1) % states.length;
        animationState = states[stateIndex];
        
        scene.remove(pencilGroup, binaryGroup, blockGroup); // Hepsini kaldır
        if(animationState === 'pencil') {
            pencilGroup = new THREE.Group(); createPencil(); scene.add(pencilGroup);
        } else if (animationState === 'binary') {
            if(font) createBinaryDigits(); scene.add(binaryGroup);
        } else if (animationState === 'blocks') {
            scene.add(blockGroup);
        }
    }, 5000);
} else {
    // GSAP yüklü ise, yumuşak geçişleri kullan
    setInterval(() => {
        const current = states[stateIndex];
        stateIndex = (stateIndex + 1) % states.length;
        const next = states[stateIndex];
        
        if (next === 'binary' && !font) {
            console.warn("Font yüklenmedi, binary adımı atlanıyor.");
            stateIndex = (stateIndex + 1) % states.length; 
        }
        startTransition(current, states[stateIndex]);
    }, 5000); // Her 5 saniyede bir geçiş yap
}

// Pencere boyutu değiştiğinde renderer ve kamera güncelle
window.addEventListener('resize', () => {
    camera.aspect = window.innerWidth / window.innerHeight;
    camera.updateProjectionMatrix();
    renderer.setSize(window.innerWidth, window.innerHeight);
});


});