using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class Piece
    {
        public int Code;
        public PType Type;
        public PColor Color;
        public int Square;
        public bool PMoved = false;
        public bool LongRange = false;

        public Piece(int code, int square, bool moved)
        {
            Code = code;
            Type = GetPieceType(code);
            Color = GetColor(code);
            Square = square;
            PMoved = moved;
            LongRange = IsLongRange(code);
        }

        public const int None =   0; // 0-2nd
        public const int Pawn =   1;
        public const int Rook =   2;
        public const int Knight = 3;
        public const int Bishop = 4;
        public const int Queen =  5;
        public const int King =   6;

        public const int White =  8;   //3rd
        public const int Black =  16;  //4th

        public const int Moved =  32;  //5th

        public static List<int> PawnMoves = new List<int> { 8, 16 };
        public static List<int> PawnCaptures = new List<int> { 7, 9 };
        public static List<int> slides = new List<int> { -8, -1, 1, 8 };
        public static List<int> diagonals = new List<int> { -9, -7, 7, 9 };
        public static List<int> knightJumps = new List<int> { -17, -15, -10, -6, 6, 10, 15, 17 };

        public enum PType{
            None,
            Pawn,
            Rook,
            Knight,
            Bishop,
            Queen,
            King
        }

        public enum PColor
        {
            None,
            White,
            Black
        }

        public static PColor GetColor(Piece p)
        {
            return GetColor(p.Code);
        }

        public static PColor GetOtherColor(Piece p)
        {
            return GetOtherColor(p.Color);
        }

        public static PColor GetOtherColor(PColor p)
        {
            return p == PColor.White ? PColor.Black : PColor.White;
        }

        public static List<Piece> GetPieces(PType type, PColor color, List<Piece> pieces)
        {
            return pieces.FindAll(delegate (Piece p) { return p.Type == type && p.Color == color; });
        }

        public static List<Piece> GetPieces(PColor color, List<Piece> pieces)
        {
            return pieces.FindAll(delegate (Piece p) { return p.Color == color; });
        }

        public static PColor GetColor(int pieceCode)
        {
            return IsWhite(pieceCode) ? PColor.White : PColor.Black;
        }

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

        public static bool IsLongRange(int piece)
        {
            int type = piece % 8;
            if (type == 2 || type == 4 || type == 5)
                return true;
            return false;
        }

        public static List<List<int>> GetMovesets(PType type)
        {
            List<List<int>> movesets = new List<List<int>>();
            switch (type)
            {
                case PType.Pawn:
                    movesets.Add(PawnMoves);
                    movesets.Add(PawnCaptures);
                    break;
                case PType.Rook:
                    movesets.Add(slides);
                    break;
                case PType.Knight:
                    movesets.Add(knightJumps);
                    break;
                case PType.Bishop:
                    movesets.Add(diagonals);
                    break;
                case PType.Queen:
                    movesets.Add(diagonals);
                    movesets.Add(slides);
                    break;
                case PType.King:
                    movesets.Add(diagonals);
                    movesets.Add(slides);
                    break;
                default:
                    break;
            }
            return movesets;
        }

        public static List<List<int>> GetMovesets(int piece)
        {
            return GetMovesets(GetPieceType(piece));
        }

        public static PType GetPieceType(int piece)
        {
            int type = piece % 8;
            PType res;
            switch (type) {
                case 1:
                    res = PType.Pawn;
                    break;
                case 2:
                    res = PType.Rook;
                    break;
                case 3:
                    res = PType.Knight;
                    break;
                case 4:
                    res = PType.Bishop;
                    break;
                case 5:
                    res = PType.Queen;
                    break;
                case 6:
                    res = PType.King;
                    break;
                default:
                    res = PType.None;
                    break;
            }
            //Debug.Log("type " + type + " res " + res);
            return res;
        }

        public static bool HasMoved(int piece)
        {
            if (IsBitSet(piece, 5))
                return true;
            return false;
        }
    }
}