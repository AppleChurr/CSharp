using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace sSocketManager
{
    public class cSocketManagerBase
    {
        public string connectionIp; // 연결할/오픈할 서버의 IP 주소.
        public int connectionPort; // 연결할/오픈할 서버의 포트 번호.

        public TcpListener server; // 서버를 위한 TcpListener 객체.
        public IPAddress localAddr; // 서버의 IP 주소.
        public TcpClient client; // TCP 클라이언트 객체.
        public bool isRunning; // 실행 상태를 나타내는 플래그.

        // 메시지 송신을 위한 큐와 락 객체.
        public readonly object sendLock = new object();
        public Queue<string> sendQueue = new Queue<string>();

        public event EventHandler<string> eventReceived; // 메시지 수신 시 발생하는 이벤트.


        /// <summary>
        /// 주어진 문자열을 IP 주소로 파싱합니다.
        /// 올바른 IP 주소 형식인 경우 IPAddress 객체를 반환하고, 그렇지 않으면 예외를 발생시킵니다.
        /// </summary>
        /// <param name="ipAddress">파싱할 IP 주소 문자열입니다.</param>
        /// <returns>파싱된 IPAddress 객체입니다.</returns>
        /// <exception cref="FormatException">잘못된 IP 주소 형식일 경우 발생합니다.</exception>
        public IPAddress ParseIPAddress(string ipAddress)
        {
            if (IPAddress.TryParse(ipAddress, out IPAddress address))
            {
                return address;
            }
            else
            {
                throw new FormatException("Invalid IP address format.");
            }
        }

        /// <summary>
        /// 주어진 문자열을 포트 번호로 파싱합니다.
        /// 올바른 포트 형식인 경우 정수로 변환하여 반환하고, 그렇지 않으면 예외를 발생시킵니다.
        /// </summary>
        /// <param name="portString">파싱할 포트 번호 문자열입니다.</param>
        /// <returns>파싱된 포트 번호를 정수로 반환합니다.</returns>
        /// <exception cref="FormatException">잘못된 포트 형식일 경우 발생합니다.</exception>
        public int ParsePort(string portString)
        {
            if (int.TryParse(portString, out int port))
            {
                return port;
            }
            else
            {
                throw new FormatException("Invalid port format.");
            }
        }

        /// <summary>
        /// 텍스트를 UI 컨트롤에 안전하게 추가합니다.
        /// UI 스레드에서 실행되어야 하는 컨트롤의 변경 사항은 Invoke를 사용하여 스레드 간 문제를 해결합니다.
        /// control이 null이 아닌 경우 InvokeRequired 속성을 검사하여 필요시 컨트롤의 스레드에서 실행됩니다.
        /// control이 null인 경우 콘솔에 텍스트를 출력합니다.
        /// </summary>
        /// <param name="control">텍스트를 추가할 Control 객체입니다.</param>
        /// <param name="text">Control에 추가할 텍스트입니다.</param>
        public void AppendText(Control control, string text)
        {
            if (control != null)
            {
                if (control.InvokeRequired)
                {
                    control.Invoke(new Action<string>(s => AppendText(control, s)), text);
                }
                else
                {
                    control.Text += text + Environment.NewLine;
                }
            }
            else
            {
                Console.WriteLine(text);
            }
        }

        /// <summary>
        /// 지정된 네트워크 스트림으로부터 데이터를 비동기적으로 수신합니다.
        /// 스트림에 데이터가 존재하는 경우에만 데이터를 읽습니다.
        /// </summary>
        /// <param name="stream">데이터를 읽을 NetworkStream 객체입니다.</param>
        /// <returns>읽은 데이터의 문자열 표현입니다. 데이터가 없는 경우 null을 반환합니다.</returns>
        public async Task<string> ReceiveDataAsync(NetworkStream stream)
        {
            byte[] buffer = new byte[1024];
            if (stream.DataAvailable)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                return bytesRead > 0 ? Encoding.UTF8.GetString(buffer, 0, bytesRead) : null;
            }
            else
                return null;
        }

        /// <summary>
        /// 지정된 네트워크 스트림에 데이터를 비동기적으로 송신합니다.
        /// </summary>
        /// <param name="stream">데이터를 송신할 NetworkStream 객체입니다.</param>
        /// <param name="data">송신할 데이터의 문자열입니다.</param>
        public async Task SendDataAsync(NetworkStream stream, string data)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(data);
            await stream.WriteAsync(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 서버를 중지하는 메서드입니다.
        /// </summary>
        public void Stop()
        {
            isRunning = false;
            server?.Stop();
            client?.Close();
        }


        /// <summary>
        /// 송신 큐에 메시지를 추가하는 메서드입니다.
        /// </summary>
        /// <param name="message">큐에 추가할 메시지입니다.</param>
        public void EnqueueMessage(string message)
        {
            lock (sendLock)
            {
                sendQueue.Enqueue(message);
            }
        }


        /// <summary>
        /// 네트워크 스트림에 메시지를 송신하는 메서드입니다.
        /// </summary>
        /// <param name="stream">사용할 NetworkStream 객체입니다.</param>
        public async Task SendMessagesAsync(NetworkStream stream)
        {
            try
            {
                string message = null;
                lock (sendLock)
                {
                    if (sendQueue.Count > 0)
                        message = sendQueue.Dequeue();
                }

                if (message != null)
                    await SendDataAsync(stream, message);

                Thread.Sleep(10);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// 네트워크 스트림에서 메시지를 수신하는 메서드입니다.
        /// </summary>
        /// <param name="stream">사용할 NetworkStream 객체입니다.</param>
        public async Task ReceiveMessagesAsync(NetworkStream stream)
        {
            try
            {
                if (stream.DataAvailable)
                {
                    string receivedData = await ReceiveDataAsync(stream);

                    if (receivedData != null)
                        eventReceived?.Invoke(this, receivedData);
                }

                Thread.Sleep(10);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

    }
}
