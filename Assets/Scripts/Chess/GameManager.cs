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
        private List<Board> results;
        private int resCounter;

        void Start()
        {
            boardGraphics = BoardGraphics.GetComponent<BoardGraphics>();
            boardGraphics.CreateBoardGraphics();
            Board = FenUtils.LoadFEN("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");

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
                        (int,int) squareId = Utils.GetSquareIdFromCoordinate(position);
                        bool emptySquare = true;
                        Square square;
                        if (squareId.Item1 >= 0)
                        {
                            square = Board.Squares[squareId];
                            if (square.Piece) emptySquare = false;
                        }
                    
                        // If no square was selected before, select a square.
                        if (inputState == InputState.None && !emptySquare && Utils.SameColor(Board.turnWhite, (square.Piece.Color == Piece.PColor.White)))
                        {
                            Board.ActiveSquare = square;
                            inputState = InputState.Selected;
                            boardGraphics.SetActiveSquare(square);
                            Board.legalMoves = Board.GetLegalMoves(square.Piece);
                            boardGraphics.HighlightLegalMoves(Board.legalMoves);
                        }
                        // If a second square is selected, move a piece.
                        else if (inputState == InputState.Selected)
                        {
                            if (Board.legalMoves.Exists(x => x.Target == square.Sq))
                            {
                                Board.DoMove(Board.legalMoves.Find(x => x.Target == square.Sq));
                                boardGraphics.UpdatePieceSprites(Board.Squares);
                            }
                            Board.ActiveSquare = null;
                            boardGraphics.SetActiveSquare(null);
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
                    boardGraphics.UpdatePieceSprites(Board.Squares);
                }
                if (Input.GetKeyDown(KeyCode.Mouse1))
                {
                    var results = Perft.Divide(Board, perftDepth);
                    foreach (var result in results)
                    {
                        Debug.Log(result.Item1 + ": " + result.Item2);
                    }
                    boardGraphics.UpdatePieceSprites(Board.Squares);
                }
                if (Input.GetKeyDown(KeyCode.D))
                {
                    results = Perft.DoPerftBoards(Board, perftDepth);
                    resCounter = 0;
                    Debug.Log("DONE!");
                }
                if (Input.GetKeyDown(KeyCode.RightArrow) && resCounter < results.Count)
                {
                    Debug.Log("Next");
                    Board.Squares = results[resCounter++].Squares;
                    boardGraphics.UpdatePieceSprites(Board.Squares);
                }

            }
        }

        private Move FindRandomLegalMove()
        {
            List<Move> moves = new List<Move>();
            moves = Board.GetAllLegalMoves();
            return moves[Random.Range(0, moves.Count - 1)];
        }

        private bool HumanPlayerTurn()
        {
            return Board.turnWhite ? humanPlayerWhite : humanPlayerBlack;
        }
    }
}