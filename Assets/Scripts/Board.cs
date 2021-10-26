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
        Piece[] _square = new Piece[64];
        public Piece[] Squares
        {
            get { return _square; }
            set { _square = value; }
        }
        public List<Piece> Pieces;

        public List<int> legalMoves;

        public int ActiveSquare;
        public Board()
        {
            ActiveSquare = -1;
            legalMoves = new List<int>();
            Pieces = new List<Piece>();
        }

        public void SetLegalMoves(Piece piece)
        {
            legalMoves.Clear();
            int square = piece.Square;
            if (piece.Type == Piece.PType.Pawn)
            {
                legalMoves = SetLegalPawnMoves(square, piece);       
            }
            else
            {
                var movesets = Piece.GetMovesets(piece.Type);
                foreach (var set in movesets)
                {
                    for (int i = 0; i < set.Count; i++)
                    {
                        int destination = square + set[i];
                        if (piece.LongRange)
                        {
                            int currentSquare = square;
                            bool foundOtherPiece = false;
                            while (SquareExists(currentSquare, set[i]) && !Blocked(piece, destination, foundOtherPiece))
                            {
                                if (TryGetPieceFromSquare(destination, out Piece _))
                                    foundOtherPiece = true;
                                legalMoves.Add(destination);
                                currentSquare = destination;
                                destination += set[i];
                            }
                        }
                        else
                        {
                            if (SquareExists(destination) && !Blocked(piece, destination))
                                legalMoves.Add(destination);
                        }
                    }
                }
            }
            //foreach (int move in legalMoves.ToList())
            //{
            //    if (WouldResultInKingCapture(square, move))
            //        legalMoves.Remove(move);
            //}
        }

        private List<int> FindLegalMoves(Piece piece)
        {
            List<int> moves = new List<int>();
            int square = piece.Square;
            if (piece.Type == Piece.PType.Pawn)
            {
                moves = SetLegalPawnMoves(square, piece);
            }
            else
            {
                var movesets = Piece.GetMovesets(piece.Type);
                foreach (var set in movesets)
                {
                    for (int i = 0; i < set.Count; i++)
                    {
                        int destination = square + set[i];
                        if (piece.LongRange)
                        {
                            int currentSquare = square;
                            bool foundOtherPiece = false;
                            while (SquareExists(currentSquare, set[i]) && !Blocked(piece, destination, foundOtherPiece))
                            {
                                if (TryGetPieceFromSquare(destination, out Piece _))
                                    foundOtherPiece = true;
                                moves.Add(destination);
                                currentSquare = destination;
                                destination += set[i];
                            }
                        }
                        else
                        {
                            if (SquareExists(destination) && !Blocked(piece, destination))
                                moves.Add(destination);
                        }
                    }
                }
            }
            return moves;
        }

        private bool WouldResultInKingCapture(int square, int move)
        {
            Piece[] newBoard = Squares;
            TryGetPieceFromSquare(square, out Piece piece);
            Piece oldDestPiece = newBoard[move];
            newBoard[move] = piece;
            piece.Square = move;
            newBoard[square] = null;
            int kingSquare = Piece.GetPieces(Piece.PType.King, Piece.GetOtherColor(piece), Pieces)[0].Square;
            foreach(Piece p in Piece.GetPieces(Piece.GetColor(piece), Pieces))
            {
                if (FindLegalMoves(p).Contains(kingSquare))
                {
                    piece.Square = square;
                    Squares[move] = oldDestPiece;
                    return true;
                }
            }
            piece.Square = square;
            Squares[move] = oldDestPiece;
            return false;
        }

        private bool SquareExists(int square, int direction)
        {
            if (square + direction < 0 || square + direction > 63)
                return false;
            (int, int) fr = Utils.SquareToFileRank(square);
            (int, int) dir = Utils.GetFileRankDirFromSquareDir(direction);
            if (fr.Item1 + dir.Item1 < 0 || fr.Item1 + dir.Item1 > 7)
                return false;
            if (fr.Item2 + dir.Item2 < 0 || fr.Item2 + dir.Item2 > 7)
                return false;
            return true;
        }

        private bool SquareExists(int destination)
        {
            return destination >= 0 && destination < 64;
        }

        private bool Blocked(Piece piece, int destination, bool foundOtherPiece = false)
        {
            if (TryGetPieceFromSquare(destination, out Piece target))
                if (foundOtherPiece || Utils.SameColor(piece, target))
                    return true;
            return false;
        }

        private List<int> SetLegalPawnMoves(int square, Piece piece)
        {
            List<int> moves = new List<int>();
            for (int i = 0; i < Piece.PawnMoves.Count; i++)
            {
                if (piece.PMoved && i == 1) break;
                int destination;
                if (piece.Color == Piece.PColor.White)
                    destination = square + Piece.PawnMoves[i];
                else
                    destination = square - Piece.PawnMoves[i];
                if (!TryGetPieceFromSquare(destination, out Piece _))
                    moves.Add(destination);
                else break;
            }
            for (int i = 0; i < Piece.PawnCaptures.Count; i++)
            {
                int destination;
                if (piece.Color == Piece.PColor.White)
                    destination = square + Piece.PawnCaptures[i];
                else
                    destination = square - Piece.PawnCaptures[i];
                if (TryGetPieceFromSquare(destination, out Piece enemyPiece))
                    if (!Utils.SameColor(piece, enemyPiece))
                        moves.Add(destination);
            }
            return moves;
        }

        public bool TryGetPieceFromSquare(int square, out Piece piece)
        {
            if (_square[square] != null)
            {
                piece = _square[square];
                return true;
            }
            piece = null;
            return false;
        }
    }
}