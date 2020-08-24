using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Data;
using System.Windows;

namespace THESISAPP
{
    //CREATE DATABASE
    //DATA ON TRANSMISSION: PACKETNUMBER;DATE;TIME;EXTENSOMETER;MOISTURE;RAINFALL
    //PUT TIME RECEIVED IN THE DATABASE
    //function for checking the mm/hr
    public static class Database
    {
        static string relativepath = @"LEWSDB.db";
        static string currentpath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
        static string absolutepath = System.IO.Path.Combine(currentpath, relativepath).Remove(0, 6);
        static string connectionstring = String.Format("Data Source={0}", absolutepath);

        //FUNCTION TO LOAD ALL THE SAVED DATA TO THE DATABASE
        public static DataTable LoadData()
        {
            DataTable savedData = new DataTable();
            string query = "SELECT DateSent as 'Date Sent',HourSent as 'Hour Sent',MinuteSent as 'Minute Sent',SecondSent as 'Second Sent',TimeReceived as 'Time Received',PacketNumber as 'Packet Number',Movement as 'Soil Movement'," +
                "Moisture1 as 'Soil Moisture Sensor 1',Moisture2 as 'Soil Moisture Sensor 2'," +
                "Moisture3 as 'Soil Moisture Sensor 3',Rainfall as 'Amount of Rainfall' FROM DataTransmission";

            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(connectionstring))
                {
                    conn.Open();

                    SQLiteCommand command = new SQLiteCommand(query, conn);
                    SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);

                    adapter.Fill(savedData);
                    conn.Close();
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message,"Error",MessageBoxButton.OK,MessageBoxImage.Error);
            }
            return savedData;
        }

        //FUNCTION TO REMOVE ALL THE SAVED DATA FROM THE DATABASE
        public static void RemoveData()
        {
            string query = "DELETE FROM DataTransmission";
            using (SQLiteConnection conn = new SQLiteConnection(connectionstring))
            {
                conn.Open();
                SQLiteCommand command = new SQLiteCommand(query, conn);                

                command.ExecuteNonQuery();
                conn.Close();
            }

        }
        //FUNCTION TO WRITE A ROW IN THE DATABASE
        public static void EnterData(SenderData newData,DateTime receiveTime)
        {
            string query = "INSERT INTO DataTransmission(DateSent,HourSent,MinuteSent,SecondSent," +
                "TimeReceived,PacketNumber,Movement,Moisture1,Moisture2," +
                "Moisture3,Rainfall) VALUES(@date,@hr,@min,@sec,@tReceived,@pNum,@movement,@m1,@m2,@m3,@rain)";

            using (SQLiteConnection conn = new SQLiteConnection(connectionstring))
            {
                conn.Open();
                SQLiteCommand command = new SQLiteCommand(query, conn);
                
                //mdy
                command.Parameters.AddWithValue("@date", newData.sentDate.Date.ToString("MM/dd/yyyy"));
                command.Parameters.AddWithValue("@tReceived", receiveTime.TimeOfDay);
                command.Parameters.AddWithValue("@hr",newData.sentDate.Hour);
                command.Parameters.AddWithValue("@min", newData.sentDate.Minute);
                command.Parameters.AddWithValue("@sec", newData.sentDate.Second);
                command.Parameters.AddWithValue("@pNum", newData.packetNumber);
                command.Parameters.AddWithValue("@movement", newData.extensometer);
                command.Parameters.AddWithValue("@m1", newData.sensor1);
                command.Parameters.AddWithValue("@m2", newData.sensor2);
                command.Parameters.AddWithValue("@m3", newData.sensor3);
                command.Parameters.AddWithValue("@rain", newData.rainsensor);

                command.ExecuteNonQuery();
                conn.Close();
            }
        }

        //FUNCTION TO GET ALL THE RAIN THE 1 HOUR RETURNS A DATATABLE
        public static DataTable getRainHour(DateTime timeSearch)
        {
            DataTable hourlyRain = new DataTable();
            string query = "SELECT DateSent,HourSent,MinuteSent,SecondSent,Rainfall " +
                "FROM DataTransmission WHERE DateSent=@dateNow AND HourSent=@hourNow";

            using (SQLiteConnection conn = new SQLiteConnection(connectionstring))
            {
                conn.Open();

                SQLiteCommand command = new SQLiteCommand(query, conn);
                SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
                command.Parameters.AddWithValue("@dateNow", timeSearch.Date.ToString("MM/dd/yyyy"));
                command.Parameters.AddWithValue("@hourNow", timeSearch.Hour);
                adapter.Fill(hourlyRain);
                conn.Close();
            }

            return hourlyRain;
        }

    }
}
