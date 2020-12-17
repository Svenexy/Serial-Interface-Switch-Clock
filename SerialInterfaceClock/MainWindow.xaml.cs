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


        public MainWindow()
        {
            InitializeComponent();

            comboboxserial = new ComboBoxSerial();
            serialPort = new SerialPort();
            
            comboboxserial.GetPortsAddToComboBox(CbSerialPort);
            
            
            startclock();


        }

        private void Bt_Connect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                serialPort.PortName = CbSerialPort.Text;
                serialPort.BaudRate = Convert.ToInt32(Cb_Baudrate.Text);
                serialPort.DataBits = 8;
                serialPort.Parity = Parity.None;
                serialPort.Handshake = Handshake.None;
                serialPort.StopBits = StopBits.One;
                serialPort.DataReceived += SerialPortDataRecieved;
                serialPort.Open();
                Debug.WriteLine($"Connected to {CbSerialPort.Text} on baudrate {Cb_Baudrate.Text}");
            }

            catch (Exception error)
            {
                MessageBox.Show(error.Message, error.Source  ,MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SerialPortDataRecieved(object sender, SerialDataReceivedEventArgs e)
        {

            int dataLength = serialPort.BytesToRead;
            byte[] dataRecieved = new byte[dataLength];
            Debug.WriteLine(dataRecieved);
            int nBytes = serialPort.Read(dataRecieved, 0, dataLength);
            if (nBytes == 0) return;

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                Lb_Recieved.Items.Add($"{Encoding.Default.GetString(dataRecieved)} {Environment.NewLine}");
            }));

            
            
            //throw new NotImplementedException();
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
