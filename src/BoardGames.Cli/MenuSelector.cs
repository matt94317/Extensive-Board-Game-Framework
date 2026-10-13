// Stream 4 — arrow-key menus for the interactive terminal. Up/Down moves
// the highlight, Enter picks it, and each option's shortcut key (1-6, L,
// H, Q, ...) picks that option straight away. Only the option lines are
// redrawn in place, so the rest of the screen is left alone. When input or
// output is redirected (piped scripts, CI) there is no keyboard to read,
// so callers fall back to typed answers instead.
namespace BoardGames.Cli;

public sealed class MenuOption
{
    // Single character that selects the option directly, e.g. "1" or "Q".
    public string Key { get; }
    public string Label { get; }

    public MenuOption(string key, string label)
    {
        Key = key;
        Label = label;
    }
}

public static class MenuSelector
{
    private const string Marker = "> ";
    private const string Indent = "  ";

    // True when there is a real keyboard and terminal to draw on.
    public static bool IsAvailable
    {
        get { return !Console.IsInputRedirected && !Console.IsOutputRedirected; }
    }

    // Shows the options and returns the index of the chosen one, or null
    // when the player presses Escape and cancelling is allowed.
    public static int? Select(IReadOnlyList<MenuOption> options, bool allowCancel)
    {
        Console.WriteLine(allowCancel
            ? "  (Up/Down to move, Enter to choose, Esc to go back)"
            : "  (Up/Down to move, Enter to choose)");

        int selected = 0;
        for (int i = 0; i < options.Count; i++)
        {
            WriteOption(options[i], i == selected);
            Console.WriteLine();
        }
        // Measured after drawing, so it stays right if the drawing scrolled.
        int top = Console.CursorTop - options.Count;

        Console.CursorVisible = false;
        try
        {
            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                int previous = selected;
                switch (key.Key)
                {
                    case ConsoleKey.UpArrow:
                        selected = (selected - 1 + options.Count) % options.Count;
                        break;
                    case ConsoleKey.DownArrow:
                        selected = (selected + 1) % options.Count;
                        break;
                    case ConsoleKey.Enter:
                        Console.SetCursorPosition(0, top + options.Count);
                        return selected;
                    case ConsoleKey.Escape:
                        if (!allowCancel)
                            continue;
                        Console.SetCursorPosition(0, top + options.Count);
                        return null;
                    default:
                        int match = FindShortcut(options, key.KeyChar);
                        if (match < 0)
                            continue;
                        selected = match;
                        Redraw(options, top, previous, selected);
                        Console.SetCursorPosition(0, top + options.Count);
                        return selected;
                }
                Redraw(options, top, previous, selected);
            }
        }
        finally
        {
            Console.CursorVisible = true;
        }
    }

    // Rewrites only the two lines whose highlight changed.
    private static void Redraw(IReadOnlyList<MenuOption> options, int top, int previous, int selected)
    {
        Console.SetCursorPosition(0, top + previous);
        WriteOption(options[previous], false);
        Console.SetCursorPosition(0, top + selected);
        WriteOption(options[selected], true);
    }

    private static void WriteOption(MenuOption option, bool highlighted)
    {
        if (!highlighted)
        {
            Console.Write(Indent + option.Label);
            return;
        }
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write(Marker + option.Label);
        Console.ResetColor();
    }

    private static int FindShortcut(IReadOnlyList<MenuOption> options, char typed)
    {
        string text = typed.ToString();
        for (int i = 0; i < options.Count; i++)
            if (string.Equals(options[i].Key, text, StringComparison.OrdinalIgnoreCase))
                return i;
        return -1;
    }
}
