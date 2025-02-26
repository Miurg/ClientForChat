using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientForChat.Data
{
    public class SelfUserDatabaseService
    {
        private readonly string _connectionString = "Data Source=app.db;Version=3;";

        public SelfUserDatabaseService()
        {
            if (!System.IO.File.Exists("app.db"))
            {
                SQLiteConnection.CreateFile("app.db");
            }
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string createTableQuery = @"CREATE TABLE IF NOT EXISTS SelfUser (
                                            Id INTEGER PRIMARY KEY AUTOINCREMENT, 
                                            Username TEXT NOT NULL)";
                using (var command = new SQLiteCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public void SaveSelfUser(string username)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string insertQuery = "INSERT INTO SelfUser (Username) VALUES (@Username)";
                using (var command = new SQLiteCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    command.ExecuteNonQuery();
                }
            }
        }

        public string GetLastSelfUser()
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string selectQuery = "SELECT Username FROM SelfUser ORDER BY Id DESC LIMIT 1";
                using (var command = new SQLiteCommand(selectQuery, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return reader.GetString(0);
                        }
                    }
                }
            }
            return null;
        }
    }
}
