using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using System.Threading.Tasks;
using System;
using System.Net.Http;

internal class Program
{
    static async Task Main(string[] args)
    {
        var botClient = new TelegramBotClient("6071026478:AAHbNtNU8JCztj2fk_wY-jlQEM2McorJwjw");

        var me = await botClient.GetMeAsync();
        Console.WriteLine($"Hello, World! I am user {me.Id} and my name is {me.FirstName}.");

        using CancellationTokenSource cts = new();

        ReceiverOptions receiverOptions = new()
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };

        botClient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: cts.Token
        );

        Console.ReadLine();

        cts.Cancel();

        async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var message = update.Message;

            var input = message.Text;

            if (!int.TryParse(input, out int number) || number < 1 || number > 123)
            {
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Пожалуйста, введите число от 1 до 123."
                );
                return;
            }

            var url = $"https://randomfox.ca/images/{number}.jpg";

            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return;

            Message sentMessage = await botClient.SendPhotoAsync(
                chatId: message.Chat.Id,
                photo: InputFile.FromUri(url),
                caption: "Вот случайное изображение лисы!",
                cancellationToken: cancellationToken
            );
        }

        Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
    }
}