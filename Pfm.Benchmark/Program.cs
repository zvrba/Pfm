﻿using System;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace Pfm.Benchmark;

// NOTE: Tree code gets heavy gains from PGO: set DOTNET_TieredPGO=1
public static class Program
{
    public static void Main() {
        System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
        var summaries = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run();
        Console.WriteLine("Done.");
    }

    internal interface IIntValueTraits : Pfm.Collections.TreeSet.IValueTraits<int>
    {
        static void Pfm.Collections.TreeSet.IValueTraits<int>.CombineValues(in int left, ref int middle, in int right) => middle = left;
        static int Pfm.Collections.TreeSet.IValueTraits<int>.CompareKey(in int left, in int right) => left - right;
    }

    internal struct IntAvlTree : IIntValueTraits, Pfm.Collections.TreeSet.IAvlTree<IntAvlTree, int>
    {
    }

    internal struct IntWBTree : IIntValueTraits, Pfm.Collections.TreeSet.IWBTree<IntWBTree, int>
    {
    }

}

#if false
internal class CustomConfig : ManualConfig
{
    public CustomConfig() {
        AddJob(Job.Default);
        AddLogger(BenchmarkDotNet.Loggers.ConsoleLogger.Default);

        AddExporter(new CsvExporter(
            CsvSeparator.CurrentCulture,
            new SummaryStyle(
                cultureInfo: System.Globalization.CultureInfo.InvariantCulture,
                printUnitsInHeader: true,
                printUnitsInContent: false,
                timeUnit: Perfolizer.Horology.TimeUnit.Millisecond,
                sizeUnit: BenchmarkDotNet.Columns.SizeUnit.KB)));
        AddHardwareCounters(HardwareCounter.TotalIssues, HardwareCounter.CacheMisses, HardwareCounter.BranchInstructions, HardwareCounter.BranchMispredictions);
    }
}
#endif
