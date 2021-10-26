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
        public int[] Squares
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
            if (Piece.GetPieceType(piece) == Piece.PieceType.Pawn)
            {
                legalMoves = SetLegalPawnMoves(square, piece);       
            }
            else
            {
                var movesets = Piece.GetMovesets(piece);
                foreach (var set in movesets)
                {
                    for (int i = 0; i < set.Count; i++)
                    {
                        int destination = square + set[i];
                        if (Piece.IsLongRange(piece))
                        {
                            int currentSquare = square;
                            bool foundOtherPiece = false;
                            while (SquareExists(currentSquare, set[i]) && !Blocked(piece, destination, foundOtherPiece))
                            {
                                if (TryGetPieceFromSquare(destination, out int _))
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

        private List<int> FindLegalMoves(int square)
        {
            List<int> moves = new List<int>();
            TryGetPieceFromSquare(square, out int piece);
            if (Piece.GetPieceType(piece) == Piece.PieceType.Pawn)
            {
                moves = SetLegalPawnMoves(square, piece);
            }
            else
            {
                var movesets = Piece.GetMovesets(piece);
                foreach (var set in movesets)
                {
                    for (int i = 0; i < set.Count; i++)
                    {
                        int destination = square + set[i];
                        if (Piece.IsLongRange(piece))
                        {
                            int currentSquare = square;
                            bool foundOtherPiece = false;
                            while (SquareExists(currentSquare, set[i]) && !Blocked(piece, destination, foundOtherPiece))
                            {
                                if (TryGetPieceFromSquare(destination, out int _))
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
            int[] newBoard = Squares;
            TryGetPieceFromSquare(square, out int piece);
            newBoard[move] = piece;
            newBoard[square] = 0;
            int kingSquare = -1;
            List<int> pieces = new List<int>();
            bool whitePerspective = Piece.IsWhite(piece);
            for (int sq = 0; sq < 64; sq++)
            {
                if (TryGetPieceFromSquare(sq, out int pc))
                    if (Piece.GetPieceType(pc) == Piece.PieceType.King)
                        if (!Utils.SameColor(Piece.IsWhite(pc), whitePerspective))
                        {
                            kingSquare = sq;
                            break;
                        }
            }
            if (kingSquare == -1)
                Debug.LogError("Enemy king not found");
            for (int sq = 0; sq < 64; sq++)
            {
                if (TryGetPieceFromSquare(sq, out int pc))
                    if (Utils.SameColor(Piece.IsWhite(pc), whitePerspective))
                        if (FindLegalMoves(sq).Contains(kingSquare))
                            return true;
            }
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

        private bool Blocked(int piece, int destination, bool foundOtherPiece = false)
        {
            if (TryGetPieceFromSquare(destination, out int target))
                if (foundOtherPiece || Utils.SameColor(Piece.IsWhite(piece), Piece.IsWhite(target)))
                    return true;
            return false;
        }

        private List<int> SetLegalPawnMoves(int square, int piece)
        {
            List<int> moves = new List<int>();
            for (int i = 0; i < Piece.PawnMoves.Count; i++)
            {
                if (Piece.HasMoved(piece) && i == 1) break;
                int destination;
                if (Piece.IsWhite(piece))
                    destination = square + Piece.PawnMoves[i];
                else
                    destination = square - Piece.PawnMoves[i];
                if (!TryGetPieceFromSquare(destination, out int _))
                    moves.Add(destination);
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
                        moves.Add(destination);
            }
            return moves;
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