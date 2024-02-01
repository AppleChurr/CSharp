using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace sControls
{
    // ConsoleBoxOption 열거형은 sConsoleBox에서 사용할 수 있는 다양한 옵션을 정의합니다.
    public enum ConsoleBoxOption
    {
        None = 0,           // 옵션이 없음
        ShowNowTime = 1,    // 현재 시간을 출력에 포함
        // Reserved0 = 2,    // 추후 사용을 위해 예약
        // Reserved1 = 4,    // 추후 사용을 위해 예약
        // Reserved2 = 8,    // 추후 사용을 위해 예약
        // Reserved3 = 16,   // 추후 사용을 위해 예약
        // Reserved4 = 32,   // 추후 사용을 위해 예약
        // Reserved5 = 64,   // 추후 사용을 위해 예약
        // Reserved6 = 128,  // 추후 사용을 위해 예약
    }

    // sConsoleBox 클래스는 TextBox를 상속받아 콘솔 출력용으로 사용됩니다.
    public class sConsoleBox : TextBox
    {
        // LineLimit 속성은 TextBox에 표시될 수 있는 최대 줄 수를 정의합니다.
        public int LineLimit { get; set; } = 100;

        // sConsoleBox 생성자는 TextBox의 기본 속성을 설정합니다.
        public sConsoleBox() : base()
        {
            this.ReadOnly = true;  // 읽기 전용으로 설정
            this.Multiline = true; // 다중 줄 입력 허용
            this.ScrollBars = ScrollBars.Both; // 수평 및 수직 스크롤바 활성화
            this.Dock = DockStyle.Fill;
        }

        // WriteLine 메서드는 주어진 문자열을 TextBox에 추가합니다.
        public void WriteLine(string str, ConsoleBoxOption option = ConsoleBoxOption.None)
        {
            try
            {
                // ShowNowTime 옵션이 활성화된 경우, 현재 시간을 문자열에 추가합니다.
                if ((option & ConsoleBoxOption.ShowNowTime) != 0)
                    str = DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss.fff] ") + str;

                // UI 스레드에서 안전하게 컨트롤을 업데이트합니다.
                if (this.IsHandleCreated)
                {
                    if (this.InvokeRequired)
                    {
                        this.Invoke(new MethodInvoker(delegate
                        {
                            AddTextAndRemoveLinesIfNecessary(str);
                        }));
                    }
                    else
                    {
                        AddTextAndRemoveLinesIfNecessary(str);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }

        // AddTextAndRemoveLinesIfNecessary 메서드는 문자열을 TextBox에 추가하고 필요한 경우 줄을 제거합니다.
        private void AddTextAndRemoveLinesIfNecessary(string str)
        {
            this.Text += str + "\r\n";

            // 설정된 LineLimit을 초과하는 경우 가장 오래된 줄을 제거합니다.
            if (this.Text.Split('\n').Length > LineLimit)
                RemoveLine();

            // 텍스트 박스의 가장 아래로 스크롤합니다.
            this.SelectionStart = this.Text.Length;
            this.ScrollToCaret();
        }

        // RemoveLine 메서드는 지정된 줄 번호의 줄을 TextBox에서 제거합니다.
        public void RemoveLine(int lineNum = 0)
        {
            try
            {
                List<string> lstring = new List<string>(this.Lines);
                if (lstring.Count > lineNum)
                {
                    lstring.RemoveAt(lineNum);

                    if (this.InvokeRequired)
                    {
                        this.Invoke(new MethodInvoker(delegate
                        {
                            this.Lines = lstring.ToArray();
                            this.SelectionStart = this.Text.Length;
                            this.ScrollToCaret();
                        }));
                    }
                    else
                    {
                        this.Lines = lstring.ToArray();
                        this.SelectionStart = this.Text.Length;
                        this.ScrollToCaret();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }

        // Clear 메서드는 TextBox의 내용을 모두 지웁니다.
        public new void Clear()
        {
            try
            {
                if (this.IsHandleCreated)
                {
                    if (this.InvokeRequired)
                    {
                        this.Invoke(new MethodInvoker(delegate
                        {
                            this.Text = "";
                        }));
                    }
                    else
                    {
                        this.Text = "";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }
    }
}
