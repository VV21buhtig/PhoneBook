namespace PhoneBook.Services
{
    /// <summary>
    /// Интерфейс сервиса диалоговых окон.
    /// Абстрагирует взаимодействие с пользователем от ViewModel,
    /// что позволяет тестировать логику без запуска UI.
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// Показать информационное сообщение.
        /// </summary>
        void ShowInfo(string message, string title = "Информация");

        /// <summary>
        /// Показать предупреждение.
        /// </summary>
        void ShowWarning(string message, string title = "Предупреждение");

        /// <summary>
        /// Показать сообщение об ошибке.
        /// </summary>
        void ShowError(string message, string title = "Ошибка");

        /// <summary>
        /// Запросить подтверждение действия (Да/Нет).
        /// Возвращает true, если пользователь нажал "Да".
        /// </summary>
        bool ShowConfirmation(string message, string title = "Подтверждение");
    }
}