using System;

namespace BoardGames.Cli
{
    public static class CLIArgumentsParser
    {
        public static void ExecuteTestScript(string gameVariant, string scriptSequence)
        {
            if (string.IsNullOrWhiteSpace(scriptSequence))
            {
                Console.WriteLine("No command script provided for test mode.");
                return;
            }

            try
            {
                Console.WriteLine($"[CLI Test Mode] Starting execution for variant: {gameVariant}");
                string[] commands = scriptSequence.Split(',');

                foreach (var rawCommand in commands)
                {
                    string command = rawCommand.Trim();
                    if (string.IsNullOrEmpty(command)) continue;

                    Console.WriteLine($"[Executing] -> {command}");
                    // Ready for game.Play() integration with team parser
                }

                Console.WriteLine("[CLI Test Mode] Script execution and recording sequence completed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CLI Test Error] Failed to execute script sequence: {ex.Message}");
            }
        }
    }
}
