using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using PhoneBook.Data;
using PhoneBook.Services;

namespace PhoneBook.ViewModels
{
    public class ContactsListViewModel : ObservableObject
    {
        public ObservableCollection<Data.Contact> Contacts { get; private set; }

        private readonly INavigationService _navigation;
        private readonly IDialogService _dialogService;
        private readonly IDbContextFactory<PhoneBookContext> _contextFactory;

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

        private Data.Contact? _selectedContact;
        public Data.Contact? SelectedContact
        {
            get => _selectedContact;
            set => Set(ref _selectedContact, value);
        }

        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand EditCommand { get; }

        public ContactsListViewModel(
          INavigationService navigation,
          IDialogService dialogService,
          IDbContextFactory<PhoneBookContext> contextFactory)
        {
            _navigation = navigation;
            _dialogService = dialogService;
            _contextFactory = contextFactory;
            LoadContacts();
        }

        private void LoadContacts()
        {
            using var context = _contextFactory.CreateDbContext();
            var dbContacts = context.Contacts.ToList();
            Contacts = new ObservableCollection<Data.Contact>(dbContacts);
            OnPropertyChanged(nameof(Contacts));
        }

        private void AddContact()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                _dialogService.ShowWarning("Введите имя контакта", "Ошибка");
                return;
            }
            if (string.IsNullOrWhiteSpace(Phone))
            {
                _dialogService.ShowWarning("Введите номер телефона", "Ошибка");
                return;
            }

            using var context = _contextFactory.CreateDbContext();

            bool exists = context.Contacts.Any(c => c.Phone == Phone);
            if (exists)
            {
                _dialogService.ShowWarning("Контакт с таким номером уже существует!", "Дубликат");
                return;
            }

            var newContact = new Data.Contact
            {
                Name = Name,
                Phone = Phone
            };

            try
            {
                context.Contacts.Add(newContact);
                context.SaveChanges();
                LoadContacts();  // reload
                Name = string.Empty;
                Phone = string.Empty;
                _dialogService.ShowInfo("Контакт успешно добавлен", "Успех");
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Ошибка при сохранении: {ex.Message}", "Ошибка БД");
            }
        }

        private void DeleteContact(Data.Contact? contact)
        {
            if (contact == null) return;

            if (!_dialogService.ShowConfirmation($"Удалить контакт '{contact.Name}'?", "Подтверждение"))
                return;

            using var context = _contextFactory.CreateDbContext();

            try
            {
                var contactToDelete = context.Contacts.Find(contact.Id);
                if (contactToDelete != null)
                {
                    context.Contacts.Remove(contactToDelete);
                    context.SaveChanges();
                    LoadContacts();  // перезагружаем список
                    _dialogService.ShowInfo("Контакт удалён", "Успех");
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Ошибка при удалении: {ex.Message}", "Ошибка БД");
            }
        }

        private void EditContact(Data.Contact? contact)
        {
            if (contact == null) return;
            _navigation.NavigateTo<ContactEditViewModel>(contact);
        }
    }
}