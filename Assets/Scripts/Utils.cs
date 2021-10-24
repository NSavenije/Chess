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

        public static string GetSquareNameFromCoordinate(int file, int rank)
        {
            char fileString = (char)(file + 65);
            string squareName = "";
            squareName += fileString;
            squareName += (rank + 1);
            return squareName;
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
    }
}
