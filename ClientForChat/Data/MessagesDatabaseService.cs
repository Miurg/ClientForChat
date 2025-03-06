using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Net.Http;
using System.Threading.Tasks;
using ClientForChat.Models;
using ClientForChat.Services;
using Newtonsoft.Json;
using Windows.Services.Maps;

namespace ClientForChat.Data
{
    public class MessagesDatabaseService
    {
        private readonly string _connectionString = "Data Source=app.db;Version=3;";
        private readonly ApiService _apiService = new ApiService();
        public MessagesDatabaseService()
        {
            if (!System.IO.File.Exists("app.db"))
            {
                SQLiteConnection.CreateFile("app.db");
            }
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string createTableQuery = @"
                CREATE TABLE IF NOT EXISTS Messages (
                    Id INTEGER PRIMARY KEY,
                    Content TEXT NOT NULL,
                    UserID INTEGER NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    FOREIGN KEY (UserID) REFERENCES Users(Id)
                )";
                using (var command = new SQLiteCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
        public void SaveMessageAsync(MessageModel message)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string insertMessage = @"
                INSERT INTO Messages (Id, Content, UserID, CreatedAt)
                VALUES (@Id, @Content, @UserID, @CreatedAt)
                ON CONFLICT(Id) DO NOTHING;";

                using (var command = new SQLiteCommand(insertMessage, connection))
                {
                    command.Parameters.AddWithValue("@Id", message.Id);
                    command.Parameters.AddWithValue("@Content", message.Content);
                    command.Parameters.AddWithValue("@UserID", message.UserID);
                    command.Parameters.AddWithValue("@CreatedAt", message.CreatedAt.ToString("o"));
                    command.ExecuteNonQuery();
                }
            }
        }

        // Получение сообщений постранично
        public async Task<List<MessageModel>> GetMessagesAsync(int offset, int limit)
        {
            var messagesToFetch = await _apiService.FetchMessagesAsync(offset, limit);
            if (messagesToFetch != null)
            {
                SaveMessages(messagesToFetch);
            }
            var messages = new List<MessageModel>();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string selectMessages = @"
                SELECT Id, Content, UserID, CreatedAt 
                FROM Messages 
                ORDER BY CreatedAt DESC
                LIMIT @Limit OFFSET @Offset";

                using (var command = new SQLiteCommand(selectMessages, connection))
                {
                    command.Parameters.AddWithValue("@Limit", limit);
                    command.Parameters.AddWithValue("@Offset", offset);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            messages.Add(new MessageModel
                            {
                                Id = reader.GetInt32(0),
                                Content = reader.GetString(1),
                                UserID = reader.GetInt32(2),
                                CreatedAt = DateTime.Parse(reader.GetString(3))
                            });
                        }
                    }
                }
            }
            messages.Reverse();
            return messages;
        }

        public void SaveMessages(List<MessageModel> messages)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    foreach (var message in messages)
                    {
                        string insertMessage = @"
                        INSERT INTO Messages (Id, Content, UserID, CreatedAt)
                        VALUES (@Id, @Content, @UserID, @CreatedAt)
                        ON CONFLICT(Id) DO NOTHING;";

                        using (var command = new SQLiteCommand(insertMessage, connection))
                        {
                            command.Parameters.AddWithValue("@Id", message.Id);
                            command.Parameters.AddWithValue("@Content", message.Content);
                            command.Parameters.AddWithValue("@UserID", message.UserID);
                            command.Parameters.AddWithValue("@CreatedAt", message.CreatedAt.ToString("o"));
                            command.ExecuteNonQuery();
                        }
                    }
                    transaction.Commit();
                }
            }
        }
    }
}

