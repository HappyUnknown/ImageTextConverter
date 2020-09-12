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
            File.Create("logs.txt").Close();
            if (!File.Exists(resPath)) File.Create(resPath).Close();
            currentFile = File.ReadAllLines(resPath);
        }
        void WriteToLog(string message, string functionName = "", string exmsg = "", string tip = "")
        {
            if (!File.Exists("logs.txt")) File.Create("logs.txt").Close();
            File.AppendAllText("logs.txt", "[" + DateTime.Now + "] ");
            if (functionName.Length > 0) File.AppendAllText("logs.txt", functionName + "()");
            File.AppendAllText("logs.txt", "-> " + message);
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
        int GetStartName(ImgType it = ImgType.PNG)
        {
            int i = 1;
            string[] filepaths = Directory.GetFiles("Images");
            while (ArrayContains(filepaths, "Images\\" + i + GetImgType(it))) i++;
            return i;
        }
        void CreateFile(List<byte[]> bytearr, ImgType type = ImgType.PNG)
        {
            try
            {
                Directory.CreateDirectory("Images");
                for (int i = 1; i < bytearr.Count; i++)
                {
                    using (var imageFile = new FileStream("Images/" + (i + GetStartName() - 1) + GetImgType(type), FileMode.Create))
                    {
                        imageFile.Write(bytearr[i], 0, bytearr[i].Length);
                        imageFile.Flush();
                    }
                }
            }
            catch (Exception ex)
            {
                WriteToLog("Failed to restore file.", new StackTrace().GetFrame(0).GetMethod().Name, ex.Message);
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
                    var paths = Directory.GetFiles(fbd.SelectedPath);
                    File.WriteAllLines(resPath, FilesToBase64(paths));
                    currentFile = File.ReadAllLines(resPath);
                    currentRow = 0;
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
                    if (!File.Exists(resPath)) File.Create(resPath).Close();
                    File.WriteAllText(resPath, FileToBase64(ofd.FileName) + Environment.NewLine);
                    currentFile = File.ReadAllLines(resPath);
                    currentRow = 0;
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
                CreateFile(bytes);
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
                else WriteToLog("Failed to switch to previous item.", new StackTrace().GetFrame(0).GetMethod().Name, ex.Message);
            }
        }
        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            Prev();
        }
        void Next()
        {
            try
            {
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
                else WriteToLog("Failed to switch to next item.", new StackTrace().GetFrame(0).GetMethod().Name, ex.Message);
            }
        }
        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            Next();
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
            int parsed;
            try
            {
                if (int.TryParse(tbWidth.Text, out parsed))
                {
                    imgFile.Width = parsed;
                    btnRestoreFiles.Width = parsed / 4.5;
                    btnChooseFile.Width = parsed / 9;
                    btnChooseFolder.Width = parsed / 9;
                    btnApply.Width = parsed / 9;
                    tbHeight.Width = parsed / 4.5;
                    tbWidth.Width = parsed / 4.5;
                    btnNext.Width = parsed / 9;
                    btnPrev.Width = parsed / 9;
                    btnRestoreFiles.Margin = new Thickness(0, 0, parsed / 15 + btnPrev.Width * 1.4, 0);
                    Width = btnNext.Width + btnPrev.Width + parsed + 16;
                }
                if (int.TryParse(tbHeight.Text, out parsed))
                {
                    imgFile.Height = parsed;
                    btnNext.Height = parsed;
                    btnPrev.Height = parsed;

                    Height = parsed + btnRestoreFiles.Height + 39;
                }
            }
            catch (Exception ex)
            {
                WriteToLog("Failed while applying new sizes to program elements.", new StackTrace().GetFrame(0).GetMethod().Name, ex.Message);
            }

        }
    }
}