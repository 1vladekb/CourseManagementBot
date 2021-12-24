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
using CourseManagementBot.Models;

namespace CourseManagementBot
{
    internal class CheckCurrentUser
    {
        private string logBotAnswer = "";
        private CourseManagementDataContext db = new CourseManagementDataContext();
        public async void Check(Models.ChattedUser CurrentChattedUser, Update UpdMsg, ITelegramBotClient bot, CancellationTokenSource cts, bool IsStartedChatting, List<ProccessCallBackUsers> proccessCallBackUsers)
        {
            // Проверка пользователя на новизну.
            if (!IsStartedChatting)
            {
                // Проверка типа сообщения.
                switch (UpdMsg.Message!.Type)
                {
                    case MessageType.Text:
                        var currentUserProccess = proccessCallBackUsers.FirstOrDefault(obj=>obj.UserID==UpdMsg.Message.From!.Id.ToString());
                        if (currentUserProccess == null)
                        {
                            // Проверка содержимого сообщения для дальнейшего ответа пользователю в зависимости от содержимого его сообщения.
                            switch (UpdMsg.Message.Text)
                            {
                                // Вывод информации о профиле.
                                case "Мой профиль":
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
                                default:
                                    logBotAnswer = "Я вас не понял. Попробуйте выберать один из вариантов ниже.";
                                    await bot.SendTextMessageAsync(
                                        chatId: UpdMsg.Message!.Chat.Id,
                                        text: logBotAnswer,
                                        replyMarkup: UserMainReplyKeyboradMarkup,
                                        cancellationToken: cts.Token);
                                    Console.WriteLine($" ~ [{DateTime.Now.ToString("yyyy-MM-dd HH-mm")}] Ответ пользователю @{UpdMsg.Message!.From!.Username}: {logBotAnswer}");
                                    break;
                            }
                        }
                        else
                        {
                            switch(currentUserProccess.CurrentCallBackProccess)
                            {
                                case "ActivateProfileToken":
                                    if (db.ActiveTokens.Any(obj=>obj.Token==UpdMsg.Message.Text) && db.ActiveTokens.First(obj=>obj.Token==UpdMsg.Message.Text).TokenType == "Выдача роли")
                                    {
                                        //
                                        // НЕ РЕАЛИЗОВАНА ПРОВЕРКА МАКСИМАЛЬНО ДОПУСТИМОГО КОЛИЧЕСТВА ВВОДА ТОКЕНА И ДОБАВЛЕНИЕ К ТОКЕНУ ПЛЮС ОДНУ ИСПОЛЬЗОВАННУЮ ПОПЫТКУ.
                                        //
                                        db.ChattedUsers.First(obj => obj.Id == UpdMsg.Message.From!.Id.ToString()).Role = db.ActiveTokens.First(obj => obj.Token == UpdMsg.Message.Text).TokenType;
                                        db.SaveChanges();
                                        logBotAnswer = $"{char.ConvertFromUtf32(0x2705)} Вы успешно применили токен на получение роли \"{db.ActiveTokens.First(obj => obj.Token == UpdMsg.Message.Text).TokenType}\"!";
                                        await bot.SendTextMessageAsync(chatId: UpdMsg.Message.Chat.Id,
                                            text: logBotAnswer,
                                            replyMarkup: UserMainReplyKeyboradMarkup,
                                            cancellationToken: cts.Token);
                                        Console.WriteLine($" ~ [{DateTime.Now.ToString("yyyy-MM-dd HH-mm")}] Ответ пользователю @{UpdMsg.Message.From!.Username}: Вы успешно применили токен на получение роли \"{db.ActiveTokens.First(obj => obj.Token == UpdMsg.Message.Text).TokenType}\"!");
                                    }
                                    else
                                    {
                                        logBotAnswer = $"{char.ConvertFromUtf32(0x274C)} Ошибка.\nДанного токена не существует.";
                                        await bot.SendTextMessageAsync(chatId: UpdMsg.Message.Chat.Id,
                                            text: logBotAnswer,
                                            cancellationToken: cts.Token);
                                        Console.WriteLine($" ~ [{DateTime.Now.ToString("yyyy-MM-dd HH-mm")}] Ответ пользователю @{UpdMsg.Message.From!.Username}: Ошибка. Данного токена не существует.");
                                    }
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
            }
        });

        InlineKeyboardMarkup GoBackInlineKeyboard = new(new[]
        {
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: "Назад", callbackData: "GoBack")
            }
        });

