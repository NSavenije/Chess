using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class GameManager : MonoBehaviour
    {
        public bool turnWhite;
        public Board Board;
        public GameObject BoardGraphics;
        public Camera cam;
        public InputState inputState;

        public enum InputState{
            None,
            Selected
        }

        private BoardGraphics boardGraphics;
        private List<Move> previousMoves; 

        void Start()
        {
            Board = new Board();
            boardGraphics = BoardGraphics.GetComponent<BoardGraphics>();
            boardGraphics.CreateBoardGraphics();
            previousMoves = new List<Move>();
            Board.Squares = FenUtils.LoadFEN(FenUtils.StartingPosition, out List<Piece> pieces);
            Board.Pieces = pieces;
            boardGraphics.UpdatePieceSprites(Board.Squares);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                Vector3 position = cam.ScreenToWorldPoint(Input.mousePosition);
                int square = Utils.GetSquareFromCoordinate(position);
                bool nonEmptySquare = Board.TryGetPieceFromSquare(square, out Piece piece);
                Move previousMove = previousMoves.Count == 0 ? new Move(-1, -1, null) : previousMoves[previousMoves.Count - 1];

                //Debug.Log($"Selecting square: {square}, nes = {nonEmptySquare} and sc = {Utils.SameColor(turnWhite, Piece.IsWhite(piece))} and wp = {Piece.IsWhite(piece)}");
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
        
        private void MovePiece(Move move)
        {
            // House Keeping.
            turnWhite = !turnWhite;
            previousMoves.Add(move);
            
            // Remove Captured Piece.
            if (Board.TryGetPieceFromSquare(move.Target, out Piece targetPiece))
                Board.Pieces.Remove(targetPiece);

            if (move.Flag == Move.MFlag.EnPassant)
            {
                Move pushMove = previousMoves[previousMoves.Count - 2];
                Board.Pieces.Remove(pushMove.Piece);
                Board.Squares[pushMove.Target] = null;
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