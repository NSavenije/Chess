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

        public static Piece[] LoadFEN(string fen, out List<Piece> pieces)
        {
            //string startingPosition = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
            string[] ranks = fen.Split('/');
            Piece[] board = new Piece[64];
            pieces = new List<Piece>();
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
                    int pieceCode = Utils.GetPieceFromChar(c);
                    int square = (7 - rank) * 8 + file;
                    Piece piece = new Piece(pieceCode, square, 0);
                    pieces.Add(piece);
                    board[square] = piece;
                }
            }
            return board;
        }
    }
}
