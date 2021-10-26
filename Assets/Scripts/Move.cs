using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    public struct Move
    {
        public int Start;
        public int Target;
        public Piece Piece;
        public int Flag;

        public struct MFlag
        {
            public const int None = 0;
            public const int PawnPush = 1;
            public const int Castling = 2;
            public const int PromotionToRook = 3;
            public const int PromotionToKnight = 4;
            public const int PromotionToBishop = 5;
            public const int PromotionToQueen = 6;
            public const int EnPassant = 7;
            public const int Capture = 8;
        }

        public Move(int start, int target, Piece piece, int flag = MFlag.None)
        {
            Start = start;
            Target = target;
            Piece = piece;
            Flag = flag;
        }
    }
}
