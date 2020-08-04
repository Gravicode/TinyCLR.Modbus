using GHIElectronics.TinyCLR.Devices.Modbus;
using GHIElectronics.TinyCLR.Devices.Modbus.Interface;
using System;

namespace ModbusMaster
{
    class ModbusSlave : ModbusDevice
    {
        // 
        // Input registers are the calibration values
        //
        public ushort[] inputRegistersCal = new ushort[40];
        public ushort[] inputRegistersSensor = new ushort[15];
        public bool[] coils = new bool[4];

        private CoilsChangedHandler onCoilsChanged;
        private InputRegistersChangedHandler onInputsChanged;

        /// <summary>
        /// Represents the delegate for the coils change event
        /// </summary>
        /// <param name="sender"></param>
        public delegate void CoilsChangedHandler(ModbusSlave sender);

        /// <summary>
        /// Represents the delegate for the intput registers changed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="calRegisters">Set if the calibration register changed</param>
        public delegate void InputRegistersChangedHandler(ModbusSlave sender, bool calRegisters);

        /// <summary>
        /// Raised when the coils are changed
        /// </summary>
        public event CoilsChangedHandler CoilsChanged;

        /// <summary>
        /// Raised when the inputs are changed
        /// </summary>
        public event InputRegistersChangedHandler RegistersChanged;

        public ModbusSlave(IModbusInterface intf, byte ModbusID, object syncObject = null)
            : base(intf, ModbusID, syncObject)
        {
            this.onCoilsChanged = this.OnCoilsChanged;
            this.onInputsChanged = this.OnInputsChanged;
        }

        protected override string OnGetDeviceIdentification(ModbusObjectId objectId)
        {
            switch (objectId)
            {
                case ModbusObjectId.VendorName:
                    return "Axon Instruments";
                case ModbusObjectId.ProductCode:
                    return "AX-100";
                case ModbusObjectId.MajorMinorRevision:
                    return "1.0";
                case ModbusObjectId.VendorUrl:
                    return "http://www.axoninstruments.biz";
                case ModbusObjectId.ProductName:
                    return "AXGIF";
                case ModbusObjectId.ModelName:
                    return "GAUGE";
                case ModbusObjectId.UserApplicationName:
                    return "GaugeModbusInterface";
            }
            return null;
        }

        protected override ModbusConformityLevel GetConformityLevel()
        {
            return ModbusConformityLevel.Regular;
        }

        protected override ModbusErrorCode OnWriteSingleCoil(bool isBroadcast, ushort address, bool value)
        {
            if (address < 5001 || address > 5003)
            {
                return ModbusErrorCode.IllegalDataAddress;
            }
            coils[address - 5001] = value;

            this.OnCoilsChanged(this);

            return ModbusErrorCode.NoError;
        }

        protected override ModbusErrorCode OnWriteMultipleCoils(bool isBroadcast, ushort startAddress, ushort outputCount, byte[] values)
        {
            return ModbusErrorCode.IllegalFunction;
        }

        protected override ModbusErrorCode OnWriteSingleRegister(bool isBroadcast, ushort address, ushort value)
        {
            if (address < 3004 || address > 3008)
            {
                if (address < 4001 || address > 4041)
                {
                    return ModbusErrorCode.IllegalDataAddress;
                }
                else
                {
                    inputRegistersCal[address - 4001] = value;

                    this.OnInputsChanged(this, (address >= 4001) ? true : false);

                    return ModbusErrorCode.NoError;
                }
            }
            inputRegistersSensor[address - 3001] = value;

            this.OnInputsChanged(this, (address >= 4001) ? true : false);

            return ModbusErrorCode.NoError;
        }

        protected override ModbusErrorCode OnReadInputRegisters(bool isBroadcast, ushort startAddress, ushort[] registers)
        {
            int len;

            if (startAddress > 4000)
            {
                len = 40 - (startAddress - 4001);   // Max read of 30 registers if start is zero, otherwise adjust it
            }
            else
            {
                len = 15 - (startAddress - 3001);   // Max read of 15 registers if start is zero, otherwise adjust it
            }
            if (startAddress < 3001 || startAddress > 3008)
            {
                if (startAddress < 4001 || startAddress > 4041)
                {
                    return ModbusErrorCode.IllegalDataAddress;
                }
                else
                {
                    if (registers.Length > len)
                    {
                        return ModbusErrorCode.IllegalDataAddress;
                    }
                    for (int index = 0; index < registers.Length; index++)
                    {
                        registers[index] = inputRegistersCal[(startAddress - 4001) + index];
                    }
                    return ModbusErrorCode.NoError;
                }
            }
            if (registers.Length > len)
            {
                return ModbusErrorCode.IllegalDataAddress;
            }
            for (int index = 0; index < registers.Length; index++)
            {
                registers[index] = inputRegistersSensor[(startAddress - 3001) + index];
            }
            return ModbusErrorCode.NoError;
        }

        protected override ModbusErrorCode OnReadCoils(bool isBroadcast, ushort startAddress, ushort coilCount, byte[] coils)
        {
            int len = (startAddress - 5001) + coilCount;

            if (len > 4)
            {
                return ModbusErrorCode.IllegalDataAddress;
            }
            if (startAddress < 5001 || startAddress > 5005)
            {
                return ModbusErrorCode.IllegalDataAddress;
            }
            byte mask = 1;
            int start = startAddress - 5001;

            coils[0] = 0;
            for (int index = 0; index < coilCount; index++)
            {
                if (this.coils[start++])
                {
                    coils[0] |= mask;
                }
                mask *= 2;
            }
            return ModbusErrorCode.NoError;
        }

        protected override ModbusErrorCode OnReadHoldingRegisters(bool isBroadcast, ushort startAddress, ushort[] registers)
        {
            return ModbusErrorCode.IllegalFunction;
        }

        protected override ModbusErrorCode OnWriteMultipleRegisters(bool isBroadcast, ushort startAddress, ushort[] registers)
        {
            int len;

            if (startAddress > 4000)
            {
                len = 40 - (startAddress - 4001);   // Max read of 30 registers if start is zero, otherwise adjust it
            }
            else
            {
                len = 15 - (startAddress - 3001);   // Max read of 15 registers if start is zero, otherwise adjust it
            }
            if (startAddress < 3004 || startAddress > 3008)
            {
                if (startAddress < 4001 || startAddress > 4041)
                {
                    return ModbusErrorCode.IllegalDataAddress;
                }
                else
                {
                    if (registers.Length > len)
                    {
                        return ModbusErrorCode.IllegalDataAddress;
                    }
                    for (int index = 0; index < registers.Length; index++)
                    {
                        inputRegistersCal[(startAddress - 4001) + index] = registers[index];
                    }
                    this.OnInputsChanged(this, true);

                    return ModbusErrorCode.NoError;
                }
            }
            if (registers.Length > len)
            {
                return ModbusErrorCode.IllegalDataAddress;
            }
            for (int index = 0; index < registers.Length; index++)
            {
                inputRegistersSensor[(startAddress - 3001) + index] = registers[index];
            }
            this.OnInputsChanged(this, false);

            return ModbusErrorCode.IllegalFunction;
        }

        private void OnCoilsChanged(ModbusSlave sender)
        {
            if (this.CoilsChanged != null)
                this.CoilsChanged(sender);
        }

        private void OnInputsChanged(ModbusSlave sender, bool calRegisters)
        {
            if (this.RegistersChanged != null)
                this.RegistersChanged(sender, calRegisters);
        }
    }
}
