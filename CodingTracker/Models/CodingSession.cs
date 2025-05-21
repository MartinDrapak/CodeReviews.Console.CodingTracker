using CodingTracker.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodingTracker.Models
{
    class CodingSession : ICodingSession
    {
        /// <summary>  
        /// Unique identifier for the coding session.  
        /// </summary>  
        public int Id { get; set; }

        /// <summary>  
        /// Start time of the coding session.  
        /// </summary>  
        public string SessionStart { get; set; } = string.Empty;

        /// <summary>  
        /// End time of the coding session.  
        /// </summary>  
        public string SessionEnd { get; set; } = string.Empty;

        /// <summary>  
        /// Duration of the coding session.  
        /// </summary>  
        public string Duration { get; set; } = string.Empty;

        /// <summary>  
        /// Constructor for the CodingSession class.  
        /// </summary>  
        /// <param name="sessionStart"></param>  
        /// <param name="sessionEnd"></param>  
        public CodingSession(string sessionStart, string sessionEnd)
        {
            SessionStart = sessionStart;
            SessionEnd = sessionEnd;
            Duration = CalculateDurationOfSession();
        }

        /// <summary>  
        /// Default constructor for the CodingSession class. Required by Dapper.  
        /// </summary>  
        public CodingSession() { }

        /// <summary>  
        /// Calls a method to DatabaseInteractor to save the session record to the database.  
        /// </summary>  
        /// <returns></returns>  
        public bool SaveSession()
        {
            try
            {
                DatabaseInteractor.CreateSessionRecord(this);
                return true;
            }
            catch (SQLiteException ex) // Catching a more specific exception type
            {
                UI.WriteError($"Database error: {ex.Message}");
                return false;
            }
            catch (FormatException ex) // Catching another specific exception type
            {
                UI.WriteError($"Formatting error: {ex.Message}");
                return false;
            }
        }

        /// <summary>  
        /// Calculates the duration of the session based on the start and end times.  
        /// </summary>  
        /// <returns></returns>  
        public string CalculateDurationOfSession()
        {
            // Calculate the duration of the session  
            if (SessionStart != null && SessionEnd != null)
            {
                DateTime start = DateTime.Parse(SessionStart,new CultureInfo("invariant"));
                DateTime end = DateTime.Parse(SessionEnd,new CultureInfo("invariant"));
                TimeSpan duration = end - start;
                return duration.ToString();
            }
            else
            {
                return "Session not started or ended.";
            }
        }
    }
}
