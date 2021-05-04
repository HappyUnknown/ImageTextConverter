//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Data;
//using System.Windows.Documents;
//using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
//using System.Windows.Navigation;
//using System.Windows.Shapes;
//using System.IO;
//using System.Diagnostics;
//using System.Runtime.InteropServices;
//using System.Threading;
//using System.Security.Cryptography;
//using System.Windows.Threading;
//using System.IO.MemoryMappedFiles;

//namespace ImageTextConverter
//{
//    /// <summary>
//    /// Логика взаимодействия для MainWindow.xaml
//    /// </summary>
//    public partial class MainWindow : Window
//    {
//        double playSpeed = -1;
//        const int MAX_HANDLABLE_MB = 10000;
//        double MAX_PLAY_SPEED = 2.5;
//        double SPEEDUP_STEP = 0.5;
//        string resPath = "result.txt";
//        string setPath = "settings.txt";
//        const char specSymbol = '?';
//        int currentRow = -1;
//        List<string> currentFile;
//        List<string> names;
//        bool withName = true;
//        readonly char[] numSymbols = new char[] { '⓿', '➊', '➋', '➌', '➍', '➎', '➏', '➐', '➑', '➒' };
//        readonly char[] nums = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
//        DispatcherTimer timer = new DispatcherTimer();//To have control over it - put it here
//        enum ImgType
//        {
//            PNG = 1,
//            JPG = 2,
//            JPEG = 3,
//            BMP = 4
//        }
//        enum VidType
//        {
//            MP4 = 1
//        }
//        void SetDefaults()
//        {
//            btnLoadFile.Width = 50;
//            btnLoadFile.Height = 25;
//            btnChooseFolder.Width = 50;
//            btnChooseFolder.Height = 25;
//            btnChangeResult.Width = 50;
//            btnChangeResult.Height = 25;
//            btnRestoreFiles.Width = 50;
//            btnRestoreFiles.Height = 25;
//            tbWidth.Width = 100;
//            tbWidth.Height = 25;
//            tbHeight.Width = 100;
//            tbHeight.Height = 25;

//            btnPrev.Width = 30;
//            btnPrev.Height = 450;
//            imgFile.Width = 450;
//            imgFile.Height = 450;
//            btnNext.Width = 30;
//            btnNext.Height = 450;

//            btnAddElement.Width = 255;
//            btnRemoveElement.Width = 255;
//            btnAddElement.Height = 20;
//            btnRemoveElement.Height = 20;

//            Height = 534;
//            Width = 526;
//        }
//        public MainWindow()
//        {
//            InitializeComponent();
//            try
//            {
//                resPath = GetLastDictionary();
//                var arr = File.ReadAllLines(resPath).ToList();
//                try
//                {
//                    currentFile = GetContent(arr);
//                }
//                catch { WriteToLog("FUCKING CONTENT"); }
//                try
//                {
//                    names = GetNames(arr);
//                }
//                catch { WriteToLog("FUCKING NAMES"); }
//                File.Create("logs.txt").Close();
//                if (!File.Exists(setPath)) File.Create(setPath).Close();
//                if (!File.Exists(resPath)) File.Create(resPath).Close();
//            }
//            catch (Exception ex) { WriteToLog("Program files check/creation error.", new StackTrace(), ex); }
//            SetDefaults();
//            ApplyResolution(534.ToString(), 526.ToString());
//            btnLoadFile.ToolTip = "File will be uploaded to preview.";
//            btnChooseFolder.ToolTip = "Whole folder would be saved to *.txt chosen.";
//            btnRestoreFiles.ToolTip = "Whole file would be restored to folder \"Unarchived\"";
//            btnChangeResult.ToolTip = "Current path: " + resPath;

