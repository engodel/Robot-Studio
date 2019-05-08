using ABB.Robotics.Controllers;
using ABB.Robotics.Controllers.Discovery;
using System;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace RobotStudio_App
{
    public partial class Form1 : Form
    {
        private NetworkScanner scanner = null;
        private RobotClass myRobot = null;
        const int PORT_NO = 5000;
        const string SERVER_IP = "127.0.0.1";
        string localIP;
        bool start = false;
        string Data = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //this.scanner = new NetworkScanner();
            //this.scanner.Scan();
            //ControllerInfoCollection controllers = scanner.Controllers;
           // foreach (ControllerInfo info in controllers)
          //  {
           //     comboBox1.Items.Add(info.ControllerName + " / " + info.IPAddress.ToString());
           //     comboBox1.SelectedIndex = 0;
           // }
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                localIP = endPoint.Address.ToString();
            }
            textBox1.Text = "Server Address is " + localIP;
           
            


        }

        private void connectController()
        {
            ControllerInfoCollection controllers = scanner.Controllers;
            foreach (ControllerInfo info in controllers)
            {
                if (comboBox1.Text.Equals(info.ControllerName + " / " + info.IPAddress.ToString()))
                {
                    if (info.Availability == Availability.Available)
                    {
                        if (myRobot != null)
                        {
                            myRobot.Dispose(); // = LogOff
                            myRobot = null;
                        }
                        myRobot = new RobotClass(ControllerFactory.CreateFrom(info));
                        myRobot.StartRapidProgram();
                        //myRobot.Controller.ConnectionChanged += new EventHandler<ConnectionChangedEventArgs>(ConnectionChanged);

                        break;
                    }
                }
                {
                    MessageBox.Show("Selected controller not available.");
                }
            }
            if (myRobot == null) MessageBox.Show("Selected controller not available. (comboBox String != controller info)");
        }

        private void connect_Click(object sender, EventArgs e)
        {
            connectController();

        }

        public void ShowResponse(string Data)
        {
            textBox2.Text = Data;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (myRobot != null)
            {
                myRobot.Dispose(); // = LogOff
                myRobot = null;
               // myRobot.Controller.ConnectionChanged += new EventHandler<ConnectionChangedEventArgs>(ConnectionChanged);

            }
        }

        private void shutDown_click(object sender, EventArgs e)
        {
            if (myRobot != null)
            {
                myRobot.StopProcess();
               // myRobot.Controller.ConnectionChanged += new EventHandler<ConnectionChangedEventArgs>(ConnectionChanged);

            }
            //myRobot.Dispose();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (myRobot != null)
            {
                myRobot.StartProcess();
            }
        }

        private string recieveResponse()
        {
            string dataReceived = null;
            start = true;
            Console.WriteLine("Server Address is " + localIP);
            IPAddress localAdd = IPAddress.Parse(SERVER_IP);
            TcpListener listener = new TcpListener(localAdd, PORT_NO);
            Console.WriteLine("Listening...");
            listener.Start();
            while (start ==true)
            {



                //---incoming client connected---
                TcpClient client = listener.AcceptTcpClient();

                //---get the incoming data through a network stream---
                NetworkStream nwStream = client.GetStream();
                byte[] buffer = new byte[client.ReceiveBufferSize];

                //---read incoming stream---
                int bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize);
                

                //---convert the data received into a string---
                 dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);
              

                //---write back the text to the client---
                Console.WriteLine("Sending back : " + dataReceived);
                nwStream.Write(buffer, 0, bytesRead);
                client.Close();
                listener.Stop();
                
                start = false;

            }

            return dataReceived;


            }

        private void button2_Click(object sender, EventArgs e)
        {
            if (bgwServer.IsBusy == false)
            {
                bgwServer.RunWorkerAsync();

            }

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void bgwServer_DoWork(object sender, DoWorkEventArgs e)
        {
            string request = recieveResponse();
            e.Result = request;
        }

        private void bgwServer_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
                    Data = e.Result.ToString();
                    ShowResponse(Data);
            if (bgwServer.IsBusy == false)
            {
                bgwServer.RunWorkerAsync();

            }

        }
    }
}
