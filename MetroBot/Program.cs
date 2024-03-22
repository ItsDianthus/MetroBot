using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramUtils;

namespace MetroBot
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                // Instantiate a FileCommunicator object to manage file-related operations.
                FileCommunicator filecom = new FileCommunicator();
                // Create the main directory and necessary files for the application to function.
                filecom.CreateMaindir();

                // Instantiate a TelegramMetroBot object to handle Telegram bot functionality.
                TelegramMetroBot metroBot = new TelegramMetroBot();
                // Start the Telegram bot, allowing it to receive updates and respond to user input.
                metroBot.Start();
            }
            catch (Exception)
            {
                Console.WriteLine("Произошла непредвиденная ошибка. Попробуйте перезапустить ботаю");
            }
        }
    }
}