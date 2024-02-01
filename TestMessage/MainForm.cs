using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using sMessage.Queue;
using sControls;

namespace TestMessage
{
    public partial class MainForm : Form
    {
        sQueue<string> queue = new sQueue<string>();

        sConsoleBox console = new sConsoleBox();

        public MainForm()
        {
            InitializeComponent();

            panel1.Controls.Add(console);

            queue.MessageAddedSync += (sender, e) =>
            {
                console.WriteLine("메시지 동기적으로 추가됨 0 : " + e.Message, ConsoleBoxOption.ShowNowTime);
            };

            queue.MessageAddedSync += (sender, e) =>
            {
                console.WriteLine("메시지 동기적으로 추가됨 1 : " + e.Message, ConsoleBoxOption.ShowNowTime);
            };

            queue.MessageAddedAsync += async (sender, e) =>
            {
                await Task.Delay(500); // 비동기 처리 예시
                console.WriteLine("메시지 비동기적으로 추가됨 0 : " + e.Message, ConsoleBoxOption.ShowNowTime);
            };

            queue.MessageAddedAsync += async (sender, e) =>
            {
                await Task.Delay(300); // 비동기 처리 예시
                console.WriteLine("메시지 비동기적으로 추가됨 1 : " + e.Message, ConsoleBoxOption.ShowNowTime);
            };
        }

        private void btnEnqueue_Click(object sender, EventArgs e)
        {
            queue.Message = "테스트 메시지";
            console.WriteLine($"Enqueue Message {queue.Count}/{queue.Size}", ConsoleBoxOption.ShowNowTime);
        }

        private void btnDequeue_Click(object sender, EventArgs e)
        {
            console.WriteLine(queue.Message, ConsoleBoxOption.ShowNowTime);
            console.WriteLine($"Dequeue Message {queue.Count}/{queue.Size}", ConsoleBoxOption.ShowNowTime);
        }

        private void nQueueSize_ValueChanged(object sender, EventArgs e)
        {
            queue.Size = (int)nQueueSize.Value;
        }
    }
}
