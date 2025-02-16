using ClientForChat.Models;
using ClientForChat.Services;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Services.Maps;

namespace ClientForChat.Data
{
    class UsersDatabaseService
    {
        private readonly string _connectionString = "Data Source=app.db;Version=3;";
        private readonly ApiService _apiService = new ApiService();

        public UsersDatabaseService()
        {
            // Создание базы данных, если она не существует
            if (!System.IO.File.Exists("app.db"))
            {
                SQLiteConnection.CreateFile("app.db");
            }

            // Создание таблицы для пользователей, если её нет
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string createTableQuery = @"CREATE TABLE IF NOT EXISTS Users (
                                            Id INTEGER PRIMARY KEY, 
                                            Username TEXT NOT NULL)";
                using (var command = new SQLiteCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public async Task<UserModel> GetOrFetchUser(int userId)
        {
            var user = GetUserById(userId);
            if (user == null)
            {
                user = await _apiService.FetchUserAsync(userId);
                if (user != null)
                {
                    AddUser(user);
                }
            }
            return user;
        }

        private UserModel GetUserById(int userId)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT Id, Username FROM Users WHERE Id = @UserId";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new UserModel
                            {
                                Id = reader.GetInt32(0),
                                Username = reader.GetString(1)
                            };
                        }
                    }
                }
            }
            return null;
        }

        // Добавление нового пользователя
        private void AddUser(UserModel user)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string query = "INSERT INTO Users (Id, Username) VALUES (@UserId, @Username)";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", user.Id);
                    command.Parameters.AddWithValue("@Username", user.Username);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
