using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Microsoft.VisualBasic.FileIO;
using System.Text.RegularExpressions;
using Microsoft.WindowsAPICodePack.Dialogs;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.SS.Util;


namespace ComicMarketCIC
{
    public partial class Form1 : Form
    {
        Settings settings;
        //読み込みデータHeader
        private static List<List<string>> readDataHeaderList = new List<List<string>>();
        //読み込みデータ
        private static List<List<string>> readDataList = new List<List<string>>();

        //private static List<List<string>> readDataList_tmp = new List<List<string>>();

        //ファイルパスList
        private static List<string> filePathList = new List<string>();

        public Form1()
        {
            InitializeComponent();
            settings = new Settings();

            settings.ReadSettingFile();

            textBox_outputFolderPath.Text = settings.outputFolderPath;
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            listBox_filePath.Items.Clear();

            int colNum = 26;

            dataGridView1.ColumnCount = colNum;
            //A-AZの記載
            for (int Colum_num = 0; Colum_num < colNum; Colum_num++)
            {
                string str = GetColumnName(Colum_num);
                dataGridView1.Columns[Colum_num].HeaderText = str;
            }
        }



        #region リストボックス関連
        private void listBox_filePath_DragDrop(object sender, DragEventArgs e)
        {
            var fileList = listBox_filePath.Items.Cast<string>().ToArray();

            //ファイルパスListの初期化
            filePathList.Clear();

            if (fileList.Count() != 0)
            {
                foreach (string filePath in fileList)
                {
                    filePathList.Add(filePath);
                }
            }

            //Console.WriteLine(e.Data.GetDataPresent(DataFormats.FileDrop));

            if (e.Data.GetDataPresent(DataFormats.FileDrop) == true)
            {
                foreach (string fileName in (string[])e.Data.GetData(DataFormats.FileDrop))
                {
                    bool isExist = false;
                    if (Path.GetExtension(fileName) == ".csv")
                    {
                        foreach (string filePath in fileList)
                        {
                            if (filePath == fileName)
                            {
                                isExist = true;
                                break;
                            }
                        }

                        if (isExist == false)
                        {
                            //dragDropされたファイルパスの追加
                            listBox_filePath.Items.Add(fileName);
                            filePathList.Add(fileName);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("無効なファイルが含まれています。", "処理エラー", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }

            Console.WriteLine( Environment.NewLine+ "ファイルパスListの中身：");
            foreach (string item in filePathList)
            {
                Console.WriteLine(item);
            }
            Console.WriteLine(Environment.NewLine);
            //データ読み込み

            try
            {
                //データの読み込み
                ReadAllData(ref readDataHeaderList, ref readDataList);
            }
            catch (Exception ex)
            {
                MessageBox.Show("データ読み込み時にエラーが発生しました。"
                                + Environment.NewLine +
                                "エラー詳細："
                                + Environment.NewLine +
                                ex.Message);
                return;
            }

            try
            {
                //データの加工
                Data_Edit(ref readDataList);
            }
            catch (Exception ex)
            {
                MessageBox.Show("データ加工時にエラーが発生しました。"
                                +Environment.NewLine+
                                "エラー詳細："
                                +Environment.NewLine+
                                ex.Message);
                return;
            }


            //データグリッドビュー反映
            Add_readDataList_to_DataGridView(readDataHeaderList,readDataList);

        }

        private void listBox_filePath_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }


        //deleteKeyでListBoxのitem削除
        private void listBox_filePath_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.KeyData == Keys.Delete)
            {
                int index = listBox_filePath.SelectedIndex;

                if (index == -1) return;
                string fp = string.Empty;
                try
                {
                    fp = listBox_filePath.SelectedItem.ToString();
                    listBox_filePath.Items.RemoveAt(index);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("ファイル削除中にエラーが発生しました。" +
                                    Environment.NewLine +
                                    Environment.NewLine +
                                    "FilePath：" +
                                    Environment.NewLine +
                                    fp +
                                    Environment.NewLine +
                                    Environment.NewLine +
                                    "エラー詳細：" +
                                    Environment.NewLine +
                                    ex.Message);
                    return;
                }

            }

            var fileList = listBox_filePath.Items.Cast<string>().ToArray();

            //ファイルパスListの初期化
            filePathList.Clear();

            if (fileList.Count() != 0)
            {
                foreach (string filePath in fileList)
                {
                    filePathList.Add(filePath);
                }
            }

            Console.WriteLine(Environment.NewLine + "ファイルパスListの中身：");
            foreach (string item in filePathList)
            {
                Console.WriteLine(item);
            }
            Console.WriteLine(Environment.NewLine);

            try
            {
                //データの読み込み
                ReadAllData(ref readDataHeaderList, ref readDataList);
            }
            catch (Exception ex)
            {
                MessageBox.Show("データ読み込み時にエラーが発生しました。"
                                + Environment.NewLine +
                                "エラー詳細："
                                + Environment.NewLine +
                                ex.Message);
                return;
            }

            try
            {
                //データの加工
                Data_Edit(ref readDataList);
            }
            catch (Exception ex)
            {
                MessageBox.Show("データ加工時にエラーが発生しました。"
                                + Environment.NewLine +
                                "エラー詳細："
                                + Environment.NewLine +
                                ex.Message);
                return;
            }

            //データグリッドビュー反映
            Add_readDataList_to_DataGridView(readDataHeaderList, readDataList);
        }
        #endregion

        #region データ読み込み関連
        //データをGridViewに反映
        private void Add_readDataList_to_DataGridView(List<List<string>> readDataHeaderList,List<List<string>> readDataList)
        {
            //dataGridView1.DataSource = readDataList;

            if (readDataHeaderList.Count() == 0 && readDataList.Count() == 0)
            {
                dataGridView1.Rows.Clear();
                return;
            }

            dataGridView1.Rows.Clear();

            dataGridView1.Rows.Add(readDataHeaderList[0].ToArray());

            for (int i = 0; i < readDataList.Count(); i++)
            {
                dataGridView1.Rows.Add(readDataList[i].ToArray());
            }

        }

        //ListBoxにいるファイルを全て読み込んで連結
        private void ReadAllData(ref List<List<string>> readDataHeaderList,ref List<List<string>> readDataList)
        {
            readDataHeaderList.Clear();
            readDataList.Clear();

            var fileList = listBox_filePath.Items.Cast<string>().ToArray();

            if (fileList.Count() == 0) return;

            //最初のファイルかどうかの判定用（ヘッダーを最初のファイルから取得するため）
            int counter = 0;
            bool isGetHeaderInfo = true;
            foreach (string filePath in fileList)
            {
                //最初のファイルでなければfalse(0回目のみtrueになる)
                if (counter != 0) isGetHeaderInfo = false;

                // 1ファイル分のデータをtmpに入れる
                List<List<string>> readDataList_tmp = new List<List<string>>();
                //ファイル読み込み
                ReadCSVfile(filePath,ref readDataList_tmp);

                if (readDataList_tmp.Count() != 0)
                {
                    //最初のファイルの場合は0番目のヘッダーをHeaderListに入れる
                    if (isGetHeaderInfo == true)
                    {
                        readDataHeaderList.Add(readDataList_tmp[0]);
                    }

                    //1から開始（2行目から）
                    for (int i = 1; i < readDataList_tmp.Count; i++)
                    {
                        readDataList.Add(readDataList_tmp[i]);
                    }
                }

                counter = counter + 1;
            }
            /*
            //先頭（1列目）に数値を差し込み
            if (readDataList.Count()!=0)
            {
                int rowNum = 1;
                foreach (List<string> readData in readDataList)
                {
                    readData.Insert(0, rowNum.ToString("D4"));
                    rowNum = rowNum + 1;
                }
            }
            */
        }
        //CSV読み込み
        private void ReadCSVfile(string filePath ,ref List<List<string>> readDataList_tmp)
        {            //Listの初期化
            //readDataHeaderList.Clear();
            //readDataList.Clear();

            if(File.Exists(filePath) == false)
            {
                MessageBox.Show("ファイルが存在しません。" + Environment.NewLine + filePath);
                return;
            }

            TextFieldParser parser = new TextFieldParser(filePath, System.Text.Encoding.GetEncoding("UTF-8"));

            using (parser)
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(","); // 区切り文字はコンマ

                // parser.HasFieldsEnclosedInQuotes = false;
                // parser.TrimWhiteSpace = false;

                while (!parser.EndOfData)
                {
                    List<string> readDataSingleRowList = new List<string>();

                    string[] row = parser.ReadFields(); // 1行読み込み

                    foreach (string field in row)
                    {
                        string f = field;
                        //f = f.Replace("\r\n", "n"); // 改行をnで表示
                        //f = f.Replace(" ", "_"); // 空白を_で表示

                        readDataSingleRowList.Add(f);
                        Console.Write(f + "\t"); // TAB区切りで出力
                    }
                    readDataList_tmp.Add(readDataSingleRowList);
                    Console.WriteLine();
                }
            }
        }

