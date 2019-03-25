using System;
using System.Net;
using System.Collections.Generic;
using BR.HwdParser;

namespace ModbusTCPBR
{
    /// <summary>
    /// BR controller module information subclass. This class holds all informations for a single module. 
    /// </summary>
    public class MasterBR_MDinfo
    {
        private MasterBR BRmaster;
        ushort mod_nr = 0;
        ushort _id;
        string _name;
        ushort _analog_in_index;
        ushort _analog_out_index;
        ushort _digital_in_index;
        ushort _digital_out_index;

        // ------------------------------------------------------------------------
        /// <summary>Read module status.</summary>
        public ushort status
        {
            get { return (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, Convert.ToUInt16(0xA000 + mod_nr * 16)); }
        }
        // ------------------------------------------------------------------------
        /// <summary>Read module hardware id.</summary>
        public ushort id
        {
            get { return _id; }
        }
        // ------------------------------------------------------------------------
        /// <summary>Read module product name.</summary>
        public string name
        {
            get { return _name; }
        }
        // ------------------------------------------------------------------------
        /// <summary>Read module serial number.</summary>
        public string serial
        {
            get
            {
                byte[] values = null;
                if (BRmaster.IsConnected) BRmaster.MBmaster.ReadHoldingRegister(MasterBR.ID_parameter, 0, Convert.ToUInt16(0xA001 + mod_nr * 16), 3, ref values);
                if (values == null) return "error";
                else if (values.Length == 6)
                {

                    if (BRmaster.Byte2Word(values[0], values[1]).ToString("000").Length > 4) return BRmaster.Byte2Word(values[0], values[1]).ToString("X").Remove(0, 4) + BRmaster.Byte2Word(values[2], values[3]).ToString("000") + BRmaster.Byte2Word(values[4], values[5]).ToString("000");
                    else return BRmaster.Byte2Word(values[0], values[1]).ToString("000") + BRmaster.Byte2Word(values[2], values[3]).ToString("000") + BRmaster.Byte2Word(values[4], values[5]).ToString("000");
                }
                else return "error";
            }
        }
        // ------------------------------------------------------------------------
        /// <summary>Read module analog input data.</summary>
        internal ushort analog_in_index
        {
            get { return _analog_in_index; }
        }
        // ------------------------------------------------------------------------
        /// <summary>Read module analog output data.</summary>
        internal ushort analog_out_index
        {
            get { return _analog_out_index; }
        }
        // ------------------------------------------------------------------------
        /// <summary>Read module digital input data.</summary>
        internal ushort digital_in_index
        {
            get { return _digital_in_index; }
        }
        // ------------------------------------------------------------------------
        /// <summary>Read module digital output data.</summary>
        internal ushort digital_out_index
        {
            get { return _digital_out_index; }
        }
        // ------------------------------------------------------------------------
        /// <summary>Read/Write module hardware ID. If the hardware ID is different from 
        /// the configuration the module will not start.</summary>
        public ushort cfg_hw
        {
            get { return (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, Convert.ToUInt16(0xA008 + mod_nr * 16)); }
            set { BRmaster.MBmaster.WriteSingleRegister(MasterBR.ID_parameter, 0, Convert.ToUInt16(0xA008 + mod_nr * 16), BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)value))); }
        }
        // ------------------------------------------------------------------------
        /// <summary>Read/Write module function module. Note! When you change the function
        /// model you have to restart the bus coupler.</summary>
        public ushort cfg_function_modell
        {
            get { return (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, Convert.ToUInt16(0xA009 + mod_nr * 16)); }
            set { BRmaster.MBmaster.WriteSingleRegister(MasterBR.ID_parameter, 0, Convert.ToUInt16(0xA009 + mod_nr * 16), BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)value))); }
        }
        // ------------------------------------------------------------------------
        /// <summary>Read/Write module configuration data.</summary>
        internal ushort cfg_index
        {
            get { return (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, Convert.ToUInt16(0xA00A + mod_nr * 16)); }
            set { BRmaster.MBmaster.WriteSingleRegister(MasterBR.ID_parameter, 0, Convert.ToUInt16(0xA00A + mod_nr * 16), BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)value))); }
        }
        // ------------------------------------------------------------------------
        /// <summary>Read/Write module configuration size.</summary>
        internal ushort cfg_size
        {
            get { return (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, Convert.ToUInt16(0xA00B + mod_nr * 16)); }
            set { BRmaster.MBmaster.WriteSingleRegister(MasterBR.ID_parameter, 0, Convert.ToUInt16(0xA00B + mod_nr * 16), BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)value))); }
        }
        // ------------------------------------------------------------------------
        /// <summary>Read module firmware version.</summary>
        public ushort cfg_firmware
        {
            get { return (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, Convert.ToUInt16(0xA00C + mod_nr * 16)); }
        }
        // ------------------------------------------------------------------------
        /// <summary>Read module hardware variant.</summary>
        public ushort cfg_variant
        {
            get { return (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, Convert.ToUInt16(0xA00D + mod_nr * 16)); }
        }

        // ------------------------------------------------------------------------
        // Get module product name
        internal string Get_HardwareName(ushort HardwareId)
        {
            try
            {
                HwdTypeBuilder HwdTypes = new HwdTypeBuilder();
                HwdTypes.Load(true);

                if (HardwareId > 0)
                {
                    List<PropertyObject> nodes = HwdTypes.GetNodes("Modno", HardwareId);
                    if (nodes.Count > 0)
                    {
                        return (nodes[0]["ID"] as string);
                    }
                }
                if (HardwareId == 41528) return "X20BT9400";
                if (HardwareId == 41865) return "X20PS9402";
                return "unknown";
            }
            catch (SystemException error)
            {
                throw (error);
            }
        }

        // ------------------------------------------------------------------------
        // Module information prototype
        internal MasterBR_MDinfo(MasterBR _BRmaster, byte _mod_nr)
        {
            BRmaster = _BRmaster;
            mod_nr = _mod_nr;
            _id = (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, Convert.ToUInt16(0xA001 + mod_nr * 16));
            _name = Get_HardwareName(_id);

            _analog_in_index = (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, Convert.ToUInt16(0xA004 + mod_nr * 16));
            _analog_out_index = (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, Convert.ToUInt16(0xA005 + mod_nr * 16));
            _digital_in_index = (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, Convert.ToUInt16(0xA006 + mod_nr * 16));
            _digital_out_index = (ushort)BRmaster.SyncReadWord(MasterBR.ID_parameter, Convert.ToUInt16(0xA007 + mod_nr * 16));
        }
    }
}
