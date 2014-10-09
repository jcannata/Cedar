﻿namespace Cedar.Testing.TestRunner
{
    using System;
    using Cedar.Testing.Execution;
    using PowerArgs;

    class Program
    {
        static void Main(string[] args)
        {
            var options = Args.Parse<TestRunnerOptions>(args);

            if (options.Help || String.IsNullOrEmpty(options.Assembly))
            {
                Console.WriteLine(ArgUsage.GetUsage<TestRunnerOptions>(options: new ArgUsageOptions
                {
                    ShowType = false,
                    ShowPosition = false,
                    ShowPossibleValues = false,
                    AppendDefaultValueToDescription = true
                }));
                return;
            }

            new ScenarioRunner(options.Assembly, options.Teamcity, options.Output, options.Formatters)
                .Run()
                .Wait();
        }
    }
}
