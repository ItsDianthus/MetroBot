using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace TelegramUtils
{
    public class UserMessages
    {
        public UserMessages() { }

        /// <summary>
        /// Sends a greeting message to the user.
        /// </summary>
        /// <param name="botClient">Instance of ITelegramBotClient for sending messages.</param>
        /// <param name="chatId">ID of the chat with the user.</param>
        /// <param name="cancellationToken">Cancellation token for the asynchronous operation.</param>
        public async Task AnsGreetings(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
        {
            ReplyKeyboardMarkup replyKeyboardMarkup = new(new[] { new KeyboardButton[] { "Add a file to work with" } })
            {
                ResizeKeyboard = true
            };
            await botClient.SendStickerAsync(
                chatId: chatId,
                sticker: InputFile.FromFileId(
                    "CAACAgIAAxkBAAJIuWX5slEaNn1sWVNvxjeB3OnXZ4zMAAINAAMkeEAbZwABrfR8DlVCNAQ"),
                cancellationToken: cancellationToken);
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Работаем, метро Люблино! \n" +
                "Этот бот работает со транспортно-пересадочными пунктами - джейсон и csv формата. Чтобы продолжить, нажмите на кнопку ниже:",
                replyMarkup: replyKeyboardMarkup,
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Sends a help message to the user.
        /// </summary>
        /// <param name="botClient">Instance of ITelegramBotClient for sending messages.</param>
        /// <param name="chatId">ID of the chat with the user.</param>
        /// <param name="cancellationToken">Cancellation token for the asynchronous operation.</param>
        public async Task AnsHelp(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
        {
            await botClient.SendStickerAsync(
                chatId: chatId,
                sticker: InputFile.FromFileId(
                    "CAACAgIAAxkBAAJI6mX7XubNLTMW3sq0MJuyLRzsrJu7AAI9AAMkeEAbyATETJxYXpw0BA"),
                cancellationToken: cancellationToken);
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Нужна помощь? \nНажмите /start чтобы начать/продолжить с того момента, где остановились." +
                "\nAdd a file to work with - прислать новый файл.",
                replyMarkup: new ReplyKeyboardRemove(),
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Sends a message requesting the user to send a file for further processing.
        /// </summary>
        /// <param name="botClient">Instance of ITelegramBotClient for sending messages.</param>
        /// <param name="chatId">ID of the chat with the user.</param>
        /// <param name="cancellationToken">Cancellation token for the asynchronous operation.</param>
        public async Task AnsAddFile(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
        {
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Пришлите файл формата json или csv, с которым Вы хотите продолжить работу! \n " +
                "Вы сможете отсортировать его или применить фильтр.",
                replyMarkup: new ReplyKeyboardRemove(),
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Sends a message indicating that the command is not recognized.
        /// </summary>
        /// <param name="botClient">Instance of ITelegramBotClient for sending messages.</param>
        /// <param name="chatId">ID of the chat with the user.</param>
        /// <param name="cancellationToken">Cancellation token for the asynchronous operation.</param>
        public async Task AnsNorDefined(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
        {
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Я не понимаю, что Вы хотите. Такая команда недоступна. \nПопробуйте нажать /help",
                cancellationToken: cancellationToken);
        }
    }
}
