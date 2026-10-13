// Stream 4 - the six variants the program can play. This is the only place in
// the CLI that names the concrete factories (ABSTRACT FACTORY); everything
// else works through IGameFactory, so adding a new game means adding one line
// here and no changes anywhere else.
using BoardGames.Cli.Parsing;
using BoardGames.Core.Variants;
using BoardGames.Core.Variants.Gomoku;
using BoardGames.Core.Variants.Reversi;

namespace BoardGames.Cli
{
    public static class GameSelector
    {
        public static List<IGameFactory> AllVariants()
        {
            return new List<IGameFactory>
            {
                new StandardGomokuFactory(),
                new GomokuPlusFactory(),
                new GomokuFogFactory(),
                new StandardReversiFactory(),
                new AntiReversiFactory(),
                new CornerReversiFactory()
            };
        }

        // Finds a variant by its --game and --variant keys, e.g. "reversi" "anti".
        public static IGameFactory? FindFactory(string gameKey, string variantKey)
        {
            foreach (IGameFactory factory in AllVariants())
            {
                if (factory.GameKey == gameKey.ToLower() && factory.VariantKey == variantKey.ToLower())
                    return factory;
            }
            return null;
        }

        public static IMoveParser CreateParser(IGameFactory factory)
        {
            if (factory.GameKey == "gomoku")
                return new GomokuMoveParser(factory.VariantKey == "plus");
            return new ReversiMoveParser();
        }
    }
}
