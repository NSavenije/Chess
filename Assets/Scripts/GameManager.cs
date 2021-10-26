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

        void Start()
        {
            Board = new Board();
            boardGraphics = BoardGraphics.GetComponent<BoardGraphics>();
            boardGraphics.CreateBoardGraphics();
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

                //Debug.Log($"Selecting square: {square}, nes = {nonEmptySquare} and sc = {Utils.SameColor(turnWhite, Piece.IsWhite(piece))} and wp = {Piece.IsWhite(piece)}");
                // If no square was selected before, select a square.
                if (inputState == InputState.None && nonEmptySquare && Utils.SameColor(turnWhite, Piece.IsWhite(piece.Code)))
                {
                    Board.ActiveSquare = square;
                    inputState = InputState.Selected;
                    boardGraphics.SetActiveSquare(square);
                    Board.legalMoves = Board.FindLegalMoves(piece);
                    boardGraphics.HighlightLegalMoves(Board.legalMoves);
                }
                // If a second square is selected, move a piece.
                else if (inputState == InputState.Selected)
                {
                    if (Board.legalMoves.Contains(square))
                    {
                        MovePiece(Board.ActiveSquare, square);
                        boardGraphics.UpdatePieceSprites(Board.Squares);
                    }
                    Board.ActiveSquare = -1;
                    boardGraphics.SetActiveSquare(-1);
                    inputState = InputState.None;
                }
            }
        }
        
        private void MovePiece(int selectedSquare, int destinationSquare)
        {
            turnWhite = !turnWhite;
            Piece piece = Board.Squares[selectedSquare];
            piece.Code |= Piece.Moved;
            piece.PMoved = true;
            piece.Square = destinationSquare;
            Board.Squares[selectedSquare] = null;
            Board.Squares[destinationSquare] = piece;
        }
        
    }
}