using Haze;

if (args.Length < 2)
{
	Console.Error.WriteLine("Usage: TablerIconGenerator <icons-folder> <generated-folder>");
	return 1;
}

string iconsFolder = args[0];
string generatedFolder = args[1];

if (!Directory.Exists(iconsFolder))
{
	Console.Error.WriteLine($"Error: Icons folder not found: {iconsFolder}");
	return 1;
}

var generator = new TablerIconGenerator();
generator.Generate(iconsFolder, generatedFolder);

Console.WriteLine("Done");
return 0;
