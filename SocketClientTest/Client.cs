using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using sSocketHelper;

namespace SocketClientTest
{
    public partial class Client : Form
    {
        private int messageCount = 0;

        public Client()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 버튼 클릭 이벤트 핸들러입니다. 연결 및 메시지 전송을 시작하는 비동기 작업을 시작합니다.
        /// </summary>
        /// <param name="sender">이벤트 발생시킨 객체입니다.</param>
        /// <param name="e">이벤트 데이터를 포함하고 있습니다.</param>
        private void button1_Click(object sender, EventArgs e)
        {
            Task.Run(() => ConnectAndSendRepeatedly());
        }

        /// <summary>
        /// 서버에 반복적으로 연결하고 메시지를 전송하는 메서드입니다.<br></br>
        /// 연결이 끊어지면 예외를 포착하여 5초 후 재연결을 시도합니다.
        /// </summary>
        private async Task ConnectAndSendRepeatedly()
        {
            // 무한 루프를 사용하여 연결 시도를 지속합니다.
            while (!IsDisposed)
            {
                try
                {
                    // 서버의 IP와 포트 번호를 텍스트 박스에서 읽어옵니다.
                    string serverIp = textBox1.Text;
                    int serverPort = cSocketHelper.ParsePort(textBox2.Text);

                    // TcpClient를 사용하여 서버에 연결을 시도합니다.
                    using (TcpClient client = new TcpClient())
                    {
                        await client.ConnectAsync(serverIp, serverPort);
                        cSocketHelper.AppendText(null, "Connected to server.");

                        // 네트워크 스트림을 얻어옵니다.
                        NetworkStream stream = client.GetStream();

                        // 서버에 메시지를 반복해서 보냅니다.
                        while (!IsDisposed)
                        {
                            string message = ComposeMessage();
                            await cSocketHelper.SendDataAsync(stream, message);
                            
                            string response = await cSocketHelper.ReceiveDataAsync(stream);
                            ProcessReceivedData(response);

                            await Task.Delay(3000); // 3초 대기
                        }
                    }
                }
                catch (Exception ex)
                {
                    // 연결 중 오류가 발생할 경우 로그를 출력하고 5초 후에 재시도합니다.
                    cSocketHelper.AppendText(null, $"Error: {ex.Message}. Retrying in 5 seconds...");
                    await Task.Delay(5000);
                }
            }
        }

        private string ComposeMessage()
        {
            return $"Hello, Server! Message number: {++messageCount}";
        }

        private void ProcessReceivedData(string data)
        {
            // 수신 데이터 처리 로직
            Console.WriteLine($"Received: {data}");
        }
    }
}
