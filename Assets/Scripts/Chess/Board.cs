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
        public List<Move> legalMoves;
        public int ActiveSquare;
        public bool turnWhite;
        public Stack<Move> previousMoves;
        private Move previousMove;
        private int enPassantSquare;
        public Dictionary<string, bool> castlingRights;
        private List<int>[] attackMask;
        private bool[] doubleCheck = { false, false };
        public int countlegalmovescheck = 0;

        public Board(List<Piece> pieces, bool turn, Dictionary<string, bool> castles, int enPassant)
        {
            // Instantiate objects
            legalMoves = new List<Move>();
            previousMoves = new Stack<Move>();
            previousMove = new Move(-1, -1, null, 0);
            //attackMask = new List<int>[2];

            // Fill board
            Pieces = pieces;
            Squares = PlacePiecesOnBoard(pieces);
            castlingRights = castles;
            //attackMask[0] = CreateInitialAttackMask(Piece.PColor.White);
            //attackMask[1] = CreateInitialAttackMask(Piece.PColor.Black);

            // Set flags
            ActiveSquare = -1;
            turnWhite = turn;
            enPassantSquare = enPassant;
        }

        private Piece[] PlacePiecesOnBoard(List<Piece> pieces)
        {
            Piece[] squares = new Piece[64];
            foreach(Piece p in pieces)
            {
                squares[p.Square] = p;
            }
            return squares;
        }

        private List<int> CreateInitialAttackMask(Piece.PColor col)
        {
            List<int> mask = new List<int>();
            List<Move> ms = GetAllLegalMoves();
            foreach(Move move in ms)
            {
                mask.Add(move.Target);
            }
            return mask;
        }

        // Generate all moves
        public List<Move> GetAllLegalMoves(bool pseudoLegal = true)
        {
            List<Move> moves = new List<Move>();
            Piece.PColor c = turnWhite ? Piece.PColor.White : Piece.PColor.Black;
            foreach (Piece p in Piece.GetPieces(c, Pieces))
            {
                moves.AddRange(GetLegalMoves(p));
            }
            return moves;
        }

        // Generate moves
        public List<Move> GetLegalMoves(Piece piece, bool FirstPassCheckForChecks = true, int dir = int.MaxValue)
        {
            countlegalmovescheck++;
            List<Move> moves = new List<Move>();
            int square = piece.Square;
            if (FirstPassCheckForChecks)
                moves = GetCastlingMoves(piece, previousMove);
            if (piece.Type == Piece.PType.Pawn)
                moves.AddRange(FindPawnMoves(piece, previousMove, FirstPassCheckForChecks));
            // check all moves or only the one point at the king?
            List<int> dirs = dir == int.MaxValue ? Piece.GetMovesets(piece.Type) : new List<int>(){dir};
            if (dir == 0)
                return moves;
            for (int i = 0; i < dirs.Count; i++)
            {
                int target = square + dirs[i];
                if (piece.LongRange)
                {
                    int currentSquare = square;
                    while (SquareExists(currentSquare, dirs[i]) && !Blocked(piece, target))
                    {
                        TryGetPieceFromSquare(target, out Piece enemyPiece);
                        moves.Add(new Move(square, target, piece, Move.MFlag.None, enemyPiece));
                        if (TryGetPieceFromSquare(target, out Piece _))
                            break;
                        currentSquare = target;
                        target += dirs[i];
                    }
                }
                else if (piece.Type != Piece.PType.Pawn)
                {
                    if (SquareExists(square, dirs[i]) && !Blocked(piece, target))
                    {
                        TryGetPieceFromSquare(target, out Piece enemyPiece);
                        moves.Add(new Move(square, target, piece, Move.MFlag.None, enemyPiece));
                    }
                }
            }
            if (FirstPassCheckForChecks)
            {
                foreach (Move move in moves.ToList())
                {
                    // If the opponent can capture my king after I do this move, (I was in check or movoed a piece that was pinned), dont.
                    if (WouldResultInKingCapture(square, move))
                        moves.Remove(move);
                }
            }
            return moves;
        }

        public void DoMove(Move move)
        {
            // House Keeping.
            turnWhite = !turnWhite;

            // Remove Captured Piece.
            if (TryGetPieceFromSquare(move.Target, out Piece targetPiece))
                Pieces.Remove(targetPiece);

            if (move.Flag == Move.MFlag.EnPassant)
            {
                Move pushMove = previousMove;
                Pieces.Remove(pushMove.Piece);
                Squares[pushMove.Target] = null;
            }

            if (move.Flag == Move.MFlag.PromotionToRook)
            {
                move.Piece.Type = Piece.PType.Rook;
                move.Piece.Code |= 1;
                move.Piece.LongRange = true;
            }

            if (move.Flag == Move.MFlag.PromotionToKnight)
            {
                move.Piece.Type = Piece.PType.Knight;
                move.Piece.Code |= 2;
                move.Piece.LongRange = false;
            }

            if (move.Flag == Move.MFlag.PromotionToBishop)
            {
                move.Piece.Type = Piece.PType.Rook;
                move.Piece.Code |= 3;
                move.Piece.LongRange = true;
            }

            if (move.Flag == Move.MFlag.PromotionToQueen)
            {
                move.Piece.Type = Piece.PType.Queen;
                move.Piece.Code |= 4;
                move.Piece.LongRange = true;
            }

            if (move.Flag == Move.MFlag.Castling)
            {
                List<Piece> rooks = Piece.GetPieces(Piece.PType.Rook, move.Piece.Color, Pieces);
                bool kingSide = move.Start < move.Target;
                Piece rook = kingSide ? rooks.Find(r => r.Square > move.Piece.Square) : rooks.Find(r => r.Square < move.Piece.Square);
                int target = kingSide ? move.Target - 1 : move.Target + 1;

                // Update the Board
                Squares[rook.Square] = null;
                Squares[target] = rook;

                // Move and Update the Rook;
                rook.Code |= Piece.Moved;
                rook.PMoved++;
                rook.Square = target;
            }

            // Move and Update the Piece.
            move.Piece.Code |= Piece.Moved;
            move.Piece.PMoved++;
            move.Piece.Square = move.Target;

            // Update the Board.
            Squares[move.Start] = null;
            Squares[move.Target] = move.Piece;

            // Update the Stack.
            previousMoves.Push(move);
            previousMove = move;
        }

        public void UndoMove()
        {
            // House Keeping.
            if (previousMoves.Count == 0)
                return;
            turnWhite = !turnWhite;
            Move move = previousMoves.Pop();
            if (previousMoves.Count > 0)
                previousMove = previousMoves.Peek();

            if (move.Flag == Move.MFlag.PromotionToRook)
            {
                move.Piece.Type = Piece.PType.Pawn;
                int mask = 1 << 1;
                move.Piece.Code &= ~mask;
                move.Piece.Code |= 1;
                move.Piece.LongRange = false;
            }

            if (move.Flag == Move.MFlag.PromotionToKnight)
            {
                move.Piece.Type = Piece.PType.Pawn;
                int mask = 1 << 1;
                move.Piece.Code &= ~mask;
            }

            if (move.Flag == Move.MFlag.PromotionToBishop)
            {
                move.Piece.Type = Piece.PType.Pawn;
                int mask = 1 << 2;
                move.Piece.Code &= ~mask;
                move.Piece.Code |= 1;
                move.Piece.LongRange = false;
            }

            if (move.Flag == Move.MFlag.PromotionToQueen)
            {
                move.Piece.Type = Piece.PType.Pawn;
                int mask = 1 << 3;
                move.Piece.Code &= ~mask;
                move.Piece.LongRange = false;
            }

            // Uncastle by finding the closest rook and putting it back.
            if (move.Flag == Move.MFlag.Castling)
            {
                bool isWhite = move.Piece.Color == Piece.PColor.White;
                List<Piece> rooks = Piece.GetPieces(Piece.PType.Rook, move.Piece.Color, Pieces);
                bool kingSide = move.Start < move.Target;
                Piece rook = kingSide ? rooks.Find(r => r.Square == move.Piece.Square - 1) : rooks.Find(r => r.Square == move.Piece.Square + 1);
                int target = kingSide ? (isWhite ? 7 : 63) : (isWhite ? 0 : 56);

                // Update the Board
                Squares[rook.Square] = null;
                Squares[target] = rook;

                // Move and Update the Rook;
                int mask = 1 << 5;
                rook.Code &= ~mask;
                rook.PMoved = 0;
                rook.Square = target;
                move.Piece.Code &= ~mask;
            }

            if (move.Flag == Move.MFlag.PawnPush)
            {
                int mask = 1 << 5;
                move.Piece.Code &= ~mask;
            }

            // Move and Update the Piece.
            // move.Piece.Code |= Piece.Moved;
            move.Piece.PMoved--;
            move.Piece.Square = move.Start;

            // Update the Board.
            Squares[move.Start] = move.Piece;
            Squares[move.Target] = null;

            // Replace Captured Piece.
            if (move.CapturedPiece != null)
            {
                Piece p = move.CapturedPiece;
                Squares[p.Square] = p;
                Pieces.Add(p);
            }
        }

        private bool WouldResultInKingCapture(int square, Move move)
        {
            if (!TryGetPieceFromSquare(square, out Piece piece))
                return false;

            //// Kings cant capture kings.  Scared, discovered attacks? 
            //if (piece.Type == Piece.PType.King){
            //    return false;
            //}

            // Do the move that could result in check without checking.
            Piece pieceAtTarget = Squares[move.Target];
            piece.Square = move.Target;
            Squares[square] = null;
            Squares[move.Target] = piece;

            // Check if my King can now be captured by any of the opposing pieces.
            int kingSquare = Piece.GetPieces(Piece.PType.King, piece.Color, Pieces)[0].Square;
            // Remove captured piece from this list
            foreach (Piece p in Piece.GetPieces(Piece.GetOtherColor(piece), Pieces))
            {
                if (p.Square == move.Target)
                    continue;
                // Im gonna filter the movesets
                int dir = int.MaxValue;
                switch (p.Type)
                {
                    case Piece.PType.Bishop:
                        Utils.SameDiagonal(p.Square, kingSquare, out dir);
                        break;
                    case Piece.PType.Rook:
                        Utils.SameFileRank(p.Square, kingSquare, out dir);
                        break;
                    case Piece.PType.Queen:
                        if (Utils.SameFileRank(p.Square, kingSquare, out dir))
                            break;
                        Utils.SameDiagonal(p.Square, kingSquare, out dir);
                        break;
                    case Piece.PType.Pawn:
                        //If a pawn
                        break;
                }
                
                
                if (dir != 0 && GetLegalMoves(p, false, dir).Exists(x => x.Target == kingSquare))
                {
                    //Reset the board to the orignal state.
                    piece.Square = square;
                    Squares[square] = piece;
                    Squares[move.Target] = pieceAtTarget;
                    return true;
                }
            }
            piece.Square = square;
            Squares[square] = piece;
            Squares[move.Target] = pieceAtTarget;
            return false;
        }

        private bool SquareExists(int square, int direction, bool reverse = false)
        {
            if (square + direction < 0 || square + direction > 63)
                return false;
            (int, int) fr = Utils.SquareToFileRank(square);
            (int, int) dir = Utils.GetFileRankDirFromSquareDir(direction);
            if (reverse)
                dir = (dir.Item1 * -1, dir.Item1 * -1);
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

        private List<Move> GetCastlingMoves(Piece king, Move previousMove)
        {
            List<Move> moves = new List<Move>();
            if (king.Type != Piece.PType.King)
                return moves;

            bool Kk = king.Color == Piece.PColor.White ? castlingRights["K"] : castlingRights["k"];
            bool Qq = king.Color == Piece.PColor.White ? castlingRights["Q"] : castlingRights["q"];

            if (king.PMoved == 0 && (Kk || Qq))
            {
                var rooks = Piece.GetPieces(Piece.PType.Rook, king.Color, Pieces);
                foreach (var rook in rooks)
                {
                    // This rook and king havent moved, check if the squares between them are open
                    if (rook.PMoved == 0)
                    {
                        if (rook.Square > king.Square && Kk) // King Side
                        {
                            bool allEmpty = true;
                            for (int s = king.Square + 1; s < rook.Square; s++)
                                if (TryGetPieceFromSquare(s, out _))
                                {
                                    allEmpty = false;
                                    break;
                                }
                            if (allEmpty)
                            {
                                bool canCastle = true;
                                foreach (Piece p in Piece.GetPieces(Piece.GetOtherColor(king), Pieces))
                                    if (GetLegalMoves(p, false).Exists(x => x.Target == king.Square || x.Target == king.Square + 1 || x.Target == king.Square + 2))
                                    {
                                        canCastle = false;
                                        break;
                                    }
                                if (canCastle)
                                    moves.Add(new Move(king.Square, king.Square + 2, king, Move.MFlag.Castling));
                            }
                        }
                        else if (Qq) // Queen side
                        {
                            bool allEmpty = true;
                            for (int s = king.Square - 1; s > rook.Square; s--)
                                if (TryGetPieceFromSquare(s, out _))
                                    allEmpty = false;
                            if (allEmpty)
                            {
                                bool canCastle = true;
                                foreach (Piece p in Piece.GetPieces(Piece.GetOtherColor(king), Pieces))
                                    if (GetLegalMoves(p, false).Exists(x => x.Target == king.Square || x.Target == king.Square - 1 || x.Target == king.Square - 2))
                                    {
                                        canCastle = false;
                                        break;
                                    }
                                if (canCastle)
                                    moves.Add(new Move(king.Square, king.Square - 2, king, Move.MFlag.Castling));
                            }
                        }
                    }
                }
            }
            return moves;
        }

        /// <summary>
        /// Generates moves for promotions pushes captures for pawns.
        /// </summary>
        /// <param name="piece">Any piece</param>
        /// <param name="previousMove">Previous move</param>
        /// <returns></returns>
        private List<Move> FindPawnMoves(Piece piece, Move previousMove, bool firstPass)
        {
            List<Move> moves = new List<Move>();
            if (piece.Type != Piece.PType.Pawn)
                return moves;

            int perspective = piece.Color == Piece.PColor.White ? 1 : -1;
            
            // Moving a pawn cant capture a king.
            if (firstPass) {
                for (int i = 0; i < Piece.PawnMoves.Count; i++)
                {
                    // If we have moved, we can no longer push the pawn.
                    if (piece.PMoved > 0 && i == 1) break;
                    int flag = 0;
                    int destination = piece.Square + (perspective * Piece.PawnMoves[i]);
                    if (destination < 8 || destination > 54)
                        flag = Move.MFlag.Promoting;
                    if (!TryGetPieceFromSquare(destination, out Piece _))
                    {
                        if (flag == Move.MFlag.Promoting)
                        {
                            moves.Add(new Move(piece.Square, destination, piece, Move.MFlag.PromotionToKnight));
                            moves.Add(new Move(piece.Square, destination, piece, Move.MFlag.PromotionToBishop));
                            moves.Add(new Move(piece.Square, destination, piece, Move.MFlag.PromotionToRook));
                            moves.Add(new Move(piece.Square, destination, piece, Move.MFlag.PromotionToQueen));
                        }
                        moves.Add(new Move(piece.Square, destination, piece, i));
                    }
                    else break; //If it is blocked, there is no use in checking if the pawn can move up further.
                } 
            }

            for (int i = 0; i < Piece.PawnCaptures.Count; i++)
            {
                int destination; int flag = Move.MFlag.None;
                if (piece.Color == Piece.PColor.White && SquareExists(piece.Square, Piece.PawnCaptures[i], false))
                    destination = piece.Square + Piece.PawnCaptures[i];
                else if ((piece.Color == Piece.PColor.Black && SquareExists(piece.Square, Piece.PawnCaptures[i], true)))
                    destination = piece.Square - Piece.PawnCaptures[i];
                else
                    continue;
                Piece enemyPiece;
                // Check if I can capture a king, then return. 
                if (!firstPass && TryGetPieceFromSquare(destination, out enemyPiece)) {
                    if (enemyPiece.Type == Piece.PType.King) {
                        moves.Add(new Move(piece.Square, destination, piece, flag, enemyPiece));
                        return moves;
                    }
                }
                if (destination < 8 || destination > 54)
                    flag = Move.MFlag.Promoting;
                if (TryGetPieceFromSquare(destination, out enemyPiece))
                {
                    if (!Utils.SameColor(piece, enemyPiece))
                    {
                        if (flag == Move.MFlag.Promoting)
                        {
                            moves.Add(new Move(piece.Square, destination, piece, Move.MFlag.PromotionToKnight, enemyPiece));
                            moves.Add(new Move(piece.Square, destination, piece, Move.MFlag.PromotionToBishop, enemyPiece));
                            moves.Add(new Move(piece.Square, destination, piece, Move.MFlag.PromotionToRook, enemyPiece));
                            moves.Add(new Move(piece.Square, destination, piece, Move.MFlag.PromotionToQueen, enemyPiece));
                        }
                        else
                            moves.Add(new Move(piece.Square, destination, piece, flag, enemyPiece));
                    }
                }
            }

            // Check for en passant
            bool sameRank = Utils.SquareToFileRank(piece.Square).Item2 == Utils.SquareToFileRank(previousMove.Target).Item2;
            if (firstPass && previousMove.Flag == Move.MFlag.PawnPush && sameRank) 
            {
                if (perspective == 1)
                {
                    if (previousMove.Target == piece.Square - 1)
                        moves.Add(new Move(piece.Square, piece.Square + 7, piece, Move.MFlag.EnPassant, previousMove.Piece));
                    else if (previousMove.Target == piece.Square + 1)
                        moves.Add(new Move(piece.Square, piece.Square + 9, piece, Move.MFlag.EnPassant, previousMove.Piece));
                }
                else
                {
                    if (previousMove.Target == piece.Square + 1)
                        moves.Add(new Move(piece.Square, piece.Square - 7, piece, Move.MFlag.EnPassant, previousMove.Piece));
                    else if (previousMove.Target == piece.Square - 1)
                        moves.Add(new Move(piece.Square, piece.Square - 9, piece, Move.MFlag.EnPassant, previousMove.Piece));
                }
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