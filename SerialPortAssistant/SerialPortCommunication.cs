using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace SerialPortAssistant
{
    class SerialPortCommunication
    {
        ConcurrentQueue<byte> readedData;
        CancellationTokenSource _continue;
        SerialPort serialPort;
        Thread receiveThread;

        SerialPortCommunication(ConcurrentQueue<byte> data)
        {
            readedData = data;
        }

        public void InitialPort(string portName, int baudRate, int dataBits, StopBits stopBits, Parity parity)
        {
            serialPort.PortName = portName;
            serialPort.BaudRate = baudRate;
            serialPort.DataBits = dataBits;
            serialPort.StopBits = stopBits;
            serialPort.Parity = parity;
        }

        public void OpenPort()
        {
            if (serialPort.IsOpen)
                MessageBox.Show("The port is already opended. Please check.");
            else
            {
                serialPort.Open();
                receiveThread = new Thread(ReceiveData);
                receiveThread.Start();
            }
        }

        public void ClosePort()
        {
            if (receiveThread != null && receiveThread.IsAlive)
            {
                _continue.Cancel();
                receiveThread = null;
            }
        }

        void ReceiveData()
        {
            _continue = new CancellationTokenSource();
            while (!_continue.Token.IsCancellationRequested)
            {
                int length = serialPort.BytesToRead;

                if (length < 1)
                    continue;

                byte[] bytes = new byte[length];
                serialPort.BaseStream.Read(bytes, 0, length);
                for (int i = 0; i < length; i++)
                {
                    readedData.Enqueue(bytes[i]);
                }
            }
            receiveThread.Join();
            serialPort.Close();
        }
    }
}
