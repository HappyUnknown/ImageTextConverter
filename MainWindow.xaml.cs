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
            catch (Exception ex) { WriteToLog("Program files check/creation error.", "MainConstructor", ex.Message); }

            btnChooseFile.ToolTip = "Single file would be saved to .txt chosen";
            btnChooseFolder.ToolTip = "Whole folder would be saved to .txt chosen";
            btnRestoreFiles.ToolTip = "Whole file would be restored to folder \"Images\"";
            btnChangeResult.ToolTip = "Changes .txt you are using to save or load files";
        }
        void WriteToLog(string message, string functionName = "", string exmsg = "", string tip = "")
        {
            if (!File.Exists("logs.txt")) File.Create("logs.txt").Close();
            File.AppendAllText("logs.txt", "[" + DateTime.Now + "] ");
            if (functionName.Length > 0) File.AppendAllText("logs.txt", functionName + "()");
            File.AppendAllText("logs.txt", "-> " + message.TrimEnd('.') + ".");
            if (exmsg.Length > 0) File.AppendAllText("logs.txt", " (" + exmsg + ") ");
            if (tip.Length > 0) File.AppendAllText("logs.txt", "[Tip: " + tip.TrimEnd('.') + "]");
            File.AppendAllText("logs.txt", Environment.NewLine);
        }
        string[] GetKnownTypes()
        {
            return new string[] { ".png", ".jpg", ".jpeg", ".bmp" };
        }
        string GetImgType(ImgType type)
        {
            string[] types = GetKnownTypes();
            if ((int)type < types.Length && (int)type > 0)
                return types[(int)type];
            return types[0];
        }
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
        string[] FilesToBase64(string[] paths)
        {
            string[] base64arr = new string[paths.Length];
            for (int i = 0; i < paths.Length; i++)
            {
                base64arr[i] = Convert.ToBase64String(File.ReadAllBytes(paths[i]));
            }
            return base64arr;
        }
        bool ArrayContains(string[] array, string element)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == element) return true;
            }
            return false;
        }
        string GetStartName(ImgType it = ImgType.PNG)
        {
            int i = 1;
            string[] filepaths = Directory.GetFiles("Images");
            for (int fp = 0; fp < filepaths.Length; fp++)
            {
                filepaths[fp] = filepaths[fp].Replace("Images\\", "");
            }
            while (ArrayContains(filepaths, i + GetImgType(it))) { i++; }
            WriteToLog("Last name possible: " + i + GetImgType(it) + " ~\"" + filepaths[i] + "\"!=\"" + i + GetImgType(it) + "\"~ ", new StackTrace().GetFrame(0).GetMethod().Name);
            return i + GetImgType(it);
        }
        string GetFileNameByIndex(int index, string[] filesNames = null)
        {
            try
            {
                if (filesNames == null) filesNames = File.ReadAllLines(namesPath);
                string[] names = filesNames;
                if (index >= names.Length)
                    return GetStartName(ImgType.PNG).ToString();
                return names[index];
            }
            catch
            {
                WriteToLog("Can't find name of file on index " + index, new StackTrace().GetFrame(0).GetMethod().Name);
                return "NO_SUCH_FILE";
            }
        }
        void CreateFiles(List<byte[]> bytearr, ImgType type = ImgType.PNG)
        {
            Directory.CreateDirectory("Images");
            try
            {
                string[] names = File.ReadAllLines(namesPath);
                for (int i = 0; i < bytearr.Count; i++)
                {
                    string fname = GetFileNameByIndex(i, names);
                    if (File.Exists(fname)) { fname = GetStartName(ImgType.PNG).ToString(); WriteToLog(fname + " file exists. Possible numeric name given.", new StackTrace().GetFrame(0).GetMethod().Name); }
                    using (var imageFile = new FileStream("Images/" + fname, FileMode.Create))
                    {
                        imageFile.Write(bytearr[i], 0, bytearr[i].Length);
                        imageFile.Flush();
                    }
                    WriteToLog("File created \"" + fname + "\"", new StackTrace().GetFrame(0).GetMethod().Name);
                }
            }
            catch (Exception ex)
            {
                WriteToLog("Failed to restore files.", new StackTrace().GetFrame(0).GetMethod().Name, ex.Message);
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
                    WriteToLog("Folder chosen to be written to dictionary: \"" + fbd.SelectedPath + "\"", new StackTrace().GetFrame(0).GetMethod().Name);
                }
            }
            catch (Exception ex)
            {
                WriteToLog("Folder content reading error.", new StackTrace().GetFrame(0).GetMethod().Name, ex.Message, "Try choosing smaller folder.");
                System.Windows.MessageBox.Show("This error usually shows up due to very large memory usage. Choose smaller folder.");
            }
        }
        private void btnChooseFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (!File.Exists(resPath)) { File.Create(resPath).Close(); WriteToLog("Images-containing file created.", new StackTrace().GetFrame(0).GetMethod().Name); }
                    if (!File.Exists(namesPath)) { File.Create(namesPath).Close(); WriteToLog("Image names file created.", new StackTrace().GetFrame(0).GetMethod().Name); }
                    File.WriteAllText(resPath, FileToBase64(ofd.FileName) + Environment.NewLine);
                    currentFile = File.ReadAllLines(resPath);
                    currentRow = 0;
                    WriteToLog("File chosen to be written to dictionary: \"" + ofd.FileName + "\"", new StackTrace().GetFrame(0).GetMethod().Name);
                }
            }
            catch (Exception ex)
            {
                WriteToLog("File reading error.", new StackTrace().GetFrame(0).GetMethod().Name, ex.Message, "Try choosing smaller file.");
                System.Windows.MessageBox.Show("This error usually shows up due to very large memory usage. Choose smaller file.");
            }
        }
        private void btnRestoreFiles_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<byte[]> bytes = Base64ToBytes(currentFile);
                CreateFiles(bytes);
                if (Directory.GetFiles("Images").Length == 0) { WriteToLog("Failed while restoring images from text. Try choosing smaller image directory (or image).", new StackTrace().GetFrame(0).GetMethod().Name); System.Windows.MessageBox.Show("Check your logs if you noticed some issues."); };
            }
            catch (Exception ex)
            {
                WriteToLog("Failed while restoring file from text.", new StackTrace().GetFrame(0).GetMethod().Name, ex.Message);
            }
        }
        void Prev()
        {
            try
            {
                currentRow--;
                if (currentRow < 0)
                    currentRow = currentFile.Length - 1;
                string thisbase64 = currentFile[currentRow];
                byte[] bytes = Convert.FromBase64String(thisbase64);
                imgFile.Source = BytesToBitmap(bytes);
            }
            catch (Exception ex)
            {
                imgFile.Source = null;
                if (currentFile.Length == 0) { System.Windows.Forms.MessageBox.Show("Current file is empty"); WriteToLog("Failed to switch to previous item because file was not chosen or it is empty for some reason."); }
                else WriteToLog("Failed to switch to previous item.", "Prev", ex.Message);
            }
        }
        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //WriteToLog("Loading prev from -> " + resPath);
                if (currentFile.Length == 0)
                {
                    currentFile = File.ReadAllLines(resPath);
                    if (currentFile.Length > 0)//If result file was empty that moment
                        WriteToLog(resPath + " content cached.", new StackTrace().GetFrame(0).GetMethod().Name);
                }
                Prev();
            }
            catch (Exception ex)
            {
                WriteToLog("Failed to switch to previous item, because list empty.", "btnPrev_Click", ex.Message, "Try relaunching app.");
            }
        }
        void Next()
        {
            try
            {
                //WriteToLog("Loading next from -> " + resPath);
                currentRow++;
                if (currentRow >= currentFile.Length)
                    currentRow = 0;
                string thisbase64 = currentFile[currentRow];
                byte[] bytes = Convert.FromBase64String(thisbase64);
                imgFile.Source = BytesToBitmap(bytes);
            }
            catch (Exception ex)
            {
                imgFile.Source = null;
                if (currentFile.Length == 0) { System.Windows.Forms.MessageBox.Show("Current file is empty"); WriteToLog("Failed to switch to next item because file was not chosen or it is empty for some reason."); }
                else WriteToLog("Failed to switch to next item.", "Next", ex.Message);
            }
        }
        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (currentFile.Length == 0)
                {
                    currentFile = File.ReadAllLines(resPath);
                    if (currentFile.Length > 0)//If result file was empty that moment
                        WriteToLog(resPath + " content cached.", new StackTrace().GetFrame(0).GetMethod().Name);
                }
                Next();
            }
            catch (Exception ex)
            {
                WriteToLog("Failed to switch to next item, because list empty.", "btnNext_Click", ex.Message, "Try relaunching app.");
            }
        }
        #region Bitmap
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
        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            int screenWidth = Screen.PrimaryScreen.Bounds.Width;
            int screenHeight = Screen.PrimaryScreen.Bounds.Height;
            double parsed;
            try
            {
                if (double.TryParse(tbWidth.Text, out parsed))
                {
                    if (parsed + parsed / 9 + parsed / 9 + 2 > screenWidth) parsed = screenWidth /*- screenWidth / 9 - 2*/;
                    imgFile.Width = parsed;
                    btnRestoreFiles.Width = parsed / 4.5;
                    btnChooseFile.Width = parsed / 9;
                    btnChooseFolder.Width = parsed / 9;
                    btnChangeResult.Width = parsed / 9;
                    btnApply.Width = parsed / 9;
                    tbHeight.Width = parsed / 4.5;
                    tbWidth.Width = parsed / 4.5;
                    btnNext.Width = parsed / 9;
                    btnPrev.Width = parsed / 9;
                    btnChangeResult.Margin = new Thickness(0, 0, parsed / 15 + btnPrev.Width * 0.4, 0);
                    Width = btnNext.Width + btnPrev.Width + parsed + 16;
                }
                if (double.TryParse(tbHeight.Text, out parsed))
                {
                    if (parsed + btnChooseFile.Height + 20 > screenHeight) parsed = screenHeight;
                    imgFile.Height = parsed;
                    btnNext.Height = parsed;
                    btnPrev.Height = parsed;
                    Height = parsed + btnRestoreFiles.Height + 39;

                }
                WriteToLog("Max sizes for image were changed to W:" + imgFile.Width + " H:" + imgFile.Height, new StackTrace().GetFrame(0).GetMethod().Name);
            }
            catch (Exception ex)
            {
                WriteToLog("Failed while applying new sizes to program elements.", new StackTrace().GetFrame(0).GetMethod().Name, ex.Message);
            }

        }
        #region Settings
        string GetLastDictionary()
        {
            try
            {
                string[] settings = File.ReadAllLines(setPath);
                foreach (string setting in settings)
                {
                    string[] settingParts = setting.Split('>');
                    if (settingParts[0] == "LastDictionary" && File.Exists(settingParts[1])) { WriteToLog("LastDictionary setting: " + settingParts[1], new StackTrace().GetFrame(0).GetMethod().Name); return settingParts[1]; }
                }
            }
            catch (Exception ex)
            {
                WriteToLog("Can't get last dictionary path from settings.", new StackTrace().GetFrame(0).GetMethod().Name, ex.Message, "Try changing dictionary again.");
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
            WriteToLog(setname + " setting added", new StackTrace().GetFrame(0).GetMethod().Name);
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
            if (rowCont.Length <= 1) WriteToLog("It seems, that setting you are interested in contains an error. [Tip: Try clearing settings file.]", new StackTrace().GetFrame(0).GetMethod().Name);
            WriteToLog(rowCont[0] + " definiton edited from " + rowCont[1] + " to " + definition, new StackTrace().GetFrame(0).GetMethod().Name);
            filecont[rowidx] = rowCont[0] + ">" + definition;
            File.WriteAllLines(setPath, filecont);
            return true;
        }
        #endregion
        #region Code not used 
        bool TypeKnown(string fname)
        {
            string[] knownTypes = GetKnownTypes();
            for (int i = 0; i < knownTypes.Length; i++)
                if (fname.Substring(fname.Length - knownTypes[i].Length, knownTypes[i].Length) == knownTypes[i]) return true;
            return false;
        }
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
                    WriteToLog("Resulting file path changed from: \"" + Environment.CurrentDirectory + resPath + "\" to \"" + ofd.FileName + "\" [Tip: if dictionary is not changing anyway - then try reopening the app]", new StackTrace().GetFrame(0).GetMethod().Name);
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
                WriteToLog("Fail on resulting txt change.", new StackTrace().GetFrame(0).GetMethod().Name, ex.Message);
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
    }
}