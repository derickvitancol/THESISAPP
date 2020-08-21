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
using System.Data;


namespace THESISAPP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// //CREATE FUNCTION FOR DATABASE COMMUNICATIONS
    /// CREATE FUNCTION FOR DATA ANALYSIS
    /// 7.5mm/hr danger
    /// soil moist 50?
    /// extenso movement > 15cm
    /// </summary>
    public partial class MainWindow : Window
    {
        
        private ObservableCollection<transmission> receivedStr; 
        private string filteredString;
        SerialPort arduinoDevice;
        private DataTable transmitTable;
        public MainWindow()
        {
            InitializeComponent();

            this.comboPortName.ItemsSource = getPorts();
            receivedStr = new ObservableCollection<transmission>();

            this.gridTransmit.ItemsSource = receivedStr;
            arduinoDevice = new SerialPort();
            transmitTable = new DataTable();

        }

        class transmission
        {
            DateTime receiveTime;
            string receiveContent;

            public DateTime timeReceived
            {
                get { return receiveTime; }
                set { receiveTime = value; }
            }

            public string content
            {
                get { return receiveContent; }
                set { receiveContent = value; }
            }
            
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

        //FUNCTION TO START THE RECEIVING OF PACKETS
        private void ButtonStartReceive_Click(object sender, RoutedEventArgs e)
        {

            arduinoDevice = new SerialPort();
            arduinoDevice.BaudRate = 9600;
            arduinoDevice.PortName = this.comboPortName.SelectedValue.ToString();
            arduinoDevice.DataReceived += listentoSerial;


            
            arduinoDevice.Open();
            if (arduinoDevice.IsOpen)
            {
                MessageBox.Show(arduinoDevice.PortName + "is open!", "Serial Port Opened", MessageBoxButton.OK,MessageBoxImage.Information);
                this.buttonStartReceive.IsEnabled = false;
                this.comboPortName.IsEnabled = false;
            }
            else
            {
                MessageBox.Show(arduinoDevice.PortName + " Failed to open!", "Failed to open", MessageBoxButton.OK,MessageBoxImage.Error);
            }
                
            
        }
        //FUNCTION THAT LISTENS TO THE SERIAL PORT
        private void listentoSerial(object sender, SerialDataReceivedEventArgs e)
        {
            var serialP = sender as SerialPort;
            string[] dataArray;
            string[] moistureArray;
            transmission latesttrans = new transmission();
                
            Application.Current.Dispatcher.Invoke(new Action(() => {
                latesttrans.content = serialP.ReadLine();
                latesttrans.timeReceived = DateTime.Now;
                //this.textboxReceived.Text = latestString;

                
                if(latesttrans.content.Length > 30)
                {
                    receivedStr.Add(latesttrans);
                    filteredString = latesttrans.content.Substring(latesttrans.content.IndexOf('*')+1, latesttrans.content.LastIndexOf('*') - latesttrans.content.IndexOf('*')-1);
                    
                    //PACKETNUMBER;DATE;TIME;SOILMOVEMENT;MOISTURE1,MOISTURE2,MOISTURE3;RAIN;
                    this.textboxRainfall.Text = filteredString;
                    dataArray = filteredString.Split(';');
                    moistureArray = dataArray[4].Split(',');

                    this.textboxDate.Text = dataArray[1];
                    this.textboxTime.Text = dataArray[2];
                    this.textboxMovement.Text = dataArray[3];
                    this.textboxS1.Text = moistureArray[0];
                    this.textboxS2.Text = moistureArray[1];
                    this.textboxS3.Text = moistureArray[2];
                    this.textboxRainfall.Text=dataArray[5];
                }
                
                

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
