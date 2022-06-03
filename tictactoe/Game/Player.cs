using System;

namespace tictactoe.Game
{
    public abstract class Player
    {
        private readonly Char symb;
        private readonly int num;

        protected Player(Char symb, int num)
        {
            this.symb = symb;
            this.num = num;
        }

        public char ToChar()
        {
            return this.symb;
        }

        public int ToInt()
        {
            return num;
        }

        public bool IsX
        {
            get
            {
                return this is PlayerX;
            }
        }
    }

    public class PlayerX : Player
    {
        public PlayerX() : base('X', 1)
        {
        }
    }

    public class PlayerO : Player
    {
        public PlayerO() : base('O', 2)
        {
        }
    }

    public class PlayerDraw : Player
    {
        public PlayerDraw() : base('D', 3)
        {
        }
    }
}
