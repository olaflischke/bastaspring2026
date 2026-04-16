// ================================
// Lamar: Code-Generierung Beispiel
// ================================

using Lamar;
using System;

namespace LamarBeispiele
{
    public interface IClock
    {
        DateTime Now();
    }

    public class SystemClock : IClock
    {
        public DateTime Now() => DateTime.Now;
    }

    public interface IReportService
    {
        void CreateDailyReport();
    }

    public class ReportService : IReportService
    {
        private readonly IClock _clock;
        private readonly ILogger _logger;

        public ReportService(IClock clock, ILogger logger)
        {
            _clock = clock;
            _logger = logger;
        }

        public void CreateDailyReport()
        {
            var now = _clock.Now();
            _logger.Log($"Creating report for {now:yyyy-MM-dd HH:mm:ss}");
        }
    }

    public class LamarCodeGenerationBeispiel
    {
        public static void Demo()
        {
            var container = new Container(_ =>
            {
                _.For<IClock>().Use<SystemClock>().Singleton();
                _.For<ILogger>().Use<ConsoleLogger>().Singleton();
                _.For<IReportService>().Use<ReportService>().Transient();
            });

            var service = container.GetInstance<IReportService>();
            service.CreateDailyReport();
        }
    }
}

// Im Hintergrund würde Lamar eine Factory-Methode generieren, 
// die ungefähr so aussieht:

// public IReportService BuildReportService()
// {
//     var clock = ResolveSingleton<SystemClock>();
//     var logger = ResolveSingleton<ConsoleLogger>();
//     return new ReportService(clock, logger);
// }
