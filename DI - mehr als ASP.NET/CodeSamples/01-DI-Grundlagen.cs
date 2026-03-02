// =========================================
// 01-DI-Grundlagen.cs
// Code-Beispiele zu Dependency Injection
// =========================================

using System;

namespace DIGrundlagen
{
    // ========================================
    // OHNE Dependency Injection (Tight Coupling)
    // ========================================
    
    public class EmailService
    {
        public void SendConfirmation(Order order)
        {
            Console.WriteLine($"Email sent for order {order.Id}");
        }
    }
    
    public class OrderServiceOhneDI
    {
        private readonly EmailService _emailService;
        
        public OrderServiceOhneDI()
        {
            // Problem: Direkte Abhängigkeit!
            // - Nicht testbar (echte Emails werden versendet)
            // - Nicht austauschbar
            // - Fest gekoppelt
            _emailService = new EmailService();
        }
        
        public void CreateOrder(Order order)
        {
            // Business Logic
            Console.WriteLine($"Creating order {order.Id}");
            
            _emailService.SendConfirmation(order);
        }
    }
    
    
    // ========================================
    // MIT Dependency Injection (Loose Coupling)
    // ========================================
    
    public interface IEmailService
    {
        void SendConfirmation(Order order);
    }
    
    public class EmailServiceImpl : IEmailService
    {
        public void SendConfirmation(Order order)
        {
            Console.WriteLine($"Email sent for order {order.Id}");
        }
    }
    
    // Für Tests: Mock-Implementierung
    public class MockEmailService : IEmailService
    {
        public void SendConfirmation(Order order)
        {
            Console.WriteLine($"MOCK: Email would be sent for order {order.Id}");
        }
    }
    
    public class OrderServiceMitDI
    {
        private readonly IEmailService _emailService;
        
        // Dependency wird über Constructor injiziert
        public OrderServiceMitDI(IEmailService emailService)
        {
            _emailService = emailService;
        }
        
        public void CreateOrder(Order order)
        {
            // Business Logic
            Console.WriteLine($"Creating order {order.Id}");
            
            // Aufruf des injizierten Service
            _emailService.SendConfirmation(order);
        }
    }
    
   
    // ========================================
    // Helper-Klassen
    // ========================================
    
    public class Order
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public decimal Amount { get; set; }
    }
}
