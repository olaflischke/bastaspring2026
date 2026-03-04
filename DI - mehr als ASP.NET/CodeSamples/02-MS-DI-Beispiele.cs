// =========================================
// 02-Microsoft-DI.cs
// Microsoft.Extensions.DependencyInjection
// - Konsole / Library
// - ASP.NET Core
// - WPF
// - .NET MAUI
// =========================================

// using System;
// using Microsoft.Extensions.DependencyInjection;

namespace MicrosoftDI
{
    // ========================================
    // Basis-Registrierung (Konsole / Library)
    // ========================================

    public class MicrosoftDIBeispiele
    {
        public static void BasisRegistrierung()
        {
            var services = new ServiceCollection();

            // Transient: Neue Instanz bei jedem Resolve
            services.AddTransient<IEmailService, EmailService>();

            // Scoped: Eine Instanz pro Scope
            services.AddScoped<IOrderService, OrderService>();

            // Singleton: Eine Instanz für gesamte App
            services.AddSingleton<IConfigService, ConfigService>();

            var serviceProvider = services.BuildServiceProvider();

            // Services auflösen
            using (var scope = serviceProvider.CreateScope())
            {
                var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
                orderService.CreateOrder(new Order { Id = 1 });
            }
        }
    }

    // ========================================
    // Keyed Services (ab .NET 8)
    // ========================================

    public class KeyedServicesBeispiel
    {
        public static void Configure()
        {
            var services = new ServiceCollection();

            // Verschiedene Implementierungen mit Schlüsseln
            services.AddKeyedSingleton<INotificationService, EmailNotificationService>("email");
            services.AddKeyedSingleton<INotificationService, SmsNotificationService>("sms");
            services.AddKeyedSingleton<INotificationService, PushNotificationService>("push");

            var serviceProvider = services.BuildServiceProvider();

            // Service nach Schlüssel auflösen
            var emailService = serviceProvider.GetRequiredKeyedService<INotificationService>("email");
            var smsService = serviceProvider.GetRequiredKeyedService<INotificationService>("sms");

            emailService.Send("Welcome via Email");
            smsService.Send("Welcome via SMS");
        }
    }

    // Consumer mit Keyed Injection (z.B. in ASP.NET Core)
    public class NotificationManager
    {
        private readonly INotificationService _emailService;
        private readonly INotificationService _smsService;

        public NotificationManager(
            [FromKeyedServices("email")] INotificationService emailService,
            [FromKeyedServices("sms")] INotificationService smsService)
        {
            _emailService = emailService;
            _smsService = smsService;
        }

        public void SendAll(string message)
        {
            _emailService.Send(message);
            _smsService.Send(message);
        }
    }

    // ========================================
    // Service Validation (ab .NET 8)
    // ========================================

    // Wann ist Validierung sinnvoll?
    // Wenn:
    // Du eine Konsolen-App oder Library baust (nicht ASP.NET Core, dort ist es default)
    // Du früh Fehler fangen möchtest statt zur Runtime
    // Du eine komplexe DI-Konfiguration hast
    // Beachte:
    // ValidateOnBuild = true kann das Startup verlangsamen (alle Services werden validiert)
    // In ASP.NET Core ist dies bereits aktiviert by default
    // Fehler beim BuildServiceProvider() → App startet gar nicht (gewünscht!)

    public class ServiceValidationBeispiel
    {
        public static void Configure()
        {
            var services = new ServiceCollection();

            services.AddTransient<IEmailService, EmailService>();
            services.AddScoped<IOrderService, OrderService>();

            // Validation aktivieren (findet z.B. zirkuläre Abhängigkeiten)
            var serviceProvider = services.BuildServiceProvider(
                new ServiceProviderOptions
                {
                    ValidateOnBuild = true,
                    ValidateScopes = true
                });
        }
    }

    // ====================================================================
    // ASP.NET Core Beispiel 
    // ====================================================================

    // using Microsoft.AspNetCore.Builder;
    // using Microsoft.Extensions.Hosting;

    namespace MicrosoftDI.AspNetCoreExample
    {
        public class StartupLikeExample
        {
            public static void ConfigureWebApplication(string[] args)
            {
                var builder = WebApplication.CreateBuilder(args);

                // Service-Registrierung
                builder.Services.AddTransient<MicrosoftDI.IEmailService, MicrosoftDI.EmailService>();
                builder.Services.AddScoped<MicrosoftDI.IOrderService, MicrosoftDI.OrderService>();
                builder.Services.AddSingleton<MicrosoftDI.IConfigService, MicrosoftDI.ConfigService>();

                builder.Services.AddControllers();

                var app = builder.Build();

                app.MapControllers();

                app.Run();
            }
        }
    }

    // ====================================================================
    // WPF + Generic Host + DI
    // ====================================================================

    // using Microsoft.Extensions.Hosting;
    // using System.Windows;

