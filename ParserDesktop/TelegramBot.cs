using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;

namespace ParserDesktop
{
    public class TelegramBot
    {
        static ITelegramBotClient bot = new TelegramBotClient("5105318731:AAFtc98CK5jl2lAuKmLJirxG1QUDojLkAyo");

        public TelegramBot()
        {
            Console.WriteLine("Запущен бот " + bot.GetMeAsync().Result.FirstName);

            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }, // receive all update types
            };
            //  bot.StartReceiving(
            //      HandleUpdateAsync,
            //      receiverOptions,
            //      cancellationToken
            // );
        }

        public async Task Execute(string text)
        {
            await bot.SendTextMessageAsync("1062249684", text);
        }
        
        // public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
        //     CancellationToken cancellationToken)
        // {
        //     var message = update.Message;
        //         await botClient.SendTextMessageAsync(message.Chat, _text);
        // }
    }
}