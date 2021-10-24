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

        void Start()
        {
            Board = new Board();
            BoardGraphics boardGraphics = BoardGraphics.GetComponent<BoardGraphics>();
            boardGraphics.CreateBoardGraphics();
            LoadFEN("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR");
            boardGraphics.UpdatePieceSprites(Board.Squares);
        }

        public void LoadFEN(string fen)
        {
            //string startingPosition = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
            string[] ranks = fen.Split('/');
            int[] board = new int[64];
            for (int rank = 7; rank >= 0; rank--)
            {
                for (int file = 0; file < 8; file++)
                {
                    char c = ranks[rank][file];
                    if (char.IsNumber(c))
                    {
                        file += (int)char.GetNumericValue(c);
                        //squarePieceRenderers[file, rank].sprite = pieceSprites[Piece.None];
                        continue;
                    }
                    char pieceCode = c;
                    int piece = 0;
                    if (char.IsUpper(c))
                    {
                        piece = Piece.White;
                        pieceCode = char.ToLower(c);
                    }
                    else
                    {
                        piece = Piece.Black;
                    }
                    switch (pieceCode)
                    {
                        case 'p':
                            piece |= Piece.Pawn;
                            break;
                        case 'r':
                            piece |= Piece.Rook;
                            break;
                        case 'n':
                            piece |= Piece.Knight;
                            break;
                        case 'b':
                            piece |= Piece.Bishop;
                            break;
                        case 'q':
                            piece |= Piece.Queen;
                            break;
                        case 'k':
                            piece |= Piece.King;
                            break;
                        default:
                            piece = Piece.None;
                            break;
                    }
                    //squarePieceRenderers[file, 7 - rank].sprite = pieceSprites[piece];
                    board[(7 - rank) * 8 + file] = piece;
                }
            }
            Board.Squares = board;
        }

        void Update()
        {
            BoardGraphics bg = BoardGraphics.GetComponent<BoardGraphics>();
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                int square = GetSquareFromCoordinate(Input.mousePosition);
                // If no square was selected, select a square.
                if (inputState == InputState.None)
                {
                    Board.ActiveSquare = square;
                    bg.SetActiveSquare(square);
                    inputState = InputState.Selected;
                }
                // If a second square is selected, move a piece.
                else
                {
                    Board.ActiveSquare = -1;
                    bg.SetActiveSquare(-1);
                    inputState = InputState.None;
                }
            }
        }

        private int GetSquareFromCoordinate(Vector3 position)
        {
            position = cam.ScreenToWorldPoint(position);
            //Debug.Log($"pos = ({Math.Floor(position.x) + 4}, {Math.Floor(position.y) + 4})");
            int file = (int)(Math.Floor(position.x) + 4);
            int rank = (int)(Math.Floor(position.y) + 4);
            int square = Utils.FileRankToSquare(file, rank);
            if (rank >= 0 && rank < 8 && file >= 0 && file < 8)
                return square;
            else
                return -1;
        }
    }
}