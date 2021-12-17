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
using Telegram.Bot.Types.ReplyMarkups;

namespace CourseManagementBot
{
    internal class CheckCurrentUser
    {
        private CourseManagementDataContext db = new CourseManagementDataContext();
        private string logBotAnswer = "";
        public async void Check(ChattedUser CurrentChattedUser, Update UpdMsg, ITelegramBotClient bot, CancellationTokenSource cts, bool IsStartedChatting)
        {
            if (!IsStartedChatting)
            {
                switch (UpdMsg.Message!.Type)
                {
                    case MessageType.Text:
                        switch (UpdMsg.Message.Text)
                        {
                            case "Мой профиль":
                                logBotAnswer = $"<b>Ваш профиль:</b>\n\nID пользователя: {UpdMsg.Message.From!.Id}\nИмя пользователя: {UpdMsg.Message.From.Username ?? UpdMsg.Message.From.FirstName}\nРоль пользователя: {CurrentChattedUser.Role}.";
                                Message sentMessage = await bot.SendTextMessageAsync(
                                    chatId: UpdMsg.Message!.Chat.Id,
                                    text: logBotAnswer,
                                    replyMarkup: ProfileInlineKeyboard,
                                    parseMode: ParseMode.Html,
                                    cancellationToken: cts.Token);
                                Console.WriteLine($" ~ [{DateTime.Now.ToString("yyyy-MM-dd HH-mm")}] Ответ пользователю @{UpdMsg.Message!.From!.Username}: Информация о его профиле: ID - {UpdMsg.Message.From!.Id}, Username - {UpdMsg.Message.From.Username ?? UpdMsg.Message.From.FirstName}, Роль - {CurrentChattedUser.Role}");
                                break;
                            case "Курсы":
                                // Переход в другой класс для полноценной работы с курсами.
                                break;
                        }
                        break;
                }
            }
            else
            {
                logBotAnswer = $"{char.ConvertFromUtf32(0x2728)} <b>Добро пожаловать в автоматизированную систему управления курсами!</b> {char.ConvertFromUtf32(0x2728)}\nЗдесь вы можете присоединиться к уже существующему курсу или создать свой курс, в котором вы сможете обучать других пользователей данной системы.\n{char.ConvertFromUtf32(0x1F60A)} <b>Начните с нами уже сейчас!</b> {char.ConvertFromUtf32(0x1F60A)}";
                Message sentMessage = await bot.SendTextMessageAsync(
                    chatId: UpdMsg.Message!.Chat.Id,
                    text: logBotAnswer,
                    replyMarkup: UserMainReplyKeyboradMarkup,
                    parseMode: ParseMode.Html,
                    cancellationToken: cts.Token);
                Console.WriteLine($" ~ [{DateTime.Now.ToString("yyyy-MM-dd HH-mm")}] Ответ пользователю @{UpdMsg.Message!.From!.Username}: *Сообщение о приветствии и информации продукта для нового пользователя*");
            }
        }

        ReplyKeyboardMarkup UserMainReplyKeyboradMarkup = new(new[]
        {
            new KeyboardButton[] { "Мой профиль", "Курсы" },
            new KeyboardButton[] { "Уведомления" },
        })
        {
            ResizeKeyboard = true
        };

        InlineKeyboardMarkup ProfileInlineKeyboard = new(new[]
        {
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: "Активировать токен", callbackData: "ActivateProfileToken"),
            },
        });

        public async void AnswerCallback (ChattedUser CurrentChattedUser, Update UpdMsg, ITelegramBotClient bot, CancellationTokenSource cts)
        {
            Console.WriteLine("Событие сработало");
            await bot.SendTextMessageAsync(chatId: UpdMsg.CallbackQuery!.From.Id,
                text: "rabotaet",
                cancellationToken: cts.Token);
        }
    }
}
