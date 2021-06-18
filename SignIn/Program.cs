// based on:
//   http://slashview.com/archive2020/20200404.html
//   https://docs.microsoft.com/zh-tw/dotnet/api/system.io.file.appendtext?view=net-5.0
// .NET Core 5.0
// .nuget
//   PCSC 5.0.0
//   PCSC.Iso7816 5.0.0
//   System.Text.Encoding.CodePages 5.0.0

using System;
using System.Linq;
using System.IO;
using System.Text;

namespace SignIn
{
    class Program
    {
        public static string filepath = "SignIn";
        public static StreamWriter sw;
        public static void Main()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); 
            //讀卡機名稱
            string cReader;
            //尋找本機讀卡機設備
            using (var oReaders = PCSC.ContextFactory.Instance.Establish(PCSC.SCardScope.User))
            {
                cReader = oReaders.GetReaders().FirstOrDefault();
                if (string.IsNullOrEmpty(cReader))
                { 
                    //找不到任何PSCS讀卡機就跳走
                    Console.WriteLine("# 系統找不到任何讀卡機，請重新檢查硬體。");
                    Console.ReadLine();
                    return;
                }
                else
                {
                    Console.Clear();
                    Console.WriteLine($"# 歡迎使用健保卡簽到系統v1.0 / by jc.chen(jcchen1478@gmail.com)");
                    Console.WriteLine($"# 共找到「{oReaders.GetReaders().Length} 部設備」並使用「{cReader}」讀卡機，程式運行期間請勿任意移除設備。");
                    //Console.WriteLine($"建立簽到表");
                    filepath += DateTime.Now.ToString("MMddHHmm");
                    filepath += ".csv";
                    sw = new StreamWriter(filepath, false, System.Text.Encoding.UTF8);
                    //sw = File.CreateText(filepath);
                    sw.WriteLine($"{"Name"},{"Gender"},{"ID"},{"Date"},{"Time"}");
                    sw.Close();
                }
            }
            //建立事件監控
            using (var oMonitor = PCSC.Monitoring.MonitorFactory.Instance.Create(PCSC.SCardScope.System))
            {
                oMonitor.CardRemoved += (oSender, oArgs) => {
                    Console.WriteLine("# 偵測到晶片卡移除。");
                    Console.WriteLine();
                };
                oMonitor.CardInserted += (oSender, oArgs) =>
                {
                    Console.WriteLine("# 偵測到晶片卡插入。");
                    GetCardInfo(cReader);  //讀取健保卡顯性資料
                };
                oMonitor.MonitorException += (oSender, oArgs) =>
                {
                    Console.WriteLine("# 讀卡機被移除或是讀取晶片卡出現異常，請重新啟動程式。");
                    System.Environment.Exit(0); //強制退出
                };
                oMonitor.Start(cReader);
                //有可能執行程式前讀卡機與卡片就都已經準備好，如此一來並不會觸發事件，因此先強制執行一次讀取看看
                try
                { GetCardInfo(cReader); }
                catch
                { } //若有讀取出錯就直接跳過（可能是未插卡）
                    //設定離開程序
                System.ConsoleKeyInfo oKey;
                do
                {
                    Console.WriteLine("# 若需要離開程式，請按下ESC按鈕。");
                    oKey = Console.ReadKey(true);
                } while (oKey.Key != System.ConsoleKey.Escape);
            }
            //程式結束
            Console.WriteLine("# 程式結束。");
        }
        /// <summary>
        /// 讀取台灣全民健保卡顯性資料
        /// </summary>
        public static void GetCardInfo(string cReader)
        {
            using (var oContext = PCSC.ContextFactory.Instance.Establish(PCSC.SCardScope.User))
            using (var oReader = new PCSC.Iso7816.IsoReader(
              context: oContext,
              readerName: cReader,
              mode: PCSC.SCardShareMode.Shared,
              protocol: PCSC.SCardProtocol.Any
            ))
            {
                //初始化健保卡
                var oAdpuInit = new PCSC.Iso7816.CommandApdu(PCSC.Iso7816.IsoCase.Case4Short, oReader.ActiveProtocol)
                {
                    CLA = 0x00,
                    INS = 0xA4,
                    P1 = 0x04,
                    P2 = 0x00,
                    Data = new byte[] { 0xD1, 0x58, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x11 }
                };
                //取得初始化健保卡回應
                var oAdpuInitResponse = oReader.Transmit(oAdpuInit);
                //檢查回應是否正確（144；00）
                if (!(oAdpuInitResponse.SW1.Equals(144) && oAdpuInitResponse.SW2.Equals(0)))
                {
                    Console.WriteLine("－－－－－");
                    Console.WriteLine("# 晶片卡並非健保卡，請換張卡片試看看。");
                    return;
                }
                //讀取健保顯性資訊
                var oAdpuProfile = new PCSC.Iso7816.CommandApdu(PCSC.Iso7816.IsoCase.Case4Short, oReader.ActiveProtocol)
                {
                    CLA = 0x00,
                    INS = 0xCA,
                    P1 = 0x11,
                    P2 = 0x00,
                    Data = new byte[] { 0x00, 0x00 }
                };
                //取得讀取健保卡顯性資訊回應
                var oAdpuProfileResponse = oReader.Transmit(oAdpuProfile);
                //檢查回應是否正確（144；00）
                if (!(oAdpuInitResponse.SW1.Equals(144) && oAdpuInitResponse.SW2.Equals(0)))
                {
                    Console.WriteLine("－－－－－");
                    Console.WriteLine("# 健保卡讀取錯誤，請換張卡片試看看。");
                    return;
                }
                //如果有回應且具備資料的話，就將其輸出到畫面上
                if (oAdpuProfileResponse.HasData)
                {
                    //播放提示音
                    Console.Beep();
                    //位元組資料
                    byte[] aryData = oAdpuProfileResponse.GetData();
                    //文字編碼解碼器
                    var oUTF8 = System.Text.Encoding.UTF8;
                    var oBIG5 = System.Text.Encoding.GetEncoding("big5");
                    //建立使用者匿名物件
                    var oUser = new
                    {
                        cCardNumber = oUTF8.GetString(aryData.Take(12).ToArray()),
                        cName = oBIG5.GetString(aryData.Skip(12).Take(20).ToArray()),
                        cID = oUTF8.GetString(aryData.Skip(32).Take(10).ToArray()),
                        cBirthday = oUTF8.GetString(aryData.Skip(42).Take(7).ToArray()),
                        cGender = oUTF8.GetString(aryData.Skip(49).Take(1).ToArray()) == "M" ? "男" : "女",
                        cCardPublish = oUTF8.GetString(aryData.Skip(50).Take(7).ToArray())
                    };
                    //輸出資料
                    Console.WriteLine("－－－－－");
                    Console.WriteLine($"姓名　　：{oUser.cName}");
                    Console.WriteLine($"身分證號：{oUser.cID}");
                    Console.WriteLine($"性別　　：{oUser.cGender}");
                    WriteToCSV(oUser.cName, oUser.cGender, oUser.cID);
                    Console.WriteLine($"簽到成功");
                }
            }
        }
        private static void WriteToCSV(string Name, string Gender, string ID)
        {
            sw = File.AppendText(filepath);
            sw.WriteLine($"{Name},{Gender},{ID},{DateTime.Now:yyyy/MM/dd},{DateTime.Now:HH:mm:ss}");
            sw.Close();
        }
    }
}