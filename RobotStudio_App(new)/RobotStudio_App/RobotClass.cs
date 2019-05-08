using System;
using System.Windows.Forms;

using ABB.Robotics;
using ABB.Robotics.Controllers;
using ABB.Robotics.Controllers.Discovery;
using ABB.Robotics.Controllers.RapidDomain;
using ABB.Robotics.Controllers.IOSystemDomain;
using System.Collections.Generic;
using System.Diagnostics;

namespace RobotStudio_App
{
    class RobotClass
    {
        private Controller controller = null;



        private RapidData rd_start = null;
        private RapidData rd_begin = null;
        private const int SHUT_DOWN = -1;
        private const int TO_WAIT = 0;
        private const int TO_PickUp = 1;
        private Num processFlag;

        public RobotClass(Controller controller)
        {
            this.controller = controller;
            this.controller.Logon(UserInfo.DefaultUser);
            InitDataStream();
        }

        public void InitDataStream()
        {
            Task tRob1 = controller.Rapid.GetTask("T_ROB1");
            if (tRob1 != null)
            {


                rd_start = tRob1.GetRapidData("Module1", "Start");

                if (rd_start.Value is Num)
                {
                    processFlag = (Num)rd_start.Value;
                }

                rd_begin = tRob1.GetRapidData("Module1", "flag");

                if (rd_begin.Value is Num)
                {
                    processFlag = (Num)rd_start.Value;
                }

            }
        }

        public void StartRapidProgram()
        {
            try
            {
                if (controller.OperatingMode == ControllerOperatingMode.Auto)
                {
                    using (Mastership m = Mastership.Request(controller.Rapid))
                    {
                        //Perform operation
                        Debug.WriteLine("Exec status of the controller ::: " + controller.Rapid.ExecutionStatus);
                        Debug.WriteLine("Controller State ::: " + controller.State);
                        controller.Rapid.Start(true);
                    }
                }
                else
                {
                    MessageBox.Show("Automatic mode is required to start execution from a remote client.");
                }
            }
            catch (System.InvalidOperationException ex)
            {
                MessageBox.Show("Mastership is held by another client." + ex.Message);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Unexpected error occurred: " + ex.Message);
            }
        }

        public void StopRapidProgram()
        {
            try
            {
                if (controller.OperatingMode == ControllerOperatingMode.Auto)
                {
                    using (Mastership m = Mastership.Request(controller.Rapid))
                    {
                        controller.Rapid.Stop();
                    }
                }
                else
                {
                    MessageBox.Show("Automatic mode is required to start execution from a remote client.");
                }
            }
            catch (System.InvalidOperationException ex)
            {
                MessageBox.Show("Mastership is held by another client." + ex.Message);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Unexpected error occurred: " + ex.Message);
            }
        }



        public void StopProcess()
        {
            //repeatBool.FillFromString2("FALSE");
            processFlag.FillFromString2(SHUT_DOWN.ToString());
            using (Mastership m = Mastership.Request(controller.Rapid))
            {
                //rd_repeat.Value = repeatBool;
                rd_start.Value = processFlag;
            }
        }

        public void StartProcess()
        {

            using (Mastership m = Mastership.Request(controller.Rapid))
            {
                rd_begin.Value = processFlag;
            }
        }

        public void Dispose()
        {
            if (controller.Rapid.ExecutionStatus == ExecutionStatus.Running)
            {
                StopProcess();
            }
            this.controller.Logoff();
            this.controller.Dispose();
            this.controller = null;
        }

        public Controller Controller
        {
            get
            {
                return controller;
            }
        }

    }
}
