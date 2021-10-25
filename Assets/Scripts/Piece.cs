using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public static class Piece
    {
        public const int None =   0;
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

        public enum PieceType{
            None,
            Pawn,
            Rook,
            Knight,
            Bishop,
            Queen,
            King
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

        public static List<List<int>> GetMovesets(PieceType type)
        {
            List<List<int>> movesets = new List<List<int>>();
            switch (type)
            {
                case PieceType.Pawn:
                    movesets.Add(PawnMoves);
                    movesets.Add(PawnCaptures);
                    break;
                case PieceType.Rook:
                    movesets.Add(slides);
                    break;
                case PieceType.Knight:
                    movesets.Add(knightJumps);
                    break;
                case PieceType.Bishop:
                    movesets.Add(diagonals);
                    break;
                case PieceType.Queen:
                    movesets.Add(diagonals);
                    movesets.Add(slides);
                    break;
                case PieceType.King:
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

        public static PieceType GetPieceType(int piece)
        {
            int type = piece % 8;
            PieceType res;
            switch (type) {
                case 1:
                    res = PieceType.Pawn;
                    break;
                case 2:
                    res = PieceType.Rook;
                    break;
                case 3:
                    res = PieceType.Knight;
                    break;
                case 4:
                    res = PieceType.Bishop;
                    break;
                case 5:
                    res = PieceType.Queen;
                    break;
                case 6:
                    res = PieceType.King;
                    break;
                default:
                    res = PieceType.None;
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