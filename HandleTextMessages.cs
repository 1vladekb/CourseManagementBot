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
        private string logBotAnswer = ""; // Строка для заполнения информации, которую будет передавать бот.
        private readonly CourseManagementDataContext db = new();
        private readonly Keyboards keyboards = new(); // Клавиатуры для дальнейшего отправления их пользователю для навигации.
        public async Task AnswerMessage(ChattedUser? CurrentChattedUser, Update UpdMsg, ITelegramBotClient bot, CancellationTokenSource cts, List<ProccessCallBackUsers> proccessCallBackUsers)
        {
            // Проверка пользователя на новизну.
            if (CurrentChattedUser != null)
            {
                // Проверка типа сообщения.
                switch (UpdMsg.Message!.Type)
                {
                    case MessageType.Text:
                        var currentUserProccess = proccessCallBackUsers.FirstOrDefault(obj=>obj.UserID==UpdMsg.Message.From!.Id.ToString()); // Попытка найти пользователя, написавшего сообщение, в процессе CallBack запроса. Вернёт null, если его там нет.
                        // Проверка переменной выше на наличие пользователя в CallBack запросе.
                        if (currentUserProccess == null)
                        {
                            // Проверка содержимого сообщения для дальнейшего ответа пользователю в зависимости от содержимого его сообщения.
                            switch (UpdMsg.Message.Text)
                            {
                                // Вывод информации о профиле.
                                case "Мой профиль":
                                    logBotAnswer = $"<b>Ваш профиль:</b>\n\nID пользователя: {UpdMsg.Message.From!.Id}\nИмя пользователя: {UpdMsg.Message.From.Username ?? UpdMsg.Message.From.FirstName}\nРоль пользователя: {CurrentChattedUser.Role}\n\nФамилия: {CurrentChattedUser.LastName??"пусто"}\nИмя: {CurrentChattedUser.FirstName??"пусто"}\nОтчество: {CurrentChattedUser.MiddleName??"пусто"}\nПочта: {CurrentChattedUser.Email??"пусто"}.";
                                    await bot.SendTextMessageAsync(
                                        chatId: UpdMsg.Message!.Chat.Id,
                                        text: logBotAnswer,
                                        replyMarkup: keyboards.ManageProfileInlineKeyboard,
                                        parseMode: ParseMode.Html,
                                        cancellationToken: cts.Token);
                                    Console.WriteLine($" ~ [{DateTime.Now:yyyy-MM-dd HH-mm}] Ответ пользователю @{UpdMsg.Message!.From!.Username}: {logBotAnswer}");
                                    break;
                                case "Курсы":
                                    // Реализовать переход в другой класс для полноценной работы с курсами.
                                    break;
                                // Вывод сообщения об ошибке пользователю в случае, если он написал сообщение, не соответствующее условиям для дальнейшей работы.
                                default:
                                    logBotAnswer = "Я вас не понял. Попробуйте выберать один из вариантов ниже.";
                                    await bot.SendTextMessageAsync(
                                        chatId: UpdMsg.Message!.Chat.Id,
                                        text: logBotAnswer,
                                        replyMarkup: keyboards.UserMainReplyKeyboradMarkup,
                                        cancellationToken: cts.Token);
                                    Console.WriteLine($" ~ [{DateTime.Now:yyyy-MM-dd HH-mm}] Ответ пользователю @{UpdMsg.Message!.From!.Username}: {logBotAnswer}");
                                    break;
                            }
                        }
                        // Алгоритм действий в случае, если пользователь находится внутри CallBack запроса (currentUserProccess != null).
                        else
                        {
                            // Проверка текущего CallBack запроса, в котором находится пользователь.
                            switch(currentUserProccess.CurrentCallBackProccess)
                            {
                                // Алгоритм действий, если пользователь ввел сообщение, находясь при этом в CallBack запросе активации токена для получения роли.
                                case "ActivateProfileToken":
                                    // Проверка введенного токена на актуальные токены в БД.
                                    ActiveToken? currentToken = db.ActiveTokens.FirstOrDefault(obj => obj.Token == UpdMsg.Message.Text);
                                    if (currentToken != null)
                                    {
                                        // Проверка типа токена на соответствующий CallBack запросу (т.к. известно, что пользователь в CallBack запросе активации токена для ПОЛУЧЕНИЯ РОЛИ, то проверять нужно тип токена, который предназначен именно на получение роли).
                                        if (currentToken.TokenType == "Выдача роли Администратор" || currentToken.TokenType == "Выдача роли Менеджер" || currentToken.TokenType == "Выдача роли Пользователь")
                                        {
                                            CurrentChattedUser.Role = currentToken.TokenType.Split(' ')[2]; // Применяем пользователю новую роль, в зависимости от той, которая прикреплена к токену, который был введен этим самым пользователем.
                                            currentToken.UsedAttempts++; // Добавляем +1 к количеству использований токена
                                            logBotAnswer = $"{char.ConvertFromUtf32(0x2705)} Вы успешно применили токен на \"{currentToken.TokenType}\"!";
                                            await bot.SendTextMessageAsync(chatId: UpdMsg.Message.Chat.Id,
                                                text: logBotAnswer,
                                                replyMarkup: keyboards.UserMainReplyKeyboradMarkup,
                                                cancellationToken: cts.Token);
                                            Console.WriteLine($" ~ [{DateTime.Now:yyyy-MM-dd HH-mm}] Ответ пользователю @{UpdMsg.Message.From!.Username}: Вы успешно применили токен на \"{currentToken.TokenType}\"!");
                                            // Проверка на достигнутый лимит по использованиям токена.
                                            if (currentToken.MaxUsesNumber == currentToken.UsedAttempts)
                                            {
                                                Console.WriteLine($" ~ Пользователь @{UpdMsg.Message.From.Username??UpdMsg.Message.From.FirstName} совершил последнюю активацию токена. Удаляем токен из БД.");
                                                db.ActiveTokens.Remove(currentToken); // Удаление токена в случае, если лимит был достигнут.
                                                Console.WriteLine(" ~ Токен был успешно удалён.");
                                            }
                                            db.SaveChanges(); // Сохранение всех изменений БД.
                                            proccessCallBackUsers.Remove(currentUserProccess); // Удаление пользователя из CallBack запроса / возвращаем пользователя в главное меню.
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
                                    // Алгоритм действий в случае, если пользователь ввёл неверный токен (тот токен, которого не существует в БД).
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
                    db.Add(chattedUser); // Добавление нового пользователя в БД
                    db.SaveChanges(); // Сохранение БД
                    Console.WriteLine(" ~ Пользователь был добавлен в БД.");
                    logBotAnswer = $"{char.ConvertFromUtf32(0x2728)} <b>Добро пожаловать в автоматизированную систему управления курсами!</b> {char.ConvertFromUtf32(0x2728)}\nЗдесь вы можете присоединиться к уже существующему курсу или создать свой курс, в котором вы сможете обучать других пользователей данной системы.\n{char.ConvertFromUtf32(0x1F60A)} <b>Начните с нами уже сейчас!</b> {char.ConvertFromUtf32(0x1F60A)}";
                    await bot.SendTextMessageAsync(
                        chatId: UpdMsg.Message!.Chat.Id,
                        text: logBotAnswer,
                        replyMarkup: keyboards.UserMainReplyKeyboradMarkup,
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

        
    }
}
