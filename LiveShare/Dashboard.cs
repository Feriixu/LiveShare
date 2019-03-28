using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;

namespace LiveShare
{
    public partial class Dashboard : Form
    {
        private TcpClient client;
        private StreamReader STR;
        private StreamWriter STW;
        private string received;
        private string textToSend;

        public Dashboard()
        {
            InitializeComponent();
            textBoxServerIP.Text = GetLocalIPAddress();
            textBoxClientIP.Text = GetLocalIPAddress();
            textBoxClientPort.Text = "59308";
            textBoxServerPort.Text = "59308";
        }

        private void buttonListen_Click(object sender, EventArgs e)
        {
            try
            {
                TcpListener listener = new TcpListener(IPAddress.Any, int.Parse(textBoxServerPort.Text));
                listener.Start();
                client = listener.AcceptTcpClient();
                STR = new StreamReader(client.GetStream());
                STW = new StreamWriter(client.GetStream())
                {
                    AutoFlush = true
                };
                MessageBox.Show($"Client connected: {client.Client.RemoteEndPoint.ToString()}");
                taskReceiveText.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error accepting Client.\n\n{ex.Message}");
            }
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            client = new TcpClient();
            try
            {
                IPEndPoint IpEnd = new IPEndPoint(IPAddress.Parse(textBoxClientIP.Text), int.Parse(textBoxClientPort.Text));
                client.Connect(IpEnd);
                if (client.Connected)
                {
                    MessageBox.Show($"Client connected to {client.Client.RemoteEndPoint}");
                    STR = new StreamReader(client.GetStream());
                    STW = new StreamWriter(client.GetStream())
                    {
                        AutoFlush = true
                    };
                    taskReceiveText.RunWorkerAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Can't connect:\n\n{0}", ex.Message.ToString()), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void taskReceiveText_DoWork(object sender, DoWorkEventArgs e)
        {
            while (client.Connected)
            {
                try
                {
                    received = STR.ReadLine();
                    //MessageBox.Show($"Received: {received}");

                    textBox.Invoke(new MethodInvoker(delegate ()
                    {
                        textBox.Text = received;
                    }));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Connection probably lost.\n{ex.Message}");
                }
            }
            MessageBox.Show("Connection Lost");
        }

        private void textBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((client != null) ? client.Connected : false)
            {
                taskSendText.RunWorkerAsync();
            }
        }

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        private void textBox_TextChanged(object sender, EventArgs e)
        {
            //if ((client != null) ? client.Connected : false)
            //{
            //    textToSend = textBox.Text;
            //    taskSendText.RunWorkerAsync();
            //}
        }

        private void taskSendText_DoWork(object sender, DoWorkEventArgs e)
        {
            if ((client != null) ? client.Connected : false)
            {
                //MessageBox.Show($"Writing to Stream: {textToSend}");
                textBox.Invoke(new MethodInvoker(delegate ()
                {
                    textToSend = textBox.Text;
                }));
                STW.WriteLine(textToSend);
            }
        }
    }
}
