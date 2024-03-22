using CsvHelper.Configuration;
using CsvHelper;
using System.Globalization;
using Telegram.Bot;

namespace DataModelsUtils
{
    public class CSVProcessing
    {
        public CSVProcessing() { }

        /// <summary>
        /// Reads data from the CSV file and converts it into an array of TransportPoints objects.
        /// </summary>
        /// <param name="stream">The stream containing the CSV data.</param>
        /// <param name="client">The TelegramBotClient object for sending messages.</param>
        /// <param name="chatId">The ID of the chat where messages will be sent.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
        /// <returns>An array of TransportPoints objects.</returns>
        public async Task<TransportPoints[]> Read(StreamReader stream, ITelegramBotClient client, long chatId,
        CancellationToken cancellationToken)
        {
            List<TransportPoints> transPoints = new List<TransportPoints>();
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
            };
            using StreamReader sr = stream;
            using var csvReader = new CsvReader(sr, config);
            try
            {
                await csvReader.ReadAsync();
                csvReader.ReadHeader();
                await csvReader.ReadAsync();
                transPoints = csvReader.GetRecords<TransportPoints>().ToList();
            }
            catch (CsvHelperException)
            {
                // Handle the case where the CSV file contains invalid data
                await client.SendTextMessageAsync(
                    chatId: chatId,
                    text: $"Данный файл некорректен, он содержит ошибочные данные.",
                    cancellationToken: cancellationToken);
                throw new FormatException();
            }
            return transPoints.ToArray();
        }

        /// <summary>
        /// Writes an array of TransportPoints objects to a CSV file.
        /// </summary>
        /// <param name="transPoints">The array of TransportPoints objects to write.</param>
        /// <param name="fileTitle">The title of the CSV file.</param>
        /// <returns>A FileStream representing the created CSV file.</returns>
        public async Task<FileStream> Write(TransportPoints[] transPoints, string fileTitle)
        {
            char separator = Path.DirectorySeparatorChar;
            string filePath = $"..{separator}..{separator}..{separator}..{separator}WorkingFiles{separator}out_{Path.GetFileNameWithoutExtension(fileTitle)}.csv";

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                HasHeaderRecord = false
            };

            await using (var streamWr = new StreamWriter(filePath))
            {
                await using (var csv = new CsvWriter(streamWr, config))
                {

                    streamWr.WriteLine("\"ID\";\"TPUName\";\"global_id\";\"AdmArea\";\"District\";\"NearStation\";\"YearOfComissioning" +
                                           "\";\"Status\";\"AvailableTransfer\";\"CarCapacity\";\"geodata_center\";\"geoarea\";");
                    streamWr.WriteLine("\"Локальный идентификатор\";\"Наименование транспортно-пересадочного узл\";\"global_id\";\"Административный округ по адресу\"" +
                        ";\"Район\";\"Станция метро или платформа, возле которой находится ТПУ\";\"Год ввода в эксплуатацию" +
                                           "\";\"Статус объекта\";\"Возможность пересадки\";\"Количество машиномест\";\"geodata_center\";\"geoarea\";");
                    await csv.WriteRecordsAsync(transPoints.ToList());
                }
            }

            return new FileStream(filePath, FileMode.Open);
        }
        public async Task Delete(string fileTitle)
        {
            var separator = Path.DirectorySeparatorChar;
            var filePath = $"..{separator}..{separator}..{separator}..{separator}WorkingFiles{separator}out_{Path.GetFileNameWithoutExtension(fileTitle)}.csv";

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
