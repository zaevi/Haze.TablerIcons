using System.Reflection;
using Godot;

namespace Haze;


/// <summary>
/// Tabler Icons.
/// </summary>
public static partial class TablerIcons
{
	internal static readonly Dictionary<(string, float), WeakReference<SvgTexture>> Textures = [];


	/// <summary>
	/// Gets the TTF font file as a stream.
	/// </summary>
	/// <returns>A stream containing the Tabler Icons TTF font data.</returns>
	public static Stream OpenFont()
	{
		var assembly = Assembly.GetExecutingAssembly();
		var resourceName = "Haze.TablerIcons.Resources.tabler-icons.ttf";
		var stream = assembly.GetManifestResourceStream(resourceName);
		if (stream == null)
		{
			throw new FileNotFoundException($"Embedded resource '{resourceName}' not found.");
		}

		return stream;
	}


	/// <summary>
	/// Clears the texture cache.
	/// </summary>
	public static void ClearCache() => Textures.Clear();
}
