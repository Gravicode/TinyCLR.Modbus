using GHIElectronics.TinyCLR.Devices.Modbus.Interface;
using GHIElectronics.TinyCLR.Devices.Uart;
using System;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace ModbusMaster
{
    class Program
    {
        static ModbusSlave slave;
        static void StartSlave()
        {
            //SLAVE
            var slavePort = GHIElectronics.TinyCLR.Devices.Uart.UartController.FromName
       (GHIElectronics.TinyCLR.Pins.SC20260.UartPort.Uart5);
            var setting = new UartSetting()
            {
                BaudRate = 19200,
                DataBits = 8,
                Parity = UartParity.None,
                StopBits = UartStopBitCount.One,
                Handshaking = UartHandshake.None,
            };

            slavePort.SetActiveSettings(setting);

            slavePort.Enable();
            //new SerialPort("COM1", 19200, Parity.None, 8, StopBits.One);
            var RtuInterface = new ModbusRtuInterface(slavePort, 19200, 8, UartStopBitCount.One, UartParity.None);
            Debug.WriteLine("rtu open:" + RtuInterface.isOpen + ", rtu connection:" + RtuInterface.IsConnectionOk);
            byte ModbusID = 5;
            slave = new ModbusSlave(RtuInterface, ModbusID);
            slave.CoilsChanged += Slave_CoilsChanged;
            slave.RegistersChanged += Slave_RegistersChanged;
            //
            // Init the SLAVE REGISTERS
            //
            initSlave();
            //
            // Start the slave now
            //
            slave.Start();
            Debug.WriteLine("running:" + slave.IsRunning);
            
        }
        static void StartMaster()
        {//MASTER
            var serial = GHIElectronics.TinyCLR.Devices.Uart.UartController.FromName
       (GHIElectronics.TinyCLR.Pins.SC20260.UartPort.Uart5);

            var setting = new UartSetting()
            {
                BaudRate = 19200,
                DataBits = 8,
                Parity = UartParity.None,
                StopBits = UartStopBitCount.One,
                Handshaking = UartHandshake.None,
            };

            serial.SetActiveSettings(setting);

            serial.Enable();

            GHIElectronics.TinyCLR.Devices.Modbus.Interface.IModbusInterface mbInterface;
            mbInterface = new GHIElectronics.TinyCLR.Devices.Modbus.Interface.ModbusRtuInterface(
                serial,
                19200,
                8,
                GHIElectronics.TinyCLR.Devices.Uart.UartStopBitCount.One,
                GHIElectronics.TinyCLR.Devices.Uart.UartParity.None);

            GHIElectronics.TinyCLR.Devices.Modbus.ModbusMaster mbMaster;
            mbMaster = new GHIElectronics.TinyCLR.Devices.Modbus.ModbusMaster(mbInterface);

            var mbTimeout = false;

            ushort[] reply = null;
            int count = 0;

            while (true)
            {
                try
                {
                    mbTimeout = false;

                    reply = mbMaster.ReadHoldingRegisters(11, 0, 1, 3333);
                    count++;

                    //if (count == 5)
                    //    break;
                }
                catch (System.Exception error)
                {
                    System.Diagnostics.Debug.WriteLine("Modbus Timeout");
                    mbTimeout = true;
                }

                if (!mbTimeout)
                {
                    System.Diagnostics.Debug.WriteLine("Modbus : " + (object)reply[0].ToString());
                }

                System.Threading.Thread.Sleep(1000);

            }
        }
        static void Main()
        {
            StartSlave();
            //StartMaster();
            Thread.Sleep(-1);
            
        }
        private static void initSlave()
        {
            var rnd = new Random();


            for (int index = 0; index < slave.inputRegistersSensor.Length; index++)
            {
                slave.inputRegistersSensor[index] = (ushort)(rnd.Next(1000));
            }
            for (int index = 0; index < slave.inputRegistersCal.Length; index++)
            {
                slave.inputRegistersCal[index] = (ushort)rnd.Next(100);

            }


            slave.coils[0] = true;          // Pressure - cal only
            slave.coils[1] = false;
            slave.coils[2] = true;          // Gauge ENABLED at startup
        }
        private static void Slave_RegistersChanged(ModbusSlave sender, bool calRegisters)
        {
            Debug.WriteLine("callreg:" + calRegisters);
        }

        private static void Slave_CoilsChanged(ModbusSlave sender)
        {
            Debug.WriteLine("coil changed");
        }
    }
}
