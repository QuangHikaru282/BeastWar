using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Antigravity.Editor
{
    [InitializeOnLoad]
    public static class ConsoleLogExporter
    {
        private static readonly string LogFilePath = Path.Combine(Directory.GetCurrentDirectory(), "unity_console.txt");

        static ConsoleLogExporter()
        {
            // Reset log file on Editor startup to keep it clean
            try
            {
                if (File.Exists(LogFilePath))
                {
                    File.Delete(LogFilePath);
                }
                File.WriteAllText(LogFilePath, $"--- Unity Editor Console Log Initiated at {DateTime.Now} ---\n");
            }
            catch (Exception e)
            {
                Debug.LogError($"[ConsoleLogExporter] Failed to initialize log file: {e.Message}");
            }

            // Listen to log messages
            Application.logMessageReceived += OnLogMessageReceived;
            Application.logMessageReceivedThreaded += OnLogMessageReceived;
        }

        private static void OnLogMessageReceived(string logString, string stackTrace, LogType type)
        {
            // We only care about Warning, Error, Exception, and Assert to keep the log file readable and focused on issues
            if (type == LogType.Log) 
                return;

            try
            {
                using (var writer = new StreamWriter(LogFilePath, true))
                {
                    writer.WriteLine($"[{DateTime.Now:HH:mm:ss}] [{type}] {logString}");
                    if (type == LogType.Exception || type == LogType.Error)
                    {
                        if (!string.IsNullOrEmpty(stackTrace))
                        {
                            writer.WriteLine("Stack Trace:");
                            writer.WriteLine(stackTrace);
                        }
                    }
                    writer.WriteLine(new string('-', 50));
                }
            }
            catch
            {
                // Ignore failures to prevent infinite log loops
            }
        }
    }
}
