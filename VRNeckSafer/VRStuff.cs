using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using Valve.VR;
using System.Windows.Forms;
using System.Threading;

namespace VRNeckSafer
{
    public class VRStuff
    {
        private CVRSystem system;
        private TrackedDevicePose_t[] Poses;
        private HmdMatrix34_t HmdPose;
        public Vector3 deltaPos;
        public double deltaRot;
        double HMDYawOffset;


        public VRStuff()
        {
            var initError = EVRInitError.None;

            Poses = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];

            system = OpenVR.Init(ref initError, EVRApplicationType.VRApplication_Overlay);


            if (initError != EVRInitError.None)
            {
                MessageBox.Show("Unable to connect to SteamVR/OpenVR", "Problem", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Application.Exit();
            }

        }

        public bool isSeatedMode()
        {
            return OpenVR.Compositor.GetTrackingSpace() == ETrackingUniverseOrigin.TrackingUniverseSeated;
        }

        public void getHmdSeatedPositionOffset()
        {
            Thread.Sleep(20);

            if (OpenVR.Compositor.GetTrackingSpace() == ETrackingUniverseOrigin.TrackingUniverseSeated)
            {
                system.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseSeated, 0.0f, Poses);
                Vector3 V1 = new Vector3(
                    Poses[0].mDeviceToAbsoluteTracking.m3,
                    Poses[0].mDeviceToAbsoluteTracking.m7,
                    Poses[0].mDeviceToAbsoluteTracking.m11);
                double rot1 = Math.Atan2(Poses[0].mDeviceToAbsoluteTracking.m2, Poses[0].mDeviceToAbsoluteTracking.m10);

                system.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseStanding, 0.0f, Poses);
                Vector3 V2 = new Vector3(
                    Poses[0].mDeviceToAbsoluteTracking.m3,
                    Poses[0].mDeviceToAbsoluteTracking.m7,
                    Poses[0].mDeviceToAbsoluteTracking.m11);
                double rot2 = Math.Atan2(Poses[0].mDeviceToAbsoluteTracking.m2, Poses[0].mDeviceToAbsoluteTracking.m10);

                deltaPos = Vector3.Subtract(V2, V1);
                deltaRot = rot2 - rot1;
            }
            else
            {
                deltaPos = new Vector3(0, 0, 0);
                deltaRot = 0;
            }
        }

        public bool HmdIsActive()
        {
            if (system.GetTrackedDeviceActivityLevel(0) == EDeviceActivityLevel.k_EDeviceActivityLevel_UserInteraction)
                return true;
            else
                return false;
        }

        public void getHmdYawOffset()
        {
            system.GetDeviceToAbsoluteTrackingPose(OpenVR.Compositor.GetTrackingSpace(), 0.0f, Poses);
            HmdPose = Poses[0].mDeviceToAbsoluteTracking;
            HMDYawOffset = (float)Math.Atan2(HmdPose.m2, HmdPose.m10);
        }

        public int getHmdYaw()
        {
            system.GetDeviceToAbsoluteTrackingPose(OpenVR.Compositor.GetTrackingSpace(), 0.0f, Poses);
            HmdPose = Poses[0].mDeviceToAbsoluteTracking;
            double HMDYaw = Math.Atan2(HmdPose.m2, HmdPose.m10);
            return (int)Math.Round((HMDYaw - HMDYawOffset) * 180.0 / Math.PI);
        }

        public void setOffset(int a, Vector3 trans)
        {

            double Angle = a * Math.PI / 180.0 + deltaRot;

            HmdMatrix34_t resetCenter = new HmdMatrix34_t() 
            { m0 = 1, m1 = 0, m2 = 0, m3 = 0, m4 = 0, m5 = 1, m6 = 0, m7 = 0, m8 = 0, m9 = 0, m10 = 1, m11 = 0 };

            setChaperone(resetCenter);

            system.GetDeviceToAbsoluteTrackingPose(OpenVR.Compositor.GetTrackingSpace(), 0.0f, Poses);
            HmdPose = Poses[0].mDeviceToAbsoluteTracking;

            double yaw = Math.Atan2(HmdPose.m2, HmdPose.m10);

            Vector3 oldHmdXyz = new Vector3(HmdPose.m3, HmdPose.m7, HmdPose.m11);
            Vector3 newHmdXyz = new Vector3(HmdPose.m3, HmdPose.m7, HmdPose.m11);

            Vector3 newtrans = rotateCoord(trans, -(HMDYawOffset - deltaRot));

            newHmdXyz = Vector3.Subtract(newHmdXyz, newtrans);

            newHmdXyz = rotateCoord(newHmdXyz, -Angle);

            Vector3 Xyz = Vector3.Subtract(oldHmdXyz, newHmdXyz);

            Xyz = Vector3.Add(Xyz, deltaPos);
 
            float c = (float)Math.Cos(Angle);
            float s = (float)Math.Sin(Angle);

            HmdMatrix34_t rotatedCenter = new HmdMatrix34_t() 
            { m0 = c, m1 = 0, m2 = s, m3 = Xyz.X, m4 = 0, m5 = 1, m6 = 0, m7 = Xyz.Y, m8 = -s, m9 = 0, m10 = c, m11 = Xyz.Z };

            setChaperone(rotatedCenter);

        }

        void setChaperone(HmdMatrix34_t newSetup)
        {

            if (OpenVR.Compositor.GetTrackingSpace() == ETrackingUniverseOrigin.TrackingUniverseSeated)
                OpenVR.ChaperoneSetup.SetWorkingSeatedZeroPoseToRawTrackingPose(ref newSetup);
            else
                OpenVR.ChaperoneSetup.SetWorkingStandingZeroPoseToRawTrackingPose(ref newSetup);

            OpenVR.ChaperoneSetup.ShowWorkingSetPreview();
        }

        public Vector3 rotateCoord(Vector3 v, double a)
        {
            double s = Math.Sin(a);
            double c = Math.Cos(a);
            double X = v.X * c - v.Z * s;
            double Z = v.X * s + v.Z * c;
            v.X = (float)X;
            v.Z = (float)Z;
            return v;
        }

    }
}
