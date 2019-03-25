using System;
using System.Net;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using ModbusTCP;


namespace ModbusTCPBR
{
    /// <summary>Modbus TCP main class for X20BC0087 controller. Requirements: Microsoft .Net Framework 2.0 or higher.</summary>
    public class MasterBR
    {
        // ------------------------------------------------------------------------
        // Public constants for exception
        /// <exclude/>
        public const byte excWatchdog = 1;
        /// <exclude/>
        public const byte excTimeout = 2;
        /// <exclude/>
        public const byte excConnection = 3;
        /// <exclude/>
        public const byte excNoModule = 10;
        /// <exclude/>
        public const byte excNoDigInData = 11;
        /// <exclude/>
        public const byte excNoDigOutData = 12;
        /// <exclude/>
        public const byte excNoAnaInData = 13;
        /// <exclude/>
        public const byte excNoAnaOutData = 14;
        /// <exclude/>
        public const byte excWrongRegData = 15;
        /// <exclude/>
        public const byte excDataSize = 16;
        /// <exclude/>
        public const byte excDataEmptyAnswer = 17;
        /// <exclude/>
        public const byte excDataRange = 20;
        /// <exclude/>
        public const byte excWrongEthernetFormat = 30;
        /// <exclude/>
        public const byte excUnhandled = 40;

        // ------------------------------------------------------------------------
        // Private declarations
        /// <exclude/>
        internal ModbusTCP.Master MBmaster;
        internal static Timer ReconnectTimer;
        internal Timer RefreshTimer;
        internal ushort RefreshCounter;
        internal ushort FrameErrorCounter;
        internal ushort ConnectionErrorCounter;
        private bool _connected = false;
        private bool IsPending = false;
        internal ushort _refresh = 0;

        internal bool[] dig_in_buffer;
        internal ushort dig_in_length = 0;
        internal bool[] dig_out_buffer;
        internal ushort dig_out_length = 0;
        internal int[] ana_in_buffer;
        internal ushort ana_in_length = 0;
        internal int[] ana_out_buffer;
        internal ushort ana_out_length = 0;

        // ------------------------------------------------------------------------
        // Internal constants
        internal const ushort ID_watchdog = 0;
        internal const ushort ID_dig_in = 1;
        internal const ushort ID_dig_out = 2;
        internal const ushort ID_ana_in = 3;
        internal const ushort ID_ana_out = 4;
        internal const ushort ID_parameter = 10;
        internal const ushort ID_register = 20;
        internal const ushort ID_value = 30;
        internal const ushort ID_boundary = 40;

        internal byte[] err_watchdog = { 0, 0xc2 };

        // ------------------------------------------------------------------------
        // Public declarations
        /// <summary>X20BC0087 controller information class. For more information see class description below. Requirements: Microsoft .Net Framework 2.0 or higher.</summary>
        public MasterBR_BCinfo BCinfo;
        /// <summary>X20 module information class. For more information see class description below.</summary>
        public MasterBR_MDinfo[] MDinfo;
        /// <exclude/>
        public delegate void BCexception(ushort reason);
        /// <summary>Exception event from X20BC0087 controller.</summary>
        /// <include file='MasterBR.xml' path='//Doc[@name="BCexception"]/remarks' />
        public event BCexception OnBCexception;

        // ------------------------------------------------------------------------
        /// <summary>Shows that a connection is active.</summary>
        public bool IsConnected
        {
            get { return _connected; }
            set { _connected = value; }
        }

        // ------------------------------------------------------------------------
        /// <summary>IO refresh timer in ms.</summary>
        public ushort PollRefresh
        {
            get { return Convert.ToUInt16(_refresh * 5); }
            set
            {
                _refresh = Convert.ToUInt16(value / 5);
                if (_connected) BCinfo.watchdog_reset();
            }
        }

        // ------------------------------------------------------------------------
        /// <overloads>This method has two overloads.</overloads>
        /// <summary>BR modbus TCP master instance without parameter.
        /// Take a look at the next constructor for more information.</summary>
        public MasterBR()
        {
        }

