using CourseManagementBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot;
using Microsoft.EntityFrameworkCore;
using Telegraph.Net.Models;
using Telegraph.Net;

namespace CourseManagementBot
{
    internal class HandleCallBacks
    {
        private string logBotAnswer = ""; // Строка для заполнения информации, которую будет передавать бот.
        private readonly CourseManagementDataContext db = new();
        private readonly Keyboards keyboards = new(); // Базовые клавиатуры для дальнейшего отправления их пользователю для навигации.
        public static Dictionary<string, Course> tempCreatingCourseObjects = new();
        // Порядок действий после того, как пользователь нажал inline кнопку.
        public async Task AnswerCallback(ChattedUser CurrentChattedUser, Update UpdMsg, ITelegramBotClient bot, CancellationTokenSource cts, List<ProccessCallBackUsers> proccessCallBackUsers)
        {
            // Проверка идентификатора нажатой inline кнопки.
            switch (UpdMsg.CallbackQuery!.Data)
            {
                // Идентификатор inline кнопки активации токена на получение роли.
                case "ActivateProfileToken":
                    // Проверяем, не пытается ли пользователь войти в CallBack запрос, когда уже находится в нем.
                    if (proccessCallBackUsers.FirstOrDefault(obj => obj.UserID == UpdMsg.CallbackQuery!.From.Id.ToString()) == null)
                    {
                        // Добавляем пользователя в лист пользователей, находящихся в CallBack запросе.
                        proccessCallBackUsers.Add(new ProccessCallBackUsers
                        {
                            UserID = UpdMsg.CallbackQuery!.From.Id.ToString(),
                            CurrentCallBackProccess = "ActivateProfileToken"
                        });
                        // Удаление inline кнопок в том сообщении, в котором пользватель нажал на одну из этих кнопок.
                        await bot.EditMessageReplyMarkupAsync(UpdMsg.CallbackQuery!.From.Id,
                            messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                            replyMarkup: null,
                            cancellationToken: cts.Token);
                        // Уведомление пользователю о том, что он перешел в процесс CallBack запроса,а также добавление кнопки "Назад".
                        logBotAnswer = "Вы перешли в процесс активации токена.";
                        await bot.SendTextMessageAsync(chatId: UpdMsg.CallbackQuery!.From.Id,
                            text: logBotAnswer,
                            replyMarkup: keyboards.GoBackInlineKeyboard,
                            cancellationToken: cts.Token);
                        Console.WriteLine($" ~ [{DateTime.Now:yyyy-MM-dd HH-mm}] Ответ пользователю @{UpdMsg.CallbackQuery!.From.Username}: {logBotAnswer}");
                        // Просьба пользователю ввести токен.
                        logBotAnswer = "Введите токен:";
                        await bot.SendTextMessageAsync(chatId: UpdMsg.CallbackQuery!.From.Id,
                            text: logBotAnswer,
                            replyMarkup: new ReplyKeyboardRemove(),
                            cancellationToken: cts.Token);
                        Console.WriteLine($" ~ [{DateTime.Now:yyyy-MM-dd HH-mm}] Ответ пользователю @{UpdMsg.CallbackQuery!.From.Username}: {logBotAnswer}");
                        break;
                    }
                    // Пропускает действие, если пользователь не находится в процессе CallBack запроса.
                    else
                        break;
                // Идентификатор inline кнопки возвращения назад (переход на другой CallBack запрос, если он несколькоступенчатый).
                case "GoBack":
                    // Проверяем, не пытается ли пользователь выйти из CallBack запроса, если он не находится ни в одном из них.
                    if (proccessCallBackUsers.FirstOrDefault(obj => obj.UserID == UpdMsg.CallbackQuery!.From.Id.ToString()) != null)
                    {
                        // Удаление пользователя из листа пользователей, находящихся внутри CallBack запроса.
                        proccessCallBackUsers.Remove(proccessCallBackUsers.First(obj => obj.UserID == UpdMsg.CallbackQuery!.From.Id.ToString()));
                        await bot.DeleteMessageAsync(UpdMsg.CallbackQuery!.From.Id,
                            messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                            cancellationToken: cts.Token);
                        // Уведомление пользователю о том, что он вернулся в главное меню, а также возвращение reply кнопок (основной панели навигации по системе).
                        logBotAnswer = "Вы вернулись в главное меню.\nВыберите следующие предложенные пункты для дальнейшей навигации по системе.";
                        await bot.SendTextMessageAsync(chatId: UpdMsg.CallbackQuery!.From.Id,
                            text: logBotAnswer,
                            replyMarkup: keyboards.UserMainReplyKeyboradMarkup,
                            cancellationToken: cts.Token);
                        Console.WriteLine($" ~ [{DateTime.Now:yyyy-MM-dd HH-mm}] Ответ пользователю @{UpdMsg.CallbackQuery!.From.Username}: {logBotAnswer}");
                        break;
                    }
                    // Пропускает действие, если пользователь не находится в процессе CallBack запроса.
                    else
                        break;

                case "JoinCourseByToken":
                    if (proccessCallBackUsers.FirstOrDefault(obj => obj.UserID == UpdMsg.CallbackQuery!.From.Id.ToString()) == null)
                    {
                        proccessCallBackUsers.Add(new ProccessCallBackUsers
                        {
                            UserID = UpdMsg.CallbackQuery!.From.Id.ToString(),
                            CurrentCallBackProccess = "JoinCourseByToken"
                        });
                        // Удаление inline кнопок в том сообщении, в котором пользватель нажал на одну из этих кнопок.
                        await bot.EditMessageReplyMarkupAsync(UpdMsg.CallbackQuery!.From.Id,
                            messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                            replyMarkup: null,
                            cancellationToken: cts.Token);
                        // Уведомление пользователю о том, что он перешел в процесс CallBack запроса,а также добавление кнопки "Назад".
                        logBotAnswer = "Вы перешли в процесс активации токена для подключения к курсу.";
                        await bot.SendTextMessageAsync(chatId: UpdMsg.CallbackQuery!.From.Id,
                            text: logBotAnswer,
                            replyMarkup: keyboards.GoBackInlineKeyboard,
                            cancellationToken: cts.Token);
                        Console.WriteLine($" ~ [{DateTime.Now:yyyy-MM-dd HH-mm}] Ответ пользователю @{UpdMsg.CallbackQuery!.From.Username}: {logBotAnswer}");
                        // Просьба пользователю ввести токен.
                        logBotAnswer = "Введите токен:";
                        await bot.SendTextMessageAsync(chatId: UpdMsg.CallbackQuery!.From.Id,
                            text: logBotAnswer,
                            replyMarkup: new ReplyKeyboardRemove(),
                            cancellationToken: cts.Token);
                        Console.WriteLine($" ~ [{DateTime.Now:yyyy-MM-dd HH-mm}] Ответ пользователю @{UpdMsg.CallbackQuery!.From.Username}: {logBotAnswer}");
                        break;
                    }
                    break;

                case "editCourseElementGoBack":
                    var currentUserInProccess = proccessCallBackUsers.FirstOrDefault(obj => obj.UserID == UpdMsg.CallbackQuery!.From.Id.ToString());
                    if (currentUserInProccess != null)
                    {
                        if (currentUserInProccess.CurrentCallBackProccess!.Contains("editCourse"))
                        {
                            await bot.DeleteMessageAsync(UpdMsg.CallbackQuery!.From.Id,
                                UpdMsg.CallbackQuery.Message!.MessageId,
                                cancellationToken: cts.Token);
                            await bot.DeleteMessageAsync(UpdMsg.CallbackQuery!.From.Id,
                                UpdMsg.CallbackQuery.Message!.MessageId+1,
                                cancellationToken: cts.Token);
                            int currentCourseId;
                            int.TryParse(string.Join("", currentUserInProccess.CurrentCallBackProccess.Where(c => char.IsDigit(c))), out currentCourseId); // позволяет из солянки символов вытащить цифры.
                            var currentEditingCourse = db.Courses.First(obj=>obj.Id == currentCourseId);
                            currentUserInProccess.CurrentCallBackProccess = "EditCourse";
                            logBotAnswer = $"Вы перешли в конструктор редактирования курса\n<b>{currentEditingCourse.Name}</b>.";
                            string isPrivateCourseAnswer = currentEditingCourse.IsPrivate == true ? "Курс является приватным" : "Курс не является приватным";
                            string tokenCourseAnswer = currentEditingCourse.Token == null ? "Добавить токен" : "Токен курса";
                            Dictionary<string, string> courseJoinChoiceButtons = new()
                            {
                                { "editCourseName", $"Название курса ({currentEditingCourse.Name})" },
                                { "editCourseDescription", "Описание курса" },
                                { "editCourseIsPrivate", $"Приватный курс ({isPrivateCourseAnswer})" },
                                { "editCourseRequisites", "Реквизиты курса" },
                                { "editCourseToken", tokenCourseAnswer },
                                { "editCourseGoBack", "Назад" }
                            };
                            var currentCourseEditingInlinePanel = new InlineKeyboardMarkup(HandleTextMessages.GetInlineKeyboard(courseJoinChoiceButtons, currentEditingCourse.Id.ToString()));
                            await bot.SendTextMessageAsync(UpdMsg.CallbackQuery!.From.Id,
                                text: logBotAnswer,
                                replyMarkup: currentCourseEditingInlinePanel,
                                parseMode: ParseMode.Html,
                                cancellationToken: cts.Token);
                            break;
                        }
                        else
                        {
                            var currentCourseCreating = tempCreatingCourseObjects.First(obj => obj.Key == UpdMsg.CallbackQuery!.From.Id.ToString());
                            await bot.DeleteMessageAsync(UpdMsg.CallbackQuery!.From.Id,
                                UpdMsg.CallbackQuery.Message!.MessageId,
                                cancellationToken: cts.Token);
                            await bot.DeleteMessageAsync(UpdMsg.CallbackQuery!.From.Id,
                                UpdMsg.CallbackQuery.Message!.MessageId + 1,
                                cancellationToken: cts.Token);
                            currentUserInProccess.CurrentCallBackProccess = "CreateCourse";
                            logBotAnswer = $"Вы перешли в конструктор создания курса.\nВыберите пункты на панели ниже для заполнения информации о курсе.\n* - обязательное поле для заполнения.";
                            Dictionary<string, string> courseCreateChoiceButtons = new()
                            {
                                { "createCourseName", currentCourseCreating.Value.Name == null ? "*Название курса" : $"Название курса ({currentCourseCreating.Value.Name})" },
                                { "createCourseDescription", "Описание курса" },
                                { "createCourseIsPrivate", currentCourseCreating.Value.IsPrivate == false ? "Сделать курс приватным" : "Сделать курс публичным" },
                                { "createCourseRequisites", "Реквизиты курса" },
                                { "createCourseToken", currentCourseCreating.Value.Token == null ? "Добавить токен" : $"Сменить токен ({currentCourseCreating.Value.Token})" },
                                { "GoBack", "Назад" },
                                { "createCourseSaveAndGoBack", "Сохранить и вернуться" }
                            };
                            var currentCourseEditingInlinePanel = new InlineKeyboardMarkup(HandleTextMessages.GetInlineKeyboard(courseCreateChoiceButtons, ""));
                            await bot.SendTextMessageAsync(UpdMsg.CallbackQuery!.From.Id,
                                text: logBotAnswer,
                                replyMarkup: currentCourseEditingInlinePanel,
                                parseMode: ParseMode.Html,
                                cancellationToken: cts.Token);
                            break;
                        }
                    }
                    break;

                case "CreateCourse":
                    if (proccessCallBackUsers.FirstOrDefault(obj => obj.UserID == UpdMsg.CallbackQuery!.From.Id.ToString()) == null)
                    {
                        // Добавляем пользователя в лист пользователей, находящихся в CallBack запросе.
                        proccessCallBackUsers.Add(new ProccessCallBackUsers
                        {
                            UserID = UpdMsg.CallbackQuery!.From.Id.ToString(),
                            CurrentCallBackProccess = "CreateCourse"
                        });
                        await bot.EditMessageReplyMarkupAsync(UpdMsg.CallbackQuery!.From.Id,
                            messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                            replyMarkup: null,
                            cancellationToken: cts.Token);
                        logBotAnswer = "Переходим в процесс создания курса.";
                        await bot.SendTextMessageAsync(chatId: UpdMsg.CallbackQuery!.From.Id,
                            text: logBotAnswer,
                            replyMarkup: new ReplyKeyboardRemove(),
                            cancellationToken: cts.Token);
                        logBotAnswer = $"Вы перешли в конструктор создания курса.\nВыберите пункты на панели ниже для заполнения информации о курсе.\n* - обязательное поле для заполнения.";
                        tempCreatingCourseObjects.Add(UpdMsg.CallbackQuery!.From.Id.ToString(), new Course());
                        var currentCreatingCourseObj = tempCreatingCourseObjects.First(obj => obj.Key == UpdMsg.CallbackQuery!.From.Id.ToString());
                        currentCreatingCourseObj.Value.IsGlobal = false;
                        currentCreatingCourseObj.Value.PublishDate = DateTime.Now.Date;
                        currentCreatingCourseObj.Value.Curator = UpdMsg.CallbackQuery!.From.Id.ToString();
                        currentCreatingCourseObj.Value.IsPrivate = false;
                        Dictionary<string, string> courseCreateChoiceButtons = new()
                        {
                            { "createCourseName", "*Название курса" },
                            { "createCourseDescription", "Описание курса" },
                            { "createCourseIsPrivate", "Сделать курс приватным" },
                            { "createCourseRequisites", "Реквизиты курса" },
                            { "createCourseToken", "Добавить токен" },
                            { "createCourseGoBack", "Назад" },
                            { "createCourseSaveAndGoBack", "Сохранить курс и вернуться" }
                        };
                        var currentCourseEditingInlinePanel = new InlineKeyboardMarkup(HandleTextMessages.GetInlineKeyboard(courseCreateChoiceButtons, ""));
                        await bot.SendTextMessageAsync(UpdMsg.CallbackQuery!.From.Id,
                            text: logBotAnswer,
                            replyMarkup: currentCourseEditingInlinePanel,
                            parseMode: ParseMode.Html,
                            cancellationToken: cts.Token);
                        break;
                    }
                    break;

                default:
                    // Проверяем, не пытается ли пользователь войти в CallBack запрос, когда уже находится в нем.
                    if (proccessCallBackUsers.FirstOrDefault(obj => obj.UserID == UpdMsg.CallbackQuery!.From.Id.ToString()) == null)
                    {
                        // Проверяем, является ли пользователь, сделавший запрос, куратором курса.
                        if (UpdMsg.CallbackQuery.Data!.Contains("ownCourses"))
                        {
                            // Удаление inline кнопок в том сообщении, в котором пользватель нажал на одну из этих кнопок.
                            await bot.EditMessageReplyMarkupAsync(UpdMsg.CallbackQuery!.From.Id,
                                messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                                replyMarkup: null,
                                cancellationToken: cts.Token);
                            var currentCourse = db.Courses.AsNoTracking().First(obj=>obj.Id==Convert.ToInt32(UpdMsg.CallbackQuery.Data!.Replace("ownCourses", "")));
                            // Проверка и обработка данных курса из БД для дальнейшего вывода информации его владельцу.
                            logBotAnswer = $"<b>{currentCourse.Name}</b>\n<u>{currentCourse.Description ?? "Описание отсутствует"}</u>\n\n";
                            if (currentCourse.IsGlobal)
                                logBotAnswer += $"Это глобальный курс, т.е. может содержать в себе подкурсы.\n";
                            logBotAnswer += $"Дата создания курса: {currentCourse.PublishDate}\n";
                            if (currentCourse.EndDate != null)
                                logBotAnswer += $"Дата окончания курса: {currentCourse.EndDate}\n";
                            if (currentCourse.IsPrivate)
                                logBotAnswer += "Курс является приватным\n";
                            else
                                logBotAnswer += "Курс не является приватным\n";
                            if (currentCourse.MembersCount != null)
                                logBotAnswer += $"Ограниченное количество участников: {db.CourseUsers.AsNoTracking().Where(obj => obj.Course == currentCourse.Id)} из {currentCourse.MembersCount}\n";
                            else
                                logBotAnswer += "Неограниченное количество участников\n";
                            if (currentCourse.Token != null)
                                logBotAnswer += $"Токен для подключения к курсу: {currentCourse.Token}";
                            else
                                logBotAnswer += "Токен для подключения к курсу отсутствует.";
                            int admittedUsers = db.CourseUsers.AsNoTracking().Where(obj => obj.Course == Convert.ToInt32(UpdMsg.CallbackQuery.Data!.Replace("ownCourses", "")) && obj.Admitted == true).Count();
                            int nonAdmittedUsers = db.CourseUsers.AsNoTracking().Where(obj => obj.Course == Convert.ToInt32(UpdMsg.CallbackQuery.Data!.Replace("ownCourses", "")) && obj.Admitted == false).Count();
                            // Создание библиотеки с данными, которые нужно будет отобразить на кнопках (если нужно больше кнопок, добавлять больше полей).
                            // Нужно это для дальнейшей отправки в метод динамического формирования inline. Почему так, а не через базовый конструктор клавитауры?
                            // Потому что для понимания следующего CallBack запроса на редактирование конкретного курса, вызваным пользователем, для inline кнопок отправляется идентификатор выбранного курса и идентификатор inline кнопок
                            // Так мы сможем понять, какую кнопку для управления курсом он нажал конкретно и каким курсом он хочет управлять.
                            Dictionary<string, string> courseManageButtons = new()
                            {
                                { "courseOwnAssignmentsList", "Темы" },
                                { "EditCourse", "Редактировать курс" },
                                { "DeleteCourse", "Удалить курс" },
                                { "CourseMemberList", $"Список участников ({admittedUsers})" },
                                { "CourseMemberRequests", $"Заявки на вступление ({nonAdmittedUsers})" }
                            };
                            // Отправка библиотеки в метод динамического формирования inline кнопок.
                            var currentCourseInlineKeyboard = new InlineKeyboardMarkup(HandleTextMessages.GetInlineKeyboard(courseManageButtons, UpdMsg.CallbackQuery.Data!.Replace("ownCourses", "")));
                            
                            await bot.EditMessageTextAsync(UpdMsg.CallbackQuery!.From.Id,
                                messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                                text: logBotAnswer,
                                replyMarkup: currentCourseInlineKeyboard,
                                parseMode: ParseMode.Html,
                                cancellationToken: cts.Token);
                            break;
                        }

                        // Проверяем, является ли пользователь, сделавший запрос, участником курса.
                        if (UpdMsg.CallbackQuery.Data!.Contains("coursesMember"))
                        {
                            // Удаление inline кнопок в том сообщении, в котором пользватель нажал на одну из этих кнопок.
                            await bot.EditMessageReplyMarkupAsync(UpdMsg.CallbackQuery!.From.Id,
                                messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                                replyMarkup: null,
                                cancellationToken: cts.Token);
                            var currentCourse = db.Courses.AsNoTracking().First(obj => obj.Id == Convert.ToInt32(UpdMsg.CallbackQuery.Data!.Replace("coursesMember", "")));
                            // Проверка и обработка данных курса из БД для дальнейшего вывода информации его владельцу.
                            logBotAnswer = $"<b>{currentCourse.Name}</b>\n<u>{currentCourse.Description ?? "Описание отсутствует"}</u>\n\n";
                            if (currentCourse.IsGlobal)
                                logBotAnswer += $"Это глобальный курс, т.е. может содержать в себе подкурсы.\n";
                            logBotAnswer += $"Дата создания курса: {currentCourse.PublishDate}\n";
                            if (currentCourse.EndDate != null)
                                logBotAnswer += $"Дата окончания курса: {currentCourse.EndDate}\n";
                            if (currentCourse.IsPrivate)
                                logBotAnswer += "Курс является приватным\n";
                            else
                                logBotAnswer += "Курс не является приватным\n";
                            if (currentCourse.MembersCount != null)
                                logBotAnswer += $"Ограниченное количество участников: {db.CourseUsers.AsNoTracking().Where(obj => obj.Course == currentCourse.Id)} из {currentCourse.MembersCount}\n";
                            else
                                logBotAnswer += "Неограниченное количество участников\n";
                            Dictionary<string, string> courseManageButtons;
                            // Проверка роли пользователя. Если это помощник куратора, то система даст ему возможность редактировать сведения о курсе.
                            if (db.CourseUsers.First(obj=>obj.Course==currentCourse.Id && obj.PinnedUser==UpdMsg.CallbackQuery!.From.Id.ToString()).CourseUserRole == "Помощник куратора")
                            {
                                if (currentCourse.Token != null)
                                    logBotAnswer += $"Токен для подключения к курсу: {currentCourse.Token}";
                                else
                                    logBotAnswer += "Токен для подключения к курсу отсутствует.";
                                int admittedUsers = db.CourseUsers.AsNoTracking().Where(obj => obj.Course == Convert.ToInt32(UpdMsg.CallbackQuery.Data!.Replace("coursesMember", "")) && obj.Admitted == false).Count();
                                int nonAdmittedUsers = db.CourseUsers.AsNoTracking().Where(obj => obj.Course == Convert.ToInt32(UpdMsg.CallbackQuery.Data!.Replace("coursesMember", "")) && obj.Admitted == true).Count();
                                // Создание библиотеки с данными, которые нужно будет отобразить на кнопках (если нужно больше кнопок, добавлять больше полей).
                                // Нужно это для дальнейшей отправки в метод динамического формирования inline. Почему так, а не через базовый конструктор клавитауры?
                                // Потому что для понимания следующего CallBack запроса на редактирование конкретного курса, вызваным пользователем, для inline кнопок отправляется идентификатор выбранного курса и идентификатор inline кнопок
                                // Так мы сможем понять, какую кнопку для управления курсом он нажал конкретно и каким курсом он хочет управлять.
                                courseManageButtons = new()
                                {
                                    { "courseMemberAssignmentsList", "Темы" },
                                    { "CourseMemberRequests", $"Заявки на вступление ({admittedUsers})" },
                                    { "CourseMemberList", $"Список участников ({nonAdmittedUsers})" },
                                    { "LeaveCourse", "Покинуть курс" }
                                };
                            }
                            else
                            {
                                int admittedUsers = db.CourseUsers.AsNoTracking().Where(obj => obj.Course == Convert.ToInt32(UpdMsg.CallbackQuery.Data!.Replace("coursesMember", "")) && obj.Admitted == false).Count();
                                courseManageButtons = new()
                                {
                                    { "courseMemberAssignmentsList", "Темы" },
                                    { "CourseMemberList", $"Список участников ({admittedUsers})" },
                                    { "LeaveCourse", "Покинуть курс" }
                                };
                            }
                            // Отправка библиотеки в метод динамического формирования inline кнопок.
                            var currentCourseInlineKeyboard = new InlineKeyboardMarkup(HandleTextMessages.GetInlineKeyboard(courseManageButtons, UpdMsg.CallbackQuery.Data!.Replace("coursesMember", "")));

                            await bot.EditMessageTextAsync(UpdMsg.CallbackQuery!.From.Id,
                                messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                                text: logBotAnswer,
                                replyMarkup: currentCourseInlineKeyboard,
                                parseMode: ParseMode.Html,
                                cancellationToken: cts.Token);
                            break;
                        }

                        if (UpdMsg.CallbackQuery.Data!.Contains("filterOwnCourses") || UpdMsg.CallbackQuery.Data!.Contains("filterCoursesMember"))
                        {
                            if (proccessCallBackUsers.FirstOrDefault(obj => obj.UserID == UpdMsg.CallbackQuery!.From.Id.ToString()) == null)
                            {
                                switch (UpdMsg.CallbackQuery.Data!)
                                {
                                    case "filterOwnCourses":
                                        // Добавляем пользователя в лист пользователей, находящихся в CallBack запросе.
                                        proccessCallBackUsers.Add(new ProccessCallBackUsers
                                        {
                                            UserID = UpdMsg.CallbackQuery!.From.Id.ToString(),
                                            CurrentCallBackProccess = "filterOwnCourses"
                                        });
                                        break;
                                    case "filterCoursesMember":
                                        // Добавляем пользователя в лист пользователей, находящихся в CallBack запросе.
                                        proccessCallBackUsers.Add(new ProccessCallBackUsers
                                        {
                                            UserID = UpdMsg.CallbackQuery!.From.Id.ToString(),
                                            CurrentCallBackProccess = "filterCoursesMember"
                                        });
                                        break;
                                }
                                // Удаление inline кнопок в том сообщении, в котором пользватель нажал на одну из этих кнопок.
                                await bot.EditMessageReplyMarkupAsync(UpdMsg.CallbackQuery!.From.Id,
                                    messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                                    replyMarkup: null,
                                    cancellationToken: cts.Token);
                                // Уведомление пользователю о том, что он перешел в процесс CallBack запроса,а также добавление кнопки "Назад".
                                logBotAnswer = "Вы перешли в процесс поиска.";
                                await bot.EditMessageTextAsync(UpdMsg.CallbackQuery!.From.Id,
                                    messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                                    text: logBotAnswer,
                                    replyMarkup: keyboards.GoBackInlineKeyboard,
                                    cancellationToken: cts.Token);
                                Console.WriteLine($" ~ [{DateTime.Now:yyyy-MM-dd HH-mm}] Ответ пользователю @{UpdMsg.CallbackQuery!.From.Username}: {logBotAnswer}");
                                // Просьба пользователю ввести токен.
                                logBotAnswer = "Введите ключевое слово (учтите - чем длиннее запрос, тем меньше результатов):";
                                await bot.SendTextMessageAsync(chatId: UpdMsg.CallbackQuery!.From.Id,
                                    text: logBotAnswer,
                                    replyMarkup: new ReplyKeyboardRemove(),
                                    cancellationToken: cts.Token);
                                Console.WriteLine($" ~ [{DateTime.Now:yyyy-MM-dd HH-mm}] Ответ пользователю @{UpdMsg.CallbackQuery!.From.Username}: {logBotAnswer}");
                                break;
                            }
                            // Пропускает действие, если пользователь не находится в процессе CallBack запроса.
                            else
                                break;
                        }

                        if (UpdMsg.CallbackQuery.Data!.Contains("LeaveCourse"))
                        {
                            if (proccessCallBackUsers.FirstOrDefault(obj => obj.UserID == UpdMsg.CallbackQuery!.From.Id.ToString()) == null)
                            {
                                await bot.EditMessageReplyMarkupAsync(UpdMsg.CallbackQuery!.From.Id,
                                    messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                                    replyMarkup: null,
                                    cancellationToken: cts.Token);
                                // Добавляем пользователя в лист пользователей, находящихся в CallBack запросе.
                                proccessCallBackUsers.Add(new ProccessCallBackUsers
                                {
                                    UserID = UpdMsg.CallbackQuery!.From.Id.ToString(),
                                    CurrentCallBackProccess = "LeaveCourse"
                                });
                                string currentCourseName = db.Courses.First(obj => obj.Id == Convert.ToInt32(UpdMsg.CallbackQuery.Data!.Replace("LeaveCourse", ""))).Name;
                                logBotAnswer = $"Вы уверены, что хотите покинуть курс <b>{currentCourseName}</b>?";
                                Dictionary<string, string> courseLeaveChoiceButtons = new()
                                {
                                    { "acceptLeaveCourse", "Да" },
                                    { "GoBack", "Нет" }
                                };
                                var courseLeaveChoiceInlineKeyboard = new InlineKeyboardMarkup(HandleTextMessages.GetInlineKeyboard(courseLeaveChoiceButtons, UpdMsg.CallbackQuery!.Data!.Replace("LeaveCourse", "")));
                                await bot.EditMessageTextAsync(UpdMsg.CallbackQuery!.From.Id,
                                    messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                                    text: logBotAnswer,
                                    replyMarkup: courseLeaveChoiceInlineKeyboard,
                                    parseMode: ParseMode.Html,
                                    cancellationToken: cts.Token);
                                break;
                            }
                        }

                        if (UpdMsg.CallbackQuery.Data!.Contains("DeleteCourse"))
                        {
                            if (proccessCallBackUsers.FirstOrDefault(obj => obj.UserID == UpdMsg.CallbackQuery!.From.Id.ToString()) == null)
                            {
                                await bot.EditMessageReplyMarkupAsync(UpdMsg.CallbackQuery!.From.Id,
                                    messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                                    replyMarkup: null,
                                    cancellationToken: cts.Token);
                                // Добавляем пользователя в лист пользователей, находящихся в CallBack запросе.
                                proccessCallBackUsers.Add(new ProccessCallBackUsers
                                {
                                    UserID = UpdMsg.CallbackQuery!.From.Id.ToString(),
                                    CurrentCallBackProccess = "DeleteCourse"
                                });
                                string currentCourseName = db.Courses.First(obj => obj.Id == Convert.ToInt32(UpdMsg.CallbackQuery.Data!.Replace("DeleteCourse", ""))).Name;
                                logBotAnswer = $"Вы уверены, что хотите удалить курс <b>{currentCourseName}</b>?";
                                Dictionary<string, string> courseLeaveChoiceButtons = new()
                                {
                                    { "acceptDeleteCourse", "Да" },
                                    { "GoBack", "Нет" }
                                };
                                var courseLeaveChoiceInlineKeyboard = new InlineKeyboardMarkup(HandleTextMessages.GetInlineKeyboard(courseLeaveChoiceButtons, UpdMsg.CallbackQuery!.Data!.Replace("DeleteCourse", "")));
                                await bot.EditMessageTextAsync(UpdMsg.CallbackQuery!.From.Id,
                                    messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                                    text: logBotAnswer,
                                    replyMarkup: courseLeaveChoiceInlineKeyboard,
                                    parseMode: ParseMode.Html,
                                    cancellationToken: cts.Token);
                                break;
                            }
                        }

                        if (UpdMsg.CallbackQuery.Data!.Contains("EditCourse"))
                        {
                            if (proccessCallBackUsers.FirstOrDefault(obj => obj.UserID == UpdMsg.CallbackQuery!.From.Id.ToString()) == null)
                            {
                                // Добавляем пользователя в лист пользователей, находящихся в CallBack запросе.
                                proccessCallBackUsers.Add(new ProccessCallBackUsers
                                {
                                    UserID = UpdMsg.CallbackQuery!.From.Id.ToString(),
                                    CurrentCallBackProccess = "EditCourse"
                                });
                                await bot.EditMessageReplyMarkupAsync(UpdMsg.CallbackQuery!.From.Id,
                                    messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                                    replyMarkup: null,
                                    cancellationToken: cts.Token);
                                logBotAnswer = "Переходим в процесс редактирования курса.";
                                await bot.SendTextMessageAsync(chatId: UpdMsg.CallbackQuery!.From.Id,
                                    text: logBotAnswer,
                                    replyMarkup: new ReplyKeyboardRemove(),
                                    cancellationToken: cts.Token);
                                var currentEditingCourse = db.Courses.First(obj => obj.Id == Convert.ToInt32(UpdMsg.CallbackQuery.Data!.Replace("EditCourse", "")));
                                logBotAnswer = $"Вы перешли в конструктор редактирования курса\n<b>{currentEditingCourse.Name}</b>.";
                                string isPrivateCourseAnswer = currentEditingCourse.IsPrivate == true ? "Курс является приватным" : "Курс не является приватным";
                                string tokenCourseAnswer = currentEditingCourse.Token == null ? "Добавить токен" : $"Токен курса - {currentEditingCourse.Token}";
                                Dictionary<string, string> courseEditChoiceButtons = new()
                                {
                                    { "editCourseName", $"Название курса ({currentEditingCourse.Name})" },
                                    { "editCourseDescription", "Описание курса" },
                                    { "editCourseIsPrivate", $"Приватный курс ({isPrivateCourseAnswer})" },
                                    { "editCourseRequisites", "Реквизиты курса" },
                                    { "editCourseToken", tokenCourseAnswer },
                                    { "editCourseGoBack", "Назад" }
                                };
                                var currentCourseEditingInlinePanel = new InlineKeyboardMarkup(HandleTextMessages.GetInlineKeyboard(courseEditChoiceButtons, currentEditingCourse.Id.ToString()));
                                await bot.SendTextMessageAsync(UpdMsg.CallbackQuery!.From.Id,
                                    text: logBotAnswer,
                                    replyMarkup: currentCourseEditingInlinePanel,
                                    parseMode: ParseMode.Html,
                                    cancellationToken: cts.Token);
                                break;
                            }
                        }

                        if (UpdMsg.CallbackQuery.Data!.Contains("courseOwnAssignmentsList"))
                        {
                            await bot.EditMessageReplyMarkupAsync(UpdMsg.CallbackQuery!.From.Id,
                                messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                                replyMarkup: null,
                                cancellationToken: cts.Token);
                            var currentCourseAssignments = db.CourseAssignments.Where(obj => obj.PinnedCourse == Convert.ToInt32(UpdMsg.CallbackQuery.Data!.Replace("courseOwnAssignmentsList", "")));
                            if (currentCourseAssignments.Count() != 0)
                            {
                                var telegraphClient = new TelegraphClient();
                                foreach (var currentCourseAssignment in currentCourseAssignments)
                                {
                                    Page page = await telegraphClient.GetPageAsync(currentCourseAssignment.Description!.Replace("https://telegra.ph/", ""), returnContent: true);
                                    if (currentCourseAssignment.Name != page.Title)
                                        currentCourseAssignment.Name = page.Title;
                                }
                                db.SaveChanges();
                                var refreshedCourseAssignments = db.CourseAssignments.AsNoTracking().Where(obj => obj.PinnedCourse == Convert.ToInt32(UpdMsg.CallbackQuery.Data!.Replace("courseOwnAssignmentsList", "")));
                                logBotAnswer = "Для возможности редактирования своих тем нажмите кнопку \"Получить доступ к редактированию с этого устр-ва\"\n\nСписок тем вашего курса:";
                                var keyboardMarkup = new InlineKeyboardMarkup(HandleTextMessages.GetUrlInlineKeyboard(refreshedCourseAssignments.ToDictionary(obj => obj.Description!, obj => obj.Name), refreshedCourseAssignments.First().PinnedCourse.ToString()+"courseOwnAssignment"));
                                await bot.SendTextMessageAsync(
                                    chatId: UpdMsg.CallbackQuery!.From.Id,
                                    text: logBotAnswer,
                                    replyMarkup: keyboardMarkup,
                                    parseMode: ParseMode.Html,
                                    cancellationToken: cts.Token);
                            }
                            else
                            {
                                logBotAnswer = "У данного курса нет ни одной темы.";
                                var keyboardMarkup = new InlineKeyboardMarkup(HandleTextMessages.GetUrlInlineKeyboard(null, UpdMsg.CallbackQuery.Data!.Replace("courseOwnAssignmentsList", "") + "courseOwnAssignment"));
                                await bot.SendTextMessageAsync(
                                    chatId: UpdMsg.CallbackQuery!.From.Id,
                                    text: logBotAnswer,
                                    replyMarkup: keyboardMarkup,
                                    cancellationToken: cts.Token);
                            }
                            break;
                        }

                        if (UpdMsg.CallbackQuery.Data!.Contains("courseMemberAssignmentsList"))
                        {
                            await bot.EditMessageReplyMarkupAsync(UpdMsg.CallbackQuery!.From.Id,
                                messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                                replyMarkup: null,
                                cancellationToken: cts.Token);
                            var currentCourseAssignments = db.CourseAssignments.Where(obj => obj.PinnedCourse == Convert.ToInt32(UpdMsg.CallbackQuery.Data!.Replace("courseMemberAssignmentsList", "")));
                            if (currentCourseAssignments.Count() != 0)
                            {
                                var telegraphClient = new TelegraphClient();
                                foreach (var currentCourseAssignment in currentCourseAssignments)
                                {
                                    Page page = await telegraphClient.GetPageAsync(currentCourseAssignment.Description!.Replace("https://telegra.ph/", ""), returnContent: true);
                                    if (currentCourseAssignment.Name != page.Title)
                                        currentCourseAssignment.Name = page.Title;
                                }
                                db.SaveChanges();
                                var refreshedCourseAssignments = db.CourseAssignments.AsNoTracking().Where(obj => obj.PinnedCourse == Convert.ToInt32(UpdMsg.CallbackQuery.Data!.Replace("courseMemberAssignmentsList", "")));
                                logBotAnswer = "Список тем выбранного курса:";
                                var keyboardMarkup = new InlineKeyboardMarkup(HandleTextMessages.GetUrlInlineKeyboard(refreshedCourseAssignments.ToDictionary(obj => obj.Description!, obj => obj.Name), refreshedCourseAssignments.First().PinnedCourse.ToString()+"courseMemberAssignment"));
                                await bot.SendTextMessageAsync(
                                    chatId: UpdMsg.CallbackQuery!.From.Id,
                                    text: logBotAnswer,
                                    replyMarkup: keyboardMarkup,
                                    parseMode: ParseMode.Html,
                                    cancellationToken: cts.Token);
                            }
                            else
                            {
                                logBotAnswer = "У данного курса нет ни одной темы.";
                                var keyboardMarkup = new InlineKeyboardMarkup(HandleTextMessages.GetUrlInlineKeyboard(null, UpdMsg.CallbackQuery.Data!.Replace("courseMemberAssignmentsList", "") + "courseMemberAssignment"));
                                await bot.SendTextMessageAsync(
                                    chatId: UpdMsg.CallbackQuery!.From.Id,
                                    text: logBotAnswer,
                                    replyMarkup: keyboardMarkup,
                                    cancellationToken: cts.Token);
                            }
                            break;
                        }

                        if (UpdMsg.CallbackQuery.Data!.Contains("createCourseAssignment"))
                        {
                            await bot.EditMessageReplyMarkupAsync(UpdMsg.CallbackQuery!.From.Id,
                                messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                                replyMarkup: null,
                                cancellationToken: cts.Token);
                            logBotAnswer = "Пожалуйста, подождите. Тема создается";
                            await bot.EditMessageTextAsync(UpdMsg.CallbackQuery.From.Id,
                                UpdMsg.CallbackQuery.Message.MessageId,
                                text: logBotAnswer,
                                cancellationToken: cts.Token);
                            var client = new TelegraphClient();
                            ITokenClient tokenClient;
                            if (db.ChattedUsers.First(obj=>obj.Id == UpdMsg.CallbackQuery.From.Id.ToString()).ChatId == "1")
                            {
                                Account tgPhAccount = await client.CreateAccountAsync(UpdMsg.CallbackQuery.From.Username ?? UpdMsg.CallbackQuery.From.FirstName, null, "http://t.me/VladEkbDevBot");
                                tokenClient = client.GetTokenClient(tgPhAccount.AccessToken);
                                db.ChattedUsers.First(obj => obj.Id == UpdMsg.CallbackQuery.From.Id.ToString()).ChatId = tgPhAccount.AccessToken;
                                db.SaveChanges();
                            }
                            else
                                tokenClient = client.GetTokenClient(db.ChattedUsers.First(obj => obj.Id == UpdMsg.CallbackQuery.From.Id.ToString()).ChatId);
                            var nodes = new List<NodeElement>();
                            nodes.Add(
                              new NodeElement("p",
                                  null /* no attribute */,
                                  "Содержимое статьи"
                              )
                            );
                            NodeElement[] nodesArray = nodes.ToArray();
                            string pageTitleSymbols = "aA1bBcC2dDeE3fFgG4hHiI5jJkK6lLmM7nNoO8pPqQ9rRsS0tTuU1vVwW2xXyY3zZ";
                            var rand = new Random();
                            string pageTitle = "";
                            for (int i = 1; i <= 15; i++)
                                pageTitle += pageTitleSymbols[rand.Next(pageTitleSymbols.Length - 1)].ToString();
                            // Create a new Page
                            Page newPage = await tokenClient.CreatePageAsync(
                              pageTitle,
                              nodesArray /* NodeElement[] */,
                              returnContent: true
                            );
                            string currentPageUrl = newPage.Url;
                            newPage = await tokenClient.EditPageAsync(currentPageUrl.Replace("https://telegra.ph/", ""),
                                "Пустая статья",
                                nodesArray);
                            CourseAssignment newCourseAssignment = new CourseAssignment
                            {
                                Name = newPage.Title,
                                PinnedCourse = Convert.ToInt32(UpdMsg.CallbackQuery.Data.Replace("createCourseAssignment", "")),
                                Description = newPage.Url
                            };
                            db.CourseAssignments.Add(newCourseAssignment);
                            db.SaveChanges();
                            var currentCourseAssignments = db.CourseAssignments.AsNoTracking().Where(obj => obj.PinnedCourse == Convert.ToInt32(UpdMsg.CallbackQuery.Data!.Replace("createCourseAssignment", "")));
                            var telegraphClient = new TelegraphClient();
                            foreach (var currentCourseAssignment in currentCourseAssignments)
                            {
                                Page page = await telegraphClient.GetPageAsync(currentCourseAssignment.Description!.Replace("https://telegra.ph/", ""), returnContent: true);
                                if (currentCourseAssignment.Name != page.Title)
                                    currentCourseAssignment.Name = page.Title;
                            }
                            db.SaveChanges();
                            var keyboardMarkup = new InlineKeyboardMarkup(HandleTextMessages.GetUrlInlineKeyboard(currentCourseAssignments.ToDictionary(obj => obj.Description!, obj => obj.Name), currentCourseAssignments.First().PinnedCourse.ToString() + "courseOwnAssignment"));
                            logBotAnswer = "Тема была успешно добавлена к вашему курсу.\n\nДля возможности редактирования своих тем нажмите кнопку \"Получить доступ к редактированию с этого устр-ва\"\n\nСписок тем вашего курса.";
                            await bot.EditMessageTextAsync(UpdMsg.CallbackQuery.From.Id,
                                UpdMsg.CallbackQuery.Message.MessageId,
                                text: logBotAnswer,
                                cancellationToken: cts.Token);
                            await bot.EditMessageReplyMarkupAsync(UpdMsg.CallbackQuery.From.Id,
                                UpdMsg.CallbackQuery.Message.MessageId,
                                keyboardMarkup,
                                cts.Token);
                            break;
                        }

                        if (UpdMsg.CallbackQuery.Data!.Contains("getAssignmentsAccess"))
                        {
                            logBotAnswer = "Пожалуйста, подождите. Происходит сброс со всех устройств.";
                            var currentMsgToEdit = await bot.SendTextMessageAsync(UpdMsg.CallbackQuery.From.Id,
                                text: logBotAnswer,
                                cancellationToken: cts.Token);
                            if (db.ChattedUsers.First(obj => obj.Id == UpdMsg.CallbackQuery.From.Id.ToString()).ChatId != "1")
                            {
                                var client = new TelegraphClient();
                                var currentUser = db.ChattedUsers.First(obj => obj.Id == UpdMsg.CallbackQuery.From.Id.ToString());
                                ITokenClient tokenClient = client.GetTokenClient(currentUser.ChatId);
                                var newAccountToken = await tokenClient.RevokeAccessTokenAsync();
                                currentUser.ChatId = newAccountToken.AccessToken;
                                db.SaveChanges();
                                InlineKeyboardMarkup getAccessUrlButton = new(new []
                                {
                                    InlineKeyboardButton.WithUrl(
                                        text: "Получить доступ с этого устройства",
                                        url: newAccountToken.AuthorizationUrl)
                                });
                                logBotAnswer = "Ваша ссылка была успешно создана!\nУчтите, что все прошлые сессии будут сброшены.\n\nДанная ссылка действует всего один раз и только первые 5 минут.";
                                await bot.EditMessageTextAsync(UpdMsg.CallbackQuery.From.Id,
                                    currentMsgToEdit.MessageId,
                                    text: logBotAnswer,
                                    replyMarkup: getAccessUrlButton,
                                    cancellationToken: cts.Token);
                            }
                            else
                            {
                                logBotAnswer = "Ошибка: вы не создали ни одной темы для ваших курсов.";
                                await bot.EditMessageTextAsync(UpdMsg.CallbackQuery.From.Id,
                                    currentMsgToEdit.MessageId,
                                    text: logBotAnswer,
                                    cancellationToken: cts.Token);
                            }
                            break;
                        }

                        if (UpdMsg.CallbackQuery.Data!.Contains("refreshCourseAssignments"))
                        {
                            await bot.EditMessageReplyMarkupAsync(UpdMsg.CallbackQuery!.From.Id,
                                messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                                replyMarkup: null,
                                cancellationToken: cts.Token);
                            logBotAnswer = "Пожалуйста, подождите. Список тем данного курса обновляется.";
                            await bot.EditMessageTextAsync(UpdMsg.CallbackQuery.From.Id,
                                UpdMsg.CallbackQuery.Message.MessageId,
                                text: logBotAnswer,
                                cancellationToken: cts.Token);
                            if (UpdMsg.CallbackQuery.Data!.Split("=>")[1].Contains("courseOwnAssignment"))
                            {
                                int currentCourseID = Convert.ToInt32(UpdMsg.CallbackQuery.Data!.Split("=>")[1].Replace("courseOwnAssignment", ""));
                                var currentCourseAssignments = db.CourseAssignments.Where(obj => obj.PinnedCourse == currentCourseID);
                                if (currentCourseAssignments.Count() != 0)
                                {
                                    var telegraphClient = new TelegraphClient();
                                    foreach (var currentCourseAssignment in currentCourseAssignments)
                                    {
                                        Page page = await telegraphClient.GetPageAsync(currentCourseAssignment.Description!.Replace("https://telegra.ph/", ""), returnContent: true);
                                        if (currentCourseAssignment.Name != page.Title)
                                            currentCourseAssignment.Name = page.Title;
                                    }
                                    db.SaveChanges();
                                    var refreshedCourseAssignments = db.CourseAssignments.AsNoTracking().Where(obj => obj.PinnedCourse == currentCourseID);
                                    logBotAnswer = "Список тем данного курса был успешно обновлен.\n\nДля возможности редактирования своих тем нажмите кнопку \"Получить доступ к редактированию с этого устр-ва\"\n\nСписок тем вашего курса:";
                                    var keyboardMarkup = new InlineKeyboardMarkup(HandleTextMessages.GetUrlInlineKeyboard(refreshedCourseAssignments.ToDictionary(obj => obj.Description!, obj => obj.Name), refreshedCourseAssignments.First().PinnedCourse.ToString() + "courseOwnAssignment"));
                                    await bot.EditMessageTextAsync(
                                        chatId: UpdMsg.CallbackQuery!.From.Id,
                                        messageId: UpdMsg.CallbackQuery.Message.MessageId,
                                        text: logBotAnswer,
                                        replyMarkup: keyboardMarkup,
                                        parseMode: ParseMode.Html,
                                        cancellationToken: cts.Token);
                                }
                                else
                                {
                                    logBotAnswer = "Список тем данного курса был успешно обновлен.\n\nУ данного курса нет ни одной темы.";
                                    var keyboardMarkup = new InlineKeyboardMarkup(HandleTextMessages.GetUrlInlineKeyboard(null, currentCourseID.ToString() + "courseOwnAssignment"));
                                    await bot.EditMessageTextAsync(
                                        chatId: UpdMsg.CallbackQuery!.From.Id,
                                        messageId: UpdMsg.CallbackQuery.Message.MessageId,
                                        text: logBotAnswer,
                                        replyMarkup: keyboardMarkup,
                                        cancellationToken: cts.Token);
                                }
                            }
                            else
                            {
                                int currentCourseID = Convert.ToInt32(UpdMsg.CallbackQuery.Data!.Split("=>")[1].Replace("courseMemberAssignment", ""));
                                var currentCourseAssignments = db.CourseAssignments.Where(obj => obj.PinnedCourse == currentCourseID);
                                if (currentCourseAssignments.Count() != 0)
                                {
                                    var telegraphClient = new TelegraphClient();
                                    foreach (var currentCourseAssignment in currentCourseAssignments)
                                    {
                                        Page page = await telegraphClient.GetPageAsync(currentCourseAssignment.Description!.Replace("https://telegra.ph/", ""), returnContent: true);
                                        if (currentCourseAssignment.Name != page.Title)
                                            currentCourseAssignment.Name = page.Title;
                                    }
                                    db.SaveChanges();
                                    var refreshedCourseAssignments = db.CourseAssignments.AsNoTracking().Where(obj => obj.PinnedCourse == currentCourseID);
                                    logBotAnswer = "Список тем данного курса был успешно обновлен.\n\nСписок тем выбранного курса:";
                                    var keyboardMarkup = new InlineKeyboardMarkup(HandleTextMessages.GetUrlInlineKeyboard(refreshedCourseAssignments.ToDictionary(obj => obj.Description!, obj => obj.Name), refreshedCourseAssignments.First().PinnedCourse.ToString() + "courseMemberAssignment"));
                                    await bot.EditMessageTextAsync(
                                        chatId: UpdMsg.CallbackQuery!.From.Id,
                                        messageId: UpdMsg.CallbackQuery.Message.MessageId,
                                        text: logBotAnswer,
                                        replyMarkup: keyboardMarkup,
                                        parseMode: ParseMode.Html,
                                        cancellationToken: cts.Token);
                                }
                                else
                                {
                                    logBotAnswer = "У данного курса нет ни одной темы.";
                                    var keyboardMarkup = new InlineKeyboardMarkup(HandleTextMessages.GetUrlInlineKeyboard(null, currentCourseID.ToString() + "courseMemberAssignment"));
                                    await bot.EditMessageTextAsync(
                                        chatId: UpdMsg.CallbackQuery!.From.Id,
                                        messageId: UpdMsg.CallbackQuery.Message.MessageId,
                                        text: logBotAnswer,
                                        replyMarkup: keyboardMarkup,
                                        cancellationToken: cts.Token);
                                }
                            }
                            break;
                        }

                        if (UpdMsg.CallbackQuery.Data!.Contains("courseAssignmentsDeleteList"))
                        {
                            await bot.DeleteMessageAsync(UpdMsg.CallbackQuery!.From.Id,
                                messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                                cancellationToken: cts.Token);
                            ProccessCallBackUsers proccessCallBackUser = new ProccessCallBackUsers
                            {
                                UserID = UpdMsg.CallbackQuery!.From.Id.ToString(),
                                CurrentCallBackProccess = "courseAssignmentsDeleteList"
                            };
                            proccessCallBackUsers.Add(proccessCallBackUser);
                            logBotAnswer = "Вы перешли в процесс удаления темы курса.";
                            await bot.SendTextMessageAsync(UpdMsg.CallbackQuery!.From.Id,
                                text: logBotAnswer,
                                replyMarkup: new ReplyKeyboardRemove(),
                                cancellationToken: cts.Token);
                            var currentCourseAssignmentsToDelete = db.CourseAssignments.AsNoTracking().Where(obj=>obj.PinnedCourse == Convert.ToInt32(UpdMsg.CallbackQuery.Data!.Replace("courseAssignmentsDeleteList", ""))).ToDictionary(obj=>obj.Id.ToString(), obj=>obj.Name);
                            currentCourseAssignmentsToDelete.Add("GoBack", "Назад");
                            var keyboard = new InlineKeyboardMarkup(HandleTextMessages.GetInlineKeyboard(currentCourseAssignmentsToDelete, "courseAssignmentToDelete"));
                            logBotAnswer = "Выберите один из ваших тем из списка для его удаления:";
                            await bot.SendTextMessageAsync(UpdMsg.CallbackQuery!.From.Id,
                                text: logBotAnswer,
                                replyMarkup: keyboard,
                                parseMode: ParseMode.Html,
                                cancellationToken: cts.Token);
                            break;
                        }

                        if (UpdMsg.CallbackQuery.Data!.Contains("CourseMemberList"))
                        {
                            var currentCourseMembersList = db.CourseUsers.AsNoTracking().Where(obj => obj.Course == Convert.ToInt32(UpdMsg.CallbackQuery.Data.Replace("CourseMemberList", "")) && obj.Admitted == true);
                            if (currentCourseMembersList.Count() != 0)
                            {
                                logBotAnswer = "Список участников:\n\n";
                                int j = 1;
                                var allChattedUsers = db.ChattedUsers.ToList();
                                foreach (var currentCourseUser in currentCourseMembersList)
                                {
                                    string currentUsername = allChattedUsers.First(obj=>obj.Id == currentCourseUser.PinnedUser).Name;
                                    logBotAnswer += $"{j}. Пользователь: {currentUsername} | Роль: {currentCourseUser.CourseUserRole} | Дата вступления: {currentCourseUser.LeavingDate.Date}\n\n";
                                    j++;
                                }
                                await bot.SendTextMessageAsync(UpdMsg.CallbackQuery!.From.Id,
                                    text: logBotAnswer,
                                    cancellationToken: cts.Token);
                                break;
                            }
                            break;
                        }

                        if (UpdMsg.CallbackQuery.Data!.Contains("CourseMemberRequests"))
                        {
                            var courseUserRequests = db.CourseUsers.AsNoTracking().Where(obj => obj.Course == Convert.ToInt32(UpdMsg.CallbackQuery.Data.Replace("CourseMemberRequests", "")) && obj.Admitted == false);
                            if (courseUserRequests.Count() != 0)
                            {
                                await bot.DeleteMessageAsync(UpdMsg.CallbackQuery!.From.Id,
                                    messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                                    cancellationToken: cts.Token);
                                ProccessCallBackUsers proccessCallBackUser = new ProccessCallBackUsers
                                {
                                    UserID = UpdMsg.CallbackQuery!.From.Id.ToString(),
                                    CurrentCallBackProccess = "CourseMemberRequests"
                                };
                                proccessCallBackUsers.Add(proccessCallBackUser);
                                string courseName = db.Courses.First(obj => obj.Id == Convert.ToInt32(UpdMsg.CallbackQuery.Data.Replace("CourseMemberRequests", ""))).Name;
                                logBotAnswer = $"Вы перешли в процесс управления заявками на вступление в курс \"{courseName}\".";
                                await bot.SendTextMessageAsync(UpdMsg.CallbackQuery!.From.Id,
                                    text: logBotAnswer,
                                    replyMarkup: new ReplyKeyboardRemove(),
                                    cancellationToken: cts.Token);
                                Dictionary<string, string> userRequestsDicButtons = new();
                                var allChattedUsers = db.ChattedUsers.ToList();
                                foreach (var userRequest in courseUserRequests)
                                    userRequestsDicButtons.Add(userRequest.PinnedUser, allChattedUsers.First(obj=>obj.Id == userRequest.PinnedUser).Name);
                                userRequestsDicButtons.Add("GoBack", "Назад");
                                var keyboard = new InlineKeyboardMarkup(HandleTextMessages.GetInlineKeyboard(userRequestsDicButtons, UpdMsg.CallbackQuery.Data.Replace("CourseMemberRequests", "") + "=>acceptUserCourseJoinRequest"));
                                logBotAnswer = "Нажмите на одного из списка заявок пользователя, чтобы принять его в курс:";
                                await bot.SendTextMessageAsync(UpdMsg.CallbackQuery!.From.Id,
                                    text: logBotAnswer,
                                    replyMarkup: keyboard,
                                    cancellationToken: cts.Token);
                                break;
                            }
                            break;
                        }
                    }

                    else
                    {
                        if (proccessCallBackUsers.First(obj => obj.UserID == UpdMsg.CallbackQuery!.From.Id.ToString()).CurrentCallBackProccess == "DeleteCourse")
                        {
                            if (UpdMsg.CallbackQuery.Data!.Contains("acceptDeleteCourse"))
                            {
                                await bot.EditMessageReplyMarkupAsync(UpdMsg.CallbackQuery!.From.Id,
                                    messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                                    replyMarkup: null,
                                    cancellationToken: cts.Token);
                                logBotAnswer = "Вы успешно удалили курс!";
                                var choosedDeleteCourse = db.Courses.First(obj => obj.Id == Convert.ToInt32(UpdMsg.CallbackQuery!.Data!.Replace("acceptDeleteCourse", "")));
                                db.Courses.Remove(choosedDeleteCourse);
                                db.SaveChanges();
                                await bot.EditMessageTextAsync(UpdMsg.CallbackQuery!.From.Id,
                                    messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                                    text: logBotAnswer,
                                    cancellationToken: cts.Token);
                                var currentUserProccess = proccessCallBackUsers.First(obj => obj.UserID == UpdMsg.CallbackQuery!.From.Id.ToString());
                                proccessCallBackUsers.Remove(currentUserProccess);
                                break;
                            }
                        }
                        if (proccessCallBackUsers.First(obj => obj.UserID == UpdMsg.CallbackQuery!.From.Id.ToString()).CurrentCallBackProccess == "JoinCourseByToken")
                        {
                            if (UpdMsg.CallbackQuery.Data!.Contains("acceptJoinCourse"))
                            {
                                string courseToken = UpdMsg.CallbackQuery.Data!.Replace("acceptJoinCourse", "").Split('-')[1];
                                ActiveToken courseTokenData = db.ActiveTokens.First(obj => obj.Token == courseToken);
                                courseTokenData.UsedAttempts++;
                                CourseUser newCourseMember;
                                int currentCourseID = Convert.ToInt32(UpdMsg.CallbackQuery.Data!.Replace("acceptJoinCourse", "").Split('-')[0]);
                                var currentCourse = db.Courses.First(obj => obj.Id == currentCourseID);
                                if (currentCourse.IsPrivate == false)
                                {
                                    newCourseMember = new CourseUser
                                    {
                                        Course = Convert.ToInt32(UpdMsg.CallbackQuery.Data!.Replace("acceptJoinCourse", "").Split('-')[0]),
                                        PinnedUser = UpdMsg.CallbackQuery!.From.Id.ToString(),
                                        CourseUserRole = "Участник",
                                        Admitted = true,
                                        LeavingDate = DateTime.Now.Date
                                    };
                                    logBotAnswer = $"{char.ConvertFromUtf32(0x2705)} Вы успешно подписались на курс <b>\"{db.Courses.First(obj => obj.Token == courseToken).Name}\"</b>!";
                                }
                                else
                                {
                                    newCourseMember = new CourseUser
                                    {
                                        Course = Convert.ToInt32(UpdMsg.CallbackQuery.Data!.Replace("acceptJoinCourse", "").Split('-')[0]),
                                        PinnedUser = UpdMsg.CallbackQuery!.From.Id.ToString(),
                                        CourseUserRole = "Участник",
                                        Admitted = false,
                                        LeavingDate = DateTime.Now.Date
                                    };
                                    logBotAnswer = $"{char.ConvertFromUtf32(0x2705)} Вы успешно подали заявку на курс <b>\"{db.Courses.First(obj => obj.Token == courseToken).Name}\"</b>!";
                                }
                                db.CourseUsers.Add(newCourseMember);
                                var currentUserProccess = proccessCallBackUsers.First(obj => obj.UserID == UpdMsg.CallbackQuery!.From.Id.ToString());
                                proccessCallBackUsers.Remove(currentUserProccess);
                                await bot.SendTextMessageAsync(chatId: UpdMsg.CallbackQuery!.From.Id,
                                    text: logBotAnswer,
                                    replyMarkup: keyboards.UserMainReplyKeyboradMarkup,
                                    parseMode: ParseMode.Html,
                                    cancellationToken: cts.Token);
                                if (courseTokenData.MaxUsesNumber == courseTokenData.UsedAttempts)
                                {
                                    Console.WriteLine($" ~ Пользователь @{UpdMsg.CallbackQuery!.From.Username ?? UpdMsg.CallbackQuery!.From.FirstName} совершил последнюю активацию токена. Удаляем токен из БД.");
                                    db.ActiveTokens.Remove(courseTokenData); // Удаление токена в случае, если лимит был достигнут.
                                    Console.WriteLine(" ~ Токен был успешно удалён.");
                                }
                                db.SaveChanges();
                                break;
                            }
                        }
                        if (proccessCallBackUsers.First(obj => obj.UserID == UpdMsg.CallbackQuery!.From.Id.ToString()).CurrentCallBackProccess == "LeaveCourse")
                        {
                            if (UpdMsg.CallbackQuery.Data!.Contains("acceptLeaveCourse"))
                            {
                                await bot.EditMessageReplyMarkupAsync(UpdMsg.CallbackQuery!.From.Id,
                                    messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                                    replyMarkup: null,
                                    cancellationToken: cts.Token);
                                var currentCourseMemberToDelete = db.CourseUsers.First(obj => obj.Course == Convert.ToInt32(UpdMsg.CallbackQuery.Data!.Replace("acceptLeaveCourse", "")) && obj.PinnedUser == UpdMsg.CallbackQuery!.From.Id.ToString());
                                db.CourseUsers.Remove(currentCourseMemberToDelete);
                                db.SaveChanges();
                                logBotAnswer = "Вы успешно покинули курс!";
                                await bot.EditMessageTextAsync(UpdMsg.CallbackQuery!.From.Id,
                                    messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                                    text: logBotAnswer,
                                    cancellationToken: cts.Token);
                                var currentUserProccess = proccessCallBackUsers.First(obj => obj.UserID == UpdMsg.CallbackQuery!.From.Id.ToString());
                                proccessCallBackUsers.Remove(currentUserProccess);
                                break;
                            }
                        }
                        if (proccessCallBackUsers.First(obj => obj.UserID == UpdMsg.CallbackQuery!.From.Id.ToString()).CurrentCallBackProccess == "EditCourse")
                        {
                            if (UpdMsg.CallbackQuery.Data!.Contains("editCourseName"))
                            {
                                var currentUserProccess = proccessCallBackUsers.First(obj => obj.UserID == UpdMsg.CallbackQuery!.From.Id.ToString());
                                var currentEditingCourse = db.Courses.AsNoTracking().First(obj => obj.Id == Convert.ToInt32(UpdMsg.CallbackQuery.Data!.Replace("editCourseName", "")));
                                currentUserProccess.CurrentCallBackProccess = "editCourseName" + currentEditingCourse.Id.ToString();
                                await bot.EditMessageReplyMarkupAsync(UpdMsg.CallbackQuery!.From.Id,
                                    messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                                    replyMarkup: null,
                                    cancellationToken: cts.Token);
                                logBotAnswer = $"Вы перешли в процесс редактирования названия курса \n<b>{currentEditingCourse.Name}</b>.";
                                await bot.EditMessageTextAsync(UpdMsg.CallbackQuery!.From.Id,
                                    messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                                    text: logBotAnswer,
                                    replyMarkup: keyboards.EditCourseElementGoBack,
                                    parseMode: ParseMode.Html,
                                    cancellationToken: cts.Token);
                                logBotAnswer = $"Ваше текущее название курса - \"<b>{currentEditingCourse.Name}</b>\"\n\nУчтите, что после ввода новых изменений, они моментально вступят в силу.\n\nВведите новое название курса:";
                                await bot.SendTextMessageAsync(UpdMsg.CallbackQuery!.From.Id,
                                    text: logBotAnswer,
                                    parseMode: ParseMode.Html,
                                    cancellationToken: cts.Token);
                                break;
                            }
                            if (UpdMsg.CallbackQuery.Data!.Contains("editCourseDescription"))
                            {
                                var currentUserProccess = proccessCallBackUsers.First(obj => obj.UserID == UpdMsg.CallbackQuery!.From.Id.ToString());
                                var currentEditingCourse = db.Courses.AsNoTracking().First(obj => obj.Id == Convert.ToInt32(UpdMsg.CallbackQuery.Data!.Replace("editCourseDescription", "")));
                                currentUserProccess.CurrentCallBackProccess = "editCourseDescription" + currentEditingCourse.Id.ToString();
                                await bot.EditMessageReplyMarkupAsync(UpdMsg.CallbackQuery!.From.Id,
                                    messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                                    replyMarkup: null,
                                    cancellationToken: cts.Token);
                                logBotAnswer = $"Вы перешли в процесс редактирования описания курса <b>{currentEditingCourse.Name}</b>.";
                                await bot.EditMessageTextAsync(UpdMsg.CallbackQuery!.From.Id,
                                    messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                                    text: logBotAnswer,
                                    replyMarkup: keyboards.EditCourseElementGoBack,
                                    parseMode: ParseMode.Html,
                                    cancellationToken: cts.Token);
                                logBotAnswer = $"Ваше текущее описание курса:\n\"<u>{currentEditingCourse.Description ?? "Описание отутствует"}</u>\"\n\nУчтите, что после ввода новых изменений, они моментально вступят в силу.\n\nВведите новое описание курса:";
                                await bot.SendTextMessageAsync(UpdMsg.CallbackQuery!.From.Id,
                                    text: logBotAnswer,
                                    parseMode: ParseMode.Html,
                                    cancellationToken: cts.Token);
                                break;
                            }
                            if (UpdMsg.CallbackQuery.Data!.Contains("editCourseRequisites"))
                            {
                                var currentUserProccess = proccessCallBackUsers.First(obj => obj.UserID == UpdMsg.CallbackQuery!.From.Id.ToString());
                                var currentEditingCourse = db.Courses.AsNoTracking().First(obj => obj.Id == Convert.ToInt32(UpdMsg.CallbackQuery.Data!.Replace("editCourseRequisites", "")));
                                currentUserProccess.CurrentCallBackProccess = "editCourseRequisites" + currentEditingCourse.Id.ToString();
                                await bot.EditMessageReplyMarkupAsync(UpdMsg.CallbackQuery!.From.Id,
                                    messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                                    replyMarkup: null,
                                    cancellationToken: cts.Token);
                                logBotAnswer = $"Вы перешли в процесс редактирования реквизитов курса <b>{currentEditingCourse.Name}</b>.";
                                await bot.EditMessageTextAsync(UpdMsg.CallbackQuery!.From.Id,
                                    messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                                    text: logBotAnswer,
                                    replyMarkup: keyboards.EditCourseElementGoBack,
                                    parseMode: ParseMode.Html,
                                    cancellationToken: cts.Token);
                                logBotAnswer = $"Ваши текущие реквизиты курса:\n\"<u>{currentEditingCourse.Requisites ?? "Реквизиты отсутствуют"}</u>\"\n\nРеквизиты нужны для поиска финансовой поддержки и дальнейшего продвижения вашего курса среди участников.\nУчтите, что после ввода новых изменений, они моментально вступят в силу.\n\nВведите новые реквизиты курса:";
                                await bot.SendTextMessageAsync(UpdMsg.CallbackQuery!.From.Id,
                                    text: logBotAnswer,
                                    parseMode: ParseMode.Html,
                                    cancellationToken: cts.Token);
                                break;
                            }
                            if (UpdMsg.CallbackQuery.Data!.Contains("editCourseIsPrivate"))
                            {
                                var currentEditingCourse = db.Courses.First(obj => obj.Id == Convert.ToInt32(UpdMsg.CallbackQuery.Data!.Replace("editCourseIsPrivate", "")));
                                await bot.EditMessageReplyMarkupAsync(UpdMsg.CallbackQuery!.From.Id,
                                    messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                                    replyMarkup: null,
                                    cancellationToken: cts.Token);
                                currentEditingCourse.IsPrivate = currentEditingCourse.IsPrivate == true ? false : true;
                                db.SaveChanges();
                                //db.Update(currentEditingCourse);
                                string isPrivateCourseAnswer = currentEditingCourse.IsPrivate == true ? "Курс является приватным" : "Курс не является приватным";
                                string tokenCourseAnswer = currentEditingCourse.Token == null ? "Добавить токен" : $"Токен курса - {currentEditingCourse.Token}";
                                Dictionary<string, string> courseJoinChoiceButtons = new()
                                {
                                    { "editCourseName", $"Название курса ({currentEditingCourse.Name})" },
                                    { "editCourseDescription", "Описание курса" },
                                    { "editCourseIsPrivate", $"Приватный курс ({isPrivateCourseAnswer})" },
                                    { "editCourseRequisites", "Реквизиты курса" },
                                    { "editCourseToken", tokenCourseAnswer },
                                    { "editCourseGoBack", "Назад" }
                                };
                                var currentCourseEditingInlinePanel = new InlineKeyboardMarkup(HandleTextMessages.GetInlineKeyboard(courseJoinChoiceButtons, currentEditingCourse.Id.ToString()));
                                await bot.EditMessageReplyMarkupAsync(UpdMsg.CallbackQuery!.From.Id,
                                    messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                                    replyMarkup: currentCourseEditingInlinePanel,
                                    cancellationToken: cts.Token);
                                break;
                            }
                            if (UpdMsg.CallbackQuery.Data!.Contains("editCourseToken"))
                            {
                                var currentEditingCourse = db.Courses.First(obj => obj.Id == Convert.ToInt32(UpdMsg.CallbackQuery.Data!.Replace("editCourseToken", "")));
                                await bot.EditMessageReplyMarkupAsync(UpdMsg.CallbackQuery!.From.Id,
                                    messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                                    replyMarkup: null,
                                    cancellationToken: cts.Token);
                                if (currentEditingCourse.Token != null)
                                    db.ActiveTokens.Remove(db.ActiveTokens.First(obj=>obj.Token==currentEditingCourse.Token));
                                string tokenSymbols = "aA1bBcC2dDeE3fFgG4hHiI5jJkK6lLmM7nNoO8pPqQ9rRsS0tTuU1vVwW2xXyY3zZ";
                                var rand = new Random();
                                string newToken = "";
                                for (int i = 1; i <= 10; i++)
                                    newToken += tokenSymbols[rand.Next(tokenSymbols.Length - 1)].ToString();
                                currentEditingCourse.Token = newToken;
                                ActiveToken newActiveToken = new ActiveToken
                                {
                                    Token = newToken,
                                    TokenType = "Присоединение к курсу",
                                    MaxUsesNumber = 0,
                                    UsedAttempts = 0
                                };
                                db.ActiveTokens.Add(newActiveToken);
                                db.SaveChanges();
                                string isPrivateCourseAnswer = currentEditingCourse.IsPrivate == true ? "Курс является приватным" : "Курс не является приватным";
                                string tokenCourseAnswer = currentEditingCourse.Token == null ? "Добавить токен" : $"Токен курса - {currentEditingCourse.Token}";
                                Dictionary<string, string> courseJoinChoiceButtons = new()
                                {
                                    { "editCourseName", $"Название курса ({currentEditingCourse.Name})" },
                                    { "editCourseDescription", "Описание курса" },
                                    { "editCourseIsPrivate", $"Приватный курс ({isPrivateCourseAnswer})" },
                                    { "editCourseRequisites", "Реквизиты курса" },
                                    { "editCourseToken", tokenCourseAnswer },
                                    { "editCourseGoBack", "Назад" }
                                };
                                var currentCourseEditingInlinePanel = new InlineKeyboardMarkup(HandleTextMessages.GetInlineKeyboard(courseJoinChoiceButtons, currentEditingCourse.Id.ToString()));
                                await bot.EditMessageReplyMarkupAsync(UpdMsg.CallbackQuery!.From.Id,
                                    messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                                    replyMarkup: currentCourseEditingInlinePanel,
                                    cancellationToken: cts.Token);
                                break;
                            }
                            if (UpdMsg.CallbackQuery.Data!.Contains("editCourseGoBack"))
                            {
                                var currentUserProccess = proccessCallBackUsers.First(obj => obj.UserID == UpdMsg.CallbackQuery!.From.Id.ToString());
                                var currentCourse = db.Courses.AsNoTracking().First(obj => obj.Id == Convert.ToInt32(UpdMsg.CallbackQuery.Data!.Replace("editCourseGoBack", "")));
                                proccessCallBackUsers.Remove(currentUserProccess);
                                await bot.EditMessageReplyMarkupAsync(UpdMsg.CallbackQuery!.From.Id,
                                    messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                                    replyMarkup: null,
                                    cancellationToken: cts.Token);
                                // Проверка и обработка данных курса из БД для дальнейшего вывода информации его владельцу.
                                logBotAnswer = $"<b>{currentCourse.Name}</b>\n<u>{currentCourse.Description ?? "Описание отсутствует"}</u>\n\n";
                                if (currentCourse.IsGlobal)
                                    logBotAnswer += $"Это глобальный курс, т.е. может содержать в себе подкурсы.\n";
                                logBotAnswer += $"Дата создания курса: {currentCourse.PublishDate}\n";
                                if (currentCourse.EndDate != null)
                                    logBotAnswer += $"Дата окончания курса: {currentCourse.EndDate}\n";
                                if (currentCourse.IsPrivate)
                                    logBotAnswer += "Курс является приватным\n";
                                else
                                    logBotAnswer += "Курс не является приватным\n";
                                if (currentCourse.MembersCount != null)
                                    logBotAnswer += $"Ограниченное количество участников: {db.CourseUsers.Where(obj => obj.Course == currentCourse.Id)} из {currentCourse.MembersCount}\n";
                                else
                                    logBotAnswer += "Неограниченное количество участников\n";
                                if (currentCourse.Token != null)
                                    logBotAnswer += $"Токен для подключения к курсу: {currentCourse.Token}";
                                else
                                    logBotAnswer += "Токен для подключения к курсу отсутствует.";
                                // Создание библиотеки с данными, которые нужно будет отобразить на кнопках (если нужно больше кнопок, добавлять больше полей).
                                // Нужно это для дальнейшей отправки в метод динамического формирования inline. Почему так, а не через базовый конструктор клавитауры?
                                // Потому что для понимания следующего CallBack запроса на редактирование конкретного курса, вызваным пользователем, для inline кнопок отправляется идентификатор выбранного курса и идентификатор inline кнопок
                                // Так мы сможем понять, какую кнопку для управления курсом он нажал конкретно и каким курсом он хочет управлять.
                                Dictionary<string, string> courseManageButtons = new()
                                {
                                    { "EditCourse", "Редактировать курс" },
                                    { "DeleteCourse", "Удалить курс" },
                                    { "CourseMemberList", "Список участников" },
                                    { "CourseMemberRequests", "Заявки на вступление" }
                                };
                                // Отправка библиотеки в метод динамического формирования inline кнопок.
                                var currentCourseInlineKeyboard = new InlineKeyboardMarkup(HandleTextMessages.GetInlineKeyboard(courseManageButtons, UpdMsg.CallbackQuery.Data!.Replace("editCourseGoBack", "")));

                                await bot.EditMessageTextAsync(UpdMsg.CallbackQuery!.From.Id,
                                    messageId: UpdMsg.CallbackQuery!.Message.MessageId,
                                    text: logBotAnswer,
                                    replyMarkup: currentCourseInlineKeyboard,
                                    parseMode: ParseMode.Html,
                                    cancellationToken: cts.Token);
                                logBotAnswer = "Вы вышли из конструктора.";
                                await bot.SendTextMessageAsync(UpdMsg.CallbackQuery!.From.Id,
                                    text: logBotAnswer,
                                    replyMarkup: keyboards.UserMainReplyKeyboradMarkup,
                                    cancellationToken: cts.Token);
                                break;
                            }
                        }
                        if (proccessCallBackUsers.First(obj => obj.UserID == UpdMsg.CallbackQuery!.From.Id.ToString()).CurrentCallBackProccess == "CreateCourse")
                        {
                            if (UpdMsg.CallbackQuery.Data! == "createCourseName")
                            {
                                var currentUserProccess = proccessCallBackUsers.First(obj => obj.UserID == UpdMsg.CallbackQuery!.From.Id.ToString());
                                currentUserProccess.CurrentCallBackProccess = "createCourseName";
                                await bot.EditMessageReplyMarkupAsync(UpdMsg.CallbackQuery!.From.Id,
                                    messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                                    replyMarkup: null,
                                    cancellationToken: cts.Token);
                                logBotAnswer = "Вы перешли в процесс редактирования названия курса.";
                                await bot.EditMessageTextAsync(UpdMsg.CallbackQuery!.From.Id,
                                    messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                                    text: logBotAnswer,
                                    replyMarkup: keyboards.EditCourseElementGoBack,
                                    parseMode: ParseMode.Html,
                                    cancellationToken: cts.Token);
                                logBotAnswer = "Введите название курса:";
                                await bot.SendTextMessageAsync(UpdMsg.CallbackQuery!.From.Id,
                                    text: logBotAnswer,
                                    parseMode: ParseMode.Html,
                                    cancellationToken: cts.Token);
                                break;
                            }
                            if (UpdMsg.CallbackQuery.Data! == "createCourseDescription")
                            {
                                var currentUserProccess = proccessCallBackUsers.First(obj => obj.UserID == UpdMsg.CallbackQuery!.From.Id.ToString());
                                currentUserProccess.CurrentCallBackProccess = "createCourseDescription";
                                await bot.EditMessageReplyMarkupAsync(UpdMsg.CallbackQuery!.From.Id,
                                    messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                                    replyMarkup: null,
                                    cancellationToken: cts.Token);
                                logBotAnswer = "Вы перешли в процесс редактирования описания курса.";
                                await bot.EditMessageTextAsync(UpdMsg.CallbackQuery!.From.Id,
                                    messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                                    text: logBotAnswer,
                                    replyMarkup: keyboards.EditCourseElementGoBack,
                                    parseMode: ParseMode.Html,
                                    cancellationToken: cts.Token);
                                logBotAnswer = "Введите описание курса:";
                                await bot.SendTextMessageAsync(UpdMsg.CallbackQuery!.From.Id,
                                    text: logBotAnswer,
                                    parseMode: ParseMode.Html,
                                    cancellationToken: cts.Token);
                                break;
                            }
                            if (UpdMsg.CallbackQuery.Data! == "createCourseRequisites")
                            {
                                var currentUserProccess = proccessCallBackUsers.First(obj => obj.UserID == UpdMsg.CallbackQuery!.From.Id.ToString());
                                currentUserProccess.CurrentCallBackProccess = "createCourseRequisites";
                                await bot.EditMessageReplyMarkupAsync(UpdMsg.CallbackQuery!.From.Id,
                                    messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                                    replyMarkup: null,
                                    cancellationToken: cts.Token);
                                logBotAnswer = "Вы перешли в процесс редактирования реквизитов курса.\n\nРеквизиты нужны для поиска финансовой поддержки и дальнейшего продвижения вашего курса среди участников.";
                                await bot.EditMessageTextAsync(UpdMsg.CallbackQuery!.From.Id,
                                    messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                                    text: logBotAnswer,
                                    replyMarkup: keyboards.EditCourseElementGoBack,
                                    parseMode: ParseMode.Html,
                                    cancellationToken: cts.Token);
                                logBotAnswer = "Введите реквизиты курса:";
                                await bot.SendTextMessageAsync(UpdMsg.CallbackQuery!.From.Id,
                                    text: logBotAnswer,
                                    parseMode: ParseMode.Html,
                                    cancellationToken: cts.Token);
                                break;
                            }
                            if (UpdMsg.CallbackQuery.Data! == "createCourseIsPrivate")
                            {
                                await bot.EditMessageReplyMarkupAsync(UpdMsg.CallbackQuery!.From.Id,
                                    messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                                    replyMarkup: null,
                                    cancellationToken: cts.Token);
                                var currentCourseCreating = tempCreatingCourseObjects.First(obj => obj.Key == UpdMsg.CallbackQuery!.From.Id.ToString());
                                currentCourseCreating.Value.IsPrivate = currentCourseCreating.Value.IsPrivate == true ? false : true;
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
                                await bot.EditMessageReplyMarkupAsync(UpdMsg.CallbackQuery!.From.Id,
                                    messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                                    replyMarkup: currentCourseEditingInlinePanel,
                                    cancellationToken: cts.Token);
                                break;
                            }
                            if (UpdMsg.CallbackQuery.Data! == "createCourseToken")
                            {
                                await bot.EditMessageReplyMarkupAsync(UpdMsg.CallbackQuery!.From.Id,
                                    messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                                    replyMarkup: null,
                                    cancellationToken: cts.Token);
                                var currentCourseCreating = tempCreatingCourseObjects.First(obj => obj.Key == UpdMsg.CallbackQuery!.From.Id.ToString());
                                string tokenSymbols = "aA1bBcC2dDeE3fFgG4hHiI5jJkK6lLmM7nNoO8pPqQ9rRsS0tTuU1vVwW2xXyY3zZ";
                                var rand = new Random();
                                string newToken = "";
                                for (int i = 1; i <= 10; i++)
                                    newToken += tokenSymbols[rand.Next(tokenSymbols.Length - 1)].ToString();
                                currentCourseCreating.Value.Token = newToken;
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
                                await bot.EditMessageReplyMarkupAsync(UpdMsg.CallbackQuery!.From.Id,
                                    messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                                    replyMarkup: currentCourseEditingInlinePanel,
                                    cancellationToken: cts.Token);
                                break;
                            }
                            if (UpdMsg.CallbackQuery.Data! == "createCourseSaveAndGoBack")
                            {
                                var currentCourseCreating = tempCreatingCourseObjects.First(obj => obj.Key == UpdMsg.CallbackQuery!.From.Id.ToString());
                                if (currentCourseCreating.Value.Name != null)
                                {
                                    if (currentCourseCreating.Value.Token != null)
                                    {
                                        ActiveToken newActiveToken = new()
                                        {
                                            Token = currentCourseCreating.Value.Token,
                                            TokenType = "Присоединение к курсу",
                                            MaxUsesNumber = 0,
                                            UsedAttempts = 0
                                        };
                                        db.ActiveTokens.Add(newActiveToken);
                                        db.SaveChanges();
                                    }
                                    db.Courses.Add(currentCourseCreating.Value);
                                    db.SaveChanges();
                                    tempCreatingCourseObjects.Remove(UpdMsg.CallbackQuery!.From.Id.ToString());
                                    await bot.DeleteMessageAsync(UpdMsg.CallbackQuery!.From.Id,
                                        messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                                        cancellationToken: cts.Token);
                                    logBotAnswer = "Ваш новый курс был успешно добавлен!\n\nВы вернулись в главное меню.";
                                    await bot.SendTextMessageAsync(chatId: UpdMsg.CallbackQuery!.From.Id,
                                        text: logBotAnswer,
                                        replyMarkup: keyboards.UserMainReplyKeyboradMarkup,
                                        cancellationToken: cts.Token);
                                    var currentUserProccess = proccessCallBackUsers.First(obj => obj.UserID == UpdMsg.CallbackQuery!.From.Id.ToString());
                                    proccessCallBackUsers.Remove(currentUserProccess);
                                }
                                else
                                {
                                    logBotAnswer = "Ошибка: Вам необходимо заполнить название курса, т.к. это поле является обязательным.\n\nВы перешли в конструктор создания курса.\nВыберите пункты на панели ниже для заполнения информации о курсе.\n* - обязательное поле для заполнения.";
                                    await bot.EditMessageTextAsync(UpdMsg.CallbackQuery!.From.Id,
                                        messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                                        text: logBotAnswer,
                                        parseMode: ParseMode.Html,
                                        cancellationToken: cts.Token);
                                }
                                break;
                            }
                            if (UpdMsg.CallbackQuery.Data! == "createCourseGoBack")
                            {
                                tempCreatingCourseObjects.Remove(UpdMsg.CallbackQuery!.From.Id.ToString());
                                proccessCallBackUsers.Remove(proccessCallBackUsers.First(obj => obj.UserID == UpdMsg.CallbackQuery!.From.Id.ToString()));
                                await bot.DeleteMessageAsync(UpdMsg.CallbackQuery!.From.Id,
                                    messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                                    cancellationToken: cts.Token);
                                // Уведомление пользователю о том, что он вернулся в главное меню, а также возвращение reply кнопок (основной панели навигации по системе).
                                logBotAnswer = "Вы вернулись в главное меню.\nВыберите следующие предложенные пункты для дальнейшей навигации по системе.";
                                await bot.SendTextMessageAsync(chatId: UpdMsg.CallbackQuery!.From.Id,
                                    text: logBotAnswer,
                                    replyMarkup: keyboards.UserMainReplyKeyboradMarkup,
                                    cancellationToken: cts.Token);
                                break;
                            }
                            break;
                        }
                        if (proccessCallBackUsers.First(obj => obj.UserID == UpdMsg.CallbackQuery!.From.Id.ToString()).CurrentCallBackProccess == "courseAssignmentsDeleteList")
                        {
                            if (UpdMsg.CallbackQuery.Data!.Contains("courseAssignmentToDelete"))
                            {
                                await bot.DeleteMessageAsync(UpdMsg.CallbackQuery.From.Id,
                                    UpdMsg.CallbackQuery.Message!.MessageId,
                                    cts.Token);
                                string currentCourseAssignmentName = db.CourseAssignments.AsNoTracking().First(obj => obj.Id == Convert.ToInt32(UpdMsg.CallbackQuery.Data!.Replace("courseAssignmentToDelete", ""))).Name;
                                Dictionary<string, string> assignmentDeletingChoice = new()
                                {
                                    { "acceptAssignmentDelete", "Удалить" },
                                    { "GoBack", "Отменить" }
                                };
                                var keyboard = new InlineKeyboardMarkup(HandleTextMessages.GetInlineKeyboard(assignmentDeletingChoice, UpdMsg.CallbackQuery.Data!.Replace("courseAssignmentToDelete", "")));
                                logBotAnswer = $"Вы выбрали тему \"{currentCourseAssignmentName}\".\n\nПодтвердите удаление:";
                                await bot.SendTextMessageAsync(UpdMsg.CallbackQuery.From.Id,
                                    text: logBotAnswer,
                                    replyMarkup: keyboard,
                                    parseMode: ParseMode.Html,
                                    cancellationToken: cts.Token);
                                break;
                            }
                            if (UpdMsg.CallbackQuery.Data!.Contains("acceptAssignmentDelete"))
                            {
                                await bot.EditMessageReplyMarkupAsync(UpdMsg.CallbackQuery.From.Id,
                                    UpdMsg.CallbackQuery.Message!.MessageId,
                                    replyMarkup: null,
                                    cancellationToken: cts.Token);
                                logBotAnswer = "Пожалуйста, подождите. Происходит удаление темы...";
                                await bot.EditMessageTextAsync(UpdMsg.CallbackQuery.From.Id,
                                    UpdMsg.CallbackQuery.Message!.MessageId,
                                    text: logBotAnswer,
                                    cancellationToken: cts.Token);
                                var currentCourseAssignment = db.CourseAssignments.First(obj=>obj.Id == Convert.ToInt32(UpdMsg.CallbackQuery.Data.Replace("acceptAssignmentDelete", "")));
                                string currentCourseAssignmentName = currentCourseAssignment.Name;
                                var telegraphClient = new TelegraphClient();
                                ITokenClient tokenClient = telegraphClient.GetTokenClient(db.ChattedUsers.First(obj=>obj.Id == UpdMsg.CallbackQuery.From.Id.ToString()).ChatId);
                                var nodes = new List<NodeElement>();
                                nodes.Add(
                                  new NodeElement("p",
                                      null /* no attribute */,
                                      "Содержимое удалено."
                                  )
                                );
                                NodeElement[] nodesArray = nodes.ToArray();
                                await tokenClient.EditPageAsync(
                                    currentCourseAssignment.Description!.Replace("https://telegra.ph/", ""),
                                    "Статья удалена",
                                    nodesArray /* NodeElement[] */
                                );
                                db.CourseAssignments.Remove(currentCourseAssignment);
                                db.SaveChanges();
                                var currentUserProccess = proccessCallBackUsers.First(obj=>obj.UserID == UpdMsg.CallbackQuery.From.Id.ToString());
                                proccessCallBackUsers.Remove(currentUserProccess);
                                await bot.DeleteMessageAsync(UpdMsg.CallbackQuery.From.Id,
                                    UpdMsg.CallbackQuery.Message!.MessageId,
                                    cts.Token);
                                logBotAnswer = $"Тема \"{currentCourseAssignmentName}\" была успешно удалена!";
                                await bot.SendTextMessageAsync(UpdMsg.CallbackQuery.From.Id,
                                    text: logBotAnswer,
                                    replyMarkup: keyboards.UserMainReplyKeyboradMarkup,
                                    parseMode: ParseMode.Html,
                                    cancellationToken: cts.Token);
                                break;
                            }
                            break;
                        }
                        if (proccessCallBackUsers.First(obj => obj.UserID == UpdMsg.CallbackQuery!.From.Id.ToString()).CurrentCallBackProccess == "CourseMemberRequests")
                        {
                            if (UpdMsg.CallbackQuery.Data!.Contains("acceptUserCourseJoinRequest"))
                            {
                                await bot.EditMessageReplyMarkupAsync(UpdMsg.CallbackQuery!.From.Id,
                                   messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                                   replyMarkup: null,
                                   cancellationToken: cts.Token);
                                int currentRequestCourseId = Convert.ToInt32(UpdMsg.CallbackQuery.Data.Replace("acceptUserCourseJoinRequest", "").Split("=>")[0]);
                                string currentRequestUserId = UpdMsg.CallbackQuery.Data.Replace("acceptUserCourseJoinRequest", "").Split("=>")[1];
                                var currentRequest = db.CourseUsers.First(obj=>obj.Course == currentRequestCourseId && obj.PinnedUser == currentRequestUserId);
                                currentRequest.Admitted = true;
                                db.SaveChanges();
                                var courseUserRequests = db.CourseUsers.AsNoTracking().Where(obj => obj.Course == currentRequestCourseId && obj.Admitted == false);
                                if (courseUserRequests.Count() != 0)
                                {
                                    Dictionary<string, string> userRequestsDicButtons = new();
                                    foreach (var userRequest in courseUserRequests)
                                        userRequestsDicButtons.Add(userRequest.PinnedUser, db.ChattedUsers.First(obj => obj.Id == userRequest.PinnedUser).Name);
                                    userRequestsDicButtons.Add("GoBack", "Назад");
                                    var keyboard = new InlineKeyboardMarkup(HandleTextMessages.GetInlineKeyboard(userRequestsDicButtons, UpdMsg.CallbackQuery.Data.Replace("CourseMemberRequests", "") + "=>acceptUserCourseJoinRequest"));
                                    logBotAnswer = $"Пользователь {db.ChattedUsers.First(obj => obj.Id == currentRequestUserId).Name} успешно стал учатсником курса \"{db.Courses.First(obj => obj.Id == currentRequestCourseId).Name}\"!\n\nНажмите на одного из списка заявок пользователя, чтобы принять его в курс:";
                                    await bot.EditMessageTextAsync(UpdMsg.CallbackQuery!.From.Id,
                                        UpdMsg.CallbackQuery.Message!.MessageId,
                                        text: logBotAnswer,
                                        replyMarkup: keyboard,
                                        cancellationToken: cts.Token);
                                }
                                else
                                {
                                    await bot.DeleteMessageAsync(UpdMsg.CallbackQuery!.From.Id,
                                        messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                                        cancellationToken: cts.Token);
                                    var currentUserProccess = proccessCallBackUsers.First(obj=>obj.UserID == UpdMsg.CallbackQuery!.From.Id.ToString());
                                    proccessCallBackUsers.Remove(currentUserProccess);
                                    logBotAnswer = $"Пользователь {db.ChattedUsers.First(obj=>obj.Id == currentRequestUserId).Name} успешно стал учатсником курса \"{db.Courses.First(obj=>obj.Id == currentRequestCourseId).Name}\"!\n\nЗаявок на вступление в курс больше нет. Вы вернулись в главное меню.";
                                    await bot.SendTextMessageAsync(UpdMsg.CallbackQuery!.From.Id,
                                        text: logBotAnswer,
                                        replyMarkup: keyboards.UserMainReplyKeyboradMarkup,
                                        cancellationToken: cts.Token);
                                }
                                break;
                            }
                            break;
                        }
                    }
                    break;
            }
        }
    }
}
