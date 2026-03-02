// =========================================
// 03-Autofac.cs
// Autofac DI-Container Beispiele
// =========================================

using Autofac;
using System;
using System.Linq;
using System.Reflection;

namespace AutofacBeispiele
{
    // ========================================
    // Basis-Registrierung mit Fluent API
    // ========================================
    
    public class AutofacBasisBeispiel
    {
        public static void BasisRegistrierung()
        {
            var builder = new ContainerBuilder();
            
            // Fluent API - lesbar und intuitiv
            builder.RegisterType<EmailService>()
                   .As<IEmailService>()
                   .SingleInstance(); // Singleton
            
            builder.RegisterType<OrderService>()
                   .As<IOrderService>()
                   .InstancePerLifetimeScope(); // Scoped
            
            builder.RegisterType<LogService>()
                   .As<ILogService>()
                   .InstancePerDependency(); // Transient
            
            var container = builder.Build();
            
            // Service auflösen
            using var scope = container.BeginLifetimeScope();
            var orderService = scope.Resolve<IOrderService>();
            orderService.CreateOrder(new Order { Id = 1 });
        }
    }
    
    
    // ========================================
    // Assembly-Scanning
    // ========================================
    
    public class AutofacScanningBeispiel
    {
        public static void AssemblyScanning()
        {
            var builder = new ContainerBuilder();
            
            // Alle Repositories automatisch registrieren
            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                   .Where(t => t.Name.EndsWith("Repository"))
                   .AsImplementedInterfaces()
                   .InstancePerLifetimeScope();
            
            // Alle Services mit "Service" im Namen
            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                   .Where(t => t.Name.EndsWith("Service"))
                   .AsImplementedInterfaces()
                   .InstancePerDependency();
            
            var container = builder.Build();
        }
    }
    
    
    // ========================================
    // Module für Wiederverwendbarkeit
    // ========================================
    
    public class DataAccessModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // Datenbankkontext
            builder.RegisterType<MyDbContext>()
                   .AsSelf()
                   .InstancePerLifetimeScope();
            
            // Alle Repositories aus Assembly
            builder.RegisterAssemblyTypes(typeof(IRepository).Assembly)
                   .Where(t => t.Name.EndsWith("Repository"))
                   .AsImplementedInterfaces()
                   .InstancePerLifetimeScope();
        }
    }
    
    public class BusinessLogicModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<EmailService>().As<IEmailService>().SingleInstance();
            builder.RegisterType<OrderService>().As<IOrderService>();
            builder.RegisterType<CustomerService>().As<ICustomerService>();
        }
    }
    
    public class AutofacModuleBeispiel
    {
        public static void ModuleVerwendung()
        {
            var builder = new ContainerBuilder();
            
            // Module registrieren - gruppierte Konfiguration
            builder.RegisterModule<DataAccessModule>();
            builder.RegisterModule<BusinessLogicModule>();
            
            var container = builder.Build();
        }
    }
    
    
    // ========================================
    // Delegate Factories (Func<T>, Owned<T>)
    // ========================================
    
    public class TimeStampService
    {
        public DateTime CreatedAt { get; } = DateTime.Now;
        
        public void PrintTime()
        {
            Console.WriteLine($"Service created at: {CreatedAt:HH:mm:ss.fff}");
        }
    }
    
    public class TimeConsumer
    {
        private readonly Func<TimeStampService> _factory;
        
        // Autofac injiziert automatisch eine Factory-Funktion
        public TimeConsumer(Func<TimeStampService> factory)
        {
            _factory = factory;
        }
        
        public void ShowTimes()
        {
            Console.WriteLine("Creating first instance:");
            var s1 = _factory(); // Neue Instanz
            s1.PrintTime();
            
            System.Threading.Thread.Sleep(100);
            
            Console.WriteLine("Creating second instance:");
            var s2 = _factory(); // Weitere neue Instanz
            s2.PrintTime();
        }
    }
    
    public class AutofacFactoryBeispiel
    {
        public static void DelegateFactory()
        {
            var builder = new ContainerBuilder();
            
            builder.RegisterType<TimeStampService>().InstancePerDependency();
            builder.RegisterType<TimeConsumer>();
            
            var container = builder.Build();
            
            var consumer = container.Resolve<TimeConsumer>();
            consumer.ShowTimes();
            // Output zeigt unterschiedliche Zeitstempel
        }
    }
    
    
    // ========================================
    // Property Injection
    // ========================================
    
    public class ServiceWithPropertyInjection
    {
        // Property wird von Autofac automatisch befüllt
        public ILogService Logger { get; set; }
        
        public void DoWork()
        {
            Logger?.Log("Working...");
        }
    }
    
    public class AutofacPropertyInjectionBeispiel
    {
        public static void PropertyInjection()
        {
            var builder = new ContainerBuilder();
            
            builder.RegisterType<LogService>().As<ILogService>();
            
            // Property Injection aktivieren
            builder.RegisterType<ServiceWithPropertyInjection>()
                   .PropertiesAutowired();
            
            var container = builder.Build();
            
            var service = container.Resolve<ServiceWithPropertyInjection>();
            service.DoWork(); // Logger ist automatisch gesetzt
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
    
    public interface IRepository { }
    
    public interface ICustomerService { }
    
    public class CustomerService : ICustomerService { }
    
    public class MyDbContext { }
    
    public class Order
    {
        public int Id { get; set; }
    }
}
