using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    static class Perft
    {
        public static int DoPerft(Board b, int depth)
        {
            int nodes = 0;
            if (depth == 0)
                return nodes;
            List<Move> ms = b.FindAllLegalMoves();
            if (depth == 1)
                nodes += ms.Count;
            foreach(Move m in ms)
            {
                b.DoMove(m);
                nodes += DoPerft(b, depth - 1);
                b.UndoMove();
            }
            return nodes;
        }

        public static List<(string, int)> Divide(Board b, int perftDepth, int moveNameDepth = 1)
        {
            List<(string, int)> res = new List<(string, int)>();
            List<Move> ms = b.FindAllLegalMoves();
            foreach(Move m in ms)
            {
                b.DoMove(m);
                int n = DoPerft(b, perftDepth - 1);
                res.Add((m.Name, n));
                b.UndoMove();
            }
            return res;
        }

        /*
          MOVE move_list[256];
          int n_moves, i;
          u64 nodes = 0;

          if (depth == 0) 
            return 1ULL;

          n_moves = GenerateLegalMoves(move_list);
          for (i = 0; i < n_moves; i++) {
            MakeMove(move_list[i]);
            nodes += Perft(depth - 1);
            UndoMove(move_list[i]);
          }
          return nodes;*/
    }

    class Tree
    {
        public List<Tree> moves;
        public Board board;
        
        public Tree(List<Tree> ms, Board b)
        {
            board = b;
            moves = ms;
        }
    }
}
