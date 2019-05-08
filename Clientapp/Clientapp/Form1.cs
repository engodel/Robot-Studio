using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Clientapp
{
   
    public partial class Form1 : Form
    {
        const int PORT_NO = 5000;
        string SERVER_IP ;
        public string message;
        public Form1()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            message = textBox1.Text.ToString();
            double messagenum = Convert.ToDouble(message);
            bool isIntString = message.All(char.IsDigit);
                if (isIntString == true) {
                if (messagenum > 0 && messagenum < 360)
                {

                    TcpClient client = new TcpClient(SERVER_IP, PORT_NO);
                    NetworkStream nwStream = client.GetStream();
                    byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(message);

                    //---send the text---
                    Console.WriteLine("Sending : " + message);
                    nwStream.Write(bytesToSend, 0, bytesToSend.Length);

                    //---read back the text---
                    byte[] bytesToRead = new byte[client.ReceiveBufferSize];
                    int bytesRead = nwStream.Read(bytesToRead, 0, client.ReceiveBufferSize);
                    Console.WriteLine("Received : " + Encoding.ASCII.GetString(bytesToRead, 0, bytesRead));
                    client.Close();
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            button1.Enabled = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if(textBox2.Text != null)
            {
                button1.Enabled = true;
                SERVER_IP = textBox2.Text.ToString();
            }

            else
            {
                button1.Enabled = true;
                SERVER_IP = "127.0.0.1";
            }
        }
    }
}
