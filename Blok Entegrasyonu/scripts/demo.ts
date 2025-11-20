import { ethers } from "hardhat";

async function main() {
  console.log("---------------------------------------------------");
  console.log("🚀 SİMÜLASYON BAŞLATILIYOR (Deploy + Test)...");

  // 1. ADIM: Kontratı Bu Script İçinde Sıfırdan Kuruyoruz
  // Sınav Tarihi: Şu andan 1 yıl sonrası (Unix zamanı)
  const examDate = Math.floor(Date.now() / 1000) + 31536000;
  
  const YKSSigorta = await ethers.getContractFactory("YKSSigorta");
  const contract = await YKSSigorta.deploy(examDate);
  
  // Kurulumun tamamlanmasını bekle
  await contract.waitForDeployment();
  
  const contractAddress = await contract.getAddress();
  console.log(`✅ Kontrat Başarıyla Kuruldu! Adresi: ${contractAddress}`);
  console.log("---------------------------------------------------");

  // 2. ADIM: Rolleri Belirle
  const [admin, student] = await ethers.getSigners();
  console.log(`👤 Admin: ${admin.address}`);
  console.log(`🎓 Öğrenci: ${student.address}`);

  // 3. ADIM: İlk Kontrol (Sigorta Var mı?)
  // 'as any' kullanarak TypeScript tip hatalarını bypass ediyoruz
  let isInsured = await (contract as any).checkInsurance(student.address);
  console.log(`\n❓ Başlangıç Durumu: Öğrenci Sigortalı mı? -> ${isInsured}`);

  // 4. ADIM: Poliçe Satın Alma (Minting)
  console.log("\n📝 Poliçe kesiliyor...");
  
  try {
    // Öğrenci adına poliçe üret
    const tx = await (contract as any).connect(admin).mintPolicy(student.address);
    await tx.wait(); // İşlemin onaylanmasını bekle
    console.log("✅ İşlem Başarılı! Poliçe bloğa yazıldı.");
  } catch (error) {
    console.error("❌ Poliçe kesilirken hata oldu:", error);
  }

  // 5. ADIM: Son Kontrol
  isInsured = await (contract as any).checkInsurance(student.address);
  console.log(`\n🎉 SONUÇ: Öğrenci Sigortalı mı? -> ${isInsured}`);
  
  console.log("---------------------------------------------------");
}

main().catch((error) => {
  console.error(error);
  process.exitCode = 1;
});