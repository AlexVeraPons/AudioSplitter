using System.Windows;
using AudioSplitter.ViewModels;

namespace AudioSplitter;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }

}