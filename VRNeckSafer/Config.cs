using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRNeckSafer
{
    public class ButtonConfig
    {
        public string JoystickGUID;
        public string Button;
        public string ModJoystickGUID;
        public string ModButton;
        public bool UseModifier;
        public ButtonConfig()
        {
            JoystickGUID = "none";
            Button = "none";
            ModJoystickGUID = "none";
            ModButton = "none";
            UseModifier = false;
        }
    }
    public class Config
    {
        public ButtonConfig LeftButton;
        public ButtonConfig RightButton;
        public ButtonConfig ResetButton;
        public ButtonConfig HoldButton1;
        public ButtonConfig HoldButton2;
        public ButtonConfig HoldButton3;
        public ButtonConfig HoldButton4;
        public int Angle;
        public int TransLR;
        public int TransF;
        public bool Additiv;
        public bool Auto;
        public int AutorotAngle;
        public int ActivationAngle;
        public int DeactivationAngle;
        public bool Use8WayHat;

        public Config()
        {
            LeftButton = new ButtonConfig();
            RightButton = new ButtonConfig();
            ResetButton = new ButtonConfig();
            HoldButton1 = new ButtonConfig();
            HoldButton2 = new ButtonConfig();
            HoldButton3 = new ButtonConfig();
            HoldButton4 = new ButtonConfig();
            Angle = 30;
            TransLR = 0;
            TransF = 0;
            Additiv = false;
            Auto = false;
            AutorotAngle = 0;
            ActivationAngle = 70;
            DeactivationAngle = 60;
            Use8WayHat = false;
        }

        static public Config ReadConfig()
        {
            try
            {
                Config c= JsonConvert.DeserializeObject<Config>(File.ReadAllText(@".\VRNeckSafer.cfg"));
                if (c.AutorotAngle == -1)
                    c.AutorotAngle = c.Angle;
                return c;
            }
            catch (Exception)
            {
                Config conf = new Config();
                conf.WriteConfig();
                return conf;
            }
        }

        public void WriteConfig()
        {
            File.WriteAllText(@".\VRNeckSafer.cfg", JsonConvert.SerializeObject(this, Formatting.Indented));
        }
    }
}
