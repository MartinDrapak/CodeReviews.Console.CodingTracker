using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CodingTracker.Models;
using Spectre.Console;
#pragma warning disable IDE0130 //Keep the namespace for easy access
namespace CodingTracker
{
   
    static class UI
    {

        #region methods
        ///<summary>
        /// Displays the main menu to the user and returns the selected option.
        /// </summary>
        public static string MainMenu()
        {
            try
            {
                WriteMessage("Main Menu", "blue");
                WriteMessage("1. Add a new coding session", "gray");
                WriteMessage("2. Start a timed coding session", "gray");
                WriteMessage("3. View coding sessions", "gray");
                WriteMessage("4. Exit", "gray");
                return Console.ReadLine() ?? string.Empty; // Ensure a non-null return value
            }
            catch (IOException ex)
            {
                AnsiConsole.MarkupLine($"[red]I/O Error: {ex.Message}[/]");
                return string.Empty; // Return a default value in case of an exception
            }
            catch (UnauthorizedAccessException ex)
            {
                AnsiConsole.MarkupLine($"[red]Access Error: {ex.Message}[/]");
                return string.Empty; // Return a default value in case of an exception
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Unexpected Error: {ex.Message}[/]");
                throw; // Rethrow the exception to allow higher-level handling
            }
        }
        /// <summary>
        /// Starts a timed coding session.
        /// </summary>
        public static void StartTimedSession()
        {
            try
            {
                WriteMessage("Type 'start' to begin your coding session:", "yellow");
                while (Console.ReadLine()?.Trim().ToLower(System.Globalization.CultureInfo.CurrentCulture) != "start")
                {
                    WriteMessage("Please type 'start' to begin.", "yellow");
                }

                DateTime sessionStart = DateTime.Now;
                WriteMessage($"Session started at {sessionStart:yyyy-MM-dd HH:mm:ss}", "green");

                bool timerIsRunning = true;
                UI.WriteMessage("Type 'stop' and press Enter to end the session.");

                var timerTask = Task.Run(() =>
                {
                    while (timerIsRunning)
                    {
                        if (!Console.KeyAvailable)
                        {
                            var elapsed = DateTime.Now - sessionStart;
                            string message = $"Time elapsed: {elapsed:hh\\:mm\\:ss}";
                            int left = Console.CursorLeft, top = Console.CursorTop;
                            Console.SetCursorPosition(0, top);
                            Console.Write($"{message.PadRight(Console.WindowWidth - 1)}");
                            Console.SetCursorPosition(left, top);
                        }
                        Thread.Sleep(1000);
                    }
                    Console.WriteLine();
                });

                // Read input in the main thread
                string? input;
                do
                {
                    input = Console.ReadLine()?.Trim().ToLower(System.Globalization.CultureInfo.CurrentCulture);
                } while (input != "stop");

                timerIsRunning = false;
                DateTime sessionEnd = DateTime.Now;

                // Use CultureInfo.InvariantCulture to ensure consistent formatting
                CodingSession session = new(
                    sessionStart.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture),
                    sessionEnd.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture)
                );

