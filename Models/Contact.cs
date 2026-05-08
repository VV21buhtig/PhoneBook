using PhoneBook.ViewModels;
using System.Text.RegularExpressions;

namespace PhoneBook.Models
{
    public class Contact : ObservableObject
    {
        private string _name = string.Empty;
        private string _phone = string.Empty;

        public Contact(string name, string phone)
        {
            Name = name;
            Phone = phone;
            Validate();
        }

        public string Name
        {
            get => _name;
            set { if (Set(ref _name, value)) Validate(); }
        }

        public string Phone
        {
            get => _phone;
            set { if (Set(ref _phone, value)) Validate(); }
        }

        public bool IsValid { get; private set; } = true;
        public string? ValidationError { get; private set; }

        private void Validate()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                IsValid = false;
                ValidationError = "Имя не может быть пустым";
            }
            else if (!Regex.IsMatch(Phone, @"^(\+7|8)?\d{10}$"))
            {
                IsValid = false;
                ValidationError = "Телефон должен быть в формате +7XXXXXXXXXX или 8XXXXXXXXXX";
            }
            else
            {
                IsValid = true;
                ValidationError = null;
            }
        }
    }
}