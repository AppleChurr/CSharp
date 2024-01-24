using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using sSocketHelper;

namespace SocketTest
{
    public partial class Server : Form
    {
        TcpListener server = null;

        public Server()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 서버를 시작하는 이벤트 핸들러입니다. 'StartServer' 메서드를 비동기적으로 실행합니다.
        /// </summary>
        /// <param name="sender">이벤트를 발생시킨 객체입니다.</param>
        /// <param name="e">이벤트 데이터를 담고 있는 EventArgs 객체입니다.</param>
        private void button1_Click(object sender, EventArgs e)
        {
            Task.Run(() => StartServer());
        }

        /// <summary>
        /// 서버를 구성하고 클라이언트의 연결을 비동기적으로 수락하는 메서드입니다. 
        /// 연결된 클라이언트는 'ProcessClientAsync' 메서드로 처리됩니다.
        /// </summary>
        /// <returns>비동기 작업을 나타내는 Task 객체입니다.</returns>
        private async Task StartServer()
        {
            try
            {
                // IP 주소와 포트 번호를 텍스트 박스에서 읽어와 서버를 설정합니다.
                IPAddress localAddr = cSocketHelper.ParseIPAddress(textBox1.Text);
                int port = cSocketHelper.ParsePort(textBox2.Text);

                // TcpListener를 생성하고, 포트 재사용 옵션을 설정합니다.
                server = new TcpListener(localAddr, port);
                server.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                server.Start();

                // 무한 루프를 사용하여 연결 요청을 계속 수락합니다.
                while (true)
                {
                    try
                    {
                        cSocketHelper.AppendText(null, "Server started...");

                        // 클라이언트의 연결을 비동기적으로 기다립니다.
                        TcpClient client = await server.AcceptTcpClientAsync();
                        cSocketHelper.AppendText(null, "Connected!");

                        // 클라이언트 처리를 별도의 비동기 작업으로 수행합니다.
                        await ProcessClientAsync(client);
                    }
                    catch (Exception ex)
                    {
                        // 연결 중 오류가 발생할 경우 로그를 출력합니다.
                        cSocketHelper.AppendText(null, $"Connection error: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                // 서버 시작 중 오류가 발생할 경우 로그를 출력합니다.
                cSocketHelper.AppendText(null, $"Error: {ex.Message}");
            }
            finally
            {
                // 서버를 정지하고 리소스를 정리합니다.
                server?.Stop();
            }
        }

        /// <summary>
        /// 연결된 클라이언트와의 통신을 처리하는 메서드입니다.
        /// 클라이언트로부터 데이터를 수신하고, 처리 후 다시 전송합니다.
        /// </summary>
        /// <param name="client">연결된 TcpClient 객체입니다.</param>
        /// <returns>비동기 작업을 나타내는 Task 객체입니다.</returns>
        private async Task ProcessClientAsync(TcpClient client)
        {
            try
            {
                Byte[] bytes = new Byte[256];
                String data = null;

                // 클라이언트와의 네트워크 스트림을 가져옵니다.
                NetworkStream stream = client.GetStream();

                // 데이터를 받고, 변환하여 다시 보내는 작업을 반복합니다.
                int i;
                while ((i = await stream.ReadAsync(bytes, 0, bytes.Length)) != 0)
                {
                    data = Encoding.ASCII.GetString(bytes, 0, i);
                    cSocketHelper.AppendText(null, $"Received: {data}");

                    data = data.ToUpper();

                    byte[] msg = Encoding.ASCII.GetBytes(data);

                    await stream.WriteAsync(msg, 0, msg.Length);
                    cSocketHelper.AppendText(null, $"Sent: {data}");
                }
            }
            catch (Exception e)
            {
                // 클라이언트 처리 중 오류가 발생할 경우 로그를 출력합니다.
                cSocketHelper.AppendText(null, $"Client process error: {e.Message}");
            }
            finally
            {
                // 클라이언트 연결을 종료합니다.
                client?.Close();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            server?.Stop();

            base.OnFormClosing(e);
        }
    }
}