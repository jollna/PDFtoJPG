using PdfiumViewer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace PDFtoJPG
{
    public partial class Form1 : Form
    {
        private List<string> filePath = new List<string>();

        public Form1()
        {
            this.StartPosition = FormStartPosition.CenterScreen;
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            List<string> filePaths = GetFiles("请选择需要转换的PDF文件", ".pdf", "PDF文件|*.pdf;*PDF");
            if (filePaths == null)
            {
                txtPDF.Text = "";
                return;
            }
            GetFileLIstToDataGridView(filePaths, false);
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
            if (filePath.Count == 0) return;
            if (filePath.Count > 1)
            {
                progressBar1.Value = 0;
                progressBar1.Minimum = 0;
                progressBar1.Maximum = filePath.Count;
                foreach (var item in filePath)
                {
                    string name = null;
                    if (comboBox1.Text == "")
                    {
                        comboBox1.Text = 1.ToString();
                    }
                    var pdf = PdfDocument.Load(item);
                    var pdfpage = pdf.PageCount;
                    var pagesizes = pdf.PageSizes;
                    for (int i = 1; i <= pdfpage; i++)
                    {
                        Size size = new Size();
                        int PDFWidth = (int)(pagesizes[(i - 1)].Width * 1.5);
                        int PDFHeight = (int)(pagesizes[(i - 1)].Height * 1.5);
                        size.Height = PDFHeight * int.Parse(comboBox1.Text);
                        size.Width = PDFWidth * int.Parse(comboBox1.Text);
                        if (size.Width * size.Height > 250000000)
                        {
                            size = Getsize(PDFWidth, PDFHeight, int.Parse(comboBox1.Text));
                            name = name + i.ToString() + "、";
                        }
                        RenderPage(item, i, size, txtJPG.Text.ToString() + "\\" + Path.GetFileNameWithoutExtension(item) + "_" + i + @".jpg");
                        Application.DoEvents();
                    }
                    progressBar1.Value++;
                }
            }
            else
            {
                string name = null;
                if (int.Parse(StartPage.Text) > int.Parse(EndPage.Text))
                {
                    MessageBox.Show("起始页不能大于结束页码，请重新输入！");
                    return;
                }
                if (comboBox1.Text == "")
                {
                    comboBox1.Text = 1.ToString();
                }
                string strpdfPath = txtPDF.Text.ToString();
                var pdf = PdfDocument.Load(strpdfPath);
                //var pdfpage = pdf.PageCount;
                var pagesizes = pdf.PageSizes;

                //设置进度条信息
                progressBar1.Value = 0;
                progressBar1.Minimum = 0;
                progressBar1.Maximum = int.Parse(EndPage.Text) - int.Parse(StartPage.Text) + 1;

                for (int i = int.Parse(StartPage.Text); i <= int.Parse(EndPage.Text); i++)
                {
                    Size size = new Size();
                    progressBar1.Value++;
                    int PDFWidth = (int)(pagesizes[(i - 1)].Width * 1.5);
                    int PDFHeight = (int)(pagesizes[(i - 1)].Height * 1.5);
                    size.Height = PDFHeight * int.Parse(comboBox1.Text);
                    size.Width = PDFWidth * int.Parse(comboBox1.Text);
                    if (size.Width * size.Height > 250000000)
                    {
                        size = Getsize(PDFWidth, PDFHeight, int.Parse(comboBox1.Text));
                        name = name + i.ToString() + "、";
                    }
                    RenderPage(strpdfPath, i, size, txtJPG.Text.ToString() + "\\" + Path.GetFileNameWithoutExtension(strpdfPath) + "_" + i + @".jpg");
                    Application.DoEvents();
                }
            }
            MessageBox.Show("转换成功！");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Process.Start(txtJPG.Text);
        }

        private void comboBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(e.KeyChar == '\b' || (e.KeyChar >= '0' && e.KeyChar <= '9')))
            {
                e.Handled = true;
            }
            if (e.KeyChar == (char)Keys.Enter)
            {
                button3.Focus(); //当在文本框1中检查到回车键时，直接将焦点转入TextBox2
            }
        }

        private void Form1_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                List<string> filePaths = new List<string>((string[])e.Data.GetData(DataFormats.FileDrop));
                GetFileLIstToDataGridView(filePaths, false);
            }
        }

        private void StartPage_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(e.KeyChar == '\b' || (e.KeyChar >= '0' && e.KeyChar <= '9')))
            {
                e.Handled = true;
            }
            if (e.KeyChar == (char)Keys.Enter)
            {
                EndPage.Focus(); //当在文本框1中检查到回车键时，直接将焦点转入TextBox2
            }
        }

        private void EndPage_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(e.KeyChar == '\b' || (e.KeyChar >= '0' && e.KeyChar <= '9')))
            {
                e.Handled = true;
            }
            if (e.KeyChar == (char)Keys.Enter)
            {
                comboBox1.Focus(); //当在文本框1中检查到回车键时，直接将焦点转入TextBox2
            }
        }

        private void StartPage_Leave(object sender, EventArgs e)
        {
            if (StartPage.Text == "0" || StartPage.Text == "")
            {
                StartPage.Text = "1";
            }
            if (txtPDF.Text != "")
            {
                if (int.Parse(StartPage.Text) > PdfDocument.Load(txtPDF.Text).PageCount)
                {
                    StartPage.Text = PdfDocument.Load(txtPDF.Text).PageCount.ToString();
                }
            }
        }

        private void EndPage_Leave(object sender, EventArgs e)
        {
            if (StartPage.Text == "0" || StartPage.Text == "")
            {
                StartPage.Text = "1";
            }
            if (txtPDF.Text != "")
            {
                if (int.Parse(EndPage.Text) > PdfDocument.Load(txtPDF.Text).PageCount)
                {
                    EndPage.Text = PdfDocument.Load(txtPDF.Text).PageCount.ToString();
                }
            }
        }

        private void dataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && e.RowIndex >= 0)
            {
                ContextMenuStrip contextMenu = new ContextMenuStrip();   // 菜单控件
                contextMenu.Items.Add("删除");
                contextMenu.Items.Add("清空");
                contextMenu.Show(MousePosition.X, MousePosition.Y);
                contextMenu.ItemClicked += new ToolStripItemClickedEventHandler(Add_item);  // 绑定事件
                dataGridView1.ClearSelection();
                dataGridView1.Rows[e.RowIndex].Selected = true;
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
            dlg.Multiselect = true;//等于true表示可以选择多个文件
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

        /// <summary>
        /// 获取最终输出的图片尺寸
        /// </summary>
        /// <param name="Width">PDF宽度数据</param>
        /// <param name="Height">PDF长度数据</param>
        /// <param name="multiple">需要放大的倍率</param>
        /// <returns></returns>
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

        /// <summary>
        /// PDF转JPG
        /// </summary>
        /// <param name="pdfPath">PDF路径</param>
        /// <param name="pageNumber">PDF页码</param>
        /// <param name="size">输出的JPG尺寸</param>
        /// <param name="outputPath">JPG保存路径</param>
        /// <param name="dpi">JPG的DPI</param>
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

        /// <summary>
        /// 判断文件格式是否为PDF
        /// http://www.cnblogs.com/babycool
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns></returns>
        public static bool IsAllowedExtension(string filePath)
        {
            FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            BinaryReader reader = new BinaryReader(stream);
            string fileclass = "";
            try
            {
                for (int i = 0; i < 2; i++)
                {
                    fileclass += reader.ReadByte().ToString();
                }
            }
            catch (Exception)
            {
                throw;
            }
            if (fileclass == "3780")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 右键菜单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Add_item(object sender, ToolStripItemClickedEventArgs e)
        {
            switch (e.ClickedItem.Text)
            {
                case "删除":
                    List<string> deletfilePath = new List<string>();
                    //filePath.Clear();
                    foreach (DataGridViewRow dr in dataGridView1.Rows)
                    {
                        if (true)//判断所得到的哪一行
                        {
                            deletfilePath.Add(dr.Cells[0].Value.ToString()); //得到某一行第一列
                        }
                    }
                    Int32 rowToDelete = dataGridView1.Rows.GetFirstRow(DataGridViewElementStates.Selected);
                    deletfilePath.RemoveAt(rowToDelete);
                    dataGridView1.DataSource = null;
                    dataGridView1.DataSource = deletfilePath.Select(x => new { Value = x }).ToList();
                    GetFileLIstToDataGridView(deletfilePath, true);//更新list数据
                    break;

                case "清空":
                    filePath.Clear();
                    dataGridView1.DataSource = null;
                    GetFileLIstToDataGridView(filePath, false);
                    break;
            }
        }

        /// <summary>
        /// 更新需要转换的list列表，以及DataGridView显示
        /// </summary>
        /// <param name="filePaths">需要重新操作的list集合</param>
        /// <param name="IsRemove">判断是否进行了删除操作</param>
        public void GetFileLIstToDataGridView(List<string> filePaths, bool IsRemove)
        {
            txtPDF.Clear();
            //判断是否有删除操作，如果是进行的删除，那么list列表数据等于DataGridView的数据，否则对现有list6列表数据进行添加并去重
            if (IsRemove)
            {
                filePath = filePaths;
            }
            else
            {
                foreach (string item in filePaths)
                {
                    if (IsAllowedExtension(item))
                    {
                        filePath.Add(item);
                    }
                }
            }
            filePath = RemoveDuplicate(filePath);

            //判断list数据更新后的数量，对界面进行选更新
            if (filePath.Count() > 1)
            {
                if (filePath.Count == 0) return;
                this.Width = 567;
                this.Height = 180;
                this.MaximumSize = new Size(567, 180);
                this.MinimumSize = new Size(567, 180);
                txtPDF.ReadOnly = true;
                DirectoryInfo pathInfo = new DirectoryInfo(filePath[0]);
                txtJPG.Text = pathInfo.Parent.FullName;
                StartPage.Text = "";
                EndPage.Text = "";
                StartPage.ReadOnly = true;
                EndPage.ReadOnly = true;
                dataGridView1.DataSource = filePath.Select(x => new { Value = x }).ToList();
            }
            else if (filePath.Count == 1)
            {
                this.Width = 297;
                this.Height = 180;
                this.MaximumSize = new Size(297, 180);
                this.MinimumSize = new Size(297, 180);
                if (IsAllowedExtension(filePath[0]))
                {
                    txtPDF.ReadOnly = false;
                    txtPDF.Text = filePath[0];
                    DirectoryInfo pathInfo = new DirectoryInfo(filePath[0]);
                    txtJPG.Text = pathInfo.Parent.FullName;
                    StartPage.Text = "1";
                    EndPage.Text = PdfDocument.Load(filePath[0]).PageCount.ToString();
                    StartPage.ReadOnly = false;
                    EndPage.ReadOnly = false;
                }
            }
            else
            {
                this.Width = 297;
                this.Height = 180;
                this.MaximumSize = new Size(297, 180);
                this.MinimumSize = new Size(297, 180);
                txtPDF.ReadOnly = false;
                txtPDF.Text = "";
                txtJPG.Text = "";
                StartPage.Text = "";
                EndPage.Text = "";
                StartPage.ReadOnly = true;
                EndPage.ReadOnly = true;
            }
        }

        /// <summary>
        /// 利用哈希进行list去重
        /// </summary>
        /// <param name="list">需要去重的list</param>
        /// <returns></returns>
        public static List<String> RemoveDuplicate(List<String> list)
        {
            HashSet<String> h = new HashSet<String>(list);
            list.Clear();
            list = new List<string>(h);
            return list;
        }
    }
}