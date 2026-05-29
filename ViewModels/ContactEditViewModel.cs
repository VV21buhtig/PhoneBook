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
        private readonly IDbContextFactory<PhoneBookContext> _contextFactory;

        private Data.Contact _contact = null!;
        private int _contactId;
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
            IDbContextFactory<PhoneBookContext> contextFactory)
        {
            _navigation = navigation;
            _dialogService = dialogService;
            _contextFactory = contextFactory;
            SaveCommand = new RelayCommand(SaveContact);
            CancelCommand = new RelayCommand(CancelEdit);
        }

        public void OnNavigatedTo(object? parameter)
        {
            if (parameter is Data.Contact contact)
            {
                _contactId = contact.Id;
                EditName = contact.Name;
                EditPhone = contact.Phone;
            }
        }

        private void SaveContact()
        {
            if (_contactId == 0) return;

            // FETCH: находим сущность в новом контексте
            using var context = _contextFactory.CreateDbContext();
            var contactToUpdate = context.Contacts.Find(_contactId);

            if (contactToUpdate == null)
            {
                _dialogService.ShowError("Контакт не найден в базе данных", "Ошибка");
                return;
            }

            // MODIFY: обновляем свойства
            contactToUpdate.Name = EditName;
            contactToUpdate.Phone = EditPhone;

            // SAVE: сохраняем
            try
            {
                context.SaveChanges();
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