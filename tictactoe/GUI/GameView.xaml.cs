using System;
using System.Collections.Generic;

using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Diagnostics;


using tictactoe.Game;
using tictactoe.Network;

namespace tictactoe.GUI
{
    /// <summary>
    /// Логика взаимодействия для GameView.xaml
    /// </summary>
    public partial class GameView : UserControl
    {
        MainWindow parent;
        GameLogic? game;
        readonly string[] xImgKeys = { "x1", "x2", "x3", "x4", "x5" };
        readonly string[] oImgKeys = { "h1", "h2", "h3", "h4", "h5" };
        readonly Random rnd = new();

        public Button[] buttonsField;

        public GameView(MainWindow parentWindow)
        {
            this.parent = parentWindow;

            //parent.netPeer.MessageReceived += RemoteClick;

            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.game = parent.game;
            lblInfo.Content = game.player is PlayerX ? "Your turn!" : "Opponent turn!";
            this.buttonsField = new Button[] { this.btnLT, this.btnMT, this.btnRT, this.btnLM, this.btnMM, this.btnRM, this.btnLB, this.btnMB, this.btnRB };
            game.GameEnded += GameEndedHandler;
        }

        private async void btnGame_Click(object sender, RoutedEventArgs e)
        {
            Button srcButton = (Button)e.Source;
            Debug.WriteLine($"GameClick! btn: {srcButton.Tag}");
            await game.GameClick(srcButton);
        }

        private void GameEndedHandler(Player player)
        {
            Dispatcher.Invoke(() =>
            {
                if (player.GetType().Equals(new PlayerDraw().GetType()))
                {
                    lblInfo.Content = "Draw!";
                }
                else
                {
                    lblInfo.Content = $"{(game.player.GetType().Equals(player.GetType()) ? "You" : "Opponent")} won!";
                }
            });
        }

        private async void btnRevenge_Click(object sender, RoutedEventArgs e)
        {
            await game.netPeer.Send(NetCodes.Revenge);
            parent.netPeer.Close();
            await parent.LoadViewAsync(parent.LoadingView);
            await parent.StartView.Connect();
        }

        private void btnQuit_Click(object sender, RoutedEventArgs e)
        {
            parent.netPeer.Close();
            parent.LoadView(parent.StartView);
        }

        public void SetBtnImage(short tag, Player player)
        {
            Dispatcher.Invoke(() =>
            {
                if (player.IsX)
                {
                    buttonsField[tag].Background = App.Current.FindResource(xImgKeys[rnd.Next(0, 4)]) as ImageBrush;
                }
                else
                {
                    buttonsField[tag].Background = App.Current.FindResource(oImgKeys[rnd.Next(0, 4)]) as ImageBrush;
                }
            });
        }        
    }
}
