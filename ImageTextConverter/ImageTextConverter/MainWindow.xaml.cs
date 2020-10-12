using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Security.Cryptography;

namespace ImageTextConverter
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string resPath = "result.txt";
        string setPath = "settings.txt";
        string namesPath = "names.txt";
        int currentRow = -1;
        string[] currentFile;
        enum ImgType
        {
            PNG = 1,
            JPG = 2,
            JPEG = 3,
            BMP = 4
        }
        enum VidType
        {
            MP4 = 1
        }
        void SetDefaults()
        {
            btnLoadFile.Width = 50;
            btnLoadFile.Height = 25;
            btnChooseFolder.Width = 50;
            btnChooseFolder.Height = 25;
            btnChangeResult.Width = 50;
            btnChangeResult.Height = 25;
            btnRestoreFiles.Width = 50;
            btnRestoreFiles.Height = 25;
            tbWidth.Width = 100;
            tbWidth.Height = 25;
            tbHeight.Width = 100;
            tbHeight.Height = 25;

            btnPrev.Width = 30;
            btnPrev.Height = 450;
            imgFile.Width = 450;
            imgFile.Height = 450;
            btnNext.Width = 30;
            btnNext.Height = 450;

            btnAddElement.Width = 255;
            btnRemoveElement.Width = 255;
            btnAddElement.Height = 20;
            btnRemoveElement.Height = 20;

            Height = 534;
            Width = 526;
        }
        public MainWindow()
        {
            InitializeComponent();
            try
            {
                resPath = GetLastDictionary();
                currentFile = File.ReadAllLines(resPath);
                File.Create("logs.txt").Close();
                if (!File.Exists(setPath)) File.Create(setPath);
                if (!File.Exists(resPath)) File.Create(resPath).Close();
            }
            catch (Exception ex) { WriteToLog("Program files check/creation error.", new StackTrace(), ex); }
            SetDefaults();
            btnLoadFile.ToolTip = "Single file would be saved to *.txt chosen";
            btnChooseFolder.ToolTip = "Whole folder would be saved to *.txt chosen";
            btnRestoreFiles.ToolTip = "Whole file would be restored to folder \"Images\"";
            btnChangeResult.ToolTip = "Changes *.txt you are using to save or load files";

            KeyDown += new System.Windows.Input.KeyEventHandler(MainWindow_KeyDown);
        }
        void WriteToLog(string message, StackTrace st = null, Exception ex = null, string tip = "")
        {
            if (!File.Exists("logs.txt")) File.Create("logs.txt").Close();
            File.AppendAllText("logs.txt", "[" + DateTime.Now + "] ");
            if (st != null) File.AppendAllText("logs.txt", st.GetFrame(0).GetMethod().Name + "()");
            File.AppendAllText("logs.txt", "-> " + message.TrimEnd('.') + ".");
            if (ex != null) File.AppendAllText("logs.txt", " (" + ex.Message + ") ");
            if (tip.Length > 0) File.AppendAllText("logs.txt", "[Tip: " + tip.TrimEnd('.') + "]");
            File.AppendAllText("logs.txt", Environment.NewLine);
        }
        #region Convertation
        string FileToBase64(string path)
        {
            return Convert.ToBase64String(File.ReadAllBytes(path));
        }
        List<byte[]> Base64ToBytes(string[] base64s)
        {
            List<byte[]> bytes = new List<byte[]>();
            for (int i = 0; i < base64s.Length; i++)
            {
                bytes.Add(Convert.FromBase64String(base64s[i]));
            }
            return bytes;
        }
        byte[] Base64ToBytes(string base64)
        {
            try
            {
                return Convert.FromBase64String(base64);
            }
            catch (Exception ex)
            {
                WriteToLog("Error during converting base64-string to byte[].", new StackTrace(), ex);
            }
            return default;
        }
        Uri BytesToUri(byte[] arr)
        {
            string base64 = Encoding.UTF8.GetString(arr);
            byte[] bytes = Convert.FromBase64String(base64);
            return new Uri(Encoding.UTF8.GetString(bytes));
        }
        Uri Base64ToUri(string base64)
        {
            byte[] bytes = Convert.FromBase64String(base64);
            return new Uri(Encoding.UTF8.GetString(bytes));
        }
        string[] FilesToBase64(string[] paths)
        {
            string[] base64arr = new string[paths.Length];
            for (int i = 0; i < paths.Length; i++)
            {
                base64arr[i] = Convert.ToBase64String(File.ReadAllBytes(paths[i]));
            }
            return base64arr;
        }
        public BitmapImage BytesToBitmap(byte[] array)
        {
            using (var ms = new System.IO.MemoryStream(array))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad; // here
                image.StreamSource = ms;
                image.EndInit();
                return image;
            }
        }
        #endregion
        bool ArrayContains(string[] array, string element)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == element) return true;
            }
            return false;
        }
        string GetStartName(string type)
        {
            int i = 1;
            try
            {
                string[] filepaths = Directory.GetFiles("Images");
                for (int fp = 0; fp < filepaths.Length; fp++)
                {
                    filepaths[fp] = filepaths[fp].Replace("Images\\", "");
                }
                while (ArrayContains(filepaths, i.ToString() + type)) { i++; }//LOOK HERE IF ALL FILES SAVED WITH 1 SHARED NAME
                WriteToLog("Last name possible: " + i, new StackTrace());
                return i.ToString() + type;
            }
            catch (Exception ex)
            {
                WriteToLog("Failed to give possible name.", new StackTrace(), ex);
            }
            return i.ToString() + type;
        }
        string GetFileNameByIndex(int index, string[] filesNames)
        {
            try
            {
                if (index >= filesNames.Length)
                    return GetStartName(GetFileTypeByBase64(currentFile[index]));
                return filesNames[index];
            }
            catch
            {
                WriteToLog("Can't find name of file on index " + index, new StackTrace());
                return "NO_SUCH_FILE";
            }
        }
        string GetFileTypeByBase64(string base64)
        {
            var data = base64.Substring(0, 5);
            switch (data.ToUpper())
            {
                case "IVBOR":
                    return ".png";
                case "/9J/4":
                    return ".jpg";
                case "AAAAI":
                    return ".mp4";
                case "JVBER":
                    return ".pdf";
                case "AAABA":
                    return ".ico";
                case "UMFYI":
                    return ".rar";
                case "E1XYD":
                    return ".rtf";
                case "U1PKC":
                    return ".txt";
                case "MQOWM":
                case "77U/M":
                    return ".srt";
                default:
                    return "." + data;
            }
        }
        void CreateFiles()
        {
            List<byte[]> bytearr = Base64ToBytes(currentFile);
            Directory.CreateDirectory("Images");
            if (!File.Exists(namesPath))
            {
                CreateFilesNoName(); return;
            }
            else
                try
                {
                    string[] names = File.ReadAllLines(namesPath);
                    for (int i = 0; i < bytearr.Count; i++)
                    {
                        string fname = GetFileNameByIndex(i, names);
                        using (var imageFile = new FileStream("Images/" + fname, FileMode.Create))
                        {
                            imageFile.Write(bytearr[i], 0, bytearr[i].Length);
                            imageFile.Flush();
                        }
                        WriteToLog("File created \"" + fname + "\"", new StackTrace());
                    }
                }
                catch (Exception ex)
                {
                    WriteToLog("Failed to restore files.", new StackTrace(), ex);
                }
        }
        void CreateFilesNoName()
        {
            List<byte[]> bytearr = Base64ToBytes(currentFile);
            for (int i = 0; i < bytearr.Count; i++)
            {
                using (var imageFile = new FileStream("Images/" + GetStartName(GetFileTypeByBase64(currentFile[i])), FileMode.Create))
                {
                    try
                    {
                        imageFile.Write(bytearr[i], 0, bytearr[i].Length);
                        imageFile.Flush();
                    }
                    catch (Exception ex)
                    {
                        WriteToLog("Can't create files with no name.", new StackTrace(), ex);
                    }
                }
            }
        }
        void CreateVideo(string name = "")
        {
            try
            {
                if (name.Length == 0) name = GetStartName(GetFileTypeByBase64(currentFile[currentRow]));
                byte[] ret = Convert.FromBase64String(currentFile[currentRow]);
                FileInfo fil = new FileInfo(name);
                using (Stream sw = fil.OpenWrite())
                {
                    sw.Write(ret, 0, ret.Length);
                    sw.Close();
                }
            }
            catch (Exception ex)
            {
                WriteToLog("Failed to reveal video from base64.", new StackTrace(), ex);
            }
        }
        private void btnChooseFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (!File.Exists(resPath)) File.Create(resPath).Close();
                    if (!File.Exists(namesPath)) File.Create(namesPath).Close();
                    var paths = Directory.GetFiles(fbd.SelectedPath);
                    File.WriteAllLines(resPath, FilesToBase64(paths));
                    for (int i = 0; i < paths.Length; i++) paths[i] = GetFileName(paths[i]);
                    File.WriteAllLines(namesPath, paths);
                    currentFile = File.ReadAllLines(resPath);
                    currentRow = 0;
                    WriteToLog("Folder chosen to be written to dictionary: \"" + fbd.SelectedPath + "\"", new StackTrace());
                }
            }
            catch (Exception ex)
            {
                WriteToLog("Folder content reading error.", new StackTrace(), ex, "Try choosing smaller folder.");
                System.Windows.MessageBox.Show("This error usually shows up due to very large memory usage. Choose smaller folder.");
            }
        }
        private void btnAddFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (!File.Exists(resPath)) { File.Create(resPath).Close(); WriteToLog("Images-containing file created.", new StackTrace()); }
                    if (!File.Exists(namesPath)) { File.Create(namesPath).Close(); WriteToLog("Image names file created.", new StackTrace()); }
                    File.WriteAllText(resPath, FileToBase64(ofd.FileName) + Environment.NewLine);
                    currentFile = File.ReadAllLines(resPath);
                    currentRow = 0;
                    WriteToLog("File chosen to be written to dictionary: \"" + ofd.FileName + "\"", new StackTrace());
                }
            }
            catch (Exception ex)
            {
                WriteToLog("File reading error.", new StackTrace(), ex, "Try choosing smaller file.");
                System.Windows.MessageBox.Show("This error usually shows up due to very large memory usage. Choose smaller file.");
            }
        }
        private void btnRestoreFiles_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CreateFiles();
                if (Directory.GetFiles("Images").Length == 0) { WriteToLog("Failed while restoring images from text. Try choosing smaller image directory (or image).", new StackTrace()); System.Windows.MessageBox.Show("Check your logs if you noticed some issues."); };
            }
            catch (Exception ex)
            {
                WriteToLog("Failed while restoring file from text.", new StackTrace(), ex);
            }
        }
        void LoadFile()
        {
            currentFile = File.ReadAllLines(resPath);
            if (currentFile.Length > 0)//If result file was empty that moment
                WriteToLog("Dictionary chosen is empty. [PATH:" + resPath + "]", new StackTrace());
        }
        #region Switch
        void MainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Right) { Next(); }
            else if (e.Key == Key.Left) { Prev(); }
            else if (tbHeight.IsFocused || tbWidth.IsFocused)
            {
                if (e.Key == Key.Enter) ApplyResolution(tbWidth.Text, tbHeight.Text);
            }
            else if (e.Key == Key.OemTilde) { Close(); Environment.Exit(0); }
            else if (e.Key == Key.Escape) { if (File.Exists("cached.mp4")) File.Delete("cached.mp4"); Close(); }
        }
        void PreviewKeyDownAnalyzer(object sender, System.Windows.Input.KeyEventArgs e)//Separate key analyzers work strangely
        {
            //https://coderoad.ru/24447602/%D0%9A%D0%B0%D0%BA-%D0%BE%D1%82%D0%BA%D0%BB%D1%8E%D1%87%D0%B8%D1%82%D1%8C-%D0%B8%D0%B7%D0%BC%D0%B5%D0%BD%D0%B5%D0%BD%D0%B8%D0%B5-%D1%84%D0%BE%D0%BA%D1%83%D1%81%D0%B0-%D1%81-%D0%BF%D0%BE%D0%BC%D0%BE%D1%89%D1%8C%D1%8E-%D0%BA%D0%BB%D0%B0%D0%B2%D0%B8%D1%88-%D1%81%D0%BE-%D1%81%D1%82%D1%80%D0%B5%D0%BB%D0%BA%D0%B0%D0%BC%D0%B8
            try
            {
                if (currentFile.Length == 0) LoadFile();
                if (e.Key == Key.Right)
                {
                    Next();
                    e.Handled = true;
                }
                else if (e.Key == Key.Left)
                {
                    Prev();
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                WriteToLog("Failed to switch to next item, because list empty.", new StackTrace(), ex, "Try relaunching app.");
            }
        }
        void EmptyFileHandle(Exception ex)
        {
            if (currentFile.Length == 0) { System.Windows.Forms.MessageBox.Show("Current file is empty"); WriteToLog("Failed to switch to previous item because file was not chosen or it is empty for some reason."); }
            else WriteToLog("Failed to switch to previous item.", new StackTrace(), ex);
        }
        void Prev()
        {
            btnLoadFile.Background = Brushes.LightGray;
            try
            {
                currentRow--;
                if (currentRow < 0)
                    currentRow = currentFile.Length - 1;
                try
                {
                    LoadPrimary();
                }
                catch
                {
                    btnLoadFile.Background = Brushes.Teal;
                    ShowSecondary();
                }
            }
            catch (Exception ex)
            {
                FilePicked();
                EmptyFileHandle(ex);
            }
        }
        void Next()
        {
            btnLoadFile.Background = Brushes.LightGray;
            try
            {
                currentRow++;
                if (currentRow >= currentFile.Length)
                    currentRow = 0;
                try
                {
                    LoadPrimary();
                }
                catch
                {
                    btnLoadFile.Background = Brushes.Teal;
                    ShowSecondary();
                }
            }
            catch (Exception ex)
            {
                FilePicked();
                EmptyFileHandle(ex);
            }
        }
        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!File.Exists(resPath)) File.Create(resPath).Close();
                if (currentFile.Length == 0) LoadFile();
                Prev();
            }
            catch (Exception ex)
            {
                WriteToLog("Failed to switch to next item, because list empty.", new StackTrace(), ex, "Try relaunching app.");
            }
        }
        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!File.Exists(resPath)) File.Create(resPath).Close();
                if (currentFile.Length == 0) LoadFile();
                Next();
            }
            catch (Exception ex)
            {
                WriteToLog("Failed to switch to next item, because list empty.", new StackTrace(), ex, "Try relaunching app.");
            }
        }
        #endregion
        void FilePicked()
        {
            string base64 = currentFile[currentRow];
            File.Create("filePicked.txt").Close();
            File.WriteAllText("filePicked.txt", base64);
            imgFile.Source = null;
            vidFile.Source = null;
        }
        #region Resolution design
        void ApplyResolution(string w, string h)
        {
            int maxAppWidth = Screen.PrimaryScreen.Bounds.Width - 6;
            int maxAppHeight = Screen.PrimaryScreen.Bounds.Height - 65;
            double appResolution;
            SetDefaults();
            try
            {
                if (double.TryParse(w, out appResolution))
                {
                    if (appResolution > maxAppWidth) appResolution = maxAppWidth;
                    if (imgFile.Visibility == Visibility.Visible)
                        imgFile.Width = appResolution - btnNext.Width * 2;
                    else if (vidFile.Visibility == Visibility.Visible)
                        vidFile.Width = appResolution - btnNext.Width * 2;
                    btnRestoreFiles.Width = appResolution * 0.1;
                    btnLoadFile.Width = appResolution * 0.1;
                    btnChooseFolder.Width = appResolution * 0.1;
                    btnChangeResult.Width = appResolution * 0.1;
                    btnApply.Width = appResolution * 0.1;
                    tbHeight.Width = appResolution * 0.2;
                    tbWidth.Width = appResolution * 0.2;
                    btnAddElement.Width = appResolution * 0.5;
                    btnRemoveElement.Width = appResolution * 0.5;
                    btnRestoreFiles.Margin = new Thickness(0, 0, appResolution * 0.1, 0);
                    Width = appResolution + 16;
                }
                if (double.TryParse(h, out appResolution))
                {
                    if (appResolution > maxAppHeight) appResolution = maxAppHeight;
                    appResolution = appResolution - btnLoadFile.Height - btnAddElement.Height;//AppHeight includes unchanging upper panel 
                    imgFile.Height = appResolution;
                    btnNext.Height = appResolution;
                    btnPrev.Height = appResolution;
                    Height = appResolution + btnLoadFile.Height + btnAddElement.Height + 36;
                }
                WriteToLog("Max sizes for image were changed to W:" + imgFile.Width + " H:" + imgFile.Height, new StackTrace());
            }
            catch (Exception ex)
            {
                WriteToLog("Failed while applying new sizes to program elements.", new StackTrace(), ex);
            }
        }
        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            ApplyResolution(tbWidth.Text, tbHeight.Text);
        }
        void ApplyResolutionPropAll(string w, string h)
        {
            int maxAppWidth = Screen.PrimaryScreen.Bounds.Width - 6;
            int maxAppHeight = Screen.PrimaryScreen.Bounds.Height - 115;
            double appResolution;
            SetDefaults();
            try
            {
                if (double.TryParse(w, out appResolution))
                {
                    if (appResolution > maxAppWidth) appResolution = maxAppWidth;
                    imgFile.Width = appResolution * 0.9;
                    btnNext.Width = appResolution * 0.05;
                    btnPrev.Width = appResolution * 0.05;
                    btnRestoreFiles.Width = appResolution * 0.1;
                    btnLoadFile.Width = appResolution * 0.1;
                    btnChooseFolder.Width = appResolution * 0.1;
                    btnChangeResult.Width = appResolution * 0.1;
                    btnApply.Width = appResolution * 0.1;
                    tbHeight.Width = appResolution * 0.2;
                    tbWidth.Width = appResolution * 0.2;
                    btnAddElement.Width = imgFile.Width / 2 + btnPrev.Width;
                    btnRemoveElement.Width = imgFile.Width / 2 + btnPrev.Width;
                    btnRestoreFiles.Margin = new Thickness(0, 0, btnPrev.Width * 2, 0);
                    Width = appResolution + 16;
                }
                if (double.TryParse(h, out appResolution))
                {
                    if (appResolution > maxAppHeight) appResolution = maxAppHeight;
                    imgFile.Height = appResolution * 0.9;
                    btnNext.Height = appResolution * 0.9;
                    btnPrev.Height = appResolution * 0.9;
                    btnAddElement.Height = imgFile.Height * 0.05;
                    btnRemoveElement.Height = imgFile.Height * 0.05;
                    btnRestoreFiles.Height = appResolution * 0.05;
                    btnLoadFile.Height = appResolution * 0.05;
                    btnChooseFolder.Height = appResolution * 0.05;
                    btnChangeResult.Height = appResolution * 0.05;
                    btnApply.Height = appResolution * 0.05;
                    tbHeight.Height = appResolution * 0.05;
                    tbWidth.Height = appResolution * 0.05;
                    Height = appResolution + 38 /*appResolution=btnPrev+btnAdd*/;
                }
                WriteToLog("Max sizes for image were changed to W:" + imgFile.Width + " H:" + imgFile.Height, new StackTrace());
            }
            catch (Exception ex)
            {
                WriteToLog("Failed while applying new sizes to program elements.", new StackTrace(), ex);
            }
        }
        void ApplyResolutionSides(string w, string h)
        {
            int maxAppWidth = Screen.PrimaryScreen.Bounds.Width - 6;
            int maxAppHeight = Screen.PrimaryScreen.Bounds.Height - 115;
            double appResolution;
            SetDefaults();
            try
            {
                if (double.TryParse(w, out appResolution))
                {
                    if (appResolution > maxAppWidth) appResolution = maxAppWidth;
                    imgFile.Width = appResolution * 0.9;
                    btnNext.Width = appResolution * 0.05;
                    btnPrev.Width = appResolution * 0.05;
                    btnRestoreFiles.Width = appResolution * 0.1;
                    btnLoadFile.Width = appResolution * 0.1;
                    btnChooseFolder.Width = appResolution * 0.1;
                    btnChangeResult.Width = appResolution * 0.1;
                    btnApply.Width = appResolution * 0.1;
                    tbHeight.Width = appResolution * 0.2;
                    tbWidth.Width = appResolution * 0.2;
                    btnAddElement.Width = imgFile.Width / 2 + btnPrev.Width;
                    btnRemoveElement.Width = imgFile.Width / 2 + btnPrev.Width;
                    btnRestoreFiles.Margin = new Thickness(0, 0, btnPrev.Width * 2, 0);
                    Width = appResolution + 16;
                }
                if (double.TryParse(h, out appResolution))
                {
                    if (appResolution > maxAppHeight) appResolution = maxAppHeight;
                    appResolution -= btnLoadFile.Height + btnAddElement.Height;
                    imgFile.Height = appResolution;
                    btnNext.Height = appResolution;
                    btnPrev.Height = appResolution;
                    Height = appResolution + btnLoadFile.Height + btnAddElement.Height + 38 /*appResolution=btnPrev+btnAdd*/;
                }
                WriteToLog("Max sizes for image were changed to W:" + imgFile.Width + " H:" + imgFile.Height, new StackTrace());
            }
            catch (Exception ex)
            {
                WriteToLog("Failed while applying new sizes to program elements.", new StackTrace(), ex);
            }
        }
        void ApplyResolutionProp(string w, string h)
        {
            int maxAppWidth = Screen.PrimaryScreen.Bounds.Width - 6;
            int maxAppHeight = Screen.PrimaryScreen.Bounds.Height - 115;
            double appResolution;
            SetDefaults();
            try
            {
                if (double.TryParse(w, out appResolution))
                {
                    if (appResolution > maxAppWidth) appResolution = maxAppWidth;
                    imgFile.Width = appResolution * 0.9;
                    btnNext.Width = appResolution * 0.05;
                    btnPrev.Width = appResolution * 0.05;
                    btnRestoreFiles.Width = appResolution * 0.1;
                    btnLoadFile.Width = appResolution * 0.1;
                    btnChooseFolder.Width = appResolution * 0.1;
                    btnChangeResult.Width = appResolution * 0.1;
                    btnApply.Width = appResolution * 0.1;
                    tbHeight.Width = appResolution * 0.2;
                    tbWidth.Width = appResolution * 0.2;
                    btnAddElement.Width = imgFile.Width / 2 + btnPrev.Width;
                    btnRemoveElement.Width = imgFile.Width / 2 + btnPrev.Width;
                    btnRestoreFiles.Margin = new Thickness(0, 0, btnPrev.Width * 2, 0);
                    Width = appResolution + 16;
                }
                if (double.TryParse(h, out appResolution))
                {
                    if (appResolution > maxAppHeight) appResolution = maxAppHeight;
                    imgFile.Height = appResolution * 0.95;
                    btnNext.Height = appResolution * 0.95;
                    btnPrev.Height = appResolution * 0.95;
                    btnAddElement.Height = imgFile.Height * 0.05;
                    btnRemoveElement.Height = imgFile.Height * 0.05;
                    Height = appResolution + btnLoadFile.Height + 38 /*appResolution=btnPrev+btnAdd*/;
                }
                WriteToLog("Max sizes for image were changed to W:" + imgFile.Width + " H:" + imgFile.Height, new StackTrace());
            }
            catch (Exception ex)
            {
                WriteToLog("Failed while applying new sizes to program elements.", new StackTrace(), ex);
            }
        }
        #endregion
        #region Settings
        string GetLastDictionary()
        {
            try
            {
                string[] settings = File.ReadAllLines(setPath);
                foreach (string setting in settings)
                {
                    string[] settingParts = setting.Split('>');
                    if (settingParts[0] == "LastDictionary" && File.Exists(settingParts[1])) { WriteToLog("LastDictionary setting: " + settingParts[1], new StackTrace()); return settingParts[1]; }
                }
            }
            catch (Exception ex)
            {
                WriteToLog("Can't get last dictionary path from settings.", new StackTrace(), ex, "Try changing dictionary again.");
            }
            return resPath;
        }
        void AddSetting(string setname, string definition = "-", int addbeforeidx = 0)
        {
            if (!File.Exists(setPath)) File.Create(setPath);
            List<string> settings = File.ReadAllLines(setPath).ToList();
            foreach (string setting in settings)            //Uniqueness check
            {
                if (setting.Split('>')[0] == setname) return;
            }
            settings.Insert(addbeforeidx, setname.Replace('>', ' ') + ">" + definition.Replace('>', ' '));
            WriteToLog(setname + " setting added", new StackTrace());
            File.WriteAllLines(setPath, settings.ToArray());
        }
        bool EditSettings(string rowName, string definition)
        {
            string[] settings = File.ReadAllLines(setPath);
            for (int i = 0; i < settings.Length; i++)
            {
                string[] rowCont = settings[i].Split('>');
                if (rowCont[0] == rowName)
                {
                    settings[i] = rowCont[0] + ">" + definition;
                    File.WriteAllLines(setPath, settings);
                    return true;
                }
            }
            return false;
        }
        bool EditSettings(int rowidx, string definition)
        {
            string[] filecont = File.ReadAllLines(setPath);
            if (rowidx >= filecont.Length) return false;
            string[] rowCont = filecont[rowidx].Split('>');
            if (rowCont.Length <= 1) WriteToLog("It seems, that setting you are interested in contains an error. [Tip: Try clearing settings file.]", new StackTrace());
            WriteToLog(rowCont[0] + " definiton edited from " + rowCont[1] + " to " + definition, new StackTrace());
            filecont[rowidx] = rowCont[0] + ">" + definition;
            File.WriteAllLines(setPath, filecont);
            return true;
        }
        #endregion
        #region Code not used 
        List<string> BytesToFile(List<byte[]> bytes)
        {
            List<string> strfrombytes = new List<string>();
            for (int i = 0; i < bytes.Count; i++)
            {
                strfrombytes.Add(Encoding.UTF8.GetString(bytes[i], i, bytes[i].Length));//Here was a problem when adding by index
            }
            return strfrombytes;
        }
        #endregion

        private void btnChangeResult_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            try
            {
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    WriteToLog("Resulting file path changed from: \"" + Environment.CurrentDirectory + resPath + "\" to \"" + ofd.FileName + "\" [Tip: if dictionary is not changing anyway - then try reopening the app]", new StackTrace());
                    resPath = ofd.FileName;
                    if (!File.Exists(GetFileHome(resPath) + "names.txt"))
                        File.Create(GetFileHome(resPath) + "names.txt");
                    namesPath = GetFileHome(resPath) + "names.txt";
                    btnChangeResult.ToolTip = "Current: \"" + resPath + "\"";
                    currentFile = File.ReadAllLines(resPath);
                    currentRow = -1;
                    if (!EditSettings("LastDictionary", ofd.FileName)) AddSetting("LastDictionary", ofd.FileName);
                }
            }
            catch (Exception ex)
            {
                WriteToLog("Fail on resulting txt change.", new StackTrace(), ex);
            }
        }
        public double GetFileSize(string path)
        {
            FileInfo fi = new FileInfo(path);
            return (double)fi.Length;
        }
        public double GetFileSize(OpenFileDialog ofd)
        {
            FileInfo fi = new FileInfo(ofd.FileName);
            return fi.Length;
        }
        public string GetFileHome(string path)
        {
            FileInfo fi = new FileInfo(path);
            return fi.FullName.Substring(0, fi.FullName.Length - fi.Name.Length);
        }
        public string GetFilePath(string path)
        {
            FileInfo fi = new FileInfo(path);
            return fi.Name;
        }
        public string GetFileHome(OpenFileDialog ofd)
        {
            FileInfo fi = new FileInfo(ofd.FileName);
            return fi.FullName.Substring(0, fi.FullName.Length - fi.Name.Length);
        }
        public string GetFilePath(OpenFileDialog ofd)
        {
            FileInfo fi = new FileInfo(ofd.FileName);
            return fi.FullName;
        }
        string GetFileName(string path)
        {
            for (int i = path.Length - 1; i >= 0; i--)
            {
                if (path[i] == '\\')
                {
                    return path.Substring(i + 1, path.Length - i - 1);
                }
            }
            return "NO_NAME";
        }

        private void btnRemoveElement_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<string> updfile = currentFile.ToList();
                updfile.RemoveAt(currentRow);
                File.WriteAllLines(resPath, updfile);
                currentFile = updfile.ToArray();
            }
            catch (Exception ex)
            {
                WriteToLog("Error on new file removing. Can't reach result file: " + resPath, new StackTrace(), ex);
                return;
            }
            try
            {
                if (File.Exists(namesPath))
                {
                    var updnames = File.ReadAllLines(namesPath).ToList();
                    updnames.RemoveAt(currentRow);
                    File.WriteAllLines(namesPath, updnames);
                }
                else WriteToLog("Handled issue on new file removing. Can't reach names file: " + namesPath, new StackTrace());
            }
            catch (Exception ex)
            {
                WriteToLog("Unhandled issue on new file removing. [PATH: " + namesPath + "]", new StackTrace(), ex);
            }
            FilePicked();
            imgFile.Source = null;
            currentRow = -1;
            WriteToLog("File removed successfuly. Check \"filePicked.txt\"", new StackTrace());
            System.Windows.Forms.MessageBox.Show("File removed.");
        }

        private void btnAddElement_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var updfile = currentFile.ToList();
                OpenFileDialog ofd = new OpenFileDialog();
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        if (File.Exists(namesPath))
                        {
                            var updnames = File.ReadAllLines(namesPath).ToList();
                            updnames.Add(ofd.FileName);
                            File.WriteAllLines(namesPath, updnames);
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteToLog("Troubling to find names.txt -> it will not be changed", new StackTrace(), ex);
                    }
                }
                updfile.Add(FileToBase64(ofd.FileName));
                File.WriteAllLines(resPath, updfile);
                currentFile = updfile.ToArray();
                WriteToLog("File " + ofd.FileName + "added successfuly.", new StackTrace());
                System.Windows.Forms.MessageBox.Show("File added.");
            }
            catch (Exception ex)
            {
                WriteToLog("Error on new file adding", new StackTrace(), ex);
            }
        }

        void HideAllModes()
        {
            imgFile.Visibility = Visibility.Collapsed;
            vidFile.Visibility = Visibility.Collapsed;
        }
        void ShowSecondary()
        {
            imgFile.Source = null;
            vidFile.Width = imgFile.Width;
            vidFile.Height = imgFile.Height;
            HideAllModes();
            vidFile.Visibility = Visibility.Visible;
        }
        void LoadSecondary()
        {
            ShowSecondary();
            try
            {
                vidFile.Source = Base64ToUri(currentFile[currentRow]);
            }
            catch (Exception ex)
            {
                WriteToLog("Failed to reveal video from base64.", new StackTrace(), ex);
            }
            WriteToLog("Attempt to display non-image file");
        }
        void LoadSecondaryCached()
        {
            ShowSecondary();
            CreateVideo("cached.mp4");
            try
            {
                vidFile.Source = new Uri(Environment.CurrentDirectory + "/" + "cached.mp4");
                vidFile.LoadedBehavior = MediaState.Manual;
                vidFile.Play();
            }
            catch (Exception ex)
            {
                WriteToLog("Unhandled exception.", new StackTrace(), ex);
            }
            WriteToLog("Attempt to display non-image file");
        }
        void ShowPrimary()
        {
            HideAllModes();
            imgFile.Width = vidFile.Width;
            imgFile.Height = vidFile.Height;
            imgFile.Visibility = Visibility.Visible;
            vidFile.Source = null;//Without it - can't display new
        }
        void LoadPrimary()
        {
            ShowPrimary();
            imgFile.Source = BytesToBitmap(Convert.FromBase64String(currentFile[currentRow]));
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {/**/}

        private void btnLoadFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (imgFile.Visibility == Visibility.Visible) LoadPrimary();
                else if (vidFile.Visibility == Visibility.Visible) LoadSecondaryCached();
                btnLoadFile.Background = Brushes.LightGray;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Press \"load\" to preview file. " + ex.Message + " " + GetFileTypeByBase64(currentFile[currentRow]));
            }
        }
    }
}