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

    static class Constants
    {
        public const double Pi = 3.14159;
    }



    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {


        ComboBoxSerial comboboxserial;
        SerialPort serialPort;

        private byte btnstate = 0;

        private bool[] ledstates = { false, false, false, false };


        public MainWindow()
        {
            InitializeComponent();

            comboboxserial = new ComboBoxSerial();
            serialPort = new SerialPort();

            comboboxserial.GetPortsAddToComboBox(CbSerialPort);


            startclock();




        }

        // Test button
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (btnstate == 0)
            {
                btnstate = 1;
                btn1.Background = Brushes.Green;
                setLedStates();
            }
            else
            {
                btnstate = 0;
                btn1.Background = Brushes.Red;
                setLedStates();

            }
        }


        // Wanneer de Connect button is geklikt dan wordt er verbinding gemaakt met de geselecteerde poort
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


        // Als er data binnen komt wordt deze in de terminal geplaatst
        private void SerialPortDataRecieved(object sender, SerialDataReceivedEventArgs e)
        {

            //int dataLength = serialPort.BytesToRead;
            //byte[] dataRecieved = new byte[dataLength];
            //int nBytes = serialPort.Read(dataRecieved, 0, dataLength);
            //if (nBytes == 0) return;


            string message = serialPort.ReadLine();
            Debug.WriteLine($"{message}");

            string[] subs = message.Split(",");
            byte[] bytesubs = new byte[10];

            for (int i = 1; i < subs.Length; i++)
            {
                bytesubs[i] = Convert.ToByte(subs[i]);
            }

            if (bytesubs[0] == 0x6c)
            {

                ledstates[0] = BitConverter.ToBoolean(bytesubs, 1);
                ledstates[1] = BitConverter.ToBoolean(bytesubs, 2);
                ledstates[2] = BitConverter.ToBoolean(bytesubs, 3);
                ledstates[3] = BitConverter.ToBoolean(bytesubs, 4);
                setLedStates();
            }



            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                //Debug.WriteLine($"{Encoding.Default.GetString(dataRecieved)}");
                //Lb_Recieved.Items.Add($"{Encoding.Default.GetString(dataRecieved)}");

                
                Lb_Recieved.AppendText($"{DateTime.Now.ToString("HH:mm:ss:fff")} --> {message}\n");
                Lb_Recieved.ScrollToEnd();
            }));



            //throw new NotImplementedException();
        }


        // een command wordt naar het bordje gestuurd
        private void SerialPortDataSend(byte[] data)
        {

            string s = "";

            for (int i = 0; i < data.Length; i++)
            {
                s += data[i].ToString() + ",";
            }

            try
            {
                serialPort.Write(data, 0, data.Length);

                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    Debug.WriteLine($"==> {s}");
                    Lb_Send.AppendText($"{DateTime.Now.ToString("HH:mm:ss:fff")} --> {s}\n");
                    Lb_Send.ScrollToEnd();
                }));

            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message, error.Source, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
        }


        private void setLedStates()
        {
            if (ledstates[0])
                led_1.Fill = Brushes.Green;
            else
                led_1.Fill = Brushes.Red;
            if (ledstates[1])
                led_2.Fill = Brushes.Green;
            else
                led_2.Fill = Brushes.Red;
            if (ledstates[2])
                led_3.Fill = Brushes.Green;
            else
                led_3.Fill = Brushes.Red;
            if (ledstates[3])
                led_4.Fill = Brushes.Green;
            else
                led_4.Fill = Brushes.Red;
        }


        private void PrintError(string error)
        {
            if (error == "")
            {
                lb_Error.Content = "";
            }
            else
            {
                lb_Error.Content = $"Error: {error}";
            }

        }





        // dit is om de systeem tijd elke seconden updaten.
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

        private void Bt_Save_Switch_Click(object sender, RoutedEventArgs e)
        {
            int led = int.Parse(cb_Led_Switch.Text);
            string time = tb_Switch_Time.Text;
            byte on_off = (byte)((bool.Parse(cb_OnOff.Text)) ? 0x01 : 0x00);

            List<string> timeList = new List<string>(time.Split(':'));
            List<byte> sendata = new List<byte>();

            sendata.Add(on_off);

            foreach (string i in timeList)
            {
                sendata.Add(byte.Parse(i));
            }

            createPacket('h', led, sendata.ToArray());

        }


        private void Bt_Save_Interval_Click(object sender, RoutedEventArgs e)
        {
            int led = int.Parse(cb_Led_Interval.Text);
            int time1 = int.Parse(tb_on_time.Text);
            int time2 = int.Parse(tb_off_time.Text);

            List<byte> sendata = new List<byte>();

            sendata.Add((byte)(time1 & 0x00FF));
            sendata.Add((byte)((time1 & 0xFF00) >> 8));
            sendata.Add((byte)(time2 & 0x00FF));
            sendata.Add((byte)((time2 & 0xFF00) >> 8));

            createPacket('i', led, sendata.ToArray());


        }

        private void ra_Led1_Clicked(object sender, MouseButtonEventArgs e)
        {
            List<byte> sendata = new List<byte>();
            sendata.Add(Convert.ToByte(!ledstates[0]));
            setLedStates();
            createPacket('l', 1, sendata.ToArray());

        }

        private void ra_Led2_Clicked(object sender, MouseButtonEventArgs e)
        {
            List<byte> sendata = new List<byte>();
            sendata.Add(Convert.ToByte(!ledstates[1]));
            setLedStates();
            createPacket('l', 2, sendata.ToArray());
        }

        private void ra_Led3_Clicked(object sender, MouseButtonEventArgs e)
        {
            List<byte> sendata = new List<byte>();
            sendata.Add(Convert.ToByte(!ledstates[2]));
            setLedStates();
            createPacket('l', 3, sendata.ToArray());
        }

        private void ra_Led4_Clicked(object sender, MouseButtonEventArgs e)
        {
            List<byte> sendata = new List<byte>();
            sendata.Add(Convert.ToByte(!ledstates[3]));
            setLedStates();
            createPacket('l', 4, sendata.ToArray());
        }


        private void createPacket(char command, int led, byte[] data)
        {

            List<byte> sendata = new List<byte>();

            sendata.Add((byte)command);
            sendata.Add((byte)led);
            sendata.AddRange(data);

            SerialPortDataSend(sendata.ToArray());
        }


    }


}
