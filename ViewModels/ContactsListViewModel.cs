using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using PhoneBook.Models;
using PhoneBook.Services;

namespace PhoneBook.ViewModels
{
    /// <summary>
    /// ViewModel экрана списка контактов.
    /// Зарегистрирован как Transient: новый экземпляр при каждом переходе.
    /// </summary>
    public class ContactsListViewModel : ObservableObject
    {
        public ObservableCollection<Contact> Contacts { get; }
        private readonly INavigationService _navigation;
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
        public ICommand EditCommand { get; }

        public ContactsListViewModel(INavigationService navigation, IDialogService dialogService)
        {
            _navigation = navigation;
            _dialogService = dialogService;
            Contacts = new ObservableCollection<Contact>();

            AddCommand = new RelayCommand(AddContact);
            DeleteCommand = new RelayCommand<Contact?>(DeleteContact);
            EditCommand = new RelayCommand<Contact?>(EditContact);
        }

        private void AddContact()
        {
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
            if (contact == null) return;
            if (!_dialogService.ShowConfirmation($"Удалить контакт '{contact.Name}'?", "Подтверждение"))
                return;

            Contacts.Remove(contact);
            _dialogService.ShowInfo("Контакт удалён", "Успех");
        }

        private void EditContact(Contact? contact)
        {
            if (contact == null) return;
            // Навигация к экрану редактирования с передачей контакта
            _navigation.NavigateTo<ContactEditViewModel>(contact);
        }
    }
}