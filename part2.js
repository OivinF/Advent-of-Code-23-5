import * as fs from 'node:fs/promises';

const text = await fs.readFile("./input.txt", { encoding: 'utf8' });

function isNumber(char) {
	return !!char.match(/\d/);
}

function parseTable(table) {
	return text
		.split(":")[table]
		.split("\r")
		.map((x) => x.slice(1, x.length))
		.map((x) => x.split(" "))
		.filter((x) => x.length === 3)
		.map((x) => x.map((y) => Number(y)));
}

//	Seeds
const seeds = text
	.split(":")[1]
	.split("\r")[0]
	.split(" ")
	.filter((x) => x !== "")
	.map((x) => Number(x));

//	Seed-to-soil map
const seed2soil = parseTable(2);

//	Soil-to-fertilizer map
const soil2fert = parseTable(3);

//	Fertilizer-to-water map
const fert2water = parseTable(4);

//	Water-to-light map
const water2light = parseTable(5);

//	Light-to-temperature map
const light2temp = parseTable(6);

//	Temperature-to-humidity map
const temp2humidity = parseTable(7);

//	Humidity-to-location map
const humidity2location = parseTable(8);

function logInput() {
	console.log("Seeds:");
	console.log(seeds);

	console.log("Seed-to-soil map:");
	console.log(seed2soil);

	console.log("Soil-to-fertilizer map:");
	console.log(soil2fert);

	console.log("Fertilizer-to-water map:");
	console.log(fert2water);

	console.log("Water-to-light map:");
	console.log(water2light);

	console.log("Light-to-temperature map:");
	console.log(light2temp);

	console.log("Temperature-to-humidity map:");
	console.log(temp2humidity);

	console.log("Humidity-to-location map:");
	console.log(humidity2location);
}


let lowestLocation = Number.MAX_SAFE_INTEGER;

//	This works but is painfully slow because of the nested loop
for (let i = 0; i < seeds.length; i += 2) {
	console.log(`Seed: ${i} range length ${seeds[i +1]}`);
	for (let j = 0; j < seeds[i + 1]; j++) {
		const soil = remap(seeds[i] + j, seed2soil);
		const fert = remap(soil, soil2fert);
		const water = remap(fert, fert2water);
		const light = remap(water, water2light);
		const temp = remap(light, light2temp);
		const humidity = remap(temp, temp2humidity);
		const location = remap(humidity, humidity2location);

		if (location < lowestLocation) {
			lowestLocation = location;
		}
	}
	
}

console.log(`Lowest location number: ${lowestLocation}`);

function remap(inputNumber, remapTable) {
	for (let i = 0; i < remapTable.length; i++) {
		//	Check if this seed can be remapped with the current range
		if (inputNumber >= remapTable[i][1] && inputNumber <= remapTable[i][1] + remapTable[i][2]) {
			return ((inputNumber - remapTable[i][1]) + remapTable[i][0]);
		}
	}
	return (inputNumber);
}