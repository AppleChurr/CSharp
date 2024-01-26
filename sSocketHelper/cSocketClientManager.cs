using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using sSocketManager;

namespace sSocketManager
{
    /// <summary>
    /// TCP 클라이언트 통신을 관리하는 클래스입니다.
    /// </summary>
    public class cSocketClientManager : cSocketManagerBase
    {
        // 클라이언트의 다양한 이벤트를 위한 이벤트 핸들러.
        public event EventHandler Connected; // 서버 연결 시 발생하는 이벤트.
        public event EventHandler Disconnected; // 연결 해제 시 발생하는 이벤트.

        public cSocketClientManager(string serverIp, int serverPort)
        {
            this.connectionIp = serverIp;
            this.connectionPort = serverPort;

        }

        /// <summary>
        /// 클라이언트를 시작하는 메서드입니다.
        /// 서버에 연결을 시도하고, 연결이 성공하면 메시지 수신 및 송신을 시작합니다.
        /// </summary>
        public void StartClient()
        {
            Task.Run(async () =>
            {
                try
                {
                    client = new TcpClient();
                    await client.ConnectAsync(connectionIp, connectionPort);
                    isRunning = true;
                    Connected?.Invoke(this, EventArgs.Empty);

                    try
                    {
                        while (isRunning)
                        {
                            if (client == null)
                                client = new TcpClient();

                            while (!client.Connected)
                            {
                                try
                                {
                                    await client.ConnectAsync(connectionIp, connectionPort);
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e.Message);
                                    Thread.Sleep(1000);
                                }
                            }

                            using (NetworkStream stream = client.GetStream())
                            {
                                try
                                {
                                    while (isRunning)
                                    {
                                        await stream.WriteAsync(new byte[] { byte.MinValue }, 0, 0);

                                        await ReceiveMessagesAsync(stream);
                                        await SendMessagesAsync(stream);

                                        await Task.Delay(500); // 0.5초 대기
                                    }
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e.Message);
                                }
                                finally
                                {
                                    stream?.Close();
                                    client.Close();

                                    client = null;
                                }
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
                    }
                }
                catch (Exception ex)
                {
                    Disconnected?.Invoke(this, EventArgs.Empty);
                }
                finally
                {
                    client?.Close();
                }
            });
        }
    }
}
