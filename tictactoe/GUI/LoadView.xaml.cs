using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace tictactoe.GUI
{
    /// <summary>
    /// Логика взаимодействия для LoadView.xaml
    /// </summary>
    public partial class LoadView : UserControl
    {
        MainWindow parent;
        public LoadView(MainWindow parentWindow)
        {
            parent = parentWindow;
            InitializeComponent();
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            parent.netPeer.Close();
            parent.LoadView(parent.StartView);
        }
    }
}
