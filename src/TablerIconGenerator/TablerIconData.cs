using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Godot;

public class TablerIconData(string Name, string Svg, string? Category, string[]? Tags, string? Version, int Unicode, string FieldName, bool filled)
{
	public string Name { get; set; } = Name;
	public string Svg { get; set; } = Svg;
	public string? Category { get; set; } = Category;
	public string[]? Tags { get; set; } = Tags;
	public string? Version { get; set; } = Version;
	public int Unicode { get; set; } = Unicode;
	public string FieldName { get; set; } = FieldName;

	public bool Filled { get; set; } = filled;

	public static TablerIconData Parse(string path)
	{
		var name = Path.GetFileNameWithoutExtension(path);
		bool filled = false;
		if (name == "icons")
		{
			name += "-icon";
		}
		if (Path.GetDirectoryName(path)?.EndsWith("filled") == true)
		{
			// name += "-filled";
			filled = true;
		}

		var text = File.ReadAllText(path);

		// 提取注释部分的属性
		string? category = null;
		string[]? tags = null;
		string? version = null;
		int unicode = 0;
		string fieldName = name;

		// 查找注释块
		var commentMatch = Regex.Match(text, @"<!--(.*?)-->", RegexOptions.Singleline);
		if (commentMatch.Success)
		{
			var commentContent = commentMatch.Groups[1].Value;

			// 解析 category
			var categoryMatch = Regex.Match(commentContent, @"category:\s*(.+)", RegexOptions.IgnoreCase);
			if (categoryMatch.Success)
			{
				category = categoryMatch.Groups[1].Value.Trim();
			}

			// 解析 tags
			var tagsMatch = Regex.Match(commentContent, @"tags:\s*\[(.*?)\]", RegexOptions.IgnoreCase);
			if (tagsMatch.Success)
			{
				var tagsContent = tagsMatch.Groups[1].Value;
				tags = tagsContent.Split(',')
					.Select(tag => tag.Trim().Trim('"', '\''))
					.Where(tag => !string.IsNullOrEmpty(tag))
					.ToArray();
			}

			// 解析 version
			var versionMatch = Regex.Match(commentContent, @"version:\s*[""'](.+?)[""']", RegexOptions.IgnoreCase);
			if (versionMatch.Success)
			{
				version = versionMatch.Groups[1].Value.Trim();
			}

			// 解析 unicode
			var unicodeMatch = Regex.Match(commentContent, @"unicode:\s*[""'](.+?)[""']", RegexOptions.IgnoreCase);
			if (unicodeMatch.Success)
			{
				var unicodeStr = unicodeMatch.Groups[1].Value.Trim();
				if (int.TryParse(unicodeStr, System.Globalization.NumberStyles.HexNumber, null, out int unicodeInt))
				{
					unicode = unicodeInt;
				}
			}
		}

		// 提取 SVG 内容（去掉注释部分）
		var svgContent = Regex.Replace(text, @"<!--.*?-->", "", RegexOptions.Singleline).Trim();

		// 去除多余的换行和空格
		svgContent = Regex.Replace(svgContent, @"\s+", " ");

		// 去除 xmlns="http://www.w3.org/2000/svg" 属性
		svgContent = Regex.Replace(svgContent, @"\s*xmlns=""http://www\.w3\.org/2000/svg""\s*", " ");

		// 清理多余的空格
		svgContent = Regex.Replace(svgContent, @"\s+", " ").Trim();

		svgContent = svgContent.Replace("currentColor", "#FFFFFF");

		// 生成字段名
		fieldName = GenerateFieldName(name);

		if (category is { Length: > 0 })
		{
			category = GenerateFieldName(category);
		}
		else
		{
			category = "Other";
		}

		Debug.Assert(unicode != 0);

		return new TablerIconData(name, svgContent, category, tags, version, unicode, fieldName, filled);
	}

	/// <summary>
	/// 将文件名转换为合法的 C# 字段名（大驼峰写法）
	/// </summary>
	/// <param name="fileName">文件名（不包含扩展名）</param>
	/// <returns>合法的 C# 字段名</returns>
	private static string GenerateFieldName(string fileName)
	{
		if (string.IsNullOrEmpty(fileName))
		{
			return "Unknown";
		}

		// 按连字符分割
		var parts = fileName.Split(['-', ' '], StringSplitOptions.RemoveEmptyEntries);

		var result = new System.Text.StringBuilder();

		foreach (var part in parts)
		{
			if (string.IsNullOrEmpty(part))
			{
				continue;
			}

			// 将每个部分转为首字母大写
			var capitalizedPart = CapitalizeFirstLetter(part);
			result.Append(capitalizedPart);
		}

		var fieldName = result.ToString();

		// 如果字段名为空，返回默认值
		if (string.IsNullOrEmpty(fieldName))
		{
			return "Unknown";
		}

		// 如果以数字开头，添加前缀
		if (char.IsDigit(fieldName[0]))
		{
			fieldName = "Icon" + fieldName;
		}

		return fieldName;
	}

	/// <summary>
	/// 将字符串的首字母转为大写，其余字母转为小写
	/// </summary>
	/// <param name="str">输入字符串</param>
	/// <returns>首字母大写的字符串</returns>
	private static string CapitalizeFirstLetter(string str)
	{
		if (string.IsNullOrEmpty(str))
		{
			return str;
		}

		if (str.Length == 1)
		{
			return str.ToUpper();
		}

		return char.ToUpper(str[0]) + str.Substring(1).ToLower();
	}
}