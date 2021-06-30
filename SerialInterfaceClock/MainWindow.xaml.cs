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
        public const byte ASCII = 48;
    }



    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {


        ComboBoxSerial comboboxserial;
        SerialPort serialPort;

        private byte btnstate = 0;

        private byte[] ledstates = { 0, 0, 0, 0};


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
                if (serialPort.IsOpen)
                {
                    serialPort.WriteLine("l,0,1,1,0\n");
                }
                else
                {
                    PrintError("Port is closed!");
                }

            }
            else
            {
                btnstate = 0;
                btn1.Background = Brushes.Red;
                if (serialPort.IsOpen)
                {
                    serialPort.WriteLine("l,1,0,0,1\n");
                }
                else
                {
                    PrintError("Port is closed!");
                }

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

            
            string message = "";
            
            try
            {
                message = serialPort.ReadLine();
            }

            catch (Exception error)
            {
                MessageBox.Show(error.Message, error.Source, MessageBoxButton.OK, MessageBoxImage.Error);
            }


            //Debug.WriteLine($"<== {message}");
            if (message != "")
            {
                string[] subs = message.Split(",");
                byte[] bytesubs = new byte[10];

                string time = "";
                string date = "";

                if (subs.Length > 0)
                {
                    byte command = ASCIIEncoding.ASCII.GetBytes(subs[0])[0];

                    //for (int i = 1; i < subs.Length; i++)
                    //{
                    //    try
                    //    {
                    //        bytesubs[i-1] = Convert.ToByte(subs[i]);
                    //    }
                    //    catch (System.FormatException)
                    //    {
                    //        Debug.WriteLine($"FormatException with message: {message}");
                    //        MessageBox.Show("FormatException with message: "+message, "FormatException", MessageBoxButton.OK, MessageBoxImage.Warning);
                    //    }
                    //}

                    if (command == 'l')
                    {
                        byte leds = byte.Parse(subs[1]);

                        ledstates[0] = (byte)(leds & 0x01);
                        ledstates[1] = (byte)((leds & 0x02) >> 1);
                        ledstates[2] = (byte)((leds & 0x04) >> 2);
                        ledstates[3] = (byte)((leds & 0x08) >> 3);
                        setLedStates();
                    }

                    if (command == 't')
                    {
                        int ec_time = Int32.Parse(subs[1]);

                        int mili = ec_time & 0x3F;
                        int sec = (ec_time & 0x3F000) >> 12;
                        int min = (ec_time & 0xFC0000) >> 18;
                        int hour = ec_time >> 24;

                        time = String.Format("{0:00}:{1:00}:{2:00}", hour, min, sec);

                    }

                    if (command == 'd')
                    {
                        int ec_date = Int32.Parse(subs[1]);

                        int day = ec_date & 0x3F;
                        int month = (ec_date & 0x3C0) >> 6;
                        int year = (ec_date & 0x3FC00) >> 10;

                        date = String.Format("{0:00}-{1:00}-20{2:00}", day, month, year);

                    }
                }

                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    //Debug.WriteLine($"{Encoding.Default.GetString(dataRecieved)}");
                    //Lb_Recieved.Items.Add($"{Encoding.Default.GetString(dataRecieved)}");

                    if ((time != "") && (Lb_EC_Time.Content != time))
                    {
                        Lb_EC_Time.Content = time;
                    }

                    if ((date != "") && (Lb_EC_Date.Content != date))
                    {
                        Lb_EC_Date.Content = date;
                    }


                    Lb_Recieved.AppendText($"{DateTime.Now.ToString("HH:mm:ss:fff")} <== {message}\n");

                    while (Lb_Recieved.LineCount > 450)
                    {
                        Lb_Recieved.Text = Lb_Recieved.Text.Remove(0, Lb_Recieved.GetLineLength(0));
                    }

                    Lb_Recieved.ScrollToEnd();
                }));
            }
            



            //throw new NotImplementedException();
        }


        // een command wordt naar het bordje gestuurd
        private void SerialPortDataSend(byte[] buffer)
        {
            string s = "";
            
            try
            {
                serialPort.Write(buffer, 0, buffer.Length);

                foreach (var item in buffer)
                {
                    s += (char)item;
                }

                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    Lb_Send.AppendText($"{DateTime.Now.ToString("HH:mm:ss:fff")} ==> {s}\n");
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

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                if (ledstates[0] == 1)
                    led_1.Fill = Brushes.Green;
                else
                    led_1.Fill = Brushes.Red;
                if (ledstates[1] == 1)
                    led_2.Fill = Brushes.Green;
                else
                    led_2.Fill = Brushes.Red;
                if (ledstates[2] == 1)
                    led_3.Fill = Brushes.Green;
                else
                    led_3.Fill = Brushes.Red;
                if (ledstates[3] == 1)
                    led_4.Fill = Brushes.Green;
                else
                    led_4.Fill = Brushes.Red;

            }));

        }


        private void PrintError(string error)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                if (error == "")
                {
                    lb_Error.Content = "";
                }
                else
                {
                    lb_Error.Content = $"Error: {error}";
                }
            }));
        }





        // dit is om de systeem tijd elke seconden updaten.
        private void startclock()
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
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
            byte led = (byte)(byte.Parse(cb_Led_Switch.Text)+48);
            string[] time = tb_Switch_Time.Text.Split(":");
            byte on_off = (byte)((bool.Parse(cb_OnOff.Text)) ? '1' : '0');

            byte[] hour = Encoding.ASCII.GetBytes(time[0]);
            byte[] minute = Encoding.ASCII.GetBytes(time[1]);
            byte[] second = Encoding.ASCII.GetBytes(time[2]);
            byte[] mili = Encoding.ASCII.GetBytes(time[3]);

            byte[] buffer =
                {
                    (byte)'h',
                    led,
                    on_off,
                    hour[0],
                    hour[1],
                    minute[0],
                    minute[1],
                    second[0],
                    second[1],
                    mili[0],
                    mili[1],
                    mili[2]
                };

            SerialPortDataSend(buffer);



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

            //createPacket('i', led, sendata.ToArray());


        }

        private void ra_Led1_Clicked(object sender, MouseButtonEventArgs e)
        {
            byte on_off;
            if (ledstates[0] == 1) on_off = 0x30;
            else on_off = 0x31;
            setLedStates();
            byte[] buffer =
                {
                    0x6C,
                    0x30,
                    on_off
                };

            SerialPortDataSend(buffer);

        }

        private void ra_Led2_Clicked(object sender, MouseButtonEventArgs e)
        {
            byte on_off;
            if (ledstates[1] == 1) on_off = 0x30;
            else on_off = 0x31;
            setLedStates();
            byte[] buffer =
                {
                    0x6C,
                    0x31,
                    on_off
                };

            SerialPortDataSend(buffer);
        }

        private void ra_Led3_Clicked(object sender, MouseButtonEventArgs e)
        {
            byte on_off;
            if (ledstates[2] == 1)  on_off = 0x30;
            else                    on_off = 0x31;

            setLedStates();
            byte[] buffer =
                {
                    0x6C,
                    0x32,
                    on_off
                };

            SerialPortDataSend(buffer);
        }

        private void ra_Led4_Clicked(object sender, MouseButtonEventArgs e)
        {
            byte on_off;
            if (ledstates[3] == 1) on_off = 0x30;
            else on_off = 0x31;
            setLedStates();
            byte[] buffer =
                {
                    0x6C,
                    0x33,
                    on_off
                };

            SerialPortDataSend(buffer);
        }

        private void Bt_Save_Time_Click(object sender, RoutedEventArgs e)
        {


            string[] time = tb_time.Text.Split(":");

            Debug.WriteLine(time[0][0]);

            byte[] hour = Encoding.ASCII.GetBytes(time[0]);
            byte[] minute = Encoding.ASCII.GetBytes(time[1]);
            byte[] second = Encoding.ASCII.GetBytes(time[2]);

            byte[] buffer =
                {
                    (byte)'t',
                    hour[0],
                    hour[1],
                    minute[0],
                    minute[1],
                    second[0],
                    second[1]
                };
            SerialPortDataSend(buffer);
        }

        private void Bt_Save_Date_Click(object sender, RoutedEventArgs e)
        {
            string[] date = tb_date.Text.Split("-");


            byte[] day = Encoding.ASCII.GetBytes(date[0]);
            byte[] month = Encoding.ASCII.GetBytes(date[1]);
            byte[] year = Encoding.ASCII.GetBytes(date[2]);

            byte[] buffer =
                {
                    (byte)'d',
                    day[0],
                    day[1],
                    month[0],
                    month[1],
                    year[0],
                    year[1]
                };
            SerialPortDataSend(buffer);
        }

        private void Bt_Set_Current_Time_Date_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Bt_Get_Time_Date_Click(object sender, RoutedEventArgs e)
        {
            byte[] buffer =
                {
                    (byte)'c'
                };
            SerialPortDataSend(buffer);
        }
    }


}
