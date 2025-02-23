using ClientForChat.Data;
using ClientForChat.Models;
using ClientForChat.VIewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientForChat.Services
{
    public class MessagesService
    {
        private readonly UsersDatabaseService _usersDatabaseService;

        public MessagesService(UsersDatabaseService usersDatabaseService)
        {
            _usersDatabaseService = usersDatabaseService;
        }

        public async Task<MessageLocalModel> MessageToLocalMessageAsync(MessageModel message)
        {
            var username = await _usersDatabaseService.GetOrFetchUser(message.UserID);
            return new MessageLocalModel { Username = username.Username, Content = message.Content };
        }
    }
}
