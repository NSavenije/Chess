namespace Assets.Scripts
{
    public class Square
    {
        public Piece Piece;
        public (int,int) Sq;
        public int File;
        public int Rank;
        public List<Piece> ControlledByWhite;
        public List<Piece> ControlledByBlack;


        public Square(int f, int r) {

        }

        public bool IsAttacked (bool isWhite)
        {
            return isWhite ? ControlledByBlack > 0 : ControlledByWhite > 0;
        }
        public List<Piece> GetAttackingPieces (bool isWhite) 
        {
            return turnWhite ? ControlledByBlack : ControlledByWhite;    
        }

        public (int,int) AddDir((int,int) sq2)
        {

            return (File + sq2.Item1, Rank + sq2.Item2);
        }
    }
}