using System.Reflection;
using Godot;

namespace Haze;


/// <summary>
/// Tabler Icons.
/// </summary>
public static partial class TablerIcons
{
	internal static readonly Dictionary<(string, float), WeakReference<DpiTexture>> Textures = [];

	public static Func<TablerIcon, float, DpiTexture>? GetOrCreateFunc = DefaultGetOrCreate;

	/// <summary>
	/// Default implementation for creating or getting an SVG texture.
	/// </summary>
	public static DpiTexture DefaultGetOrCreate(TablerIcon icon, float scale)
	{
		var key = (icon.Name, scale);
		if (TablerIcons.Textures.TryGetValue(key, out var textureRef) && textureRef.TryGetTarget(out var texture) && GodotObject.IsInstanceValid(texture))
		{
			return texture;
		}

		texture = icon.CreateTexture(scale);
		TablerIcons.Textures[key] = new WeakReference<DpiTexture>(texture);
		return texture;
	}

	/// <summary>
	/// Clears the texture cache.
	/// </summary>
	public static void ClearCache() => Textures.Clear();

	/// <summary>
	/// Gets the TTF font file as a stream.
	/// </summary>
	/// <returns>A stream containing the Tabler Icons TTF font data.</returns>
	public static Stream OpenFont()
	{
		var assembly = System.Reflection.Assembly.GetExecutingAssembly();
		var resourceName = "Haze.TablerIcons.Resources.tabler-icons.ttf";
		var stream = assembly.GetManifestResourceStream(resourceName);
		if (stream == null)
		{
			throw new FileNotFoundException($"Embedded resource '{resourceName}' not found.");
		}

		return stream;
	}

	/// <summary>
	/// Empty icon.
	/// </summary>
	public static TablerIcon Empty => field ??= new TablerIcon("Empty", "  ", "", "", []);
}
