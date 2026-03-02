using System;
using Autofac;

// So kannst du sehr elegant zusätzliche Funktionalität um Services herumlegen, 
// ohne die eigentliche Implementierung zu verändern – und die Verdrahtung 
// bleibt dank Autofac‑RegisterDecorator auf Konfigurationsebene.

namespace AutofacDecoratorDemo
{
    // Service-Contract
    public interface IOrderService
    {
        void PlaceOrder(int orderId);
    }

    // Konkrete Implementierung (Kernlogik)
    public class OrderService : IOrderService
    {
        public void PlaceOrder(int orderId)
        {
            Console.WriteLine($"[OrderService] Placing order {orderId}...");
            // eigentliche Business-Logik, z.B. DB-Zugriff etc.
        }
    }

    // Decorator für Logging (kann beliebige Cross-Cutting-Concern sein:
    // Logging, Caching, Retry, Validation, Metrics, ...)
    public class LoggingOrderServiceDecorator : IOrderService
    {
        private readonly IOrderService _inner;

        public LoggingOrderServiceDecorator(IOrderService inner)
        {
            _inner = inner;
        }

        public void PlaceOrder(int orderId)
        {
            Console.WriteLine($"[LOG] Before PlaceOrder({orderId})");

            // Aufruf des dekorierten Services
            _inner.PlaceOrder(orderId);

            Console.WriteLine($"[LOG] After PlaceOrder({orderId})");
        }
    }

    class Program
    {
        static void Main()
        {
            var builder = new ContainerBuilder();

            // 1. Konkrete Implementierung registrieren
            builder.RegisterType<OrderService>()
                   .As<IOrderService>();

            // 2. Decorator registrieren
            //    Autofac wickelt alle IOrderService-Implementierungen
            //    automatisch in LoggingOrderServiceDecorator ein.
            builder.RegisterDecorator<LoggingOrderServiceDecorator, IOrderService>();

            var container = builder.Build();

            var svc = container.Resolve<IOrderService>();
            svc.PlaceOrder(42);

            Console.ReadLine();
        }
    }
}
