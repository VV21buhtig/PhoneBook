namespace PhoneBook.Services
{
    /// <summary>
    /// Интерфейс сервиса навигации.
    /// Отвечает за переключение между экранами (ViewModel) в приложении.
    /// Зарегистрирован как Singleton в DI-контейнере.
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// Текущая активная ViewModel, отображаемая в ContentControl.
        /// </summary>
        object? CurrentViewModel { get; }

        /// <summary>
        /// Выполняет навигацию к указанной ViewModel.
        /// Если ViewModel реализует INavigationAware, вызывается OnNavigatedTo.
        /// </summary>
        /// <typeparam name="TViewModel">Тип целевой ViewModel</typeparam>
        /// <param name="parameter">Опциональный параметр для передачи данных</param>
        void NavigateTo<TViewModel>(object? parameter = null) where TViewModel : class;
    }
}