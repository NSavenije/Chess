using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public static class FenUtils
    {
        public const string StartingPosition = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        public static Board LoadFEN(string fen = StartingPosition)
        {
            //string startingPosition = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
            string[] data = fen.Split(' ');
            string[] ranks = data[0].Split('/');
            List<Piece> pieces = new List<Piece>();
            for (int rank = 7; rank >= 0; rank--)
            {
                int f = 0;
                for (int file = 0; file < ranks[rank].Length; file++)
                {
                    string r = ranks[rank];
                    Debug.Log($"{r}: {file}");
                    char c = r[file];
                    if (char.IsNumber(c))
                    {
                        f += (int)char.GetNumericValue(c);
                        continue;
                    }
                    int pieceCode = Utils.GetPieceFromChar(c);
                    int square = (7 - rank) * 8 + f;

                    Piece piece = new Piece(pieceCode, square, 0);
                    pieces.Add(piece);
                    f++;
                }
            }
            //Active colour which is either 'w' or 'b'.
            bool turnWhite = data[1] == "w";

            //Castling availability (KQkq) white kingside queenside; black kingside queenside.
            Dictionary<string, bool> castles = new Dictionary<string, bool>
            {
                { "K", true },
                { "Q", true },
                { "k", true },
                { "q", true }
            };
            if (!data[2].Contains("K"))
                castles["K"] = false;
            if (!data[2].Contains("k"))
                castles["k"] = false;
            if (!data[2].Contains("Q"))
                castles["Q"] = false;
            if (!data[2].Contains("q"))
                castles["q"] = false;

            int enPassantSquare = -1;
            if (data[3] != "-")
            {
                enPassantSquare = Convert.ToInt32(data[3]);
            }

            Board board = new Board(pieces, turnWhite, castles, enPassantSquare);
            return board;
        }
    }
}
