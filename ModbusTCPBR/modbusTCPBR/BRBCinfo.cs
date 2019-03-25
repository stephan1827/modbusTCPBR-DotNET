using System;
using System.Net;
using System.Threading;
using ModbusTCP;

namespace ModbusTCPBR
{
    /// <summary>
    /// BR controller information subclass. This class holds all information for the 
    /// controller like bus statistics etc. It can also be used to restart the controller.
    /// </summary>
    public class MasterBR_BCinfo
    {
        // ------------------------------------------------------------------------
        // Private declarations
        private MasterBR BRmaster;

        // ------------------------------------------------------------------------
        // Information class prototype.
        internal MasterBR_BCinfo(MasterBR _BRmaster)
        {
            BRmaster = _BRmaster;
        }

        #region communication
        // ------------------------------------------------------------------------
        /// <summary>Read current ip address from controller.</summary>
        public string com_ip
        {
            get
            {
                byte[] values = null;
                if (BRmaster.IsConnected) BRmaster.MBmaster.ReadHoldingRegister(MasterBR.ID_parameter, 0, 0x1013, 4, ref values);
                if (values == null) return "error";
                else if (values.Length == 8) return values[1].ToString() + "." + values[3].ToString() + "." + values[5].ToString() + "." + values[7].ToString();
                else return "error";
            }
        }
        // ------------------------------------------------------------------------
        /// <summary>Read ip address from controller that is stored in flash.</summary>
        public string com_ip_flash
        {
            get
            {
                byte[] values = null;
                if (BRmaster.IsConnected) BRmaster.MBmaster.ReadHoldingRegister(MasterBR.ID_parameter, 0, 0x1003, 4, ref values);
                if (values == null) return "error";
                else if (values.Length == 8) return values[1].ToString() + "." + values[3].ToString() + "." + values[5].ToString() + "." + values[7].ToString();
                else return "error";
            }
            set
            {
                byte[] values = String2Adr(value);
                if ((BRmaster.IsConnected) && (values != null))
                {
                    BRmaster.MBmaster.WriteMultipleRegister(MasterBR.ID_parameter, 0, 0x1003, values);
                }
            }
        }

