# PhoneBook - Документация по проекту

## Содержание
1. [App.xaml](#appxaml)
2. [App.xaml.cs](#appxamlcs)
3. [PhoneBook.csproj](#phonebookcsproj)
4. [Data](#data)
   1. [Contact.cs](#contactcs)
   2. [PhoneBookContext.cs](#phonebookcontextcs)
5. [Models](#models)
   1. [Contact.cs](#contactcs)
6. [Services](#services)
   1. [DialogService.cs](#dialogservicecs)
   2. [IDialogService.cs](#idialogservicecs)
   3. [INavigationAware.cs](#inavigationawarecs)
   4. [INavigationService.cs](#inavigationservicecs)
   5. [NavigationService.cs](#navigationservicecs)
7. [Viewmodels](#viewmodels)
   1. [AboutViewModel.cs](#aboutviewmodelcs)
   2. [ContactEditViewModel.cs](#contacteditviewmodelcs)
   3. [ContactsListViewModel.cs](#contactslistviewmodelcs)
   4. [MainWindowViewModel.cs](#mainwindowviewmodelcs)
   5. [ObservableObject.cs](#observableobjectcs)
   6. [RelayCommand.cs](#relaycommandcs)
8. [Views](#views)
   1. [AboutView.xaml](#aboutviewxaml)
   2. [AboutView.xaml.cs](#aboutviewxamlcs)
   3. [ContactEditView.xaml](#contacteditviewxaml)
   4. [ContactEditView.xaml.cs](#contacteditviewxamlcs)
   5. [ContactsListView.xaml](#contactslistviewxaml)
   6. [ContactsListView.xaml.cs](#contactslistviewxamlcs)
   7. [MainWindow.xaml](#mainwindowxaml)
   8. [MainWindow.xaml.cs](#mainwindowxamlcs)

## FILE 1: App.xaml

<a id='appxaml'></a>

```xml
﻿<Application x:Class="PhoneBook.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:vm="clr-namespace:PhoneBook.ViewModels"
             xmlns:v="clr-namespace:PhoneBook.Views">

    <Application.Resources>
        <!-- DataTemplate: автоматический выбор View по типу ViewModel -->
        <DataTemplate DataType="{x:Type vm:ContactsListViewModel}">
            <v:ContactsListView/>
        </DataTemplate>

        <DataTemplate DataType="{x:Type vm:ContactEditViewModel}">
            <v:ContactEditView/>
        </DataTemplate>

        <DataTemplate DataType="{x:Type vm:AboutViewModel}">
            <v:AboutView/>
        </DataTemplate>
    </Application.Resources>
</Application>
```

---

## FILE 2: App.xaml.cs

<a id='appxamlcs'></a>

```csharp
﻿using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PhoneBook.Data;
using PhoneBook.Services;
using PhoneBook.ViewModels;
using PhoneBook.Views;

namespace PhoneBook
{
    public partial class App : Application
    {
        private IServiceProvider? _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            var services = new ServiceCollection();

            services.AddDbContext<PhoneBookContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<INavigationService, NavigationService>();

            services.AddTransient<ContactsListViewModel>();
            services.AddTransient<ContactEditViewModel>();
            services.AddTransient<AboutViewModel>();

            services.AddSingleton<MainWindowViewModel>();

            services.AddSingleton<MainWindow>(sp =>
            {
                var window = new MainWindow();
                window.DataContext = sp.GetRequiredService<MainWindowViewModel>();
                return window;
            });

            _serviceProvider = services.BuildServiceProvider();
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            (_serviceProvider as IDisposable)?.Dispose();
            base.OnExit(e);
        }
    }
}
```

---

## FILE 3: Contact.cs

<a id='contactcs'></a>

```csharp
﻿using System;
using System.Collections.Generic;

namespace PhoneBook.Data;

public partial class Contact
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Phone { get; set; } = null!;
}
```

---

## FILE 4: PhoneBookContext.cs

<a id='phonebookcontextcs'></a>

```csharp
﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PhoneBook.Data;

public partial class PhoneBookContext : DbContext
{
    public PhoneBookContext()
    {
    }

    public PhoneBookContext(DbContextOptions<PhoneBookContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Contact> Contacts { get; set; }

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//        => optionsBuilder.UseSqlServer("Server=LAPTOP-EU7O01O0\\SQLEXPRESS01;Database=PhoneBookDB_Бобков_2407СА2;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Contact>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Contacts__3214EC072ADD423E");

            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
```

---

## FILE 5: Contact.cs

<a id='contactcs'></a>

```csharp
﻿using PhoneBook.ViewModels;
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
```

---

## FILE 6: PhoneBook.csproj

<a id='phonebookcsproj'></a>

```xml
﻿<Project Sdk="Microsoft.NET.Sdk">
	<PropertyGroup>
		<OutputType>WinExe</OutputType>
		<TargetFramework>net8.0-windows</TargetFramework>
		<Nullable>enable</Nullable>
		<UseWPF>true</UseWPF>
		<ImplicitUsings>enable</ImplicitUsings>
	</PropertyGroup>

	<ItemGroup>
		<PackageReference Include="CommunityToolkit.Mvvm" Version="8.4.0" />
		<PackageReference Include="FontAwesome.Sharp" Version="6.6.0" />
		<PackageReference Include="MaterialDesignColors" Version="5.2.1" />
		<PackageReference Include="MaterialDesignThemes" Version="5.2.1" />
		<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
		<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0">
			<PrivateAssets>all</PrivateAssets>
			<IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
		</PackageReference>
		<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
		<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.0">
			<PrivateAssets>all</PrivateAssets>
			<IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
		</PackageReference>
		<PackageReference Include="Microsoft.Extensions.Configuration" Version="8.0.0" />
		<PackageReference Include="Microsoft.Extensions.Configuration.Binder" Version="8.0.0" />
		<PackageReference Include="Microsoft.Extensions.Configuration.FileExtensions" Version="8.0.0" />
		<PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="8.0.0" />
		<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
		<PackageReference Include="Microsoft.Extensions.Hosting" Version="8.0.0" />
	</ItemGroup>

	<ItemGroup>
	  <Folder Include="Data\" />
		<None Update="appsettings.json">
			<CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
		</None>
	</ItemGroup>
</Project>
```

---

## FILE 7: DialogService.cs

<a id='dialogservicecs'></a>

```csharp
﻿using System.Windows;

namespace PhoneBook.Services
{
    public class DialogService : IDialogService
    {
        public void ShowInfo(string message, string title = "Информация") =>
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);

        public void ShowWarning(string message, string title = "Предупреждение") =>
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);

        public void ShowError(string message, string title = "Ошибка") =>
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);

        public bool ShowConfirmation(string message, string title = "Подтверждение") =>
            MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
    }
}
```

---

## FILE 8: IDialogService.cs

<a id='idialogservicecs'></a>

```csharp
﻿namespace PhoneBook.Services
{
    public interface IDialogService
    {
        void ShowInfo(string message, string title = "Информация");
        void ShowWarning(string message, string title = "Предупреждение");
        void ShowError(string message, string title = "Ошибка");
        bool ShowConfirmation(string message, string title = "Подтверждение");
    }
}
```

---

## FILE 9: INavigationAware.cs

<a id='inavigationawarecs'></a>

```csharp
﻿namespace PhoneBook.Services
{
    /// <summary>
    /// Интерфейс для ViewModel, которые хотят получать данные при навигации.
    /// Реализуется экранами, которым нужны параметры (например, редактирование контакта).
    /// </summary>
    public interface INavigationAware
    {
        /// <summary>
        /// Вызывается сервисом навигации при переходе на этот экран.
        /// </summary>
        /// <param name="parameter">Параметр, переданный при навигации</param>
        void OnNavigatedTo(object? parameter);
    }
}
```

---

## FILE 10: INavigationService.cs

<a id='inavigationservicecs'></a>

```csharp
﻿namespace PhoneBook.Services
{
    /// <summary>
    /// Интерфейс сервиса навигации.
    /// Отвечает за переключение между экранами (ViewModel) в приложении.
    /// Зарегистрирован как Singleton в DI-контейнере.
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// Текущая активная ViewModel, отображаемая в ContentControl.
        /// </summary>
        object? CurrentViewModel { get; }

        /// <summary>
        /// Выполняет навигацию к указанной ViewModel.
        /// Если ViewModel реализует INavigationAware, вызывается OnNavigatedTo.
        /// </summary>
        /// <typeparam name="TViewModel">Тип целевой ViewModel</typeparam>
        /// <param name="parameter">Опциональный параметр для передачи данных</param>
        void NavigateTo<TViewModel>(object? parameter = null) where TViewModel : class;
    }
}
```

---

## FILE 11: NavigationService.cs

<a id='navigationservicecs'></a>

```csharp
﻿using Microsoft.Extensions.DependencyInjection;
using PhoneBook.ViewModels;

namespace PhoneBook.Services
{
    /// <summary>
    /// Реализация сервиса навигации.
    /// Получает ViewModel из DI-контейнера, вызывает OnNavigatedTo если нужно,
    /// и обновляет CurrentViewModel для ContentControl.
    /// Зарегистрирован как Singleton.
    /// </summary>
    public class NavigationService : ObservableObject, INavigationService
    {
        private readonly IServiceProvider _serviceProvider;
        private object? _currentViewModel;

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public object? CurrentViewModel
        {
            get => _currentViewModel;
            private set
            {
                _currentViewModel = value;
                OnPropertyChanged();
            }
        }

        public void NavigateTo<TViewModel>(object? parameter = null) where TViewModel : class
        {
            // 1. Получаем ViewModel из контейнера DI (Transient = новый экземпляр)
            var vm = _serviceProvider.GetRequiredService<TViewModel>();

            // 2. Если ViewModel поддерживает приём параметров — передаём их
            if (vm is INavigationAware navigationAware)
            {
                navigationAware.OnNavigatedTo(parameter);
            }

            // 3. Обновляем CurrentViewModel — ContentControl автоматически перерисует содержимое
            CurrentViewModel = vm;
        }
    }
}
```

---

## FILE 12: AboutViewModel.cs

<a id='aboutviewmodelcs'></a>

```csharp
﻿namespace PhoneBook.ViewModels
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
```

---

## FILE 13: ContactEditViewModel.cs

<a id='contacteditviewmodelcs'></a>

```csharp
﻿using System.Windows.Input;
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
```

---

## FILE 14: ContactsListViewModel.cs

<a id='contactslistviewmodelcs'></a>

```csharp
﻿using System.Collections.ObjectModel;
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
            PhoneBookContext context)
        {
            _navigation = navigation;
            _dialogService = dialogService;
            _context = context;

            LoadContacts();

            AddCommand = new RelayCommand(AddContact);
            DeleteCommand = new RelayCommand<Data.Contact?>(DeleteContact);
            EditCommand = new RelayCommand<Data.Contact?>(EditContact);
        }

        private void LoadContacts()
        {
            var dbContacts = _context.Contacts.ToList();
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

            bool exists = _context.Contacts.Any(c => c.Phone == Phone);
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
                _context.Contacts.Add(newContact);
                _context.SaveChanges();
                Contacts.Add(newContact);
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

            try
            {
                _context.Contacts.Remove(contact);
                _context.SaveChanges();
                Contacts.Remove(contact);
                _dialogService.ShowInfo("Контакт удалён", "Успех");
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
```

---

## FILE 15: MainWindowViewModel.cs

<a id='mainwindowviewmodelcs'></a>

```csharp
﻿using System.Windows.Input;
using PhoneBook.Services;

namespace PhoneBook.ViewModels
{
    /// <summary>
    /// ViewModel оболочки приложения (Shell).
    /// Управляет навигацией между экранами через кнопки меню.
    /// Зарегистрирован как Singleton (одно главное окно на приложение).
    /// </summary>
    public class MainWindowViewModel : ObservableObject
    {
        public INavigationService NavigationService { get; }

        public ICommand ShowContactsCommand { get; }
        public ICommand ShowAboutCommand { get; }

        public MainWindowViewModel(INavigationService navigation)
        {
            NavigationService = navigation;
            ShowContactsCommand = new RelayCommand(() => NavigationService.NavigateTo<ContactsListViewModel>());
            ShowAboutCommand = new RelayCommand(() => NavigationService.NavigateTo<AboutViewModel>());

            // При запуске автоматически открываем экран контактов
            NavigationService.NavigateTo<ContactsListViewModel>();
        }
    }
}
```

---

## FILE 16: ObservableObject.cs

<a id='observableobjectcs'></a>

```csharp
﻿using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PhoneBook.ViewModels
{
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool Set<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
```

---

## FILE 17: RelayCommand.cs

<a id='relaycommandcs'></a>

```csharp
﻿using System.Windows.Input;

namespace PhoneBook.ViewModels
{
    public class RelayCommand(Action execute, Func<bool>? canExecute = null) : ICommand
    {
        private readonly Action _execute = execute;
        private readonly Func<bool>? _canExecute = canExecute;

        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;
        public void Execute(object? parameter) { if (CanExecute(parameter)) _execute.Invoke(); }
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }

    public class RelayCommand<T>(Action<T?> execute, Predicate<T?>? canExecute = null) : ICommand
    {
        private readonly Action<T?> _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        private readonly Predicate<T?>? _canExecute = canExecute;

        public bool CanExecute(object? parameter) => _canExecute?.Invoke((T?)parameter) ?? true;
        public void Execute(object? parameter) { if (CanExecute(parameter)) _execute.Invoke((T?)parameter); }
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
```

---

## FILE 18: AboutView.xaml

<a id='aboutviewxaml'></a>

```xml
﻿<UserControl x:Class="PhoneBook.Views.AboutView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             mc:Ignorable="d"
             d:DesignHeight="300" d:DesignWidth="400">
    <StackPanel Margin="20" HorizontalAlignment="Center" VerticalAlignment="Center">
        <TextBlock Text="{Binding AppName}" FontSize="24" FontWeight="Bold" Margin="0,0,0,10"/>
        <TextBlock Text="{Binding Version}" FontSize="16" Margin="0,0,0,5"/>
        <TextBlock Text="{Binding Author}" FontSize="14" Margin="0,0,0,15"/>
        <TextBlock Text="{Binding Description}" TextWrapping="Wrap" MaxWidth="300"/>
    </StackPanel>
</UserControl>
```

---

## FILE 19: AboutView.xaml.cs

<a id='aboutviewxamlcs'></a>

```csharp
﻿using System.Windows.Controls;

namespace PhoneBook.Views
{
    public partial class AboutView : UserControl
    {
        public AboutView()
        {
            InitializeComponent();
        }
    }
}
```

---

## FILE 20: ContactEditView.xaml

<a id='contacteditviewxaml'></a>

```xml
﻿<UserControl x:Class="PhoneBook.Views.ContactEditView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             mc:Ignorable="d"
             d:DesignHeight="300" d:DesignWidth="400">
    <StackPanel Margin="20">
        <TextBlock Text="Редактирование контакта" FontSize="20" FontWeight="Bold" Margin="0,0,0,20"/>

        <TextBlock Text="Имя:" Margin="0,0,0,5"/>
        <TextBox Text="{Binding EditName, UpdateSourceTrigger=PropertyChanged}" Margin="0,0,0,15"/>

        <TextBlock Text="Телефон:" Margin="0,0,0,5"/>
        <TextBox Text="{Binding EditPhone, UpdateSourceTrigger=PropertyChanged}" Margin="0,0,0,20"/>

        <StackPanel Orientation="Horizontal" HorizontalAlignment="Center">
            <Button Content="Сохранить" Width="100" Margin="0,0,10,0" 
                    Command="{Binding SaveCommand}"/>
            <Button Content="Отмена" Width="100" 
                    Command="{Binding CancelCommand}"/>
        </StackPanel>
    </StackPanel>
</UserControl>
```

---

## FILE 21: ContactEditView.xaml.cs

<a id='contacteditviewxamlcs'></a>

```csharp
﻿using System.Windows.Controls;

namespace PhoneBook.Views
{
    public partial class ContactEditView : UserControl
    {
        public ContactEditView()
        {
            InitializeComponent();
        }
    }
}
```

---

## FILE 22: ContactsListView.xaml

<a id='contactslistviewxaml'></a>

```xml
﻿<UserControl x:Class="PhoneBook.Views.ContactsListView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             mc:Ignorable="d"
             d:DesignHeight="400" d:DesignWidth="600">
    <Grid Margin="10">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>

        <TextBox Grid.Row="0" Margin="0,5" 
                 Text="{Binding Name, UpdateSourceTrigger=PropertyChanged}"
                 ToolTip="Введите имя контакта"/>

        <TextBox Grid.Row="1" Margin="0,5" 
                 Text="{Binding Phone, UpdateSourceTrigger=PropertyChanged}"
                 ToolTip="Введите телефон в формате +7XXXXXXXXXX"/>



        <StackPanel Grid.Row="3" Orientation="Horizontal" Margin="0,10">
            <Button Content="Добавить" Width="100" Margin="0,0,10,0" 
                    Command="{Binding AddCommand}"/>
            <Button Content="Удалить" Width="100" Margin="0,0,10,0"
                    Command="{Binding DeleteCommand}" 
                    CommandParameter="{Binding SelectedContact}"/>
            <Button Content="Редактировать" Width="100"
                    Command="{Binding EditCommand}" 
                    CommandParameter="{Binding SelectedContact}"/>
        </StackPanel>

        <DataGrid Grid.Row="4" AutoGenerateColumns="False" IsReadOnly="True"
                  ItemsSource="{Binding Contacts}" 
                  SelectedItem="{Binding SelectedContact}"
                  MouseDoubleClick="DataGrid_MouseDoubleClick">
            <DataGrid.Columns>
                <DataGridTextColumn Header="Имя" Binding="{Binding Name}" Width="*"/>
                <DataGridTextColumn Header="Телефон" Binding="{Binding Phone}" Width="*"/>
            </DataGrid.Columns>
        </DataGrid>
    </Grid>
</UserControl>
```

---

## FILE 23: ContactsListView.xaml.cs

<a id='contactslistviewxamlcs'></a>

```csharp
﻿using System.Windows.Controls;
using System.Windows.Input;
using PhoneBook.ViewModels;

namespace PhoneBook.Views
{
    public partial class ContactsListView : UserControl
    {
        public ContactsListView()
        {
            InitializeComponent();
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is ContactsListViewModel vm && vm.SelectedContact != null)
            {
                vm.EditCommand.Execute(vm.SelectedContact);
            }
        }
    }
}
```

---

## FILE 24: MainWindow.xaml

<a id='mainwindowxaml'></a>

```xml
﻿<Window x:Class="PhoneBook.Views.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Телефонная книга" Height="450" Width="800">
    <DockPanel>
        <!-- Меню навигации -->
        <StackPanel DockPanel.Dock="Top" Orientation="Horizontal" Background="LightGray" Margin="5">
            <Button Content="Контакты" Command="{Binding ShowContactsCommand}" 
                    Margin="5" Padding="10,2" Width="100"/>
            <Button Content="О программе" Command="{Binding ShowAboutCommand}" 
                    Margin="5" Padding="10,2" Width="100"/>
        </StackPanel>

        <!-- Область контента: автоматически подставляет View по DataTemplate -->
        <ContentControl Content="{Binding NavigationService.CurrentViewModel}"/>
    </DockPanel>
</Window>
```

---

## FILE 25: MainWindow.xaml.cs

<a id='mainwindowxamlcs'></a>

```csharp
﻿using System.Windows;

namespace PhoneBook.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // DataContext устанавливается из App.xaml.cs через DI
        }
    }
}
```

---

