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
        private string logBotAnswer = "";
        public async void Check(Models.ChattedUser CurrentChattedUser, Update UpdMsg, ITelegramBotClient bot, CancellationTokenSource cts, bool IsStartedChatting, List<string> ProccessCallBackUsers)
        {
            // Проверка пользователя на новизну.
            if (!IsStartedChatting)
            {
                // Проверка типа сообщения.
                switch (UpdMsg.Message!.Type)
                {
                    case MessageType.Text:
                        string currentUserProccess = "";
                        foreach (string proccessCallBackUser in ProccessCallBackUsers)
                        {
                            string[] currentUserProccessInfo = proccessCallBackUser.Split(' ');
                            if (UpdMsg.Message.From!.Id.ToString() == currentUserProccessInfo[0])
                            {
                                currentUserProccess = proccessCallBackUser;
                                break;
                            }
                        }
                        if (currentUserProccess == "")
                        {
                            // Проверка содержимого сообщения для дальнейшего ответа пользователю в зависимости от содержимого его сообщения.
                            switch (UpdMsg.Message.Text)
                            {
                                case "Мой профиль":
                                    // Вывод информации о профиле.
                                    logBotAnswer = $"<b>Ваш профиль:</b>\n\nID пользователя: {UpdMsg.Message.From!.Id}\nИмя пользователя: {UpdMsg.Message.From.Username ?? UpdMsg.Message.From.FirstName}\nРоль пользователя: {CurrentChattedUser.Role}.";
                                    await bot.SendTextMessageAsync(
                                        chatId: UpdMsg.Message!.Chat.Id,
                                        text: logBotAnswer,
                                        replyMarkup: ManageProfileInlineKeyboard,
                                        parseMode: ParseMode.Html,
                                        cancellationToken: cts.Token);
                                    Console.WriteLine($" ~ [{DateTime.Now.ToString("yyyy-MM-dd HH-mm")}] Ответ пользователю @{UpdMsg.Message!.From!.Username}: Информация о его профиле: [ID - {UpdMsg.Message.From!.Id}], [Username - {UpdMsg.Message.From.Username ?? UpdMsg.Message.From.FirstName}], [Роль - {CurrentChattedUser.Role}]");
                                    break;
                                case "Курсы":
                                    // Реализовать переход в другой класс для полноценной работы с курсами.
                                    break;
                            }
                        }
                        else
                        {
                            switch(currentUserProccess.Split(' ')[1])
                            {
                                case "ActivateProfileToken":
                                    // Здесь происходит проверка токена.
                                    break;
                            }
                        }
                        break;
                }
            }
            // Если пользователь только что написал боту, возвращает ему сообщение с приветствием и информацией о проекте.
            else
            {
                logBotAnswer = $"{char.ConvertFromUtf32(0x2728)} <b>Добро пожаловать в автоматизированную систему управления курсами!</b> {char.ConvertFromUtf32(0x2728)}\nЗдесь вы можете присоединиться к уже существующему курсу или создать свой курс, в котором вы сможете обучать других пользователей данной системы.\n{char.ConvertFromUtf32(0x1F60A)} <b>Начните с нами уже сейчас!</b> {char.ConvertFromUtf32(0x1F60A)}";
                await bot.SendTextMessageAsync(
                    chatId: UpdMsg.Message!.Chat.Id,
                    text: logBotAnswer,
                    replyMarkup: UserMainReplyKeyboradMarkup,
                    parseMode: ParseMode.Html,
                    cancellationToken: cts.Token);
                Console.WriteLine($" ~ [{DateTime.Now.ToString("yyyy-MM-dd HH-mm")}] Ответ пользователю @{UpdMsg.Message!.From!.Username}: *Сообщение о приветствии и информации продукта для нового пользователя*");
            }
        }

        // Основная панель кнопок для пользователя (Main menu).
        ReplyKeyboardMarkup UserMainReplyKeyboradMarkup = new(new[]
        {
            new KeyboardButton[] { "Мой профиль", "Курсы" },
            new KeyboardButton[] { "Уведомления" },
        })
        {
            ResizeKeyboard = true // Выравнивать кнопки под текст, чтобы они не были большими.
        };

        // Панель с inline кнопками при переходе в раздел "Мой профиль".
        InlineKeyboardMarkup ManageProfileInlineKeyboard = new(new []
        {
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: "Активировать токен", callbackData: "ActivateProfileToken")
            },
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: "Редактировать профиль", callbackData: "EditProfile")
            },
        });

        // Порядок действий после того, как пользователь нажал inline кнопку.
        public async void AnswerCallback(Models.ChattedUser CurrentChattedUser, Update UpdMsg, ITelegramBotClient bot, CancellationTokenSource cts, List<string> ProccessCallBackUsers)
        {
            // Проверка идентификатора нажатой кнопки.
            switch (UpdMsg.CallbackQuery!.Data)
            {
                case "ActivateProfileToken":
                    ProccessCallBackUsers.Add($"{UpdMsg.CallbackQuery!.From.Id} ActivateProfileToken");
                    await bot.EditMessageReplyMarkupAsync(UpdMsg.CallbackQuery!.From.Id,
                        UpdMsg.CallbackQuery.Message!.MessageId,
                        replyMarkup: null,
                        cancellationToken: cts.Token);
                    logBotAnswer = "Введите токен:";
                    await bot.SendTextMessageAsync(chatId: UpdMsg.CallbackQuery!.From.Id,
                        text: logBotAnswer,
                        cancellationToken: cts.Token);
                    Console.WriteLine($" ~ [{DateTime.Now.ToString("yyyy-MM-dd HH-mm")}] Ответ пользователю @{UpdMsg.CallbackQuery!.From.Username}: {logBotAnswer}.");
                    break;
                case "EditProfile":
                    await bot.SendTextMessageAsync(chatId: UpdMsg.CallbackQuery!.From.Id,
                        text: "Здесь будет функционал редактирования профиля.",
                        cancellationToken: cts.Token);
                    break;
            }
        }
    }
}
