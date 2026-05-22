namespace PhoneBook.ViewModels
{
    /// <summary>
    /// ViewModel экрана «О программе».
    /// Простая статическая информация, не требует навигационных параметров.
    /// Зарегистрирован как Transient.
    /// </summary>
    public class AboutViewModel : ObservableObject
    {
        public string AppName => "Телефонная книга - PhoneBook";
        public string Version => "ЛАБ 13 (.NET. CRUD)";
        public string Author => "Выполнил: Бобков М.С группа 2407СА2";
        public string Description => "Entity Framework Core";
    }
}