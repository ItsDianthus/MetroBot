using Newtonsoft.Json;
using Telegram.Bot;

namespace DataModelsUtils
{
    public class JSONProcessing
    {
        public JSONProcessing() { }

        /// <summary>
        /// Reading json file.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="client"></param>
        /// <param name="chatId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<TransportPoints[]> Read(StreamReader stream, ITelegramBotClient client, long chatId,
            CancellationToken cancellationToken)
        {
            var transPoints = new List<TransportPoints>();
            string jsonString;
            using (var sr = stream)
            {
                jsonString = await stream.ReadToEndAsync();
            }
            try
            {
                transPoints = JsonConvert.DeserializeObject<List<TransportPoints>>(jsonString);
            }
            catch (JsonException ex)
            {
                await client.SendTextMessageAsync(
                    chatId: chatId,
                    text: $"Данный файл некорректен, он содержит ошибочные данные.",
                    cancellationToken: cancellationToken);
                throw new FormatException();
            }

            return transPoints!.ToArray();
        }

        /// <summary>
        /// Writing csv file.
        /// </summary>
        /// <param name="transPoints"></param>
        /// <param name="fileName"></param>
        /// <returns>Возвращает поток</returns>
        public async Task<FileStream> Write(TransportPoints[] transPoints, string fileTitle)
        {
            var separator = Path.DirectorySeparatorChar;
            var filePath = $"..{separator}..{separator}..{separator}..{separator}WorkingFiles{separator}out_{Path.GetFileNameWithoutExtension(fileTitle)}.json";

            await using (var streamWr = new StreamWriter(filePath))
            {
                var jsonString = JsonConvert.SerializeObject(transPoints, Formatting.Indented);
                await streamWr.WriteAsync(jsonString);
            }

            return new FileStream(filePath, FileMode.Open);
        }
        public async Task Delete(string fileTitle)
        {
            var separator = Path.DirectorySeparatorChar;
            var filePath = $"..{separator}..{separator}..{separator}..{separator}WorkingFiles{separator}out_{Path.GetFileNameWithoutExtension(fileTitle)}.json";

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            else
            {
                Console.WriteLine($"Файл по пути '{filePath}' не существует.");
            }
        }
    }
}
