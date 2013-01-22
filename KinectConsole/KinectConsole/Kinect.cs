using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading.Tasks;
using Microsoft.Kinect;

namespace KinectConsole
{
    class Kinect
    {
        KinectSensor kinect = null;
        Skeleton[] skeletonData = null;
        JointCollection jointCollection = null;

        public Kinect()
        {
            startKinectSensor();
        }

        private void startKinectSensor()
        {
            kinect = KinectSensor.KinectSensors.FirstOrDefault(s => s.Status == KinectStatus.Connected); // Get first Kinect Sensor
            kinect.SkeletonStream.Enable(); // Enable skeletal tracking

            skeletonData = new Skeleton[kinect.SkeletonStream.FrameSkeletonArrayLength]; // Allocate ST data

            kinect.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(kinect_SkeletonFrameReady); // Get Ready for Skeleton Ready Events

            kinect.Start(); // Start Kinect sensor
        }

        private void kinect_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame()) // Open the Skeleton frame
            {
                if (skeletonFrame != null && this.skeletonData != null) // check that a frame is available
                {
                    skeletonFrame.CopySkeletonDataTo(this.skeletonData); // get the skeletal information in this frame
                }

                drawSkeletons();
            }
        }

        private void drawSkeletons()
        {
            foreach (Skeleton skeleton in this.skeletonData)
            {
                if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                {
                    jointCollection = skeleton.Joints;
                }
            }
        }

        public JointCollection getJointCollection()
        {
            return jointCollection;
        }

        public void printJointCollection(JointCollection jointCollection)
        {
            Console.WriteLine("Elbow right: <" + jointCollection[JointType.ElbowRight].Position.X + ", " + jointCollection[JointType.ElbowRight].Position.Y + ">");
            Console.WriteLine("Elbow left: <" + jointCollection[JointType.ElbowLeft].Position.X + ", " + jointCollection[JointType.ElbowLeft].Position.Y + ">");
            Console.WriteLine("Knee right: <" + jointCollection[JointType.KneeRight].Position.X + ", " + jointCollection[JointType.KneeRight].Position.Y + ">");
            Console.WriteLine("Knee left: <" + jointCollection[JointType.KneeLeft].Position.X + ", " + jointCollection[JointType.KneeLeft].Position.Y + ">");
        }
    }

    class Arduino
    {
        SerialPort port = null;

        public Arduino()
        {
            port = new SerialPort("COM3", 9600);
            port.Open();           
        }

        private void establishContact()
        {
            port.ReadLine();
            port.Write("A");
        }

        public void servoUp()
        {
            port.Write("U");
        }

        public void servoDown()
        {
            port.Write("D");
        }
    }

    class Principal
    {
        static void Main(string[] args)
        {
            Kinect kinect = new Kinect();
            Arduino arduino = new Arduino();

            JointCollection jointCollection = null;
            float[,] baseJoint = new float[4,2];

            baseJoint[0,0] = -10;

            while(true)
            {
                jointCollection = kinect.getJointCollection();

                if (jointCollection != null)
                {
                    if (baseJoint[0,0] == -10)
                        kinect.printJointCollection(jointCollection);
                    else
                    {
                        Console.WriteLine("(Base) Elbow right: <" + baseJoint[0,0] + ", " + baseJoint[0,1] + ">");
                        Console.WriteLine("(Base) Elbow left: <" + baseJoint[1,0] + ", " + baseJoint[1,1] + ">");
                        Console.WriteLine("(Base) Knee right: <" + baseJoint[2,0] + ", " + baseJoint[2,1] + ">");
                        Console.WriteLine("(Base) Knee left: <" + baseJoint[3,0] + ", " + baseJoint[3,1] + ">");
                    }

                    if (Convert.ToChar(Console.Read()) == 'G')
                    {
                        baseJoint[0,0] = jointCollection[JointType.ElbowRight].Position.X;
                        baseJoint[0,1] = jointCollection[JointType.ElbowRight].Position.Y;

                        baseJoint[1,0] = jointCollection[JointType.ElbowLeft].Position.X;
                        baseJoint[1,1] = jointCollection[JointType.ElbowLeft].Position.Y;

                        baseJoint[2,0] = jointCollection[JointType.KneeRight].Position.X;
                        baseJoint[2,1] = jointCollection[JointType.KneeRight].Position.Y;

                        baseJoint[3,0] = jointCollection[JointType.KneeLeft].Position.X;
                        baseJoint[3,1] = jointCollection[JointType.KneeLeft].Position.Y;
                    }
                }
            }
        }


    }
}
