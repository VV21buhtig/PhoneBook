using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using PhoneBook.Data;
using PhoneBook.Services;

namespace PhoneBook.ViewModels
{
    public class ContactEditViewModel : ObservableObject, INavigationAware
    {
        private readonly INavigationService _navigation;
        private readonly IDialogService _dialogService;
        private readonly PhoneBookContext _context;

        private Data.Contact _contact = null!;

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

        public ContactEditViewModel(
            INavigationService navigation,
            IDialogService dialogService,
            PhoneBookContext context)
        {
            _navigation = navigation;
            _dialogService = dialogService;
            _context = context;

            SaveCommand = new RelayCommand(SaveContact);
            CancelCommand = new RelayCommand(CancelEdit);
        }

        public void OnNavigatedTo(object? parameter)
        {
            if (parameter is Data.Contact contact)
            {
                _contact = contact;
                EditName = contact.Name;
                EditPhone = contact.Phone;
            }
        }

        private void SaveContact()
        {
            if (_contact == null) return;

            _contact.Name = EditName;
            _contact.Phone = EditPhone;

            try
            {
                _context.Entry(_contact).State = EntityState.Modified;
                _context.SaveChanges();
                _dialogService.ShowInfo("Контакт сохранён", "Успех");
                _navigation.NavigateTo<ContactsListViewModel>();
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Ошибка при сохранении: {ex.Message}", "Ошибка БД");
            }
        }

        private void CancelEdit()
        {
            _navigation.NavigateTo<ContactsListViewModel>();
        }
    }
}