using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ModbusSampleBR;
using ModbusTCPBR;

namespace ModbusSampleBR
{
    public partial class frmMain : Form
    {
        private ModbusTCPBR.MasterBR BRmaster;
        
        public frmMain()
        {
            InitializeComponent();
        }

        #region start communication

        private void frmMain_Load(object sender, EventArgs e)
        {
            btnConnect.Image = imageList1.Images[1];
        }

        // ------------------------------------------------------------------------
        // Open or close connection to bus coupler
        private void btnConnect_Click(object sender, EventArgs e)
        {
            // ------------------------------------------------------------------------
            // Close connection to bus coupler
            if (btnConnect.Text == "Connected")
            {
                DoDisconnect();
            }
            // ------------------------------------------------------------------------
            // Open connection to bus coupler
            else
            {
                try
                {
                    btnConnect.Text = "Connecting...";
                    btnConnect.Refresh();

                    // Create new master instance, add events and reset watchdog
                    BRmaster = new ModbusTCPBR.MasterBR();
                    BRmaster.OnBCexception += new ModbusTCPBR.MasterBR.BCexception(BRmaster_OnBCexception);
                    BRmaster.PollRefresh = 200;
                    BRmaster.Connect(txtIP.Text, 502);

                    BRmaster.BCinfo.watchdog_reset();
                    BRmaster.BCinfo.watchdog_threshold = 6000;

                    ReadControllerInfo(true);

                    // Set up buttons and tabs
                    btnConnect.Image = imageList1.Images[0];
                    btnConnect.Text = "Connected";
                    btnBusCoupler.Enabled = true;
                    btnModules.Enabled = true;
                    btnModules_Click(null, null);

                    // Read module information and fill list box with data
                    lstModules.Items.Clear();
                    for (int x = 0; x <= BRmaster.MDinfo.GetUpperBound(0); x++)
                    {
                        if (BRmaster.MDinfo[x].name == "unknown") lstModules.Items.Add("Module" + x.ToString());
                        else lstModules.Items.Add(BRmaster.MDinfo[x].name);
                    }
                    if (lstModules.Items.Count >=0) lstModules.SelectedIndex = 0;
                }
                catch (SystemException error)
                {
                    DoDisconnect();
                    // Catch message
                    if (error.GetType().ToString() == "System.Net.Sockets.SocketException")
                    {
                        MessageBox.Show("Check IP address or host name!", "Can't find X20BC0087 controller", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    MessageBox.Show(error.Message);
                }
            }
        }

        // ------------------------------------------------------------------------
        // Show bus coupler data
        private void btnBusCoupler_Click(object sender, EventArgs e)
        {
            btnBusCoupler.BackColor = Color.OliveDrab;
            grpBusCoupler.Visible = true;
            btnModules.BackColor = Color.Gainsboro;
            grpModules.Visible = false;
            RefreshTimer.Enabled = true;
        }

        // ------------------------------------------------------------------------
        // Refresh bus coupler data
        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            ReadControllerInfo(false);
        }

        // ------------------------------------------------------------------------
        // Show module data
        private void btnModules_Click(object sender, EventArgs e)
        {
            btnBusCoupler.BackColor = Color.Gainsboro;
            grpBusCoupler.Visible = false;
            btnModules.BackColor = Color.OliveDrab;
            grpModules.Visible = true;
            RefreshTimer.Enabled = false;
        }

        // ------------------------------------------------------------------------
        // X20BC0087 controller exception
        private void BRmaster_OnBCexception(ushort reason)
        {
            // ------------------------------------------------------------------
            // Seperate calling threads
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new ModbusTCPBR.MasterBR.BCexception(BRmaster_OnBCexception), new object[] { reason });
                return;
            }

