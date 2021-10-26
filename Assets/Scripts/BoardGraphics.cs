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

        public void HighlightLegalMoves(List<int> legalmoves)
        {
            foreach(int square in legalmoves)
            {
                //Debug.Log("Squarenr " + square);
                (int, int) fr = Utils.SquareToFileRank(square);
                squareRenderers[fr.Item1, fr.Item2].material.color = legalMoveSquareColor;
            }
        }

        public void SetActiveSquare(int square)
        {
            ResetBoardColors();
            if (square >= 0)
            {
                (int, int) fr = Utils.SquareToFileRank(square);
                squareRenderers[fr.Item1, fr.Item2].material.color = activeSquareColor;
            }
        }

        public void UpdatePieceSprites(Piece[] pieces)
        {
            for(int i = 0; i < 64; i++)
            {
                (int, int) fileRank = Utils.SquareToFileRank(i);
                if (pieces[i] != null)
                    squarePieceRenderers[fileRank.Item1, fileRank.Item2].sprite = pieceSprites[pieces[i].Code % 32];
                else
                    squarePieceRenderers[fileRank.Item1, fileRank.Item2].sprite = pieceSprites[0];
            }
        }

        

        void Update()
        {

        }
    }
}

