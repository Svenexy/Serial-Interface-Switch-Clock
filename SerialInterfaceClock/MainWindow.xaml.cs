using System;
using System.Collections.Generic;
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
using System.Windows.Threading;
using System.IO.Ports;
using System.Diagnostics;

namespace SerialInterfaceClock
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {


        ComboBoxSerial comboboxserial;
        SerialPort serialPort;

        private byte btnstate = 0;


        public MainWindow()
        {
            InitializeComponent();

            comboboxserial = new ComboBoxSerial();
            serialPort = new SerialPort();

            comboboxserial.GetPortsAddToComboBox(CbSerialPort);


            startclock();


        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (btnstate == 0)
            {
                SerialPortDataSend("1");
                btnstate = 1;
                btn1.Background = Brushes.Green;
            }
            else
            {
                SerialPortDataSend("2");
                btnstate = 0;
                btn1.Background = Brushes.Red;
            }
        }

        private void Bt_Connect_Click(object sender, RoutedEventArgs e)
        {

            string comPoort = comboboxserial.GetSelectedCompoort(CbSerialPort.Text);

            if (!serialPort.IsOpen)
            {
                try
                {
                    serialPort.PortName = comPoort;
                    serialPort.BaudRate = Convert.ToInt32(Cb_Baudrate.Text);
                    serialPort.DataBits = 8;
                    serialPort.Parity = Parity.None;
                    serialPort.Handshake = Handshake.None;
                    serialPort.StopBits = StopBits.One;
                    serialPort.DataReceived += SerialPortDataRecieved;
                    serialPort.Open();
                    Debug.WriteLine($"Connected to {comPoort} on baudrate {Cb_Baudrate.Text}");
                    Bt_Connect.Content = "Disconnect";
                    Lb_Connecting.Content = "Connected";
                    Lb_Connecting.Foreground = Brushes.Green;
                }
                catch (Exception error)
                {
                    MessageBox.Show(error.Message, error.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                    PrintError("No Port selected");
                }
            }
            else
            {
                serialPort.Close();
                Debug.WriteLine($"Disconnected from {CbSerialPort.Text}");
                Bt_Connect.Content = "Connect";
                Lb_Connecting.Content = "Disconnected";
                Lb_Connecting.Foreground = Brushes.Red;
            }
            
        }

        private void SerialPortDataRecieved(object sender, SerialDataReceivedEventArgs e)
        {

            int dataLength = serialPort.BytesToRead;
            byte[] dataRecieved = new byte[dataLength];
            int nBytes = serialPort.Read(dataRecieved, 0, dataLength);
            if (nBytes == 0) return;

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                Debug.WriteLine($"{Encoding.Default.GetString(dataRecieved)}");
                Lb_Recieved.Items.Add($"{Encoding.Default.GetString(dataRecieved)}");
            }));



            //throw new NotImplementedException();
        }


        private void SerialPortDataSend(string data)
        {            
            try
            {
                serialPort.Write(data);

                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    Debug.WriteLine($"==> {data}");
                    Lb_Send.Items.Add($"==> {data}");
                }));

            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message, error.Source, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
        }


        private void PrintError (string error)
        {
            if(error == "")
            {
                lb_Error.Content = "";
            }
            else
            {
                lb_Error.Content = $"Error: {error}";
            }
            
        }


        private void startclock()
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += tickevent;
            timer.Start();

        }

        private void tickevent(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            Lb_Time.Content = DateTime.Now.ToString("HH:mm:ss");
            Lb_Date.Content = DateTime.Now.ToString("dd-MM-yyyy");
        }
    }


}
