using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace LogikOverlay
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer eshaanTimer = new();
        private readonly DispatcherTimer lyciaTimer = new();
        private readonly DispatcherTimer roganTimer = new();
        private readonly DispatcherTimer vemetTimer = new();

        private DateTime eshaanEnd;
        private DateTime lyciaEnd;
        private DateTime roganEnd;
        private DateTime vemetEnd;
        public MainWindow()
        {
            InitializeComponent();

            SetupTimer(eshaanTimer, () => UpdateDisplay("Eshaan", eshaanEnd, Eshaan));
            SetupTimer(lyciaTimer, () => UpdateDisplay("Lycia", lyciaEnd, Lycia));
            SetupTimer(roganTimer, () => UpdateDisplay("Rogan", roganEnd, Rogan));
            SetupTimer(vemetTimer, () => UpdateDisplay("Vemet", vemetEnd, Vemet));
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void SetupTimer(DispatcherTimer timer, Action tickAction)
        {
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) => tickAction();
        }

        private void UpdateDisplay(string system, DateTime end, Button sender)
        {
            var remaining = end - DateTime.Now;
            if (remaining <= TimeSpan.Zero)
            {
                sender.Content = $"{system}: READY";
            }
            else
            {
                sender.Content = $"{system}: {remaining:mm\\:ss} remaining";
            }
        }

        private void StartEshaan(object sender, RoutedEventArgs e)
        {
            eshaanEnd = DateTime.Now.AddMinutes(30);
            eshaanTimer.Start();
            UpdateDisplay("Eshaan", eshaanEnd, Eshaan);
        }

        private void StartLycia(object sender, RoutedEventArgs e)
        {
            lyciaEnd = DateTime.Now.AddMinutes(30);
            lyciaTimer.Start();
            UpdateDisplay("Lycia", lyciaEnd, Lycia);
        }

        private void StartRogan(object sender, RoutedEventArgs e)
        {
            roganEnd = DateTime.Now.AddMinutes(30);
            roganTimer.Start();
            UpdateDisplay("Rogan", roganEnd, Rogan);
        }

        private void StartVemet(object sender, RoutedEventArgs e)
        {
            vemetEnd = DateTime.Now.AddMinutes(30);
            vemetTimer.Start();
            UpdateDisplay("Vemet", vemetEnd, Vemet);
        }
    }
}