﻿using System;
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
using Microsoft.EntityFrameworkCore;

namespace CourseManagementBot
{
    internal class HandleTextMessages
    {
        private string logBotAnswer = ""; // Строка для заполнения информации, которую будет передавать бот.
        private readonly CourseManagementDataContext db = new();
        private readonly Keyboards keyboards = new(); // Клавиатуры для дальнейшего отправления их пользователю для навигации
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
                                // Вывод информации о собственных курсах.
                                case "Мои курсы":
                                    // Переменная, получающая список курсов, которыми владеет пользователь.
                                    var currentUserCourses = db.Courses.AsNoTracking().Where(obj=>obj.Curator==UpdMsg.Message.From!.Id.ToString()).ToDictionary(obj=>obj.Id.ToString(),obj=>obj.Name);
                                    // Проверка на наличие владения хотя бы одним курсом пользователя.
                                    if (currentUserCourses.Count() != 0)
                                    {
                                        logBotAnswer = "Список ваших курсов:";
                                        // Создаем динамическую клавиатуру из записей всех курсов, принадлежащих пользователю, и передаем их с уникальным идентификатором CallBackData - ownCourses для дальнейшей обработки конкретной записи.
                                        var keyboardMarkup = new InlineKeyboardMarkup(GetInlineKeyboard(currentUserCourses, "ownCourses"));

                                        await bot.SendTextMessageAsync(
                                            chatId: UpdMsg.Message!.Chat.Id,
                                            text: logBotAnswer,
                                            replyMarkup: keyboardMarkup,
                                            parseMode: ParseMode.Html,
                                            cancellationToken: cts.Token);
                                        Console.WriteLine($" ~ [{DateTime.Now:yyyy-MM-dd HH-mm}] Ответ пользователю @{UpdMsg.Message!.From!.Username}: {logBotAnswer}");
                                    }
                                    // Вывод сообщения об ошибке пользователю, если пользователь не владеет ни одним курсом.
                                    else
                                    {
                                        logBotAnswer = "Вы не владеете ни одним курсом.";
                                        await bot.SendTextMessageAsync(
                                            chatId: UpdMsg.Message!.Chat.Id,
                                            text: logBotAnswer,
                                            replyMarkup: keyboards.CreateCourseInlineKeyboard, // Прикрепление функции создания курса для пользователя после ошибки.
                                            cancellationToken: cts.Token);
                                        Console.WriteLine($" ~ [{DateTime.Now:yyyy-MM-dd HH-mm}] Ответ пользователю @{UpdMsg.Message!.From!.Username}: {logBotAnswer}");
                                    }
                                    break;
                                // Вывод информации о курсах, на которые пользователь подписался.
                                case "Подписки":
                                    var currentCoursesMember = (from courses in db.Courses
                                                               join courseUser in db.CourseUsers on courses.Id equals courseUser.Course
                                                               where courseUser.PinnedUser == UpdMsg.Message.From!.Id.ToString() && courseUser.Admitted == true
                                                               select courses).ToDictionary(obj=>obj.Id.ToString(), obj=>obj.Name);
                                    // Проверка на подписку хотя бы на один курс.
                                    if (currentCoursesMember.Count() != 0)
                                    {
                                        logBotAnswer = "Список ваших подписок на курсы:";
                                        // Создаем динамическую клавиатуру из записей всех курсов, На которых подписан пользователь, и передаем их с уникальным идентификатором CallBackData - coursesMember для дальнейшей обработки конкретной записи.
                                        var keyboardMarkup = new InlineKeyboardMarkup(GetInlineKeyboard(currentCoursesMember, "coursesMember"));

                                        await bot.SendTextMessageAsync(
                                            chatId: UpdMsg.Message!.Chat.Id,
                                            text: logBotAnswer,
                                            replyMarkup: keyboardMarkup,
                                            parseMode: ParseMode.Html,
                                            cancellationToken: cts.Token);
                                        Console.WriteLine($" ~ [{DateTime.Now:yyyy-MM-dd HH-mm}] Ответ пользователю @{UpdMsg.Message!.From!.Username}: {logBotAnswer}");
                                    }
                                    // Вывод сообщения об ошибке пользователю, если пользователь не подписан ни на один курс.
                                    else
                                    {
                                        logBotAnswer = "Вы не подписаны ни на один курс.";
                                        await bot.SendTextMessageAsync(
                                            chatId: UpdMsg.Message!.Chat.Id,
                                            text: logBotAnswer,
                                            replyMarkup: keyboards.JoinCourseByTokenInlineKeyboard, // Прикрепление функции присоединения к курсу по токену для пользователя после ошибки.
                                            cancellationToken: cts.Token);
                                        Console.WriteLine($" ~ [{DateTime.Now:yyyy-MM-dd HH-mm}] Ответ пользователю @{UpdMsg.Message!.From!.Username}: {logBotAnswer}");
                                    }
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
                                case "filterOwnCourses":
                                    var currentUserCoursesFiltered = db.Courses.Where(obj => obj.Curator == UpdMsg.Message.From!.Id.ToString() && obj.Name.Contains(UpdMsg.Message.Text!)).ToDictionary(obj => obj.Id.ToString(), obj => obj.Name);
                                    if (currentUserCoursesFiltered.Count() != 0)
                                    {
                                        logBotAnswer = "Результаты поиска:";
                                        // Создаем динамическую клавиатуру из записей всех курсов, принадлежащих пользователю, и передаем их с уникальным идентификатором CallBackData - ownCourses для дальнейшей обработки конкретной записи.
                                        var keyboardMarkup = new InlineKeyboardMarkup(GetInlineKeyboard(currentUserCoursesFiltered, "ownCourses"));
                                        await bot.SendTextMessageAsync(
                                            chatId: UpdMsg.Message!.Chat.Id,
                                            text: logBotAnswer,
                                            replyMarkup: keyboards.UserMainReplyKeyboradMarkup,
                                            cancellationToken: cts.Token);
                                        logBotAnswer = $"Всего найдено {currentUserCoursesFiltered.Count()} результатов";
                                        await bot.SendTextMessageAsync(
                                            chatId: UpdMsg.Message!.Chat.Id,
                                            text: logBotAnswer,
                                            replyMarkup: keyboardMarkup,
                                            parseMode: ParseMode.Html,
                                            cancellationToken: cts.Token);
                                        Console.WriteLine($" ~ [{DateTime.Now:yyyy-MM-dd HH-mm}] Ответ пользователю @{UpdMsg.Message!.From!.Username}: {logBotAnswer}");
                                        proccessCallBackUsers.Remove(currentUserProccess); // Удаление пользователя из CallBack запроса / возвращаем пользователя в главное меню.
                                    }
                                    // Вывод сообщения об ошибке пользователю, если пользователь не владеет ни одним курсом.
                                    else
                                    {
                                        logBotAnswer = "Совпадения не найдены.";
                                        await bot.SendTextMessageAsync(
                                            chatId: UpdMsg.Message!.Chat.Id,
                                            text: logBotAnswer, // Прикрепление функции создания курса для пользователя после ошибки.
                                            cancellationToken: cts.Token);
                                        Console.WriteLine($" ~ [{DateTime.Now:yyyy-MM-dd HH-mm}] Ответ пользователю @{UpdMsg.Message!.From!.Username}: {logBotAnswer}");
                                    }
                                    break;
                                case "filterCoursesMember":
                                    var currentCoursesMemberFiltered = (from courses in db.Courses
                                                                join courseUser in db.CourseUsers on courses.Id equals courseUser.Course
                                                                where courseUser.PinnedUser == UpdMsg.Message.From!.Id.ToString() && courseUser.Admitted == true
                                                                select courses).Where(obj=>obj.Name.Contains(UpdMsg.Message.Text!)).ToDictionary(obj => obj.Id.ToString(), obj => obj.Name);
                                    if (currentCoursesMemberFiltered.Count() != 0)
                                    {
                                        logBotAnswer = "Результат поиска:";
                                        // Создаем динамическую клавиатуру из записей всех курсов, принадлежащих пользователю, и передаем их с уникальным идентификатором CallBackData - ownCourses для дальнейшей обработки конкретной записи.
                                        var keyboardMarkup = new InlineKeyboardMarkup(GetInlineKeyboard(currentCoursesMemberFiltered, "coursesMember"));
                                        await bot.SendTextMessageAsync(
                                            chatId: UpdMsg.Message!.Chat.Id,
                                            text: logBotAnswer,
                                            replyMarkup: keyboards.UserMainReplyKeyboradMarkup,
                                            cancellationToken: cts.Token);
                                        logBotAnswer = $"Всего найдено {currentCoursesMemberFiltered.Count()} результатов";
                                        await bot.SendTextMessageAsync(
                                            chatId: UpdMsg.Message!.Chat.Id,
                                            text: logBotAnswer,
                                            replyMarkup: keyboardMarkup,
                                            parseMode: ParseMode.Html,
                                            cancellationToken: cts.Token);
                                        Console.WriteLine($" ~ [{DateTime.Now:yyyy-MM-dd HH-mm}] Ответ пользователю @{UpdMsg.Message!.From!.Username}: {logBotAnswer}");
                                        proccessCallBackUsers.Remove(currentUserProccess); // Удаление пользователя из CallBack запроса / возвращаем пользователя в главное меню.
                                    }
                                    // Вывод сообщения об ошибке пользователю, если пользователь не владеет ни одним курсом.
                                    else
                                    {
                                        logBotAnswer = "Совпадения не найдены.";
                                        await bot.SendTextMessageAsync(
                                            chatId: UpdMsg.Message!.Chat.Id,
                                            text: logBotAnswer, // Прикрепление функции создания курса для пользователя после ошибки.
                                            cancellationToken: cts.Token);
                                        Console.WriteLine($" ~ [{DateTime.Now:yyyy-MM-dd HH-mm}] Ответ пользователю @{UpdMsg.Message!.From!.Username}: {logBotAnswer}");
                                    }
                                    break;
                                case "JoinCourseByToken":
                                    // Проверка введенного токена на актуальные токены в БД.
                                    ActiveToken? currentCourseToken = db.ActiveTokens.FirstOrDefault(obj => obj.Token == UpdMsg.Message.Text);
                                    if (currentCourseToken != null)
                                    {
                                        var currentCourse = db.Courses.FirstOrDefault(obj => obj.Token == currentCourseToken.Token);
                                        if (currentCourse != null)
                                        {
                                            // Проверка типа токена на соответствующий CallBack запросу (т.к. известно, что пользователь в CallBack запросе активации токена для ПОЛУЧЕНИЯ РОЛИ, то проверять нужно тип токена, который предназначен именно на получение роли).
                                            if (currentCourseToken.TokenType == "Присоединение к курсу")
                                            {
                                                if (db.CourseUsers.FirstOrDefault(obj=>obj.Course == currentCourse.Id && obj.PinnedUser == UpdMsg.Message.From!.Id.ToString()) == null)
                                                {
                                                    logBotAnswer = $"Найден следующий курс:\n\n<b>{currentCourse.Name}</b>\n\nВы уверены, что хотите подписаться на курс?";
                                                    Dictionary<string, string> courseJoinChoiceButtons = new()
                                                    {
                                                        { "acceptJoinCourse-" + currentCourseToken.Token, "Да" },
                                                        { "GoBack", "Нет" }
                                                    };
                                                    var courseJoinChoiceInlineKeyboard = new InlineKeyboardMarkup(HandleTextMessages.GetInlineKeyboard(courseJoinChoiceButtons, currentCourse.Id.ToString()));
                                                    await bot.SendTextMessageAsync(
                                                        chatId: UpdMsg.Message!.Chat.Id,
                                                        text: logBotAnswer,
                                                        replyMarkup: courseJoinChoiceInlineKeyboard,
                                                        parseMode: ParseMode.Html,
                                                        cancellationToken: cts.Token);
                                                }
                                                else
                                                {
                                                    logBotAnswer = "Ошибка. Вы уже являетесь участником данного курса или уже подали заявку на вступление.";
                                                    await bot.SendTextMessageAsync(
                                                        chatId: UpdMsg.Message!.Chat.Id,
                                                        text: logBotAnswer,
                                                        cancellationToken: cts.Token);
                                                }
                                            }
                                            else
                                            {
                                                logBotAnswer = $"{char.ConvertFromUtf32(0x274C)} Ошибка.\nТокен был введён неверно.";
                                                await bot.SendTextMessageAsync(chatId: UpdMsg.Message.Chat.Id,
                                                    text: logBotAnswer,
                                                    cancellationToken: cts.Token);
                                                Console.WriteLine($" ~ [{DateTime.Now:yyyy-MM-dd HH-mm}] Ответ пользователю @{UpdMsg.Message.From!.Username ?? UpdMsg.Message.From.FirstName}: Ошибка. Токен был введён неверно.");
                                            }
                                        }
                                    }
                                    // Алгоритм действий в случае, если пользователь ввёл неверный токен (тот токен, которого не существует в БД).
                                    else
                                    {
                                        logBotAnswer = $"{char.ConvertFromUtf32(0x274C)} Ошибка.\nДанного токена не существует.";
                                        await bot.SendTextMessageAsync(chatId: UpdMsg.Message.Chat.Id,
                                            text: logBotAnswer,
                                            cancellationToken: cts.Token);
                                        Console.WriteLine($" ~ [{DateTime.Now:yyyy-MM-dd HH-mm}] Ответ пользователю @{UpdMsg.Message.From!.Username ?? UpdMsg.Message.From.FirstName}: Ошибка. Данного токена не существует.");
                                    }
                                    break;
                                default:
                                    if (currentUserProccess.CurrentCallBackProccess!.Contains("editCourseName"))
                                    {
                                        var currentCourseNameEdit = db.Courses.First(obj => obj.Id == Convert.ToInt32(currentUserProccess.CurrentCallBackProccess.Replace("editCourseName", "")));
                                        currentCourseNameEdit.Name = UpdMsg.Message.Text!;
                                        db.SaveChanges();
                                        logBotAnswer = "Название было успешно изменено";
                                        await bot.SendTextMessageAsync(chatId: UpdMsg.Message.From!.Id,
                                            text: logBotAnswer,
                                            cancellationToken: cts.Token);
                                        logBotAnswer = $"Вы перешли в конструктор редактирования курса\n<b>{currentCourseNameEdit.Name}</b>.";
                                        string isPrivateCourseAnswer = currentCourseNameEdit.IsPrivate == true ? "Курс является приватным" : "Курс не является приватным";
                                        string tokenCourseAnswer = currentCourseNameEdit.Token == null ? "Добавить токен" : $"Токен курса - {currentCourseNameEdit.Token}";
                                        Dictionary<string, string> courseEditChoiceButtons = new()
                                        {
                                            { "editCourseName", $"Название курса ({currentCourseNameEdit.Name})" },
                                            { "editCourseDescription", "Описание курса" },
                                            { "editCourseIsPrivate", $"Приватный курс ({isPrivateCourseAnswer})" },
                                            { "editCourseRequisites", "Реквизиты курса" },
                                            { "editCourseToken", tokenCourseAnswer },
                                            { "editCourseGoBack", "Назад" }
                                        };
                                        var currentCourseEditingInlinePanel = new InlineKeyboardMarkup(HandleTextMessages.GetInlineKeyboard(courseEditChoiceButtons, currentCourseNameEdit.Id.ToString()));
                                        await bot.SendTextMessageAsync(UpdMsg.Message.From!.Id,
                                            text: logBotAnswer,
                                            replyMarkup: currentCourseEditingInlinePanel,
                                            parseMode: ParseMode.Html,
                                            cancellationToken: cts.Token);
                                        currentUserProccess.CurrentCallBackProccess = "EditCourse";
                                    }
                                    if (currentUserProccess.CurrentCallBackProccess!.Contains("editCourseDescription"))
                                    {
                                        var currentCourseDescriptionEdit = db.Courses.First(obj => obj.Id == Convert.ToInt32(currentUserProccess.CurrentCallBackProccess.Replace("editCourseDescription", "")));
                                        currentCourseDescriptionEdit.Description = UpdMsg.Message.Text!;
                                        db.SaveChanges();
                                        db.Update(currentCourseDescriptionEdit);
                                        logBotAnswer = "Описание курса было успешно изменено";
                                        await bot.SendTextMessageAsync(chatId: UpdMsg.Message.From!.Id,
                                            text: logBotAnswer,
                                            cancellationToken: cts.Token);
                                        logBotAnswer = $"Вы перешли в конструктор редактирования курса\n<b>{currentCourseDescriptionEdit.Name}</b>.";
                                        string isPrivateCourseAnswer = currentCourseDescriptionEdit.IsPrivate == true ? "Курс является приватным" : "Курс не является приватным";
                                        string tokenCourseAnswer = currentCourseDescriptionEdit.Token == null ? "Добавить токен" : $"Токен курса - {currentCourseDescriptionEdit.Token}";
                                        Dictionary<string, string> courseEditChoiceButtons = new()
                                        {
                                            { "editCourseName", $"Название курса ({currentCourseDescriptionEdit.Name})" },
                                            { "editCourseDescription", "Описание курса" },
                                            { "editCourseIsPrivate", $"Приватный курс ({isPrivateCourseAnswer})" },
                                            { "editCourseRequisites", "Реквизиты курса" },
                                            { "editCourseToken", tokenCourseAnswer },
                                            { "editCourseGoBack", "Назад" }
                                        };
                                        var currentCourseEditingInlinePanel = new InlineKeyboardMarkup(HandleTextMessages.GetInlineKeyboard(courseEditChoiceButtons, currentCourseDescriptionEdit.Id.ToString()));
                                        await bot.SendTextMessageAsync(UpdMsg.Message.From!.Id,
                                            text: logBotAnswer,
                                            replyMarkup: currentCourseEditingInlinePanel,
                                            parseMode: ParseMode.Html,
                                            cancellationToken: cts.Token);
                                        currentUserProccess.CurrentCallBackProccess = "EditCourse";
                                    }
                                    if (currentUserProccess.CurrentCallBackProccess!.Contains("editCourseRequisites"))
                                    {
                                        var currentCourseRequisitesEdit = db.Courses.First(obj => obj.Id == Convert.ToInt32(currentUserProccess.CurrentCallBackProccess.Replace("editCourseName", "")));
                                        currentCourseRequisitesEdit.Requisites = UpdMsg.Message.Text!;
                                        db.SaveChanges();
                                        db.Update(currentCourseRequisitesEdit);
                                        logBotAnswer = "Реквизиты курса были успешно изменены";
                                        await bot.SendTextMessageAsync(chatId: UpdMsg.Message.From!.Id,
                                            text: logBotAnswer,
                                            cancellationToken: cts.Token);
                                        logBotAnswer = $"Вы перешли в конструктор редактирования курса\n<b>{currentCourseRequisitesEdit.Name}</b>.";
                                        string isPrivateCourseAnswer = currentCourseRequisitesEdit.IsPrivate == true ? "Курс является приватным" : "Курс не является приватным";
                                        string tokenCourseAnswer = currentCourseRequisitesEdit.Token == null ? "Добавить токен" : $"Токен курса - {currentCourseRequisitesEdit.Token}";
                                        Dictionary<string, string> courseEditChoiceButtons = new()
                                        {
                                            { "editCourseName", $"Название курса ({currentCourseRequisitesEdit.Name})" },
                                            { "editCourseDescription", "Описание курса" },
                                            { "editCourseIsPrivate", $"Приватный курс ({isPrivateCourseAnswer})" },
                                            { "editCourseRequisites", "Реквизиты курса" },
                                            { "editCourseToken", tokenCourseAnswer },
                                            { "editCourseGoBack", "Назад" }
                                        };
                                        var currentCourseEditingInlinePanel = new InlineKeyboardMarkup(HandleTextMessages.GetInlineKeyboard(courseEditChoiceButtons, currentCourseRequisitesEdit.Id.ToString()));
                                        await bot.SendTextMessageAsync(UpdMsg.Message.From!.Id,
                                            text: logBotAnswer,
                                            replyMarkup: currentCourseEditingInlinePanel,
                                            parseMode: ParseMode.Html,
                                            cancellationToken: cts.Token);
                                        currentUserProccess.CurrentCallBackProccess = "EditCourse";
                                    }
                                    if (currentUserProccess.CurrentCallBackProccess! == "createCourseName")
                                    {
                                        var currentCourseCreating = HandleCallBacks.tempCreatingCourseObjects.FirstOrDefault(obj => obj.Key == UpdMsg.Message.From!.Id.ToString());
                                        currentCourseCreating.Value.Name = UpdMsg.Message.Text!;
                                        logBotAnswer = "Название курса было успешно изменено.";
                                        await bot.SendTextMessageAsync(chatId: UpdMsg.Message.From!.Id,
                                            text: logBotAnswer,
                                            cancellationToken: cts.Token);
                                        logBotAnswer = $"Вы перешли в конструктор создания курса.\nВыберите пункты на панели ниже для заполнения информации о курсе.\n* - обязательное поле для заполнения.";
                                        Dictionary<string, string> courseCreateChoiceButtons = new()
                                        {
                                            { "createCourseName", currentCourseCreating.Value.Name == null ? "*Название курса" : $"Название курса ({currentCourseCreating.Value.Name})" },
                                            { "createCourseDescription", "Описание курса" },
                                            { "createCourseIsPrivate", currentCourseCreating.Value.IsPrivate == false ? "Сделать курс приватным" : "Сделать курс публичным" },
                                            { "createCourseRequisites", "Реквизиты курса" },
                                            { "createCourseToken", currentCourseCreating.Value.Token == null ? "Добавить токен" : $"Сменить токен ({currentCourseCreating.Value.Token})" },
                                            { "createCourseGoBack", "Назад" },
                                            { "createCourseSaveAndGoBack", "Сохранить и вернуться" }
                                        };
                                        var currentCourseEditingInlinePanel = new InlineKeyboardMarkup(HandleTextMessages.GetInlineKeyboard(courseCreateChoiceButtons, ""));
                                        await bot.SendTextMessageAsync(UpdMsg.Message.From!.Id,
                                            text: logBotAnswer,
                                            replyMarkup: currentCourseEditingInlinePanel,
                                            parseMode: ParseMode.Html,
                                            cancellationToken: cts.Token);
                                        currentUserProccess.CurrentCallBackProccess = "CreateCourse";
                                    }
                                    if (currentUserProccess.CurrentCallBackProccess! == "createCourseDescription")
                                    {
                                        HandleCallBacks handleCallBacks = new();
                                        var currentCourseCreating = HandleCallBacks.tempCreatingCourseObjects.First(obj => obj.Key == UpdMsg.Message.From!.Id.ToString());
                                        currentCourseCreating.Value.Name = UpdMsg.Message.Text!;
                                        logBotAnswer = "Описание курса было успешно изменено.";
                                        await bot.SendTextMessageAsync(chatId: UpdMsg.Message.From!.Id,
                                            text: logBotAnswer,
                                            cancellationToken: cts.Token);
                                        logBotAnswer = $"Вы перешли в конструктор создания курса.\nВыберите пункты на панели ниже для заполнения информации о курсе.\n* - обязательное поле для заполнения.";
                                        Dictionary<string, string> courseCreateChoiceButtons = new()
                                        {
                                            { "createCourseName", currentCourseCreating.Value.Name == null ? "*Название курса" : $"Название курса ({currentCourseCreating.Value.Name})" },
                                            { "createCourseDescription", "Описание курса" },
                                            { "createCourseIsPrivate", currentCourseCreating.Value.IsPrivate == false ? "Сделать курс приватным" : "Сделать курс публичным" },
                                            { "createCourseRequisites", "Реквизиты курса" },
                                            { "createCourseToken", currentCourseCreating.Value.Token == null ? "Добавить токен" : $"Сменить токен ({currentCourseCreating.Value.Token})" },
                                            { "createCourseGoBack", "Назад" },
                                            { "createCourseSaveAndGoBack", "Сохранить и вернуться" }
                                        };
                                        var currentCourseEditingInlinePanel = new InlineKeyboardMarkup(HandleTextMessages.GetInlineKeyboard(courseCreateChoiceButtons, ""));
                                        await bot.SendTextMessageAsync(UpdMsg.Message.From!.Id,
                                            text: logBotAnswer,
                                            replyMarkup: currentCourseEditingInlinePanel,
                                            parseMode: ParseMode.Html,
                                            cancellationToken: cts.Token);
                                        currentUserProccess.CurrentCallBackProccess = "CreateCourse";
                                    }
                                    if (currentUserProccess.CurrentCallBackProccess! == "createCourseRequisites")
                                    {
                                        HandleCallBacks handleCallBacks = new();
                                        var currentCourseCreating = HandleCallBacks.tempCreatingCourseObjects.First(obj => obj.Key == UpdMsg.Message.From!.Id.ToString());
                                        currentCourseCreating.Value.Name = UpdMsg.Message.Text!;
                                        logBotAnswer = "Реквизи курса было успешно изменены.";
                                        await bot.SendTextMessageAsync(chatId: UpdMsg.Message.From!.Id,
                                            text: logBotAnswer,
                                            cancellationToken: cts.Token);
                                        logBotAnswer = $"Вы перешли в конструктор создания курса.\nВыберите пункты на панели ниже для заполнения информации о курсе.\n* - обязательное поле для заполнения.";
                                        Dictionary<string, string> courseCreateChoiceButtons = new()
                                        {
                                            { "createCourseName", currentCourseCreating.Value.Name == null ? "*Название курса" : $"Название курса ({currentCourseCreating.Value.Name})" },
                                            { "createCourseDescription", "Описание курса" },
                                            { "createCourseIsPrivate", currentCourseCreating.Value.IsPrivate == false ? "Сделать курс приватным" : "Сделать курс публичным" },
                                            { "createCourseRequisites", "Реквизиты курса" },
                                            { "createCourseToken", currentCourseCreating.Value.Token == null ? "Добавить токен" : $"Сменить токен ({currentCourseCreating.Value.Token})" },
                                            { "createCourseGoBack", "Назад" },
                                            { "createCourseSaveAndGoBack", "Сохранить и вернуться" }
                                        };
                                        var currentCourseEditingInlinePanel = new InlineKeyboardMarkup(HandleTextMessages.GetInlineKeyboard(courseCreateChoiceButtons, ""));
                                        await bot.SendTextMessageAsync(UpdMsg.Message.From!.Id,
                                            text: logBotAnswer,
                                            replyMarkup: currentCourseEditingInlinePanel,
                                            parseMode: ParseMode.Html,
                                            cancellationToken: cts.Token);
                                        currentUserProccess.CurrentCallBackProccess = "CreateCourse";
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
                        ChatId = "1",
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

        // Метод для динамического формирования inline кнопок.
        public static InlineKeyboardButton[][] GetInlineKeyboard(Dictionary<string, string> keyboardsValues, string callbackName)
        {
            int countKeyboards = (callbackName == "ownCourses" || callbackName == "coursesMember") ? keyboardsValues.Count + 1 : keyboardsValues.Count;
            var keyboardInline = new InlineKeyboardButton[countKeyboards][];
            int j = 0; // Переменная для обозначения итерации цикла.

            foreach (var keyboardsValue in keyboardsValues) // Цикл, шагающий по библиотеке данных о курсах для дальнейшего размещения их на кнопки (1 аргумент).
            {
                var keyboardButtons = new InlineKeyboardButton[1];
                if (keyboardsValue.Key == "GoBack")
                {
                    keyboardButtons[0] = new InlineKeyboardButton(keyboardsValue.Value)
                    {
                        Text = keyboardsValue.Value,
                        CallbackData = keyboardsValue.Key,
                    };
                }
                else
                {
                    keyboardButtons[0] = new InlineKeyboardButton(keyboardsValue.Value)
                    {
                        Text = keyboardsValue.Value,
                        CallbackData = callbackName + keyboardsValue.Key,
                    };
                }
                keyboardInline[j] = keyboardButtons;
                j++;
            }
            if (callbackName == "ownCourses")
            {
                var keyboardButtons = new InlineKeyboardButton[2];
                keyboardButtons[0] = new InlineKeyboardButton("Поиск")
                {
                    Text = "Поиск",
                    CallbackData = "filterOwnCourses"
                };
                keyboardButtons[1] = new InlineKeyboardButton("Создать курс")
                {
                    Text = "Создать курс",
                    CallbackData = "CreateCourse"
                };
                keyboardInline[j] = keyboardButtons;
            }
            if (callbackName == "coursesMember")
            {
                var keyboardButtons = new InlineKeyboardButton[2];
                keyboardButtons[0] = new InlineKeyboardButton("Поиск")
                {
                    Text = "Поиск",
                    CallbackData = "filterCoursesMember"
                };
                keyboardButtons[1] = new InlineKeyboardButton("Присоединиться к курсу по токену")
                {
                    Text = "Присоединиться к курсу по токену",
                    CallbackData = "JoinCourseByToken"
                };
                keyboardInline[j] = keyboardButtons;
            }

            return keyboardInline;
        }
        public static InlineKeyboardButton[][] GetUrlInlineKeyboard(Dictionary<string, string>? keyboardsValues, string callbackName)
        {
            int countKeyboards = (callbackName.Contains("courseOwnAssignment")) ? (keyboardsValues!=null ? keyboardsValues.Count + 3 : 2) : (keyboardsValues!=null ? keyboardsValues.Count + 1 : 1);
            var keyboardInline = new InlineKeyboardButton[countKeyboards][];
            int j = 0; // Переменная для обозначения итерации цикла.
            if (keyboardsValues != null)
            {

                foreach (var keyboardsValue in keyboardsValues) // Цикл, шагающий по библиотеке данных о курсах для дальнейшего размещения их на кнопки (1 аргумент).
                {
                    var keyboardButtons = new InlineKeyboardButton[1];
                    keyboardButtons[0] = new InlineKeyboardButton(keyboardsValue.Value)
                    {
                        Text = keyboardsValue.Value,
                        Url = keyboardsValue.Key
                    };
                    keyboardInline[j] = keyboardButtons;
                    j++;
                }
            }
            if (callbackName.Contains("courseOwnAssignment"))
            {
                var keyboardButtons = new InlineKeyboardButton[2];
                keyboardButtons[0] = new InlineKeyboardButton("Создать тему")
                {
                    Text = "Создать тему",
                    CallbackData = "createCourseAssignment" + callbackName.Replace("courseOwnAssignment", "")
                };
                keyboardButtons[1] = new InlineKeyboardButton("Получить доступ к редактированию с этого устр-ва")
                {
                    Text = "Получить доступ к редактированию с этого устр-ва",
                    CallbackData = "getAssignmentsAccess" + callbackName.Replace("courseOwnAssignment", "")
                };
                keyboardInline[j] = keyboardButtons;
                j++;
                if (keyboardsValues != null)
                {
                    keyboardButtons = new InlineKeyboardButton[1];
                    keyboardButtons[0] = new InlineKeyboardButton("Удалить тему")
                    {
                        Text = "Удалить тему",
                        CallbackData = "courseAssignmentsDeleteList" + callbackName.Replace("courseOwnAssignment", "")
                    };
                    keyboardInline[j] = keyboardButtons;
                    j++;
                }
            }
            var refreshButton = new InlineKeyboardButton[1];
            refreshButton[0] = new InlineKeyboardButton("Обновить")
            {
                Text = "Обновить",
                CallbackData = "refreshCourseAssignments=>" + callbackName
            };
            keyboardInline[j] = refreshButton;
            return keyboardInline;
        }
    }
}
