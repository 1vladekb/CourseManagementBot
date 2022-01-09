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
    internal class HandleTextMessages
    {
        private string logBotAnswer = "";
        private readonly CourseManagementDataContext db = new();
        public async Task AnswerMessage(ChattedUser? CurrentChattedUser, Update UpdMsg, ITelegramBotClient bot, CancellationTokenSource cts, List<ProccessCallBackUsers> proccessCallBackUsers)
        {
            // Проверка пользователя на новизну.
            if (CurrentChattedUser != null)
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
                                    Console.WriteLine($" ~ [{DateTime.Now:yyyy-MM-dd HH-mm}] Ответ пользователю @{UpdMsg.Message!.From!.Username}: Информация о его профиле: [ID - {UpdMsg.Message.From!.Id}], [Username - {UpdMsg.Message.From.Username ?? UpdMsg.Message.From.FirstName}], [Роль - {CurrentChattedUser.Role}]");
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
                                    Console.WriteLine($" ~ [{DateTime.Now:yyyy-MM-dd HH-mm}] Ответ пользователю @{UpdMsg.Message!.From!.Username}: {logBotAnswer}");
                                    break;
                            }
                        }
                        else
                        {
                            switch(currentUserProccess.CurrentCallBackProccess)
                            {
                                case "ActivateProfileToken":
                                    ActiveToken? currentToken = db.ActiveTokens.FirstOrDefault(obj => obj.Token == UpdMsg.Message.Text);
                                    if (currentToken != null)
                                    {
                                        if (currentToken.TokenType == "Выдача роли Администратор" || currentToken.TokenType == "Выдача роли Менеджер" || currentToken.TokenType == "Выдача роли Пользователь")
                                        {
                                            db.ChattedUsers.First(obj => obj.Id == UpdMsg.Message.From!.Id.ToString()).Role = currentToken.TokenType.Split(' ')[2];
                                            currentToken.UsedAttempts += 1;
                                            logBotAnswer = $"{char.ConvertFromUtf32(0x2705)} Вы успешно применили токен на \"{currentToken.TokenType}\"!";
                                            await bot.SendTextMessageAsync(chatId: UpdMsg.Message.Chat.Id,
                                                text: logBotAnswer,
                                                replyMarkup: UserMainReplyKeyboradMarkup,
                                                cancellationToken: cts.Token);
                                            Console.WriteLine($" ~ [{DateTime.Now:yyyy-MM-dd HH-mm}] Ответ пользователю @{UpdMsg.Message.From!.Username}: Вы успешно применили токен на \"{currentToken.TokenType}\"!");
                                            if (currentToken.MaxUsesNumber == currentToken.UsedAttempts)
                                            {
                                                Console.WriteLine($" ~ Пользователь @{UpdMsg.Message.From.Username??UpdMsg.Message.From.FirstName} совершил последнюю активацию токена. Удаляем токен из БД.");
                                                db.ActiveTokens.Remove(currentToken);
                                                Console.WriteLine(" ~ Токен был успешно удалён.");
                                            }
                                            db.SaveChanges();
                                            proccessCallBackUsers.Remove(currentUserProccess);
                                        }
                                        else
                                        {
                                            logBotAnswer = $"{char.ConvertFromUtf32(0x274C)} Ошибка.\nТокен был введён неверно.";
                                            await bot.SendTextMessageAsync(chatId: UpdMsg.Message.Chat.Id,
                                                text: logBotAnswer,
                                                cancellationToken: cts.Token);
                                            Console.WriteLine($" ~ [{DateTime.Now:yyyy-MM-dd HH-mm}] Ответ пользователю @{UpdMsg.Message.From!.Username??UpdMsg.Message.From.FirstName}: Ошибка. Токен был введён неверно.");
                                        }
                                    }
                                    else
                                    {
                                        logBotAnswer = $"{char.ConvertFromUtf32(0x274C)} Ошибка.\nДанного токена не существует.";
                                        await bot.SendTextMessageAsync(chatId: UpdMsg.Message.Chat.Id,
                                            text: logBotAnswer,
                                            cancellationToken: cts.Token);
                                        Console.WriteLine($" ~ [{DateTime.Now:yyyy-MM-dd HH-mm}] Ответ пользователю @{UpdMsg.Message.From!.Username??UpdMsg.Message.From.FirstName}: Ошибка. Данного токена не существует.");
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
                Console.WriteLine(" ~ Пользователь не найден в БД. Он впервые пишет боту.");
                // Проверка на /start в сообщении.
                if (UpdMsg.Message!.Text == "/start")
                {
                    Console.WriteLine(" ~ Пользователь запустил бота командой. Добавляем его в БД.");
                    // Процесс добавления нового пользователя в таблицу БД ChattedUsers и сохранение БД.
                    ChattedUser chattedUser = new()
                    {
                        Id = UpdMsg.Message.From!.Id.ToString(),
                        ChatId = UpdMsg.Message.Chat.Id.ToString(),
                        Name = UpdMsg.Message.From.Username ?? UpdMsg.Message.From.FirstName, // Если имя пользователя отсутствует, будет присваиваться обычное имя.
                        Role = "Пользователь"
                    };
                    db.Add(chattedUser); // Добавление новой записи в БД
                    db.SaveChanges(); // Сохранение БД
                    Console.WriteLine(" ~ Пользователь был добавлен в БД.");
                    logBotAnswer = $"{char.ConvertFromUtf32(0x2728)} <b>Добро пожаловать в автоматизированную систему управления курсами!</b> {char.ConvertFromUtf32(0x2728)}\nЗдесь вы можете присоединиться к уже существующему курсу или создать свой курс, в котором вы сможете обучать других пользователей данной системы.\n{char.ConvertFromUtf32(0x1F60A)} <b>Начните с нами уже сейчас!</b> {char.ConvertFromUtf32(0x1F60A)}";
                    await bot.SendTextMessageAsync(
                        chatId: UpdMsg.Message!.Chat.Id,
                        text: logBotAnswer,
                        replyMarkup: UserMainReplyKeyboradMarkup,
                        parseMode: ParseMode.Html,
                        cancellationToken: cts.Token);
                    Console.WriteLine($" ~ [{DateTime.Now:yyyy-MM-dd HH-mm}] Ответ пользователю @{UpdMsg.Message!.From!.Username}: *Сообщение о приветствии и информации продукта для нового пользователя*");
                }
                // Порядок действий, если пользователя нет в БД и он не написал /start.
                else
                {
                    // Логирование в консоли и отправка сообщения пользователю о том, что нужно прописать /start для дальнейшего взаимодействия с ботом.
                    logBotAnswer = "Для того, чтобы начать взаимодействовать с ботом, напишите /start";
                    await bot.SendTextMessageAsync(
                        chatId: UpdMsg.Message.Chat.Id,
                        text: logBotAnswer,
                        cancellationToken: cts.Token);
                    Console.WriteLine($" ~ [{DateTime.Now:yyyy-MM-dd HH-mm}] Ответ пользователю {UpdMsg.Message!.From!.Username}: {logBotAnswer}");
                }
            }
        }

        // Основная панель кнопок для пользователя (Main menu).
        readonly ReplyKeyboardMarkup UserMainReplyKeyboradMarkup = new(new[]
        {
            new KeyboardButton[] { "Мой профиль", "Курсы" },
            new KeyboardButton[] { "Уведомления" },
        })
        {
            ResizeKeyboard = true // Выравнивать кнопки под текст, чтобы они не были большими.
        };

        // Панель с inline кнопками при переходе в раздел "Мой профиль".
        readonly InlineKeyboardMarkup ManageProfileInlineKeyboard = new(new []
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
        readonly InlineKeyboardMarkup GoBackInlineKeyboard = new(new[]
        {
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: "Назад", callbackData: "GoBack")
            }
        });

        // Порядок действий после того, как пользователь нажал inline кнопку.
        public async void AnswerCallback(ChattedUser CurrentChattedUser, Update UpdMsg, ITelegramBotClient bot, CancellationTokenSource cts, List<ProccessCallBackUsers> proccessCallBackUsers)
        {
            // Проверка идентификатора нажатой кнопки.
            switch (UpdMsg.CallbackQuery!.Data)
            {
                case "ActivateProfileToken":
                    if (proccessCallBackUsers.FirstOrDefault(obj => obj.UserID == UpdMsg.CallbackQuery!.From.Id.ToString()) == null)
                    {
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
                        Console.WriteLine($" ~ [{DateTime.Now:yyyy-MM-dd HH-mm}] Ответ пользователю @{UpdMsg.CallbackQuery!.From.Username}: {logBotAnswer}");
                        logBotAnswer = "Введите токен:";
                        await bot.SendTextMessageAsync(chatId: UpdMsg.CallbackQuery!.From.Id,
                            text: logBotAnswer,
                            replyMarkup: new ReplyKeyboardRemove(),
                            cancellationToken: cts.Token);
                        Console.WriteLine($" ~ [{DateTime.Now:yyyy-MM-dd HH-mm}] Ответ пользователю @{UpdMsg.CallbackQuery!.From.Username}: {logBotAnswer}");
                        break;
                    }
                    else
                        break;
                case "GoBack":
                    if (proccessCallBackUsers.FirstOrDefault(obj => obj.UserID == UpdMsg.CallbackQuery!.From.Id.ToString()) != null)
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
                        Console.WriteLine($" ~ [{DateTime.Now:yyyy-MM-dd HH-mm}] Ответ пользователю @{UpdMsg.CallbackQuery!.From.Username}: {logBotAnswer}");
                        break;
                    }
                    else
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