        #endregion

        #region データ加工

        private void Data_Edit(ref List<List<string>> readDataList)
        {
            List<string> idList = new List<string>();

            if (readDataList.Count() == 0) return;

            //CircleのIDを取得
            foreach (List<string> readData in readDataList)
            {
                if (readData[0] == "Circle")
                {
                    idList.Add(readData[1]);
                }
            }

            //重複を検出
            List<string> duplicateList = ListUtils.FindDuplication(idList);

            //重複しているIndexを取得するためのList
            List<List<string>> duplicateDeleteList = new List<List<string>>();


            //重複があれば
            if (duplicateList.Count() != 0)
            {
                string comment = string.Empty;
                string errorMassage = string.Empty;

                foreach (string id in duplicateList)
                {
                    comment = string.Empty;
                    //重複の1個目かどうかのフラグ
                    bool isFirstIndex = true;
                    int firstIndex = 0;
                    for (int i = 0; i < readDataList.Count(); i++)
                    {
                        if (readDataList[i][0] == "Circle" && readDataList[i][1] == id)
                        {
                            if (readDataList[i].Count() <= 17)
                            {
                                MessageBox.Show("カラムが足りないため処理できませんでした。" +
                                                Environment.NewLine +
                                                "読み込みデータ：" + (i + 1).ToString() + "行目");
                                continue;
                            }

                            if (isFirstIndex == true)
                            {
                                if (readDataList[i][17] != null) comment = readDataList[i][17];

                                firstIndex = i;
                                isFirstIndex = false;
                            }
                            else
                            {
                                if (readDataList[i][17] != null) comment = comment + " " + readDataList[i][17];
                                duplicateDeleteList.Add(readDataList[i]);

                            }
                        }

                        if (readDataList[i][0] == "UnKnown")
                        {
                            duplicateDeleteList.Add(readDataList[i]);
                        }
                    }

                    //追加
                    if (readDataList[firstIndex][17] != null) readDataList[firstIndex][17] = comment;

                }

                if (duplicateDeleteList.Count()!=0)
                {
                    //readDataListから重複データの削除
                    foreach (List<string> removeData in duplicateDeleteList)
                    {
                        readDataList.Remove(removeData);
                    }
                }


            }

        }

