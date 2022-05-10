using MySqlConnector;
using System;
using System.Data;
using System.Data.Common;

namespace WeiZed.MySQL
{

    class MySQLStatic
    {
        private static MySqlConnection connection;
        // Статик конструктор вызывается один раз за все время, когда как-либо используешь определенный класс.
        // Он может использоваться для инициализации статических переменных или какой-либо предварительной настройки для класса в целом
        static MySQLStatic()
        {
            string connectionString = "SERVER=127.0.0.1;" + "DATABASE=myroleplaydb;" + "UID=root;" + "PASSWORD=;";
            connection = new MySqlConnection(connectionString);
        }


        public static void Query(string query)
        {
            using (MySqlCommand sqlCommand = new MySqlCommand(query, connection))
            {
                connection.Open();
                sqlCommand.ExecuteNonQuery();
                connection.Close();
            }
        }

        public static string QueryRead(string query)
        {
            using (MySqlCommand sqlCommand = new MySqlCommand(query, connection))
            {
                connection.Open();
                string output = Convert.ToString(sqlCommand.ExecuteScalar());
                connection.Close();
                return output;
            }
        }

        // Read whole table
        public static DataTable QueryReadTable(string query)
        {
            using (MySqlCommand sqlCommand = new MySqlCommand(query, connection))
            {
                connection.Open();
                DbDataReader reader = sqlCommand.ExecuteReader();
                DataTable result = new DataTable();
                result.Load(reader);
                connection.Close();
                return result;
            }
        }
    }
}
