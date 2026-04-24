using System.Windows;
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