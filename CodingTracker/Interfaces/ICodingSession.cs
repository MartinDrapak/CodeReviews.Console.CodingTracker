using System.Linq;

namespace CodingTracker.Interfaces
{
     interface ICodingSession
    {
        #region properties
        int Id { get; set; }
        string SessionStart { get; set; }
        string SessionEnd { get; set; }
        string Duration { get; set; }
        #endregion
        #region methods
        public bool SaveSession();
        public string CalculateDurationOfSession();

        #endregion
    }
}
