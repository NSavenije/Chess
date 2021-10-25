using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public static class Piece
    {
        public const int None = 0;
        public const int Pawn = 1;
        public const int Rook = 2;
        public const int Knight = 3;
        public const int Bishop = 4;
        public const int Queen = 5;
        public const int King = 6;

        public const int White = 8;
        public const int Black = 16;

        public const int Moved = 32;

        public static List<int> PawnMoves = new List<int> { 8, 16 };
        public static List<int> PawnCaptures = new List<int> { 7, 9 };

        static bool IsBitSet(int b, int pos)
        {
            return (b & (1 << pos)) != 0;
        }

        public static bool IsWhite(int piece)
        {
            if (IsBitSet(piece, 3))
                return true;
            return false;
        }

        public static bool HasMoved(int piece)
        {
            if (IsBitSet(piece, 5))
                return true;
            return false;
        }
    }
}