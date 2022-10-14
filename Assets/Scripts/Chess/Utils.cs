using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    static class Utils
    {
        public static int FileRankToSquare(int file, int rank)
        {
            return rank * 8 + file;
        }

        public static int GetPieceFromChar(char c)
        {
            int piece = 0;
            char pieceCode = c;
            if (char.IsUpper(c))
            {
                piece = Piece.White;
                pieceCode = char.ToLower(c);
            }
            else
            {
                piece = Piece.Black;
            }

            switch (pieceCode)
            {
                case 'p':
                    piece |= Piece.Pawn;
                    break;
                case 'r':
                    piece |= Piece.Rook;
                    break;
                case 'n':
                    piece |= Piece.Knight;
                    break;
                case 'b':
                    piece |= Piece.Bishop;
                    break;
                case 'q':
                    piece |= Piece.Queen;
                    break;
                case 'k':
                    piece |= Piece.King;
                    break;
                default:
                    piece = Piece.None;
                    break;
            }
            return piece;
        }

        public static int GetSquareFromCoordinate(Vector3 position)
        {
            int file = (int)(Math.Floor(position.x) + 4);
            int rank = (int)(Math.Floor(position.y) + 4);
            int square = Utils.FileRankToSquare(file, rank);
            if (rank >= 0 && rank < 8 && file >= 0 && file < 8)
                return square;
            else
                return -1;
        }

        public static (int,int) GetFileRankDirFromSquareDir(int dir)
        {
            switch (dir) {
                case -17:
                    return (-1, -2);
                case -15:
                    return ( 1, -2);
                case -10:
                    return (-2, -1);
                case -9:
                    return (-1, -1);
                case -8:
                    return ( 0, -1);
                case -7:
                    return ( 1, -1);
                case -6:
                    return ( 2, -1);
                case -1:
                    return (-1,  0);
                case 1:
                    return ( 1,  0);
                case 6:
                    return (-2,  1);
                case 7:
                    return (-1,  1);
                case 8:
                    return ( 0,  1);
                case 9:
                    return ( 1,  1);
                case 10:
                    return ( 2,  1);
                case 15:
                    return (-1,  2);
                case 17:
                    return ( 1,  2);
            }
            return (int.MinValue, int.MinValue);
        }

        public static bool SameColor(bool turnWhite, bool selectedPieceWhite)
        {
            if (turnWhite && selectedPieceWhite)
                return true;
            if (!turnWhite && !selectedPieceWhite)
                return true;
            return false;
        }

        public static bool SameColor(Piece p1, Piece p2)
        {
            return p1.Color == p2.Color;
        }

        public static string GetSquareNameFromCoordinate(int file, int rank)
        {
            char fileString = char.ToLower((char)(file + 65));
            string squareName = "";
            squareName += fileString;
            squareName += (rank + 1);
            return squareName;
        }

        public static string GetSquareNameFromCoordinate(int squarenr)
        {
            (int, int) fr = SquareToFileRank(squarenr);
            return GetSquareNameFromCoordinate(fr.Item1, fr.Item2);
        }

        public static int FileRankToSquare(Vector2 pos)
        {
            return ((int)pos.y * 8 + (int)pos.x);
        }

        public static (int, int) SquareToFileRank(int square)
        {
            int file = square % 8;
            int rank = square / 8;
            return (file, rank);
        }

        public static bool SameDiagonal(int s1, int s2, out int dir)
        {
            var (f1, r1) = SquareToFileRank(s1);
            var (f2, r2) = SquareToFileRank(s2);
            dir = 0;

            // Same diagonal?
            if (Math.Abs(f1 - f2) != Math.Abs(r1 - r2))
                return false;

            /* Is s2 to the TR of s1?
            0,3 | 1,3 | 2,3 | 3,3
            0,2 | 1,2 | 2,2 | 3,2
            0,1 | 1,1 | 2,1 | 3,1
            0,0 | 1,0 | 2,0 | 3,0
            */
            if (f1 -)
            
        }
    }
}