        // ------------------------------------------------------------------------
        /// <summary>BR modbus TCP master instance with connection parameters.</summary>
        /// <param name="ip">IP adress or host name of the X20BC0087 controller.
        /// See FAQ section for more information how to determinate the host name.</param>
        /// <param name="port">Port number of X20BC0087 controller. Usually port 502 is used.</param>
        /// <include file='MasterBR.xml' path='//Doc[@name="MasterBR"]/example' />
        /// <include file='MasterBR.xml' path='//Doc[@name="MasterBR"]/code' />
        public MasterBR(string ip, ushort port)
        {
            Connect(ip, port);
        }

        // ------------------------------------------------------------------------
        /// <summary>Connect to bus controller.</summary>
        /// <param name="ip">IP adress or host name of the X20BC0087 controller.
        /// See FAQ section for more information how to determinate the host name.</param>
        /// <param name="port">Port number of X20BC0087 controller. Usually port 502 is used.</param>
        public void Connect(string ip, ushort port)
        {
            byte[] result = { };

            while (ReconnectTimer != null)
            {
                Thread.Sleep(100);
            }

            try
            {
                MBmaster = new Master(ip, port, false);
                MBmaster.OnResponseData += new ModbusTCP.Master.ResponseData(MBmaster_OnResponseData);
                MBmaster.OnException += new ModbusTCP.Master.ExceptionData(MBmaster_OnException);
                _connected = true;

                // Disable boundary check
                MBmaster.WriteSingleRegister(MasterBR.ID_boundary, 0, 0x1182, BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)0xC0)), ref result);
                // Retry when bus coupler is still booting up retry after 10s
                if (result == null)
                {
                    Thread.Sleep(10000);
                    MBmaster.WriteSingleRegister(MasterBR.ID_boundary, 0, 0x1182, BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)0xC0)), ref result);
                    if (result == null)
                    {
                        _connected = false;
                        if (MBmaster != null)
                        {
                            MBmaster.Dispose();
                            MBmaster = null;
                            BCinfo = null;
                        }
                        throw new System.Net.Sockets.SocketException();
                    }
                }

                // Read module info, disable boundary check
                BCinfo = new MasterBR_BCinfo(this);
                MasterMDinfo();

                dig_in_length = Convert.ToUInt16(BCinfo.process_digital_inp_cnt * 8);
                dig_in_buffer = new bool[dig_in_length];
                dig_out_length = Convert.ToUInt16(BCinfo.process_digital_out_cnt * 8);
                dig_out_buffer = new bool[dig_out_length];
                ana_in_length = Convert.ToUInt16(BCinfo.process_analog_inp_cnt);
                ana_in_buffer = new int[ana_in_length];
                ana_out_length = Convert.ToUInt16(BCinfo.process_analog_out_cnt * 8);
                ana_out_buffer = new int[ana_out_length];
            }
            catch (Exception error)
            {
                _connected = false;
                if (MBmaster != null)
                {
                    MBmaster.Dispose();
                    MBmaster = null;
                    BCinfo = null;
                }
                throw (error);
            }
        }

        // ------------------------------------------------------------------------
        /// <summary>Disconnect from bus controller.</summary>
        public void Disconnect()
        {
            if (MBmaster != null)
            {
                MBmaster.Dispose();
                MBmaster = null;
                BCinfo = null;
            }
            else return;
            _connected = false;
            RefreshTimer.Change(0, 0);
            ReconnectTimer = new Timer(new TimerCallback(ReconnectTimer_Elapsed), this, 10000, 10000);
        }

        // ------------------------------------------------------------------------
        /// <summary>Refresh module information.</summary>
        public void MasterMDinfo()
        {
            MDinfo = new MasterBR_MDinfo[0];
            MasterBR_MDinfo MDinfo_tmp;

            try
            {
                do
                {
                    MDinfo_tmp = new MasterBR_MDinfo(this, Convert.ToByte(MDinfo.Length));
                    if (MDinfo_tmp.status != 0)
                    {
                        Array.Resize(ref MDinfo, MDinfo.Length + 1);
                        MDinfo[MDinfo.Length - 1] = MDinfo_tmp;
                    }
                }
                while (MDinfo_tmp.status != 0);
            }
            catch (Exception error)
            {
                throw (error);
            }
        }

        // ------------------------------------------------------------------------
        /// <summary>Sychnronous read digital inputs with no offset.</summary>
        /// <param name="module_nr">The position of the module.</param>
        /// <param name="size">Number of bits to read.</param>
        /// <returns>Result of the read command as bool arrary.</returns>
        public bool[] ReadDigitalInputs(byte module_nr, byte size)
        {
            return ReadDigitalInputs(module_nr, 0, size);
        }

        // ------------------------------------------------------------------------
        /// <summary>Sychnronous read digital inputs starting from a specified offset.</summary>
        /// <param name="module_nr">The position of the module.</param>
        /// <param name="offset">Starting offset for digital read command.</param>
        /// <param name="size">Number of bits to read.</param>
        /// <returns>Result of the read command as bool arrary.</returns>
        public bool[] ReadDigitalInputs(byte module_nr, ushort offset, ushort size)
        {
            byte[] values = { };
            bool[] data = new bool[size];

            if (ValidateData(module_nr, size))
            {
                if (MDinfo[module_nr].digital_in_index != 0xFFFF)
                {
                    if (size <= dig_in_buffer.Length - (MDinfo[module_nr].digital_in_index * 8 + offset))
                    {
                        // ------------------------------------------------------------------------
                        // Request value from bus coupler
                        if (_refresh == 0)
                        {
                            // Read digital inputs, convert data to bool array
                            MBmaster.ReadDiscreteInputs(ID_value, 0, Convert.ToUInt16(MDinfo[module_nr].digital_in_index * 8 + offset), size, ref values);
                            if ((values == null) && (OnBCexception != null)) OnBCexception(excDataEmptyAnswer);
                            else
                            {
                                BitArray tmp1 = new System.Collections.BitArray(values);
                                bool[] tmp2 = new bool[tmp1.Count];
                                tmp1.CopyTo(tmp2, 0);
                                Array.Copy(tmp2, 0, data, 0, size);
                            }
                        }
                        // ------------------------------------------------------------------------
                        // Request value from internal buffer
                        else Array.Copy(dig_in_buffer, MDinfo[module_nr].digital_in_index * 8 + offset, data, 0, size);
                        return data;
                    }
                    else if (OnBCexception != null) OnBCexception(excDataSize);
                }
                else if (OnBCexception != null) OnBCexception(excNoDigInData);
            }
            return null;
        }

        // ------------------------------------------------------------------------
        /// <summary>Sychnronous write digital outputs with no offset.</summary>
        /// <param name="module_nr">The position of the module.</param>
        /// <param name="values">Data as integer array.</param>
        /// <returns>Result of the write command. (True = success, False = failed)</returns>
        public bool WriteDigitalOutputs(byte module_nr, bool[] values)
        {
            return WriteDigitalOutputs(module_nr, 0, values);
        }

        // ------------------------------------------------------------------------
        /// <summary>Sychnronous write digital outputs starting from a specified offset.</summary>
        /// <param name="module_nr">The position of the module.</param>
        /// <param name="offset">Starting offset for digital write command.</param>
        /// <param name="values">Data as integer array.</param>
        /// <returns>Result of the write command. (True = success, False = failed)</returns>
        public bool WriteDigitalOutputs(byte module_nr, byte offset, bool[] values)
        {
            byte[] result = { };

            if (ValidateData(module_nr, Convert.ToUInt16(values.Length)))
            {
                if (MDinfo[module_nr].digital_out_index != 0xFFFF)
                {
                    if (values.Length <= dig_out_buffer.Length - (MDinfo[module_nr].digital_out_index * 8 + offset))
                    {
                        // ------------------------------------------------------------------------
                        // Send value from bus coupler
                        if (_refresh == 0) MBmaster.WriteMultipleCoils(ID_value, 0, Convert.ToUInt16(MDinfo[module_nr].digital_out_index * 8 + offset), Convert.ToUInt16(values.Length), Bit2Byte(values), ref result);
                        // ------------------------------------------------------------------------
                        // Send value from internal buffer
                        else Array.Copy(values, 0, dig_out_buffer, MDinfo[module_nr].digital_out_index * 8 + offset, Convert.ToUInt16(values.Length));
                        return true;
                    }
                    else if (OnBCexception != null) OnBCexception(excDataSize);
                }
                else if (OnBCexception != null) OnBCexception(excNoDigOutData);
            }
            return false;
        }

        // ------------------------------------------------------------------------
        /// <summary>Sychnronous read analog inputs with no offset.</summary>
        /// <param name="module_nr">The position of the module.</param>
        /// <param name="size">Number of words to read.</param>
        /// <returns>Result of the read command as integer arrary.</returns>
        public int[] ReadAnalogInputs(byte module_nr, byte size)
        {
            return ReadAnalogInputs(module_nr, 0, size);
        }

        // ------------------------------------------------------------------------
        /// <summary>Sychnronous read analog inputs starting from a specified offset.</summary>
        /// <param name="module_nr">The position of the module.</param>
        /// <param name="offset">Starting offset for analog read command.</param>
        /// <param name="size">Number of words to read.</param>
        /// <returns>Result of the read command as integer arrary. UDINT and DINT values are returned as two integers.</returns>
        public int[] ReadAnalogInputs(byte module_nr, byte offset, byte size)
        {
            byte[] values = { };
            int[] data = new int[size]; ;

            if (ValidateData(module_nr, size))
            {
                if (MDinfo[module_nr].analog_in_index != 0xFFFF)
                {
                    if (size <= ana_in_buffer.Length - (MDinfo[module_nr].analog_in_index / 2 + offset))
                    {
                        // Request values from bus coupler
                        if (_refresh == 0)
                        {
                            MBmaster.ReadInputRegister(ID_value, 0, Convert.ToUInt16(0x0000 + MDinfo[module_nr].analog_in_index / 2 + offset), size, ref values);
                            if ((values == null) && (OnBCexception != null)) OnBCexception(excDataEmptyAnswer);
                            else return ByteArray2WordArray(values);
                        }
                        // ------------------------------------------------------------------------
                        // Request value from internal buffer
                        else
                        {
                            Array.Copy(ana_in_buffer, MDinfo[module_nr].analog_in_index / 2 + offset, data, 0, size);
                            return data;
                        }
                    }
                    else if (OnBCexception != null) OnBCexception(excDataSize);
                }
                else if (OnBCexception != null) OnBCexception(excNoAnaInData);
            }
            return null;
        }

        // ------------------------------------------------------------------------
        /// <summary>Sychnronous write analog outputs with no offset.</summary>
        /// <param name="module_nr">The position of the module.</param>
        /// <param name="values">Data as integer array. UDINT and DINT values have be split into two integers.</param>
        /// <returns>Result of the write command. (True = success, False = failed)</returns>
        public bool WriteAnalogOutputs(byte module_nr, int[] values)
        {
            return WriteAnalogOutputs(module_nr, 0, values);
        }

        // ------------------------------------------------------------------------
        /// <summary>Sychnronous write analog outputs starting from a specified offset.</summary>
        /// <param name="module_nr">The position of the module.</param>
        /// <param name="offset">Starting offset for analog read command.</param>
        /// <param name="values">Data as integer array.</param>
        /// <returns>Result of the write command. (True = success, False = failed)</returns>
        public bool WriteAnalogOutputs(byte module_nr, byte offset, int[] values)
        {
            byte[] result = { };

            if (ValidateData(module_nr, Convert.ToUInt16(values.Length)))
            {
                if (MDinfo[module_nr].analog_out_index != 0xFFFF)
                {
                    if (values.Length <= ana_out_buffer.Length - (MDinfo[module_nr].analog_out_index / 2 + offset))
                    {
                        // ------------------------------------------------------------------------
                        // Send values from bus coupler
                        if (_refresh == 0) MBmaster.WriteMultipleRegister(ID_value, 0, Convert.ToUInt16(0x0800 + MDinfo[module_nr].analog_out_index / 2 + offset), WordArray2WordByte(values), ref result);
                        // ------------------------------------------------------------------------
                        // Send value from internal buffer
                        else Array.Copy(values, 0, ana_out_buffer, MDinfo[module_nr].analog_out_index / 2 + offset, Convert.ToUInt16(values.Length));
                        return true;
                    }
                    else if (OnBCexception != null) OnBCexception(excDataSize);
                }
                else if (OnBCexception != null) OnBCexception(excNoAnaOutData);
            }
            return false;
        }

        // ------------------------------------------------------------------------
        /// <summary>Sychnronous read register data.</summary>
        /// <param name="module_nr">The position of the module.</param>
        /// <param name="register">Configuration register to read.</param>
        /// <returns>The method always returns 2 integer values with low and high word of the register.</returns>
        public int[] ReadRegister(byte module_nr, int register)
        {
            byte[] values = { };
            int[] swap = { 0, 0 };
            int[] reg = { module_nr, register };

            MBmaster.ReadWriteMultipleRegister(ID_register, 0, 0x1286, 2, 0x1284, WordArray2WordByte(reg), ref values);
            if ((values == null) && (OnBCexception != null)) OnBCexception(excDataEmptyAnswer);
            else if (values.Length == 4)
            {
                swap[0] = Byte2Word(values[2], values[3]);
                swap[1] = Byte2Word(values[0], values[1]);
                return swap;
            }
            return null;
        }

        // ------------------------------------------------------------------------
        /// <summary>Sychnronous write register data.</summary>
        /// <param name="module_nr">The position of the module.</param>
        /// <param name="register">Configuration register to read.</param>
        /// <param name="values">Values should be an array of 2 integer values with low and high word of the register.</param>
        /// <returns>Result of the write command. (True = success, False = failed)</returns>
        public bool WriteRegister(byte module_nr, int register, int[] values)
        {
            if ((values.Length != 2) && (OnBCexception != null))
            {
                OnBCexception(excDataSize);
                return false;
            }
            else
            {
                byte[] result = { };
                int[] reg = { module_nr, register, values[1], values[0] };

                MBmaster.WriteMultipleRegister(ID_register, 0, 0x1280, WordArray2WordByte(reg), ref result);
                return true;
            }
        }

        // ------------------------------------------------------------------------
        // Validate read/write data
        internal bool ValidateData(byte module_nr, ushort size)
        {
            if ((!_connected) && (OnBCexception != null))
            {
                OnBCexception(excConnection);
                return false;
            }
            if ((module_nr > MDinfo.GetUpperBound(0)) && (OnBCexception != null))
            {
                OnBCexception(excNoModule);
                return false;
            }
            if ((size == 0) && (OnBCexception != null))
            {
                OnBCexception(excDataSize);
                return false;
            }
            return true;
        }

        // ------------------------------------------------------------------------
        // Read word value
        internal int SyncReadWord(ushort id, ushort adr)
        {
            byte[] values = null;
            if (_connected) MBmaster.ReadHoldingRegister(id, 0, adr, 1, ref values);
            if (values == null) return 0xFFFF;
            else if (values.Length == 2) return Byte2Word(values[0], values[1]);
            else return -1;
        }

        // ------------------------------------------------------------------------
        // Read long value
        internal long SyncReadLong(ushort id, ushort adr)
        {
            byte[] values = null;
            if (_connected) MBmaster.ReadHoldingRegister(id, 0, adr, 2, ref values);
            if (values == null) return 0xFFFF;
            else if (values.Length == 4) return Byte2Long(values[0], values[1], values[2], values[3]);
            else return -1;
        }

        //// ------------------------------------------------------------------------
        //// Look if message is watchdog telegram
        //internal bool IsWatchdog(byte[] data)
        //{
        //    if ((data != null) &&
        //        (data.Length == 2) &&
        //        (data[0] == err_watchdog[0]) &&
        //        (data[1] == err_watchdog[1]))
        //    {
        //        if (OnBCexception != null) OnBCexception(excWatchdog);
        //        if (RefreshTimer != null) RefreshTimer.Change(0, 0);
        //        return true;
        //    }
        //    else return false;
        //}

        // ------------------------------------------------------------------------
        // Convert bit to byte
        internal byte[] Bit2Byte(bool[] values)
        {
            byte[] data = new byte[values.Length / 8 + Convert.ToUInt16((values.Length % 8) > 0)];

            for (int x = 0; x < data.Length; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    data[x] = Convert.ToByte(data[x] | Convert.ToByte(values[x * 8 + y]) << y);
                    if (x * 8 + y + 1 == values.Length) break;
                }
            }
            return data;
        }

        // ------------------------------------------------------------------------
        // Convert byte to word
        internal int Byte2Word(byte byte1, byte byte2)
        {
            byte[] bytes = { byte2, byte1 };

            return BitConverter.ToInt16(bytes, 0);
        }

        // ------------------------------------------------------------------------
        // Convert byte to long
        internal long Byte2Long(byte byte1, byte byte2, byte byte3, byte byte4)
        {
            byte[] bytes = { byte4, byte3, byte2, byte1 };

            return BitConverter.ToInt32(bytes, 0);
        }

        // ------------------------------------------------------------------------
        // Convert byte array to word array
        internal int[] ByteArray2WordArray(byte[] values)
        {
            int[] result = new int[values.Length / 2];
            for (int x = 0; x < values.GetUpperBound(0); x = x + 2)
            {
                result[x / 2] = Byte2Word(values[x], values[x + 1]);
            }
            return result;
        }

        // ------------------------------------------------------------------------
        // Convert word array to byte array
        internal byte[] WordArray2WordByte(int[] values)
        {
            byte[] result = new byte[values.Length * 2];
            for (int x = 0; x < values.Length; x++)
            {
                byte[] _val = BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)values[x]));
                result[2 * x] = _val[0];
                result[2 * x + 1] = _val[1];
            }
            return result;
        }

        // ------------------------------------------------------------------------
        // Modbus response data
        private void MBmaster_OnResponseData(ushort ID, byte unit, byte function, byte[] data)
        {
            switch (ID)
            {
                // ------------------------------------------------------------------------
                // Watchdog telegram
                case ID_watchdog:
                    if ((data[0] == err_watchdog[0]) &&
                        (data[1] == err_watchdog[1]) &&
                        (RefreshTimer != null))
                    {
                        RefreshTimer.Change(0, 0);
                        if (OnBCexception != null) OnBCexception(excWatchdog);
                    }
                    break;
                // ------------------------------------------------------------------------
                // Digital input data
                case MasterBR.ID_dig_in:
                    BitArray tmp_bit = new System.Collections.BitArray(data);
                    tmp_bit.CopyTo(dig_in_buffer, 0);
                    break;
                // ------------------------------------------------------------------------
                // Analog input data
                case MasterBR.ID_ana_in:
                    int[] tmp_word;
                    tmp_word = ByteArray2WordArray(data);
                    tmp_word.CopyTo(ana_in_buffer, 0);
                    break;
            }
            IsPending = false;
            FrameErrorCounter = 0;
            ConnectionErrorCounter = 0;
        }

        // ------------------------------------------------------------------------
        // Modbus exception
        internal void MBmaster_OnException(ushort id, byte unit, byte function, byte exception)
        {
            if (MBmaster == null) return;

            // --------------------------------------------------------------------
            // Timeout exception
            if ((exception == ModbusTCP.Master.excExceptionTimeout) &&
                (OnBCexception != null))
            {
                Disconnect();
                if (OnBCexception != null) OnBCexception(excTimeout);
                return;
            }
            // --------------------------------------------------------------------
            // Connection lost exception
            else if ((exception == ModbusTCP.Master.excExceptionConnectionLost) ||
                     (exception == ModbusTCP.Master.excExceptionNotConnected))
            {
                Disconnect();
                if (OnBCexception != null) OnBCexception(excConnection);
                return;
            }

            // --------------------------------------------------------------------
            // Register read/write exception
            if ((exception == ModbusTCP.Master.excIllegalDataVal) &&
                (id == ID_register) &&
                (OnBCexception != null)) OnBCexception(excWrongRegData);
            // --------------------------------------------------------------------
            // Wrong register address exception
            else if ((id == ID_register) &&
                     (exception == ModbusTCP.Master.excIllegalDataAdr) &&
                     (OnBCexception != null)) OnBCexception(excWrongRegData);
            // --------------------------------------------------------------------
            // Wrong data address exception
            else if ((exception == ModbusTCP.Master.excIllegalDataAdr) &&
                     (OnBCexception != null)) OnBCexception(excDataSize);
            else if (OnBCexception != null)
            {
                OnBCexception(excUnhandled);
            }
        }

        // ------------------------------------------------------------------------
        // Refresh data telegram
        internal void RefreshTimer_Elapsed(object state)
        {
            try
            {
                if (_connected)
                {
                    // ------------------------------------------------------------------------
                    // Make sure we have no pending tickets
                    if (IsPending == false)
                    {
                        // ------------------------------------------------------------------------
                        // Read watchdog status
                        if (RefreshCounter == ID_watchdog)
                        {
                            IsPending = true;
                            MBmaster.ReadHoldingRegister(ID_watchdog, 0, 0x1042, 1);
                        }
                        // ------------------------------------------------------------------------
                        // Read digital inputs
                        if (RefreshCounter == ID_dig_in)
                        {
                            if (dig_in_length == 0) RefreshCounter++;
                            else
                            {
                                IsPending = true;
                                MBmaster.ReadDiscreteInputs(ID_dig_in, 0, 0x0000, dig_in_length);
                            }
                        }
                        // ------------------------------------------------------------------------
                        // Write digital outputs
                        if (RefreshCounter == ID_dig_out)
                        {
                            if (dig_out_length == 0) RefreshCounter++;
                            else
                            {
                                IsPending = true;
                                MBmaster.WriteMultipleCoils(ID_dig_out, 0, 0x0000, dig_out_length, Bit2Byte(dig_out_buffer));
                            }
                        }
                        // ------------------------------------------------------------------------
                        // Read analog inputs
                        if (RefreshCounter == ID_ana_in)
                        {
                            if (ana_in_length == 0) RefreshCounter++;
                            else
                            {
                                IsPending = true;
                                MBmaster.ReadInputRegister(ID_ana_in, 0, 0x0000, ana_in_length);
                            }
                        }
                        // ------------------------------------------------------------------------
                        // Write analog outputs
                        if (RefreshCounter == ID_ana_out)
                        {
                            if (ana_out_length == 0) RefreshCounter++;
                            else
                            {
                                IsPending = true;
                                MBmaster.WriteMultipleRegister(ID_ana_out, 0, 0x0800, WordArray2WordByte(ana_out_buffer));
                            }
                        }

                        if (_refresh != 0) RefreshCounter++;
                        if ((RefreshCounter > ID_ana_out) || (_refresh == 0)) RefreshCounter = 0;
                    }
                    // ------------------------------------------------------------------------
                    // Request next packet
                    else
                    {
                        FrameErrorCounter++;
                        if (FrameErrorCounter > 3)
                        {
                            IsPending = false;
                            FrameErrorCounter = 0;
                            ConnectionErrorCounter++;
                            // ------------------------------------------------------------------------
                            // Assume that connection is lost after 3 attempts
                            if (ConnectionErrorCounter > 3)
                            {
                                MBmaster_OnException(0, 0, 0, ModbusTCP.Master.excExceptionTimeout);
                            }
                        }
                    }
                }
            }
            catch (System.Exception error)
            {
                throw (error);
            }
        }

        // ------------------------------------------------------------------------
        // Wait until reconnect
        internal void ReconnectTimer_Elapsed(object state)
        {
            if (ReconnectTimer != null) ReconnectTimer.Dispose();
            ReconnectTimer = null;
        }

    }
}
