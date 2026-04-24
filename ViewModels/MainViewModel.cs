using System.Collections.ObjectModel;
using System.Windows.Input;
using PhoneBook.Models;

namespace PhoneBook.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        public ObservableCollection<Contact> Contacts { get; }

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

        private string _inputError = string.Empty;
        public string InputError
        {
            get => _inputError;
            set => Set(ref _inputError, value);
        }

        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }

        public MainViewModel()
        {
            Contacts = new ObservableCollection<Contact>();
            AddCommand = new RelayCommand(AddContact);
            DeleteCommand = new RelayCommand<Contact?>(DeleteContact);
        }

        private void AddContact()
        {
            var contact = new Contact(Name, Phone);

            if (!contact.IsValid)
            {
                InputError = contact.ValidationError;
                return;
            }

            InputError = string.Empty;
            Contacts.Add(contact);
            Name = string.Empty;
            Phone = string.Empty;
        }

        private void DeleteContact(Contact? contact)
        {
            if (contact != null && Contacts.Contains(contact))
            {
                Contacts.Remove(contact);
            }
        }
    }
}