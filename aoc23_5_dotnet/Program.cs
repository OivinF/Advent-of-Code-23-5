﻿using System.IO;
using System.Reflection;
using System.Linq;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

//	Read file longo string

var assembly = Assembly.GetExecutingAssembly();
var resourceName = "aoc23_5_dotnet.input.txt";

string text = "";
using (Stream stream = assembly.GetManifestResourceStream(resourceName))
using (StreamReader reader = new StreamReader(stream))
{
	text = reader.ReadToEnd();
}

//	Parse string

long[][] parseTable(long tableIndex)
{
	string table = text.Split(':')[tableIndex];
	string[] rows = table.Split('\r', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
	string[][] dataStrings = rows.Select((x) => x.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToArray()).ToArray();
	dataStrings = dataStrings.Where((x) => x.Length == 3).ToArray();
	long[][] data = dataStrings.Select((x) => x.Select((y) => long.Parse(y)).ToArray()).ToArray();

	data = data.OrderBy(x => x[1]).ToArray();

	/*    for (long i = 0; i < data.Length; i++)
		{
			for (long j = 0; j < data[i].Length; j++)
			{
				Console.Write($"{data[i][j]} ");
			}
			Console.Write("\n");
		}*/

	return data;
}

long[] parseSeeds()
{
	string table = text.Split(':')[1];
	string row = table.Split('\r')[0];
	string[] elements = row.Split(' ', StringSplitOptions.RemoveEmptyEntries);
	long[] result = elements.Select(x => long.Parse(x)).ToArray();

	return result;
}

long[] seeds = parseSeeds();

//	Seed-to-soil map
long[][] seed2soil = parseTable(2);

//	Soil-to-fertilizer map
long[][] soil2fert = parseTable(3);

//	Fertilizer-to-water map
long[][] fert2water = parseTable(4);

//	Water-to-light map
long[][] water2light = parseTable(5);

//	Light-to-temperature map
long[][] light2temp = parseTable(6);

//	Temperature-to-humidity map
long[][] temp2humidity = parseTable(7);

//	Humidity-to-location map
long[][] humidity2location = parseTable(8);


// To remap any seed to any table
long remap(long inputNumber, long[][] remapTable)
{
	for (long i = 0; i < remapTable.Length; i++)
	{
		//	Because the ranges are sorted, if the seed falls below the current range it falls below all ranges
		if (inputNumber <= remapTable[i][1])
		{
			return (inputNumber);
		}

		//	Check if this seed can be remapped with the current range
		if (inputNumber <= remapTable[i][1] + remapTable[i][2])
		{
			return ((inputNumber - remapTable[i][1]) + remapTable[i][0]);
		}
	}
	return (inputNumber);
}

void NaiveSolution()
{
	long lowestLocation = long.MaxValue;
	Stopwatch sw = new Stopwatch();
	sw.Start();

	for (long i = 0; i < seeds.Length; i += 2)
	{
		Console.WriteLine($"Seed: {i} range length {seeds[i + 1]}");

		for (int j = 0; j < seeds[i + 1]; j++)
		{
			long soil = remap(seeds[i] + j, seed2soil);
			long fert = remap(soil, soil2fert);
			long water = remap(fert, fert2water);
			long light = remap(water, water2light);
			long temp = remap(light, light2temp);
			long humidity = remap(temp, temp2humidity);
			long location = remap(humidity, humidity2location);

			if (location < lowestLocation)
			{
				lowestLocation = location;
			}
		}
	}

	sw.Stop();
	Console.WriteLine($"Elapsed ms: {sw.Elapsed.TotalMilliseconds}");
	Console.WriteLine($"Lowest location number: {lowestLocation}");
}

void BakeMaps()
{
	long[][][] maps = [seed2soil, soil2fert, fert2water, water2light, light2temp, temp2humidity, humidity2location];
	string json = JsonSerializer.Serialize(maps);
	File.WriteAllText(@"D:\maps.json", json);
}

async Task ComputeMultiThreaded()
{
	Thread mainThread = Thread.CurrentThread;

	Stopwatch sw = new();
	sw.Start();

	Task<long>[] tasks = new Task<long>[seeds.Length / 2];

	for (int i = 0; i < seeds.Length / 2; i++)
	{
		Console.WriteLine($"Dispatching task {i}");
		int seedIndex = i * 2;
		int rangeIndex = seedIndex + 1;
		tasks[i] = Task.Run(() => CheckSingleSeedRange(seeds[seedIndex], seeds[rangeIndex]));
	}

	Console.WriteLine("Waiting for tasks...");
	long[] taskResults = await Task.WhenAll(tasks);
	Console.WriteLine("Tasks complete!");

	long res = taskResults.Min();

	sw.Stop();
	Console.WriteLine($"Elapsed: {sw.Elapsed}");
	Console.WriteLine($"Lowest location number: {res}");

	long CheckSingleSeedRange(long seed, long range)
	{
		long lowestLocation = long.MaxValue;

		for (int j = 0; j < range; j++)
		{
			long soil = remap(seed + j, seed2soil);
			long fert = remap(soil, soil2fert);
			long water = remap(fert, fert2water);
			long light = remap(water, water2light);
			long temp = remap(light, light2temp);
			long humidity = remap(temp, temp2humidity);
			long location = remap(humidity, humidity2location);

			if (location < lowestLocation)
			{
				lowestLocation = location;
			}
		}

		return lowestLocation;
	}

}

await ComputeMultiThreaded();
