using Light.GuardClauses;

namespace WpfClient
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public MainWindow(MainWindowViewModel viewModel) : this()
        {
            DataContext = viewModel.MustNotBeNull(nameof(viewModel));
        }
    }
}