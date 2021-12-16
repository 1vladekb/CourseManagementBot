using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CourseManagementBot
{
    internal class CheckCurrentUser
    {
        private CourseManagementBot.CourseManagementDataContext db = new CourseManagementBot.CourseManagementDataContext();
        public async void Check (ChattedUser currentChattedUser, Update updMsg)
        {

        }
    }
}
