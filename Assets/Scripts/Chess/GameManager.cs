using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts
{
    public class GameManager : MonoBehaviour
    {
        public bool turnWhite;
        public bool humanPlayerWhite;
        public bool humanPlayerBlack;
        public Board Board;
        public GameObject BoardGraphics;
        public Camera cam;
        public InputState inputState;
        public Algo ComputerPlayerAlgorithm;
        public bool Paused;

        public enum InputState
        {
            None,
            Selected
        }
        public enum Algo
        {
            None,
            Random
        }

        private BoardGraphics boardGraphics;
        private List<Move> previousMoves;
        private Move previousMove;

        void Start()
        {
            Board = new Board();
            boardGraphics = BoardGraphics.GetComponent<BoardGraphics>();
            boardGraphics.CreateBoardGraphics();
            previousMoves = new List<Move>();
            previousMove = new Move(-1, -1, null, 0);
            Board.Squares = FenUtils.LoadFEN(FenUtils.StartingPosition, out List<Piece> pieces);
            Board.Pieces = pieces;
            boardGraphics.UpdatePieceSprites(Board.Squares);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
                Paused = !Paused;
            if (!Paused)
            {
                if (HumanPlayerTurn())
                {
                    if (Input.GetKeyDown(KeyCode.Mouse0))// && previousMove.Flag != Move.MFlag.Promoting)
                    {
                        Vector3 position = cam.ScreenToWorldPoint(Input.mousePosition);
                        int square = Utils.GetSquareFromCoordinate(position);
                        bool nonEmptySquare = Board.TryGetPieceFromSquare(square, out Piece piece);

                        // If no square was selected before, select a square.
                        if (inputState == InputState.None && nonEmptySquare && Utils.SameColor(turnWhite, Piece.IsWhite(piece.Code)))
                        {
                            Board.ActiveSquare = square;
                            inputState = InputState.Selected;
                            boardGraphics.SetActiveSquare(square);
                            Board.legalMoves = Board.FindLegalMoves(piece, previousMove);
                            boardGraphics.HighlightLegalMoves(Board.legalMoves);
                        }
                        // If a second square is selected, move a piece.
                        else if (inputState == InputState.Selected)
                        {
                            if (Board.legalMoves.Exists(x => x.Target == square))
                            {
                                MovePiece(Board.legalMoves.Find(x => x.Target == square));
                                boardGraphics.UpdatePieceSprites(Board.Squares);
                            }
                            Board.ActiveSquare = -1;
                            boardGraphics.SetActiveSquare(-1);
                            inputState = InputState.None;
                        }
                    }
                }
                else // computers turn
                {
                    Move nextMove;
                    switch (ComputerPlayerAlgorithm)
                    {
                        default:
                            nextMove = FindRandomLegalMove();
                            break;
                    }
                    MovePiece(nextMove);
                    boardGraphics.UpdatePieceSprites(Board.Squares);
                }
            }
        }

        private Move FindRandomLegalMove()
        {
            List<Move> moves = new List<Move>();
            Piece.PColor c = Piece.PColor.White;
            if (previousMove.Piece != null)    
                c = Piece.GetOtherColor(previousMove.Piece);
            foreach (Piece piece in Piece.GetPieces(c, Board.Pieces))
                moves.AddRange(Board.FindLegalMoves(piece, previousMove));
            return moves[Random.Range(0, moves.Count - 1)];   
        }

        private bool HumanPlayerTurn()
        {
            return turnWhite ? humanPlayerWhite : humanPlayerBlack;
        }
        
        private void MovePiece(Move move)
        {
            // House Keeping.
            turnWhite = !turnWhite;
            previousMoves.Add(move);
            previousMove = move;
            
            // Remove Captured Piece.
            if (Board.TryGetPieceFromSquare(move.Target, out Piece targetPiece))
                Board.Pieces.Remove(targetPiece);

            if (move.Flag == Move.MFlag.EnPassant)
            {
                Move pushMove = previousMoves[previousMoves.Count - 2];
                Board.Pieces.Remove(pushMove.Piece);
                Board.Squares[pushMove.Target] = null;
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
                List<Piece> rooks = Piece.GetPieces(Piece.PType.Rook, move.Piece.Color, Board.Pieces);
                bool kingSide = move.Start < move.Target;
                Piece rook = kingSide ? rooks.Find(r => r.Square > move.Piece.Square) : rooks.Find(r => r.Square < move.Piece.Square);
                int target = kingSide ? move.Target - 1 : move.Target + 1;

                // Update the Board
                Board.Squares[rook.Square] = null;
                Board.Squares[target] = rook;

                // Move and Update the Rook;
                rook.Code |= Piece.Moved;
                rook.PMoved = true;
                rook.Square = target;
            }

            // Move and Update the Piece.
            move.Piece.Code |= Piece.Moved;
            move.Piece.PMoved = true;
            move.Piece.Square = move.Target;

            // Update the Board.
            Board.Squares[move.Start] = null;
            Board.Squares[move.Target] = move.Piece;
        }
        
    }
}