//            KeyDown += new System.Windows.Input.KeyEventHandler(MainWindow_KeyDown);
//            vidFile.MediaEnded += new RoutedEventHandler(MediaEnded);//Loop
//        }
//        void MediaEnded(object sender, RoutedEventArgs e)
//        {
//            vidFile.Position = TimeSpan.FromSeconds(0);
//            vidFile.Play();
//        }
//        void WriteToLog(string message, StackTrace st = null, Exception ex = null, string tip = "")
//        {
//            try { if (!File.Exists("logs.txt")) File.Create("logs.txt").Close(); } catch { WriteToLog("Can't reach log file.", new StackTrace()); }
//            File.AppendAllText("logs.txt", "[" + DateTime.Now + "] ");
//            if (st != null) File.AppendAllText("logs.txt", st.GetFrame(0).GetMethod().Name + "()");
//            File.AppendAllText("logs.txt", "-> " + message.TrimEnd('.') + ".");
//            if (ex != null) File.AppendAllText("logs.txt", " (" + ex.Message + ") ");
//            if (tip.Length > 0) File.AppendAllText("logs.txt", "[Tip: " + tip.TrimEnd('.') + "]");
//            File.AppendAllText("logs.txt", Environment.NewLine);
//        }
//        #region Archive
//        bool IsNum(char sym)
//        {
//            int temp;
//            return int.TryParse(sym.ToString(), out temp);
//        }
//        int ToNum(char sym)
//        {
//            int temp;
//            if (!int.TryParse(sym.ToString(), out temp)) temp = 0;
//            return temp;
//        }
//        string TranslateNums(string text)
//        {
//            for (int i = 0; i < 10; i++)
//            {
//                text = text.Replace(nums[i], numSymbols[i]);
//            }
//            return text;
//        }
//        string DecodeNums(string text)
//        {
//            for (int i = 0; i < 10; i++)
//            {
//                text = text.Replace(numSymbols[i], nums[i]);
//            }
//            return text;
//        }
//        bool EqualsCodedNum(char s)
//        {
//            return s == '⓿'
//                || s == '➊' || s == '➋' || s == '➌'
//                || s == '➍' || s == '➎' || s == '➏'
//                || s == '➐' || s == '➑' || s == '➒';
//        }
//        bool EqualsOrdiNum(char s)
//        {
//            return s == '0'
//                || s == '1' || s == '2' || s == '3'
//                || s == '4' || s == '5' || s == '6'
//                || s == '7' || s == '8' || s == '9';
//        }
//        string Compress2to9(string text)
//        {
//            text = TranslateNums(text);
//            string zip = "";
//            try
//            {
//                int quantity = 1;
//                if (text.Length > 1)
//                {
//                    for (int i = 0; i < text.Length; i++)
//                        if (i < text.Length - 1)//Going through the symbol line till last
//                        {
//                            if (text[i] == text[i + 1] && quantity < 9)//Count untill we reach 9 or earlier
//                            {
//                                quantity++;
//                            }
//                            else//Add counted symbol with count
//                            {
//                                if (quantity > 1)
//                                    zip += text[i] + quantity.ToString();
//                                else
//                                    zip += text[i];
//                                quantity = 1;
//                            }
//                        }
//                        else//Processing last symbol
//                        {
//                            if (text[text.Length - 2] == text[text.Length - 1])
//                            {
//                                zip += text[i] + quantity.ToString();
//                            }
//                            else zip += text[i];
//                        }
//                }
//                else
//                {
//                    zip = text;
//                }
//            }
//            catch (Exception ex) { MessageBox.Show(ex.Message); }
//            return zip;
//        }
//        string Decode(string zip)
//        {
//            string text = "";
//            try
//            {
//                if (zip.Length > 1)
//                {
//                    for (int i = 0; i < zip.Length; i++)
//                    {
//                        if (i < zip.Length - 1)//Until last
//                        {
//                            if (EqualsCodedNum(zip[i]))//Symbol is number
//                            {
//                                if (IsNum(zip[i + 1]))
//                                {
//                                    for (int j = 0; j < ToNum(zip[i + 1]); text += zip[i], j++) ;
//                                    i++;//Skipping repeatative num procession
//                                }
//                                else
//                                {
//                                    text += zip[i];
//                                }
//                                text = DecodeNums(text);
//                            }
//                            else if (!IsNum(zip[i]))
//                            {
//                                if (IsNum(zip[i + 1]))
//                                {
//                                    for (int j = 0; j < ToNum(zip[i + 1]); text += zip[i], j++) ;
//                                    i++;//Skipping repeatative num procession
//                                }
//                                else
//                                {
//                                    text += zip[i];
//                                }
//                            }
//                            else if (IsNum(zip[i]))
//                            {
//                                continue;
//                            }
//                            else//Last
//                            {
//                                if (!IsNum(zip[i]))//If symbol
//                                {
//                                    text += zip[i];
//                                }
//                            }
//                        }
//                        else { text = zip; }
//                    }
//                }
//            }
//            catch (Exception ex) { MessageBox.Show(ex.Message); }
//            return text;
//        }
//        string[] CompressArr(string[] arr)
//        {
//            for (int i = 0; i < arr.Length; i++)
//            {
//                arr[i] = Compress2to9(arr[i]);
//            }
//            return arr;
//        }
//        string[] DecodeArr(string[] arr)
//        {
//            for (int i = 0; i < arr.Length; i++)
//            {
//                arr[i] = Decode(arr[i]);
//            }
//            return arr;
//        }
//        #endregion
//        #region Convertation
//        string FileToBase64(string path)
//        {
//            return Convert.ToBase64String(File.ReadAllBytes(path));
//        }
//        List<byte[]> Base64ToBytes(List<string> base64s)
//        {
//            List<byte[]> bytes = new List<byte[]>();
//            for (int i = 0; i < base64s.Count; i++)
//            {
//                bytes.Add(Convert.FromBase64String(base64s[i]));
//            }
//            return bytes;
//        }
//        byte[] Base64ToBytes(string base64)
//        {
//            try
//            {
//                return Convert.FromBase64String(base64);
//            }
//            catch (Exception ex)
//            {
//                WriteToLog("Error during converting base64-string to byte[].", new StackTrace(), ex);
//            }
//            return default;
//        }
//        Uri BytesToUri(byte[] arr)
//        {
//            string base64 = Encoding.UTF8.GetString(arr);
//            byte[] bytes = Convert.FromBase64String(base64);
//            return new Uri(Encoding.UTF8.GetString(bytes));
//        }
//        Uri Base64ToUri(string base64)
//        {
//            byte[] bytes = Convert.FromBase64String(base64);
//            return new Uri(Encoding.UTF8.GetString(bytes));
//        }
//        List<string> FilesToBase64(List<string> paths)
//        {
//            List<string> base64arr = new List<string>();
//            for (int i = 0; i < paths.Count; i++)
//            {
//                base64arr.Add(Convert.ToBase64String(File.ReadAllBytes(paths[i])));
//            }
//            return base64arr;
//        }
//        public BitmapImage BytesToBitmap(byte[] array)
//        {
//            using (var ms = new System.IO.MemoryStream(array))
//            {
//                var image = new BitmapImage();
//                image.BeginInit();
//                image.CacheOption = BitmapCacheOption.OnLoad; // here
//                image.StreamSource = ms;
//                image.EndInit();
//                return image;
//            }
//        }
//        #endregion
//        List<string> GetContent(List<string> cf)
//        {
//            List<string> fs = new List<string>();
//            for (int i = 0; i < cf.Count; i++)
//            {
//                try
//                {
//                    fs.Add(cf[i].Split()[0]);
//                }
//                catch (Exception ex)
//                {
//                    btnLoadFile.ToolTip = "GetContent(): File was too big to handle or something. Size: " + GetMBSize(resPath) + "Mb , file is bigger than expected: " + (MAX_HANDLABLE_MB < GetMBSize(resPath)) + ex.Message;
//                }
//            }
//            return fs;
//        }
//        List<string> GetNames(List<string> cf)
//        {
//            MessageBox.Show(cf[0]);
//            List<string> fs = new List<string>();
//            string[] rawArr;
//            for (int i = 0; i < cf.Count; i++)
//            {
//                try
//                {
//                    try
//                    {
//                        rawArr = cf[i].Split();
//                        try
//                        {
//                            if (rawArr.Length > 1)
//                                fs.Add(rawArr[1].Replace(specSymbol, ' '));
//                            else
//                                fs.Add("");
//                        }
//                        catch (Exception ex2) { MessageBox.Show("GetNames(), name adding:" + ex2.Message); }
//                    }
//                    catch (Exception ex1)
//                    {
//                        //MessageBox.Show("GetNames(), Splitting raw line #" + i + ":" + ex1.Message); 
//                    }
//                }
//                catch (Exception ex)
//                {
//                    MessageBox.Show("GetNames(): Index-" + i + ">" + (cf.Count - 1) + ";" + ex.Message);
//                }
//            }
//            return fs;
//        }
//        List<string> ConstructContent(List<string> cfc, List<string> cfn)
//        {
//            List<string> cc = new List<string>();
//            try
//            {
//                for (int i = 0; i < cfc.Count; i++)
//                    cc.Add(cfc[i] + " " + cfn[i].Replace(' ', specSymbol));
//            }
//            catch (Exception ex) { MessageBox.Show("ConstructContent(): " + ex.Message); }
//            return cc;
//        }
//        bool ArrayContains(string[] array, string element)
//        {
//            for (int i = 0; i < array.Length; i++)
//            {
//                if (array[i] == element) return true;
//            }
//            return false;
//        }
//        string GetStartName(string type, string dirPath = "Unarchived")
//        {
//            if (dirPath.Length == 0) dirPath = Environment.CurrentDirectory + GetStartName(type);
//            dirPath += "\\";
//            int i = 1;
//            try
//            {
//                string[] filepaths = Directory.GetFiles(dirPath);
//                for (int fp = 0; fp < filepaths.Length; fp++)
//                {
//                    filepaths[fp] = filepaths[fp].Replace(dirPath, "");
//                }
//                for (int fp = 0; fp < filepaths.Length; fp++)
//                {
//                    if (filepaths[fp].StartsWith("."))
//                        filepaths[fp] = GetStartName(type) + filepaths[fp];
//                }
//                while (ArrayContains(filepaths, i.ToString() + type)) { i++; }//LOOK HERE IF ALL FILES SAVED WITH 1 SHARED NAME
//                WriteToLog("Last name possible: " + i, new StackTrace());
//                return i.ToString() + type;
//            }
//            catch (Exception ex)
//            {
//                WriteToLog("Failed to give possible name.", new StackTrace(), ex);
//            }
//            return i.ToString() + type;
//        }
//        string GetFileNameByIndex(int index, List<string> filesNames)
//        {
//            try
//            {
//                if (index >= filesNames.Count)
//                    return GetStartName(GetFileTypeByBase64(currentFile[index]));
//                return filesNames[index];
//            }
//            catch
//            {
//                WriteToLog("Can't find name of file on index " + index, new StackTrace());
//                return "NO_SUCH_FILE";
//            }
//        }
//        string GetFileTypeByBase64(string base64)
//        {
//            var data = base64.Substring(0, 5);
//            switch (data.ToUpper())
//            {
//                case "IVBOR":
//                    return ".png";
//                case "/9J/4":
//                    return ".jpg";
//                case "AAAAI":
//                case "AAAAG":
//                case "AAAAF":
//                case "AAAAH":
//                    return ".mp4";
//                case "R0lGO":
//                    return ".gif";
//                case "U1PKC":
//                    return ".txt";
//                case "UKLGR":
//                    return ".wav";
//                case "SUQZB":
//                    return ".mp3";
//                case "JVBER":
//                    return ".pdf";
//                case "AAABA":
//                    return ".ico";
//                case "UMFYI":
//                    return ".rar";
//                case "E1XYD":
//                    return ".rtf";
//                case "MQOWM":
//                case "77U/M":
//                    return ".srt";
//                default:
//                    return "." + data;
//            }
//        }
//        void CreateFileFromBase64(string base64, string name = "")
//        {
//            try
//            {
//                name = GetStartName(GetFileTypeByBase64(base64), "Temp\\");
//                if (names[currentRow].Length > 0)
//                {
//                    string savedName = names[currentRow];
//                    if (savedName.Length > 1)
//                        name = savedName;
//                }
//                else name += GetStartName(GetFileTypeByBase64(base64), "Temp\\");
//                if (!Directory.Exists("Temp")) Directory.CreateDirectory("Temp");
//                File.WriteAllBytes(Environment.CurrentDirectory + "\\Temp\\" + name, Convert.FromBase64String(base64));
//            }
//            catch (Exception ex)
//            {
//                WriteToLog("Can't restore file from text. [Name:" + name + "]", new StackTrace(), ex);
//            }
//        }
//        /// <summary>
//        /// Array of files will be converted to byte list
//        /// </summary>
//        void CreateFiles()
//        {
//            Directory.CreateDirectory("Unarchived");
//            if (!withName)
//            {
//                CreateFilesNoName(); return;
//            }
//            else//Named unarchive
//                try
//                {
//                    List<byte[]> bytearr = Base64ToBytes(currentFile);
//                    for (int i = 0; i < bytearr.Count; i++)
//                    {
//                        using (var imageFile = new FileStream("Unarchived/" + names[i], FileMode.Create))
//                        {
//                            try
//                            {
//                                imageFile.Write(bytearr[i], 0, bytearr[i].Length);
//                                imageFile.Flush();
//                            }
//                            catch (Exception ex)
//                            {
//                                WriteToLog("Can't create files with no name.", new StackTrace(), ex);
//                            }
//                        }
//                    }
//                }
//                catch (Exception ex)
//                {
//                    WriteToLog("Failed to restore files.", new StackTrace(), ex);
//                }
//        }
//        void CreateFilesNoName()
//        {
//            Directory.CreateDirectory("Unarchived");
//            List<byte[]> bytearr = Base64ToBytes(currentFile);
//            for (int i = 0; i < bytearr.Count; i++)
//            {
//                using (var imageFile = new FileStream("Unarchived/" + GetStartName(GetFileTypeByBase64(currentFile[i])), FileMode.Create))
//                {
//                    try
//                    {
//                        imageFile.Write(bytearr[i], 0, bytearr[i].Length);
//                        imageFile.Flush();
//                    }
//                    catch (Exception ex)
//                    {
//                        WriteToLog("Can't create files with no name.", new StackTrace(), ex);
//                    }
//                }
//            }
//        }
//        void CreateVideo(string name = "")
//        {
//            try
//            {
//                if (name.Length == 0) name = GetStartName(GetFileTypeByBase64(currentFile[currentRow]));
//                byte[] ret = Convert.FromBase64String(currentFile[currentRow]);
//                FileInfo fil = new FileInfo(name);
//                using (Stream sw = fil.OpenWrite())
//                {
//                    sw.Write(ret, 0, ret.Length);
//                    sw.Close();
//                }
//            }
//            catch (Exception ex)
//            {
//                WriteToLog("Failed to reveal video from base64.", new StackTrace(), ex);
//            }
//        }
//        void CreateVideoFromBase64(string base64 = "")
//        {
//            try
//            {
//                if (base64.Length == 0) base64 = GetFileTypeByBase64(currentFile[currentRow]);
//                byte[] ret = Convert.FromBase64String(currentFile[currentRow]);
//                FileInfo fil = new FileInfo(base64);
//                using (Stream sw = fil.OpenWrite())
//                {
//                    sw.Write(ret, 0, ret.Length);
//                    sw.Close();
//                }
//            }
//            catch (Exception ex)
//            {
//                WriteToLog("Failed to reveal video from base64.", new StackTrace(), ex);
//            }
//        }
//        private void btnChooseFolder_Click(object sender, RoutedEventArgs e)
//        {
//            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
//            fbd.Description = "Choose folder to add all files";
//            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
//            {
//                try
//                {
//                    WriteToLog("Cleared current file.");
//                    List<string> paths = Directory.GetFiles(fbd.SelectedPath).ToList();
//                    WriteToLog("Got all files from root.");
//                    if (withName)
//                        File.WriteAllLines(resPath, ConstructContent(FilesToBase64(paths), GetNamesByPaths(paths)));
//                    else
//                        File.WriteAllLines(resPath, FilesToBase64(paths));
//                    WriteToLog("Wrote new folder's content to result file.");
//                    var rawArr = File.ReadAllLines(resPath).ToList();
//                    currentFile = GetContent(rawArr);
//                    names = GetNames(rawArr);
//                    WriteToLog("Read new result file.");
//                    currentRow = 0;
//                    WriteToLog("Folder chosen to be written to dictionary: \"" + fbd.SelectedPath + "\"", new StackTrace());
//                }
//                catch (Exception ex)
//                {
//                    WriteToLog("Folder content reading error.", new StackTrace(), ex, "Try choosing smaller folder.");
//                    System.Windows.MessageBox.Show("This error usually shows up due to very large memory usage. If any issues - choose smaller folder. " + ex.Message);
//                }
//            }
//        }
//        List<string> GetNamesByPaths(List<string> paths)
//        {
//            for (int i = 0; i < paths.Count; i++)
//            {
//                paths[i] = GetFileName(paths[i]);
//            }
//            return paths;
//        }
//        private /*async*/ void btnAddElement_Click(object sender, RoutedEventArgs e)
//        {
//            try
//            {
//                System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
//                ofd.Title = "Choose file to add";
//                ofd.Filter = "All|*.*|Image|*.jpg;*.jpeg;*.png|Video|*.mp4";
//                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
//                {
//                    currentFile.Add(FileToBase64(ofd.FileName));
//                    names.Add(GetFileName(ofd.FileName).Replace(' ', specSymbol));
//                    MessageBox.Show(names[names.Count - 1]);
//                    //    await 
//                    //        Task.Run(() => );
//                    if (withName)
//                        File.AppendAllText(resPath, FileToBase64(ofd.FileName) + " " + GetFileName(ofd.FileName).Replace(' ', specSymbol) + Environment.NewLine);
//                    else File.AppendAllText(resPath, FileToBase64(ofd.FileName) + Environment.NewLine);
//                    //    await Task.Run(() => );
//                    //LoadRes();//Takes too much of memory
//                    WriteToLog("\"" + ofd.FileName + "\" added successfuly.", new StackTrace());
//                    System.Windows.Forms.MessageBox.Show("\"" + ofd.FileName + "\" added successfuly.");
//                }
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show("Error on new file adding: " + ex.Message + " " + ex.Source);
//                WriteToLog("Error on new file adding", new StackTrace(), ex);
//            }
//        }
//        private void btnRestoreFiles_Click(object sender, RoutedEventArgs e)
//        {
//            try
//            {
//                CreateFiles();
//                try { if (Directory.GetFiles("Unarchived").Length == 0) { CreateFilesNoName(); if (Directory.GetFiles("Unarchived").Length == 0) WriteToLog("Failed while restoring images from text. Try choosing smaller image directory (or image).", new StackTrace()); System.Windows.MessageBox.Show("Check your logs if you noticed some issues."); }; } catch { }
//            }
//            catch (Exception ex)
//            {
//                WriteToLog("Failed while restoring file from text.", new StackTrace(), ex);
//            }
//        }
//        async void LoadRes()//removed variable because it caused out of memory
//        {
//            try
//            {
//                currentFile = null;
//                names = null;
//                var arr = File.ReadAllLines(resPath).ToList();
//                try
//                {
//                    currentFile = GetContent(arr);
//                }
//                catch { WriteToLog("FUCKING CONTENT"); }
//                try
//                {
//                    names = GetNames(arr);
//                }
//                catch { WriteToLog("FUCKING NAMES"); }
//                if (currentFile.Count > 0)//If result file was empty that moment
//                    WriteToLog("Dictionary chosen is empty. [PATH:" + resPath + "]", new StackTrace());
//            }
//            catch (Exception ex) { MessageBox.Show("LoadRes():" + ex.Message); }
//        }
//        #region Switch
//        void MainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
//        {
//            try
//            {
//                if (e.Key == Key.Right) { Next(); }
//                else if (e.Key == Key.Left) { Prev(); }
//                else if (e.Key == Key.RightCtrl) { btnLoadFile_Click(sender, e); }
//                else if (e.Key == Key.RightShift) { btnAutoSwitch_Click(sender, e); }
//                else if (tbHeight.IsFocused || tbWidth.IsFocused)
//                {
//                    if (e.Key == Key.Enter) ApplyResolution(tbWidth.Text, tbHeight.Text);
//                }
//                else if (e.Key == Key.OemTilde) { Close(); Environment.Exit(0); }
//                else if (e.Key == Key.Escape) { Directory.Delete("Temp", true); Close(); }
//            }
//            catch (Exception ex)
//            {
//                WriteToLog("KeyDown error.", new StackTrace(), ex);
//            }
//        }
//        void PreviewKeyDownAnalyzer(object sender, System.Windows.Input.KeyEventArgs e)//Separate key analyzers work strangely
//        {
//            //https://coderoad.ru/24447602/%D0%9A%D0%B0%D0%BA-%D0%BE%D1%82%D0%BA%D0%BB%D1%8E%D1%87%D0%B8%D1%82%D1%8C-%D0%B8%D0%B7%D0%BC%D0%B5%D0%BD%D0%B5%D0%BD%D0%B8%D0%B5-%D1%84%D0%BE%D0%BA%D1%83%D1%81%D0%B0-%D1%81-%D0%BF%D0%BE%D0%BC%D0%BE%D1%89%D1%8C%D1%8E-%D0%BA%D0%BB%D0%B0%D0%B2%D0%B8%D1%88-%D1%81%D0%BE-%D1%81%D1%82%D1%80%D0%B5%D0%BB%D0%BA%D0%B0%D0%BC%D0%B8
//            try
//            {
//                if (currentFile.Count == 0) LoadRes();
//                if (e.Key == Key.Right)
//                {
//                    Next();
//                    e.Handled = true;
//                }
//                else if (e.Key == Key.Left)
//                {
//                    Prev();
//                    e.Handled = true;
//                }
//            }
//            catch (Exception ex)
//            {
//                WriteToLog("Failed to switch to next item, because list empty.", new StackTrace(), ex, "Try relaunching app.");
//            }
//        }
//        void EmptyFileHandle(Exception ex)
//        {
//            if (currentFile.Count == 0) { System.Windows.Forms.MessageBox.Show("Current file is empty"); WriteToLog("Failed to switch to previous item because file was not chosen or it is empty for some reason."); }
//            else WriteToLog("Failed to switch to previous item.", new StackTrace(), ex);
//        }
//        double LoadFileIfValid(string path)
//        {
//            double size = GetMBSize(path);
//            if (size < MAX_HANDLABLE_MB) LoadRes();
//            else SplitFile(resPath);
//            return size;
//        }
//        double GetMBSize(string path)
//        {
//            return new FileInfo(resPath).Length / 1000000;
//        }
//        void Prev()
//        {
//            btnLoadFile.Background = Brushes.LightGray;
//            if (currentFile.Count > 0) currentRow--;
//            else MessageBox.Show("File \"" + resPath + "\" is empty.");
//            vidFile.Source = null;
//            imgFile.Source = null;
//            if (currentRow < 0) currentRow = currentFile.Count - 1;
//            if (currentFile == null || currentFile.Count == 0) LoadRes();
//            if (!IsVideo(currentFile[currentRow]))
//            {
//                ShowPrimary();
//                imgFile.Source = BytesToBitmap(Convert.FromBase64String(currentFile[currentRow]));
//            }
//            else
//                btnLoadFile.Background = Brushes.Teal;//Why no impact
//        }
//        void Next()
//        {
//            btnLoadFile.Background = Brushes.LightGray;
//            if (currentFile.Count > 0) currentRow++;
//            else MessageBox.Show("File \"" + resPath + "\" is empty.");
//            vidFile.Source = null;
//            imgFile.Source = null;
//            if (currentRow >= currentFile.Count) currentRow = 0;
//            if (currentFile == null || currentFile.Count == 0) LoadRes();
//            if (!IsVideo(currentFile[currentRow]))
//            {
//                ShowPrimary();
//                imgFile.Source = BytesToBitmap(Convert.FromBase64String(currentFile[currentRow]));
//            }
//            else
//                btnLoadFile.Background = Brushes.Teal;//Why no impact
//        }
//        private void btnPrev_Click(object sender, RoutedEventArgs e)
//        {
//            try
//            {
//                StopTimer();
//                Prev();
//                if (!File.Exists(resPath)) { File.Create(resPath).Close(); MessageBox.Show("Resulting file was created anew, because file \"" + resPath + "\" was missing"); }
//                else if (currentFile.Count == 0) MessageBox.Show("File \"" + names[currentRow] + "\" is empty.");
//            }
//            catch (Exception ex)
//            {
//                WriteToLog("Failed to switch to next item, because list empty.", new StackTrace(), ex, "Try relaunching app.");
//            }
//            try
//            {
//                if (names[currentRow].Trim(' ').Length > 0)
//                    btnLoadFile.ToolTip = "#" + (currentRow + 1) + " with no name will be uploaded to preview.";
//                btnLoadFile.ToolTip = "#" + (currentRow + 1) + " \"" + names[currentRow] + "\" will be uploaded to preview.";
//            }
//            catch (Exception ex) { MessageBox.Show("btnPrev_Click, GetElSwMsg:" + ex.Message); }
//        }
//        private void btnNext_Click(object sender, RoutedEventArgs e)
//        {
//            try
//            {
//                StopTimer();
//                Next();
//                if (!File.Exists(resPath)) { File.Create(resPath).Close(); MessageBox.Show("Resulting file was created anew, because file \"" + resPath + "\" was missing"); }
//                else if (currentFile.Count == 0) MessageBox.Show("File \"" + names[currentRow] + "\" is empty.");
//            }
//            catch (Exception ex)
//            {
//                WriteToLog("Failed to switch to next item, because list empty.", new StackTrace(), ex, "Try relaunching app.");
//            }
//            try
//            {
//                if (names[currentRow].Trim(' ').Length > 0)
//                    btnLoadFile.ToolTip = "#" + (currentRow + 1) + " with no name will be uploaded to preview.";
//                btnLoadFile.ToolTip = "#" + (currentRow + 1) + " \"" + names[currentRow] + "\" will be uploaded to preview.";
//            }
//            catch (Exception ex) { MessageBox.Show("btnPrev_Click, GetElSwMsg:" + ex.Message); }
//        }
//        #endregion
//        void FilePicked()
//        {
//            string base64 = File.ReadLines(resPath).Skip(currentRow).First();
//            try { File.Create("filePicked.txt").Close(); } catch (Exception ex) { WriteToLog("Couldn't reach filepicked.txt", new StackTrace(), ex); }
//            File.WriteAllText("filePicked.txt", base64);
//            imgFile.Source = null;
//            vidFile.Source = null;
//        }
//        #region Resolution design
//        void ApplyResolution(string w, string h)
//        {
//            const int UPPER_LEFT_COUNT = 5, UPPER_RIGHT_COUNT = 4, BOTTOM_COUNT = 2;
//            int maxAppWidth = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width - 6;
//            int maxAppHeight = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height - 65;
//            double appResolution;
//            SetDefaults();
//            try
//            {
//                if (double.TryParse(w, out appResolution))
//                {
//                    if (appResolution > maxAppWidth) appResolution = maxAppWidth;
//                    double UPPER_LEFT_WIDTH = appResolution * 0.4, UPPER_RIGHT_WIDTH = appResolution * 0.5, MAIN_WIDTH = appResolution - btnNext.Width * 2, BOTTOM_WIDTH = appResolution;
//                    imgFile.Width = MAIN_WIDTH;
//                    vidFile.Width = MAIN_WIDTH;
//                    btnLoadFile.Width = UPPER_LEFT_WIDTH / UPPER_LEFT_COUNT;
//                    btnRestoreFiles.Width = UPPER_LEFT_WIDTH / UPPER_LEFT_COUNT;
//                    btnChooseFolder.Width = UPPER_LEFT_WIDTH / UPPER_LEFT_COUNT;
//                    btnChangeResult.Width = UPPER_LEFT_WIDTH / UPPER_LEFT_COUNT;
//                    btnSplitFile.Width = UPPER_LEFT_WIDTH / UPPER_LEFT_COUNT;
//                    btnApply.Width = UPPER_RIGHT_WIDTH / UPPER_RIGHT_COUNT * 1;
//                    tbHeight.Width = UPPER_RIGHT_WIDTH / UPPER_RIGHT_COUNT * 1.5;// /5*2
//                    tbWidth.Width = UPPER_RIGHT_WIDTH / UPPER_RIGHT_COUNT * 1.5;
//                    btnAddElement.Width = BOTTOM_WIDTH / BOTTOM_COUNT;
//                    btnRemoveElement.Width = BOTTOM_WIDTH / BOTTOM_COUNT;
//                    btnAutoSwitch.Width = appResolution;
//                    btnSplitFile.Margin = new Thickness(0, 0, appResolution * 0.1, 0);
//                    Width = appResolution + 16;
//                }
//                if (double.TryParse(h, out appResolution))
//                {
//                    if (appResolution > maxAppHeight) appResolution = maxAppHeight;
//                    appResolution = appResolution - btnAutoSwitch.Height - btnLoadFile.Height - btnAddElement.Height;//AppHeight includes unchanging upper panel 
//                    imgFile.Height = appResolution;
//                    vidFile.Height = appResolution;
//                    btnNext.Height = appResolution;
//                    btnPrev.Height = appResolution;
//                    Height = appResolution + btnLoadFile.Height + btnAutoSwitch.Height + btnAddElement.Height + 36;
//                }
//                WriteToLog("Max sizes for image were changed to W:" + imgFile.Width + " H:" + imgFile.Height, new StackTrace());
//            }
//            catch (Exception ex)
//            {
//                WriteToLog("Failed while applying new sizes to program elements.", new StackTrace(), ex);
//            }
//        }
//        private void btnApply_Click(object sender, RoutedEventArgs e)
//        {
//            ApplyResolution(tbWidth.Text, tbHeight.Text);
//        }
//        void BackToSize()
//        {
//            SizeToContent = SizeToContent.WidthAndHeight;//Restrict resize
//        }
//        void ApplyResolutionPropAll(string w, string h)
//        {
//            int maxAppWidth = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width - 6;
//            int maxAppHeight = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height - 115;
//            double appResolution;
//            SetDefaults();
//            try
//            {
//                if (double.TryParse(w, out appResolution))
//                {
//                    if (appResolution > maxAppWidth) appResolution = maxAppWidth;
//                    imgFile.Width = appResolution * 0.9;
//                    vidFile.Width = appResolution * 0.9;
//                    btnNext.Width = appResolution * 0.05;
//                    btnPrev.Width = appResolution * 0.05;
//                    btnRestoreFiles.Width = appResolution * 0.1;
//                    btnLoadFile.Width = appResolution * 0.1;
//                    btnChooseFolder.Width = appResolution * 0.1;
//                    btnChangeResult.Width = appResolution * 0.1;
//                    btnApply.Width = appResolution * 0.1;
//                    tbHeight.Width = appResolution * 0.2;
//                    tbWidth.Width = appResolution * 0.2;
//                    btnAddElement.Width = imgFile.Width / 2 + btnPrev.Width;
//                    btnRemoveElement.Width = imgFile.Width / 2 + btnPrev.Width;
//                    btnRestoreFiles.Margin = new Thickness(0, 0, btnPrev.Width * 2, 0);
//                    Width = appResolution + 16;
//                }
//                if (double.TryParse(h, out appResolution))
//                {
//                    if (appResolution > maxAppHeight) appResolution = maxAppHeight;
//                    imgFile.Height = appResolution * 0.9;
//                    vidFile.Height = appResolution * 0.9;
//                    btnNext.Height = appResolution * 0.9;
//                    btnPrev.Height = appResolution * 0.9;
//                    btnAddElement.Height = imgFile.Height * 0.05;
//                    btnRemoveElement.Height = imgFile.Height * 0.05;
//                    btnRestoreFiles.Height = appResolution * 0.05;
//                    btnLoadFile.Height = appResolution * 0.05;
//                    btnChooseFolder.Height = appResolution * 0.05;
//                    btnChangeResult.Height = appResolution * 0.05;
//                    btnApply.Height = appResolution * 0.05;
//                    tbHeight.Height = appResolution * 0.05;
//                    tbWidth.Height = appResolution * 0.05;
//                    Height = appResolution + 38 /*appResolution=btnPrev+btnAdd*/;
//                }
//                WriteToLog("Max sizes for image were changed to W:" + imgFile.Width + " H:" + imgFile.Height, new StackTrace());
//            }
//            catch (Exception ex)
//            {
//                WriteToLog("Failed while applying new sizes to program elements.", new StackTrace(), ex);
//            }
//        }
//        void ApplyResolutionSides(string w, string h)
//        {
//            int maxAppWidth = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width - 6;
//            int maxAppHeight = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height - 115;
//            double appResolution;
//            SetDefaults();
//            try
//            {
//                if (double.TryParse(w, out appResolution))
//                {
//                    if (appResolution > maxAppWidth) appResolution = maxAppWidth;
//                    imgFile.Width = appResolution * 0.9;
//                    btnNext.Width = appResolution * 0.05;
//                    btnPrev.Width = appResolution * 0.05;
//                    btnRestoreFiles.Width = appResolution * 0.1;
//                    btnLoadFile.Width = appResolution * 0.1;
//                    btnChooseFolder.Width = appResolution * 0.1;
//                    btnChangeResult.Width = appResolution * 0.1;
//                    btnApply.Width = appResolution * 0.1;
//                    tbHeight.Width = appResolution * 0.2;
//                    tbWidth.Width = appResolution * 0.2;
//                    btnAddElement.Width = imgFile.Width / 2 + btnPrev.Width;
//                    btnRemoveElement.Width = imgFile.Width / 2 + btnPrev.Width;
//                    btnRestoreFiles.Margin = new Thickness(0, 0, btnPrev.Width * 2, 0);
//                    Width = appResolution + 16;
//                }
//                if (double.TryParse(h, out appResolution))
//                {
//                    if (appResolution > maxAppHeight) appResolution = maxAppHeight;
//                    appResolution -= btnLoadFile.Height + btnAddElement.Height;
//                    imgFile.Height = appResolution;
//                    btnNext.Height = appResolution;
//                    btnPrev.Height = appResolution;
//                    Height = appResolution + btnLoadFile.Height + btnAddElement.Height + 38 /*appResolution=btnPrev+btnAdd*/;
//                }
//                WriteToLog("Max sizes for image were changed to W:" + imgFile.Width + " H:" + imgFile.Height, new StackTrace());
//            }
//            catch (Exception ex)
//            {
//                WriteToLog("Failed while applying new sizes to program elements.", new StackTrace(), ex);
//            }
//        }
//        void ApplyResolutionProp(string w, string h)
//        {
//            int maxAppWidth = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width - 6;
//            int maxAppHeight = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height - 115;
//            double appResolution;
//            SetDefaults();
//            try
//            {
//                if (double.TryParse(w, out appResolution))
//                {
//                    if (appResolution > maxAppWidth) appResolution = maxAppWidth;
//                    imgFile.Width = appResolution * 0.9;
//                    btnNext.Width = appResolution * 0.05;
//                    btnPrev.Width = appResolution * 0.05;
//                    btnRestoreFiles.Width = appResolution * 0.1;
//                    btnLoadFile.Width = appResolution * 0.1;
//                    btnChooseFolder.Width = appResolution * 0.1;
//                    btnChangeResult.Width = appResolution * 0.1;
//                    btnApply.Width = appResolution * 0.1;
//                    tbHeight.Width = appResolution * 0.2;
//                    tbWidth.Width = appResolution * 0.2;
//                    btnAddElement.Width = imgFile.Width / 2 + btnPrev.Width;
//                    btnRemoveElement.Width = imgFile.Width / 2 + btnPrev.Width;
//                    btnRestoreFiles.Margin = new Thickness(0, 0, btnPrev.Width * 2, 0);
//                    Width = appResolution + 16;
//                }
//                if (double.TryParse(h, out appResolution))
//                {
//                    if (appResolution > maxAppHeight) appResolution = maxAppHeight;
//                    imgFile.Height = appResolution * 0.95;
//                    btnNext.Height = appResolution * 0.95;
//                    btnPrev.Height = appResolution * 0.95;
//                    btnAddElement.Height = imgFile.Height * 0.05;
//                    btnRemoveElement.Height = imgFile.Height * 0.05;
//                    Height = appResolution + btnLoadFile.Height + 38 /*appResolution=btnPrev+btnAdd*/;
//                }
//                WriteToLog("Max sizes for image were changed to W:" + imgFile.Width + " H:" + imgFile.Height, new StackTrace());
//            }
//            catch (Exception ex)
//            {
//                WriteToLog("Failed while applying new sizes to program elements.", new StackTrace(), ex);
//            }
//        }
//        #endregion
//        #region Settings
//        string GetLastDictionary()
//        {
//            try
//            {
//                string[] settings = File.ReadAllLines(setPath);
//                foreach (string setting in settings)
//                {
//                    string[] settingParts = setting.Split('>');
//                    if (settingParts[0] == "LastDictionary") { WriteToLog("LastDictionary setting: " + settingParts[1], new StackTrace()); if (!File.Exists(settingParts[1])) File.Create(settingParts[1]); return settingParts[1]; }
//                }
//            }
//            catch (Exception ex)
//            {
//                WriteToLog("Can't get last dictionary path from settings.", new StackTrace(), ex, "Try changing dictionary again.");
//            }
//            return "result.txt";
//        }
//        string GetLastNames()
//        {
//            try
//            {
//                string[] settings = File.ReadAllLines(setPath);
//                foreach (string setting in settings)
//                {
//                    string[] settingParts = setting.Split('>');
//                    if (settingParts[0] == "LastNames") { WriteToLog("LastNames setting: " + settingParts[1], new StackTrace()); if (!File.Exists(settingParts[1])) File.Create(settingParts[1]); return settingParts[1]; }
//                }
//            }
//            catch (Exception ex)
//            {
//                WriteToLog("Can't get last dictionary path from settings.", new StackTrace(), ex, "Try changing dictionary again.");
//            }
//            return "result_namesfile.txt";
//        }
//        void AddSetting(string setname, string definition = "-", int addbeforeidx = 0)
//        {
//            if (!File.Exists(setPath)) File.Create(setPath);
//            List<string> settings = File.ReadAllLines(setPath).ToList();
//            foreach (string setting in settings)            //Uniqueness check
//            {
//                if (setting.Split('>')[0] == setname) return;
//            }
//            settings.Insert(addbeforeidx, setname.Replace('>', ' ') + ">" + definition.Replace('>', ' '));
//            WriteToLog(setname + " setting added", new StackTrace());
//            File.WriteAllLines(setPath, settings.ToArray());
//        }
//        bool EditSettings(string rowName, string definition)
//        {
//            if (!File.Exists(setPath)) File.Create(setPath);
//            string[] settings = File.ReadAllLines(setPath);
//            for (int i = 0; i < settings.Length; i++)
//            {
//                string[] rowCont = settings[i].Split('>');
//                if (rowCont[0] == rowName)
//                {
//                    settings[i] = rowCont[0] + ">" + definition;
//                    File.WriteAllLines(setPath, settings);
//                    return true;
//                }
//            }
//            return false;
//        }
//        bool EditSettings(int rowidx, string definition)
//        {
//            string[] filecont = File.ReadAllLines(setPath);
//            if (rowidx >= filecont.Length) return false;
//            string[] rowCont = filecont[rowidx].Split('>');
//            if (rowCont.Length <= 1) WriteToLog("It seems, that setting you are interested in contains an error. [Tip: Try clearing settings file.]", new StackTrace());
//            WriteToLog(rowCont[0] + " definiton edited from " + rowCont[1] + " to " + definition, new StackTrace());
//            filecont[rowidx] = rowCont[0] + ">" + definition;
//            File.WriteAllLines(setPath, filecont);
//            return true;
//        }
//        #endregion
//        #region Code not used 
//        List<string> BytesToFile(List<byte[]> bytes)
//        {
//            List<string> strfrombytes = new List<string>();
//            for (int i = 0; i < bytes.Count; i++)
//            {
//                strfrombytes.Add(Encoding.UTF8.GetString(bytes[i], i, bytes[i].Length));//Here was a problem when adding by index
//            }
//            return strfrombytes;
//        }
//        #endregion
//        private void btnChangeResult_Click(object sender, RoutedEventArgs e)
//        {
//            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
//            ofd.Title = "Choose text file to use as archive";
//            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
//            {
//                try
//                {
//                    WriteToLog("Resulting file path changed from: \"" + Environment.CurrentDirectory + resPath + "\" to \"" + ofd.FileName + "\" [Tip: if dictionary is not changing anyway - then try reopening the app]", new StackTrace());
//                    ResetDefaults();
//                    resPath = ofd.FileName;
//                    btnChangeResult.ToolTip = "Current: \"" + resPath + "\"";
//                    currentRow = -1;
//                    vidFile.Source = null;
//                    imgFile.Source = null;
//                    currentFile = null;
//                    LoadRes();
//                    btnLoadFile.ToolTip = "File will be updoaded to preview.";
//                    MessageBox.Show("Resulting file was changed to " + GetFileName(resPath));
//                    if (!EditSettings("LastDictionary", ofd.FileName)) AddSetting("LastDictionary", ofd.FileName);
//                    if (!EditSettings("LastNames", ofd.FileName.Replace(".txt", "_namesfile.txt"))) AddSetting("LastNames", ofd.FileName.Replace(".", "_namesfile."));
//                }
//                catch (Exception ex)
//                {
//                    WriteToLog("Fail on resulting txt change.", new StackTrace(), ex);
//                    MessageBox.Show("Couldn't change: " + ex.Message + " [PATH: " + ofd.FileName + "]");
//                }
//            }
//        }
//        public double GetFileSize(string path)
//        {
//            FileInfo fi = new FileInfo(path);
//            return (double)fi.Length;
//        }
//        public double GetFileSize(System.Windows.Forms.OpenFileDialog ofd)
//        {
//            FileInfo fi = new FileInfo(ofd.FileName);
//            return fi.Length;
//        }
//        public string GetFileHome(string path)
//        {
//            FileInfo fi = new FileInfo(path);
//            return fi.FullName.Substring(0, fi.FullName.Length - fi.Name.Length);
//        }
//        public string GetFilePath(string path)
//        {
//            FileInfo fi = new FileInfo(path);
//            return fi.Name;
//        }
//        public string GetFileHome(System.Windows.Forms.OpenFileDialog ofd)
//        {
//            FileInfo fi = new FileInfo(ofd.FileName);
//            return fi.FullName.Substring(0, fi.FullName.Length - fi.Name.Length);
//        }
//        public string GetFilePath(System.Windows.Forms.OpenFileDialog ofd)
//        {
//            FileInfo fi = new FileInfo(ofd.FileName);
//            return fi.FullName;
//        }
//        string GetFileName(string path)
//        {
//            try
//            {
//                for (int i = path.Length - 1; i >= 0; i--)
//                {
//                    if (path[i] == '\\')
//                    {
//                        return path.Substring(i + 1, path.Length - i - 1);
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                WriteToLog("Name was misdiected to path trimming.", new StackTrace(), ex);
//            }
//            return "PATH_IS_NAME";
//        }

