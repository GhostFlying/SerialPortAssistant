using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SerialPortAssistant
{
    class SerialPortCommunication
    {
        CancellationTokenSource _continue;
        SerialPort serialPort;
        Thread receiveThread;
        TextBox receivedTextBox;
        public bool isOpen = false;

        public SerialPortCommunication(TextBox received)
        {
            serialPort = new SerialPort();
            receivedTextBox = received;
        }

        public void InitialPort(string portName, int baudRate, int dataBits, StopBits stopBits, Parity parity)
        {
            serialPort.PortName = portName;
            serialPort.BaudRate = baudRate;
            serialPort.DataBits = dataBits;
            serialPort.StopBits = stopBits;
            serialPort.Parity = parity;
        }

        public string[] GetPorts()
        {
            return SerialPort.GetPortNames();
        }

        public bool OpenPort()
        {
            if (serialPort.IsOpen)
                MessageBox.Show("The port is already opended. Please check.");
            else
            {
                serialPort.Open();
                isOpen = true;
                receiveThread = new Thread(ReceiveData);
                receiveThread.Start();
            }
            return true;
        }

        public void ClosePort()
        {
            if (receiveThread != null && receiveThread.IsAlive)
            {
                _continue.Cancel();
                receiveThread = null;
            }
            else if (serialPort.IsOpen)
            {
                serialPort.Close();
            }
            isOpen = false;
        }

        public void SendData(string text, bool isNewLine)
        {
            if (serialPort.IsOpen)
            {
                if (isNewLine)
                    serialPort.WriteLine(text);
                else
                    serialPort.Write(text);
            }
        }

        public void SendData(byte[] data)
        {
            if (serialPort.IsOpen)
                serialPort.Write(data, 0, data.Length);
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
                AppendTextToReceived(bytes);
            }
            serialPort.Close();
        }

        void AppendTextToReceived(byte[] bytes)
        {
            string text = "";
            if (MainWindow.showHex)
            {
                text = BitConverter.ToString(bytes).Replace("-", " ") + " ";
            }
            else
            {
                text = Encoding.Default.GetString(bytes);
            }

            Application.Current.Dispatcher.BeginInvoke(new Action(delegate
            {
                receivedTextBox.Text += text;
            }));
        }
    }
}
