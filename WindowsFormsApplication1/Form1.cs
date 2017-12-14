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
using System.Drawing.Imaging;
using System.Threading;
using static System.GC; // для чистильщика мусора

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //this.HelpButtonClicked += new CancelEventHandler(hb); //в обработчике загрузки формы подключаем обработчик щелчка по HelpButton
        }

        private void ResizePicture(string path_in, string path_out)
        {
            //Size resolution = Screen.PrimaryScreen.Bounds.Size;
            Size resolution = new Size();
            resolution.Height = 1080;
            resolution.Width = 1920;
            Bitmap bmp = (Bitmap)Image.FromFile(path_in);
            Size s = bmp.Size;
            double s_Width = Convert.ToDouble(s.Width);
            double s_Height = Convert.ToDouble(s.Height);
            double p = s_Width / s_Height; // пропорция изображения
            double kh = (s.Height > resolution.Height) ? (s_Height / resolution.Height) : 1; // если в фото больше, чем в разреш 
            s.Height = Convert.ToInt32(s_Height / kh);
            double width = s.Height * p;
            s.Width = Convert.ToInt32(width);
            Bitmap b = new Bitmap(Image.FromFile(path_in), s);
            b.Save(path_out, ImageFormat.Jpeg);
            b.Dispose();
            bmp.Dispose();
            Collect(); // вызов чистильщика мусора
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox4.Text = Convert.ToString(folderBrowserDialog1.SelectedPath);
                string root_path = folderBrowserDialog1.SelectedPath;
                string new_root_path = root_path + "_Full-HD";

                string[] all_files;
                try
                {
                    all_files = Directory.GetFiles(root_path, "*.*", SearchOption.AllDirectories);
                    if (((Directory.GetFiles(root_path, "*.jpg", SearchOption.AllDirectories)).Length > 0) || (Directory.GetFiles(root_path, "*.jpeg", SearchOption.AllDirectories).Length > 0))
                    {
                        label1.Text = "working...";
                        button1.Enabled = false;
                    }
                    else
                    {
                        MessageBox.Show("Folder does not contain JPEG/JPG-files", "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return;
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show("Access denied", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                
                long before = 0;
                long after = 0;

                CheckForIllegalCrossThreadCalls = false;

                Thread yooha = new Thread(delegate ()
                {
                    DirectoryInfo new_folder;
                    FileInfo f_info;
                    for (int i = 0; i < all_files.Length; i++)
                    {

                        string new_path = all_files[i].Replace(root_path, new_root_path);
                        if ((checkBox1.CheckState == CheckState.Checked) || (all_files[i].Contains(".jpg")) || (all_files[i].Contains(".JPG") || (all_files[i].Contains(".jpeg") || (all_files[i].Contains(".JPEG")))))
                        {
                            new_folder = new DirectoryInfo(new_path);
                            if (!new_folder.Parent.Exists)
                            {
                                new_folder.Parent.Create();
                            }
                        }
                        
                        
                        
                        Task task;
                        if (all_files[i].Contains(".jpg") || all_files[i].Contains(".JPG") || all_files[i].Contains(".jpeg") || all_files[i].Contains(".JPEG"))
                        {
                            task = Task.Run(delegate() {
                                ResizePicture(all_files[i], new_path);
                                pictureBox1.Image = Image.FromFile(new_path);

                                f_info = new FileInfo(@all_files[i]);
                                before += (f_info.Length / 1024); // KB
                                f_info = new FileInfo(@new_path);
                                after += (f_info.Length / 1024); // KB
                            });
                            task.Wait();
                        }
                        else
                        {
                            if (checkBox1.CheckState == CheckState.Checked)
                            {
                                task = Task.Run(() => File.Copy(@all_files[i], @new_path, true));
                                task.Wait();
                            }
                        }

                        progressBar1.Value = (i * 100) / (all_files.Length - 1);
                    }
                });
                yooha.Start();
                while (yooha.IsAlive)
                {
                    Application.DoEvents();
                }                

                label1.Text = "finished";
                textBox1.Text = Convert.ToString(before);
                textBox2.Text = Convert.ToString(after);
                textBox3.Text = Convert.ToString(((before - after) * 100) / before);
                before = 0;
                after = 0;
                button1.Enabled = true;
                MessageBox.Show("Completed :)", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }
    }
}