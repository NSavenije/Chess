using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Unity;

namespace Assets.Scripts
{
    public class Board
    {
        int[] _squares = new int[64];
        public int[] Squares
        {
            get { return _squares; }
            set { _squares = value; }
        }

        public int ActiveSquare;
        public Board()
        {
            ActiveSquare = -1;
        }
    }
}