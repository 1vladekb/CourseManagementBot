using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot;
using Telegram.Bot.Types;
using CourseManagementBot.Models;

namespace CourseManagementBot
{
    internal class Keyboards
    {
        // Основная панель с reply кнопками для пользователя (Main menu).
        public readonly ReplyKeyboardMarkup UserMainReplyKeyboradMarkup = new(new[]
        {
            new KeyboardButton[] { "Мой профиль", "Курсы" },
            new KeyboardButton[] { "Уведомления" },
        })
        {
            ResizeKeyboard = true // Выравнивать кнопки под текст, чтобы они не были большими.
        };

        // Панель с inline кнопками в разделе "Мой профиль".
        public readonly InlineKeyboardMarkup ManageProfileInlineKeyboard = new(new[]
        {
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: "Активировать токен", callbackData: "ActivateProfileToken")
            },
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: "Редактировать профиль", callbackData: "EditProfile")
            }
        });

        // Панель с inline кнопкой при переходе пользователя в любой процесс CallBack запроса
        public readonly InlineKeyboardMarkup GoBackInlineKeyboard = new(new[]
        {
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: $"{char.ConvertFromUtf32(0x2B05)} Назад", callbackData: "GoBack")
            }
        });
    }
}
