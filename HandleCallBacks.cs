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
        private readonly Keyboards keyboards = new(); // Клавиатуры для дальнейшего отправления их пользователю для навигации.
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
                        // Уведомление пользователю о том, что он перешел в процесс CallBack запроса с кнопкой "Назад".
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
                    await bot.SendTextMessageAsync(chatId: UpdMsg.CallbackQuery!.From.Id,
                        text: "Здесь будет функционал редактирования профиля.",
                        cancellationToken: cts.Token);
                    break;
            }
        }
    }
}
