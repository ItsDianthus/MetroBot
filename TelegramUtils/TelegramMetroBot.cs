using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using DataModelsUtils;
using static TelegramUtils.UserState;
using Document = Telegram.Bot.Types.Document;
using System.IO;

namespace TelegramUtils
{
    /// <summary>
    /// Represents a Telegram bot for handling user interactions and file processing.
    /// </summary>
    public class TelegramMetroBot
    {
        // This is our private field with the bot.
        private readonly TelegramBotClient _client;

        // Constructor without any parameters.
        public TelegramMetroBot()
        {
            _client = new TelegramBotClient("7170064401:AAHPDL81nQOvKL7DTuJlyiErbOwQD4xp2Dc");
        }

        /// <summary>
        /// Starts the bot and uses Console.ReadLine to continue work with bot, starts bot's receiving.
        /// </summary>
        public void Start()
        {
            ReceiverOptions receiverOptions = new()
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            };

            using CancellationTokenSource cts = new();

            _client.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cts.Token
            );

            Thread.Sleep(-1);
            cts.Cancel();
        }
        

        async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            // Making massive of TransportPoints objects.
            TransportPoints[] transpoints = Array.Empty<TransportPoints>();

            // стейт - состояние
            State state = new State();

            if (update.Message is { } message)
            {
                long chatId = message.Chat.Id;
                Console.WriteLine($"Received a '{message.Text}' message in chat {chatId}.");
                if (message.Text is { } messageText)
                {
                    await HandleUserMessage(botClient, chatId, cancellationToken, messageText, state);
                }
                else if (message.Document is { } messageDoc) // DOCUMENTTTT
                {
                    await HandleUserDocument(botClient, chatId, cancellationToken, messageDoc);
                }
            }
            else if (update.CallbackQuery is { } callbackQuery)
            {
                Console.WriteLine("Not text");
            }

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

        /// <summary>
        /// Handles user documents received by the bot.
        /// </summary>
        private async Task HandleUserDocument(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken, Document document)
        {
            // Создаем файлы.
            
            var file = await botClient.GetFileAsync(document.FileId, cancellationToken); // file object
            string fileExtension = Path.GetExtension(file.FilePath)!; // extention

            // Проверяем расширение
            if (fileExtension != ".csv" && fileExtension != ".json")
            {
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Данный файл не в формате csv или json. Пожалуйста, отправьте корректный файл!",
                    cancellationToken: cancellationToken);
                return;
            }

            State state = new State();
            var fileName = document.FileName; // telegram obj name
            var fileNameInner = $"{chatId}_{fileName}";

            char separator = Path.DirectorySeparatorChar;
            var pathInner = $"..{separator}..{separator}..{separator}..{separator}WorkingFiles{separator}{fileNameInner}";
            var pathTelegram = file.FilePath; // telegram path
            
            try
            {
                // ГРУЗИМ ФАЙЛ В ПАПОЧКУ, ТУДА НАПРАВЛЕН СТРИМ
                await using (var stream = System.IO.File.Create(pathInner))
                {
                    await botClient.DownloadFileAsync(
                        filePath: pathTelegram!,
                        destination: stream,
                        cancellationToken: cancellationToken);
                }

                // Вытягиваем данные из файла и смотрим их соответствие формату
                await ProcessFile(pathInner, botClient, chatId, cancellationToken);

                // Здесь мы запомнили путь до файла
                state.SetUserPath(chatId, pathInner);
                state.SetUserState(chatId, "hasFile");

                // Say that we have our file been downloaded.
                await AnsFileIsReady(botClient, chatId, cancellationToken);
                
            }
            catch (FormatException)
            {
                return;
            }
            catch
            {
                Console.WriteLine("File download problems");
                return;
            }

            
        }

        /// <summary>
        /// Processes the file located at the specified file path based on its extension (CSV or JSON).
        /// Reads the file content and converts it into an array of TransportPoints objects.
        /// </summary>
        /// <param name="filePath">The path to the file to be processed.</param>
        /// <param name="botClient">The Telegram bot client for communication.</param>
        /// <param name="chatId">The ID of the chat where the message was received.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>An array of TransportPoints representing the data read from the file.</returns>
        private async Task<TransportPoints[]> ProcessFile(string filePath, ITelegramBotClient botClient, long chatId,
            CancellationToken cancellationToken)
        {
            TransportPoints[] transPoints;

            if (Path.GetExtension(filePath) == ".csv")
            {

                var csvProcessing = new CSVProcessing();
                using var streamReader = new StreamReader(filePath);
                transPoints = await csvProcessing.Read(streamReader, botClient, chatId, cancellationToken);
            }
            else
            {
                var jsonProcessing = new JSONProcessing();
                using var streamReader = new StreamReader(filePath);
                transPoints = await jsonProcessing.Read(streamReader, botClient, chatId, cancellationToken);
            }

            return transPoints;
        }

        /// <summary>
        /// Handles user messages for performing filtering operations based on the current user state.
        /// Filters the data based on the user's input keyword and updates the user state accordingly.
        /// </summary>
        /// <param name="botClient">The Telegram bot client for communication.</param>
        /// <param name="chatId">The ID of the chat where the message was received.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <param name="messageText">The text of the user message.</param>
        /// <param name="state">The current state of the user.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task HandleUserMessage(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken,
        string messageText, State state)
        {
            UserMessages userMess = new UserMessages();

            if (state.GetUserState(chatId) == "distFilter" || state.GetUserState(chatId) == "carFilter" || state.GetUserState(chatId) == "statAndStatFilter")
            {
                string keyWord = messageText;
                var transPoints = await ProcessFile(state.GetUserPath(chatId), botClient, chatId, cancellationToken);
                var dataProcess = new DataProcessing();
                TransportPoints[] transPointsNew;
                switch (state.GetUserState(chatId))
                {
                    case "distFilter":
                        transPointsNew = dataProcess.Filter(transPoints, keyWord, "District");
                        await MessageTwoFiles(transPointsNew, state, botClient, chatId, cancellationToken);
                        break;
                    case "carFilter":
                        transPointsNew = dataProcess.Filter(transPoints, keyWord, "CarCapacity");
                        await MessageTwoFiles(transPointsNew, state, botClient, chatId, cancellationToken);
                        break;
                    case "statAndStatFilter":
                        string[] keywords = keyWord.Split(':');
                        if (keywords.Length == 2)
                        {
                            transPointsNew = dataProcess.FilterByTwo(transPoints, keywords[0], keywords[1]);
                            await MessageTwoFiles(transPointsNew, state, botClient, chatId, cancellationToken);
                        }
                        else
                        {
                            await AnsErrorField(botClient, chatId, cancellationToken);
                            await AnsGetFilterWord(botClient, chatId, cancellationToken, true);
                            return;
                        }
                        break;
                }


                // Filtering performed successfully.
                state.SetUserState(chatId, "hasFile");
                await AnsProcessFile(botClient, chatId, cancellationToken);
            }
            else if (messageText == "/start") 
            {
                // We add new user.
                if (state.GetUserState(chatId) == "addFile")
                {
                    await userMess.AnsAddFile(botClient, chatId, cancellationToken);
                }
                else if (state.GetUserState(chatId) == "start")
                {
                    await userMess.AnsGreetings(botClient, chatId, cancellationToken);
                }
                else if (state.GetUserState(chatId) == "startFilterFile")
                {
                    await AnsGetFilterField(botClient, chatId, cancellationToken);
                }
                else
                {
                    await AnsProcessFile(botClient, chatId, cancellationToken);
                }
            }
            else if (messageText == "/help")
            {
                await userMess.AnsHelp(botClient, chatId, cancellationToken);
            }
            else if (messageText == "Sort up")
            {
                if (state.GetUserState(chatId) == "hasFile")
                {
                    // Ставим юзеру статус отсортированного.
                    state.SetUserState(chatId, "startSortUpFile");
                    await AnsChooseFormat(botClient, chatId, cancellationToken);
                    // ВЫБОР В КАКОМ ФОРМАТЕ ЕГО ВЫВОДИТЬ ХОТИТЕ
                }
                else
                {
                    await userMess.AnsNorDefined(botClient, chatId, cancellationToken);
                }
            }
            else if (messageText == "Sort down")
            {
                if (state.GetUserState(chatId) == "hasFile")
                {
                    // Ставим юзеру статус отсортированного.
                    state.SetUserState(chatId, "startSortDownFile");
                    await AnsChooseFormat(botClient, chatId, cancellationToken);
                    // ВЫБОР В КАКОМ ФОРМАТЕ ЕГО ВЫВОДИТЬ ХОТИТЕ
                }
                else
                {
                    await userMess.AnsNorDefined(botClient, chatId, cancellationToken);
                }

            }
            else if (messageText == "Filter" && state.GetUserState(chatId) == "hasFile")
            {
                // Ставим юзеру статус отсортированного.
                state.SetUserState(chatId, "startFilterFile");
                await AnsGetFilterField(botClient, chatId, cancellationToken);
                // ВЫБОР В КАКОМ ФОРМАТЕ ЕГО ВЫВОДИТЬ ХОТИТЕ
            }
            else if (state.GetUserState(chatId) == "startFilterFile")
            {
                switch (messageText)
                {
                    case "District":
                        state.SetUserState(chatId, "distFilter");
                        await AnsGetFilterWord(botClient, chatId, cancellationToken);
                        break;
                    case "CarCapacity":
                        state.SetUserState(chatId, "carFilter");
                        await AnsGetFilterWord(botClient, chatId, cancellationToken);
                        break;
                    case "Status & NearStation":
                        state.SetUserState(chatId, "statAndStatFilter");
                        await AnsGetFilterWord(botClient, chatId, cancellationToken, true);
                        break;
                    default:
                        await AnsGetFilterField(botClient, chatId, cancellationToken);
                        break;
                }
            }
            else if (messageText == "Add a file to work with")
            {
                state.SetUserState(chatId, "addFile");
                await userMess.AnsAddFile(botClient, chatId, cancellationToken);
            }
            else if ((messageText == "CSV" || messageText == "JSON") && (state.GetUserState(chatId) == "startSortUpFile" || state.GetUserState(chatId) == "startSortDownFile" ||
                state.GetUserState(chatId) == "startSortUpFile" || state.GetUserState(chatId) == "startSortUpFile"))
            { // МЫ ИХ ТИПА ОТСОРТИЛИ УРААА
                var transPoints = await ProcessFile(state.GetUserPath(chatId), botClient, chatId, cancellationToken);
                var dataProcess = new DataProcessing();
                TransportPoints[] transPointsNew;
                if (state.GetUserState(chatId) == "startSortUpFile")
                {
                    transPointsNew = dataProcess.SortByAvailTransAscending(transPoints);
                }
                else 
                {
                    transPointsNew = dataProcess.SortByYearDescending(transPoints);
                }
                FileStream stream;
                switch (messageText)
                {
                    case "CSV":
                        CSVProcessing csv = new CSVProcessing();
                        stream = await csv.Write(transPointsNew, state.GetUserPath(chatId));
                        await UploadFile(stream, ".csv", botClient, chatId, cancellationToken);
                        await csv.Delete(state.GetUserPath(chatId));
                        break;
                    case "JSON":
                        JSONProcessing js = new JSONProcessing();
                        stream = await js.Write(transPointsNew, state.GetUserPath(chatId));
                        await UploadFile(stream, ".json", botClient, chatId, cancellationToken);
                        await js.Delete(state.GetUserPath(chatId));
                        break;
                }
                state.SetUserState(chatId, "hasFile");
                await AnsProcessFile(botClient, chatId, cancellationToken);
            }
            else
            {
                await userMess.AnsNorDefined(botClient, chatId, cancellationToken);
            }
        }

        /// <summary>
        /// Функция получает поток и расширение и выводит файл пользователю
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="extention"></param>
        /// <param name="client"></param>
        /// <param name="chatId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task UploadFile(FileStream stream, string extention, ITelegramBotClient client, long chatId,
            CancellationToken cancellationToken)
        {
            try
            {
                var state = new State();
                await using var streamWriter = new StreamWriter(stream);

                await client.SendDocumentAsync(
                    chatId: chatId,
                    document: InputFile.FromStream(
                        stream: stream,
                        fileName: ("out_" + Path.GetFileNameWithoutExtension(state.GetUserPath(chatId))) + extention),
                    caption: "Файл успешно обработан:",
                    cancellationToken: cancellationToken);
            }
            catch
            {
                throw new Exception("Некорректно использован данный метод");
            }
        }

        /// <summary>
        /// Messages two files csv format and json format.
        /// </summary>
        /// <param name="transPointsNew"></param>
        /// <param name="state"></param>
        /// <param name="botClient"></param>
        /// <param name="chatId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task MessageTwoFiles(TransportPoints[] transPointsNew, State state, ITelegramBotClient botClient, long chatId,
            CancellationToken cancellationToken)
        {
            FileStream stream;

            CSVProcessing csv = new CSVProcessing();
            stream = await csv.Write(transPointsNew, state.GetUserPath(chatId));
            await UploadFile(stream, ".csv", botClient, chatId, cancellationToken);
            await csv.Delete(state.GetUserPath(chatId));

            JSONProcessing js = new JSONProcessing();
            stream = await js.Write(transPointsNew, state.GetUserPath(chatId));
            await UploadFile(stream, ".json", botClient, chatId, cancellationToken);
            await js.Delete(state.GetUserPath(chatId));
        }

        private async Task AnsFileIsReady(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
        {
            ReplyKeyboardMarkup replyKeyboardMarkup = new(new[] { new KeyboardButton[] { "Sort up", "Sort down", "Filter" } })
            {
                ResizeKeyboard = true
            };
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Загрузка файла произошла успешно. Теперь Вы можете отфильтровать или отсортировать его. \n" +
                "Sort up - сортировка в алфавитном порядке по полю AvailableTransfer, \n" +
                "Sort down - сортировка в порядке убывания по полю YearOfComissioning.",
                replyMarkup: replyKeyboardMarkup,
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Prompts the user to choose a file format.
        /// </summary>
        private async Task AnsChooseFormat(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
        {
            ReplyKeyboardMarkup replyKeyboardMarkup = new(new[] { new KeyboardButton[] { "CSV", "JSON" } })
            {
                ResizeKeyboard = true
            };
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Файл успешно обработан! Выберите формат файла, который хотите получить.",
                replyMarkup: replyKeyboardMarkup,
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Prompts the user to enter filter words.
        /// </summary>
        private async Task AnsGetFilterWord(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken, bool IsMultivalued=false)
        {
            if (IsMultivalued)
            {
                await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Пришлите слова, по которому будет производиться выборка этих полей. \n" +
                "Необходимо прислать их через двоеточие, например \"проект:станция метро «Бульвар Дмитрия Донского»\"",
                replyMarkup: new ReplyKeyboardRemove(),
                cancellationToken: cancellationToken);
            }
            else
            {
                await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Пришлите слово, по которому будет производиться выборка этого поля.",
                replyMarkup: new ReplyKeyboardRemove(),
                cancellationToken: cancellationToken);
            }
        }

        /// <summary>
        /// Prompts the user to select a filter field.
        /// </summary>
        private async Task AnsGetFilterField(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
        {
            ReplyKeyboardMarkup replyKeyboardMarkup = new(new[] { new KeyboardButton[] { "District", "CarCapacity", "Status & NearStation" } })
            {
                ResizeKeyboard = true
            };
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Для начала нужно выбрать поле для выборки: \nРайон, количество машиномест, Статус объекта & Станция метро рядом. " +
                "(Нажмите соответствующую кнопку)",
                replyMarkup: replyKeyboardMarkup,
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Informs the user that an incorrect field was entered.
        /// </summary>
        private async Task AnsErrorField(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
        {
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Некорректно задано поле. Попробуйте ещё раз.",
                replyMarkup: new ReplyKeyboardRemove(),
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Prompts the user for further actions after processing a file.
        /// </summary>
        private async Task AnsProcessFile(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
        {
            ReplyKeyboardMarkup replyKeyboardMarkup = new(new[] { new KeyboardButton[] { "Sort up", "Sort down", "Filter", "Add a file to work with" } })
            {
                ResizeKeyboard = true
            };
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Вы можете продолжить работу с файлом, а можете начать работу с новым. \nНажмите соответствующие кнопки!",
                replyMarkup: replyKeyboardMarkup,
                cancellationToken: cancellationToken);
        }
    }
}
