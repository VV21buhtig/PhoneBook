# PhoneBook - Документация по проекту

## Содержание
1. [App.xaml](#appxaml)
2. [App.xaml.cs](#appxamlcs)
3. [PhoneBook.csproj](#phonebookcsproj)
4. [Models](#models)
   1. [Contact.cs](#contactcs)
5. [Services](#services)
   1. [DialogService.cs](#dialogservicecs)
   2. [IDialogService.cs](#idialogservicecs)
6. [Viewmodels](#viewmodels)
   1. [MainViewModel.cs](#mainviewmodelcs)
   2. [ObservableObject.cs](#observableobjectcs)
   3. [RelayCommand.cs](#relaycommandcs)
7. [Views](#views)
   1. [MainWindow.xaml](#mainwindowxaml)
   2. [MainWindow.xaml.cs](#mainwindowxamlcs)

## FILE 1: App.xaml

<a id='appxaml'></a>

```xml
﻿<Application x:Class="PhoneBook.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
	<Application.Resources>
	</Application.Resources>
</Application>
```

---

## FILE 2: App.xaml.cs

<a id='appxamlcs'></a>

```csharp
﻿using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using PhoneBook.Services;
using PhoneBook.ViewModels;
using PhoneBook.Views;

namespace PhoneBook
{
    /// <summary>
    /// Точка входа приложения.
    /// Отвечает за настройку Dependency Injection и запуск MainWindow.
    /// </summary>
    public partial class App : Application
    {
        private IServiceProvider? _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1. Создаём коллекцию сервисов
            var services = new ServiceCollection();

            // 2. Регистрируем сервисы с указанием времени жизни (Lifetime)

            // DialogService — Singleton: один экземпляр на всё приложение,
            // так как сервис не хранит состояние пользователя и безопасен для многократного использования.
            services.AddSingleton<IDialogService, DialogService>();

            // MainViewModel — Transient: новый экземпляр при каждом запросе.
            // Это важно для будущей навигации: каждый экран получит свежий ViewModel.
            services.AddTransient<MainViewModel>();

            // MainWindow — Singleton с ручной инъекцией DataContext.
            // Лямбда-выражение позволяет создать окно через стандартный конструктор
            // (чтобы сработал InitializeComponent), но подставить ViewModel из контейнера.
            services.AddSingleton<MainWindow>(sp =>
            {
                var window = new MainWindow();
                window.DataContext = sp.GetRequiredService<MainViewModel>();
                return window;
            });

            // 3. Создаём контейнер (ServiceProvider)
            _serviceProvider = services.BuildServiceProvider();

            // 4. Получаем главное окно и запускаем его
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Освобождаем ресурсы контейнера при завершении приложения
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
﻿using PhoneBook.ViewModels;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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
            set
            {
                if (Set(ref _name, value))
                    Validate();
            }
        }

        public string Phone
        {
            get => _phone;
            set
            {
                if (Set(ref _phone, value))
                    Validate();
            }
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

## FILE 4: PhoneBook.csproj

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
	  <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="10.0.7" />
	</ItemGroup>

</Project>
```

---

## FILE 5: DialogService.cs

<a id='dialogservicecs'></a>

```csharp
﻿using System.Windows;

namespace PhoneBook.Services
{
    /// <summary>
    /// Реализация IDialogService с использованием стандартного MessageBox WPF.
    /// Зарегистрирован как Singleton в IoC-контейнере, так как не хранит состояние.
    /// </summary>
    public class DialogService : IDialogService
    {
        public void ShowInfo(string message, string title = "Информация")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void ShowWarning(string message, string title = "Предупреждение")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public void ShowError(string message, string title = "Ошибка")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public bool ShowConfirmation(string message, string title = "Подтверждение")
        {
            var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
            return result == MessageBoxResult.Yes;
        }
    }
}
```

---

## FILE 6: IDialogService.cs

<a id='idialogservicecs'></a>

```csharp
﻿namespace PhoneBook.Services
{
    /// <summary>
    /// Интерфейс сервиса диалоговых окон.
    /// Абстрагирует взаимодействие с пользователем от ViewModel,
    /// что позволяет тестировать логику без запуска UI.
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// Показать информационное сообщение.
        /// </summary>
        void ShowInfo(string message, string title = "Информация");

        /// <summary>
        /// Показать предупреждение.
        /// </summary>
        void ShowWarning(string message, string title = "Предупреждение");

        /// <summary>
        /// Показать сообщение об ошибке.
        /// </summary>
        void ShowError(string message, string title = "Ошибка");

        /// <summary>
        /// Запросить подтверждение действия (Да/Нет).
        /// Возвращает true, если пользователь нажал "Да".
        /// </summary>
        bool ShowConfirmation(string message, string title = "Подтверждение");
    }
}
```

---

## FILE 7: MainViewModel.cs

<a id='mainviewmodelcs'></a>

```csharp
﻿using System.Collections.ObjectModel;
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
```

---

## FILE 8: ObservableObject.cs

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

## FILE 9: RelayCommand.cs

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

        public void Execute(object? parameter)
        {
            if (CanExecute(parameter))
                _execute.Invoke();
        }

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

        public void Execute(object? parameter)
        {
            if (CanExecute(parameter))
                _execute.Invoke((T?)parameter);
        }

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
```

---

## FILE 10: MainWindow.xaml

<a id='mainwindowxaml'></a>

```xml
﻿<Window x:Class="PhoneBook.Views.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Телефонная книга" Height="450" Width="600">

    <Grid Margin="10">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>

        <TextBox Grid.Row="0" Margin="0,5" 
                 Text="{Binding Name, UpdateSourceTrigger=PropertyChanged}"/>

        <TextBox Grid.Row="1" Margin="0,5" 
                 Text="{Binding Phone, UpdateSourceTrigger=PropertyChanged}"/>

        <TextBlock Grid.Row="2" Foreground="Red" Margin="0,5" 
                   Text="{Binding InputError}" />

        <StackPanel Grid.Row="3" Orientation="Horizontal" Margin="0,10">
            <Button Content="Добавить" Width="100" Margin="0,0,10,0" 
                    Command="{Binding AddCommand}"/>
            <Button Content="Удалить" Width="100" 
                    Command="{Binding DeleteCommand}" 
                    CommandParameter="{Binding SelectedContact}"/>
        </StackPanel>

        <DataGrid Grid.Row="4" AutoGenerateColumns="False" IsReadOnly="True"
                  ItemsSource="{Binding Contacts}" 
                  SelectedItem="{Binding SelectedContact}">
            <DataGrid.Columns>
                <DataGridTextColumn Header="Имя" Binding="{Binding Name}" Width="*"/>
                <DataGridTextColumn Header="Телефон" Binding="{Binding Phone}" Width="*"/>
            </DataGrid.Columns>
        </DataGrid>
    </Grid>
</Window>
```

---

## FILE 11: MainWindow.xaml.cs

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
        }
    }
}
```

---



## Ответы на контрольные вопросы (лабораторная №10)

---

**Вопрос 1. В чём проблема прямого вызова MessageBox.Show() из ViewModel? Как это влияет на тестирование?**

Прямой вызов MessageBox.Show() создаёт жёсткую связь ViewModel с WPF-сборкой PresentationFramework. Это нарушает принцип единственной ответственности —
ViewModel начинает управлять UI, а не только бизнес-логикой.

При юнит-тестировании вызов MessageBox блокирует выполнение теста, так как в тестовом окружении нет графического интерфейса. Тест либо зависнет,
либо упадёт с исключением. Абстрагирование через интерфейс IDialogService позволяет подменить реальную реализацию на mock-объект, который возвращает нужные значения без вызова окон.

---

**Вопрос 2. Что такое Dependency Injection? Назовите способы внедрения. Почему Constructor Injection предпочтительнее?**

Dependency Injection (внедрение зависимостей) — принцип, при котором объект не создаёт свои зависимости сам, а получает их извне.

**Три способа внедрения:**
- **Constructor Injection** — через конструктор (зависимость обязательна)
- **Setter Injection** — через публичное свойство (зависимость опциональна)
- **Method Injection** — через параметр конкретного метода

**Constructor Injection предпочтительнее, потому что:**
- Явно показывает все зависимости класса
- Гарантирует, что объект нельзя создать без необходимых зависимостей
- Упрощает тестирование — зависимости легко передать в конструктор теста
- Объект полностью инициализирован после создания

---

**Вопрос 3. Для чего нужен IoC-контейнер? Объясните роль ServiceCollection и BuildServiceProvider().**

IoC-контейнер — это автоматический "разрешатель" зависимостей. Он создаёт объекты и подставляет им нужные зависимости по зарегистрированным правилам.

**ServiceCollection** — коллекция, куда регистрируются интерфейсы и их реализации с указанием времени жизни (Transient, Singleton). Это "список рецептов" — что и как создавать.

**BuildServiceProvider()** — метод, который создаёт готовый контейнер (ServiceProvider) на основе зарегистрированных сервисов. 
После этого можно вызывать GetRequiredService<T>() и получать готовые объекты со всеми внедрёнными зависимостями.

---

**Вопрос 4. В чём разница между AddTransient и AddSingleton? Почему DialogService — Singleton, а MainViewModel — Transient?**

**Разница:**
- **Transient** — новый экземпляр при каждом запросе
- **Singleton** — один экземпляр на всё приложение

**DialogService — Singleton, потому что:**
- Не хранит состояние пользователя
- Безопасен для многократного использования
- Нет смысла создавать несколько экземпляров

**MainViewModel — Transient, потому что:**
- Хранит состояние конкретного экрана (список контактов, выбранный элемент, поля ввода)
- При навигации каждый экран должен получать свежий экземпляр
- Изоляция состояния предотвращает утечку данных между экранами

---

**Вопрос 5. Почему удалили StartupUri из App.xaml и DataContext из MainWindow.xaml? Какую проблему решает такой подход?**

**Удаление StartupUri** переносит запуск приложения из декларативной разметки в процедурный код. Это позволяет:
- Настроить контейнер DI до создания окна
- Внедрить зависимости в окно и ViewModel
- Контролировать порядок инициализации

**Удаление DataContext из XAML** устраняет жёсткую привязку View к конкретному типу ViewModel. View больше не знает, какая ViewModel её обслуживает — она получает её извне через DI-контейнер.

**Какая проблема решается:** сильная связанность View и ViewModel. Теперь View можно тестировать изолированно, подменять ViewModel
, а сама ViewModel создаётся и конфигурируется централизованно. Это соответствует принципу инверсии управления (IoC) — контроль над созданием объектов передан контейнеру.