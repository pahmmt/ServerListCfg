using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ServerList
{
    public partial class Form1 : Form
    {
        private bool isEdited = false; // theo dõi có chỉnh sửa chưa

        public Form1()
        {
            // Cho phép dùng các encoding như GB2312
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            InitializeComponent();

            // Drag & Drop toàn bộ form
            this.DragEnter += Form_DragEnter;
            this.DragDrop += Form_DragDrop;

            // Cho phép drag & drop lên RichTextBox
            rtbContent.AllowDrop = true;
            rtbContent.DragEnter += Form_DragEnter;
            rtbContent.DragDrop += Form_DragDrop;

            // Khi chỉnh sửa nội dung -> đánh dấu đã chỉnh sửa
            rtbContent.TextChanged += (s, e) => { isEdited = true; };

            // Nút Browse
            btnBrowse.Click += BtnBrowse_Click;

            // Nút Save
            btnSave.Click += BtnSave_Click;

            // Khi đóng form, nếu có chỉnh sửa sẽ hỏi
            this.FormClosing += Form1_FormClosing;

            // Setup menu chuột phải cho editor
            SetupRtbContextMenu();

            // --- Mặc định load serverlist.ini nếu có ---
            string defaultFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "serverlist.ini");
            if (File.Exists(defaultFile))
            {
                LoadFile(defaultFile);
            }
        }

        #region Drag & Drop

        private void Form_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        private void Form_DragDrop(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0)
            {
                LoadFile(files[0]);
            }
        }

        #endregion

        #region Context Menu cho editor

        private void SetupRtbContextMenu()
        {
            var menu = new ContextMenuStrip();
            menu.Items.Add("Cắt", null, (s, e) => rtbContent.Cut());
            menu.Items.Add("Sao chép", null, (s, e) => rtbContent.Copy());
            menu.Items.Add("Dán", null, (s, e) => rtbContent.Paste());
            menu.Items.Add("Chọn tất cả", null, (s, e) => rtbContent.SelectAll());
            rtbContent.ContextMenuStrip = menu;
        }

        #endregion

        #region Browse & Load/Save

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
                ofd.Filter = "File INI (*.ini)|*.ini|Tất cả các file (*.*)|*.*";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    string basePath = AppDomain.CurrentDomain.BaseDirectory;
                    string relativePath = Path.GetRelativePath(basePath, ofd.FileName);

                    LoadFile(ofd.FileName);

                    txtFilePath.Text = relativePath;
                }
            }
        }

        private void LoadFile(string path)
        {
            try
            {
                byte[] data = File.ReadAllBytes(path);
                Decrypt(ref data);

                string content;

                // Kiểm tra UTF8 BOM
                if (data.Length >= 3 && data[0] == 0xEF && data[1] == 0xBB && data[2] == 0xBF)
                {
                    content = Encoding.UTF8.GetString(data);
                }
                else
                {
                    try
                    {
                        content = Encoding.UTF8.GetString(data);
                        byte[] test = Encoding.UTF8.GetBytes(content);
                        if (!test.SequenceEqual(data))
                            throw new Exception();
                    }
                    catch
                    {
                        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                        Encoding gb2312 = Encoding.GetEncoding("GB2312");
                        content = gb2312.GetString(data);
                    }
                }

                rtbContent.Text = content;
                txtFilePath.Text = path;
                isEdited = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi load file: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            string fullPath = string.IsNullOrEmpty(txtFilePath.Text)
                ? null
                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, txtFilePath.Text);

            // Nếu chưa có file, mở SaveFileDialog
            if (string.IsNullOrEmpty(fullPath) || !File.Exists(fullPath))
            {
                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    sfd.FileName = string.IsNullOrEmpty(txtFilePath.Text) ? "serverlist.ini" : txtFilePath.Text;
                    sfd.Filter = "File INI (*.ini)|*.ini|Tất cả các file (*.*)|*.*";

                    if (sfd.ShowDialog() != DialogResult.OK)
                        return;

                    fullPath = sfd.FileName;
                }
            }
            else
            {
                // Hỏi ghi đè nếu file đã tồn tại
                var res = MessageBox.Show("File đã tồn tại, có muốn ghi đè không?", "Xác nhận lưu", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (res != DialogResult.Yes) return;
            }

            // Validate nội dung INI
            if (!ValidateIni(rtbContent.Text, out string err))
            {
                MessageBox.Show("Không thể lưu file: " + err, "Lỗi INI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                File.SetAttributes(fullPath, FileAttributes.Normal);
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                Encoding utf8 = Encoding.UTF8;
                byte[] data = utf8.GetBytes(rtbContent.Text);
                Encrypt(ref data);
                File.WriteAllBytes(fullPath, data);
                File.SetAttributes(fullPath, FileAttributes.ReadOnly);

                MessageBox.Show("Lưu file thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                isEdited = false;

                txtFilePath.Text = Path.GetRelativePath(AppDomain.CurrentDomain.BaseDirectory, fullPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lưu file: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Form Closing

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (isEdited)
            {
                var result = MessageBox.Show(
                    "Bạn có chắc muốn đóng? Các thay đổi chưa lưu sẽ bị mất.",
                    "Xác nhận đóng",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (result == DialogResult.No)
                    e.Cancel = true;
            }
        }

        #endregion

        #region Encryption / Decryption

        private bool Decrypt(ref byte[] Src)
        {
            if (Src == null) return false;

            string Keys = "Designed by Brianyao 2007 Kingsoft Blaze Game Studio Copyright";
            int KeyIndex = 0;
            long Index = 0, Count = Src.LongLength;
            while (Count > 0)
            {
                Src[Index] = Convert.ToByte(Src[Index] ^ Keys[KeyIndex]);
                KeyIndex = (KeyIndex + 1) & 0x40;
                Index++;
                Count--;
            }
            return true;
        }

        private bool Encrypt(ref byte[] Src)
        {
            return Decrypt(ref Src);
        }

        #endregion

        #region Validate INI

        private bool ValidateIni(string content, out string error)
        {
            error = null;
            var lines = content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();

                if (string.IsNullOrEmpty(line) || line.StartsWith(";"))
                    continue; // comment hoặc blank ok

                if (line.StartsWith("[") && line.EndsWith("]"))
                    continue; // section ok

                if (line.Contains("="))
                    continue; // key=value ok

                error = $"Lỗi ở dòng {i + 1}: {line}";
                return false;
            }

            return true;
        }

        #endregion
    }
}
