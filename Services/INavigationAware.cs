namespace PhoneBook.Services
{
    /// <summary>
    /// Интерфейс для ViewModel, которые хотят получать данные при навигации.
    /// Реализуется экранами, которым нужны параметры (например, редактирование контакта).
    /// </summary>
    public interface INavigationAware
    {
        /// <summary>
        /// Вызывается сервисом навигации при переходе на этот экран.
        /// </summary>
        /// <param name="parameter">Параметр, переданный при навигации</param>
        void OnNavigatedTo(object? parameter);
    }
}