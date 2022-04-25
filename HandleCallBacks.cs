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

namespace CourseManagementBot
{
    internal class HandleCallBacks
    {
        private string logBotAnswer = ""; // Строка для заполнения информации, которую будет передавать бот.
        private readonly CourseManagementDataContext db = new();
        private readonly Keyboards keyboards = new(); // Базовые клавиатуры для дальнейшего отправления их пользователю для навигации.
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
                        // Удаление кнопки "Назад" в том сообщении, в котором она была нажата.
                        await bot.EditMessageReplyMarkupAsync(UpdMsg.CallbackQuery!.From.Id,
                            messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                            replyMarkup: null,
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
                // Идентификатор inline кнопки редактирования профиля.
                case "EditProfile":
                    // Проверяем, не пытается ли пользователь войти в CallBack запрос, когда уже находится в нем.
                    if (proccessCallBackUsers.FirstOrDefault(obj => obj.UserID == UpdMsg.CallbackQuery!.From.Id.ToString()) == null)
                    {
                        // Добавляем пользователя в лист пользователей, находящихся в CallBack запросе.
                        proccessCallBackUsers.Add(new ProccessCallBackUsers
                        {
                            UserID = UpdMsg.CallbackQuery!.From.Id.ToString(),
                            CurrentCallBackProccess = "EditProfile"
                        });
                        // Удаление inline кнопок в том сообщении, в котором пользватель нажал на одну из этих кнопок.
                        await bot.EditMessageReplyMarkupAsync(UpdMsg.CallbackQuery!.From.Id,
                            messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                            replyMarkup: null,
                            cancellationToken: cts.Token);
                        // Уведомление пользователю о том, что он перешел в процесс CallBack запроса, а также временное удаление reply кнопок.
                        logBotAnswer = "Вы перешли в процесс редактирования профиля.";
                        await bot.SendTextMessageAsync(chatId: UpdMsg.CallbackQuery!.From.Id,
                            text: logBotAnswer,
                            replyMarkup: new ReplyKeyboardRemove(),
                            cancellationToken: cts.Token);
                        Console.WriteLine($" ~ [{DateTime.Now:yyyy-MM-dd HH-mm}] Ответ пользователю @{UpdMsg.CallbackQuery!.From.Username}: {logBotAnswer}");
                        // Вывод пользователю кнопок для дальнейшего редактирования профиля.
                        logBotAnswer = "Выберите один из следующих пунктов, которые вы хотите отредактировать:";
                        await bot.SendTextMessageAsync(chatId: UpdMsg.CallbackQuery!.From.Id,
                            text: logBotAnswer,
                            replyMarkup: keyboards.EditProfileInlineKeyboard,
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
                            var currentCourse = db.Courses.First(obj=>obj.Id==Convert.ToInt32(UpdMsg.CallbackQuery.Data!.Replace("ownCourses", "")));
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
                            var currentCourseInlineKeyboard = new InlineKeyboardMarkup(HandleTextMessages.GetInlineKeyboard(courseManageButtons, UpdMsg.CallbackQuery.Data!.Replace("ownCourses", "")));
                            
                            await bot.EditMessageTextAsync(UpdMsg.CallbackQuery!.From.Id,
                                messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                                text: logBotAnswer,
                                replyMarkup: currentCourseInlineKeyboard,
                                parseMode: ParseMode.Html,
                                cancellationToken: cts.Token);
                        }

                        // Проверяем, является ли пользователь, сделавший запрос, участником курса.
                        if (UpdMsg.CallbackQuery.Data!.Contains("coursesMember"))
                        {
                            // Удаление inline кнопок в том сообщении, в котором пользватель нажал на одну из этих кнопок.
                            await bot.EditMessageReplyMarkupAsync(UpdMsg.CallbackQuery!.From.Id,
                                messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                                replyMarkup: null,
                                cancellationToken: cts.Token);
                            var currentCourse = db.Courses.First(obj => obj.Id == Convert.ToInt32(UpdMsg.CallbackQuery.Data!.Replace("coursesMember", "")));
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
                            Dictionary<string, string> courseManageButtons;
                            // Проверка роли пользователя. Если это помощник куратора, то система даст ему возможность редактировать сведения о курсе.
                            if (db.CourseUsers.First(obj=>obj.Course==currentCourse.Id && obj.PinnedUser==UpdMsg.CallbackQuery!.From.Id.ToString()).CourseUserRole == "Помощник куратора")
                            {
                                if (currentCourse.Token != null)
                                    logBotAnswer += $"Токен для подключения к курсу: {currentCourse.Token}";
                                else
                                    logBotAnswer += "Токен для подключения к курсу отсутствует.";
                                // Создание библиотеки с данными, которые нужно будет отобразить на кнопках (если нужно больше кнопок, добавлять больше полей).
                                // Нужно это для дальнейшей отправки в метод динамического формирования inline. Почему так, а не через базовый конструктор клавитауры?
                                // Потому что для понимания следующего CallBack запроса на редактирование конкретного курса, вызваным пользователем, для inline кнопок отправляется идентификатор выбранного курса и идентификатор inline кнопок
                                // Так мы сможем понять, какую кнопку для управления курсом он нажал конкретно и каким курсом он хочет управлять.
                                courseManageButtons = new()
                                {
                                    { "CourseMemberRequests", "Заявки на вступление" },
                                    { "CourseMemberList", "Список участников" },
                                    { "LeaveCourse"+currentCourse.Id.ToString(), "Покинуть курс" }
                                };
                            }
                            else
                            {
                                courseManageButtons = new()
                                {
                                    { "CourseMemberList", "Список участников" },
                                    { "LeaveCourse"+currentCourse.Id.ToString(), "Покинуть курс" }
                                };
                            }
                            // Отправка библиотеки в метод динамического формирования inline кнопок.
                            var currentCourseInlineKeyboard = new InlineKeyboardMarkup(HandleTextMessages.GetInlineKeyboard(courseManageButtons, UpdMsg.CallbackQuery.Data!.Replace("ownCourses", "")));

                            await bot.EditMessageTextAsync(UpdMsg.CallbackQuery!.From.Id,
                                messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                                text: logBotAnswer,
                                replyMarkup: currentCourseInlineKeyboard,
                                parseMode: ParseMode.Html,
                                cancellationToken: cts.Token);
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

                        if (UpdMsg.CallbackQuery.Data!.Contains("acceptJoinCourse"))
                        {
                            string courseToken = UpdMsg.CallbackQuery.Data!.Replace("acceptJoinCourse", "").Split('-')[1];
                            ActiveToken courseTokenData = db.ActiveTokens.First(obj => obj.Token == courseToken);
                            courseTokenData.UsedAttempts++;
                            CourseUser newCourseMember = new CourseUser
                            {
                                Course = Convert.ToInt32(UpdMsg.CallbackQuery.Data!.Replace("acceptJoinCourse", "").Split('-')[0]),
                                PinnedUser = UpdMsg.CallbackQuery!.From.Id.ToString(),
                                CourseUserRole = "Участник",
                                Admitted = true,
                                LeavingDate = DateTime.Now.Date
                            };
                            db.CourseUsers.Add(newCourseMember);
                            var currentUserProccess = proccessCallBackUsers.First(obj => obj.UserID == UpdMsg.CallbackQuery!.From.Id.ToString());
                            proccessCallBackUsers.Remove(currentUserProccess);
                            logBotAnswer = $"{char.ConvertFromUtf32(0x2705)} Вы успешно подписались на курс <b>\"{db.Courses.First(obj=>obj.Token==courseToken).Name}\"</b>!";
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
                            }
                        }

                        if (UpdMsg.CallbackQuery.Data!.Contains("acceptLeaveCourse"))
                        {
                            await bot.EditMessageReplyMarkupAsync(UpdMsg.CallbackQuery!.From.Id,
                                messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                                replyMarkup: null,
                                cancellationToken: cts.Token);
                            var currentCourseMemberToDelete = db.CourseUsers.First(obj=>obj.Course==Convert.ToInt32(UpdMsg.CallbackQuery.Data!.Replace("acceptLeaveCourse", "")) && obj.PinnedUser== UpdMsg.CallbackQuery!.From.Id.ToString());
                            db.CourseUsers.Remove(currentCourseMemberToDelete);
                            db.SaveChanges();
                            logBotAnswer = "Вы успешно покинули курс!";
                            await bot.EditMessageTextAsync(UpdMsg.CallbackQuery!.From.Id,
                                messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                                text: logBotAnswer,
                                cancellationToken: cts.Token);
                            var currentUserProccess = proccessCallBackUsers.First(obj => obj.UserID == UpdMsg.CallbackQuery!.From.Id.ToString());
                            proccessCallBackUsers.Remove(currentUserProccess);
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
                            }
                        }

                        if (UpdMsg.CallbackQuery.Data!.Contains("acceptDeleteCourse"))
                        {
                            await bot.EditMessageReplyMarkupAsync(UpdMsg.CallbackQuery!.From.Id,
                                messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                                replyMarkup: null,
                                cancellationToken: cts.Token);
                            logBotAnswer = "Вы успешно удалили курс!";
                            var choosedDeleteCourse = db.Courses.First(obj=>obj.Id==Convert.ToInt32(UpdMsg.CallbackQuery!.Data!.Replace("acceptDeleteCourse", "")));
                            db.Courses.Remove(choosedDeleteCourse);
                            db.SaveChanges();
                            await bot.EditMessageTextAsync(UpdMsg.CallbackQuery!.From.Id,
                                messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                                text: logBotAnswer,
                                cancellationToken: cts.Token);
                            var currentUserProccess = proccessCallBackUsers.First(obj => obj.UserID == UpdMsg.CallbackQuery!.From.Id.ToString());
                            proccessCallBackUsers.Remove(currentUserProccess);
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
                                var currentEditingCourse = db.Courses.First(obj => obj.Id == Convert.ToInt32(UpdMsg.CallbackQuery.Data!.Replace("EditCourse", "")));
                                logBotAnswer = $"Вы перешли в конструктор редактирования курса\n<b>{currentEditingCourse.Name}</b>.";
                                // Здесь создать динамическую клавиатуру, которая даст возможность редактировать каждое значение курса по отдельности.
                                await bot.EditMessageTextAsync(UpdMsg.CallbackQuery!.From.Id,
                                    messageId: UpdMsg.CallbackQuery.Message!.MessageId,
                                    text: logBotAnswer,
                                    replyMarkup:, // Сюда примотать клаваитуру.
                                    cancellationToken: cts.Token);
                            }
                        }
                    }
                    break;
            }
        }
    }
}
