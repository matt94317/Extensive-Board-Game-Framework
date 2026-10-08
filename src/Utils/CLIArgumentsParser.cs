using System;

namespace BoardGameFramework.Utils
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
                    ValidateAndProcessCommand(gameVariant, command);
                }

                Console.WriteLine("[CLI Test Mode] Script execution completed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CLI Test Error] Failed to execute script sequence: {ex.Message}");
            }
        }

        private static void ValidateAndProcessCommand(string variant, string command)
        {
            if (command.Equals("PASS", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("  -> Validated: PASS issued.");
                return;
            }

            if (command.Length < 4)
            {
                throw new FormatException($"Invalid command syntax: '{command}'. Expected format like '05:3' or 'P3:4'.");
            }

            char prefix = char.ToUpper(command[0]);
            Console.WriteLine($"  -> Validated: Command prefix '{prefix}' parsed successfully for {variant}.");
        }
    }
}
