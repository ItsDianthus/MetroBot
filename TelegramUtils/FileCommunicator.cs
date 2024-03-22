using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramUtils
{
    /// <summary>
    /// Class responsible for file-related operations.
    /// </summary>
    public class FileCommunicator
    {
        /// <summary>
        /// Initializes a new instance of the FileCommunicator class.
        /// </summary>
        public FileCommunicator() { }

        /// <summary>
        /// Creates the main directory and the state file if they do not exist.
        /// </summary>
        public void CreateMaindir()
        {
            char separator = Path.DirectorySeparatorChar;
            string pathToWorkdir = $"..{separator}..{separator}..{separator}..{separator}WorkingFiles";
            string pathToStateFile = $"..{separator}..{separator}..{separator}..{separator}WorkingFiles{separator}States.txt";

            try
            {
                // Create the main directory if it does not exist
                if (!Directory.Exists(pathToWorkdir))
                {
                    Directory.CreateDirectory(pathToWorkdir);
                }

                // Create the state file if it does not exist
                if (!File.Exists(pathToWorkdir))
                {
                    File.Create(pathToStateFile);
                }

            }
            catch (UnauthorizedAccessException ex)
            {
                // Handle the case where the program does not have permission to create the necessary directory
                Console.WriteLine($"Программа не имеет разрешения на создание нужной для ее корректной работы папки!: {ex}");
            }
        }
    }
}
