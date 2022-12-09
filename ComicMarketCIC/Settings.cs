using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using Newtonsoft.Json;
using System.Windows.Forms;

namespace ComicMarketCIC
{
    public class Settings
    {
        public string outputFolderPath { get; set; } = string.Empty;

        public readonly static string settingFileName = @"comicMarketCIC_setting.json";
        public readonly static string settingFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                                                                      @"凸版印刷株式会社",
                                                                      FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductName,
                                                                      settingFileName);


        public void ReadSettingFile()
        {
            if (File.Exists(settingFilePath) == true)
            {
                try
                {
                    Settings settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(settingFilePath));

                    this.outputFolderPath = settings.outputFolderPath;

                }
                catch
                {
                    Debug.WriteLine("設定ファイル読み込みエラー");
                    //MessageBox.Show(@"環境設定の読み込みができません" + Environment.NewLine + @"環境設定を再設定してください");
                }
            }
        }

        public void WriteSettingFile()
        {
            try
            {
                DirectoryInfo dirInfo = Directory.GetParent(settingFilePath);
                if (Directory.Exists(dirInfo.FullName) == false)
                {
                    dirInfo.Create();
                }
                String jsonStr = JsonConvert.SerializeObject(this, Formatting.Indented);

                using (StreamWriter sw = new StreamWriter(settingFilePath, false, Encoding.UTF8))
                {
                    sw.Write(jsonStr);
                }
            }
            catch
            {
                Debug.WriteLine("環境設定の書き出しエラー");
                //MessageBox.Show(@"環境設定が書き込みできません" + Environment.NewLine + @"このアプリケーションを再起動して、もう一度試してみてください");
            }
        }
    }
}
