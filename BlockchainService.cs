using Nethereum.Web3;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using System.Numerics;
using System.Threading.Tasks;

namespace SinavSigortasiWeb3.Services // BURAYI KENDİ NAMESPACE'İN YAP (Örn: SınavSigortasi.Services)
{
    public class BlockchainService
    {
        // 1. BURAYI DOLDUR: Demo.ts çalışınca çıkan "0x..." adresi
        private const string ContractAddress = "0x5FbDB2315678afecb367f032d93F642f64180aa3"; 

        // 2. BURAYI DOLDUR: Hardhat terminalindeki Account #0 Private Key'i
        private const string PrivateKey = "0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80";

        // Hardhat Yerel Ağ Adresi
        private const string RpcUrl = "http://127.0.0.1:8545"; 
        
        private readonly Web3 _web3;
        private readonly Contract _contract;

        public BlockchainService()
        {
            // Hesabı ve ağı hazırla
            var account = new Nethereum.Web3.Accounts.Account(PrivateKey);
            _web3 = new Web3(account, RpcUrl);

            // Senin yazdığın kontratın özeti (ABI)
            // Bunu elle yazdım ki dosya aramakla uğraşma
            string abi = @"[
                {
                    'inputs': [{'internalType': 'address', 'name': 'student', 'type': 'address'}],
                    'name': 'checkInsurance',
                    'outputs': [{'internalType': 'bool', 'name': '', 'type': 'bool'}],
                    'stateMutability': 'view',
                    'type': 'function'
                },
                {
                    'inputs': [{'internalType': 'address', 'name': 'student', 'type': 'address'}],
                    'name': 'mintPolicy',
                    'outputs': [],
                    'stateMutability': 'nonpayable',
                    'type': 'function'
                }
            ]";

            // Tırnak işaretlerini düzelt
            abi = abi.Replace("'", "\"");

            // Kontrata bağlan
            _contract = _web3.Eth.GetContract(abi, ContractAddress);
        }

        // FONKSİYON 1: Öğrencinin sigortası var mı?
        public async Task<bool> IsInsuredAsync(string studentAddress)
        {
            var function = _contract.GetFunction("checkInsurance");
            return await function.CallAsync<bool>(studentAddress);
        }

        // FONKSİYON 2: Poliçe Kes (Blok zincirine yazma işlemi)
        public async Task<string> CreatePolicyAsync(string studentAddress)
        {
            var function = _contract.GetFunction("mintPolicy");

            // İşlemi gönder (Gas ücretini Account #0 öder)
            // Tahmini Gas limiti koyuyoruz (Hardhat için genelde 200.000 yeterli)
            var gas = new HexBigInteger(200000);
            var value = new HexBigInteger(0); // Fonksiyona ETH göndermiyoruz, sadece çağırıyoruz

            string txHash = await function.SendTransactionAsync(
                _web3.TransactionManager.Account.Address, // Gönderen (Admin)
                gas, 
                value, 
                studentAddress // Parametre (Öğrenci Adresi)
            );

            return txHash; // İşlem numarasını döndür
        }
    }
}