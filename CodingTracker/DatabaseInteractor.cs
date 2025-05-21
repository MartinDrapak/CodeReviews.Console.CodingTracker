using CodingTracker.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using Dapper;

namespace CodingTracker
{
    /// <summary>
    /// This class is responsible for interacting with the SQLite database.
    /// </summary>
    class DatabaseInteractor
    {
        #region properties
        /// <summary>
        /// SQLite connection object.
        /// </summary>
        public static SQLiteConnection? Connection { get; set; }
        #endregion
        #region constructors
        /// <summary>
        /// Initializes a new instance of the DatabaseInteractor class and creates an SQLite connection.
        /// </summary>
        public DatabaseInteractor()
        {
            if (Connection == null)
            {
                Connection = CreateSqliteConnection();
                CreateDatabaseFile();
                EnsureCodingTrackerTable();
            }
        }
        #endregion
        #region methods
        /// <summary>
        /// Closes the database connection and disposes of the object.
        /// </summary>
        public static void Dispose()
        {
            if (Connection != null)
            {
                Connection.Close();
                Connection.Dispose();
                Connection = null;
            }
        }
        /// <summary>
        /// Retrieves all coding sessions from the database.
        /// </summary>
        /// <returns>List<CodingSession></returns>
        public static List<CodingSession> GetAllSessions()
        {
            string selectQuery = "SELECT * FROM CodingSession;";
            try
            {
                if (Connection == null)
                {
                    throw new InvalidOperationException("Database connection is not initialized.");
                }

                // Simplified collection initialization
                return Connection.Query<CodingSession>(selectQuery).ToList() ?? [];
            }
            catch (SQLiteException ex)
            {
                UI.WriteError("Database error retrieving sessions: " + ex.Message);
                return [];
            }
            catch (InvalidOperationException ex)
            {
                UI.WriteError("Invalid operation: " + ex.Message);
                return [];
            }
        }
        /// <summary>
        /// Inserts a new coding session record into the database.
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        public static bool CreateSessionRecord(CodingSession session)
        {
            string insertQuery = @"INSERT INTO CodingSession(SessionStart, SessionEnd, Duration)
            VALUES(@SessionStart, @SessionEnd, @Duration);";

            try
            {
                if (Connection == null)
                {
                    throw new InvalidOperationException("Database connection is not initialized.");
                }

                int rowsAffected = Connection.Execute(insertQuery, new
                {
                    session.SessionStart,
                    session.SessionEnd,
                    session.Duration
                });

                return rowsAffected > 0;
            }
            catch (SQLiteException ex)
            {
                UI.WriteError("Database error inserting session: " + ex.Message);
                return false;
            }
            catch (InvalidOperationException ex)
            {
                UI.WriteError("Invalid operation: " + ex.Message);
                return false;
            }
        }
        /// <summary>
        /// Creates the database file if it does not exist.
        /// </summary>
        /// <returns>True if successful false if not</returns>
        public static bool CreateDatabaseFile()
        {
            try
            {
                if (!System.IO.File.Exists("Database/CodingTracker.db"))
                {
                    // Create the database file
                    using var stream = System.IO.File.Create("Database/CodingTracker.db");
                    // Close the stream
                    stream.Close();
                }
                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                UI.WriteError("Access denied while creating database file: " + ex.Message);
                return false;
            }
            catch (IOException ex)
            {
                UI.WriteError("I/O error while creating database file: " + ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                // Rethrow unexpected exceptions to avoid swallowing critical errors
                throw new InvalidOperationException("Unexpected error while creating database file.", ex);
            }
        }
        /// <summary>
        /// Creates a SQLite connection using the connection string from the app.config file.
        /// </summary>
        /// <returns>SQLiteConnection</returns>
        public static SQLiteConnection CreateSqliteConnection()
        {
            string? connectionString = System.Configuration.ConfigurationManager.AppSettings.Get("ConnectionString");
            SQLiteConnection connection = new(connectionString);
            try
            {
                connection.Open();
            }
            catch (SQLiteException ex)
            {
                UI.WriteError("Error opening database connection: " + ex.Message);
                throw; // Rethrow the exception to avoid swallowing it
            }
            return connection;
        }

        /// <summary>
        /// Ensures that the CodingSession table exists in the database.
        /// </summary>
        /// <returns>True if successful, false if not</returns>
        public static bool EnsureCodingTrackerTable()
        {
            try
            {
                if (Connection == null)
                {
                    throw new InvalidOperationException("Database connection is not initialized.");
                }

                string checkTableQuery = "SELECT name FROM sqlite_master WHERE type='table' AND name='CodingSession';";
                var result = Connection.ExecuteScalar<string>(checkTableQuery);

                if (result == null)
                {
                    // Table does not exist, create it using Dapper
                    string createTableQuery = Resources.Resources.EnsureCodingSessionSQL;
                    Connection.Execute(createTableQuery);
                }

                return true;
            }
            catch (SQLiteException ex)
            {
                UI.WriteError("SQLite error ensuring CodingSession table: " + ex.Message);
                return false;
            }
            catch (InvalidOperationException ex)
            {
                UI.WriteError("Invalid operation: " + ex.Message);
                return false;
            }
        }
        #endregion
    }
}
