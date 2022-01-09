using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using CourseManagementBot;
using CourseManagementBot.Models;

TelegramBotClient botClient = new("2056051853:AAHKZxIFf7CYf31AgOuJX4r4yiUXj4w6jQ4"); // Токен бота
using CancellationTokenSource cts = new();
HandleTextMessages handleTextMessage = new(); // Экземпляр объекта проверки пользователя
CourseManagementDataContext db = new(); // Экземпляр объекта БД
List<ProccessCallBackUsers> proccessCallBackUsers = new();

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
    // Определение типа апдейта (callback, сообщение и т. д.).
    switch (update.Type)
    {
        case UpdateType.CallbackQuery: // Проверка на CallBack запрос, если пользователь нажал на inline кнопку во вложенном сообщении.
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH-mm}] Пользователь @{update.CallbackQuery!.From.Username} нажал на inline кнопку, содержащую в себе событие {update.CallbackQuery!.Data}."); // Логирование в консоль запуска события пользователем.
            handleTextMessage.AnswerCallback(db.ChattedUsers.First(obj => obj.Id == update.CallbackQuery!.From.Id.ToString()), update, botClient, cts, proccessCallBackUsers); // Вызов обработки события callback запроса от пользователя.
            break;
        case UpdateType.Message: // Проверка на сообщение, отправленного боту.
            // Определение типа сообщения (текст, файл, фото и т. д.) и его пропуск, если тип сообщения не подходит.
            switch (update.Message!.Type)
            {
                case MessageType.Text: // Алгоритм действий в случае, если в сообщении пользователя только текст.
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH-mm}] Пользователь {update.Message!.From!.Username} отправил текстовое сообщение: {update.Message.Text}"); // Логирование в консоль содержимого сообщения пользователя.
                    try
                    {
                        if (!db.ChattedUsers.Any(obj => obj.Id == update.Message.From!.Id.ToString())) // Проверка на отрицательный результат нахождения пользователя в БД (1 если пользователь новенький).
                            await handleTextMessage.AnswerMessage(null, update, botClient, cts, proccessCallBackUsers); // Отправка приветствия новому пользователю.
                        else
                            await handleTextMessage.AnswerMessage(db.ChattedUsers.First(obj => obj.Id == update.Message.From!.Id.ToString()), update, botClient, cts, proccessCallBackUsers); // Вызов проверки текущего сообщения для дальнейшего ответа от бота.
                    }
                    // Ошибка БД.
                    catch (Exception ex)
                    {
                        Console.WriteLine($" ~ Ошибка БД: {ex.Message}\n\n"); // Логирование в консоль деталей ошибки БД. Скорее всего проблема связана с подключением к БД.
                    }
                    break;
                case MessageType.Photo: // Алгоритм действий в случае, если в сообщении пользователя фото.
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH-mm}] Пользователь {update.Message!.From!.Username} отправил фотографию."); // Логирование в консоль содержимого сообщения пользователя.
                    break;
                // Пропускает сообщение мимо в случае, если тип полученного сообщения пользователя не соответствует допустимым типам.
                default:
                    return;
            }
            break;
        // Пропускает сообщение, если оно имеет неверный тип апдейта.
        default:
            break;
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