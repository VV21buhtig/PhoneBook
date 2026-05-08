using Microsoft.Extensions.DependencyInjection;
using PhoneBook.ViewModels;

namespace PhoneBook.Services
{
    /// <summary>
    /// Реализация сервиса навигации.
    /// Получает ViewModel из DI-контейнера, вызывает OnNavigatedTo если нужно,
    /// и обновляет CurrentViewModel для ContentControl.
    /// Зарегистрирован как Singleton.
    /// </summary>
    public class NavigationService : ObservableObject, INavigationService
    {
        private readonly IServiceProvider _serviceProvider;
        private object? _currentViewModel;

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public object? CurrentViewModel
        {
            get => _currentViewModel;
            private set
            {
                _currentViewModel = value;
                OnPropertyChanged();
            }
        }

        public void NavigateTo<TViewModel>(object? parameter = null) where TViewModel : class
        {
            // 1. Получаем ViewModel из контейнера DI (Transient = новый экземпляр)
            var vm = _serviceProvider.GetRequiredService<TViewModel>();

            // 2. Если ViewModel поддерживает приём параметров — передаём их
            if (vm is INavigationAware navigationAware)
            {
                navigationAware.OnNavigatedTo(parameter);
            }

            // 3. Обновляем CurrentViewModel — ContentControl автоматически перерисует содержимое
            CurrentViewModel = vm;
        }
    }
}