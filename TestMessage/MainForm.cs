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

namespace TestMessage
{
    public partial class MainForm : Form
    {
        sQueue<string> queue = new sQueue<string>();

        public MainForm()
        {
            InitializeComponent();

            queue.MessageAddedSync += (sender, e) =>
            {
                Console.WriteLine("메시지 동기적으로 추가됨 0 : " + e.Message);
            };

            queue.MessageAddedSync += (sender, e) =>
            {
                Console.WriteLine("메시지 동기적으로 추가됨 1 : " + e.Message);
            };

            queue.MessageAddedAsync += async (sender, e) =>
            {
                await Task.Delay(500); // 비동기 처리 예시
                Console.WriteLine("메시지 비동기적으로 추가됨 0 : " + e.Message);
            };

            queue.MessageAddedAsync += async (sender, e) =>
            {
                await Task.Delay(300); // 비동기 처리 예시
                Console.WriteLine("메시지 비동기적으로 추가됨 1 : " + e.Message);
            };
        }

        private void btnEnqueue_Click(object sender, EventArgs e)
        {
            queue.Message = "테스트 메시지";
            Console.WriteLine("큐 용량 : " + queue.Count + " / " + queue.Size + "\n");
        }

        private void btnDequeue_Click(object sender, EventArgs e)
        {
            Console.WriteLine(queue.Message);
            Console.WriteLine("큐 용량 : " + queue.Count + " / " + queue.Size + "\n");
        }

        private void nQueueSize_ValueChanged(object sender, EventArgs e)
        {
            queue.Size = (int)nQueueSize.Value;
            Console.WriteLine("큐 용량 : " + queue.Count + " / " + queue.Size + "\n");
        }
    }
}
