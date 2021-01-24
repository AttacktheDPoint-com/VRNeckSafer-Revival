using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.DirectInput;
using System.IO;

namespace VRNeckSafer
{
    public class JoystickButton
    {
        public String JoystickGUID;
        public String Button;
        public String ModJoystickGUID;
        public String ModButton;
    }

    public class JoystickStuff
    {
        DirectInput DInput;
        public IList<DeviceInstance> ll;
        public List<Joystick> Sticks;
        public Joystick Stick;
        public JoyBut jb;

        public List<bool[]> LastButtons;
        public List<int[]> LastPOVs;

        public JoystickStuff()
        {
            DInput = new DirectInput();
            GetJoysticks();
        }

        public string NameFromGuid(string guid)
        {
            string name = "none";
            for (int i = 0; i < ll.Count; i++)
            {
                if (ll[i].InstanceGuid.ToString() == guid)
                {
                    name = ll[i].InstanceName;
                    break;
                }
            }
            return name;
        }
        public int IndexFromGuid(string guid)
        {
            int index=-1;
            for (int i = 0; i < ll.Count; i++)
            {
                if (ll[i].InstanceGuid.ToString() == guid)
                {
                    index = i;
                    break;
                }
            }
            return index;
        }
        public void GetJoysticks()
        {
            ll = DInput.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly);
            Sticks = new List<Joystick>();
            LastButtons = new List<bool[]>();
            LastPOVs = new List<int[]>();

            for (int i = 0; i < ll.Count; i++)
            {
                LastButtons.Add(new bool[128]);
                LastPOVs.Add(new int[4]);
                Sticks.Add(new Joystick(DInput, ll[i].InstanceGuid));
                Sticks[i].Acquire();

            }
        }
        public bool IsButtonPressed(ButtonConfig butconf)
        {
            int j, b=-1, p=-1;
            int mj, mb, mp;

            j = IndexFromGuid(butconf.JoystickGUID);
            if (j == -1) return false;

            if (butconf.Button.StartsWith("But:"))
            {
                int.TryParse(butconf.Button.Substring(5), out b);
            }
            else if ((butconf.Button.StartsWith("Pov ")))
            {
                int.TryParse(butconf.Button.Substring(4, 1), out p);
                switch (butconf.Button.Substring(7))
                {
                    case "U":
                        b = 0;
                        break;
                    case "R":
                        b = 9000;
                        break;
                    case "D":
                        b = 18000;
                        break;
                    case "L":
                        b = 27000;
                        break;
                }
            }
            else if ((butconf.Button.StartsWith("P")))
            {
                int.TryParse(butconf.Button.Substring(1, 1), out p);
                int.TryParse(butconf.Button.Substring(4), out b);
                b *= 100;
            }

            JoystickState State = Sticks[j].GetCurrentState();
            if ((p == -1) && (b == -1)) return false;

            if (p == -1)
            {
                return State.Buttons[b - 1];
            }
            else
            {
                if (State.PointOfViewControllers[p] == -1) return false;
                //                if (use8wayhat)
                //                    return State.PointOfViewControllers[pov] == but;
                return Math.Abs(State.PointOfViewControllers[p] - b) < 5000;
            }
        }
        public JoyBut ScanJoysticks()
        {
            jb = new JoyBut() { joyIndex = -1, btn = -1, pov = -1 };

            bool found = false;
            for (int i = 0; i < Sticks.Count; i++)
            {
                JoystickState State = Sticks[i].GetCurrentState();
                for (int k = 0; k < Sticks[i].Capabilities.ButtonCount; k++)
                {
                    if (State.Buttons[k] != LastButtons[i][k])
                    {
                        jb.joyIndex = i;
                        jb.btn = k;
                        jb.pov = -1;
                        found = true;
                        break;
                    }
                }
                for (int k = 0; k < Sticks[i].Capabilities.PovCount; k++)
                {
                    if (State.PointOfViewControllers[k] != LastPOVs[i][k])
                    {
                        jb.joyIndex = i;
                        jb.btn = State.PointOfViewControllers[k];
                        jb.pov = k;
                        found = true;
                        break;
                    }
                }
                if (found)
                    break;
            }
            return jb;
        }
        public void InitScan()
        {
            for (int i = 0; i < Sticks.Count; i++)
            {
                JoystickState State = Sticks[i].GetCurrentState();
                Array.Copy(State.Buttons, LastButtons[i], 128);
                Array.Copy(State.PointOfViewControllers, LastPOVs[i], 4);
            }
        }
    }

    public class JoyBut
    {
        public int joyIndex;
        public int btn;
        public int pov;

    }
}
