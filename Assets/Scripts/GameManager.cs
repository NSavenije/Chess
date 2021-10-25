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
            Board.Square = FenUtils.LoadFEN(FenUtils.StartingPosition);
            boardGraphics.UpdatePieceSprites(Board.Square);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                Vector3 position = cam.ScreenToWorldPoint(Input.mousePosition);
                int square = Utils.GetSquareFromCoordinate(position);
                bool nonEmptySquare = Board.TryGetPieceFromSquare(square, out int piece);

                //Debug.Log($"Selecting square: {square}, nes = {nonEmptySquare} and sc = {Utils.SameColor(turnWhite, Piece.IsWhite(piece))} and wp = {Piece.IsWhite(piece)}");
                // If no square was selected before, select a square.
                if (inputState == InputState.None && nonEmptySquare && Utils.SameColor(turnWhite, Piece.IsWhite(piece)))
                {
                    Board.ActiveSquare = square;
                    inputState = InputState.Selected;
                    boardGraphics.SetActiveSquare(square);
                    Board.SetLegalMoves(square);
                    boardGraphics.HighlightLegalMoves(Board.legalMoves);
                }
                // If a second square is selected, move a piece.
                else if (inputState == InputState.Selected)
                {
                    if (Board.legalMoves.Contains(square))
                    {
                        MovePiece(Board.ActiveSquare, square);
                        boardGraphics.UpdatePieceSprites(Board.Square);
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
            int piece = Board.Square[selectedSquare];
            piece |= Piece.Moved;
            Board.Square[selectedSquare] = 0;
            Board.Square[destinationSquare] = piece;
        }
        
    }
}