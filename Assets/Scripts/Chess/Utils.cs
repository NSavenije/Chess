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

        public static Piece.PColor GetPieceColorFromChar(char c)
        {
            return char.IsUpper(c) ? Piece.PColor.White : Piece.PColor.Black;
        }

        public static Piece.PType GetPieceTypeFromChar(char c)
        {
            Piece.PType piece;
            char pieceCode = char.ToLower(c);

            switch (pieceCode)
            {
                case 'p':
                    piece = Piece.PType.Pawn;
                    break;
                case 'r':
                    piece = Piece.PType.Rook;
                    break;
                case 'n':
                    piece = Piece.PType.Knight;
                    break;
                case 'b':
                    piece = Piece.PType.Bishop;
                    break;
                case 'q':
                    piece = Piece.PType.Queen;
                    break;
                case 'k':
                    piece = Piece.PType.King;
                    break;
                default:
                    piece = Piece.PType.None;
                    break;
            }
            return piece;
        }

        public static (int, int) AddDir((int,int) sq1, (int,int) sq2)
        {
            return (sq1.Item1 + sq2.Item1, sq1.Item2 + sq2.Item2);
        }

        public static (int,int) GetSquareIdFromCoordinate(Vector3 position)
        {
            int file = (int)(Math.Floor(position.x) + 4);
            int rank = (int)(Math.Floor(position.y) + 4);
            if (rank >= 0 && rank < 8 && file >= 0 && file < 8)
                return (file,rank);
            else
                return (-1,-1);
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

        public static (int,int) GetSquareIdFromSquareName(string name)
        {
            char[] fr = name.ToCharArray();
            //char 97 = a, char 104 = h
            int file = Convert.ToInt32(fr[0]) - 97;
            int rank = fr[1] - 1;
            return(file,rank);
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

        // Determine Direction from sq1 to sq2. Only for attacking pieces.
        public static (int, int) DetermineAttackDirection((int, int) sq1, (int, int) sq2)
        {
            (int, int) dir;
            int f1 = sq1.Item1;
            int r1 = sq1.Item2;
            int f2 = sq2.Item1;
            int r2 = sq2.Item2;
            
            int fd = f2 - f1;
            int rd = r2 - r1;

            // Rook or Queen
            if (rd == 0)
                return (fd / Math.Abs(fd), 0);

            // Rook or Queen
            if (fd == 0)
                return (0, rd / Math.Abs(rd));

            // Knight
            if ((Math.Abs(fd) == 1 && Math.Abs(rd) == 2) || (Math.Abs(fd) == 2 && Math.Abs(rd) == 1))
                return (fd, rd);

            // Bishop or Queen
            return (fd / Math.Abs(fd), rd / Math.Abs(rd));
        }

        public static double DetermineAttackAngle((int,int) origin, (int,int) target)
        {
            return Math.Atan2(target.Item2 - origin.Item2, target.Item1 - origin.Item1);
        }

        private bool SquareExists((int f ,int r) square)
        {
            return (square.f >= 0 &&
                    square.r >= 0 &&
                    square.f <= 7 &&
                    square.r <= 7);
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
            if (f2 - f1 == r2 - r1 && f2 - f1 > 0)
            {
                dir = 9;
                return true;
            }
            // TOP LEFT
            else if (f1 - f2 == r2 - r1 && f1 - f2 > 0) 
            {
                dir = 7;
                return true;
            }
            // BOTTOM RIGHT
            else if (f2 - f1 == r1 - r2 && f2 - f1 > 0)
            {
                dir = -7;
                return true;
            }
            // BOTTOM LEFT
            else if (f1 - f2 == r1 - r2 && f1 - f2 > 0)
            {
                dir = -9;
                return true;
            }
            else Debug.LogError("Impossible location");
            return false;
        }
        
        public static bool SameFileRank(int s1, int s2, out int dir)
        {
            var (f1, r1) = SquareToFileRank(s1);
            var (f2, r2) = SquareToFileRank(s2);
            dir = 0;

            // Same Rank?
            if (r1 == r2){
                //Is s2 to the right of s1? Else left.
                dir = f1 < f2 ? 1 : -1;
                return true;
            }
            // Same file?
            else if (f1 == f2){
                //Is s2 above s1? Else below
                dir = r1 < r2 ? 8 : -8;
                return true;
            }

            return false;
        }
    }
}
