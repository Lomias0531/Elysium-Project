namespace FishSqueezer
{
    using Newtonsoft.Json;
    using System.Numerics;
    using System.Drawing;
    using System.IO;
    using System.Windows.Forms;
    using MSWord = Microsoft.Office.Interop.Word;
    using System.Reflection;

    public partial class Form1 : Form
    {
        string configPath;

        Config Config;

        System.Drawing.Point point;
        bool isMoving = false;

        string tempFilePath = "";
        string StoredPassword;
        List<string> AvailableExtensions = new List<string>()
        {
            ".txt",
            ".docx",
            ".doc",
        };
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var folderPath = System.Windows.Forms.Application.StartupPath;
            configPath = folderPath + "\\" + "config.txt";

            if (!File.Exists(configPath))
            {
                Config = new Config()
                {
                    TextPath = folderPath + "\\text.txt",
                    StartUpLocation = new Point(50, 50),
                    WindowSize = new Size(300, 50),
                    BackgroundColor = "#262626",
                    FontColor = "#8a8a8a",
                    Anchor = 0,
                };
            }
            else
            {
                var configText = File.ReadAllText(configPath);
                Config = JsonConvert.DeserializeObject<Config>(configText);
            }

            if (File.Exists(Config.TextPath))
            {
                TextEditer.Text = File.ReadAllText(Config.TextPath);
                TextEditer.SelectionStart = Config.Anchor;
                TextEditer.ScrollToCaret();
            }
            else
            {
                Config.TextPath = folderPath + "\\text.txt";
            }

            Location = Config.StartUpLocation;
            Size = Config.WindowSize;

            if (!string.IsNullOrEmpty(Config.BackgroundColor))
            {
                TextEditer.BackColor = System.Drawing.ColorTranslator.FromHtml(Config.BackgroundColor);
                this.BackColor = System.Drawing.ColorTranslator.FromHtml(Config.BackgroundColor);
            }
            if (!string.IsNullOrEmpty(Config.FontColor))
                TextEditer.ForeColor = System.Drawing.ColorTranslator.FromHtml(Config.FontColor);
            //this.Size = Config.WindowSize;
            this.Size = new Size(Config.WindowSize.Width + 10, Config.WindowSize.Height);
            TextEditer.Size = Config.WindowSize;

        }
        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            point = e.Location;
            isMoving = true;
        }
        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && isMoving)
            {
                Point newP = new Point(e.Location.X - point.X, e.Location.Y - point.Y);
                Location += new Size(newP);
            }
        }
        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            isMoving = false;
            Config.StartUpLocation = Location;
            Config.Anchor = TextEditer.SelectionStart;
        }
        private void Form1_DropEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }
        private void Form1_FileDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            TextEditer.AllowDrop = false;
            if (files.Length > 0)
            {
                var extension = Path.GetExtension(files[0]);
                if (AvailableExtensions.Contains(extension))
                {
                    tempFilePath = files[0];
                    switch (extension)
                    {
                        default:
                            {
                                break;
                            }
                        case ".txt":
                            {
                                LoadTxt(files[0]);
                                break;
                            }
                        case ".docx":
                            {
                                LoadDoc(files[0]);
                                break;
                            }
                        case ".doc":
                            {
                                LoadDoc(files[0]);
                                break;
                            }
                    }
                }
            }
            TextEditer.AllowDrop = true;
        }
        void LoadTxt(string filePath)
        {
            TextEditer.Text = File.ReadAllText(filePath);
        }
        void LoadDoc(string filePath)
        {
            try
            {
                MSWord.Application app = new MSWord.Application();
                MSWord.Document doc = null;

                object missing = System.Reflection.Missing.Value;
                object File = filePath;
                object readOnly = false;//²»ÊÇÖ»¶Á
                object isVisible = true;

                object password = TextEditer.Text;
                StoredPassword = TextEditer.Text;

                object unknow = Type.Missing;

                try
                {
                    doc = app.Documents.Open(ref File, ref missing, ref readOnly,
                     ref missing, ref password, ref missing, ref missing, ref missing,
                     ref missing, ref missing, ref missing, ref isVisible, ref missing,
                     ref missing, ref missing, ref missing);

                    TextEditer.Text = doc.Content.Text;
                }
                finally
                {
                    if (doc != null)
                    {
                        doc.Close(ref missing, ref missing, ref missing);
                        doc = null;
                    }

                    if (app != null)
                    {
                        app.Quit(ref missing, ref missing, ref missing);
                        app = null;
                    }
                }
            }
            catch(Exception ex)
            {
                TextEditer.Text = ex.Message;
            }
            

            TextEditer.SelectionStart = TextEditer.Text.Length;
            TextEditer.ScrollToCaret();
        }
        private void Form1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            OnApplicationQuit();
            Application.Exit();
        }

        private void OnApplicationQuit()
        {
            if (string.IsNullOrEmpty(tempFilePath))
            {
                File.WriteAllText(Config.TextPath, TextEditer.Text);
            }
            else
            {
                var extension = Path.GetExtension(tempFilePath);
                switch (extension)
                {
                    default:
                        {
                            break;
                        }
                    case ".txt":
                        {
                            File.WriteAllText(tempFilePath, TextEditer.Text);
                            break;
                        }
                    case ".docx":
                        {
                            SaveDocFile(tempFilePath);
                            break;
                        }
                    case ".doc":
                        {
                            SaveDocFile(tempFilePath);
                            break;
                        }
                }
            }
            var configText = JsonConvert.SerializeObject(Config);
            File.WriteAllText(configPath, configText);
        }
        private void Form1_MouseWheelMove(object sender, MouseEventArgs e)
        {
            var text = TextEditer.Text;
            int curIndex = TextEditer.SelectionStart;
            try
            {
                if (e.Delta < 0)
                {
                    int nextLineStart = text.IndexOf('\n', curIndex) + 1;
                    if (nextLineStart > 0)
                    {
                        TextEditer.SelectionStart = nextLineStart;
                    }
                }
                else
                {
                    int lastLineStart = text.LastIndexOf('\n', curIndex) - 1;
                    if (lastLineStart > 0)
                    {
                        TextEditer.SelectionStart = lastLineStart;
                    }
                }
                TextEditer.ScrollToCaret();
            }
            catch
            {
                curIndex -= 1;
                TextEditer.SelectionStart = curIndex;
                TextEditer.ScrollToCaret();
            }
        }
        void SaveDocFile(string path)
        {
            MSWord.Application app = new MSWord.Application();
            MSWord.Document doc = app.Documents.Open(path, PasswordDocument: StoredPassword);

            doc.Content.Text = TextEditer.Text;
            app.Visible = false;
            doc.Save();

            if (doc != null)
            {
                doc.Close();
                doc = null;
            }

            if (app != null)
            {
                app.Quit();
                app = null;
            }
        }
    }
    public class Config
    {
        public string? TextPath;
        public Point StartUpLocation;
        public Size WindowSize;
        public string? BackgroundColor;
        public string? FontColor;
        public int Anchor;
    }
}
