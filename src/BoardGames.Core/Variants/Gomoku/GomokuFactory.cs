// Stream 1 — shared base of the three Gomoku factories. Everything the
// variants have in common is created here exactly once; each leaf factory
// overrides only the product that genuinely differs.
using BoardGames.Core.Engine;
using BoardGames.Core.Model.Pieces;
using BoardGames.Core.Players.Strategies;
using BoardGames.Core.Rules;
using BoardGames.Core.Rules.Gomoku;

namespace BoardGames.Core.Variants.Gomoku;

public abstract class GomokuFactory : IGameFactory
{
    public string GameKey
    {
        get { return "gomoku"; }
    }

    public abstract string VariantKey { get; }

    protected abstract string VariantName { get; }
    protected virtual bool SpecialStonesEnabled { get { return false; } }
    protected virtual bool HiddenInformation { get { return false; } }

    public virtual IMoveRules CreateMoveRules()
    {
        return new GomokuMoveRules(SpecialStonesEnabled);
    }

    public virtual IWinCondition CreateWinCondition()
    {
        return new FiveInARowCondition();
    }

    public virtual IPerspective CreatePerspective()
    {
        return new FullPerspective();
    }

    public virtual IPieceFactory CreatePieceFactory()
    {
        return new GomokuPieceFactory();
    }

    public virtual IMoveStrategy CreateSmartStrategy()
    {
        return new GomokuSmartStrategy();
    }

    public Game CreateGame()
    {
        return new GomokuGame(VariantName, CreateMoveRules(), CreateWinCondition(),
                              CreatePerspective(), CreatePieceFactory(),
                              SpecialStonesEnabled, HiddenInformation);
    }
}
