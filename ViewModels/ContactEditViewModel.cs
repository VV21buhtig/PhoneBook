using System.Windows.Input;
using PhoneBook.Models;
using PhoneBook.Services;

namespace PhoneBook.ViewModels
{
    /// <summary>
    /// ViewModel экрана редактирования контакта.
    /// Реализует INavigationAware для получения контакта при навигации.
    /// Зарегистрирован как Transient.
    /// </summary>
    public class ContactEditViewModel : ObservableObject, INavigationAware
    {
        private readonly INavigationService _navigation;
        private Contact _contact = null!;

        private string _editName = string.Empty;
        public string EditName
        {
            get => _editName;
            set => Set(ref _editName, value);
        }

        private string _editPhone = string.Empty;
        public string EditPhone
        {
            get => _editPhone;
            set => Set(ref _editPhone, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public ContactEditViewModel(INavigationService navigation)
        {
            _navigation = navigation;
            SaveCommand = new RelayCommand(SaveContact);
            CancelCommand = new RelayCommand(CancelEdit);
        }

        /// <summary>
        /// Вызывается автоматически при навигации на этот экран.
        /// Получает контакт из параметра и инициализирует поля редактирования.
        /// </summary>
        public void OnNavigatedTo(object? parameter)
        {
            if (parameter is Contact contact)
            {
                _contact = contact;
                EditName = contact.Name;
                EditPhone = contact.Phone;
            }
        }

        private void SaveContact()
        {
            _contact.Name = EditName;
            _contact.Phone = EditPhone;
            _navigation.NavigateTo<ContactsListViewModel>();
        }

        private void CancelEdit()
        {
            _navigation.NavigateTo<ContactsListViewModel>();
        }
    }
}