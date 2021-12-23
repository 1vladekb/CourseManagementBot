using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

TelegramBotClient botClient = new TelegramBotClient("2056051853:AAHKZxIFf7CYf31AgOuJX4r4yiUXj4w6jQ4"); // Токен бота
using CancellationTokenSource cts = new CancellationTokenSource();
CourseManagementBot.CheckCurrentUser CheckUser = new CourseManagementBot.CheckCurrentUser(); // Экземпляр объекта проверки пользователя
CourseManagementBot.Models.CourseManagementDataContext db = new CourseManagementBot.Models.CourseManagementDataContext(); // Экземпляр объекта БД
List<string> ProccessCallBackUsers = new List<string>();

var receiverOptions = new ReceiverOptions
{
    AllowedUpdates = { } // Разрешение на получение сообщений в любом виде для дальнейшей его обработки.
};
botClient.StartReceiving(
    HandleUpdateAsync,
    HandleErrorAsync,
    receiverOptions,
    cancellationToken: cts.Token); // Запуск бота.

var botInfo = await botClient.GetMeAsync(); // Получение информации о боте

Console.WriteLine($"~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\n\nЗапущен проект автоматизации управления онлайн-курсами\nПроект привязан к боту @{botInfo.Username}\n\n~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
Console.ReadLine();
cts.Cancel();


// Алгоритм действий после получения сообщения.
async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    // Строки для отображения информации о пользователе и времени в логах.
    string logDateMsg = $"[{DateTime.Now.ToString("yyyy-MM-dd HH-mm")}] ",
        logBotAnswer = "";
    // Определение типа апдейта (callback, сообщение и т. д.).
    switch (update.Type)
    {
        case UpdateType.CallbackQuery: // Проверка на CallBack, если пользователь нажал на inline кнопку во вложенном сообщении.
            Console.WriteLine($"{logDateMsg}Пользователь {update.CallbackQuery!.From.Id} нажал на inline кнопку, содержащую в себе событие {update.CallbackQuery!.Data}."); // Логирование в консоль запуска события пользователем.
            CheckUser.AnswerCallback(db.ChattedUsers.First(obj => obj.Id == update.CallbackQuery!.From.Id.ToString()), update, botClient, cts, ProccessCallBackUsers); // Вызов обработки события callback запроса от пользователя.
            break;
        case UpdateType.Message: // Проверка на сообщение, отправленного боту.
            // Определение типа сообщения (текст, файл, фото и т. д.) и его пропуск, если тип сообщения не подходит.
            switch (update.Message!.Type)
            {
                case MessageType.Text: // Алгоритм действий в случае, если в сообщении пользователя только текст.
                    Console.WriteLine($"{logDateMsg}Пользователь {update.Message!.From!.Username} отправил текстовое сообщение: {update.Message.Text}."); // Логирование в консоль содержимого сообщения пользователя.
                    break;
                case MessageType.Photo: // Алгоритм действий в случае, если в сообщении пользователя фото.
                    Console.WriteLine($"{logDateMsg}Пользователь {update.Message!.From!.Username} отправил фотографию."); // Логирование в консоль содержимого сообщения пользователя.
                    break;
                // Пропускает сообщение мимо в случае, если тип полученного сообщения пользователя не соответствует допустимым типам.
                default:
                    return;
            }
            break;
        // Пропускает сообщение, если оно имеет неверный тип апдейта.
        default:
            return;
    }
    // Проверка наличия содержимого сообщения.
    if (update.Message != null)
    {
        try
        {
            if (!db.ChattedUsers.Any(obj => obj.Id == update.Message.From!.Id.ToString())) // Проверка на отрицательный результат нахождения пользователя в БД (новенький пользователь или нет).
            {
                Console.WriteLine(" ~ Пользователь не найден в БД. Он впервые пишет боту.");
                // Проверка на /start в сообщении.
                if (update.Message.Text == "/start")
                {
                    Console.WriteLine(" ~ Пользователь запустил бота командой. Добавляем его в БД.");
                    // Процесс добавления нового пользователя в таблицу БД ChattedUsers и сохранение БД.
                    CourseManagementBot.Models.ChattedUser chattedUser = new CourseManagementBot.Models.ChattedUser()
                    {
                        Id = update.Message.From!.Id.ToString(),
                        ChatId = update.Message.Chat.Id.ToString(),
                        Name = update.Message.From.Username ?? update.Message.From.FirstName, // Если имя пользователя отсутствует, будет присваиваться обычное имя.
                        Role = "Пользователь"
                    };
                    db.Add(chattedUser); // Добавление новой записи в БД
                    db.SaveChanges(); // Сохранение БД
                    Console.WriteLine(" ~ Пользователь был добавлен в БД.");
                    CheckUser.Check(db.ChattedUsers.First(obj=>obj.Id == chattedUser.Id), update, botClient, cts, true, ProccessCallBackUsers); // Отправка приветствия новому пользователю.
                }
                // Порядок действий, если пользователя нет в БД и он не написал /start.
                else
                {
                    // Логирование в консоли и отправка сообщения пользователю о том, что нужно прописать /start для дальнейшего взаимодействия с ботом.
                    logBotAnswer = "Для того, чтобы начать взаимодействовать с ботом, напишите /start";
                    Console.WriteLine($" ~ {logDateMsg}Ответ пользователю {update.Message!.From!.Username}: {logBotAnswer}");
                    Message sentMessage = await botClient.SendTextMessageAsync(
                        chatId: update.Message.Chat.Id,
                        text: logBotAnswer,
                        cancellationToken: cancellationToken);
                }
            }
            else
                CheckUser.Check(db.ChattedUsers.First(obj=>obj.Id == update.Message.From!.Id.ToString()), update, botClient, cts, false, ProccessCallBackUsers); // Вызов проверки текущего сообщения для дальнейшего ответа от бота.
        }
        // Ошибка БД.
        catch (Exception ex)
        {
            Console.WriteLine($" ~ Ошибка БД: {ex.Message}\n\n"); // Логирование в консоль деталей ошибки БД. Скорее всего проблема связана с подключением к БД.
        }
    }
}

Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    var ErrorMessage = exception switch
    {
        ApiRequestException apiRequestException
            => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        _ => exception.ToString()
    };

    Console.WriteLine(ErrorMessage);
    return Task.CompletedTask;
}