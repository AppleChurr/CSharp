using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace sSocketHelper
{
    public class cSocketServerManager
    {
        private TcpListener server;
        private IPAddress localAddr;
        private int port;
        private bool isServerRunning;
        private readonly object sendLock = new object();
        private Queue<string> sendQueue = new Queue<string>();

        public event EventHandler<string> Received;
        public event EventHandler Connected;
        public event EventHandler Disconnected;

        public cSocketServerManager(string ipAddress, int port)
        {
            localAddr = cSocketHelper.ParseIPAddress(ipAddress);
            this.port = port;
        }

        public void StartServer()
        {
            Task.Run(async () =>
            {
                try
                {
                    server = new TcpListener(localAddr, port);
                    server.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    server.Start();
                    isServerRunning = true;

                    while (isServerRunning)
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

        public void StopServer()
        {
            isServerRunning = false;
            server?.Stop();
        }

        private async Task ProcessClientAsync(TcpClient client)
        {

            try
            {
                while (isServerRunning)
                {
                    try
                    {
                        using (NetworkStream stream = client.GetStream())
                        {
                            while (isServerRunning)
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

        private async Task ReceiveMessagesAsync(NetworkStream stream)
        {
            try
            {
                string receivedData = await cSocketHelper.ReceiveDataAsync(stream);

                if (receivedData != null)
                    Received?.Invoke(this, receivedData);

                Thread.Sleep(10);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private async Task SendMessagesAsync(NetworkStream stream)
        {
            try
            {
                string response = "";
                lock (sendLock)
                {
                    if (sendQueue.Count > 0)
                        response = sendQueue.Dequeue();
                }

                if (!string.IsNullOrEmpty(response))
                    await cSocketHelper.SendDataAsync(stream, response);

                Thread.Sleep(10);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void EnqueueResponse(string response)
        {
            lock (sendLock)
            {
                sendQueue.Enqueue(response);
            }
        }
    }
}
