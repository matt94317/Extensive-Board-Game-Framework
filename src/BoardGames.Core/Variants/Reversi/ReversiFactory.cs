// Stream 2 — shared base of the three Reversi factories. Rules, perspective
// and pieces are identical across the family and created here once; each
// leaf factory overrides only its win condition (and the matching help text).
using BoardGames.Core.Engine;
using BoardGames.Core.Model.Pieces;
using BoardGames.Core.Rules;
using BoardGames.Core.Rules.Reversi;

namespace BoardGames.Core.Variants.Reversi;

public abstract class ReversiFactory : IGameFactory
{
    public string GameKey
    {
        get { return "reversi"; }
    }

    public abstract string VariantKey { get; }

    protected abstract string VariantName { get; }
    protected abstract string WinRule { get; }

    public virtual IMoveRules CreateMoveRules()
    {
        return new ReversiMoveRules(new FlankingEngine(), CreatePieceFactory());
    }

    public abstract IWinCondition CreateWinCondition();

    public virtual IPerspective CreatePerspective()
    {
        return new FullPerspective();
    }

    public virtual IPieceFactory CreatePieceFactory()
    {
        return new ReversiPieceFactory();
    }

    public Game CreateGame()
    {
        return new ReversiGame(VariantName, WinRule, CreateMoveRules(), CreateWinCondition(),
                               CreatePerspective(), CreatePieceFactory());
    }
}
