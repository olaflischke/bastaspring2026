// =========================================
// 04-Lamar.cs
// Lamar DI-Container Beispiele
// =========================================

using Lamar;
using Microsoft.AspNetCore.Builder;
using Lamar.Microsoft.DependencyInjection;
using System;
using System.Reflection;

namespace LamarBeispiele
{
    // ========================================
    // Basis-Registrierung mit Registry-DSL
    // ========================================
    
    public class LamarBasisBeispiel
    {
        public static void BasisRegistrierung()
        {
            var container = new Container(reg =>
            {
                // Registry-DSL im StructureMap-Stil
                reg.For<IEmailService>()
                 .Use<EmailService>()
                 .Singleton();
                
                reg.For<IOrderService>()
                 .Use<OrderService>()
                 .Scoped(); // Entspricht InstancePerScope
                
                reg.For<ILogService>()
                 .Use<LogService>()
                 .Transient();
            });
            
            // Service auflösen
            var orderService = container.GetInstance<IOrderService>();
            orderService.CreateOrder(new Order { Id = 1 });
        }
    }
    
    
    // ========================================
    // ASP.NET Core Integration
    // ========================================
    
    public class LamarAspNetCoreBeispiel
    {
        public static void AspNetCoreIntegration()
        {
            var builder = WebApplication.CreateBuilder();
            
            // Standard MS DI Registrierungen funktionieren weiterhin
            builder.Services.AddControllers();
            builder.Services.AddScoped<IEmailService, EmailService>();
            
            // Lamar als ServiceProvider einsetzen
            // Ersetzt MS DI komplett, behält aber Kompatibilität
            builder.Host.UseLamar();
            
            var app = builder.Build();
            app.MapControllers();
            app.Run();
        }
        
        // Mit zusätzlichen Lamar-Registrierungen
        public static void AspNetCoreWithLamarConfig()
        {
            var builder = WebApplication.CreateBuilder();
            
            builder.Services.AddControllers();
            
            // Lamar mit eigener Konfiguration
            builder.Host.UseLamar((context, registry) =>
            {
                // Lamar-spezifische Registry verwenden
                registry.For<IEmailService>().Use<EmailService>().Singleton();
                registry.For<IOrderService>().Use<OrderService>();
                
                // Policies und Scanning
                registry.Scan(s =>
                {
                    s.Assembly(Assembly.GetExecutingAssembly());
                    s.WithDefaultConventions();
                });
            });
            
            var app = builder.Build();
            app.Run();
        }
    }
    
    
    // ========================================
    // Registry und konventionsbasiertes Scanning
    // ========================================
    
    public class AppRegistry : ServiceRegistry
    {
        public AppRegistry()
        {
            // Konventionsbasiertes Scanning
            Scan(s =>
            {
                s.Assembly(Assembly.GetExecutingAssembly());
                
                // Standard-Konventionen:
                // - IFoo -> Foo
                // - Registriert automatisch passende Implementierungen
                s.WithDefaultConventions();
                
                // Alle Implementierungen von IRepository finden
                s.AddAllTypesOf<IRepository>();
                
                // Nur bestimmte Typen
                s.AssembliesAndExecutablesFromApplicationBaseDirectory(
                    assembly => assembly.FullName.StartsWith("MyApp"));
            });
            
            // Explizite Registrierungen
            For<ILogger>().Use<ConsoleLogger>().Singleton();
            For<IEmailService>().Use<EmailService>().Singleton();
            
            // Policies: Globale Regeln für Registrierungen
            Policies.SetAllProperties(p => 
                p.OfType<ILogger>() && p.Name == "Logger");
        }
    }
    
    public class LamarRegistryBeispiel
    {
        public static void RegistryVerwendung()
        {
            // Registry als wiederverwendbare Konfiguration
            var container = new Container(new AppRegistry());
            
            var orderService = container.GetInstance<IOrderService>();
            orderService.CreateOrder(new Order { Id = 1 });
        }
    }
    
    
    // ========================================
    // Factory-Funktionen
    // ========================================
    
    public class ConfigurableService
    {
        private readonly string _config;
        
        public ConfigurableService(string config)
        {
            _config = config;
        }
        
        public void PrintConfig()
        {
            Console.WriteLine($"Config: {_config}");
        }
    }
    
    public class LamarFactoryBeispiel
    {
        public static void LambdaFactory()
        {
            var container = new Container(reg =>
            {
                // Lambda-Factory für komplexe Initialisierung
                reg.For<ConfigurableService>().Use(ctx =>
                {
                    var config = Environment.GetEnvironmentVariable("MY_CONFIG") ?? "default";
                    return new ConfigurableService(config);
                });
                
                // Factory mit Context-Zugriff
                reg.For<IEmailService>().Use(ctx =>
                {
                    var logger = ctx.GetInstance<ILogger>();
                    return new EmailServiceWithLogging(logger);
                });
            });
            
            var service = container.GetInstance<ConfigurableService>();
            service.PrintConfig();
        }
    }
    
    
    // ========================================
    // Nested Containers für Isolation
    // ========================================
    
    public class LamarNestedContainerBeispiel
    {
        public static void NestedContainers()
        {
            var rootContainer = new Container(reg =>
            {
                reg.For<IEmailService>().Use<EmailService>().Singleton();
                reg.For<IOrderService>().Use<OrderService>().Transient();
            });
            
            // Nested Container für Request/Scope-Isolation
            using (var nested = rootContainer.GetNestedContainer())
            {
                // Überschreibt Registrierung nur in diesem Scope
                nested.Configure(reg =>
                {
                    reg.For<IOrderService>().Use<PremiumOrderService>();
                });
                
                var orderService = nested.GetInstance<IOrderService>();
                // Gibt PremiumOrderService zurück
            }
            
            // Außerhalb: Standard OrderService
            var standardService = rootContainer.GetInstance<IOrderService>();
        }
    }
    
    
    // ========================================
    // Interface- und Klassen-Definitionen
    // ========================================
    
    public interface IEmailService
    {
        void SendEmail(string to, string subject);
    }
    
    public class EmailService : IEmailService
    {
        public void SendEmail(string to, string subject)
        {
            Console.WriteLine($"Email to {to}: {subject}");
        }
    }
    
    public class EmailServiceWithLogging : IEmailService
    {
        private readonly ILogger _logger;
        
        public EmailServiceWithLogging(ILogger logger)
        {
            _logger = logger;
        }
        
        public void SendEmail(string to, string subject)
        {
            _logger.Log($"Sending email to {to}");
            Console.WriteLine($"Email to {to}: {subject}");
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
            _emailService.SendEmail("customer@example.com", $"Order {order.Id}");
        }
    }
    
    public class PremiumOrderService : IOrderService
    {
        public void CreateOrder(Order order)
        {
            Console.WriteLine($"PREMIUM Order {order.Id} created");
        }
    }
    
    public interface ILogService
    {
        void Log(string message);
    }
    
    public class LogService : ILogService
    {
        public void Log(string message)
        {
            Console.WriteLine($"[LOG] {message}");
        }
    }
    
    public interface ILogger
    {
        void Log(string message);
    }
    
    public class ConsoleLogger : ILogger
    {
        public void Log(string message)
        {
            Console.WriteLine($"[LOGGER] {message}");
        }
    }
    
    public interface IRepository { }
    
    public class Order
    {
        public int Id { get; set; }
    }
}
