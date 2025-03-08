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
using System.Collections.Specialized;
using System.Diagnostics;

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
            var selfUserService = new SelfUserDatabaseService();
            var messagesService = new MessagesService(usersService, selfUserService);
            var selfUserDatabaseService = new SelfUserDatabaseService();
            var messagesdatabase = new MessagesDatabaseService();
            var tokenService = new TokenService();
            DataContext = new MessangerViewModel(messagesService, selfUserDatabaseService, messagesdatabase, tokenService);
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Send.Command.Execute("SendMessageCommand");
            }
        }

        private static bool isSubscribed = false;
        private bool alreadyHaveItem = false;
        private object currentLastMessage;
        private async void MessagesList_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox == null) return;

            var scrollViewer = GetScrollViewer(listBox);
            if (scrollViewer == null) return;

            var vm = DataContext as MessangerViewModel;
            if (vm == null || !vm.LoadOlderMessagesCommand.CanExecute(null) || vm.IsLoading) return;

            if (listBox.Items.Count == 0 || listBox.ItemsSource == null) return;

            if (!isSubscribed)
            {
                ((INotifyCollectionChanged)listBox.Items).CollectionChanged += (s, args) =>
                {
                    if (args.Action == NotifyCollectionChangedAction.Add && vm.IsLoading && !alreadyHaveItem)
                    {
                        listBox.ScrollIntoView(0);
                        alreadyHaveItem = true;
                        currentLastMessage = listBox.Items[1];
                    }
                    else if (args.Action == NotifyCollectionChangedAction.Add && vm.IsLoading)
                    {
                        scrollViewer.ScrollToBottom();
                        listBox.ScrollIntoView(currentLastMessage);
                    }
                    else if (args.Action == NotifyCollectionChangedAction.Add)
                    {
                        listBox.ScrollIntoView(args.NewItems[0]);
                        alreadyHaveItem = false;
                    }
                };
                vm.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == "LoadOlderMessagesCommand")
                    {
                        alreadyHaveItem = false;
                    }
                };
                isSubscribed = true;
            }

            if (scrollViewer.VerticalOffset == 0)  
            {
                vm.LoadOlderMessagesCommand.Execute(null);
            }
        }

        private ScrollViewer GetScrollViewer(DependencyObject depObj)
        {
            if (depObj is ScrollViewer viewer) return viewer;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);
                var result = GetScrollViewer(child);
                if (result != null) return result;
            }
            return null;
        }

    }
}
