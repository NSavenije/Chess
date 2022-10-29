using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class Piece
    {
        public string Name {
            get { return GetPieceName(this); } 
        }
        public PType Type;
        public PColor Color;
        public (int,int) Square;
        public int PMoved = 0;
        public bool LongRange = false;
        public List<(int,int)> Moveset;

        public Piece(PType type, PColor color, (int,int) sq, int moved)
        {
            Type = type;
            Color = color;
            Square = sq;
            PMoved = moved;
            LongRange = IsLongRange(type);
            Moveset = GetMoveset(type);
        }

        public enum PType{
            None = 0,
            Pawn = 1,
            Rook = 2,
            Knight = 3,
            Bishop = 4,
            Queen = 5,
            King = 6
        }

        public enum PColor
        {
            None = 0,
            White = 8,
            Black = 16
        }

        private static string GetPieceName(Piece p)
        {
            string name = "";
            switch (p.Type)
            {
                case PType.Rook:
                    name += "R";
                    break;
                case PType.Knight:
                    name += "N";
                    break;
                case PType.Bishop:
                    name += "B";
                    break;
                case PType.Queen:
                    name += "Q";
                    break;
                case PType.King:
                    name += "K";
                    break;
                default:
                    break;
                
            }
            var fr = Utils.SquareToFileRank(p.Square);
            name += Utils.GetSquareNameFromCoordinate(fr.Item1, fr.Item2);
            return name;
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

        public static bool IsLongRange(PType type)
        {
            if (type == PType.Rook || type == PType.Bishop || type == PType.Queen)
                return true;
            return false;
        }

        public static List<int> GetMoveset(PType type)
        {
            List<int> movesets = new List<int>();
            switch (type)
            {
                case PType.Pawn:
                    movesets.AddRange(PawnMoves);
                    movesets.AddRange(PawnCaptures);
                    break;
                case PType.Rook:
                    movesets.AddRange(slides);
                    break;
                case PType.Knight:
                    movesets.AddRange(knightJumps);
                    break;
                case PType.Bishop:
                    movesets.AddRange(diagonals);
                    break;
                case PType.Queen:
                    movesets.AddRange(diagonals);
                    movesets.AddRange(slides);
                    break;
                case PType.King:
                    movesets.AddRange(diagonals);
                    movesets.AddRange(slides);
                    break;
                default:
                    break;
            }
            return movesets;
        }

        public static string ToString(Piece p)
        {
            return $"{GetPieceName(p)}: Type: {p.Type.ToString()}, Color: {p.Color.ToString()}, #moves: {p.PMoved}";
        }

        public static List<(int,int)> PawnMoves = new List<(int,int)> { (0,1), (0,2) };
        public static List<(int,int)> PawnCaptures = new List<(int,int)> { (-1,1), (1,1) };
        public static List<(int,int)> slides = new List<(int,int)> { (0,-1), (-1,0), (1,0), (0,1) };
        public static List<(int,int)> diagonals = new List<(int,int)> { (-1,-1), (1,-1), (-1,1), (1,1) };
        public static List<(int,int)> knightJumps = new List<(int,int)> { (-2,-1), (-2,1), (-1,-2), (-1,2), (1,-2), (1,2), (2,-1), (2,1) };
    }
}