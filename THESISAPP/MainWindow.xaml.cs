using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Media;
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
        bool setAlarm = true;
        SoundPlayer soundPlay;
        bool soundPlaying = false;
        public MainWindow()

        {
            InitializeComponent();

            this.comboPortName.ItemsSource = getPorts();
            receivedStr = new ObservableCollection<transmission>();

            this.gridTransmit.ItemsSource = receivedStr;
            this.labelWarnHeader.Visibility = Visibility.Hidden;
            this.warningData.Visibility = Visibility.Hidden;
            arduinoDevice = new SerialPort();
            transmitTable = new DataTable();
            soundPlay = new SoundPlayer("appAlarm.wav");

        }

        //FUNCTION TO GET THE COM PORTS CURRENTLY CONNECTED
        public static List<string> getPorts()
        {
            List<string> portsConnected = new List<string>();

            string[] pNames = SerialPort.GetPortNames();

            foreach (string str in pNames)
            {
                portsConnected.Add(str);
            }

            return portsConnected;
        }
        //FUNCTION TO REFRESH THE COM PORTS CONNECTED
        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            this.comboPortName.ItemsSource = getPorts();
            MessageBox.Show("The Port have been refreshed.","Refreshed",MessageBoxButton.OK,MessageBoxImage.Information);
        }

        //FUNCTION TO START THE RECEIVING OF PACKETS
        private void ButtonStartReceive_Click(object sender, RoutedEventArgs e)
        {
            if(String.IsNullOrWhiteSpace(this.comboPortName.Text) != true)
            {
                arduinoDevice = new SerialPort();
                arduinoDevice.BaudRate = 9600;
                arduinoDevice.PortName = this.comboPortName.SelectedValue.ToString();
                arduinoDevice.DataReceived += listentoSerial;

                arduinoDevice.Open();
                if (arduinoDevice.IsOpen)
                {
                    MessageBox.Show(arduinoDevice.PortName + "is open!", "Serial Port Opened", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.buttonStartReceive.IsEnabled = false;
                    this.comboPortName.IsEnabled = false;
                }
                else
                {
                    MessageBox.Show(arduinoDevice.PortName + " Failed to open!", "Failed to Open", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select the correct port name!","Invalid Port Name",MessageBoxButton.OK,MessageBoxImage.Exclamation);
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
                
                //THIS CODE RUNS IN A DIFFERENT THREAD
                if ((latesttrans.content.IndexOf('*') > 0) &&(latesttrans.content.Length > 50))
                {
                    receivedStr.Add(latesttrans);
                    
                    filteredString = latesttrans.content.Substring(latesttrans.content.IndexOf('*') + 1, latesttrans.content.LastIndexOf('*') - latesttrans.content.IndexOf('*') - 1);

                    //PACKETNUMBER;DATE;TIME;SOILMOVEMENT;MOISTURE1,MOISTURE2,MOISTURE3;RAIN;
                    this.textboxRainfall.Text = filteredString;
                    dataArray = filteredString.Split(';');
                    moistureArray = dataArray[4].Split(',');

                    this.textboxDate.Text = dataArray[1];
                    string[] strarr = dataArray[1].Split('/');
                                        //MONTH,DAY,YEAR
                    int[] dateInt = { int.Parse(strarr[0]), int.Parse(strarr[1]), int.Parse(strarr[2]) };
                    
                    this.textboxTime.Text = dataArray[2];
                    strarr = dataArray[2].Split(':');//h,m,s
                    int[] timeInt = { int.Parse(strarr[0]), int.Parse(strarr[1]), int.Parse(strarr[2]) };
                    this.textboxMovement.Text = dataArray[3];
                    this.textboxS1.Text = moistureArray[0];
                    this.textboxS2.Text = moistureArray[1];
                    this.textboxS3.Text = moistureArray[2];
                    this.textboxRainfall.Text = dataArray[5];

                    //SEND TO DB HERE
                    SenderData senderData = new SenderData();

                    //y,m,d,h,m,s
                    senderData.sentDate = new DateTime(dateInt[2], dateInt[0], dateInt[1], timeInt[0], timeInt[1], timeInt[2]);

                    senderData.packetNumber = Convert.ToInt16(dataArray[0]);
                    senderData.extensometer = Convert.ToInt16(dataArray[3]);
                    senderData.sensor1 = Convert.ToDouble(moistureArray[0]);
                    senderData.sensor2 = Convert.ToDouble(moistureArray[1]);
                    senderData.sensor3 = Convert.ToDouble(moistureArray[2]);
                    senderData.rainsensor = Convert.ToDouble(dataArray[5]);
                    try
                    {
                        Database.EnterData(senderData, latesttrans.timeReceived);
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show(ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                   

                    //CREATE DATA ANALYSIS HERE IF TRUE SET THE ALARM 
                    //SOIL MOISTURE 
                    double rainfallRate = getRainRate(senderData.sentDate);
                    this.textboxRainRate.Text = rainfallRate.ToString();
                    if ((senderData.sensor1 >= 50) || (senderData.sensor3 >= 50) || (senderData.sensor3 >= 50))
                    {
                        //MAKETHE TEXT INSIDE THE GROUPBOX TO VALUE 
                        this.labelWarnHeader.Visibility = Visibility.Visible;
                        this.warningData.Visibility = Visibility.Visible;
                        this.warningData.Text = String.Format("Moisture Levels: S1:{0} S2:{1} S3:{2}", senderData.sensor1.ToString(), senderData.sensor2.ToString(), senderData.sensor3.ToString());
                        playAlarm();
                    }//RAINFALL 
                    else if (rainfallRate > 7.5)
                    {
                        this.labelWarnHeader.Visibility = Visibility.Visible;
                        this.warningData.Visibility = Visibility.Visible;
                        this.warningData.Text = String.Format("Rainfall Rate: {0}", senderData.rainsensor.ToString());
                        playAlarm();
                    }//soilmovement
                    else if (senderData.extensometer >=10)
                    {
                        this.labelWarnHeader.Visibility = Visibility.Visible;
                        this.warningData.Visibility = Visibility.Visible;
                        this.warningData.Text = String.Format("Soil Movement: {0}", senderData.extensometer.ToString());
                        playAlarm();
                    }
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

        private double getRainRate(DateTime timeSearch)
        {
            double rainRate=0;
            DataTable rainTable = Database.getRainHour(timeSearch); 

            foreach(DataRow rainRow in rainTable.Rows)
            {
                rainRate += (double)rainRow["Rainfall"];
            }
            return rainRate;
        }

        private void playAlarm()
        {
            if(this.setAlarm == true && this.soundPlaying == false)
            {
                this.soundPlay.PlayLooping();
                this.soundPlaying = true;
            }
            
        }
        //FUNCTION FOR SETTING THE SILENT ALARM OPTION
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(this.setAlarm == false)
            {//PLAY SOUND IF TRUE
                this.setAlarm = true;
                buttonSilent.Content = "🔊";
                buttonSilent.FontSize = 24;
            }
            else
            {
                this.setAlarm = false;
                buttonSilent.Content = "🔇";
                buttonSilent.FontSize = 32;
            }
        }

        private void ButtonStopAlarm_Click(object sender, RoutedEventArgs e)
        {
            if(this.soundPlaying == true)
            {
                this.soundPlay.Stop();
                this.soundPlaying = false;
                
            }
            else
            {
                MessageBox.Show("The alarm is not playing!","Alarm",MessageBoxButton.OK,MessageBoxImage.Information);
            }
        }

        private void ButtonShowData_Click(object sender, RoutedEventArgs e)
        {
            DataWindow dataWin = new DataWindow(this);
            this.Hide();
            dataWin.Show();
        }
    }
    //CLASS USED FOR TRANSMISSION OF DATA 
    public class transmission
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

    public class SenderData
    {
        DateTime dateSent;
        //DateTime timeSent;

        int packetNum;
        int soilMove;

        double s1; double s2; double s3;
        double rain;

        public DateTime sentDate { get { return dateSent; } set { dateSent = value; } }
        //public DateTime sentTime { get { return timeSent; } set { timeSent = value; } }
        public int packetNumber { get { return packetNum; } set { packetNum = value; } }
        public int extensometer { get { return soilMove; } set { soilMove = value; } }
        public double sensor1 { get { return this.s1; } set { s1 = value; } }
        public double sensor2 { get { return this.s2; } set { s2 = value; } }
        public double sensor3 { get { return s3; } set { s3 = value; } }
        public double rainsensor { get { return rain; } set { rain = value; } }
    }


}
