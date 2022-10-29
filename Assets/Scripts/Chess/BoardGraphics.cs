using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class BoardGraphics : MonoBehaviour
    {
        public Color lightSquareColor;
        public Color darkSquareColor;
        public Color activeSquareColor;
        public Color legalMoveSquareColor;

        MeshRenderer[,] squareRenderers;
        SpriteRenderer[,] squarePieceRenderers;
        int activeSquare;
        Color cachedSquareColor;
        public Sprite[] pieceSprites;
        void Start()
        {
            activeSquare = -1;
            cachedSquareColor = Color.gray;
        }

        public void CreateBoardGraphics()
        {
            Shader squareShader = Shader.Find("Unlit/Color");
            squareRenderers = new MeshRenderer[8, 8];
            squarePieceRenderers = new SpriteRenderer[8, 8];
            for (int rank = 0; rank < 8; rank++)
            {
                for (int file = 0; file < 8; file++)
                {
                    bool isLightSquare = (file + rank) % 2 == 1;
                    Color squareColor = isLightSquare ? lightSquareColor : darkSquareColor;
                    Vector2 squarePosition = new Vector2(file - 3.5f, rank - 3.5f);
                    Transform square = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
                    square.parent = transform;
                    square.name = Utils.GetSquareNameFromCoordinate(file, rank);
                    square.position = squarePosition;
                    Material squareMaterial = new Material(squareShader);
                    squareMaterial.color = squareColor;

                    squareRenderers[file, rank] = square.gameObject.GetComponent<MeshRenderer>();
                    squareRenderers[file, rank].material = squareMaterial;

                    SpriteRenderer pieceRenderer = new GameObject("Piece").AddComponent<SpriteRenderer>();
                    pieceRenderer.transform.parent = square;
                    pieceRenderer.transform.position = squarePosition;
                    pieceRenderer.transform.localScale = Vector3.one * 100 / (2000 / 6f);
                    squarePieceRenderers[file, rank] = pieceRenderer;
                }
            }
        }

        public void ResetBoardColors()
        {
            for(int i = 0; i < 64; i++)
            {
                (int, int) fr = Utils.SquareToFileRank(i);
                squareRenderers[fr.Item1, fr.Item2].material.color = (fr.Item1 + fr.Item2) % 2 == 0 ? darkSquareColor : lightSquareColor;
            }
        }

        public void HighlightLegalMoves(List<Move> legalmoves)
        {
            foreach(Move move in legalmoves)
            {
                //Debug.Log("Squarenr " + square);
                squareRenderers[move.Target.Item1, move.Target.Item2].material.color = legalMoveSquareColor;
            }
        }

        public void SetActiveSquare(Square square)
        {
            ResetBoardColors();
            if (square)
                squareRenderers[square.Sq].material.color = activeSquareColor;
        }

        public void UpdatePieceSprites(Square[,] squares)
        {
            for(int file = 0; file < 8; file++)
            {
                for (int rank = 0; rank < 8; rank++)
                {
                    //
                    Piece p = squares[file,rank].Piece;
                    if (p != null)
                    { 
                        int index = (int)p.Type + (int)p.Color;
                        squarePieceRenderers[file, file].sprite = pieceSprites[index];
                    }
                    else
                        squarePieceRenderers[file, rank].sprite = pieceSprites[0];
               }
            }
        }
    }
}

