using System;
using System.IO;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using System.Runtime.Versioning;

class Program
{
    static void Main()
    {
        Console.WriteLine("Starting monitoring application...");

        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        bool loggingEnabled = configuration["Logging:Enabled"] != null && bool.Parse(configuration["Logging:Enabled"] ?? "false");
        string logFilePath = configuration["Logging:LogFilePath"] ?? "log.txt";
        int memoryThresholdMB = configuration["Thresholds:MemoryThresholdMB"] != null ? int.Parse(configuration["Thresholds:MemoryThresholdMB"] ?? "100") : 100;
        int cpuThresholdPercent = configuration["Thresholds:CpuThresholdPercent"] != null ? int.Parse(configuration["Thresholds:CpuThresholdPercent"] ?? "80") : 80;

        if (loggingEnabled && !string.IsNullOrEmpty(logFilePath))
        {
            LogToFile(logFilePath, $"Monitoring started at {DateTime.Now}");
            Console.WriteLine($"Logging to file {logFilePath}");
        }

        // Monitor system parameters
        MonitorSystem(memoryThresholdMB, cpuThresholdPercent, logFilePath);
    }

    [SupportedOSPlatform("windows")]
    static void MonitorSystem(int memoryThresholdMB, int cpuThresholdPercent, string logFilePath)
    {
        PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        PerformanceCounter memCounter = new PerformanceCounter("Memory", "Available MBytes");

        Console.WriteLine("Monitoring system parameters...");

        while (true)
        {
            float cpuUsage = cpuCounter.NextValue();
            float availableMemory = memCounter.NextValue();

            Console.WriteLine($"Current CPU Usage: {cpuUsage}%");
            Console.WriteLine($"Available Memory: {availableMemory}MB");

            if (cpuUsage > cpuThresholdPercent)
            {
                LogToFile(logFilePath, $"CPU usage exceeded {cpuThresholdPercent}%: {cpuUsage}%");
                Console.WriteLine($"CPU usage exceeded {cpuThresholdPercent}%: {cpuUsage}%");
            }

            if (availableMemory < memoryThresholdMB)
            {
                LogToFile(logFilePath, $"Available memory below {memoryThresholdMB}MB: {availableMemory}MB");
                Console.WriteLine($"Available memory below {memoryThresholdMB}MB: {availableMemory}MB");
            }

            System.Threading.Thread.Sleep(1000); // Sleep for 1 second
        }
    }

    static void LogToFile(string filePath, string message)
    {
        if (!string.IsNullOrEmpty(filePath))
        {
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine($"{DateTime.Now}: {message}");
            }
        }
    }
}
