using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using File = System.IO.File;

namespace TelegramUtils
{
    /// <summary>
    /// Manages the state and file paths associated with each user.
    /// </summary>
    internal class UserState
    {
        /// <summary>
        /// Class representing the state and file paths for each user.
        /// </summary>
        public class State
        {
            // Getting the separator char for our operating system.
            private static readonly char Separator = Path.DirectorySeparatorChar;
            private readonly string _systemFile =
                    $"..{Separator}..{Separator}..{Separator}..{Separator}WorkingFiles{Separator}States.txt";
            private static Dictionary<long, List<string>> _allUsersState = new Dictionary<long, List<string>>();


            /// <summary>
            /// Adds a user to the dictionary with their initial state.
            /// </summary>
            /// <param name="chatId">The ID of the user's chat.</param>
            public void AddUserToDict(long chatId)
            {
                // ДОБАВИЛИ ЮЗЕРА В ФАЙЛИК ДЛЯ ЗАПОМИНАНИЯ
                _allUsersState[chatId] = new List<string>{ "start", "pathToFile"};
                string newStateLine = chatId.ToString() + ":start:path";

                using (var streamWriter = new StreamWriter(_systemFile))
                {
                    streamWriter.WriteLine(newStateLine);
                }
            }

            /// <summary>
            /// Gets the state of the specified user.
            /// </summary>
            /// <param name="chatId">The ID of the user's chat.</param>
            /// <returns>The state of the user.</returns>
            public string GetUserState(long chatId)
            {
                if (!_allUsersState.ContainsKey(chatId))
                {
                    AddUserToDict(chatId);
                }
                return _allUsersState[chatId][0];
            }

            /// <summary>
            /// Sets the state of the specified user.
            /// </summary>
            /// <param name="chatId">The ID of the user's chat.</param>
            /// <param name="newState">The new state to set.</param>
            public void SetUserState(long chatId, string newState)
            {
                if (!_allUsersState.ContainsKey(chatId))
                {
                    AddUserToDict(chatId);
                }
                _allUsersState[chatId][0] = newState;
            }

            /// <summary>
            /// Sets the file path for the specified user.
            /// </summary>
            /// <param name="chatId">The ID of the user's chat.</param>
            /// <param name="newPath">The new file path to set.</param>
            public void SetUserPath(long chatId, string newPath)
            {
                if (!_allUsersState.ContainsKey(chatId))
                {
                    AddUserToDict(chatId);
                }
                _allUsersState[chatId][1] = newPath;
            }

            /// <summary>
            /// Gets the file path associated with the specified user.
            /// </summary>
            /// <param name="chatId">The ID of the user's chat.</param>
            /// <returns>The file path associated with the user.</returns>
            public string GetUserPath(long chatId)
            {
                return _allUsersState[chatId][1];
            }

            /// <summary>
            /// Adds a new user entry to the system file if it does not already exist.
            /// </summary>
            /// <param name="chatId">The ID of the user's chat.</param>
            public void AddUser(long chatId)
            {
                var lines = new List<string>();

                using (var streamReader = new StreamReader(_systemFile))
                {
                    while (streamReader.ReadLine() is { } line)
                    {
                        lines.Add(line);
                    }
                }

                if (!lines.Any(line => line.Contains(chatId.ToString())))
                {
                    lines.Add($"{chatId}: pass: false");
                }

                using (var streamWriter = new StreamWriter(_systemFile))
                {
                    foreach (var line in lines)
                    {
                        streamWriter.WriteLine(line);
                    }
                }
            }

            /// <summary>
            /// Adds a file path to the user's entry in the system file.
            /// </summary>
            /// <param name="chatId">The ID of the user's chat.</param>
            /// <param name="filePath">The file path to add.</param>
            public void AddFileToUser(long chatId, string filePath)
            {
                var lines = new List<string>();

                using (var streamReader = new StreamReader(_systemFile))
                {
                    while (streamReader.ReadLine() is { } line)
                    {
                        lines.Add(line);
                    }
                }

                foreach (var line in lines.Where(line => line.Contains(chatId.ToString())))
                {
                    var values = line.Split(": ");
                    lines[Array.IndexOf(lines.ToArray(), line)] = $"{values[0]}: {filePath}: {values[2]}";
                    break;
                }

                using (var streamWriter = new StreamWriter(_systemFile))
                {
                    foreach (var line in lines)
                    {
                        streamWriter.WriteLine(line);
                    }
                }
            }

        }
    }
}