            switch (reason)
            {
                //-----------------------------------------------------------------
                case MasterBR.excWatchdog:
                    DoDisconnect();
                    MessageBox.Show("Watchdog exception!", "ModbusTCP error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                //-----------------------------------------------------------------
                case MasterBR.excTimeout:
                    DoDisconnect();
                    MessageBox.Show("Connection timeout!", "ModbusTCP error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                //-----------------------------------------------------------------
                case MasterBR.excConnection:
                    DoDisconnect();
                    MessageBox.Show("Connection is lost!", "ModbusTCP error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                //-----------------------------------------------------------------
                case MasterBR.excNoModule:
                    MessageBox.Show("Wrong module number!", "ModbusTCP error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                //-----------------------------------------------------------------
                case MasterBR.excNoDigInData:
                    MessageBox.Show("This module has no digital input data!", "ModbusTCP error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                //-----------------------------------------------------------------
                case MasterBR.excNoDigOutData:
                    MessageBox.Show("This module has no digital output data!", "ModbusTCP error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                //-----------------------------------------------------------------
                case MasterBR.excNoAnaInData:
                    MessageBox.Show("This module has no analog input data!", "ModbusTCP error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                //-----------------------------------------------------------------
                case MasterBR.excNoAnaOutData:
                    MessageBox.Show("This module has no analog output data!", "ModbusTCP error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                //-----------------------------------------------------------------
                case MasterBR.excWrongRegData:
                    MessageBox.Show("Register data is wrong. Check module and register nr!", "ModbusTCP error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                //-----------------------------------------------------------------
                case MasterBR.excDataSize:
                    MessageBox.Show("Data size is not correct!", "ModbusTCP error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                //-----------------------------------------------------------------
                case MasterBR.excDataRange:
                    MessageBox.Show("Maximum value is 255", "ModbusTCP error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                //-----------------------------------------------------------------
                case MasterBR.excWrongEthernetFormat:
                    MessageBox.Show("The ethernet data you try to change is incorrect", "ModbusTCP error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                //-----------------------------------------------------------------
                case MasterBR.excUnhandled:
                    MessageBox.Show("This is an unhandled exception. Please contact BuR support!", "ModbusTCP error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }
        }

		// ------------------------------------------------------------------------
		// Quitt application
        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (BRmaster != null) BRmaster = null;
            Application.Exit();
        }

        // ------------------------------------------------------------------------
        // Disconnect from bus coupler
        private void DoDisconnect()
        {
            if (BRmaster != null)
            {
                BRmaster.Disconnect();
                BRmaster = null;
            }
            btnConnect.Text = "Connect";
            btnConnect.Image = imageList1.Images[1];
            btnBusCoupler.BackColor = Color.Gainsboro;
            btnBusCoupler.Enabled = false;
            btnModules.Enabled = false;
            btnModules.BackColor = Color.Gainsboro;
            grpModules.Visible = false;
            grpBusCoupler.Visible = false;
            RefreshTimer.Enabled = false;
        }

        #endregion

        #region module data exchange synchronous

        // --------------------------------------------------------------------
        // Request module info data when lisbox changes
        private void lstModules_SelectedIndexChanged(object sender, EventArgs e)
        {
            labModStatus.Text = BRmaster.MDinfo[lstModules.SelectedIndex].status.ToString();
            labModID.Text = BRmaster.MDinfo[lstModules.SelectedIndex].id.ToString();
            labModSerial.Text = BRmaster.MDinfo[lstModules.SelectedIndex].serial.ToString();
            labModHWid.Text = BRmaster.MDinfo[lstModules.SelectedIndex].cfg_hw.ToString();
            labModHWver.Text = BRmaster.MDinfo[lstModules.SelectedIndex].cfg_variant.ToString();
            labModModell.Text = BRmaster.MDinfo[lstModules.SelectedIndex].cfg_function_modell.ToString();
            labModFirmware.Text = BRmaster.MDinfo[lstModules.SelectedIndex].cfg_firmware.ToString();
        }

        // --------------------------------------------------------------------
        // Make sure only numbers are entered
        private void ValidateNumber(object sender, KeyPressEventArgs e)
        {
            if (char.IsNumber(e.KeyChar) != true)
                e.Handled = true;
        }

        // --------------------------------------------------------------------
        // Change state of digital in and out buttons
        private void btnDIGdat_Click(object sender, EventArgs e)
        {
            Button ButtonClicked;

            ButtonClicked = (Button)sender;
            if (ButtonClicked.Text == "false")
            {
                ButtonClicked.BackColor = Color.Green;
                ButtonClicked.Text = "true";
            }
            else
            {
                ButtonClicked.BackColor = Color.Red;
                ButtonClicked.Text = "false";
            }
        }

 		// --------------------------------------------------------------------
		// Button read digital input
        private void btnDIGread_Click(object sender, EventArgs e)
        {
            btnDIGdat1.Text = btnDIGdat2.Text = btnDIGdat3.Text = btnDIGdat4.Text = btnDIGdat5.Text = btnDIGdat6.Text = btnDIGdat7.Text = btnDIGdat8.Text = btnDIGdat9.Text = btnDIGdat10.Text = btnDIGdat11.Text = btnDIGdat12.Text = "-";
            btnDIGdat1.BackColor = btnDIGdat2.BackColor = btnDIGdat3.BackColor = btnDIGdat4.BackColor = btnDIGdat5.BackColor = btnDIGdat6.BackColor = btnDIGdat7.BackColor = btnDIGdat8.BackColor = btnDIGdat9.BackColor = btnDIGdat10.BackColor = btnDIGdat11.BackColor = btnDIGdat12.BackColor = Color.Gray;
            bool[] values = BRmaster.ReadDigitalInputs(Convert.ToByte(lstModules.SelectedIndex), Convert.ToByte(txtDIGoffset.Text), Convert.ToByte(txtDIGsize.Text));
            if (values == null) return;

            for (int x = 0; x <= values.GetUpperBound(0); x++)
            {
                Control[] c = Controls.Find("btnDIGdat" + (x + 1).ToString(), true);
                Button ButtonRead = (Button)c[0];
                ButtonRead.Text = values.GetValue(x).ToString().ToLower();
                if(ButtonRead.Text.ToLower() == "false") ButtonRead.BackColor = Color.Red;
                else ButtonRead.BackColor = Color.Green;
            }
        }

		// --------------------------------------------------------------------
		// Button write digital output
        private void btnDIGwrite_Click(object sender, EventArgs e)
        {
            btnDIGdat1.BackColor = btnDIGdat2.BackColor = btnDIGdat3.BackColor = btnDIGdat4.BackColor = btnDIGdat5.BackColor = btnDIGdat6.BackColor = btnDIGdat7.BackColor = btnDIGdat8.BackColor = btnDIGdat9.BackColor = btnDIGdat10.BackColor = btnDIGdat11.BackColor = btnDIGdat12.BackColor = Color.Gray;
          
            try
            {
                int size = Convert.ToByte(txtDIGsize.Text);
                bool[] values = new bool[size];
                if (size == 0) throw new ArgumentException("Exception");

                for (int x = 0; x <= values.GetUpperBound(0); x++)
                {
                    Control[] c = Controls.Find("btnDIGdat" + (x + 1).ToString(), true);
                    Button ButtonWrite = (Button)c[0];
                    values[x] = Convert.ToBoolean(ButtonWrite.Text.Replace("1", "true"));
                    if (ButtonWrite.Text.ToLower() == "false") ButtonWrite.BackColor = Color.Red;
                    else ButtonWrite.BackColor = Color.Green;
                }
                BRmaster.WriteDigitalOutputs(Convert.ToByte(lstModules.SelectedIndex), Convert.ToByte(txtDIGoffset.Text), values);
            }
            catch (SystemException)
            {
                MessageBox.Show("Please enter valid size and data!", "Wrong value", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // --------------------------------------------------------------------
        // Button read analog input
        private void btnANAread_Click(object sender, EventArgs e)
        {
            txtANAdat1.Text = txtANAdat2.Text = txtANAdat3.Text = txtANAdat4.Text = "-";
            int[] values = BRmaster.ReadAnalogInputs(Convert.ToByte(lstModules.SelectedIndex), Convert.ToByte(txtANAoffset.Text), Convert.ToByte(txtANAsize.Text));
            if (values == null) return;

            for (int x = 0; x <= values.GetUpperBound(0); x++)
            {
                Control[] c = Controls.Find("txtANAdat" + (x + 1).ToString(), true);
                TextBox TextBoxTmp = (TextBox)c[0];
                TextBoxTmp.Text = values.GetValue(x).ToString();
            }
        }

        // --------------------------------------------------------------------
        // Button write analog output
        private void btnANAwrite_Click(object sender, EventArgs e)
        {

            try
            {
                int size = Convert.ToByte(txtANAsize.Text);
                int[] values = new int[size];
                if (size == 0) throw new ArgumentException("Exception");

                for (int x = 0; x <= values.GetUpperBound(0); x++)
                {
                    Control[] c = Controls.Find("txtANAdat" + (x + 1).ToString(), true);
                    TextBox TextBoxTmp = (TextBox)c[0];
                    values[x] = Convert.ToInt16(TextBoxTmp.Text);
                }
                BRmaster.WriteAnalogOutputs(Convert.ToByte(lstModules.SelectedIndex), Convert.ToByte(txtANAoffset.Text), values);
            }
            catch (SystemException)
            {
                MessageBox.Show("Please enter valid size and data!", "Wrong value", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // --------------------------------------------------------------------
        // Button read register
        private void btnREGread_Click(object sender, EventArgs e)
        {
            int[] values = { };

            txtREGdat1.Text = "";
            txtREGdat2.Text = "";

            try
            {
                values = BRmaster.ReadRegister(Convert.ToByte(lstModules.SelectedIndex), Convert.ToInt16(txtREGregister.Text));

                if (values == null) return;
                if (values.GetUpperBound(0) >= 0) txtREGdat1.Text = values.GetValue(0).ToString();
                if (values.GetUpperBound(0) >= 1) txtREGdat2.Text = values.GetValue(1).ToString();
            }
            catch (SystemException)
            {
                MessageBox.Show("Please enter valid data!", "Wrong value", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

		// --------------------------------------------------------------------
		// Button write register        
        private void btnREGwrite_Click(object sender, EventArgs e)
        {
            try
            {
                int[] values = new int[2];
                values[0] = Convert.ToInt16(txtREGdat1.Text);
                values[1] = Convert.ToInt16(txtREGdat2.Text);

                BRmaster.WriteRegister(Convert.ToByte(lstModules.SelectedIndex), Convert.ToInt16(txtREGregister.Text), values);
            }
            catch (SystemException)
            {
                MessageBox.Show("Please enter valid data!", "Wrong value", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region bus controller settings and statistics

        // ------------------------------------------------------------------------
        // Save new ethernet configuration
        private void btnCOMsave_Click(object sender, EventArgs e)
        {
            BRmaster.BCinfo.com_ip_flash = txtCOMip_flash.Text;
            BRmaster.BCinfo.com_subnet_mask = txtCOMsubnet.Text;
            BRmaster.BCinfo.com_gateway = txtCOMgateway.Text;
            BRmaster.BCinfo.ctrl_save();
        }

        // --------------------------------------------------------------------
        // Control command reboot
        private void btnCTRLreboot_Click(object sender, EventArgs e)
        {
            if (BRmaster != null) BRmaster.BCinfo.ctrl_reboot();
        }

 		// --------------------------------------------------------------------
		// Control command close connections
        private void btnCTRLdisconnect_Click(object sender, EventArgs e)
        {
            if (BRmaster != null) BRmaster.BCinfo.ctrl_close();
        }

        // --------------------------------------------------------------------
        // Control command erase configuration
        private void btnCTRLerase_Click(object sender, EventArgs e)
        {
            if (BRmaster != null) BRmaster.BCinfo.ctrl_erase();
        }

        // --------------------------------------------------------------------
        // Reset controller module configuration
        private void btnCTRLreset_mod_cfg_Click(object sender, EventArgs e)
        {
            if (BRmaster != null) BRmaster.BCinfo.ctrl_reset_cfg();
        }

        // ------------------------------------------------------------------------
        // Read values
        private void ReadControllerInfo(bool ReadAll)
        {
            try
            {
                // --------------------------------------------------------------------
                // Communication
                if (ReadAll)
                {
                    txtCOMip.Text = BRmaster.BCinfo.com_ip;
                    txtCOMip_flash.Text = BRmaster.BCinfo.com_ip_flash;
                    txtCOMsubnet.Text = BRmaster.BCinfo.com_subnet_mask;
                }
                txtCOMmac.Text = BRmaster.BCinfo.com_mac;
                txtCOMgateway.Text = BRmaster.BCinfo.com_gateway;
                txtCOMport.Text = BRmaster.BCinfo.com_port.ToString();
                txtCOMduration.Text = BRmaster.BCinfo.com_duration.ToString();
                txtCOMmtu.Text = BRmaster.BCinfo.com_mtu.ToString();
                txtCOMx2x.Text = BRmaster.BCinfo.com_x2x.ToString();
                txtCOMx2x_length.Text = BRmaster.BCinfo.com_x2x_length.ToString();
                //// --------------------------------------------------------------------
                //// Watchdog
                txtWDthreshold.Text = BRmaster.BCinfo.watchdog_threshold.ToString();
                txtWDstatus.Text = BRmaster.BCinfo.watchdog_status.ToString();
                txtWDelapsed.Text = BRmaster.BCinfo.watchdog_elapsed.ToString();
                txtWDmode.Text = BRmaster.BCinfo.watchdog_mode.ToString();
                //// --------------------------------------------------------------------
                //// Product data
                txtPDserial.Text = BRmaster.BCinfo.productdata_serial;
                txtPDcode.Text = BRmaster.BCinfo.productdata_code.ToString();
                txtPDhw_major.Text = BRmaster.BCinfo.productdata_hw_major.ToString();
                txtPDhw_minor.Text = BRmaster.BCinfo.productdata_hw_minor.ToString();
                txtPDfw_major.Text = BRmaster.BCinfo.productdata_fw_major.ToString();
                txtPDfw_minor.Text = BRmaster.BCinfo.productdata_fw_minor.ToString();
                txtPDfpga_hw.Text = BRmaster.BCinfo.productdata_hw_fpga.ToString();
                txtPDboot.Text = BRmaster.BCinfo.productdata_boot.ToString();
                txtPDdef_fw_major.Text = BRmaster.BCinfo.productdata_fw_major_def.ToString();
                txtPDdef_fw_minor.Text = BRmaster.BCinfo.productdata_fw_minor_def.ToString();
                txtPDupd_fw_major.Text = BRmaster.BCinfo.productdata_fw_major_upd.ToString();
                txtPDupd_fw_minor.Text = BRmaster.BCinfo.productdata_fw_minor_upd.ToString();
                txtPDdef_fpga_fw.Text = BRmaster.BCinfo.productdata_fw_fpga_def.ToString();
                txtPDupd_fpga_fw.Text = BRmaster.BCinfo.productdata_fw_fpga_upd.ToString();
                //// --------------------------------------------------------------------
                //// Modbus protocol
                txtMBclients.Text = BRmaster.BCinfo.modbus_clients.ToString();
                txtMBglobal_tel_cnt.Text = BRmaster.BCinfo.modbus_global_tel_cnt.ToString();
                txtMBlocal_tel_cnt.Text = BRmaster.BCinfo.modbus_local_tel_cnt.ToString();
                txtMBlocal_tel_cnt.Text = BRmaster.BCinfo.modbus_local_tel_cnt.ToString();
                txtMBglobal_prot_cnt.Text = BRmaster.BCinfo.modbus_global_prot_cnt.ToString();
                txtMBlocal_prot_cnt.Text = BRmaster.BCinfo.modbus_local_prot_cnt.ToString();
                txtMBglobal_prot_frag_cnt.Text = BRmaster.BCinfo.modbus_global_prot_frag_cnt.ToString();
                txtMBlocal_prot_frag_cnt.Text = BRmaster.BCinfo.modbus_local_prot_frag_cnt.ToString();
                txtMBglobal_max_cmd.Text = BRmaster.BCinfo.modbus_global_max_cmd.ToString();
                txtMBlocal_max_cmd.Text = BRmaster.BCinfo.modbus_local_max_cmd.ToString();
                txtMBglobal_min_cmd.Text = BRmaster.BCinfo.modbus_global_min_cmd.ToString();
                txtMBlocal_min_cmd.Text = BRmaster.BCinfo.modbus_local_min_cmd.ToString();
                //// --------------------------------------------------------------------
                //// Process data
                txtPRmodules.Text = BRmaster.BCinfo.process_modules.ToString();
                txtPRanalog_inp_cnt.Text = BRmaster.BCinfo.process_analog_inp_cnt.ToString();
                txtPRanalog_inp_size.Text = BRmaster.BCinfo.process_analog_inp_size.ToString();
                txtPRanalog_out_cnt.Text = BRmaster.BCinfo.process_analog_out_cnt.ToString();
                txtPRanalog_out_size.Text = BRmaster.BCinfo.process_analog_out_size.ToString();
                txtPRdigital_inp_cnt.Text = BRmaster.BCinfo.process_digital_inp_cnt.ToString();
                txtPRdigital_inp_size.Text = BRmaster.BCinfo.process_digital_inp_size.ToString();
                txtPRdigital_out_cnt.Text = BRmaster.BCinfo.process_digital_out_cnt.ToString();
                txtPRdigital_out_size.Text = BRmaster.BCinfo.process_digital_out_size.ToString();
                txtPRstatus_out_cnt.Text = BRmaster.BCinfo.process_status_out_cnt.ToString();
                txtPRstatus_out_size.Text = BRmaster.BCinfo.process_status_out_size.ToString();
                txtPRstatus_x2x_cnt.Text = BRmaster.BCinfo.process_status_x2x_cnt.ToString();
                txtPRstatus_x2x_size.Text = BRmaster.BCinfo.process_status_x2x_size.ToString();
                //// --------------------------------------------------------------------
                //// Misc information
                txtMISCnode.Text = BRmaster.BCinfo.misc_node.ToString();
                txtMISCinit_delay.Text = BRmaster.BCinfo.misc_init_delay.ToString();
                txtMISCcheck_io.Text = BRmaster.BCinfo.misc_check_io.ToString();
                txtMISCtelnet_pw.Text = BRmaster.BCinfo.misc_telnet_pw.ToString();
                txtMISCcfg_changed.Text = BRmaster.BCinfo.misc_cfg_changed.ToString();
                txtMISCstatus.Text = BRmaster.BCinfo.misc_status.ToString();
                txtMISCstatus_error.Text = BRmaster.BCinfo.misc_status_error.ToString();
                //// --------------------------------------------------------------------
                //// X2X information
                txtX2Xcnt.Text = BRmaster.BCinfo.x2x_cnt.ToString();
                txtX2Xbusoff.Text = BRmaster.BCinfo.x2x_bus_off.ToString();
                txtX2Xsyn_cnt.Text = BRmaster.BCinfo.x2x_syn_err.ToString();
                txtX2Xsyn_bus_timing.Text = BRmaster.BCinfo.x2x_syn_bus_timing.ToString();
                txtX2Xsyn_frame_timing.Text = BRmaster.BCinfo.x2x_syn_frame_timing.ToString();
                txtX2Xsyn_frame_crc.Text = BRmaster.BCinfo.x2x_syn_frame_crc.ToString();
                txtX2Xsyn_frame_pending.Text = BRmaster.BCinfo.x2x_syn_frame_pending.ToString();
                txtX2Xsyn_buffer_underrun.Text = BRmaster.BCinfo.x2x_syn_buffer_underrun.ToString();
                txtX2Xsyn_buffer_overflow.Text = BRmaster.BCinfo.x2x_syn_buffer_overflow.ToString();
                txtX2Xasyn_cnt.Text = BRmaster.BCinfo.x2x_asyn_err.ToString();
                txtX2Xasyn_bus_timing.Text = BRmaster.BCinfo.x2x_asyn_bus_timing.ToString();
                txtX2Xasyn_frame_timing.Text = BRmaster.BCinfo.x2x_asyn_frame_timing.ToString();
                txtX2Xasyn_frame_crc.Text = BRmaster.BCinfo.x2x_asyn_frame_crc.ToString();
                txtX2Xasyn_frame_pending.Text = BRmaster.BCinfo.x2x_asyn_frame_pending.ToString();
                txtX2Xasyn_buffer_underrun.Text = BRmaster.BCinfo.x2x_asyn_buffer_underrun.ToString();
                txtX2Xasyn_buffer_overflow.Text = BRmaster.BCinfo.x2x_asyn_buffer_overflow.ToString();
                //// --------------------------------------------------------------------
                //// Network statistics
                txtNScnt.Text = BRmaster.BCinfo.ns_cnt.ToString();
                txtNSlost_cnt.Text = BRmaster.BCinfo.ns_lost_cnt.ToString();
                txtNSoversize_cnt.Text = BRmaster.BCinfo.ns_oversize_cnt.ToString();
                txtNScrc_err_cnt.Text = BRmaster.BCinfo.ns_crc_cnt.ToString();
                txtNScollision_cnt.Text = BRmaster.BCinfo.ns_collision_cnt.ToString();
            }
            catch (SystemException) { };
        }
        #endregion

    }
}