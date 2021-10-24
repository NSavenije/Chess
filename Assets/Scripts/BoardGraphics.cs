using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class BoardGraphics : MonoBehaviour
    {
        public Color lightSquareColor;
        public Color darkSquareColor;

        MeshRenderer[,] squareRenderers;
        SpriteRenderer[,] squarePieceRenderers;
        public Sprite[] pieceSprites;
        void Start()
        {
            CreateBoardGraphics();
        }

        void CreateBoardGraphics()
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
                    square.name = GetSquareNameFromCoordinate(file, rank);
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
                    //pieceRenderer.sprite = pieceSprites[Piece.Bishop | Piece.White];
                    
                }
            }
        }

        private void LoadFEN(string fen)
        {
            "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
            string[] ranks = fen.Split('/');
            for (int rank = 0; rank < ranks.Length; rank++)
            {
                foreach (char c in ranks[rank])
                {

                }
            }
        }

        private string GetSquareNameFromCoordinate(int file, int rank)
        {
            char fileString = (char)(file + 65);
            string squareName = "";
            squareName += fileString;
            squareName += (rank + 1);
            return squareName;
        }

        void Update()
        {

        }
    }
}