//        private void btnRemoveElement_Click(object sender, RoutedEventArgs e)
//        {
//            try
//            {
//                if (System.Windows.MessageBox.Show("Do you intent to remove this element?", "Removing element.", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
//                {
//                    FilePicked();
//                    List<string> updfile = currentFile.ToList();
//                    updfile.RemoveAt(currentRow);
//                    if (withName)
//                        File.WriteAllLines(resPath, ConstructContent(updfile, names));
//                    else
//                        File.WriteAllLines(resPath, FilesToBase64(updfile));
//                    currentFile = updfile;
//                }
//            }
//            catch (Exception ex)
//            {
//                WriteToLog("Error on new file removing. Can't reach result file: " + resPath, new StackTrace(), ex);
//                return;
//            }
//            imgFile.Source = null;
//            currentRow = -1;
//            WriteToLog("File removed successfuly. Check \"filePicked.txt\"", new StackTrace());
//            System.Windows.Forms.MessageBox.Show("File removed.");
//        }

//        void HideAllModes()
//        {
//            imgFile.Visibility = Visibility.Collapsed;
//            vidFile.Visibility = Visibility.Collapsed;
//        }
//        void ShowSecondary()
//        {
//            imgFile.Source = null;
//            vidFile.Width = imgFile.Width;
//            vidFile.Height = imgFile.Height;
//            HideAllModes();
//            vidFile.Visibility = Visibility.Visible;
//        }
//        void LoadSecondary()
//        {
//            ShowSecondary();
//            try
//            {
//                vidFile.Source = Base64ToUri(currentFile[currentRow]);
//            }
//            catch (Exception ex)
//            {
//                WriteToLog("Failed to reveal video from base64.", new StackTrace(), ex);
//            }
//            WriteToLog("Attempt to display non-image file");
//        }
//        /// <summary>
//        /// Creates video and plays it previously switching player
//        /// </summary>
//        async void LoadSecondaryCreate()
//        {
//            StopTimer();
//            ShowSecondary();
//            if (!Directory.Exists(Environment.CurrentDirectory + "\\Temp")) Directory.CreateDirectory(Environment.CurrentDirectory + "\\Temp");
//            //if (withName) CreateVideo(Environment.CurrentDirectory + "\\Temp\\" + GetCurrName());
//            //else 
//            await Task.Run(() => CreateVideo(Environment.CurrentDirectory + "\\Temp\\cachedvid.mp4"));
//            try
//            {
//                vidFile.Source = new Uri(Environment.CurrentDirectory + "\\Temp\\cachedvid.mp4");
//                vidFile.LoadedBehavior = MediaState.Manual;
//                vidFile.Play();
//            }
//            catch (Exception ex)
//            {
//                WriteToLog("Unhandled exception.", new StackTrace(), ex);
//            }
//            WriteToLog("Attempt to display non-image file");
//        }
//        void ShowPrimary()
//        {
//            HideAllModes();
//            imgFile.Visibility = Visibility.Visible;
//            vidFile.Source = null;//Without it - can't display new
//        }
//        void LoadPrimary()
//        {
//            ShowPrimary();
//            imgFile.Source = BytesToBitmap(Convert.FromBase64String(currentFile[currentRow]));
//        }

