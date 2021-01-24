using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using SharpDX.DirectInput;
using Newtonsoft.Json;
using SharpDX;

namespace VRNeckSafer
{

    public partial class MainForm : Form
    {
        public JoystickStuff js;
        public VRStuff vr;
        public Config conf;

        public int but_left;
        public int pov_left;
        public int but_right;
        public int pov_right;
        public int but_reset;
        public int pov_reset;
        public int but_disableauto;
        public int pov_disableauto;

        public int joy_offset_angle;
        public int auto_offset_angle;
        public int sum_offset_angle;
        public int last_offset_angle;

        public float trans_offset_LR;
        public float trans_offset_F;
        public Vector3 trans_offset;

        public int hmdYaw;

        public bool lastpressed;
        public MainForm()
        {

            conf = Config.ReadConfig();

            InitializeComponent();

            js = new JoystickStuff();
            vr = new VRStuff();

            angleNUD.Value = conf.Angle;
            transFNUP.Value = conf.TransF;
            transLRNUP.Value = conf.TransLR;
            additivRB.Checked = conf.Additiv;
            autoCB.Checked = conf.Auto;
            autoRotNUD.Value = conf.AutorotAngle;
            activateNUP.Value = conf.ActivationAngle;
            deactivateNUD.Value = conf.DeactivationAngle;
            if (conf.Auto) enableAuto(true);
            else enableAuto(false);


            setButtonLabels();
 
            loopTimer.Start();
        }

        private void enableAuto(bool enable)
        {
            label4.Enabled = enable;
            label5.Enabled = enable;
            label6.Enabled = enable;
            label7.Enabled = enable;
            label18.Enabled = enable;
            label20.Enabled = enable;
            label13.Enabled = enable;
            autoRotNUD.Enabled = enable;
            activateNUP.Enabled = enable;
            deactivateNUD.Enabled = enable;
            if (!enable) auto_offset_angle = 0;
        }

        private void setButtonLabels()
        {
            LeftButtonLabel.Text = "none";
            RightButtonLabel.Text = "none";

            LeftButtonLabel.Text = conf.LeftButton.Button +" on " + js.NameFromGuid(conf.LeftButton.JoystickGUID);
            if (conf.LeftButton.UseModifier) LeftButtonLabel.Text += " (+ Mod)";
            RightButtonLabel.Text = conf.RightButton.Button + " on " + js.NameFromGuid(conf.RightButton.JoystickGUID);
            if (conf.RightButton.UseModifier) RightButtonLabel.Text += " (+ Mod)";
        }


        private void angleNUD_ValueChanged(object sender, EventArgs e)
        {
            conf.Angle = (int)angleNUD.Value;
            conf.WriteConfig();
        }
        private void angleNUD_KeyUp(object sender, KeyEventArgs e)
        {
            conf.Angle = (int)angleNUD.Value;
            conf.WriteConfig();
        }

        private void activateNUP_ValueChanged(object sender, EventArgs e)
        {
            conf.ActivationAngle = (int)activateNUP.Value;
            conf.WriteConfig();
        }
        private void activateNUP_KeyUp(object sender, KeyEventArgs e)
        {
            conf.ActivationAngle = (int)activateNUP.Value;
            conf.WriteConfig();
        }

        private void deactivateNUD_ValueChanged(object sender, EventArgs e)
        {
            conf.DeactivationAngle = (int)deactivateNUD.Value;
            conf.WriteConfig();
        }

        private void deactivateNUD_KeyUp(object sender, KeyEventArgs e)
        {
            conf.DeactivationAngle = (int)deactivateNUD.Value;
            conf.WriteConfig();
        }

        private void additivRB_CheckedChanged(object sender, EventArgs e)
        {
            conf.Additiv = additivRB.Checked;
            groupAuto.Enabled = !additivRB.Checked;
            HMDYawBox.Enabled = !additivRB.Checked;
            translationBox1.Enabled = !additivRB.Checked;
            conf.WriteConfig();
        }

        private void autoCB_CheckedChanged(object sender, EventArgs e)
        {
            conf.Auto = autoCB.Checked;
            enableAuto(autoCB.Checked);
            conf.WriteConfig();
        }

