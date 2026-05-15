using MetricsOOP.Core;
using NUnit;
using MediatR;

var metricsOOPAnalyzer = MetricsOOPUtilityService.CreateWithContext("FluentValidation");

var nocDistribution = metricsOOPAnalyzer.CalculateNumberOfChildren();
foreach (var kv in nocDistribution)
{
    Console.WriteLine($"Number of children: {kv.Key}, Number of classes: {kv.Value.Count} - {string.Join(", ", kv.Value.Select(c => c.Name))}");
}

Console.WriteLine();

var doi = metricsOOPAnalyzer.CalculateMaxDepthOfInheritance();

Console.WriteLine($"Max depth of inheritance: {doi.MaxDepth}");
Console.WriteLine($"Depth chain: {string.Join(" <- ", doi.DepthChain.Select(c => c.Name))}");

Console.WriteLine();

var mhf = metricsOOPAnalyzer.CalculateMethodHidingFactor();
Console.WriteLine($"Method Hiding Factor: {mhf:0.####}");

Console.WriteLine();

var ahf = metricsOOPAnalyzer.CalculateAttributeHidingFactor();
Console.WriteLine($"Attribute Hiding Factor: {ahf:0.####}");

Console.WriteLine();

var mif = metricsOOPAnalyzer.CalculateMethodInheritanceFactor();
Console.WriteLine($"Method Inheritance Factor: {mif:0.####}");

Console.WriteLine();

var aif = metricsOOPAnalyzer.CalculateAttributeInheritanceFactor();
Console.WriteLine($"Attribute Inheritance Factor: {aif:0.####}");

Console.WriteLine();

var pof = metricsOOPAnalyzer.CalculatePolymorphismObjectFactor();
Console.WriteLine($"Polymorphism Object Factor: {pof:0.####}");