//        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {/**/}

//        private void btnLoadFile_Click(object sender, RoutedEventArgs e)
//        {
//            if (currentRow < currentFile.Count && currentRow >= 0)
//            {
//                string type = GetFileTypeByBase64(currentFile[currentRow]);
//                try
//                {
//                    if (type == ".png" || type == ".jpg") LoadPrimary();
//                    else LoadSecondaryCreate();
//                    btnLoadFile.Background = Brushes.LightGray;
//                }
//                catch (Exception ex)
//                {
//                    try
//                    {
//                        LoadSecondaryCreate();
//                    }
//                    catch
//                    {
//                        System.Windows.MessageBox.Show("btnLoadFile_Click(): " + ex.Message + " " + type);
//                    }
//                }
//            }
//            else
//            {
//                WriteToLog("Can't load empty file", new StackTrace());
//                MessageBox.Show("Wrong index to load the element: " + currentRow);
//            }
//        }
//        bool IsVideo(string base64)
//        {
//            string type = GetFileTypeByBase64(base64);
//            return type == ".mp4" || type == ".avi";
//        }
//        private void btnAutoSwitch_Click(object sender, RoutedEventArgs e)
//        {
//            if (!IsVideo(currentFile[currentRow]) && btnLoadFile.Background != Brushes.Teal)
//            {
//                if (timer.IsEnabled)
//                {
//                    if (timer.Interval < TimeSpan.FromSeconds(1))
//                    {
//                        StopTimer();
//                    }
//                    else
//                    {
//                        timer.Interval = TimeSpan.FromSeconds(timer.Interval.TotalSeconds / 2);
//                        btnAutoSwitch.Content = "Auto switch (" + timer.Interval + ")";
//                    }
//                }
//                else
//                {
//                    timer.Interval = TimeSpan.FromSeconds(20);
//                    timer.Tick += timer_Tick;
//                    btnAutoSwitch.Background = Brushes.DarkGreen;
//                    btnAutoSwitch.Content = "Auto switch (" + timer.Interval + ")";
//                    timer.Start();
//                }
//                //MessageBox.Show("NotAVideo");
//            }
//            else
//            {
//                //MessageBox.Show("btnAutoSwitch_Click video");
//                SlowenVideo();
//                if (btnLoadFile.Background == Brushes.LightGray)
//                {
//                    btnAutoSwitch.Content = "Playback speed (" + playSpeed + ")";

