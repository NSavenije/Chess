using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Unity;

namespace Assets.Scripts
{
    public class Board
    {
        int[] _square = new int[64];
        public int[] Square
        {
            get { return _square; }
            set { _square = value; }
        }

        public List<int> legalMoves;

        public int ActiveSquare;
        public Board()
        {
            ActiveSquare = -1;
            legalMoves = new List<int>();
        }

        public void SetLegalMoves(int square)
        {
            legalMoves.Clear();
            TryGetPieceFromSquare(square, out int piece);
            
            for(int i = 0; i < Piece.PawnMoves.Count; i++)
            {
                if (Piece.HasMoved(piece) && i == 1) break;
                int destination;
                if (Piece.IsWhite(piece))
                    destination = square + Piece.PawnMoves[i];
                else
                    destination = square - Piece.PawnMoves[i];
                if (!TryGetPieceFromSquare(destination, out int _))
                    legalMoves.Add(destination);
                else break;
            }
            for (int i = 0; i < Piece.PawnCaptures.Count; i++)
            {
                int destination;
                if (Piece.IsWhite(piece))
                    destination = square + Piece.PawnCaptures[i];
                else
                    destination = square - Piece.PawnCaptures[i];
                if (TryGetPieceFromSquare(destination, out int enemyPiece))
                    if (!Utils.SameColor(Piece.IsWhite(piece), Piece.IsWhite(enemyPiece)))
                        legalMoves.Add(destination);
            }
        }

        public bool TryGetPieceFromSquare(int square, out int piece)
        {
            if (_square[square] > 0)
            {
                piece = _square[square];
                return true;
            }
            piece = -1;
            return false;
        }
    }
}