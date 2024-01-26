using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace sSocketManager
{
    /// <summary>
    /// TCP 소켓 서버를 관리하는 클래스입니다.
    /// 서버의 시작, 클라이언트 연결 수락, 메시지 수신 및 송신을 처리합니다.
    /// </summary>
    public class cSocketServerManager : cSocketManagerBase
    {
        // 서버의 다양한 이벤트를 위한 이벤트 핸들러.
        public event EventHandler<string> Received; // 메시지 수신 시 발생하는 이벤트.
        public event EventHandler Connected; // 클라이언트 연결 시 발생하는 이벤트.
        public event EventHandler Disconnected; // 클라이언트 연결 해제 시 발생하는 이벤트.


        /// <summary>
        /// 서버 매니저의 생성자입니다.
        /// </summary>
        /// <param name="ipAddress">서버의 IP 주소입니다.</param>
        /// <param name="port">서버의 포트 번호입니다.</param>
        public cSocketServerManager(string ipAddress, int port)
        {
            localAddr = ParseIPAddress(ipAddress);
            this.connectionPort = port;

            eventReceived += CSocketServerManager_eventReceived;
        }

        private void CSocketServerManager_eventReceived(object sender, string e)
        {
            Received?.Invoke(this, e);
        }

        /// <summary>
        /// 서버를 시작하는 메서드입니다.
        /// 비동기적으로 클라이언트 연결을 수락하고 처리합니다.
        /// </summary>
        public void StartServer()
        {
            Task.Run(async () =>
            {
                try
                {
                    server = new TcpListener(localAddr, connectionPort);
                    server.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    server.Start();
                    isRunning = true;

                    while (isRunning)
                    {
                        TcpClient client = await server.AcceptTcpClientAsync();
                        
                        Connected?.Invoke(this, EventArgs.Empty);

                        await ProcessClientAsync(client);
                    }
                }
                catch (Exception ex)
                {
                    Disconnected?.Invoke(this, EventArgs.Empty);
                }
            });
        }


        /// <summary>
        /// 클라이언트와의 통신을 처리하는 메서드입니다.
        /// 클라이언트로부터 메시지를 수신하고 응답을 송신합니다.
        /// </summary>
        /// <param name="client">연결된 TcpClient 객체입니다.</param>
        private async Task ProcessClientAsync(TcpClient client)
        {

            try
            {
                while (isRunning)
                {
                    try
                    {
                        using (NetworkStream stream = client.GetStream())
                        {
                            while (isRunning)
                            {
                                await stream.WriteAsync(new byte[] { byte.MinValue }, 0, 0);

                                await ReceiveMessagesAsync(stream);
                                await SendMessagesAsync(stream);

                                Thread.Sleep(100);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                    finally
                    {
                        client?.Close();
                        client = await server.AcceptTcpClientAsync();
                    }
                }
            }
            catch (Exception e)
            {
                Disconnected?.Invoke(this, EventArgs.Empty);
                Console.WriteLine(e.Message);
            }
            finally
            {
                client?.Close();
            }
        }
    }
}