//                }
//            }
//        }
//        void SpeedupVideo()
//        {
//            if (playSpeed == -1)
//            {
//                playSpeed = 0;
//                vidFile.SpeedRatio = playSpeed;
//            }
//            else
//            {
//                if (playSpeed >= 1) playSpeed += SPEEDUP_STEP;
//                else playSpeed += SPEEDUP_STEP / 2;
//                if (playSpeed > MAX_PLAY_SPEED || playSpeed < 0) playSpeed = 0;
//                vidFile.SpeedRatio = playSpeed;
//            }
//        }
//        void SlowenVideo()
//        {
//            if (playSpeed == -1)
//            {
//                playSpeed = MAX_PLAY_SPEED;
//                vidFile.SpeedRatio = playSpeed;
//            }
//            else
//            {
//                if (playSpeed > 1) playSpeed -= SPEEDUP_STEP;
//                else playSpeed -= SPEEDUP_STEP / 2;
//                if (playSpeed < 0) playSpeed = MAX_PLAY_SPEED;
//                else if (playSpeed > MAX_PLAY_SPEED) playSpeed = 0;
//                vidFile.SpeedRatio = playSpeed;
//            }
//        }

//        void StopTimer()
//        {
//            timer.Stop();
//            timer.Tick -= timer_Tick;
//            btnAutoSwitch.Content = "Auto switch";
//            btnAutoSwitch.Background = Brushes.LightGray;
//            playSpeed = -1;
//            vidFile.SpeedRatio = 1;
//        }
//        void ResetDefaults()
//        {
//            btnLoadFile.Background = Brushes.LightGray;
//            StopTimer();
//        }
//        void timer_Tick(object sender, EventArgs e)
//        {
//            Next();
//        }
//        void SplitFile(string filepath)
//        {
//            long inputLenght = new FileInfo(filepath).Length;
//            using (MemoryMappedFile mappedFile = MemoryMappedFile.CreateFromFile(filepath))
//            {
//                long firstLength = inputLenght / 2;
//                long secondLength = inputLenght - firstLength;

//                using (MemoryMappedViewStream viewStream = mappedFile.CreateViewStream(0, firstLength, MemoryMappedFileAccess.Read))
//                using (var fstream = File.Create(filepath.Replace(".", "1.")))
//                {
//                    viewStream.CopyTo(fstream);
//                }

//                using (MemoryMappedViewStream viewStream = mappedFile.CreateViewStream(firstLength, secondLength, MemoryMappedFileAccess.Read))
//                using (var fstream = File.Create(filepath.Replace(".", "2.")))
//                {
//                    viewStream.CopyTo(fstream);
//                }
//            }
//        }

//        private async void btnSplitFile_Click(object sender, RoutedEventArgs e)
//        {
//            await Task.Run(() => SplitFile(resPath));
//        }

//        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
//        {
//            try
//            {
//                BackToSize();
//            }
//            catch { }
//        }
//    }
//}