        private void loopTimer_Tick(object sender, EventArgs e)
        {

            bool l_pressed = js.IsButtonPressed(conf.LeftButton);
            bool r_pressed = js.IsButtonPressed(conf.RightButton);
            bool autofrozen = js.IsButtonPressed(conf.HoldButton1);

            trans_offset = new Vector3(0, 0, 0);

            if (vr.isSeatedMode())
            {
                modeLB.Text = "(playing mode: seated)";
            }
            else
            {
                modeLB.Text = "(playing mode: standing)";
            }

            if (additivRB.Checked)
            {
                if (l_pressed && !lastpressed)
                    joy_offset_angle -= (int)angleNUD.Value;
                if (r_pressed && !lastpressed)
                    joy_offset_angle += (int)angleNUD.Value;
                HMDYawLabel.Text = "HMD yaw: -- deg";
            }
            else
            {
                if (l_pressed)
                {
                    joy_offset_angle = -(int)angleNUD.Value;
                    trans_offset.X = trans_offset_LR;
                    trans_offset.Z = trans_offset_F;
                }
                else if (r_pressed)
                {
                    joy_offset_angle = (int)angleNUD.Value;
                    trans_offset.X = -trans_offset_LR;
                    trans_offset.Z = trans_offset_F;
                }
                else
                {
                    joy_offset_angle = 0;
                    trans_offset.X = 0;
                    trans_offset.Z = 0;
                }
                if (js.IsButtonPressed(conf.ResetButton))
                {
                    vr.getHmdSeatedPositionOffset();
                    vr.getHmdYawOffset();
                }

                int hmdYaw = -(vr.getHmdYaw() + sum_offset_angle);

                while (hmdYaw < -180) hmdYaw += 360;
                while (hmdYaw > 180) hmdYaw -= 360;

                if (vr.HmdIsActive())
                    HMDYawLabel.Text = "HMD yaw: " + hmdYaw + " deg";
                else
                    HMDYawLabel.Text = "HMD yaw: standby";

                if (autoCB.Checked && !autofrozen)
                {
                    if (hmdYaw > 0 && hmdYaw > activateNUP.Value)
                    {
                        auto_offset_angle = (int)autoRotNUD.Value;
                        trans_offset.X = -trans_offset_LR;
                        trans_offset.Z = trans_offset_F;
                    }
                    else if (hmdYaw > 0 && hmdYaw < deactivateNUD.Value)
                    {
                        auto_offset_angle = 0;
                    }
                    else if (hmdYaw < 0 && hmdYaw < -activateNUP.Value)
                    {
                        auto_offset_angle = -(int)autoRotNUD.Value;
                        trans_offset.X = trans_offset_LR;
                        trans_offset.Z = trans_offset_F;
                    }
                    else if (hmdYaw < 0 && hmdYaw > -deactivateNUD.Value)
                    {
                        auto_offset_angle = 0;
                    }
                }
            }

            sum_offset_angle = joy_offset_angle + auto_offset_angle;


            if (last_offset_angle != sum_offset_angle)
            {
                vr.setOffset(sum_offset_angle, trans_offset);
            }

            lastpressed = l_pressed || r_pressed;

            last_offset_angle = sum_offset_angle;

            Text = "VRNeckSafer (" + sum_offset_angle + " deg)";
        }

        private void zeroBT_Click(object sender, EventArgs e)
        {
            vr.getHmdSeatedPositionOffset();
            vr.getHmdYaw();
        }


        private void transFNUP_ValueChanged(object sender, EventArgs e)
        {
            conf.TransF = (int)transFNUP.Value;
            trans_offset_F = (float)transFNUP.Value / 100F;
            conf.WriteConfig();
        }
        private void transFNUP_KeyUp(object sender, KeyEventArgs e)
        {
            conf.TransF = (int)transFNUP.Value;
            trans_offset_F = (float)transFNUP.Value / 100F;
            conf.WriteConfig();
        }

        private void transLRNUP_ValueChanged(object sender, EventArgs e)
        {
            conf.TransLR = (int)transLRNUP.Value;
            trans_offset_LR = (float)transLRNUP.Value / 100F;
            conf.WriteConfig();
        }

        private void transLRNUP_KeyUp(object sender, KeyEventArgs e)
        {
            conf.TransLR = (int)transLRNUP.Value;
            trans_offset_LR = (float)transLRNUP.Value / 100F;
            conf.WriteConfig();
        }

        private void autoRotNUD_ValueChanged(object sender, EventArgs e)
        {
            conf.AutorotAngle = (int)autoRotNUD.Value;
            conf.WriteConfig();
        }

        private void autoRotNUD_KeyUp(object sender, KeyEventArgs e)
        {
            conf.AutorotAngle = (int)autoRotNUD.Value;
            conf.WriteConfig();
        }

        private void SetLeftButton_Click(object sender, EventArgs e)
        {
            ButtonForm frm = new ButtonForm(this, "Button for Left Rotation:", conf.LeftButton);
            frm.ShowDialog();
            setButtonConf(frm, ref conf.LeftButton);
            conf.WriteConfig();
            setButtonLabels();
        }

        private void SetRightButton_Click(object sender, EventArgs e)
        {
            ButtonForm frm = new ButtonForm(this, "Button for Right Rotation:",conf.RightButton);
            frm.ShowDialog();
            setButtonConf(frm, ref conf.RightButton);
            conf.WriteConfig();
            setButtonLabels();
        }

        private void setButtonConf(ButtonForm frm, ref ButtonConfig but)
        {
            if (frm.MainDeviceComboBox.SelectedIndex == 0)
                but.JoystickGUID = "none";
            else
                but.JoystickGUID = js.ll[frm.MainDeviceComboBox.SelectedIndex - 1].InstanceGuid.ToString();
            but.Button = frm.MainButtonComboBox.Text;

            if (frm.ModifierDeviceComboBox.SelectedIndex == 0)
                but.ModJoystickGUID = "none";
            else
                but.ModJoystickGUID = js.ll[frm.ModifierDeviceComboBox.SelectedIndex - 1].InstanceGuid.ToString();
            but.ModButton = frm.ModifierButtonComboBox.Text;

            but.UseModifier = frm.UseModifierCheckBox.Checked;
        }
    }
}
