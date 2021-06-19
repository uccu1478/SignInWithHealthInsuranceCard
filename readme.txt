可直接下載SignIn.zip使用
先接上讀卡機並安裝驅動(atmsetup.exe，如使用IT-500U)
若Smart Card服務未啟動，執行scardutl.exe
執行SignIn/SignIn.exe
輸出為SignIn{MMddHHmm}.csv檔

// based on:
//   http://slashview.com/archive2020/20200404.html
//   https://docs.microsoft.com/zh-tw/dotnet/api/system.io.file.appendtext?view=net-5.0
// .NET Core 5.0
// .nuget
//   PCSC 5.0.0
//   PCSC.Iso7816 5.0.0
//   System.Text.Encoding.CodePages 5.0.0