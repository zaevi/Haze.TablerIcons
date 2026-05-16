using Asteroid.CodeBuilder;

using Godot;

namespace Haze;

public record class TablerIcon(string Name, string Unicode, string Svg, string Category, string[] Tags);
public static class TablerIcons;


public class TablerIconGenerator
{
	const string NAMESPACE = nameof(Haze);

	public void Generate(string iconsFolder, string generatedFolder)
	{
		var iconGroups = ParseIcons(iconsFolder);
		var builders = GenerateFileBuilders(iconGroups);

		Directory.CreateDirectory(generatedFolder);

		foreach (var builder in builders)
		{
			var path = Path.Join(generatedFolder, builder.Name);
			// var path = generatedFolder.PathJoin(builder.Name);
			File.WriteAllText(path, builder.ToCode(default));
		}
	}

	List<FileBuilder> GenerateFileBuilders(Dictionary<string, List<TablerIconData>> iconGroups)
	{
		List<FileBuilder> builders = [];

		ExpNodes.New iconsExp;
		ExpNodes.Switch getSwitchExp;
		{
			var builder = FileBuilder.New("TablerIcons.g.cs", "GENERATED");
			var f = builder.Root;
			f.NullableEnable();
			f.FileNamespace(NAMESPACE);
			var klass = f.Class(nameof(TablerIcons)).AsPublic().AsStatic().AsPartial();
			klass.GetterProperty(typeof(List<TablerIcon>), "Icons")
				.AsPublic()
				.AsStatic()
				.With(Exp.Field.NCAssign(Exp.New(typeof(List<TablerIcon>)).Ref(out iconsExp).AsMultiLine()));

			klass.Method(Exp.Type<TablerIcon>().Nullable(), "Get").AsPublic().AsStatic()
				.Param(Exp.Type<string>().Nullable(), "id")
				.With(Exp.Switch("id").Ref(out getSwitchExp));
			builders.Add(builder);
		}

		foreach (var (category, icons) in iconGroups)
		{
			DeclNodes.ClassBase.Section iconSection;
			DeclNodes.Class iconSourcesClass;

			Setup();

			foreach (var data in icons)
			{
				var categoryFieldName = data.FieldName;

				iconSourcesClass.Field(typeof(string), categoryFieldName)
					.AsPublic().AsStatic().AsReadonly()
					.With(Exp.RawString(data.Svg).AsMultiLine(false));

				ExpNode unicodeExp;
				if (data.Unicode > 0xFFFF)
				{
					TypeName c = typeof(char);
					unicodeExp = c.Acc(nameof(char.ConvertFromUtf32))
						.Invoke([Exp.Literal(data.Unicode).AsHexadecimal()]);
				}
				else
				{
					unicodeExp = Exp.String("\\u" + data.Unicode.ToString("X4"));
				}

				var tagsExp = Exp.Collection();
				foreach (var tag in data.Tags ?? [])
				{
					tagsExp.Add(Exp.String(tag));
				}

				iconSection.GetterProperty(typeof(TablerIcon), categoryFieldName)
					.AsPublic().AsStatic()
					.With(Exp.Field.NCAssign(Exp.New().Arg([
						Exp.String(data.FieldName), unicodeExp, Exp.Acc("Sources", categoryFieldName),
						Exp.String(category), tagsExp
					])));

				iconsExp.Init(categoryFieldName);

				getSwitchExp.Arm(Exp.String(data.FieldName), categoryFieldName);
			}

			void Setup()
			{
				{
					var builder = FileBuilder.New($"TablerIcons_{category}.g.cs", $"GENERATED");
					var f = builder.Root;
					f.FileNamespace(NAMESPACE);
					var iconsClass = f.Class(nameof(TablerIcons)).AsPublic().AsStatic().AsPartial();

					iconSection = iconsClass.Section();
					builders.Add(builder);
				}

				{
					var builder = FileBuilder.New($"TablerIcons_{category}_Sources.g.cs", $"GENERATED");
					var f = builder.Root;
					f.FileNamespace(NAMESPACE);
					var iconsClass = f.Class(nameof(TablerIcons)).AsPublic().AsStatic().AsPartial();
					iconSourcesClass = iconsClass.Class("Sources").AsPublic().AsStatic().AsPartial();
					builders.Add(builder);
				}
			}
		}

		getSwitchExp.Arm(Exp.Discard, Exp.Null);

		return builders;
	}

	Dictionary<string, List<TablerIconData>> ParseIcons(string iconsFolder)
	{
		Dictionary<string, TablerIconData> map = [];

		var outlineFolder = Path.Join(iconsFolder, "outline");
		foreach (var file in Directory.GetFiles(outlineFolder, "*.svg", SearchOption.AllDirectories))
		{
			if (TablerIconData.Parse(file) is { } iconData)
			{
				map[iconData.FieldName] = iconData;
			}
		}

		var filledFolder = Path.Join(iconsFolder, "filled");
		foreach (var file in Directory.GetFiles(filledFolder, "*.svg", SearchOption.AllDirectories))
		{
			if (TablerIconData.Parse(file) is not { } iconData)
			{
				continue;
			}

			if (map.TryGetValue(iconData.FieldName, out var outlineData))
			{
				iconData.Category = outlineData.Category;
				iconData.Tags = outlineData.Tags;
			}

			iconData.FieldName += "Filled";

			map[iconData.FieldName] = iconData;
		}

		Dictionary<string, List<TablerIconData>> ret = [];
		foreach (var data in map.Values.OrderBy(d => d.FieldName))
		{
			var category = data.Category ?? "Uncategorized";
			if (!ret.TryGetValue(category, out var list))
			{
				list = [];
				ret[category] = list;
			}

			list.Add(data);
		}

		return ret;
	}
}