        // ------------------------------------------------------------------------
        /// <summary>Read mac address from controller.</summary>
        public string com_mac
        {
            get
            {
                byte[] values = null;
                if (BRmaster.IsConnected) BRmaster.MBmaster.ReadHoldingRegister(MasterBR.ID_parameter, 0, 0x1000, 3, ref values);
                if (values == null) return "error";
                else if (values.Length == 6) return string.Format("{0:X2}", values[0]) + "-" + string.Format("{0:X2}", values[1]) + "-" + string.Format("{0:X2}", values[2]) + "-" + string.Format("{0:X2}", values[3]) + "-" + string.Format("{0:X2}", values[4]) + "-" + string.Format("{0:X2}", values[5]);
                else return "error";
            }
        }
        // ------------------------------------------------------------------------
        /// <summary>Read subnet mask from controller.</summary>
        public string com_subnet_mask
        {
            get
            {
                byte[] values = null;
                if (BRmaster.IsConnected) BRmaster.MBmaster.ReadHoldingRegister(MasterBR.ID_parameter, 0, 0x1007, 4, ref values);
                if (values == null) return "error";
                else if (values.Length == 8) return values[1].ToString() + "." + values[3].ToString() + "." + values[5].ToString() + "." + values[7].ToString();
                else return "error";
            }
            set
            {
                byte[] values = String2Adr(value);
                if ((BRmaster.IsConnected) && (values != null))
                {
                    BRmaster.MBmaster.WriteMultipleRegister(MasterBR.ID_parameter, 0, 0x1007, values);
                }
            }
        }
        /// <summary>Read gateway address from controller.</summary>
        public string com_gateway
        {
            get
            {
                byte[] values = null;
                if (BRmaster.IsConnected) BRmaster.MBmaster.ReadHoldingRegister(MasterBR.ID_parameter, 0, 0x100B, 4, ref values);
                if (values == null) return "error";
                else if (values.Length == 8) return values[1].ToString() + "." + values[3].ToString() + "." + values[5].ToString() + "." + values[7].ToString();
                else return "error";
            }
            set
            {
                byte[] values = String2Adr(value);
                if ((BRmaster.IsConnected) && (values != null))
                {
                    BRmaster.MBmaster.WriteMultipleRegister(MasterBR.ID_parameter, 0, 0x100B, values);
                }
            }
        }
        // ------------------------------------------------------------------------
        /// <summary>Read/Write port number from controller.</summary>
        public ushort com_port
        {
            get { return (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x100F); }
            set { BRmaster.MBmaster.WriteSingleRegister(MasterBR.ID_parameter, 0, 0x100F, BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)value))); }
        }
        // ------------------------------------------------------------------------
        /// <summary>Read/Write duration of current connection.</summary>
        public ushort com_duration
        {
            get { return (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x1010); }
            set { BRmaster.MBmaster.WriteSingleRegister(MasterBR.ID_parameter, 0, 0x1010, BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)value))); }
        }
        // ------------------------------------------------------------------------
        /// <summary>Read/Write MTU of current connection.</summary>
        public ushort com_mtu
        {
            get { return (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x1011); }
            set { BRmaster.MBmaster.WriteSingleRegister(MasterBR.ID_parameter, 0, 0x1011, BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)value))); }
        }
        // ------------------------------------------------------------------------
        /// <summary>Read/Write x2x cycle time of current connection.</summary>
        public ushort com_x2x
        {
            get { return (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x1012); }
            set { BRmaster.MBmaster.WriteSingleRegister(MasterBR.ID_parameter, 0, 0x1012, BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)value))); }
        }
        // ------------------------------------------------------------------------
        /// <summary>Read/Write x2x cable length.</summary>
        public ushort com_x2x_length
        {
            get { return (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x1017); }
            set { BRmaster.MBmaster.WriteSingleRegister(MasterBR.ID_parameter, 0, 0x1017, BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)value))); }
        }
        #endregion

        #region watchdog
        // ------------------------------------------------------------------------
        /// <summary>Read/Write watchdog threshold from controller.</summary>
        public ushort watchdog_threshold
        {
            get { return (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x1040); }
            set { BRmaster.MBmaster.WriteSingleRegister(MasterBR.ID_parameter, 0, 0x1040, BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)value))); }
        }
        // ------------------------------------------------------------------------
        /// <summary>Read/Write watchdog elapsed time from controller.</summary>
        public ushort watchdog_elapsed
        {
            get { return (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x1041); }
        }
        // ------------------------------------------------------------------------
        /// <summary>Read watchdog status from controller.</summary>
        public ushort watchdog_status
        {
            get { return (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x1042); }
        }
        // ------------------------------------------------------------------------
        /// <summary>Read/Write watchdog mode from controller. Possible values are</summary>
        /// <summary>0xC0    deactivates the watchdog</summary>
        /// <summary>0xC1    resets watchdog with every access</summary>
        /// <summary>0xC2    resets watchdog only with write access</summary>
        public ushort watchdog_mode
        {
            get { return (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x1043); }
            set { BRmaster.MBmaster.WriteSingleRegister(MasterBR.ID_parameter, 0, 0x1043, BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)value))); }
        }
        // ------------------------------------------------------------------------
        /// <summary>Reset watchdog and enable auto watchdog timer.</summary>
        public void watchdog_reset()
        {
            ushort refresh_timer;
            if (BRmaster._refresh == 0) refresh_timer = Convert.ToUInt16(watchdog_threshold / 2);
            else refresh_timer = BRmaster._refresh;

            if (BRmaster.IsConnected)
            {
                BRmaster.MBmaster.WriteSingleRegister(0, 0, 0x1044, BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)0xc1)));
                // ------------------------------------------------------------------------
                // Destroy old timer and create new one
                if (BRmaster.RefreshTimer != null) BRmaster.RefreshTimer.Change(0, refresh_timer);
                else BRmaster.RefreshTimer = new Timer(new TimerCallback(BRmaster.RefreshTimer_Elapsed), this, 100, refresh_timer);
            }
        }
        #endregion

        #region product data
        // ------------------------------------------------------------------------
        /// <summary>Read serial number from controller.</summary>
        public string productdata_serial
        {
            get
            {
                byte[] values = null;
                if (BRmaster.IsConnected) BRmaster.MBmaster.ReadHoldingRegister(MasterBR.ID_parameter, 0, 0x1080, 3, ref values);
                if (values == null) return "error";
                else if (values.Length == 6) return BRmaster.Byte2Word(values[0], values[1]).ToString("000") + BRmaster.Byte2Word(values[2], values[3]).ToString("000") + BRmaster.Byte2Word(values[4], values[5]).ToString("000");
                else return "error";
            }
        }
        // ------------------------------------------------------------------------
        /// <summary>Read product code from controller.</summary>
        public ushort productdata_code
        {
            get { return (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x1083); }
        }
        // ------------------------------------------------------------------------
        /// <summary>Read hardware major revision from controller.</summary>
        public ushort productdata_hw_major
        {
            get { return (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x1084); }
        }
        // ------------------------------------------------------------------------
        /// <summary>Read hardware minor revision from controller.</summary>
        public ushort productdata_hw_minor
        {
            get { return (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x1085); }
        }
        // ------------------------------------------------------------------------
        /// <summary>Read firmware major revision from controller.</summary>
        public ushort productdata_fw_major
        {
            get { return (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x1086); }
        }
        // ------------------------------------------------------------------------
        /// <summary>Read firmware minor revision from controller.</summary>
        public ushort productdata_fw_minor
        {
            get { return (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x1087); }
        }
        // ------------------------------------------------------------------------
        /// <summary>Read fpga hardware revision from controller.</summary>
        public ushort productdata_hw_fpga
        {
            get { return (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x1088); }
        }
        // ------------------------------------------------------------------------
        /// <summary>Read active boot block from controller.</summary>
        public ushort productdata_boot
        {
            get { return (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x1089); }
        }
        // ------------------------------------------------------------------------
        /// <summary>Read default firmware major revision from controller.</summary>
        public ushort productdata_fw_major_def
        {
            get { return (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x108A); }
        }
        // ------------------------------------------------------------------------
        /// <summary>Read default firmware minor revision from controller.</summary>
        public ushort productdata_fw_minor_def
        {
            get { return (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x108B); }
        }
        // ------------------------------------------------------------------------
        /// <summary>Read update firmware major revision from controller.</summary>
        public ushort productdata_fw_major_upd
        {
            get { return (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x108C); }
        }
        // ------------------------------------------------------------------------
        /// <summary>Read update firmware minor revision from controller.</summary>
        public ushort productdata_fw_minor_upd
        {
            get { return (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x108D); }
        }
        // ------------------------------------------------------------------------
        /// <summary>Read fpga default firmware revision from controller.</summary>
        public ushort productdata_fw_fpga_def
        {
            get { return (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x108E); }
        }
        // ------------------------------------------------------------------------
        /// <summary>Read fpga update firmware revision from controller.</summary>
        public ushort productdata_fw_fpga_upd
        {
            get { return (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x108F); }
        }
        #endregion

        #region modbus protocol
        // ------------------------------------------------------------------------
        /// <summary>Resfresh timer for TCP polling. The modbus thread will look for an
        /// answer every X ms. The default value is 10ms.</summary>
        public ushort modbus_refresh
        {
            get { return BRmaster.MBmaster.refresh; }
            set { BRmaster.MBmaster.refresh = value; }
        }

        // ------------------------------------------------------------------------
        /// <summary>Get number of connected clients.</summary>
        public ushort modbus_clients
        {
            get { return (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x10C0); }
        }
        // ------------------------------------------------------------------------
        /// <summary>Get number of global telegram counter.</summary>
        public long modbus_global_tel_cnt
        {
            get { return BRmaster.SyncReadLong(MasterBR.ID_parameter, 0x10C1); }
        }
        // ------------------------------------------------------------------------
        /// <summary>Get number of local telegram counter.</summary>
        public long modbus_local_tel_cnt
        {
            get { return BRmaster.SyncReadLong(MasterBR.ID_parameter, 0x10C3); }
        }
        // ------------------------------------------------------------------------
        /// <summary>Get number of global protocol counter.</summary>
        public long modbus_global_prot_cnt
        {
            get { return BRmaster.SyncReadLong(MasterBR.ID_parameter, 0x10C5); }
        }
        // ------------------------------------------------------------------------
        /// <summary>Get number of local protocol counter.</summary>
        public long modbus_local_prot_cnt
        {
            get { return BRmaster.SyncReadLong(MasterBR.ID_parameter, 0x10C7); }
        }
        // ------------------------------------------------------------------------
        /// <summary>Get number of global protocol fragment counter.</summary>
        public long modbus_global_prot_frag_cnt
        {
            get { return BRmaster.SyncReadLong(MasterBR.ID_parameter, 0x10D1); }
        }
        // ------------------------------------------------------------------------
        /// <summary>Get number of local protocol fragment counter.</summary>
        public long modbus_local_prot_frag_cnt
        {
            get { return BRmaster.SyncReadLong(MasterBR.ID_parameter, 0x10D3); }
        }
        // ------------------------------------------------------------------------
        /// <summary>Get global maximum command execution time.</summary>
        public long modbus_global_max_cmd
        {
            get { return BRmaster.SyncReadLong(MasterBR.ID_parameter, 0x10C9); }
        }
        // ------------------------------------------------------------------------
        /// <summary>Get local maximum command execution time.</summary>
        public long modbus_local_max_cmd
        {
            get { return BRmaster.SyncReadLong(MasterBR.ID_parameter, 0x10CB); }
        }
        // ------------------------------------------------------------------------
        /// <summary>Get global minimum command execution time.</summary>
        public long modbus_global_min_cmd
        {
            get { return BRmaster.SyncReadLong(MasterBR.ID_parameter, 0x10CD); }
        }
        // ------------------------------------------------------------------------
        /// <summary>Get local minimum command execution time.</summary>
        public long modbus_local_min_cmd
        {
            get { return BRmaster.SyncReadLong(MasterBR.ID_parameter, 0x10CF); }
        }
        #endregion

        #region process data
        // ------------------------------------------------------------------------
        /// <summary>Get number of connected clients.</summary>
        public ushort process_modules
        {
            get { return (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x1100); }
        }
        // ------------------------------------------------------------------------
        /// <summary>Get number of analog input registers.</summary>
        public long process_analog_inp_cnt
        {
            get { return BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x1101); }
        }
        // ------------------------------------------------------------------------
        /// <summary>Get size of analog input registers.</summary>
        public long process_analog_inp_size
        {
            get { return BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x1102); }
        }
        // ------------------------------------------------------------------------
        /// <summary>Get number of analog output registers.</summary>
        public long process_analog_out_cnt
        {
            get { return BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x1103); }
        }
        // ------------------------------------------------------------------------
        /// <summary>Get size of analog output registers.</summary>
        public long process_analog_out_size
        {
            get { return BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x1104); }
        }
        // ------------------------------------------------------------------------
        /// <summary>Get number of digital input registers.</summary>
        public long process_digital_inp_cnt
        {
            get { return BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x1105); }
        }
        // ------------------------------------------------------------------------
        /// <summary>Get size of digital input registers.</summary>
        public long process_digital_inp_size
        {
            get { return BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x1106); }
        }
        // ------------------------------------------------------------------------
        /// <summary>Get number of digital output registers.</summary>
        public long process_digital_out_cnt
        {
            get { return BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x1107); }
        }
        // ------------------------------------------------------------------------
        /// <summary>Get size of digital output registers.</summary>
        public long process_digital_out_size
        {
            get { return BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x1108); }
        }

        // ------------------------------------------------------------------------
        /// <summary>Get number of status output registers.</summary>
        public long process_status_out_cnt
        {
            get { return BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x1107); }
        }
        // ------------------------------------------------------------------------
        /// <summary>Get size of status output registers.</summary>
        public long process_status_out_size
        {
            get { return BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x1108); }
        }
        // ------------------------------------------------------------------------
        /// <summary>Get number of status x2x registers.</summary>
        public long process_status_x2x_cnt
        {
            get { return BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x1105); }
        }
        // ------------------------------------------------------------------------
        /// <summary>Get size of status x2x registers.</summary>
        public long process_status_x2x_size
        {
            get { return BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x1106); }
        }
        #endregion

        #region control
        // ------------------------------------------------------------------------
        /// <summary>Save configuration to flash.</summary>
        public void ctrl_save()
        {
            if (BRmaster.IsConnected) BRmaster.MBmaster.WriteSingleRegister(MasterBR.ID_parameter, 0, 0x1140, BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)0xC1)));
        }
        // ------------------------------------------------------------------------
        /// <summary>Load configuration from flash.</summary>
        public void ctrl_load()
        {
            if (BRmaster.IsConnected) BRmaster.MBmaster.WriteSingleRegister(MasterBR.ID_parameter, 0, 0x1141, BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)0xC1)));
        }
        // ------------------------------------------------------------------------
        /// <summary>Erase controller configuration from flash.</summary>
        public void ctrl_erase()
        {
            if (BRmaster.IsConnected) BRmaster.MBmaster.WriteSingleRegister(MasterBR.ID_parameter, 0, 0x1142, BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)0xC1)));
        }
        // ------------------------------------------------------------------------
        /// <summary>Reboot control. This will close the current connection!</summary>
        public void ctrl_reboot()
        {
            if (BRmaster.IsConnected) BRmaster.MBmaster.WriteSingleRegister(MasterBR.ID_parameter, 0, 0x1143, BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)0xC0)));
        }
        // ------------------------------------------------------------------------
        /// <summary>Close all TCP connections. This will close the current connection!</summary>
        public void ctrl_close()
        {
            if (BRmaster.IsConnected) BRmaster.MBmaster.WriteSingleRegister(MasterBR.ID_parameter, 0, 0x1144, BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)0xC1)));
        }
        // ------------------------------------------------------------------------
        /// <summary>Resets the module configuration and sets the controller back to partital configuration.</summary>
        public void ctrl_reset_cfg()
        {
            if (BRmaster.IsConnected)
            {
                BRmaster.MBmaster.WriteSingleRegister(MasterBR.ID_parameter, 0, 0x1145, BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)0xC0)));
                Thread.Sleep(20);
                BRmaster.MBmaster.WriteSingleRegister(MasterBR.ID_parameter, 0, 0x1146, BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)0xC1)));
                Thread.Sleep(20);
                BRmaster.MBmaster.WriteSingleRegister(MasterBR.ID_parameter, 0, 0x1188, BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)0xC0)));
                Thread.Sleep(50);
                BRmaster.MBmaster.WriteSingleRegister(MasterBR.ID_parameter, 0, 0x1140, BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)0xC1)));
                Thread.Sleep(2000);
                BRmaster.MBmaster.WriteSingleRegister(MasterBR.ID_parameter, 0, 0x1143, BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)0xC1)));
            }
        }

        #endregion

        #region misc
        // ------------------------------------------------------------------------
        /// <summary>Read node number.</summary>
        public ushort misc_node
        {
            get { return (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x1180); }
        }
        // ------------------------------------------------------------------------
        /// <summary>Read/Write module init delay.</summary>
        public ushort misc_init_delay
        {
            get { return (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x1181); }
            set { BRmaster.MBmaster.WriteSingleRegister(MasterBR.ID_parameter, 0, 0x1181, BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)value))); }
        }
        // ------------------------------------------------------------------------
        /// <summary>Read/Write io limit check.</summary>
        public ushort misc_check_io
        {
            get { return (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x1182); }
            set { BRmaster.MBmaster.WriteSingleRegister(MasterBR.ID_parameter, 0, 0x1182, BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)value))); }
        }
        // ------------------------------------------------------------------------
        /// <summary>Read/Write enable telnet password.</summary>
        public ushort misc_telnet_pw
        {
            get { return (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x1183); }
            set { BRmaster.MBmaster.WriteSingleRegister(MasterBR.ID_parameter, 0, 0x1183, BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)value))); }
        }
        // ------------------------------------------------------------------------
        /// <summary>Read/Write module configuration changed flag. If this flag is true a new configuration
        /// was downloaded with the target designer.</summary>
        public bool misc_cfg_changed
        {
            get
            {
                int values = BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x1184);
                if (values == 0xC1) return true;
                else return false;
            }
            set
            {
                if (value == true) BRmaster.MBmaster.WriteSingleRegister(MasterBR.ID_parameter, 0, 0x1184, BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)0xC1)));
                else BRmaster.MBmaster.WriteSingleRegister(MasterBR.ID_parameter, 0, 0x1184, BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)0xC0)));
            }
        }
        // ------------------------------------------------------------------------
        /// <summary>Read controller status.</summary>
        public ushort misc_status
        {
            get { return (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x1186); }
        }
        // ------------------------------------------------------------------------
        /// <summary>Read controller status when an error occurs. The error number is bit coded:
        /// Bit 0: Config data error
        /// Bit 1: Flash read error
        /// Bit 2: IP address conflict
        /// Bit 3: Module mismatch
        /// Bit 4: Runtime module error
        /// Bit 5: Watchdog expired</summary>
        public ushort misc_status_error
        {
            get { return (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x1187); }
        }
        #endregion

        #region x2x
        // ------------------------------------------------------------------------
        /// <summary>Read x2x cycle count.</summary>
        public ushort x2x_cnt
        {
            get { return (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x11C0); }
        }
        // ------------------------------------------------------------------------
        /// <summary>Read x2x bus off.</summary>
        public ushort x2x_bus_off
        {
            get { return (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x11C1); }
        }
        // ------------------------------------------------------------------------
        /// <summary>Read x2x sync errors.</summary>
        public ushort x2x_syn_err
        {
            get { return (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x11C2); }
        }
        // ------------------------------------------------------------------------
        /// <summary>Read x2x sync bus timing errors.</summary>
        public ushort x2x_syn_bus_timing
        {
            get { return (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x11C3); }
        }				// ------------------------------------------------------------------------
        /// <summary>Read x2x sync frame timing errors.</summary>
        public ushort x2x_syn_frame_timing
        {
            get { return (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x11C4); }
        }				// ------------------------------------------------------------------------
        /// <summary>Read x2x sync frame checksum errors.</summary>
        public ushort x2x_syn_frame_crc
        {
            get { return (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x11C5); }
        }				// ------------------------------------------------------------------------
        /// <summary>Read x2x sync frame pending errors.</summary>
        public ushort x2x_syn_frame_pending
        {
            get { return (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x11C6); }
        }				// ------------------------------------------------------------------------
        /// <summary>Read x2x sync buffer underrun errors.</summary>
        public ushort x2x_syn_buffer_underrun
        {
            get { return (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x11C7); }
        }
        /// <summary>Read x2x sync buffer overflow errors.</summary>
        public ushort x2x_syn_buffer_overflow
        {
            get { return (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x11C8); }
        }
        // ------------------------------------------------------------------------
        /// <summary>Read x2x async errors.</summary>
        public ushort x2x_asyn_err
        {
            get { return (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x11C9); }
        }
        // ------------------------------------------------------------------------
        /// <summary>Read x2x async bus timing errors.</summary>
        public ushort x2x_asyn_bus_timing
        {
            get { return (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x11CA); }
        }				// ------------------------------------------------------------------------
        /// <summary>Read x2x async frame timing errors.</summary>
        public ushort x2x_asyn_frame_timing
        {
            get { return (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x11CB); }
        }				// ------------------------------------------------------------------------
        /// <summary>Read x2x async frame checksum errors.</summary>
        public ushort x2x_asyn_frame_crc
        {
            get { return (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x11CC); }
        }				// ------------------------------------------------------------------------
        /// <summary>Read x2x async frame pending errors.</summary>
        public ushort x2x_asyn_frame_pending
        {
            get { return (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x11CD); }
        }				// ------------------------------------------------------------------------
        /// <summary>Read x2x async buffer underrun errors.</summary>
        public ushort x2x_asyn_buffer_underrun
        {
            get { return (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x11CE); }
        }
        /// <summary>Read x2x async buffer overflow errors.</summary>
        public ushort x2x_asyn_buffer_overflow
        {
            get { return (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x11CF); }
        }
        #endregion

        #region network statistic
        // ------------------------------------------------------------------------
        /// <summary>Read frame counter.</summary>
        public ushort ns_cnt
        {
            get { return (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x1200); }
        }
        // ------------------------------------------------------------------------
        /// <summary>Read frame lost counter.</summary>
        public ushort ns_lost_cnt
        {
            get { return (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x1201); }
        }
        // ------------------------------------------------------------------------
        /// <summary>Read frame oversize counter.</summary>
        public ushort ns_oversize_cnt
        {
            get { return (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x1202); }
        }
        // ------------------------------------------------------------------------
        /// <summary>Read frame crc counter.</summary>
        public ushort ns_crc_cnt
        {
            get { return (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x1203); }
        }
        // ------------------------------------------------------------------------
        /// <summary>Read frame counter.</summary>
        public ushort ns_collision_cnt
        {
            get { return (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, 0x1206); }
        }
        #endregion

        // ------------------------------------------------------------------------
        /// <summary>Convert int to uint.</summary>
        internal ushort Int2UInt(int value)
        {
            byte[] bytes1 = BitConverter.GetBytes(value);
            byte[] bytes2 = { bytes1[1], bytes1[0] };
            return BitConverter.ToUInt16(bytes1, 0);
        }

        // ------------------------------------------------------------------------
        /// <summary>Convert string to ethernet address.</summary>
        internal byte[] String2Adr(string adr)
        {
            byte[] values = { 0, 0, 0, 0, 0, 0, 0, 0 };
            char[] separator = { '.' };
            string[] sep = adr.Split(separator);

            if (sep.GetUpperBound(0) == 3)
            {
                for (ushort x = 0; x < 4; x++)
                {
                    if (Convert.ToInt16(sep[x]) <= 255) values[2 * x + 1] = Convert.ToByte(sep[x]);
                    else
                    {
                        BRmaster.MBmaster_OnException(0, 0, 0 ,MasterBR.excWrongEthernetFormat);
                        return null;
                    }
                }
            }
            else
            {
                BRmaster.MBmaster_OnException(0, 0, 0, MasterBR.excWrongEthernetFormat);
                return null;
            }
            return values;
        }
    }
}
