using Spectre.Console;

namespace CodingTracker
{
    class Program
    {
        DatabaseInteractor Db { get; set; } = new();
        public static void Main()
        {
            try
            {
                UI.WelcomeMessage();
                UI.EvaluateInput(UI.MainMenu());
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
       
        #endregion
        #region methods
        public static void Dispose()
        {
            DatabaseInteractor.Dispose();
        }
        #endregion
    }
}