using Avalonia.Controls;
using MusicPlayer.Models;
using MusicPlayer.ViewModels;

namespace MusicPlayer.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel(this, new Track());
        }
    }
}
