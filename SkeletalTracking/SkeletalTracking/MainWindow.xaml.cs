/////////////////////////////////////////////////////////////////////////
//
// Copyright (c) Microsoft Corporation. All rights reserved.
// This code is licensed under the terms of the Microsoft Kinect for
// Windows SDK (Beta) License Agreement:
// http://kinectforwindows.org/KinectSDK-ToU
//
// Modified on 01/29/2013
/////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO.Ports;
using Microsoft.Kinect;

namespace SkeletalTracking
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        KinectSensor kinect;
        Skeleton[] skeletonData = null;
        Skeleton[] baseSkeletonData = null;

        Arduino arduino = new Arduino();

        Canvas mainCanvas = null;
        Canvas baseCanvas = null;
        Button baseButton = null;
        TextBlock msgs = null;

        bool calibrado = false;
        bool calibre = false;

        const double mult = -100;
        const double deslocamento = 50;
        const double raio = 8;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetupKinect();
            SetupUI();
        }

        private void SetupKinect()
        {
            kinect = KinectSensor.KinectSensors.FirstOrDefault(s => s.Status == KinectStatus.Connected); // Get first Kinect Sensor
            kinect.SkeletonStream.Enable(); // Enable skeletal tracking

            skeletonData = new Skeleton[kinect.SkeletonStream.FrameSkeletonArrayLength]; // Allocate ST data
            baseSkeletonData = new Skeleton[kinect.SkeletonStream.FrameSkeletonArrayLength]; // Allocate ST data

            kinect.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(Kinect_SkeletonFrameReady); // Get Ready for Skeleton Ready Events

            kinect.Start(); // Start Kinect sensor
        }

        private void SetupUI()
        {
            mainCanvas = (Canvas)FindName("MainCanvas");
            baseCanvas = (Canvas)FindName("BaseCanvas");

            baseButton = (Button)FindName("BaseButton");
            BaseButton.Click += new RoutedEventHandler(BaseButton_Click);

            msgs = (TextBlock)FindName("Msgs");
        }

        void Kinect_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame()) // Open the Skeleton frame
            {
                if (skeletonFrame != null && this.skeletonData != null) // check that a frame is available
                {
                    skeletonFrame.CopySkeletonDataTo(this.skeletonData); // get the skeletal information in this frame

                    if (calibre)
                    {
                        skeletonFrame.CopySkeletonDataTo(this.baseSkeletonData);
                        calibre = false;
                        calibrado = true;
                    }
                }

                DrawSkeletons(skeletonData, mainCanvas);

                if (calibrado)
                {
                    DrawSkeletons(baseSkeletonData, baseCanvas);
                    CalculateAngles();
                }
            }
        }

        void BaseButton_Click(object sender, RoutedEventArgs e)
        {
            calibre = true;
        }

        private void CalculateAngles()
        {
            foreach (Skeleton baseSkeleton in baseSkeletonData)
            {
                if (baseSkeleton.TrackingState == SkeletonTrackingState.Tracked)
                {
                    // temos o baseSkeleton tracked!
                    foreach (Skeleton skeleton in skeletonData)
                    {
                        if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            // temos ambos os esqueletos!
                            int leftArmAngle = CalculateAngle(skeleton.Joints[JointType.ShoulderLeft].Position, skeleton.Joints[JointType.ElbowLeft].Position, baseSkeleton.Joints[JointType.ShoulderLeft].Position, baseSkeleton.Joints[JointType.ElbowLeft].Position);
                            int rightArmAngle = CalculateAngle(skeleton.Joints[JointType.ShoulderRight].Position, skeleton.Joints[JointType.ElbowRight].Position, baseSkeleton.Joints[JointType.ShoulderRight].Position, baseSkeleton.Joints[JointType.ElbowRight].Position);

                            msgs.Text = "Left: " + leftArmAngle.ToString() + "; Right: " + rightArmAngle.ToString();
                            arduino.SetAngleServo(Arduino.LEFT_SERVO, leftArmAngle);
                            arduino.SetAngleServo(Arduino.RIGHT_SERVO, rightArmAngle);
                        }
                    }
                }
            }
        }

        private void DrawSkeletons(Skeleton[] skeletonData, Canvas canvas)
        {
            canvas.Children.Clear();

            foreach (Skeleton skeleton in skeletonData)
            {
                if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                {
                    DrawTrackedSkeletonJoints(skeleton.Joints, canvas);
                }
            }
        }

        private void DrawTrackedSkeletonJoints(JointCollection jointCollection, Canvas canvas)
        {
            // Render Head and Shoulders
            DrawBone(jointCollection[JointType.Head], jointCollection[JointType.ShoulderCenter], canvas);
            DrawBone(jointCollection[JointType.ShoulderCenter], jointCollection[JointType.ShoulderLeft], canvas);
            DrawBone(jointCollection[JointType.ShoulderCenter], jointCollection[JointType.ShoulderRight], canvas);

            // Render Left Arm
            DrawBone(jointCollection[JointType.ShoulderLeft], jointCollection[JointType.ElbowLeft], canvas);
            DrawBone(jointCollection[JointType.ElbowLeft], jointCollection[JointType.WristLeft], canvas);
            DrawBone(jointCollection[JointType.WristLeft], jointCollection[JointType.HandLeft], canvas);

            // Render Right Arm
            DrawBone(jointCollection[JointType.ShoulderRight], jointCollection[JointType.ElbowRight], canvas);
            DrawBone(jointCollection[JointType.ElbowRight], jointCollection[JointType.WristRight], canvas);
            DrawBone(jointCollection[JointType.WristRight], jointCollection[JointType.HandRight], canvas);

            // Render other bones...
        }

        private void DrawBone(Joint jointFrom, Joint jointTo, Canvas canvas)
        {
            if (jointFrom.TrackingState == JointTrackingState.NotTracked ||
            jointTo.TrackingState == JointTrackingState.NotTracked)
            {
                return; // nothing to draw, one of the joints is not tracked
            }

            if (jointFrom.TrackingState == JointTrackingState.Inferred ||
            jointTo.TrackingState == JointTrackingState.Inferred)
            {
                //DrawNonTrackedBoneLine(jointFrom.Position, jointTo.Position);  // Draw thin lines if either one of the joints is inferred
            }

            if (jointFrom.TrackingState == JointTrackingState.Tracked &&
            jointTo.TrackingState == JointTrackingState.Tracked)
            {
                DrawTrackedBoneLine(jointFrom.Position, jointTo.Position, canvas);  // Draw bold lines if the joints are both tracked
            }
        }

        private void DrawTrackedBoneLine(SkeletonPoint from, SkeletonPoint to, Canvas canvas)
        {
            DrawJoint(from, canvas);
            DrawJoint(to, canvas);

            Line line = new Line();

            line.X1 = FixPointPosition(from.X);
            line.Y1 = FixPointPosition(from.Y);
            line.X2 = FixPointPosition(to.X);
            line.Y2 = FixPointPosition(to.Y);

            line.StrokeThickness = 2;
            line.Stroke = System.Windows.Media.Brushes.Black;
            line.SnapsToDevicePixels = true;
            line.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
            
            canvas.Children.Add(line);
        }

        private void DrawJoint(SkeletonPoint joint, Canvas canvas)
        {
            Ellipse e = new Ellipse();
            e.Width = raio;
            e.Height = raio;
            e.Fill = System.Windows.Media.Brushes.Blue;
            e.Stroke = System.Windows.Media.Brushes.Black;
            e.StrokeThickness = 1;

            e.Margin = new Thickness(FixPointPosition(joint.X) - raio / 2, FixPointPosition(joint.Y) - raio / 2, 0, 0);
            canvas.Children.Add(e);
        }

        private int CalculateAngle(SkeletonPoint origem, SkeletonPoint destino, SkeletonPoint baseOrigem, SkeletonPoint baseDestino)
        {
            Vector skeletonVector = new Vector(destino.X - origem.X, destino.Y - origem.Y);
            Vector baseVector = new Vector(baseDestino.X - baseOrigem.X, baseDestino.Y - baseOrigem.Y);

            skeletonVector.Normalize();
            baseVector.Normalize();

            double angle = Vector.AngleBetween(baseVector, skeletonVector);
            return (int) Math.Round(Math.Abs(angle));
        }

        private double FixPointPosition(double p)
        {
            return p * mult + deslocamento;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            //Cleanup
        }
    }

    class Arduino
    {
        public const char LEFT_SERVO = 'L';
        public const char RIGHT_SERVO = 'R';

        SerialPort port = null;

        public Arduino()
        {
            port = new SerialPort("COM3", 9600);
            port.Open();
        }

        public void SetAngleServo(char motor, int angle)
        {
            String stringAngle = "#" + motor.ToString() + angle.ToString() + "#";
            port.Write(stringAngle);
        }
    }
}