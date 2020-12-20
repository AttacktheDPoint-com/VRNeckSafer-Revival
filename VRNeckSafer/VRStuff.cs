using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using Valve.VR;

namespace VRNeckSafer
{
    class VRStuff
    {
        private CVRSystem system;
        private TrackedDevicePose_t[] Poses;
        private HmdMatrix34_t HmdPose;
        private Vector3 HmdRot;
        private Vector3 lastHmdRot;
         
        private float Angle;
        private float lastAngle;

        public  VRStuff()
        {
            var initError = EVRInitError.None;

            system = OpenVR.Init(ref initError, EVRApplicationType.VRApplication_Utility);

            if (initError != EVRInitError.None)
                return;

            Poses = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
            HmdRot = new Vector3();

        }

        public int getHmdYaw()
        {
            system.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseStanding, 0.0f, Poses);
            HmdPose = Poses[0].mDeviceToAbsoluteTracking;
            
            return (int) (Math.Round(Math.Atan2(HmdPose.m2, HmdPose.m10)* 180.0/Math.PI));
        }

        public void setOffsetAngle(float a)
        {
            Angle = a;

            system.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseStanding, 0.0f, Poses);
            HmdPose = Poses[0].mDeviceToAbsoluteTracking;

            Vector3 oldHmdXyz = new Vector3(HmdPose.m3, 0.0F, HmdPose.m11);
            Vector3 newHmdXyz = new Vector3(HmdPose.m3, 0.0F, HmdPose.m11);

            // Convert oldHmdXyz into un-rotated coordinates.
            oldHmdXyz = rotateCoord(oldHmdXyz, -lastAngle);
            // Set newHmdXyz to have additional rotation from incoming angle change.
            newHmdXyz = rotateCoord(newHmdXyz, -Angle);

            HmdRot = Vector3.Subtract(oldHmdXyz, newHmdXyz);

            if ((lastHmdRot.X != HmdRot.X) || (lastHmdRot.Z != HmdRot.Z))
            {
                HmdMatrix34_t offsetCenter = new HmdMatrix34_t();

                float c = (float)Math.Cos(Angle);
                float s = (float)Math.Sin(Angle);

                offsetCenter.m0 = c;
                offsetCenter.m1 = 0;
                offsetCenter.m2 = s;
                offsetCenter.m3 = 0;
                offsetCenter.m4 = 0;
                offsetCenter.m5 = 1;
                offsetCenter.m6 = 0;
                offsetCenter.m7 = 0;
                offsetCenter.m8 = -s;
                offsetCenter.m9 = 0;
                offsetCenter.m10 = c;
                offsetCenter.m11 = 0;

                OpenVR.ChaperoneSetup.SetWorkingStandingZeroPoseToRawTrackingPose(ref offsetCenter);
                OpenVR.ChaperoneSetup.ShowWorkingSetPreview();

                lastAngle = Angle;
                lastHmdRot.X = HmdRot.X;
                lastHmdRot.Z = HmdRot.Z;
            }
        }
        
        private Vector3 rotateCoord(Vector3 v, float a)
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