                session.SaveSession();
                WriteMessage($"Session stopped at {sessionEnd:yyyy-MM-dd HH:mm:ss}", "green");
                WriteMessage($"Total duration: {(sessionEnd - sessionStart):hh\\:mm\\:ss}", "green");
                WriteMessage("____________________________", "white");
            }
            catch (IOException ex)
            {
                WriteError($"I/O Error: {ex.Message}");
            }
            catch (UnauthorizedAccessException ex)
            {
                WriteError($"Access Error: {ex.Message}");
            }
            catch (FormatException ex)
            {
                WriteError($"Format Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                WriteError($"Unexpected Error: {ex.Message}");
                throw; // Rethrow the exception to allow higher-level handling
            }
        }
        /// <summary>
        /// Creates a new session record in the database.
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        public static bool CreateSessionRecord(CodingSession session)
        {
            try
            {
                DatabaseInteractor.CreateSessionRecord(session);
                WriteMessage($"Session created successfully! Start: {session.SessionStart}, End: {session.SessionEnd}, Duration: {session.Duration}", "green");
                return true;
            }
            catch (InvalidOperationException ex)
            {
                WriteError($"Invalid operation: {ex.Message}");
                return false;
            }
            catch (SQLiteException ex)
            {
                WriteError($"Database error: {ex.Message}");
                return false;
            }
            catch (Exception ex) when (ex is ArgumentNullException || ex is FormatException)
            {
                WriteError($"Input error: {ex.Message}");
                return false;
            }
        }
        /// <summary>
        /// Gets input from the user for the coding session and calls a method that creates a new session record.
        /// </summary>
        /// <returns></returns>
        public static bool CreateSessionRecordManually()
        {
            try
            {
                string startTime;
                while (true)
                {
                    UI.WriteMessage("Enter the start time of the session (yyyy-MM-dd HH:mm:ss):", "white");
                    startTime = Console.ReadLine() ?? string.Empty; // Ensure a non-null value is assigned
                    if (DateTime.TryParseExact(startTime, "yyyy-MM-dd HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out _))
                        break;
                    UI.WriteError("Invalid format. Please use yyyy-MM-dd HH:mm:ss.[/]");
                }

                string endTime;
                while (true)
                {
                    UI.WriteMessage("Enter the end time of the session (yyyy-MM-dd HH:mm:ss):", "white");
                    endTime = Console.ReadLine() ?? string.Empty; // Ensure a non-null value is assigned
                    if (DateTime.TryParseExact(endTime, "yyyy-MM-dd HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out _))
                        break;
                    UI.WriteError("Invalid format. Please use yyyy-MM-dd HH:mm:ss.");
                }

                CodingSession session = new(startTime, endTime);
                DatabaseInteractor.CreateSessionRecord(session);
                UI.WriteMessage($"Session created successfully! Start: {session.SessionStart}, End: {session.SessionEnd}, Duration: {session.Duration}", "green");
                return true;
            }
            catch (ArgumentNullException ex)
            {
                UI.WriteError($"Input error: {ex.Message}");
                return false;
            }
            catch (FormatException ex)
            {
                UI.WriteError($"Input format error: {ex.Message}");
                return false;
            }
            catch (SQLiteException ex)
            {
                UI.WriteError($"Database error: {ex.Message}");
                return false;
            }
            catch (InvalidOperationException ex)
            {
                UI.WriteError($"Invalid operation: {ex.Message}");
                return false;
            }
        }
        /// <summary>
        /// Displays all coding sessions to the user.
        /// </summary>
        /// <returns></returns>
        public static bool ViewAllSessions()
        {
            try
            {
                List<CodingSession> sessions = DatabaseInteractor.GetAllSessions();
                if (sessions.Count == 0)
                {
                    UI.WriteMessage("No sessions found.");
                    return false;
                }
                else
                {
                    UI.WriteMessage("All Coding Sessions:", "green");
                    foreach (var session in sessions)
                    {
                        UI.WriteMessage($"Session ID: {session.Id}, Start: {session.SessionStart}, End: {session.SessionEnd}, Duration: {session.Duration}");
                        UI.WriteMessage("---------------------------------------");
                    }
                    return true;
                }
            }
            catch (SQLiteException ex)
            {
                UI.WriteError($"Database error: {ex.Message}");
                return false;
            }
            catch (InvalidOperationException ex)
            {
                UI.WriteError($"Invalid operation: {ex.Message}");
                return false;
            }
        }
        /// <summary>
        /// Evaluates the user's input from the main menu and performs the corresponding action.
        /// </summary>
        /// <param name="input"></param>
        public static void EvaluateInput(string input)
        {
            switch (input)
            {
                case "1":
                    CreateSessionRecordManually();
                    EvaluateInput(MainMenu());
                    break;
                case "2":
                    StartTimedSession();
                    EvaluateInput(MainMenu());
                    break;
                case "3":
                    ViewAllSessions();
                    EvaluateInput(MainMenu());
                    break;
                case "4":
                    Environment.Exit(0);
                    break;
                default:
                    WriteError("Invalid input. Please try again.");
                    EvaluateInput(MainMenu());
                    break;
            }

        }
        /// <summary>
        /// Displays a welcome message to the user.
        /// </summary>
        /// <returns></returns>
        public static bool WelcomeMessage()
        {
            try
            {
                AnsiConsole.Write(new Rule("[green]Welcome to Coding Tracker![/]").Centered());
                AnsiConsole.MarkupLine("[white]To start select an option from the menu below. [/]");
                return true;
            }
            catch (System.IO.IOException ex)
            {
                AnsiConsole.MarkupLine($"[red]I/O Error: {ex.Message}[/]");
                return false;
            }
            catch (UnauthorizedAccessException ex)
            {
                AnsiConsole.MarkupLine($"[red]Access Error: {ex.Message}[/]");
                return false;
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Unexpected Error: {ex.Message}[/]");
                throw; // Rethrow the exception to allow higher-level handling
            }
        }
        /// <summary>
        /// Displays a message to the user.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool WriteMessage(string input)
        {
            try
            {
                AnsiConsole.MarkupLine($"[white]{input}[/]");
                return true;
            }
            catch (System.IO.IOException ex)
            {
                AnsiConsole.MarkupLine($"[red]I/O Error: {ex.Message}[/]");
                return false;
            }
            catch (UnauthorizedAccessException ex)
            {
                AnsiConsole.MarkupLine($"[red]Access Error: {ex.Message}[/]");
                return false;
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Unexpected Error: {ex.Message}[/]");
                throw; // Rethrow the exception to allow higher-level handling
            }
        }
        /// <summary>
        /// Displays a message to the user with a specified color.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public static bool WriteMessage(string input, string color)
        {
            try
            {
                AnsiConsole.MarkupLine($"[{color}]{input}[/]");
                return true;
            }
            catch (System.IO.IOException ex)
            {
                AnsiConsole.MarkupLine($"[red]I/O Error: {ex.Message}[/]");
                return false;
            }
            catch (UnauthorizedAccessException ex)
            {
                AnsiConsole.MarkupLine($"[red]Access Error: {ex.Message}[/]");
                return false;
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Unexpected Error: {ex.Message}[/]");
                throw; // Rethrow the exception to allow higher-level handling
            }
        }
        /// <summary>
        /// Displays an error message to the user.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool WriteError(string input)
        {
            try
            {
                AnsiConsole.MarkupLine($"[red]{input}[/]");
                return true;
            }
            catch (IOException ex)
            {
                AnsiConsole.MarkupLine($"[red]I/O Error displaying error: {ex.Message}[/]");
                return false;
            }
            catch (UnauthorizedAccessException ex)
            {
                AnsiConsole.MarkupLine($"[red]Access Error displaying error: {ex.Message}[/]");
                return false;
            }
            catch (Exception ex)
            {
                // Rethrow the exception to avoid suppressing unexpected errors
                throw new InvalidOperationException("An unexpected error occurred while displaying an error message.", ex);
            }
        }
        #endregion
    }
}