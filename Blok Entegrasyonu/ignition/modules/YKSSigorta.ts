import { buildModule } from "@nomicfoundation/hardhat-ignition/modules";

// Sınav Tarihi olarak şimdiki zamandan 1 yıl sonrasını varsayalım
const ONE_YEAR_IN_FUTURE = Math.floor(Date.now() / 1000) + 31536000;

const YKSSigortaModule = buildModule("YKSSigortaModule", (m) => {
  // 1. Parametre: Sınav Tarihi (Dışarıdan değiştirilebilir, varsayılanı 1 yıl sonrası)
  const examDate = m.getParameter("examDate", ONE_YEAR_IN_FUTURE);

  // 2. Kontratı Deploy Et
  const yks = m.contract("YKSSigorta", [examDate]);

  // 3. Deploy edilen kontratı döndür
  return { yks };
});

export default YKSSigortaModule;