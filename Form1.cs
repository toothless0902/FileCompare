namespace FileCompare
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnLeftDir_Click(object sender, EventArgs e)
        {
            using (var dlg = new FolderBrowserDialog())
            {
                dlg.Description = "폴더를선택하세요.";
                // 현재텍스트박스에있는경로를초기선택폴더로설정
                if (!string.IsNullOrWhiteSpace(txtLeftDir.Text) &&
                        Directory.Exists(txtLeftDir.Text))
                {
                    dlg.SelectedPath = txtLeftDir.Text;
                }
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    txtLeftDir.Text = dlg.SelectedPath;
                }
            }
        }

        private void btnRightDir_Click(object sender, EventArgs e)
        {
            using (var dlg = new FolderBrowserDialog())
            {
                dlg.Description = "폴더를선택하세요.";
                // 현재텍스트박스에있는경로를초기선택폴더로설정
                if (!string.IsNullOrWhiteSpace(txtRightDir.Text) &&
                        Directory.Exists(txtRightDir.Text))
                {
                    dlg.SelectedPath = txtRightDir.Text;
                }
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    txtRightDir.Text = dlg.SelectedPath;
                }
            }
        }

        private void btnCopyFromLeft_Click(object sender, EventArgs e)
        {

        }

        private void btnCopyFromRight_Click(object sender, EventArgs e)
        {

        }
    }
}
