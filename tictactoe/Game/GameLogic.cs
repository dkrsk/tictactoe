using System;
using System.Diagnostics;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using tictactoe.Network;
using tictactoe.GUI;
using tictactoe.States;

namespace tictactoe.Game
{
    public class GameLogic
    {
        public readonly NetPeer netPeer;
        bool RemoteIsReady = false;

        public Player? player;
        public Player? opponent;

        public MainWindow Window { get; }
        public GameView View { get; set; }

        readonly Random rand;
        short myChoose;
        short? prevRemoteChoose;
        
        State state;
        public readonly IdleState idleState;
        public readonly MyTurnState myTurnState;

        public int?[] gameField = new int?[9];

        public delegate void EndGameHandler(Player player);
        public event EndGameHandler GameEnded;
        private bool isEnded = false;
        public bool IsEnded { get { return isEnded; } }


        public GameLogic(NetPeer netPeer, MainWindow window)
        {
            GameEnded += (Player player) => Debug.WriteLine($"player{(player == null ? "NULL" : player.ToChar())} won!");

            this.Window = window;
            Window.Dispatcher.Invoke(() => this.View = new GameView(window));

            this.netPeer = netPeer;
            this.netPeer.MessageReceived += InitGameHandler;

            idleState = new(this);
            myTurnState = new(this);

            ChangeState(idleState);

            rand = new(netPeer.ExternalIP.GetHashCode() + netPeer.RemotePort + DateTime.Now.Millisecond);
            this.myChoose = (short)rand.Next(10, 101);

            //Thread.Sleep(500);
            new Thread(async () =>
            {
                do
                {
                    await netPeer.Send(NetCodes.Connect);
                    Thread.Sleep(100);
                } while (!RemoteIsReady);
                

                short firstChoose = myChoose;
                while (player == null && firstChoose == myChoose)
                {
                    await netPeer.Send(myChoose);
                    Thread.Sleep(100);
                }
            }).Start();
        }

        private void InitGameHandler(short remoteChoose)
        {
            if (remoteChoose == NetCodes.Connect)
            {
                Debug.WriteLine("!! Remote is ready!");
                RemoteIsReady = true;
                return;
            }

            if (remoteChoose < 10 || remoteChoose > 100)
            {
                return;
            }

            if (prevRemoteChoose != null && remoteChoose == prevRemoteChoose)
            {
                netPeer.Send(myChoose);
                return;
            }

            prevRemoteChoose = remoteChoose;

            Debug.WriteLine($"remC: {remoteChoose} | myC: {myChoose}");
            if (myChoose > remoteChoose)
            {
                InitPlayer(new PlayerX());

                //netPeer.MessageReceived -= InitGameHandler;

                StartGame();
            }
            else if (myChoose < remoteChoose)
            {
                InitPlayer(new PlayerO());

                //netPeer.MessageReceived -= InitGameHandler;

                StartGame();
            }
            else
            {
                this.myChoose = (short)rand.Next(1,101);
                netPeer.Send(myChoose);
            }
        }

        private void StartGame()
        {
            Window.LoadView(View);

            netPeer.MessageReceived += RemoteClick;

            if (player.IsX)
            {
                ChangeState(myTurnState);
            }
        }

        private void InitPlayer(Player player)
        {
            this.player = player;
            if (player.IsX)
            {
                this.opponent = new PlayerO();
            }
            else
            {
                this.opponent = new PlayerX();
            }
        }

        public void ChangeState(State state)
        {
            this.state = state;
        }

        public Player? CheckMove()
        {
            for(int i=0; i<=2; i++)
            {
                if (gameField[i] != null && gameField[i] == gameField[i+3] && gameField[i] == gameField[i + 6])
                {
                    return gameField[i] == 1 ? new PlayerX() : new PlayerO();
                }
            }
            for (int i = 0; i <= 6; i += 3)
            {
                if(gameField[i] != null && gameField[i] == gameField[i + 1] && gameField[i] == gameField[i + 2])
                {
                    return gameField[i] == 1 ? new PlayerX() : new PlayerO();
                }
            }
            if (gameField[0] != null && gameField[0] == gameField[4] && gameField[0] == gameField[8])
            {
                return gameField[0] == 1 ? new PlayerX() : new PlayerO();
            }
            if (gameField[2] != null && gameField[2] == gameField[4] && gameField[2] == gameField[6])
            {
                return gameField[2] == 1 ? new PlayerX() : new PlayerO();
            }
            return null;
        }

        private void RemoteClick(short cell)
        {
            if (cell <= 8 && gameField[cell] == null)
            {
                gameField[cell] = opponent.ToInt();

                View.Dispatcher.Invoke(() =>
                {
                    View.lblInfo.Content = "Your turn!";
                });
                View.SetBtnImage(cell, opponent);

                var winner = CheckMove();
                if (winner != null)
                {
                    FinishTheGame(winner);
                }

                ChangeState(myTurnState);
            }
        }

        public async Task GameClick(Button btn)
        {
            await state.Click(btn);
        }

        public void FinishTheGame(Player winner)
        {
            isEnded = true;
            ChangeState(idleState);
            GameEnded.Invoke(winner);
            View.Dispatcher.Invoke(() => View.btnRevenge.Visibility = System.Windows.Visibility.Visible);
        }

        public void Disconnect()
        {
            netPeer.Close();
            Window.LoadView(Window.StartView);
        }

    }
}
