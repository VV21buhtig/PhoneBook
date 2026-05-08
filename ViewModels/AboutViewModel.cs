namespace PhoneBook.ViewModels
{
    /// <summary>
    /// ViewModel экрана «О программе».
    /// Простая статическая информация, не требует навигационных параметров.
    /// Зарегистрирован как Transient.
    /// </summary>
    public class AboutViewModel : ObservableObject
    {
        public string AppName => "Телефонная книга MVVM";
        public string Version => "ЛАБ 11 (With Navigation)";
        public string Author => "Выполнил: Бобков М.С группа 2407са2";
        public string Description => "Приложение демонстрирует применение паттерна MVVM с навигацией ViewModel-First.";
    }
}