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
            manager.eventReceived += Manager_Received;
        }

        private void Manager_Received(object sender, string e)
        {
            Console.WriteLine($"[Main] Received Data >> {e}");
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
            Console.WriteLine($"Received: button1_Click");
            manager.StartServer();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            manager.EnqueueMessage("Hi! I'm Server!");
        }
    }
}