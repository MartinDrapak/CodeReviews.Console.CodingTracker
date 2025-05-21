using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
