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
using System.Windows.Shapes;
using System.Data;

namespace THESISAPP
{
    /// <summary>
    /// Interaction logic for DataWindow.xaml
    /// </summary>
    public partial class DataWindow : Window
    {
        MainWindow previousWin;
        private DataTable sensorData;
        public DataWindow()
        {
            InitializeComponent();
            sensorData = new DataTable();
            sensorData = fixData(Database.LoadData());
            this.datagridSensorData.ItemsSource = sensorData.DefaultView;
        }

        public DataWindow(MainWindow mainwin)
        {

            InitializeComponent();
            sensorData = new DataTable();
            sensorData = fixData(Database.LoadData());
            this.datagridSensorData.ItemsSource = sensorData.DefaultView;
            previousWin = mainwin;

        }

        private void ButtonReturn_Click(object sender, RoutedEventArgs e)
        {
            
            MessageBoxResult messRes = MessageBox.Show("Are you sure you want to return to the main window? ", "Go to Main Window", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (messRes == MessageBoxResult.Yes)
            {
                this.previousWin.Show();
                this.Close();
            }
        }

        private DataTable fixData(DataTable inputTable)
        {
            //CREATE A NEW DATATABLE
            DataTable datTable = new DataTable();
            //date sent time sent time recevied PACKETNUMBER;EXTENSOMETER;MOISTURE;RAINFALL
            datTable.Columns.Add("Date Sent",typeof(string));
            datTable.Columns.Add("Time Sent", typeof(string));
            datTable.Columns.Add("Time Received", typeof(string));
            datTable.Columns.Add("Packet Number", typeof(int));
            datTable.Columns.Add("Extensometer", typeof(int));
            datTable.Columns.Add("Moisture Sensor 1", typeof(double));
            datTable.Columns.Add("Moisture Sensor 2", typeof(double));
            datTable.Columns.Add("Moisture Sensor 3", typeof(double));
            datTable.Columns.Add("Amount of Rainfall", typeof(double));
            foreach (DataRow inputRow in inputTable.Rows )
            {
                datTable.Rows.Add(inputRow[0],String.Format("{0}:{1}:{2}",inputRow[1], inputRow[2], inputRow[3]),inputRow[4],
                    inputRow[5],inputRow[6],inputRow[7],inputRow[8],inputRow[9],inputRow[10]);
            }


            return datTable;
        }

        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            sensorData = new DataTable();
            sensorData = fixData(Database.LoadData());
            this.datagridSensorData.ItemsSource = sensorData.DefaultView;
            MessageBox.Show("The grid is refreshed.","Refreshed",MessageBoxButton.OK,MessageBoxImage.Information);
        }

        private void ButtonClearData_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messRes = MessageBox.Show("Are you sure to clear all the data from the database? ","Clear Data",MessageBoxButton.YesNo,MessageBoxImage.Question);
            if(messRes == MessageBoxResult.Yes)
            {
                Database.RemoveData();
                sensorData = new DataTable();
                sensorData = fixData(Database.LoadData());
                this.datagridSensorData.ItemsSource = sensorData.DefaultView;
                MessageBox.Show("The database has been cleared", "Database Cleared", MessageBoxButton.OK, MessageBoxImage.Information);
            }
           
        }
    }
}
