using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;

namespace FileCompare
{
    public partial class Form1 : Form
    {
        private void PopulateListView(ListView lv, string folderPath)
        {
            lv.BeginUpdate(); // 화면 깜빡임 방지
            lv.Items.Clear(); // 기존 목록 초기화

            try
            {
                // 1. 하위 폴더 목록 추가
                var dirs = Directory.EnumerateDirectories(folderPath)
                                    .Select(p => new DirectoryInfo(p))
                                    .OrderBy(d => d.Name);

                foreach (var d in dirs)
                {
                    var item = new ListViewItem(d.Name);
                    item.SubItems.Add("<DIR>"); // 크기 열에 폴더임을 표시
                    item.SubItems.Add(d.LastWriteTime.ToString("g")); // 수정 날짜
                    lv.Items.Add(item);
                }

                // 2. 파일 목록 추가
                var files = Directory.EnumerateFiles(folderPath)
                                     .Select(p => new FileInfo(p))
                                     .OrderBy(f => f.Name);

                foreach (var f in files)
                {
                    var item = new ListViewItem(f.Name);
                    item.SubItems.Add(f.Length.ToString("N0") + " 바이트"); // 콤마 포함 크기
                    item.SubItems.Add(f.LastWriteTime.ToString("g")); // 수정 날짜
                    lv.Items.Add(item);
                }

                // 3. 열 너비 자동 조정
                for (int i = 0; i < lv.Columns.Count; i++)
                {
                    lv.AutoResizeColumn(i, ColumnHeaderAutoResizeStyle.ColumnContent);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("폴더를 읽는 중 오류가 발생했습니다: " + ex.Message);
            }
            finally
            {
                lv.EndUpdate(); // 업데이트 종료
            }
        }

        private void CompareListViews()
        {
            // 1. 색상 초기화 (기본 검은색)
            foreach (ListViewItem item in lvwLeftDir.Items) item.ForeColor = Color.Black;
            foreach (ListViewItem item in lvwRightDir.Items) item.ForeColor = Color.Black;

            // 2. 왼쪽 리스트를 기준으로 오른쪽과 비교
            foreach (ListViewItem leftItem in lvwLeftDir.Items)
            {
                string fileName = leftItem.Text;
                if (!DateTime.TryParse(leftItem.SubItems[2].Text, out DateTime leftDate))
                    continue;

                // 오른쪽에서 같은 이름의 파일 찾기
                ListViewItem? rightMatch = null;
                foreach (ListViewItem rightItem in lvwRightDir.Items)
                {
                    if (rightItem.Text == fileName)
                    {
                        rightMatch = rightItem;
                        break;
                    }
                }

                if (rightMatch == null)
                {
                    leftItem.ForeColor = Color.Purple; // 왼쪽만 있는 파일 (보라색)
                }
                else
                {
                    if (!DateTime.TryParse(rightMatch.SubItems[2].Text, out DateTime rightDate))
                        continue;

                    if (leftDate > rightDate)
                    {
                        leftItem.ForeColor = Color.Red;    // 왼쪽이 최신 (빨간색)
                        rightMatch.ForeColor = Color.Gray; // 오른쪽이 예전 (회색)
                    }
                    else if (leftDate < rightDate)
                    {
                        leftItem.ForeColor = Color.Gray;   // 왼쪽이 예전 (회색)
                        rightMatch.ForeColor = Color.Red;  // 오른쪽이 최신 (빨간색)
                    }
                    // 날짜가 같으면 검은색 유지
                }
            }

            // 3. 오른쪽 리스트에만 있는 단독 파일 처리
            foreach (ListViewItem rightItem in lvwRightDir.Items)
            {
                bool existsInLeft = false;
                foreach (ListViewItem leftItem in lvwLeftDir.Items)
                {
                    if (leftItem.Text == rightItem.Text)
                    {
                        existsInLeft = true;
                        break;
                    }
                }
                if (!existsInLeft) rightItem.ForeColor = Color.Purple; // 보라색
            }
        }

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
                    PopulateListView(lvwLeftDir, dlg.SelectedPath);
                    CompareListViews();

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
                    PopulateListView(lvwRightDir, dlg.SelectedPath);
                    CompareListViews();


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
