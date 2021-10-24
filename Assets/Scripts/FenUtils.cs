using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    public static class FenUtils
    {
        public static string StartingPosition = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR";

        public static int[] LoadFEN(string fen)
        {
            //string startingPosition = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
            string[] ranks = fen.Split('/');
            int[] board = new int[64];
            for (int rank = 7; rank >= 0; rank--)
            {
                for (int file = 0; file < 8; file++)
                {
                    char c = ranks[rank][file];
                    if (char.IsNumber(c))
                    {
                        file += (int)char.GetNumericValue(c);
                        continue;
                    }
                    int piece = Utils.GetPieceFromChar(c);
                    board[(7 - rank) * 8 + file] = piece;
                }
            }
            return board;
        }
    }
}
