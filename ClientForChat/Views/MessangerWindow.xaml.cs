using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using ClientForChat.Models;
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
using ClientForChat.Data;
using ClientForChat.VIewModels;

namespace ClientForChat.Views
{
    /// <summary>
    /// Логика взаимодействия для MessangerWindow.xaml
    /// </summary>
    public partial class MessangerWindow : Window, INotifyPropertyChanged
    {
        public ObservableCollection<MessageViewModel> Messages { get; set; } = new();
        private readonly UsersDatabaseService _usersDatabaseService = new UsersDatabaseService();
        public MessangerWindow()
        {
            InitializeComponent();
            //UserModel user1 = _usersDatabaseService.GetOrFetchUser(2)
            //Messages.Add(new MessageViewModel { Username = _usersDatabaseService.GetOrFetchUser(2).Username, Content = "Привет!"});
            //Messages.Add(new MessageViewModel { Username = _usersDatabaseService.GetOrFetchUser(3).Username, Content = "Привет! Как дела?"});

            DataContext = this;
        }

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

        public async void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(NewMessage))
            {
                UserModel newuser = await _usersDatabaseService.GetOrFetchUser(3);
                Messages.Add(new MessageViewModel { Username = newuser.Username, Content = NewMessage});
                NewMessage = "";  // Очищаем поле ввода
                ScrollToBottom();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ScrollToBottom()
        {
            if (MessagesList.Items.Count > 0)
            {
                MessagesList.ScrollIntoView(MessagesList.Items[MessagesList.Items.Count - 1]);
            }
        }
    }
}
