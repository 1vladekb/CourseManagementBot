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
            new KeyboardButton[] { "Мой профиль", "Подписки" },
            new KeyboardButton[] { "Мои курсы" },
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

        // Панель с inline кнопкой для создания курса в разделе "Мои курсы"
        public readonly InlineKeyboardMarkup CreateCourseInlineKeyboard = new(new[]
        {
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: "Создать курс", callbackData: "CreateCourse")
            }
        });

        // Панель с inline кнопкой для присоединения к курсу по токену в разделе "Подписки"
        public readonly InlineKeyboardMarkup JoinCourseByTokenInlineKeyboard = new(new[]
        {
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: "Присоединиться к курсу по токену", callbackData: "JoinCourseByToken")
            }
        });

        public readonly InlineKeyboardMarkup EditCourseElementGoBack = new(new[]
        {
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: "Назад", callbackData: "editCourseElementGoBack")
            }
        });
    }
}
