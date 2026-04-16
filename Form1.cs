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

            // Determine folder roles by folder names (if possible)
            string leftName = string.Empty;
            string rightName = string.Empty;
            try
            {
                string leftFull = Path.GetFullPath(txtLeftDir.Text ?? string.Empty).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                string rightFull = Path.GetFullPath(txtRightDir.Text ?? string.Empty).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                leftName = Path.GetFileName(leftFull) ?? string.Empty;
                rightName = Path.GetFileName(rightFull) ?? string.Empty;
            }
            catch
            {
                // ignore
            }

            bool leftIsOld = leftName.Equals("old", StringComparison.OrdinalIgnoreCase) && rightName.Equals("new", StringComparison.OrdinalIgnoreCase);
            bool leftIsNew = leftName.Equals("new", StringComparison.OrdinalIgnoreCase) && rightName.Equals("old", StringComparison.OrdinalIgnoreCase);

            // 2. Compare items present in both lists
            foreach (ListViewItem leftItem in lvwLeftDir.Items)
            {
                string fileName = leftItem.Text;

                // find matching item on right
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
                    // only on left
                    leftItem.ForeColor = Color.Purple;
                    continue;
                }

                // both exist — compare by date (if possible)
                if (!DateTime.TryParse(leftItem.SubItems[2].Text, out DateTime leftDate) || !DateTime.TryParse(rightMatch.SubItems[2].Text, out DateTime rightDate))
                {
                    // If date parsing fails, fallback to leaving both black
                    continue;
                }

                if (leftDate == rightDate)
                {
                    // identical
                    leftItem.ForeColor = Color.Black;
                    rightMatch.ForeColor = Color.Black;
                }
                else
                {
                    // different: if folder names indicate old/new, color new=red old=gray regardless of which date is newer
                    if (leftIsNew)
                    {
                        leftItem.ForeColor = Color.Red;
                        rightMatch.ForeColor = Color.Gray;
                    }
                    else if (leftIsOld)
                    {
                        leftItem.ForeColor = Color.Gray;
                        rightMatch.ForeColor = Color.Red;
                    }
                    else
                    {
                        // fallback: newer = red, older = gray
                        if (leftDate > rightDate)
                        {
                            leftItem.ForeColor = Color.Red;
                            rightMatch.ForeColor = Color.Gray;
                        }
                        else
                        {
                            leftItem.ForeColor = Color.Gray;
                            rightMatch.ForeColor = Color.Red;
                        }
                    }
                }
            }

            // 3. 오른쪽에만 있는 파일 처리
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
                if (!existsInLeft) rightItem.ForeColor = Color.Purple;
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
        private void CopyFileWithSafeCheck(string srcPath, string destPath)
        {
            try
            {
                // 1. 대상 경로에 이미 파일이 있는지 확인
                if (File.Exists(destPath))
                {
                    FileInfo srcFile = new FileInfo(srcPath);
                    FileInfo destFile = new FileInfo(destPath);

                    // [안전 로직] 원본이 대상보다 더 오래된(Old) 경우만 물어봄
                    if (srcFile.LastWriteTime < destFile.LastWriteTime)
                    {
                        string msg = "대상 폴더의 파일이 더 최신입니다. 그래도 덮어쓰시겠습니까?\n\n" +
                                     "원본(Old): " + srcFile.LastWriteTime + "\n" +
                                     "대상(New): " + destFile.LastWriteTime;

                        DialogResult result = MessageBox.Show(msg, "덮어쓰기 경고",
                                              MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                        if (result == DialogResult.No) return; // '아니오' 누르면 복사 취소
                    }
                }

                // 2. 파일 복사 수행 (true는 덮어쓰기 허용을 의미)
                File.Copy(srcPath, destPath, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show("복사 중 오류 발생: " + ex.Message);
            }
        }

        private void btnCopyFromLeft_Click(object sender, EventArgs e)
        {
            if (lvwLeftDir.SelectedItems.Count == 0) return;

            // per-file confirmation is handled inside CopyFileWithSafeCheck (it asks only when source is older than destination)

            foreach (ListViewItem item in lvwLeftDir.SelectedItems)
            {
                // 폴더는 제외하고 파일만 처리 (과제 4단계 전까지)
                if (item.SubItems[1].Text == "<DIR>") continue;

                string srcPath = Path.Combine(txtLeftDir.Text, item.Text);
                string destPath = Path.Combine(txtRightDir.Text, item.Text);

                CopyFileWithSafeCheck(srcPath, destPath);
            }

            // 복사 완료 후 양쪽 리스트 갱신 및 색상 비교 재실행
            PopulateListView(lvwLeftDir, txtLeftDir.Text);
            PopulateListView(lvwRightDir, txtRightDir.Text);
            CompareListViews();
        }

        private void btnCopyFromRight_Click(object sender, EventArgs e)
        {
            if (lvwRightDir.SelectedItems.Count == 0) return;

            foreach (ListViewItem item in lvwRightDir.SelectedItems)
            {
                if (item.SubItems[1].Text == "<DIR>") continue;

                string srcPath = Path.Combine(txtRightDir.Text, item.Text);
                string destPath = Path.Combine(txtLeftDir.Text, item.Text);

                CopyFileWithSafeCheck(srcPath, destPath);
            }

            PopulateListView(lvwLeftDir, txtLeftDir.Text);
            PopulateListView(lvwRightDir, txtRightDir.Text);
            CompareListViews();
        }
    }
}
