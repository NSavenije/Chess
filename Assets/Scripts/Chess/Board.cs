using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    public class Board
    {
        Square[,] _square = new Square[8,8];
        public Square[,] Squares
        {
            get { return _square; }
            set { _square = value; }
        }
        public List<Piece> Pieces;
        public List<Move> legalMoves;
        public Square ActiveSquare;
        public bool turnWhite;
        public Stack<Move> previousMoves;
        private Move previousMove;
        private Square enPassantTargetSquare;
        public Dictionary<string, bool> castlingRights;
        int halfMovesSinceLastCapture;
        int moveCount;
        private List<int>[] attackMask;
        private bool[] doubleCheck = { false, false };
        // private (List<Square>, List<Square>) potentialCheckSquares;
        public int countlegalmovescheck = 0;

        public Board(List<Piece> pieces, bool turn, Dictionary<string, bool> castles, int enPassant, int movesSinceCapture, int moveCounter)
        {
            // Instantiate objects
            legalMoves = new List<Move>();
            previousMoves = new Stack<Move>();
            previousMove = new Move(-1, -1, null, 0);
            //attackMask = new List<int>[2];

            // Fill board
            Pieces = pieces;
            Squares = InitializeSquares(pieces);
            // potentialCheckSquares = UpdatePotentialCheckSquares();
            castlingRights = castles;

            // Set flags
            ActiveSquare = null;
            turnWhite = turn;
            enPassantSquare = enPassant;
            halfMovesSinceLastCapture = movesSinceCapture;
            moveCount = moveCounter;

            // Build Initial attack mask for both colours
            CreateInitialAttackMask();
        }

        private Square[,] InitializeSquares(List<Piece> pieces)
        {
            Square[,] squares = new Square[8,8];
            for(int f = 0; f < 8; f++){
                for (int r = 0; r < 8; r++)
                {
                    square[f,r] = new Square(f,r);
                }
            }
            foreach(Piece p in pieces)
            {
                squares[p.Square].Piece = p;
            }
        }

        // private (List<Square>, List<Square>) UpdatePotentialCheckSquares()
        // {
        //     (List<Square>, List<Square>) result;
        //     result.Item1 = UpdatePotentialCheckSquares(Piece.GetPieces(Piece.PType.King, Piece.PColor.White, Pieces)[0]);
        //     result.Item2 = UpdatePotentialCheckSquares(Piece.GetPieces(Piece.PType.King, Piece.PColor.Black, Pieces)[0]);
        //     return result;
        // }

        // private (List<Square>, List<Square>) UpdatePotentialCheckSquares(Piece king)
        // {
            
        // }

        private void CreateInitialAttackMask()
        {
            bool origalTurn = turnWhite;
            turnWhite = true;
            List<Move> ms = GetAllLegalMoves();
            foreach(Move move in ms)
                move.Target.ControlledByWhite.Add(move.Piece);
            ms.Clear();
            turnWhite = false;
            ms = GetAllLegalMoves();
            foreach(Move move in ms)
                move.Target.ControlledByBlack.Add(move.Piece);
            ms.Clear();
            turnWhite = origalTurn;
        }

        // Generate all moves
        public List<Move> GetAllLegalMoves(bool pseudoLegal = true)
        {
            List<Move> moves = new List<Move>();
            Piece.PColor c = turnWhite ? Piece.PColor.White : Piece.PColor.Black;
            //AM I IN CHECK?
            Square king = Squares[Piece.GetPieces(c, Piece.PType.King, Pieces)[0].Square];
            // Double check?
            if (king.GetAttackingPieces(isWhite).Count > 1)
            {
                return GetLegalMoves(king.Piece);
            }
            // else if (king.GetAttackingPieces(isWhite).Count > 0)
            // {
            //     doubleCheck = true;
            // }
            foreach (Piece p in Piece.GetPieces(c, Pieces))
            {
                moves.AddRange(GetLegalMoves(p, king));
            }
            return moves;
        }

        // Generate moves
        public List<Move> GetLegalMoves(Piece piece, Square kingSquare)
        {
            countlegalmovescheck++;
            List<Move> moves = new List<Move>();
            Square square = Squares[piece.Square];
            
            bool inCheck = kingSquare.IsAttacked(turnWhite);
            bool inPin = SquarePinned(square, out Piece pinningPiece);

            // Cant not move a pinned piece if in check
            if (inCheck && inPin && piece.Type != Piece.PType.King)
                return moves;

            // Can only castle while not in check
            if (piece.Type == Piece.PType.King && !inCheck)
                moves = GetCastlingMoves(piece, previousMove);

            if (piece.Type == Piece.PType.Pawn)
                moves.AddRange(FindPawnMoves(piece, previousMove));

            List<(int,int)> dirs = piece.Moveset;
            if (inPin)
            {
                // Can only move on the axis between the attacking piece and the king.
                (int,int) pinningDir = Utils.DetermineAttackDirection(kingSquare.Sq, pinningPiece.Square);
                List<(int,int)> pinDirs = new();
                pinDirs.Add(pinningDir);
                pinDirs.Add((pinningDir.Item1 * -1),(pinningDir.Item2 * -1));
                dirs = dirs.FindAll(d => pinDirs.Contains(d));
                pinDirs.Clear();
            }
            else if (inCheck)
            {
                // Can I Take the attacking piece?
                // Can I Move to block
                // Except for knights checking my king
                Piece attackingPiece = kingSquare.GetAttackingPieces(turnWhite)[0];
                List<(int,int)> legalSquares = new();
                if (attackingPiece.Type != Piece.PType.Knight)
                {
                    legalSquares = GetSquaresInRay(attackingPiece.Square, kingSquare.Sq, Utils.DetermineAttackDirection(attackingPiece.Square, kingSquare.Sq));
                }
                else {
                    legalSquares.Add(Squares[attackingPiece.Square]);
                }
            }
            foreach(var dir in dirs)
            {
                (int,int) target = square.AddDir(dir);
                if (piece.LongRange)
                {
                    Square currentSquare = square;
                    while (SquareExists(currentSquare, dir) && !Blocked(piece, target))
                    {
                        Piece enemy = Squares[target].Piece;
                        if ((inCheck && legalSquares.Contains(square)) || !inCheck)
                            moves.Add(new Move(square, target, piece, Move.MFlag.None, enemy));
                        if (enemy)
                            break;
                        currentSquare = target;
                        target = Utils.AddDir(target, dir);
                    }
                }
                else if (piece.Type != Piece.PType.Pawn)
                {
                    if (SquareExists(square, dir) && !Blocked(piece, target))
                    {
                        Piece enemy = Squares[target].Piece;
                        if ((inCheck && legalSquares.Contains(square)) || !inCheck)
                            moves.Add(new Move(square, target, piece, Move.MFlag.None, enemy));
                    }
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


        private bool SquareExists(Square square, (int,int) dir, bool reverse = false)
        {
            if (reverse)
                dir = (dir.Item1, dir.Item2 * -1);
            if (square.File + dir.Item1 < 0 || square.Rank + dir.Item1 > 7)
                return false;
            if (square.File + dir.Item2 < 0 || square.Rank + dir.Item2 > 7)
                return false;
            return true;
        }

        private bool Blocked(Piece piece, Square destination)
        {
            if (destination.Piece)
                if (Utils.SameColor(piece, destination.Piece))
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
                int rank = turnWhite ? 0 : 7;
                Square kingSquare = Squares[4 /*e*/, rank];
                if (kingSquare.IsAttacked(turnWhite))
                    return moves;
                bool canCastle = true;
                // TODO Not sure if allowed
                if (Squares[0,rank].Piece?.PMoved == 0 && Qq)
                {
                    for (int file = 1; file <= 3; file++)
                    {
                        var sq = Squares[file,rank];
                        // piece in the way
                        if (sq.Piece) {
                            canCastle = false;
                        }
                        // King would move through check.
                        else if (sq.IsAttacked(turnWhite)) {
                            canCastle = false; 
                        }
                        if (!canCastle) break;
                    }
                    if (canCastle) 
                        moves.Add(new Move(kingSquare, Squares[1,rank], king, Move.MFlag.Castling));
                }
                if (Squares[7,rank].Piece?.PMoved == 0 && Kk)
                {
                    bool canCastle = true;
                    for (int file = 5; file <= 6; file++)
                    {
                        var sq = Squares[file,rank];
                        // piece in the way
                        if (sq.Piece) {
                            canCastle = false;
                        }
                        // King would move through check.
                        else if (sq.IsAttacked(turnWhite)) {
                            canCastle = false; 
                        }
                        if (!canCastle) break;
                    }
                    if (canCastle) 
                        moves.Add(new Move(kingSquare, Squares[6,rank], king, Move.MFlag.Castling));
                }   
            }
            return moves;
        }

        /// <summary>
        /// Generates moves for promotions pushes captures for pawns.
        /// move forward if not blocked
        /// push is on original rank && not blocked
        /// capture if enemy piece there
        /// capture if en passant
        /// promote
        /// </summary>
        /// <param name="piece">Any piece</param>
        /// <param name="previousMove">Previous move</param>
        /// <returns></returns>
        private List<Move> FindPawnMoves(Piece pawn, Move previousMove, bool firstPass)
        {
            List<Move> moves;
            Square sq = Squares[piece.Square];
            int direction = turnWhite ? 1 : -1;
            int promotionRank = direction == 1 ? 7 : 0;
            int pushRank = direction == 1 ? 1 : 6;

            #region Pinned
            if (SquarePinned(sq, out Piece pinningPiece))
            {
                // Capture pinning piece?
                if ((pinningPiece.Square.Item1 == sq.File - 1 || pinningPiece.Square.Item1 == sq.File + 1) && pinningPiece.Square.Item2 = sq.Rank + direction)
                {
                    if (sq.Rank + direction == promotionRank)
                        moves.AddRange(AddPromotionMoves(sq, Squares[pinningPiece.Square], pawn, p));
                    else
                        moves.Add(new Move(sq, Squares[pinningPiece.Square], pawn, 0, pinningPiece));
                    return moves;
                }
                // Move towards pinning piece?
                else if (Utils.DetermineAttackDirection(pawn, pinningPiece) == (0,direction))
                {
                    if (Squares[sq.File, sq.Rank + direction].Piece == null)
                    {
                        //We can never promote as we are partially pinned.
                        moves.Add(new Move(sq, Squares[sq.File, sq.Rank + direction], pawn));
                    }
                    // Can we push this pawn?
                    if(sq.Rank == pushRank && Squares[sq.File, sq.Rank + 2 * direction].Piece == null)
                    {
                        moves.Add(new Move(sq, Squares[sq.File, sq.Rank + 2 * direction], pawn));
                    }
                }
                // We are pinned and we can not
                // 1) Capature our attacker
                // 2a) Move Towards our attacker
                // 2b) Push Towards our attacker
                else
                {
                    return moves;
                }
            }
            #endregion
            #region Moving
            // Can I move forward?
            if (Squares[sq.File, sq.Rank + direction].Piece == null)
            {
                //Will I promote? 
                if (sq.Rank + direction == promotionRank)
                {
                    moves.AddRange(AddPromotionMoves(sq, Squares[sq.File, sq.Rank + direction], pawn));
                }
                //No promotion
                else
                {
                    moves.Add(new Move(sq, Squares[sq.File, sq.Rank + direction], pawn));
                    // Can I push this pawn?
                    if(sq.Rank == pushRank && Squares[sq.File, sq.Rank + 2 * direction].Piece == null)
                    {
                        moves.Add(new Move(sq, Squares[sq.File, sq.Rank + 2 * direction], pawn));
                    }
                }
            }
            #endregion
            #region Capturing
            // Would I promote on captue?
            for (int file = -1; file <= 1; file += 2)
            {
                Square dest = Squares[sq.File + file, sq.Rank + direction];
                if (dest.Sq == enPassantTargetSquare)
                    moves.Add(new Move(sq, dest, pawn, Move.MFlag.EnPassant));//, Squares[sq.File + file, sq.Rank].Piece));
                Piece p = dest.Piece;
                if (p?.Color == Piece.GetOtherColor(pawn))
                {
                    if (sq.Rank + direction == promotionRank)
                        moves.AddRange(AddPromotionMoves(sq, Squares[p.Square], pawn, p));
                    else
                        moves.Add(new Move(sq, Squares[p.Square], pawn, 0, p));
                }
            }
            #endregion
            return moves;
        }

        private List<Move> AddPromotionMoves(Square orig, Square dest, Piece p, Piece p2 = null)
        {
            List<Move> moves;
            moves.Add(new Move(orig, dest, p, Move.MFlag.PromotionToKnight), p2);
            moves.Add(new Move(orig, dest, p, Move.MFlag.PromotionToBishop), p2);
            moves.Add(new Move(orig, dest, p, Move.MFlag.PromotionToRook), p2);
            moves.Add(new Move(orig, dest, p, Move.MFlag.PromotionToQueen), p2);
            return moves;
        }

        private bool SquarePinned(Square sq, out Piece pinningPiece)
        {
            // If im not attacked, im not pinned
            if (!sq.IsAttacked)
                return false;
            
            foreach(Piece p in sq.GetAttackingPieces(turnWhite))
            {
                // Cant be pinned by pawns, knights or kings
                if (p.LongRange)
                {
                    Piece king = Piece.GetPieces(Piece.PType.King, sq.Piece.Color, Pieces)[0];
                    var dirToMe   = Utils.DetermineAttackAngle(p.Square, sq.Sq);
                    var dirToKing = Utils.DetermineAttackAngle(p.Square, king.Square);
                    if (dirToMe == dirToKing)
                    {
                        var dir = Utils.DetermineAttackDirection(p.Square, sq.Sq);
                        var testSquare = sq.Sq;
                        while(Utils.SquareExists(testSquare))
                        {
                            testSquare = (testSquare.Item1 + dir.Item1, testSquare.item2 + dir.Item2);
                            var testPiece = Square[testSquare].Piece;
                            // Im pinned
                            if (testPiece == king)
                            {
                                pinningPiece = p;
                                return true;
                            }
                            // There is a piece between me and the king.
                            if (testPiece != null)
                                break;                             
                        }
                    }
                }
            }
        }

        private List<(int, int)> GetSquaresInRay((int, int) sq1, (int, int) sq2, (int,int) dir)
        {
            List<(int,int)> squaresInRay = new List<(int, int)>();
            int f1 = sq1.Item1;
            int r1 = sq1.Item2;
            int f2 = sq2.Item1;
            int r2 = sq2.Item2;
            while(f1 != f2 && r1 != r2)
            {
                squaresInRay.Add((f1,r2));
                f1 += dir.Item1;
                r1 += dir.Item2;
            }
            return squaresInRay;
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