using Godot;

namespace Haze;

/// <summary>
/// Tabler Icons.
/// </summary>
public static partial class TablerIcons
{
	internal static readonly Dictionary<(string, float), WeakReference<SvgTexture>> Textures = [];

	/// <summary>
	/// Clears the texture cache.
	/// </summary>
	public static void ClearCache() => Textures.Clear();
}
