using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SerialPortAssistant
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<string> ComsList { get;private set; }
        public bool SendNewLine { get; set; }
        public bool SendHex { get; set; }
        public bool ShowHex
        {
            get
            {
                return showHex;
            }
            set
            {
                showHex = value;
            }
        }
        SerialPortCommunication sp;
        public static bool showHex = true;
        public MainWindow()
        {
            InitializeComponent();
            sp = new SerialPortCommunication(ReceivedData);
            ComsList = new ObservableCollection<string>(sp.GetPorts());
            DataContext = this;
        }

        private void OpenComButton_Click(object sender, RoutedEventArgs e)
        {
            if (ComSelectedComboBox.SelectedItem == null)
            {
                MessageBox.Show("请先选择串口");
                return;
            }
            string comName = ComSelectedComboBox.SelectedItem as string;
            int baudRate = int.Parse(BaudRateSelectComboBox.SelectionBoxItem as string);
            int dataBits = DataBitsSelectComboBox.SelectedIndex + 5;
            System.IO.Ports.Parity parity;
            System.IO.Ports.StopBits stopBits;

            switch (StopBitsSelectComboBox.SelectedIndex)
            {
                case 0:
                    {
                        stopBits = System.IO.Ports.StopBits.One;
                        break;
                    }
                case 1:
                    {
                        stopBits = System.IO.Ports.StopBits.OnePointFive;
                        break;
                    }
                case 2:
                    {
                        stopBits = System.IO.Ports.StopBits.Two;
                        break;
                    }
                default:
                    {
                        stopBits = System.IO.Ports.StopBits.One;
                        break;
                    }
            }

            switch (CheckBitsSelectComboBox.SelectedIndex)
            {
                case 0:
                    {
                        parity = System.IO.Ports.Parity.None;
                        break;
                    }
                case 1:
                    {
                        parity = System.IO.Ports.Parity.Odd;
                        break;
                    }
                case 2:
                    {
                        parity = System.IO.Ports.Parity.Even;
                        break;
                    }
                case 3:
                    {
                        parity = System.IO.Ports.Parity.Mark;
                        break;
                    }
                case 4:
                    {
                        parity = System.IO.Ports.Parity.Space;
                        break;
                    }
                default:
                    {
                        parity = System.IO.Ports.Parity.None;
                        break;
                    }
            }

            sp.InitialPort(comName, baudRate, dataBits, stopBits, parity);
            if (sp.OpenPort())
            {
                OpenComButton.Content = "关闭串口";
                OpenComButton.Click -= OpenComButton_Click;
                OpenComButton.Click += CloseComButton_Click;
                SendButton.IsEnabled = true;
                if (FileDirText.Text.Length > 0)
                    SendFileButton.IsEnabled = true;
            }
        }

        private void CloseComButton_Click(object sender, RoutedEventArgs e)
        {
            sp.ClosePort();
            OpenComButton.Content = "打开串口";
            OpenComButton.Click -= CloseComButton_Click;
            OpenComButton.Click += OpenComButton_Click;
            SendButton.IsEnabled = false;
            SendFileButton.IsEnabled = false;
        }

        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results 
            if (result == true)
            {
                // Open document 
                string filename = dlg.FileName;
                FileDirText.Text = filename;
                if (sp.isOpen)
                    SendFileButton.IsEnabled = true;
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if (SendHex)
            {
                if (SendTextBox.Text.Length % 3 == 1)
                {
                    MessageBox.Show("Hex发送时，发送框内容必须形如 00 01");
                    return;
                }
                sp.SendData(StringConverter.HexStringToByteArray(SendTextBox.Text));
            }
            else
            {
                string text = SendTextBox.Text;
                sp.SendData(text, SendNewLine);
            }
        }

        private void SendHexCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            SendTextBox.Text = StringConverter.StringToHexString(SendTextBox.Text);
        }



        private void SendHexCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            SendTextBox.Text = StringConverter.HexStringToString(SendTextBox.Text);
        }

        private void ShowHexCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ReceivedData.Text = StringConverter.StringToHexString(ReceivedData.Text);
            if (!ReceivedData.Text.Equals(""))
                ReceivedData.Text += " ";
        }

        private void ShowHexCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ReceivedData.Text = StringConverter.HexStringToString(ReceivedData.Text);
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ReceivedData.Text = "";
        }

        private void SendFileButton_Click(object sender, RoutedEventArgs e)
        {
            SendFile(FileDirText.Text, TimeSpan.FromMilliseconds((SendByLineInterval.Value ?? default(int))));
        }

        private async void SendFile(string filename, TimeSpan interval)
        {
            string[] content = await Task.Run(() => File.ReadAllLines(filename));
            byte[][] sendData = await Task.Run(() => StringConverter.HexStringsToByteArrays(content));
            for (int i = 0; i < sendData.Length; i++)
            {
                byte[] data = sendData[i];
                await Task.Run(() => sp.SendData(data));
                await Task.Delay(interval);
            }
            MessageBox.Show("文件发送完毕");
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            sp.ClosePort();
        }

        
    }
}
