# 健保卡簽到系統v2.0 by jc.chen

## 直接下載SignIn.zip使用
1. 接上讀卡機並安裝對應之驅動程式
2. 執行SignIn.exe(建議使用win10 64位元作業系統)
3. 簽到表輸出檔名為SignIn月份日期-小時分鐘秒數.csv
   * 若插卡無反應，請在[這裡](http://www.ittec.com.tw/FAQ/Reader_FAQ.htm)下載"**下載執行智慧卡修正程式**"，執行scardutl.exe
   * 手動重啟服務：Win+R執行services.msc，選擇Smart Card/智慧卡並重啟

## v2.0 更新
* 新增 readme.md
* 變更 console輸出
  * 減少不必要資訊
  * 更改輸出格式
  * 顯示當前總人數
* 變更 CSV檔名
  * 減號區隔日期時間
  * 加上秒數
* 變更 CSV輸出
  * ID遮蔽中間三碼
* 刪除 readme.txt

## based on
* http://slashview.com/archive2020/20200404.html
* https://docs.microsoft.com/zh-tw/dotnet/api/system.io.file.appendtext?view=net-5.0
* .NET Core 5.0
* .nuget
  * PCSC 5.0.0
  * PCSC.Iso7816 5.0.0
  * System.Text.Encoding.CodePages 5.0.0