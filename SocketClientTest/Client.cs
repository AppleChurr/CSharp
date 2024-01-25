using System;
using System.Windows.Forms;
using sSocketHelper;

namespace SocketClientTest
{
    public partial class Client : Form
    {
        private cSocketClientManager clientManager;

        public Client()
        {
            InitializeComponent();
            clientManager = new cSocketClientManager(textBox1.Text, cSocketHelper.ParsePort(textBox2.Text));
            clientManager.Connected += OnConnected;
            clientManager.Disconnected += OnDisconnected;
            clientManager.Received += OnReceived;
        }

        private void OnConnected(object sender, EventArgs e)
        {
            cSocketHelper.AppendText(null, "Connected to server.");
        }

        private void OnDisconnected(object sender, EventArgs e)
        {
            cSocketHelper.AppendText(null, "Disconnected from server.");
        }

        private void OnReceived(object sender, string data)
        {
            Console.WriteLine($"Received: {data}");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Console.WriteLine($"Received: button1_Click");
            clientManager.StartClient();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            clientManager.EnqueueMessage("Hi~ I'm Client!");
        }

    }
}
