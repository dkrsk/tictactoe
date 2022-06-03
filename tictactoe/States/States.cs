using System.Diagnostics;
using System.Windows.Controls;
using System.Threading.Tasks;

using tictactoe.Game;
using tictactoe.Network;
using tictactoe.GUI;

namespace tictactoe.States
{
    public abstract class State
    {
        protected readonly GameLogic game;
        protected readonly NetPeer netPeer;
        protected readonly MainWindow window;

        public State(GameLogic game)
        {
            this.game = game;
            this.netPeer = game.netPeer;
            this.window = game.Window;
        }

        public abstract Task Click(Button btn);

        public void Disconnect()
        {
            game.Disconnect();
        }
    }

    public class MyTurnState : State
    {
        public MyTurnState(GameLogic game) : base(game)
        {
        }

        public override async Task Click(Button btn)
        {
            if (game.IsEnded)
            {
                game.ChangeState(game.idleState);
                return;
            }

            Player player = game.player;
            short tag = short.Parse(btn.Tag.ToString());
            Debug.WriteLine("MyTurn Click");

            if (game.gameField[tag] == null)
            {
                game.View.Dispatcher.Invoke(() => game.View.lblInfo.Content = "Opponent turn!");
                game.View.SetBtnImage(tag, player);

                game.gameField[tag] = player.ToInt();
                await netPeer.Send(tag);
                game.ChangeState(game.idleState);

                var winner = game.CheckMove();
                if (winner != null)
                {
                    Debug.WriteLine("winner is not null!");
                    game.FinishTheGame(winner);
                }
            }
        }
    }

    public class IdleState : State
    {
        public IdleState(GameLogic game) : base(game)
        {
        }

        public override async Task Click(Button btn)
        {
            Debug.WriteLine("Idle Click");
        }
    }
}
