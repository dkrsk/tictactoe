using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Net;

using tictactoe.Network;
using tictactoe.Game;

namespace tictactoe.GUI
{
    public partial class MainWindow : Window
    {
        public NetPeer? netPeer;
        public GameLogic? game;

        public MainView StartView { get; }
        public LoadView LoadingView { get; }

        private bool isRevageRequested = false;
        public bool IsRevageRequested { get { return isRevageRequested; } }

        public MainWindow()
        {
            StartView = new(this);
            LoadingView = new(this);

            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = new MainView(this);
            this.DataContext = vm;
            LoadView(StartView);
        }

        public void LoadView(UserControl view)
        {
            Dispatcher.Invoke((UserControl view) =>
            {
                this.OutputView.Content = view;
            }, view);
            //Dispatcher.Invoke((ViewType viewType) =>
            //{
            //    switch (viewType)
            //    {
            //        case ViewType.Main:
            //            this.OutputView.Content = mainView;
            //            break;

            //        case ViewType.Loading:
            //            this.OutputView.Content = loadView;
            //            break;

            //        case ViewType.Game:
            //            this.OutputView.Content = new GameView(this);
            //            break;
            //    }
            //}, viewType);   
        }

        public async Task LoadViewAsync(UserControl view)
        {
            await Task.Run(() => LoadView(view));
        }

        public void InitializeNetPeer(IPAddress adress, PortPair portPair)
        {
            new Thread(async () =>
            {
                this.netPeer = new(adress, portPair);
                netPeer.MessageReceived += ConnectedHandler;
                netPeer.MessageReceived += DisconnectedHandler;
                while (!netPeer.IsConnected)
                {
                    Thread.Sleep(10);
                }
                await netPeer.Send(NetCodes.Connect);
            }).Start();
        }

        public void InitializeNetPeer(string adress, PortPair portPair)
        {
            InitializeNetPeer(IPAddress.Parse(adress), portPair);
        }

        private void ConnectedHandler(short message)
        {
            if (message == NetCodes.Connect)
            {
                Debug.WriteLine("Connected");
                netPeer.MessageReceived -= ConnectedHandler;
                this.game = new(netPeer, this);
            }
        }
        private void DisconnectedHandler(short message)
        {
            if(message == NetCodes.Revenge)
            {
                isRevageRequested = true;
                return;
            }
            if (message == NetCodes.Disconnect && isRevageRequested == false)
            {
                netPeer.Close();
                game.FinishTheGame(game.player);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (netPeer != null)
            {
                netPeer.Close();
            }
            Thread.Sleep(100);
            App.Current.Shutdown(0);
            System.Environment.Exit(0);
        }
    }

    public enum ViewType
    {
        Main,
        Loading,
        Game
    }
}
