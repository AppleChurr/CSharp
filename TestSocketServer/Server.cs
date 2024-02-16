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
using sSocketManager;
using System.Text.Json;

namespace SocketTest
{
    public partial class Server : Form
    {
        //TcpListener server = null;

        cSocketServerManager manager;

        public Server()
        {
            InitializeComponent();
            manager = new cSocketServerManager(textBox1.Text, int.Parse(textBox2.Text));
            manager.Connected += Manager_Connected;
            manager.Disconnected += Manager_Disconnected;
            manager.Received += Manager_Received;

            comboBox1.SelectedIndex = 0;
        }

        private void Manager_Received(object sender, string e)
        {
            Console.WriteLine($"[Main] Received Data >> {e}");

            var message = "Hello World";

            string response = "";

            comboBox1.Invoke(new MethodInvoker(delegate
            {
                switch (comboBox1.SelectedIndex)
                {
                    case 1:
                        response = $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {message.Length}\r\n\r\n{message}";
                        break;

                    case 2:
                        var jMessage = JsonSerializer.Serialize(new JsonMessage() { message = message });
                        response = $"HTTP/1.1 200 OK\r\nContent-Type: application/json\r\nContent-Length: {jMessage.Length}\r\n\r\n{jMessage}";
                        break;

                    default:
                        response = message;
                        break;
                }

            }));

            manager.EnqueueMessage(response);
        }

        private void Manager_Disconnected(object sender, EventArgs e)
        {
            Console.WriteLine("[Main] Disconnected Server.");
        }

        private void Manager_Connected(object sender, EventArgs e)
        {
            Console.WriteLine("[Main] Connected Server.");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Console.WriteLine($"Open Server");
            manager.StartServer();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var message = "Hello World";

            string response = "";
            
            comboBox1.Invoke(new MethodInvoker(delegate
            {
                switch (comboBox1.SelectedIndex)
                {
                    case 1:
                        response = $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {message.Length}\r\n\r\n{message}";
                        break;

                    case 2:
                        var jMessage = JsonSerializer.Serialize(new JsonMessage() { message = message });
                        response = $"HTTP/1.1 200 OK\r\nContent-Type: application/json\r\nContent-Length: {jMessage.Length}\r\n\r\n{jMessage}";
                        break;

                    default:
                        response = message;
                        break;
                }
            }));

            manager.EnqueueMessage(response);
        }
    }

    public class JsonMessage
    {
        public string message { get; set; }
    }
}