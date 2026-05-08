using System.Windows.Input;
using PhoneBook.Services;

namespace PhoneBook.ViewModels
{
    /// <summary>
    /// ViewModel оболочки приложения (Shell).
    /// Управляет навигацией между экранами через кнопки меню.
    /// Зарегистрирован как Singleton (одно главное окно на приложение).
    /// </summary>
    public class MainWindowViewModel : ObservableObject
    {
        public INavigationService NavigationService { get; }

        public ICommand ShowContactsCommand { get; }
        public ICommand ShowAboutCommand { get; }

        public MainWindowViewModel(INavigationService navigation)
        {
            NavigationService = navigation;
            ShowContactsCommand = new RelayCommand(() => NavigationService.NavigateTo<ContactsListViewModel>());
            ShowAboutCommand = new RelayCommand(() => NavigationService.NavigateTo<AboutViewModel>());

            // При запуске автоматически открываем экран контактов
            NavigationService.NavigateTo<ContactsListViewModel>();
        }
    }
}