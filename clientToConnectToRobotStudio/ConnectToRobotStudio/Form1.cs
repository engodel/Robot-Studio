/* This code was developed by Derek English as part of a Final Year Project
 * The Rapid code used is pasted below, feel free to copy, you may have to adjust the names of your
 * targets and workareas.
 */

/*   HOW IT WORKS
 *   
 *   When the form loads, any virtual controller from ABB will load into the combo box.
 *   CONNECT CONTROLLER shold start the imulation in Robot studio, if it doesnt please start manually in robot studio
 *   The below code has a server side TCP/IP code, The server address will display on the top of the form
 *   
 *   A client app will need to be used to send data,
 *   If you send a number if will send it to robot studio and the manipulator will rotate at the correct angle to pick up the lens
 *   
 *   

 */

#region Rapid Code
/*
 ! The Targets were set up by rapid after manually teaching the robot the positions
	CONST robtarget Approach:=[[-16.727,-5.391,110.302],[0,0,1,0],[-1,0,-1,0],[9E+09,9E+09,9E+09,9E+09,9E+09,9E+09]];
	CONST robtarget Lens_10:=[[-16.727,-5.391,20.302],[0,0,1,0],[-1,0,-1,0],[9E+09,9E+09,9E+09,9E+09,9E+09,9E+09]];
	CONST robtarget Home:=[[-5.145284786,-8.163001228,273.975774499],[0.042882028,-0.037026318,0.998115966,-0.023552117],[-1,-1,-1,0],[9E+09,9E+09,9E+09,9E+09,9E+09,9E+09]];
    
    ! Flags to let Rapid know what to do
    const num SHUT_DOWN := -1;
    const num  TO_WAIT := 0;
    const num TO_PickUP := 1;
    
    VAR num flag :=0;
    VAR num Start :=1;
    Pers num pos;
   PERS num position;
    PERS tooldata Servo:=[TRUE,[[0,0,114.2],[1,0,0,0]],[0.215,[8.7,12.3,49.2],[1,0,0,0],0.00021,0.00024,0.00009]];
    TASK PERS wobjdata Workobject_2:=[FALSE,TRUE,"",[[350,0,200],[1,0,0,0]],[[0,0,0],[1,0,0,0]]];
    
        PROC main()
            
            WHILE flag > SHUT_DOWN DO
               IF flag = TO_PickUP THEN PickUpLens;
                ENDIF
            ENDWHILE
        
    ENDPROC
   
    
    PROC PickUpLens()
           
       WaitTime 5;
        MoveL Approach,v1000,fine,Servo\WObj:=Workobject_2;
        MoveJ RelTool(Approach,0,0,0,\Rz:=position),v1000,fine,Servo\WObj:=Workobject_2;
        MoveL Reltool(Lens_10,0,0,0,\Rz:=position),v500,fine,Servo\WObj:=Workobject_2;	          
	    WaitTime 2;
	    SetDO Gripper,1;
	    MoveL Approach,v1000,fine,Servo\WObj:=Workobject_2;
        MoveJ RelTool(Approach,0,0,0,\Rz:=-position),v1000,fine,Servo\WObj:=Workobject_2;
	    MoveL Lens_10,v50,fine,Servo\WObj:=Workobject_2;
	    SetDO Gripper,0;
	    MoveL Home,v1000,fine,Servo\WObj:=Workobject_2;
        flag:=TO_WAIT;
       
	ENDPROC
ENDMODULE
 */

#endregion Rapid Code


using ABB.Robotics.Controllers;
using ABB.Robotics.Controllers.Discovery;
using System;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace ConnectToRobotStudio
{
    public partial class Form1 : Form
    {
        private NetworkScanner scanner = null;
        private RobotClass myRobot = null;
        const int PortNo = 5000;
        string SERVER_IP;

        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            string localIP;
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endpoint = socket.LocalEndPoint as IPEndPoint;
                localIP = endpoint.Address.ToString();
                textBoxServer.Text = " Serevr address is " + localIP;
                SERVER_IP = localIP;
            }
                //This will check for ABB controllers and list them in a combo box
                this.scanner = new NetworkScanner();
            this.scanner.Scan();
            ControllerInfoCollection controllers = scanner.Controllers;
            foreach (ControllerInfo info in controllers)
            {
                comboBox1.Items.Add(info.ControllerName + " / " + info.IPAddress.ToString());
                comboBox1.SelectedIndex = 0;
                
            }

            if(backgroundWorker1.IsBusy != true)
            {
                backgroundWorker1.RunWorkerAsync();
            }
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
                        

                        break;
                    }
                }
                {
                    MessageBox.Show("Selected controller not available.");
                }
            }
            if (myRobot == null) MessageBox.Show("Selected controller not available. (comboBox String != controller info)");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            connectController();
        }
        private void butStart_Click(object sender, EventArgs e)
        {
           
        }

        private void splitter1_SplitterMoved(object sender, SplitterEventArgs e)
        {

        }

        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            bool flag = true;
            IPAddress localAdd = IPAddress.Parse(SERVER_IP);
            TcpListener listner = new TcpListener(localAdd, PortNo);
            listner.Start();
            while (flag == true)
            {
                // incoming client connected
                TcpClient client = listner.AcceptTcpClient();

                //get incoming data
                NetworkStream nwStream = client.GetStream();
                byte[] buffer = new byte[client.ReceiveBufferSize];

                // read incoming stream
                int bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize);

                // convert to a string
                string dataRecieved = Encoding.ASCII.GetString(buffer, 0, bytesRead);
   
                e.Result= dataRecieved ;

                // write back to the client
                nwStream.Write(buffer, 0, bytesRead);
                client.Close();
                listner.Stop();
                flag = false;
            }
            
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            string position = e.Result.ToString();
            textBoxPos.Text = "The position is " + position.ToString() + " degrees";
            myRobot.StartProcess(position);
            
            if(backgroundWorker1.IsBusy!= true)
            {
                backgroundWorker1.RunWorkerAsync();
            }
        }
    }
}