        #endregion



        #region その他、汎用メソッドなど
        //インデックスを与えるとエクセル形式の行番号が返ってくる関数
        private string GetColumnName(int index)
        {
            string str = string.Empty;
            do
            {
                str = Convert.ToChar(index % 26 + 0x41) + str;
            } while ((index = index / 26 - 1) != -1);

            return str;
        }

        //閉じるボタンClick
        private void button_close_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        #endregion


        #region 環境設定

        //参照ボタンクリック
        private void button_outputFolderPath_select_Click(object sender, EventArgs e)
        {
            string path = string.Empty;

            if (FolderSelect(ref path, @"出力先を選択してください。")) textBox_outputFolderPath.Text = path;

            settings.outputFolderPath = path;

            settings.WriteSettingFile();
        }

        //フォルダ選択ダイアログ
        private Boolean FolderSelect(ref String folderPath, String message)
        {
            var dialog = new CommonOpenFileDialog(message);

            dialog.IsFolderPicker = true;

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                folderPath = dialog.FileName;
            }
            else
            {
                return false;
            }

            return true;
        }

        #endregion


        #region Excel関連

        private void ExportExcel(string folderPath,
                                 string fileName_withoutExtension,
                                 List<List<string>> readDataHeaderList,
                                 List<List<string>> readDataList)
        {
            if (readDataList.Count == 0) return;

            //出力ファイルパス
            string filePath = Path.Combine(folderPath, fileName_withoutExtension + ".xlsx");

            if (File.Exists(filePath) == true)
            {
                MessageBox.Show("「"+fileName_withoutExtension+"」と同名ファイルが存在します。");
                return;
            }



            IWorkbook book;
            ISheet    sheet;

            book = new XSSFWorkbook();
            sheet = book.CreateSheet();


            try
            {
                if (readDataHeaderList[0].Count()!=0)
                {
                    //ヘッダー
                    for (int col = 0; col < readDataHeaderList[0].Count(); col++)
                    {
                        WriteCell(sheet, 0, col, readDataHeaderList[0][col]);
                    }
                }

                if (readDataList.Count()!=0)
                {
                    for (int row = 0; row < readDataList.Count(); row++)
                    {
                        for (int col = 0; col < readDataList[row].Count(); col++)
                        {
                            WriteCell(sheet, row+1, col, readDataList[row][col]);
                        }
                    }
                }

                //ブックを保存
                using (var fs = new FileStream(filePath, FileMode.Create))
                {
                    book.Write(fs);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Excel出力時にエラー発生" +
                                 Environment.NewLine +
                                 Environment.NewLine +
                                 "詳細："+
                                 Environment.NewLine +
                                 ex.Message);
                throw;
            }

            MessageBox.Show("Success!");
        }

