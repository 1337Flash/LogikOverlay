using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace LogikOverlay
{
    public enum SpawnType
    {
        Scouts,         //https://stfc.johnwsiskar.com/federation-romulan-klingon-scouts/
        Augments
    }
    /// <summary>
    /// Interaction logic for LogikOverlay.xaml
    /// </summary>
    public partial class frmLogikOverlay : Window
    {
        private readonly DispatcherTimer _uiTimer = new();

        public OverlayViewModel VM { get; } = new();
        public frmLogikOverlay()
        {
            InitializeComponent();
            DataContext = VM;

            RestoreWindowPosition();

            LocationChanged += (_, __) => SaveWindowPosition();
            Closing += (_, __) => SaveWindowPosition();

            // Example Scouts (remove or change as needed)
            VM.AddSpawnTimer("Eshaan", 30, SpawnType.Scouts);
            VM.AddSpawnTimer("Lycia", 30, SpawnType.Scouts);
            VM.AddSpawnTimer("Rogan", 30, SpawnType.Scouts);
            VM.AddSpawnTimer("Vemet", 30, SpawnType.Scouts);
            VM.AddSpawnTimer("Eshaan", 30, SpawnType.Scouts);
            VM.AddSpawnTimer("Lycia", 30, SpawnType.Scouts);
            VM.AddSpawnTimer("Rogan", 30, SpawnType.Scouts);
            VM.AddSpawnTimer("Vemet", 30, SpawnType.Scouts);
            VM.AddSpawnTimer("Eshaan", 30, SpawnType.Scouts);
            VM.AddSpawnTimer("Lycia", 30, SpawnType.Scouts);
            VM.AddSpawnTimer("Rogan", 30, SpawnType.Scouts);
            VM.AddSpawnTimer("Vemet", 30, SpawnType.Scouts);

            // Example Augments
            VM.AddSpawnTimer("Augment System 1", 45, SpawnType.Augments);
            VM.AddSpawnTimer("Augment System 2", 45, SpawnType.Augments);

            // One timer updates all items
            _uiTimer.Interval = TimeSpan.FromSeconds(1);
            _uiTimer.Tick += (_, __) => VM.TickAll();
            _uiTimer.Start();

            Loaded += (_, __) =>
            {
                // Force the first tab to be selected
                var tc = FindTabControl();
                if (tc != null && tc.Items.Count > 0)
                    tc.SelectedIndex = 0;
            };
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void RestoreWindowPosition()
        {
            var left = Properties.Settings.Default.WindowLeft;
            var top = Properties.Settings.Default.WindowTop;

            // If never saved yet, don't override default placement
            if (double.IsNaN(left) || double.IsNaN(top))
                return;

            // Optional safety: keep it on-screen (at least partially)
            if (!IsOnAnyScreen(left, top, Width, Height))
                return;

            Left = left;
            Top = top;
        }

        private void SaveWindowPosition()
        {
            Properties.Settings.Default.WindowLeft = Left;
            Properties.Settings.Default.WindowTop = Top;
            Properties.Settings.Default.Save();
        }

        // Keeps window from restoring off-screen (multi-monitor changes)
        private static bool IsOnAnyScreen(double left, double top, double width, double height)
        {
            double screenLeft = SystemParameters.VirtualScreenLeft;
            double screenTop = SystemParameters.VirtualScreenTop;
            double screenRight = screenLeft + SystemParameters.VirtualScreenWidth;
            double screenBottom = screenTop + SystemParameters.VirtualScreenHeight;

            // Window rectangle
            double winLeft = left;
            double winTop = top;
            double winRight = left + width;
            double winBottom = top + height;

            bool horizontallyVisible = winRight > screenLeft && winLeft < screenRight;
            bool verticallyVisible = winBottom > screenTop && winTop < screenBottom;

            return horizontallyVisible && verticallyVisible;
        }

        private TabControl? FindTabControl()
        {
            return this.FindName("MainTabControl") as TabControl;
        }
    }

    public sealed class OverlayViewModel : NotifyBase
    {
        // Tabs shown in the UI (one per SpawnType that exists)
        public ObservableCollection<SpawnTypeTab> Tabs { get; } = new();

        // Easy lookup: SpawnType -> Tab
        private readonly Dictionary<SpawnType, SpawnTypeTab> _tabByType = new();

        public void AddSpawnTimer(string nameIn, double respawnTimeMinutesIn, SpawnType spawnTypeIn)
        {
            if (!_tabByType.TryGetValue(spawnTypeIn, out var tab))
            {
                tab = new SpawnTypeTab(spawnTypeIn);
                _tabByType.Add(spawnTypeIn, tab);
                Tabs.Add(tab);
            }

            var item = new SpawnTimerItem(nameIn, TimeSpan.FromMinutes(respawnTimeMinutesIn));
            tab.Items.Add(item);

            // Optional: immediately show initial label
            item.RefreshDisplayText();
        }

        public void TickAll()
        {
            foreach (var tab in Tabs)
                foreach (var item in tab.Items)
                    item.RefreshDisplayText();
        }
    }

    public sealed class SpawnTypeTab : NotifyBase
    {
        public SpawnType Type { get; }
        public ObservableCollection<SpawnTimerItem> Items { get; } = new();

        public SpawnTypeTab(SpawnType type)
        {
            Type = type;
        }
    }

    public sealed class SpawnTimerItem : NotifyBase
    {
        private readonly TimeSpan _respawn;
        private DateTime? _endTime;

        public string Name { get; }

        private string _displayText = "";
        public string DisplayText
        {
            get => _displayText;
            private set { _displayText = value; OnPropertyChanged(); }
        }

        public ICommand StartCommand { get; }

        public SpawnTimerItem(string name, TimeSpan respawn)
        {
            Name = name;
            _respawn = respawn;

            StartCommand = new RelayCommand(_ => Start());
            RefreshDisplayText(); // initial state
        }

        private void Start()
        {
            _endTime = DateTime.Now.Add(_respawn);
            RefreshDisplayText(); // manual "tick" immediately
        }

        public void RefreshDisplayText()
        {
            if (_endTime is null)
            {
                DisplayText = $"{Name} – Start {(int)_respawn.TotalMinutes}m";
                return;
            }

            var remaining = _endTime.Value - DateTime.Now;

            if (remaining <= TimeSpan.Zero)
            {
                DisplayText = $"{Name}: READY";
            }
            else
            {
                DisplayText = $"{Name}: {remaining:mm\\:ss} remaining";
            }
        }
    }

    public sealed class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;

        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

        public void Execute(object? parameter) => _execute(parameter);

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }

    public abstract class NotifyBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
