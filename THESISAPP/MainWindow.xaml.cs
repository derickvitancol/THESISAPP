using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO.Ports;


namespace THESISAPP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool readStop;
        ObservableCollection<string> receivedStr;
        SerialPort arduinoDevice;
        public MainWindow()
        {
            InitializeComponent();

            this.comboPortName.ItemsSource = getPorts();
            readStop = false;
            receivedStr = new ObservableCollection<string>();
            this.lbox.ItemsSource = receivedStr;
            arduinoDevice = new SerialPort();

        }

        //FUNCTION TO GET THE COM PORTS CURRENTLY CONNECTED
        public static List<string> getPorts()
        {
            List<string> portsConnected = new List<string>();

            string[] pNames = SerialPort.GetPortNames();

            foreach(string str in pNames )
            {
                portsConnected.Add(str);
            }

            return portsConnected;
        }
        //FUNCTION TO REFRESH THE COM PORTS CONNECTED
        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            this.comboPortName.ItemsSource = getPorts();
        }

        //FUNCTION TO START THE 
        private void ButtonStartReceive_Click(object sender, RoutedEventArgs e)
        {

            arduinoDevice = new SerialPort();
            arduinoDevice.BaudRate = 9600;
            arduinoDevice.PortName = this.comboPortName.SelectedValue.ToString();
            arduinoDevice.DataReceived += listentoSerial;



             arduinoDevice.Open();
            MessageBox.Show(arduinoDevice.PortName + "is open!","Serial Port Opened",MessageBoxButton.OK);
            this.buttonStartReceive.IsEnabled = false;
            this.comboPortName.IsEnabled = false;
        }

        private void listentoSerial(object sender, SerialDataReceivedEventArgs e)
        {
            var serialP = sender as SerialPort;
            

            Application.Current.Dispatcher.Invoke(new Action(() => {
                string str = serialP.ReadLine();
                this.textboxReceived.Text = str;

                receivedStr.Add(str);
            }));

        }

        private void ComboPortName_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void ButtonStopReceive_Click(object sender, RoutedEventArgs e)
        {
            if (arduinoDevice.IsOpen)
            {
                arduinoDevice.DiscardInBuffer();
                arduinoDevice.DiscardOutBuffer();

                arduinoDevice.Close();
                GC.Collect();
                MessageBox.Show("The port is closed!", "Port closed", MessageBoxButton.OK, MessageBoxImage.Information);
                this.buttonStartReceive.IsEnabled = true;
                this.comboPortName.IsEnabled = true;
            }
            else
            {
                MessageBox.Show("The port is closed!", "Port closed", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }



    
}