        // Порядок действий после того, как пользователь нажал inline кнопку.
        public async void AnswerCallback(Models.ChattedUser CurrentChattedUser, Update UpdMsg, ITelegramBotClient bot, CancellationTokenSource cts, List<ProccessCallBackUsers> proccessCallBackUsers)
        {
            // Проверка идентификатора нажатой кнопки.
            switch (UpdMsg.CallbackQuery!.Data)
            {
                case "ActivateProfileToken":
                    proccessCallBackUsers.Add(new ProccessCallBackUsers
                    {
                        UserID = UpdMsg.CallbackQuery!.From.Id.ToString(),
                        CurrentCallBackProccess = "ActivateProfileToken"
                    });
                    await bot.EditMessageReplyMarkupAsync(UpdMsg.CallbackQuery!.From.Id,
                        messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                        replyMarkup: null,
                        cancellationToken: cts.Token);
                    logBotAnswer = "Вы перешли в процесс активации токена.";
                    await bot.SendTextMessageAsync(chatId: UpdMsg.CallbackQuery!.From.Id,
                        text: logBotAnswer,
                        replyMarkup: GoBackInlineKeyboard,
                        cancellationToken: cts.Token);
                    Console.WriteLine($" ~ [{DateTime.Now.ToString("yyyy-MM-dd HH-mm")}] Ответ пользователю @{UpdMsg.CallbackQuery!.From.Username}: {logBotAnswer}");
                    logBotAnswer = "Введите токен:";
                    await bot.SendTextMessageAsync(chatId: UpdMsg.CallbackQuery!.From.Id,
                        text: logBotAnswer,
                        replyMarkup: new ReplyKeyboardRemove(),
                        cancellationToken: cts.Token);
                    Console.WriteLine($" ~ [{DateTime.Now.ToString("yyyy-MM-dd HH-mm")}] Ответ пользователю @{UpdMsg.CallbackQuery!.From.Username}: {logBotAnswer}");
                    break;
                case "GoBack":
                    try
                    {
                        proccessCallBackUsers.Remove(proccessCallBackUsers.First(obj => obj.UserID == UpdMsg.CallbackQuery!.From.Id.ToString()));
                        await bot.EditMessageReplyMarkupAsync(UpdMsg.CallbackQuery!.From.Id,
                            messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                            replyMarkup: null,
                            cancellationToken: cts.Token);
                        logBotAnswer = "Вы вернулись в главное меню.\nВыберите следующие предложенные пункты для дальнейшей навигации по системе.";
                        await bot.SendTextMessageAsync(chatId: UpdMsg.CallbackQuery!.From.Id,
                            text: logBotAnswer,
                            replyMarkup: UserMainReplyKeyboradMarkup,
                            cancellationToken: cts.Token);
                        Console.WriteLine($" ~ [{DateTime.Now.ToString("yyyy-MM-dd HH-mm")}] Ответ пользователю @{UpdMsg.CallbackQuery!.From.Username}: {logBotAnswer}");
                        break;
                    }
                    catch (Exception)
                    {
                        break;
                    }
                case "EditProfile":
                    await bot.SendTextMessageAsync(chatId: UpdMsg.CallbackQuery!.From.Id,
                        text: "Здесь будет функционал редактирования профиля.",
                        cancellationToken: cts.Token);
                    break;
            }
        }
    }
}
