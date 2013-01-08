using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;

namespace KinectConsole
{
    class Program
    {
        KinectSensor kinect = null;
        Skeleton[] skeletonData = null;

        static void Main(string[] args)
        {
            Program p = new Program();
            p.StartKinectST();

            while (true)
            {
                System.Threading.Thread.Sleep(50000);
            }
        }

        void StartKinectST()
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

                DrawSkeletons();
            }
        }

        private void DrawSkeletons()
        {
            foreach (Skeleton skeleton in this.skeletonData)
            {
                if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                {
                    DrawTrackedSkeletonJoints(skeleton.Joints);
                }
            }
        }

        private void DrawTrackedSkeletonJoints(JointCollection jointCollection)
        {
            Console.WriteLine("Elbow right: <" + jointCollection[JointType.ElbowRight].Position.X + ", " + jointCollection[JointType.ElbowRight].Position.Y + ">");
            Console.WriteLine("Elbow left: <" + jointCollection[JointType.ElbowLeft].Position.X + ", " + jointCollection[JointType.ElbowLeft].Position.Y + ">");
            Console.WriteLine("Knee right: <" + jointCollection[JointType.KneeRight].Position.X + ", " + jointCollection[JointType.KneeRight].Position.Y + ">");
            Console.WriteLine("Knee left: <" + jointCollection[JointType.KneeLeft].Position.X + ", " + jointCollection[JointType.KneeLeft].Position.Y + ">");
        }
    }
}
