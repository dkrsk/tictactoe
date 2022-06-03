using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Net;
using System.Text.RegularExpressions;

using tictactoe.Network;

namespace tictactoe.GUI
{
    /// <summary>
    /// Логика взаимодействия для MainMenu.xaml
    /// </summary>
    public partial class MainView : UserControl
    {
        MainWindow parent;
        IPAddress? iPAddress;

        const int MaxPort = 65535;

        public MainView(MainWindow parentWindow)
        {
            this.parent = parentWindow;
            InitializeComponent();
        }

        private async void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            await Dispatcher.Invoke(async () => await Connect());
        }

        public async Task Connect()
        {
            PortPair ppair = new PortPair();

            if (int.TryParse(txbLPort.Text, out int lport) && lport <= MaxPort)
            {
                ppair.LPort = lport;
            }
            else
            {
                MessageBox.Show("Invalid local port");
                return;
            }

            if (int.TryParse(txbRPort.Text, out int rport) && rport <= MaxPort)
            {
                ppair.RPort = rport;
            }
            else
            {
                MessageBox.Show("Invalid remote port");
                return;
            }

            txbIPa.Text = txbIPa.Text.Trim();

            if (IPAddress.TryParse(txbIPa.Text, out iPAddress)) //127.0.0.1
            {
                Debug.WriteLine($" IP: {iPAddress}");
                await parent.LoadViewAsync(parent.LoadingView);
                parent.InitializeNetPeer(iPAddress, ppair);
            }
        }

        private async void txbPort_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Return)
            {
                await Connect();
                return;
            }
        }

        private void txbIP_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex reg = new(@"[0-9]");
            e.Handled = !(reg.IsMatch(e.Text) || e.Text == ".");
        }

        private void txbPort_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex reg = new(@"[0-9]");
            e.Handled = !(reg.IsMatch(e.Text));
        }
    }
}
