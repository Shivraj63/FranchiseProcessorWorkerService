using FranchiseProcessorWorkerService.Models;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FranchiseProcessorWorkerService
{
    public class FranchiseProcessingJob : IJob
    {
        private readonly ILogger<FranchiseProcessingJob> _logger;
        private readonly string _logFilePath;

        public FranchiseProcessingJob(ILogger<FranchiseProcessingJob> logger)
        {
            _logger = logger;
            _logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"FailedFranchises_{DateTime.UtcNow:yyyy-MM-dd}.log");
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Job Started: Processing franchises...");

            var franchises = GetFranchises();

            await Parallel.ForEachAsync(franchises, new ParallelOptions { MaxDegreeOfParallelism = 5 }, async (franchise, ct) =>
            {
                try
                {
                    await ProcessFranchise(franchise);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Franchise {franchise.Name} failed: {ex.Message}");
                    await File.AppendAllTextAsync(_logFilePath, JsonSerializer.Serialize(franchise) + Environment.NewLine);
                }
            });

            _logger.LogInformation("Job Completed.");
        }

        private List<Franchise> GetFranchises() => new()
    {
        new Franchise { Id = 1, Name = "Franchise A" },
        new Franchise { Id = 2, Name = "Franchise B" },
        new Franchise { Id = 3, Name = "Franchise C" },
        new Franchise { Id = 4, Name = "Franchise D" }
    };

        private async Task ProcessFranchise(Franchise franchise)
        {
            _logger.LogInformation($"Processing {franchise.Name}...");
            await Task.Delay(Random.Shared.Next(1000, 3000)); // Simulate work
            if (Random.Shared.Next(1, 5) == 3) throw new Exception("Random failure occurred.");
            _logger.LogInformation($"{franchise.Name} processed successfully.");
        }
    }
}