        public static void WriteCell(ISheet sheet, int rowIndex, int columnIndex, string value)
        {
            var row = sheet.GetRow(rowIndex) ?? sheet.CreateRow(rowIndex);
            var cell = row.GetCell(columnIndex) ?? row.CreateCell(columnIndex);

            cell.SetCellValue(value);
        }

        #endregion

        #region データ出力

        private void ExportCSV(string folderPath,
                                 string fileName_withoutExtension,
                                 List<List<string>> readDataHeaderList,
                                 List<List<string>> readDataList)
        {

            if (readDataList.Count == 0)
            {
                MessageBox.Show("データが空のため出力できません。");
                return;
            }

            //出力ファイルパス
            string filePath = Path.Combine(folderPath, fileName_withoutExtension + ".csv");

            if (File.Exists(filePath) == true)
            {
                MessageBox.Show("「" + fileName_withoutExtension + "」と同名ファイルが存在します。");
                return;
            }

            //加工後データ
            List<string> readDataList_output = new List<string>();

            if (readDataHeaderList.Count()!=0)
            {
                string line = string.Empty;
                bool isFirst = true;
                foreach (string value in readDataHeaderList[0])
                {
                    if (isFirst == true)
                    {
                        //line = ("\"" + value + "\""); //元のやつ
                        line = value;
                        isFirst = false;
                    }
                    else
                    {
                        //line = line + "," + ("\"" + value + "\"");//元のやつ
                        line = line + "," + value;
                    }
                }
                readDataList_output.Add(line);


                /*
                foreach (var data in readDataHeaderList)
                {
                    readDataList_output.Add(string.Join(",", data));
                }
                */
            }

            if (readDataList.Count() != 0)
            {
                foreach (List<string> data in readDataList)
                {
                    string line = string.Empty;
                    bool isFirst = true;
                    for (int i = 0; i < data.Count; i++)
                    {
                        if (isFirst==true)
                        {
                            //line = ("\"" + data[i] + "\"");//元のやつ
                            line = (data[i]);
                            isFirst = false;
                        }
                        else
                        {
                            //line = line + "," + ("\"" + data[i] + "\""); //もとのやつ
                            line = line + "," + data[i];
                        }
                    }
                    readDataList_output.Add(line);
                    //readDataList_output.Add(string.Join(",", data));
                }
            }

            try
            {
                using (StreamWriter sw = new StreamWriter(filePath, false,Encoding.GetEncoding("UTF-8")))
                {
                    foreach (string line in readDataList_output)
                    {
                        sw.WriteLine(line);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return;
            }

            MessageBox.Show("Success!");
        }

        #endregion

        //処理スタート
        private void button_start_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox_outputFileName.Text) == true) return;

            //ExportExcel(textBox_outputFolderPath.Text, textBox_outputFileName.Text, readDataHeaderList, readDataList);
            ExportCSV(textBox_outputFolderPath.Text, textBox_outputFileName.Text, readDataHeaderList, readDataList);
        }
    }
}

