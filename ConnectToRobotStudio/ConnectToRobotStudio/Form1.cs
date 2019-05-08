/* This code was developed by Derek English as part of a Final Year Project
 * The Rapid code used is pasted below, feel free to copy, you may have to adjust the names of your
 * targets and workareas.
 */

 /*   HOW IT WORKS
  *   
  *   When the form loads, any virtual controller from ABB will load into the combo box.
  *   Before connecting to the controller, please start the simulation
  *   you will notice that the rapid code is caught in a while loop waiting to start
  *   CONNECT CONTROLLER
  *   PRESS START
  *   once you press start, this passes the variable 1 to RAPID which is an indication that
  *   the program should start
  
  */

#region Rapid Code
/*
 *MODULE Module1
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
        MoveJ RelTool(Approach,0,0,0,\Rz:=pos),v1000,fine,Servo\WObj:=Workobject_2;
        MoveL Reltool(Lens_10,0,0,0,\Rz:=pos),v500,fine,Servo\WObj:=Workobject_2;	          
	    WaitTime 2;
	    SetDO Gripper,1;
	    MoveL Approach,v1000,fine,Servo\WObj:=Workobject_2;
        MoveJ RelTool(Approach,0,0,0,\Rz:=-pos),v1000,fine,Servo\WObj:=Workobject_2;
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

namespace ConnectToRobotStudio
{
    public partial class Form1 : Form
    {
        private NetworkScanner scanner = null;
        private RobotClass myRobot = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //This will check for ABB controllers and list them in a combo box
            this.scanner = new NetworkScanner();
            this.scanner.Scan();
            ControllerInfoCollection controllers = scanner.Controllers;
            foreach (ControllerInfo info in controllers)
            {
                comboBox1.Items.Add(info.ControllerName + " / " + info.IPAddress.ToString());
                comboBox1.SelectedIndex = 0;
                
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
            myRobot.StartProcess();
        }
    }
}
