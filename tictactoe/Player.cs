using System;

namespace tictactoe.Game
{
    public abstract class Player
    {
        private readonly Char symb;
        private readonly int num;

        public Player(Char symb)
        {
            this.symb = symb;
            this.num = IsX ? 1 : 2;
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
        public PlayerX() : base('X')
        {
        }
    }

    public class PlayerO : Player
    {
        public PlayerO() : base('O')
        {
        }
    }
}
