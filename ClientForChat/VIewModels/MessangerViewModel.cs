using ClientForChat.Models;
using ClientForChat.Services;
using ClientForChat.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.AspNetCore.SignalR.Client;
using Windows.System;
using ClientForChat.Data;
using System.Net.Http;

namespace ClientForChat.VIewModels
{
    public class MessangerViewModel : INotifyPropertyChanged
    {
        private readonly SelfUserDatabaseService _selfUserDatabaseService;
        private readonly MessagesService _messagesService;
        private readonly HubConnection _hubConnection;
        public ObservableCollection<MessageLocalModel> Messages { get; } = new();

        private string _newMessage;
        public string NewMessage
        {
            get => _newMessage;
            set
            {
                _newMessage = value;
                OnPropertyChanged(nameof(NewMessage));
            }
        }

        public ICommand SendMessageCommand { get; }

        public MessangerViewModel(MessagesService messagesService, SelfUserDatabaseService selfUserDatabaseService)
        {
            _messagesService = messagesService;
            _selfUserDatabaseService = selfUserDatabaseService;
            SendMessageCommand = new RelayCommand(async () => await SendMessage(), () => !string.IsNullOrWhiteSpace(NewMessage));
            var handler = new HttpClientHandler(); 
            handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

            _hubConnection = new HubConnectionBuilder()
                .WithUrl("https://localhost:7168/messageHub", options => {
                    options.HttpMessageHandlerFactory = _ => handler;
                }).Build();
            _hubConnection.On<MessageModel>("ReceiveMessage", (message) =>
            {
                App.Current.Dispatcher.Invoke(async () =>
                {
                    MessageLocalModel localMessage = await _messagesService.MessageToLocalMessageAsync(message);
                    Messages.Add(localMessage);
                });
            });
            _ = ConnectToHub();
        }
        private async Task ConnectToHub()
        {
            try
            {
                await _hubConnection.StartAsync();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"❌ Ошибка подключения: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка подключения к SignalR: {ex.Message}");
            }

        }

        private async Task SendMessage()
        {
            MessageLocalModel message = new MessageLocalModel {Content = NewMessage, Username = _selfUserDatabaseService.GetLastSelfUser()};
            Messages.Add(message);
            NewMessage = "";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
