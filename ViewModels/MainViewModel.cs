using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using PhoneBook.Models;
using PhoneBook.Services;

namespace PhoneBook.ViewModels
{
    /// <summary>
    /// ViewModel главного окна.
    /// Зарегистрирован как Transient в IoC-контейнере:
    /// каждый раз создаётся новый экземпляр при навигации.
    /// </summary>
    public class MainViewModel : ObservableObject
    {
        public ObservableCollection<Contact> Contacts { get; }

        // Зависимость от сервиса диалогов — внедряется через конструктор (Constructor Injection)
        private readonly IDialogService _dialogService;

        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }

        private string _phone = string.Empty;
        public string Phone
        {
            get => _phone;
            set => Set(ref _phone, value);
        }

        private Contact? _selectedContact;
        public Contact? SelectedContact
        {
            get => _selectedContact;
            set => Set(ref _selectedContact, value);
        }

        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }

        /// <summary>
        /// Конструктор с внедрением зависимости.
        /// DI-контейнер автоматически передаст реализацию IDialogService.
        /// </summary>
        public MainViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            Contacts = new ObservableCollection<Contact>();

            AddCommand = new RelayCommand(AddContact);
            DeleteCommand = new RelayCommand<Contact?>(DeleteContact);
        }

        private void AddContact()
        {
            // Валидация: проверка на дубликат по номеру телефона
            if (Contacts.Any(c => c.Phone == Phone))
            {
                _dialogService.ShowWarning("Контакт с таким номером уже существует!", "Дубликат");
                return;
            }

            var contact = new Contact(Name, Phone);

            if (!contact.IsValid)
            {
                _dialogService.ShowError(contact.ValidationError ?? "Ошибка валидации", "Некорректные данные");
                return;
            }

            Contacts.Add(contact);
            Name = string.Empty;
            Phone = string.Empty;

            _dialogService.ShowInfo($"Контакт '{contact.Name}' успешно добавлен", "Успех");
        }

        private void DeleteContact(Contact? contact)
        {
            if (contact == null)
                return;

            // Запрос подтверждения перед удалением
            if (!_dialogService.ShowConfirmation($"Удалить контакт '{contact.Name}'?", "Подтверждение"))
            {
                return; // Пользователь отменил удаление
            }

            Contacts.Remove(contact);
            _dialogService.ShowInfo("Контакт удалён", "Успех");
        }
    }
}