// Stream 1 — the heart of the framework.
//
// TEMPLATE METHOD PATTERN: Play(move) is the one fixed turn algorithm every
// variant uses — validate, create command, execute, log, evaluate outcome,
// swap sides, notify observers — in that order, always. What varies between
// games is plugged in as strategies (IMoveRules, IWinCondition,
// IPerspective) chosen by the variant's factory, plus the small hooks below.
//
// OBSERVER PATTERN: the game is the subject; views attach via Attach() and
// are pushed a GameEvent after every change (see GameEvent.cs).
//
// Stream 3 seam: executed commands are logged chronologically in MoveLog and
// announced through OnCommandExecuted(); the undo/redo stacks (CommandHistory)
// build on exactly these two members.
using BoardGames.Core.Commands;
using BoardGames.Core.Model;
using BoardGames.Core.Rules;

namespace BoardGames.Core.Engine;

public abstract class Game
{
    private readonly List<IGameObserver> observers = new List<IGameObserver>();
    private readonly List<IGameCommand> moveLog = new List<IGameCommand>();

    public Board Board { get; }
    public IMoveRules Rules { get; }
    public IWinCondition WinCondition { get; }
    public IPerspective Perspective { get; }

    public PlayerSide ActiveSide { get; protected set; } = PlayerSide.PlayerOne;
    public int TurnCount { get; protected set; }
    public GameResult Result { get; protected set; } = GameResult.InProgress();

    protected Game(Board board, IMoveRules rules,
                   IWinCondition winCondition, IPerspective perspective)
    {
        Board = board;
        Rules = rules;
        WinCondition = winCondition;
        Perspective = perspective;
    }

    // ---- identity, supplied by each family/variant ----
    public abstract string FamilyName { get; }
    public abstract string VariantName { get; }

    // Polymorphic in-game help: family, variant, rules and command syntax.
    public abstract string GetHelp();

    // ---- the template method: one fixed turn for all six variants ----
    public void Play(Move move)
    {
        if (Result.State != GameState.InProgress)
            throw new InvalidMoveException("The game is already over.");

        Rules.Validate(this, move);                       // variant strategy
        IGameCommand command = Rules.CreateCommand(this, move);
        command.Execute(this);
        moveLog.Add(command);
        OnCommandExecuted(command);                       // Stream 3 hook
        TurnCount++;
        Result = WinCondition.Evaluate(this);             // variant strategy
        if (Result.State == GameState.InProgress)
            ActiveSide = PlayerSides.Opponent(ActiveSide);
        Notify(FamilyName + " move played");
    }

    // Chronological record of every executed command (oldest first). Save /
    // load (Stream 3) replays it; undo/redo stacks are built from it.
    public IReadOnlyList<IGameCommand> MoveLog
    {
        get { return moveLog; }
    }

    // Hook for Stream 3: called straight after a command executes, before
    // the outcome check. CommandHistory records the command (and clears any
    // redo branch) here, without changing the template method itself.
    protected virtual void OnCommandExecuted(IGameCommand command) { }

    // ---- observer (subject) interface ----
    public void Attach(IGameObserver observer)
    {
        observers.Add(observer);
    }

    public void Detach(IGameObserver observer)
    {
        observers.Remove(observer);
    }

    public void Announce(string description)
    {
        Notify(description);
    }

    protected void Notify(string description)
    {
        GameEvent gameEvent = new GameEvent(this, description);
        foreach (IGameObserver observer in observers)
            observer.Update(gameEvent);
    }

    // One-line summary a view can print under the board.
    public virtual string StatusLine()
    {
        if (Result.State == GameState.Won)
            return "*** " + PlayerSides.DisplayName(Result.Winner) + " wins! ***";
        if (Result.State == GameState.Draw)
            return "*** The game is a draw. ***";
        return "Turn " + (TurnCount + 1) + " - "
             + PlayerSides.DisplayName(ActiveSide) + " to move.";
    }
}
