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
        private readonly MessagesDatabaseService _messagesDatabaseService;
        private readonly TokenService _tokenService;
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

        public MessangerViewModel(MessagesService messagesService,
            SelfUserDatabaseService selfUserDatabaseService,
            MessagesDatabaseService messagesDatabaseService,
            TokenService tokenService)
        {
            _messagesService = messagesService;
            _selfUserDatabaseService = selfUserDatabaseService;
            _messagesDatabaseService = messagesDatabaseService;
            _tokenService = tokenService;
            SendMessageCommand = new RelayCommand(async () => await SendMessage(), () => !string.IsNullOrWhiteSpace(NewMessage));
            var handler = new HttpClientHandler(); 
            handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true; //ONLY FOR DEVELOPMENT 

            _hubConnection = new HubConnectionBuilder()
                .WithUrl("https://26.74.71.132:7168/messageHub", options => {
                    options.HttpMessageHandlerFactory = _ => handler;
                    options.Headers.Add("Authorization", $"Bearer {_tokenService.GetToken()}");
                })
                .WithAutomaticReconnect()
                .Build();
            _hubConnection.On<MessageModel>("ReceiveMessage", (message) =>
            {
                App.Current.Dispatcher.Invoke(async () =>
                {
                    _messagesDatabaseService.SaveMessageAsync(message);
                    MessageLocalModel localMessage = await _messagesService.MessageToLocalMessageAsync(message);
                    Messages.Add(localMessage);
                });
            });
            _hubConnection.On<string>("SystemMessage", (message) =>
            {
                App.Current.Dispatcher.Invoke(async () =>
                {
                    MessageLocalModel localMessage = new MessageLocalModel { Content = message, Username = "System" };
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
                Console.WriteLine($"Connection Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

        }

        private async Task SendMessage()
        {
            MessageLocalModel message = new MessageLocalModel {Content = NewMessage, Username = _selfUserDatabaseService.GetLastSelfUser()};
            NewMessageModel newMessage = new NewMessageModel{ Content = message.Content};
            await _hubConnection.InvokeAsync("SendMessage", newMessage);
            //Messages.Add(message);
            NewMessage = "";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