    namespace MicrosoftDI.WpfExample
    {
        // Einstiegspunkt (statt App.xaml StartupUri)
        internal class Program
        {
            [STAThread]
            public static void Main(string[] args)
            {
                var host = Host.CreateDefaultBuilder(args)
                    .ConfigureServices(services =>
                    {
                        // WPF App und MainWindow über DI bereitstellen
                        services.AddSingleton<App>();
                        services.AddSingleton<MainWindow>();

                        // Fachliche Services wiederverwenden
                        services.AddSingleton<MicrosoftDI.IEmailService, MicrosoftDI.EmailService>();
                        services.AddSingleton<MicrosoftDI.IOrderService, MicrosoftDI.OrderService>();
                    })
                    .Build();

                // ServiceProvider als Singleton registrieren
                // (vereinfacht den Zugriff auf Services in WPF, z.B. in XAML)
                var serviceProvider = host.Services;
                services.AddSingleton(serviceProvider);

                // App aus dem Container holen und starten
                var app = host.Services.GetRequiredService<App>();
                app.Run();
            }
        }

        // WPF App, bekommt MainWindow via DI
        // StartupUri in XAML entfällt, da wir die App manuell starten!
        public class App : Application
        {
            private readonly MainWindow _mainWindow;

            public App(MainWindow mainWindow)
            {
                _mainWindow = mainWindow;
            }

            protected override void OnStartup(StartupEventArgs e)
            {
                base.OnStartup(e);
                _mainWindow.Show();
            }
        }

        // Einfaches MainWindow, das einen Service injiziert bekommt
        public partial class MainWindow : Window
        {
            private readonly MicrosoftDI.IOrderService _orderService;

            public MainWindow(MicrosoftDI.IOrderService orderService)
            {
                _orderService = orderService;
                InitializeComponent();

                Loaded += (sender, args) =>
                {
                    _orderService.CreateOrder(new MicrosoftDI.Order { Id = 42 });
                };
            }
        }
    }

    // ====================================================================
    // .NET MAUI + DI
    // ====================================================================

    // using Microsoft.Maui;
    // using Microsoft.Maui.Controls;
    // using Microsoft.Maui.Hosting;

    namespace MicrosoftDI.MauiExample
    {
        public static class MauiProgram
        {
            public static MauiApp CreateMauiApp()
            {
                var builder = MauiApp.CreateBuilder();

                builder
                    .UseMauiApp<App>()
                    .ConfigureFonts(fonts =>
                    {
                        fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    });

                // DI-Registrierungen
                builder.Services.AddSingleton<MicrosoftDI.IEmailService, MicrosoftDI.EmailService>();
                builder.Services.AddSingleton<MicrosoftDI.IOrderService, MicrosoftDI.OrderService>();

                // Pages
                builder.Services.AddTransient<MainPage>();

                return builder.Build();
            }
        }

        public partial class App : Application
        {
            public App(MainPage mainPage)
            {
                InitializeComponent();
                MainPage = new NavigationPage(mainPage);
            }
        }

        public partial class MainPage : ContentPage
        {
            private readonly MicrosoftDI.IOrderService _orderService;

            // DI per Konstruktor
            public MainPage(MicrosoftDI.IOrderService orderService)
            {
                _orderService = orderService;

                Title = "DI mit .NET MAUI";

                Content = new VerticalStackLayout
                {
                    Padding = 24,
                    Children =
                {
                    new Label { Text = "Hello DI!", FontSize = 24 },
                    new Button
                    {
                        Text = "Bestellung anlegen",
                        Command = new Command(() =>
                        {
                            _orderService.CreateOrder(new MicrosoftDI.Order { Id = 1 });
                        })
                    }
                }
                };
            }
        }
    }


    // ========================================
    // Interfaces und Basisklassen
    // ========================================

    public interface IEmailService
    {
        void SendEmail(string to, string subject, string body);
    }

    public class EmailService : IEmailService
    {
        public void SendEmail(string to, string subject, string body)
        {
            Console.WriteLine($"Email sent to {to}: {subject}");
        }
    }

    public interface IOrderService
    {
        void CreateOrder(Order order);
    }

    public class OrderService : IOrderService
    {
        private readonly IEmailService _emailService;

        public OrderService(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public void CreateOrder(Order order)
        {
            Console.WriteLine($"Order {order.Id} created");
            _emailService.SendEmail("customer@example.com",
                "Order Confirmation",
                $"Order {order.Id} created successfully.");
        }
    }

    public interface IConfigService
    {
        string GetConnectionString();
    }

    public class ConfigService : IConfigService
    {
        public string GetConnectionString()
        {
            return "Server=localhost;Database=MyDb;";
        }
    }

    public interface INotificationService
    {
        void Send(string message);
    }

    public class EmailNotificationService : INotificationService
    {
        public void Send(string message) => Console.WriteLine($"Email: {message}");
    }

    public class SmsNotificationService : INotificationService
    {
        public void Send(string message) => Console.WriteLine($"SMS: {message}");
    }

    public class PushNotificationService : INotificationService
    {
        public void Send(string message) => Console.WriteLine($"Push: {message}");
    }

    public class Order
    {
        public int Id { get; set; }
    }
}