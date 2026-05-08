using System.Windows;

namespace PhoneBook.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // DataContext устанавливается из App.xaml.cs через DI
        }
    }
}