using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using sSocketHelper;

namespace sSocketHelper
{
    public class cSocketClientManager
    {
        private TcpClient client;
        private string serverIp;
        private int serverPort;
        private bool isClientRunning;
        private readonly object sendLock = new object();
        private Queue<string> sendQueue = new Queue<string>();

        public event EventHandler<string> Received;
        public event EventHandler Connected;
        public event EventHandler Disconnected;

        public cSocketClientManager(string serverIp, int serverPort)
        {
            this.serverIp = serverIp;
            this.serverPort = serverPort;
        }

        public void StartClient()
        {
            Task.Run(async () =>
            {
                try
                {
                    client = new TcpClient();
                    await client.ConnectAsync(serverIp, serverPort);
                    isClientRunning = true;
                    Connected?.Invoke(this, EventArgs.Empty);

                    try
                    {
                        while (isClientRunning)
                        {
                            if (client == null)
                                client = new TcpClient();

                            while (!client.Connected)
                            {
                                try
                                {
                                    await client.ConnectAsync(serverIp, serverPort);
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
                                    while (isClientRunning)
                                    {
                                        await stream.WriteAsync(new byte[] { byte.MinValue }, 0, 0);

                                        await SendMessagesAsync(stream);
                                        await ReceiveMessagesAsync(stream);

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

        public void StopClient()
        {
            isClientRunning = false;
            client?.Close();
        }

        private async Task ReceiveMessagesAsync(NetworkStream stream)
        {
            try
            {
                if (stream.DataAvailable)
                {
                    string receivedData = await cSocketHelper.ReceiveDataAsync(stream);

                    if (receivedData != null)
                        Received?.Invoke(this, receivedData);
                }

                Thread.Sleep(10);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private async Task SendMessagesAsync(NetworkStream stream)
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
                    await cSocketHelper.SendDataAsync(stream, message);

                Thread.Sleep(10);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void EnqueueMessage(string message)
        {
            lock (sendLock)
            {
                sendQueue.Enqueue(message);
            }
        }
    }
}
