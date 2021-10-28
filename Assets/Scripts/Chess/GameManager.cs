using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts
{
    public class GameManager : MonoBehaviour
    {
        public bool humanPlayerWhite;
        public bool humanPlayerBlack;
        public Board Board;
        public GameObject BoardGraphics;
        public Camera cam;
        public InputState inputState;
        public Algo ComputerPlayerAlgorithm;
        public bool Paused;
        public int perftDepth;

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
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Paused = !Paused;
                Debug.Log("Pause Toggled");
            }
            if (!Paused)
            {
                if (HumanPlayerTurn())
                {
                    if (Input.GetKeyDown(KeyCode.Z))
                    {
                        Board.UndoMove();
                        boardGraphics.UpdatePieceSprites(Board.Squares);
                    }
                    if (Input.GetKeyDown(KeyCode.Mouse0))// && previousMove.Flag != Move.MFlag.Promoting)
                    {
                        Vector3 position = cam.ScreenToWorldPoint(Input.mousePosition);
                        int square = Utils.GetSquareFromCoordinate(position);
                        bool nonEmptySquare = Board.TryGetPieceFromSquare(square, out Piece piece);

                        // If no square was selected before, select a square.
                        if (inputState == InputState.None && nonEmptySquare && Utils.SameColor(Board.turnWhite, Piece.IsWhite(piece.Code)))
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
                            if (Board.legalMoves.Exists(x => x.Target == square))
                            {
                                Board.DoMove(Board.legalMoves.Find(x => x.Target == square));
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
                    Board.DoMove(nextMove);
                    boardGraphics.UpdatePieceSprites(Board.Squares);
                }
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    Debug.Log("perft: " + Perft.DoPerft(Board, perftDepth));
                }
                if (Input.GetKeyDown(KeyCode.Mouse1))
                {
                    var results = Perft.Divide(Board, perftDepth);
                    foreach (var result in results)
                    {
                        Debug.Log(result.Item1 + ": " + result.Item2);
                    }
                }

            }
        }

        private Move FindRandomLegalMove()
        {
            List<Move> moves = new List<Move>();
            moves = Board.FindAllLegalMoves();
            return moves[Random.Range(0, moves.Count - 1)];
        }

        private bool HumanPlayerTurn()
        {
            return Board.turnWhite ? humanPlayerWhite : humanPlayerBlack;
        }
    }
}