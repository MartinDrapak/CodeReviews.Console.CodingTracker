using CodingTracker;
using Spectre.Console;

namespace CodingTracker
{
    class Program
    {
        DatabaseInteractor Db { get; set; }
        public static void Main()
        {
            try
            {
                Program program = new();
            }
            catch (InvalidOperationException ex)
            {
                UI.WriteError($"Invalid operation: {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                UI.WriteError($"Argument error: {ex.Message}");
            }
            catch (Exception ex)
            {
                UI.WriteError($"An unexpected error occurred: {ex.Message}");
                throw;
            }
        }
        #region properties
        #endregion
        #region constructors
        Program()
        {
            Db = new();
            UI.WelcomeMessage();
            UI.EvaluateInput(UI.MainMenu());
            Dispose();
        }
        #endregion
        #region methods
        public static void Dispose()
        {
            DatabaseInteractor.Dispose();
        }
        #endregion
    }
}