// Stream 3 — COMMAND PATTERN consumer: turn-level undo/redo.
//
// A "turn" is one or two IGameCommand objects: both players' moves in HvH,
// or the human move and the computer's response in HvC (spec 4.2). One
// Undo()/Redo() always moves a whole turn, so control always lands back
// with whichever player it belongs to — never mid-turn. This class knows
// nothing about Gomoku or Reversi: it is built entirely on the seam Game.cs
// exposes for exactly this purpose (CommandExecuted, ApplyUndo, ApplyRedo),
// so the same CommandHistory works unchanged for all six variants.
//
// Grouping into turns is automatic and requires no cooperation from the CLI
// (Stream 4): Attach() subscribes to Game.CommandExecuted, and every
// executed command is folded into the turn currently being built. A turn is
// sealed - pushed onto the undo stack as one unit - the moment it holds two
// commands, or the moment the game ends (so the final, possibly solo, move
// of a finished game is still undoable).
using BoardGames.Core.Engine;
using BoardGames.Core.Rules;

namespace BoardGames.Core.Commands;

public sealed class CommandHistory
{
    private readonly List<List<IGameCommand>> undoStack = new List<List<IGameCommand>>();
    private readonly List<List<IGameCommand>> redoStack = new List<List<IGameCommand>>();
    private List<IGameCommand> pendingTurn = new List<IGameCommand>();
    private Game? game;

    // How many complete turns are currently undoable / redoable. Exposed
    // mainly for views and tests; Undo()/Redo() do not need it themselves.
    public int UndoableTurns
    {
        get { return undoStack.Count; }
    }

    public int RedoableTurns
    {
        get { return redoStack.Count; }
    }

    public bool CanUndo
    {
        get { return undoStack.Count > 0; }
    }

    public bool CanRedo
    {
        get { return redoStack.Count > 0; }
    }

    // True once the active player has played part of the turn currently
    // being formed (only possible in HvH, between the two players' moves).
    // Undo/redo are only ever offered to a player at the START of their
    // turn, before anything has been played in it - see Undo()/Redo().
    public bool TurnInProgress
    {
        get { return pendingTurn.Count > 0; }
    }

    // Wires this history up to a live game. Call once, right after the game
    // is created (fresh game) or reconstructed (SnapshotMapper.Load) - every
    // command Play() executes from that point on is recorded automatically.
    public void Attach(Game attachedGame)
    {
        if (game != null)
            throw new InvalidOperationException("This CommandHistory is already attached to a game.");
        game = attachedGame;
        game.CommandExecuted += OnCommandExecuted;
    }

    private void OnCommandExecuted(IGameCommand command)
    {
        pendingTurn.Add(command);

        // Branching history (spec 4.2): playing a move - the only way this
        // handler fires - immediately discards any redo history, since the
        // future it pointed to no longer matches the present.
        redoStack.Clear();

        // Game.Play() fires CommandExecuted BEFORE it (re)computes Result,
        // so game.Result here is still last move's verdict, not this one's.
        // WinCondition.Evaluate is a pure read of the current board/side,
        // so calling it again is safe and gives this move's real outcome -
        // the same one Play() is about to compute a moment later.
        bool turnHasBothMoves = pendingTurn.Count == 2;
        bool gameJustEnded = game!.WinCondition.Evaluate(game).State != GameState.InProgress;
        if (turnHasBothMoves || gameJustEnded)
            SealPendingTurn();
    }

    private void SealPendingTurn()
    {
        undoStack.Add(pendingTurn);
        pendingTurn = new List<IGameCommand>();
    }

    // Reverts the most recently completed turn - one command if it was the
    // game's first/last half-turn, otherwise both. Throws if there is
    // nothing to undo, or if the active player has already played part of
    // the turn now being formed (that half-played turn must finish before
    // anyone can reach further back).
    public void Undo()
    {
        RequireAttached();
        if (TurnInProgress)
            throw new InvalidMoveException(
                "Cannot undo once the current turn has started; finish the turn first.");
        if (!CanUndo)
            throw new InvalidMoveException("There is nothing to undo.");

        List<IGameCommand> turn = undoStack[undoStack.Count - 1];
        undoStack.RemoveAt(undoStack.Count - 1);
        for (int i = turn.Count - 1; i >= 0; i--)
            game!.ApplyUndo(turn[i]);
        redoStack.Add(turn);
    }

    // Re-applies the most recently undone turn. Same turn-in-progress guard
    // as Undo(), for the same reason.
    public void Redo()
    {
        RequireAttached();
        if (TurnInProgress)
            throw new InvalidMoveException(
                "Cannot redo once the current turn has started; finish the turn first.");
        if (!CanRedo)
            throw new InvalidMoveException("There is nothing to redo.");

        List<IGameCommand> turn = redoStack[redoStack.Count - 1];
        redoStack.RemoveAt(redoStack.Count - 1);
        foreach (IGameCommand command in turn)
            game!.ApplyRedo(command);
        undoStack.Add(turn);
    }

    private void RequireAttached()
    {
        if (game == null)
            throw new InvalidOperationException("CommandHistory is not attached to a game.");
    }
}
