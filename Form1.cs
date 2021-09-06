using Microsoft.Win32;
using PdfiumViewer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PDFtoJPG
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            this.StartPosition = FormStartPosition.CenterScreen;
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            txtPDF.Clear();
            List<string> fileList;
            fileList = GetFiles("请选择需要转换的PDF文件", ".pdf", "PDF文件|*.pdf;*PDF");
            if (fileList == null)
            {
                txtPDF.Text = "";
                return;
            }
            foreach (var item in fileList)
            {
                txtPDF.Text += item;
                DirectoryInfo pathInfo = new DirectoryInfo(item);
                txtJPG.Text = pathInfo.Parent.FullName;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folder = new System.Windows.Forms.FolderBrowserDialog();
            if (folder.ShowDialog() == DialogResult.OK)
            {
                //SelectedPath:获取文件夹绝对路径,显示在textbox里面
                this.txtJPG.Text = folder.SelectedPath;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {

            string name = null;
            if (!File.Exists(txtPDF.Text))
            {
                MessageBox.Show("输入的PDF文件不存在，请重新选择");
                return;
            }
            if (!Directory.Exists(txtJPG.Text))
            {
                MessageBox.Show("选择的保存位置不存在，请重新选择");
                return;
            }
            if (comboBox1.Text == "")
            {
                comboBox1.Text = 1.ToString();
            }
            string strpdfPath = txtPDF.Text.ToString();
            var pdf = PdfDocument.Load(strpdfPath);
            var pdfpage = pdf.PageCount;
            var pagesizes = pdf.PageSizes;

            //设置进度条信息
            progressBar1.Value = 0;
            progressBar1.Minimum = 0;
            progressBar1.Maximum = pdfpage;

            for (int i = 1; i <= pdfpage; i++)
            {
                Size size = new Size();
                progressBar1.Value++;
                Thread.Sleep(500);
                int PDFWidth = (int)(pagesizes[(i - 1)].Width * 1.5);
                int PDFHeight = (int)(pagesizes[(i - 1)].Height * 1.5);
                size.Height = PDFHeight * int.Parse(comboBox1.Text);
                size.Width = PDFWidth * int.Parse(comboBox1.Text);
                if (size.Width * size.Height > 250000000)
                {
                    size = Getsize(PDFWidth, PDFHeight, int.Parse(comboBox1.Text));
                    name = name + i.ToString() + "、";
                }
                RenderPage(strpdfPath, i, size, txtJPG.Text.ToString() + "\\" + Path.GetFileNameWithoutExtension(strpdfPath) + i + @".jpg");
            }
            if (name != null)
            {
                MessageBox.Show("转换成功！\n第" + name + "张图分辨率过大，已自动调整为适合输出的最大倍率。");
            }
            else
            {
                MessageBox.Show("转换成功！");
            }
        }

        public void RenderPage(string pdfPath, int pageNumber, System.Drawing.Size size, string outputPath, int dpi = 300)
        {
            using (var document = PdfiumViewer.PdfDocument.Load(pdfPath))
            using (var stream = new FileStream(outputPath, FileMode.Create))
            using (var image = GetPageImage(pageNumber, size, document, dpi))
            {
                image.Save(stream, ImageFormat.Jpeg);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageNumber">pdf文件张数</param>
        /// <param name="size">pdf文件尺寸</param>
        /// <param name="document">pdf文件位置</param>
        /// <param name="dpi"></param>
        /// <returns></returns>
        private static System.Drawing.Image GetPageImage(int pageNumber, Size size, PdfiumViewer.PdfDocument document, int dpi)
        {
            return document.Render(pageNumber - 1, size.Width, size.Height, dpi, dpi, PdfRenderFlags.Annotations);
        }

        private void comboBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(e.KeyChar == '\b' || (e.KeyChar >= '0' && e.KeyChar <= '9')))
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// 调用系统资源管理器，打开文件夹选择文件
        /// </summary>
        /// <param name="Title">文件对话框标题</param>
        /// <param name="DefaultExt">设置默认文件扩展名</param>
        /// <param name="Filter">设置当前文件名筛选器字符串，该字符串决定对话框的“另存为文件类型”或“文件类型”框中出现的选择内容。</param>
        /// <returns></returns>
        public static List<string> GetFiles(string Title, string DefaultExt, string Filter)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = Title;
            dlg.Multiselect = false;//等于true表示可以选择多个文件
            dlg.DefaultExt = DefaultExt;
            dlg.Filter = Filter;
            var fileList = new List<string>();
            if (dlg.ShowDialog() != DialogResult.OK)
            {
                return null;
            }
            foreach (string file in dlg.FileNames)
            {
                fileList.Add(file);
            }
            return fileList;
        }

        public static Size Getsize(int Width, int Height, int multiple)
        {
            Size size = new Size();
            size.Width = Width * multiple;
            size.Height = Height * multiple;
            int i = size.Height * size.Width;
            if (size.Height * size.Width < 250000000)
                return size;
            return Getsize(Width, Height, multiple - 1);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Process.Start(txtJPG.Text);
        }
    }
}
