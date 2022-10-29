using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    public struct Move
    {
        public string Name
        {
            get { return GetMoveName(this); }
        }
        public Square Start;
        public Square Target;
        public Piece Piece;
        public int Flag;
        public Piece CapturedPiece;

        public struct MFlag
        {
            public const int None = 0;
            public const int PawnPush = 1;
            public const int Castling = 2;
            public const int Promoting = 3;
            public const int PromotionToRook = 4;
            public const int PromotionToKnight = 5;
            public const int PromotionToBishop = 6;
            public const int PromotionToQueen = 7;
            public const int EnPassant = 8;
        }

        public Move(Square start, Square target, Piece piece, int flag = MFlag.None, Piece capturedPiece = null)
        {
            Start = start;
            Target = target;
            Piece = piece;
            Flag = flag;
            CapturedPiece = capturedPiece;
        }

        private string GetMoveName(Move move)
        {
            string pname = move.Piece.Name;
            string start = Utils.GetSquareNameFromCoordinate(move.Start);
            return start + "->" + pname;
        }
    }
}
