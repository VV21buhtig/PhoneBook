using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using PhoneBook.Data;
using PhoneBook.Models;
using PhoneBook.Services;
using DbContact = PhoneBook.Data.Contact;      // псевдоним для сущности БД

namespace PhoneBook.ViewModels
{
    /// <summary>
    /// ViewModel экрана списка контактов.
    /// Загружает контакты из базы данных при создании.
    /// Зарегистрирован как Transient: новый экземпляр при каждом переходе.
    /// </summary>
    public class ContactsListViewModel : ObservableObject
    {
        public ObservableCollection<Models.Contact> Contacts { get; }
        private readonly INavigationService _navigation;
        private readonly IDialogService _dialogService;
        private readonly PhoneBookContext _context;

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

        private Models.Contact? _selectedContact;
        public Models.Contact? SelectedContact
        {
            get => _selectedContact;
            set => Set(ref _selectedContact, value);
        }

        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand EditCommand { get; }

        public ContactsListViewModel(INavigationService navigation, IDialogService dialogService, PhoneBookContext context)
        {
            _navigation = navigation;
            _dialogService = dialogService;
            _context = context;

            // Загрузка контактов из БД при запуске (только чтение)
            var dbContacts = _context.Contacts.ToList();
            Contacts = new ObservableCollection<Models.Contact>(
                dbContacts.Select(c => new Models.Contact(c.Name, c.Phone)));

            AddCommand = new RelayCommand(AddContact);
            DeleteCommand = new RelayCommand<Models.Contact?>(DeleteContact);
            EditCommand = new RelayCommand<Models.Contact?>(EditContact);
        }

        private void AddContact()
        {
            if (Contacts.Any(c => c.Phone == Phone))
            {
                _dialogService.ShowWarning("Контакт с таким номером уже существует!", "Дубликат");
                return;
            }

            var contact = new Models.Contact(Name, Phone);
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

        private void DeleteContact(Models.Contact? contact)
        {
            if (contact == null) return;
            if (!_dialogService.ShowConfirmation($"Удалить контакт '{contact.Name}'?", "Подтверждение"))
                return;

            Contacts.Remove(contact);
            _dialogService.ShowInfo("Контакт удалён", "Успех");
        }

        private void EditContact(Models.Contact? contact)
        {
            if (contact == null) return;
            _navigation.NavigateTo<ContactEditViewModel>(contact);
        }
    }
}