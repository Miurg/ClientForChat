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
using ClientForChat.Services;

namespace ClientForChat.Views
{
    /// <summary>
    /// Логика взаимодействия для MessangerWindow.xaml
    /// </summary>
    public partial class MessangerWindow : Window
    {
        public MessangerWindow()
        {
            InitializeComponent();
            var usersService = new UsersDatabaseService();
            var messagesService = new MessagesService(usersService);
            var selfUserDatabaseService = new SelfUserDatabaseService();
            DataContext = new MessangerViewModel(messagesService, selfUserDatabaseService);
        }
    }
}
