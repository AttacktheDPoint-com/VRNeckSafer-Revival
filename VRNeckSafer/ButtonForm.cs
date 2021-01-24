using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VRNeckSafer
{
    public partial class ButtonForm : Form
    {
        private MainForm mf;
        private ScanForm sf;
        public JoyBut jb;

        public ButtonForm(MainForm f, String titel, ButtonConfig butconf)
        {
            mf = f;
            InitializeComponent();
            StartPosition = FormStartPosition.Manual;
            Top = mf.Top;
            Left = mf.Right - 10;
            Text = titel;
            mf.js.GetJoysticks();

            MainDeviceComboBox.Items.Clear();
            MainDeviceComboBox.Items.Add("none");
            ModifierDeviceComboBox.Items.Clear();
            ModifierDeviceComboBox.Items.Add("none");
            MainButtonComboBox.Items.Clear();
            MainButtonComboBox.Items.Add("none");
            ModifierButtonComboBox.Items.Clear();
            ModifierButtonComboBox.Items.Add("none");

            UseModifierCheckBox.Checked = butconf.UseModifier;

            for (int i = 0; i < mf.js.ll.Count; i++)
            {

                MainDeviceComboBox.Items.Add(mf.js.ll[i].InstanceName);
                ModifierDeviceComboBox.Items.Add(mf.js.ll[i].InstanceName);
                //                if (js.ll[i].InstanceGuid.ToString() == conf.JoystickGUID)
                //                {
                //                    JoystickCB.SelectedIndex = i;
                //                    js.setJoystick(js.ll[i].InstanceGuid);
                //                }
            }
            int Index = mf.js.IndexFromGuid(butconf.JoystickGUID);
            MainDeviceComboBox.SelectedIndex = Index + 1;
            FillButtonComboBox(Index, MainButtonComboBox);
            MainButtonComboBox.Text = butconf.Button;

            Index = mf.js.IndexFromGuid(butconf.ModJoystickGUID);
            ModifierDeviceComboBox.SelectedIndex = Index + 1;
            MainButtonComboBox.Text = butconf.Button;
            FillButtonComboBox(Index, ModifierButtonComboBox);
            ModifierButtonComboBox.Text = butconf.ModButton;
        }

        void FillButtonComboBox(int joyIndex, ComboBox cb)
        {
            cb.Items.Clear();
            cb.Items.Add("none");
            if (joyIndex == -1) return;
            for (int i = 0; i < mf.js.Sticks[joyIndex].Capabilities.ButtonCount; i++)
            {
                cb.Items.Add("But: " + (i + 1));
            }
            for (int i = 0; i < mf.js.Sticks[joyIndex].Capabilities.PovCount; i++)
            {
//                if (conf.Use8WayHat)
//                {
//                    for (int j = 0; j < 360; j += 45)
//                    {
//                        cb.Items.Add("P" + i + ": " + j);
//                    }
//                }
//                else
                {
                    cb.Items.Add("Pov " + i + ": U");
                    cb.Items.Add("Pov " + i + ": R");
                    cb.Items.Add("Pov " + i + ": D");
                    cb.Items.Add("Pov " + i + ": L");
                }
            }

        }

        private void MainScanButton_Click(object sender, EventArgs e)
        {
            sf = new ScanForm(this);
            sf.StartPosition = FormStartPosition.Manual;
            sf.Top = Top+10;
            sf.Left = Left;

            mf.js.InitScan();
            scanTimer.Start();
            sf.ShowDialog();

            MainDeviceComboBox.Text = MainDeviceComboBox.Items[jb.joyIndex + 1].ToString();
            FillButtonComboBox(jb.joyIndex, MainButtonComboBox);
            if (jb.pov == -1)
            {
                MainButtonComboBox.Text = MainButtonComboBox.Items[jb.btn + 1].ToString();
            }
            else
            {
                int butindex =
                    mf.js.Sticks[jb.joyIndex].Capabilities.ButtonCount
                    + jb.pov * 4
                    + jb.btn / 9000
                    + 1;
                MainButtonComboBox.Text = MainButtonComboBox.Items[butindex].ToString();
            }
        }

        private void ModifierScanButton_Click(object sender, EventArgs e)
        {
            sf = new ScanForm(this);
            sf.StartPosition = FormStartPosition.Manual;
            sf.Top = Top + 10;
            sf.Left = Left;

            mf.js.InitScan();
            scanTimer.Start();
            sf.ShowDialog();

            ModifierDeviceComboBox.Text = ModifierDeviceComboBox.Items[jb.joyIndex + 1].ToString();
            FillButtonComboBox(jb.joyIndex, ModifierButtonComboBox);
            if (jb.pov == -1)
            {
                ModifierButtonComboBox.Text = ModifierButtonComboBox.Items[jb.btn + 1].ToString();
            }
            else
            {
                int butindex =
                    mf.js.Sticks[jb.joyIndex].Capabilities.ButtonCount
                    + jb.pov * 4
                    + jb.btn / 9000
                    + 1;
                ModifierButtonComboBox.Text = ModifierButtonComboBox.Items[butindex].ToString();
            }
        }

        private void ScanTimerLoop(object sender, EventArgs e)
        {
            jb = mf.js.ScanJoysticks();
            if (jb.joyIndex != -1)
            {
                scanTimer.Stop();
                sf.Close();
            }
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            MainDeviceComboBox.Text = "none";
            MainButtonComboBox.Text = "none";
            ModifierDeviceComboBox.Text = "none";
            ModifierButtonComboBox.Text = "none";
        }

        private void UseModifierCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            label3.Enabled = UseModifierCheckBox.Checked;
            ModifierDeviceComboBox.Enabled = UseModifierCheckBox.Checked;
            ModifierButtonComboBox.Enabled = UseModifierCheckBox.Checked;
            ModifierScanButton.Enabled= UseModifierCheckBox.Checked;
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            mf.conf.WriteConfig();
            Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            mf.conf = Config.ReadConfig();
            Close();
        }
    }
}
