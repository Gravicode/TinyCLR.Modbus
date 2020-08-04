using GHIElectronics.TinyCLR.Devices.Uart;
using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace ModbusClient
{
    class Program
    {
        static void Main()
        {
            var serial = GHIElectronics.TinyCLR.Devices.Uart.UartController.FromName
       (GHIElectronics.TinyCLR.Pins.SC20260.UartPort.Uart5);

            var setting = new UartSetting() {
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

                    reply = mbMaster.ReadHoldingRegisters(10, 0, 1, 3333);
                    count++;

                    if (count == 5)
                        break;
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
    }
}
