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

        public Kinect()
        {
            startKinectSensor();
        }

        public void startKinectSensor()
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

        public void drawSkeletons()
        {
            foreach (Skeleton skeleton in this.skeletonData)
            {
                if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                {
                    drawTrackedSkeletonJoints(skeleton.Joints);
                }
            }
        }

        private void drawTrackedSkeletonJoints(JointCollection jointCollection)
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

        public void turnLedOn()
        {
            port.Write("O");
        }

        public void turnLedOff()
        {
            port.Write("F");
        }
    }

    class Principal
    {
        static void Main(string[] args)
        {
            Arduino arduino = new Arduino();

            while (true)
            {
                arduino.turnLedOff();
                System.Threading.Thread.Sleep(1000);
                arduino.turnLedOn();
                System.Threading.Thread.Sleep(1000);
            }
        }
    }
}
