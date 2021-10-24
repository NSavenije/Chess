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

        public void SetActiveSquare(int square)
        {
            (int, int) fr     = Utils.SquareToFileRank(square);
            (int, int) active = Utils.SquareToFileRank(activeSquare);
            if (square < 0 || squarePieceRenderers[fr.Item1, fr.Item2].sprite == null)
            {
                if (cachedSquareColor != Color.gray)
                    squareRenderers[active.Item1, active.Item2].material.color = cachedSquareColor;
                cachedSquareColor = Color.gray;
                activeSquare = -1;
                return;
            }
            if (cachedSquareColor != Color.gray)
                squareRenderers[active.Item1, active.Item2].material.color = cachedSquareColor;
            activeSquare = square;
            cachedSquareColor = squareRenderers[fr.Item1, fr.Item2].material.color;
            squareRenderers[fr.Item1, fr.Item2].material.color = activeSquareColor;
        }

        public void UpdatePieceSprites(int[] pieces)
        {
            for(int i = 0; i < 64; i++)
            {
                (int, int) fileRank = Utils.SquareToFileRank(i);
                squarePieceRenderers[fileRank.Item1, fileRank.Item2].sprite = pieceSprites[pieces[i]];
            }
        }

        

        void Update()
        {

        }
    }
}

