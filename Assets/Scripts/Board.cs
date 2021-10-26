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
        // Generate moves
        public List<int> FindLegalMoves(Piece piece, bool checkForChecks = true)
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
                            if (SquareExists(square, set[i]) && !Blocked(piece, destination))
                                moves.Add(destination);
                        }
                    }
                }
            }
            if (checkForChecks)
            {
                foreach (int move in moves.ToList())
                {
                    // If the opponent can capture my king after I do this move, (I was in check or a piece was pinned), dont.
                    if (WouldResultInKingCapture(square, move))
                        moves.Remove(move);
                }
            }
            return moves;
        }

        private bool WouldResultInKingCapture(int square, int move)
        {
            if (!TryGetPieceFromSquare(square, out Piece piece))
                return false;

            // Do the move that could result in check without checking.
            Piece pieceAtTarget = Squares[move];
            piece.Square = move;
            Squares[square] = null;
            Squares[move] = piece;            
            
            // Check if my King can now be captured by any of the opposing pieces.
            int kingSquare = Piece.GetPieces(Piece.PType.King, piece.Color, Pieces)[0].Square;
            // Remove captured piece from this list
            foreach (Piece p in Piece.GetPieces(Piece.GetOtherColor(piece), Pieces))
            {
                if (p.Square == move)
                    continue;
                if (FindLegalMoves(p, false).Contains(kingSquare))
                {
                    //Reset the board to the orignal state.
                    piece.Square = square;
                    Squares[square] = piece;
                    Squares[move] = pieceAtTarget;
                    //Pieces.Add(pieceAtTarget);
                    return true;
                }
            }
            piece.Square = square;
            Squares[square] = piece;
            Squares[move] = pieceAtTarget;
            //Pieces.Add(pieceAtTarget);
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