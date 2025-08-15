using Godot;

namespace Haze;

/// <summary>
/// Represents a Tabler icon with its metadata and provides methods to convert it to Godot textures.
/// </summary>
public record class TablerIcon(string Name, string Svg, string Category, string[] Tags)
{
	/// <summary>
	/// Gets or creates a cached SvgTexture for this icon with the specified scale.
	/// Uses weak reference caching for performance optimization.
	/// </summary>
	/// <param name="scale">The scale factor for the texture (default: 1.0)</param>
	/// <returns>A cached or new SvgTexture instance</returns>
	public SvgTexture GetTexture(float scale = 1.0f)
	{
		return GetOrCreate(scale);
	}

	/// <summary>
	/// Creates a new SvgTexture instance for this icon with the specified scale.
	/// This method always creates a new instance and doesn't use caching.
	/// </summary>
	/// <param name="scale">The scale factor for the texture (default: 1.0)</param>
	/// <returns>A new SvgTexture instance</returns>
	public SvgTexture CreateTexture(float scale = 1.0f) => SvgTexture.CreateFromString(Svg, scale);

	/// <summary>
	/// Implicitly converts a TablerIcon to an SvgTexture.
	/// </summary>
	public static implicit operator SvgTexture(TablerIcon icon) => icon.GetTexture();

	SvgTexture GetOrCreate(float scale)
	{
		var key = (Name, scale);
		if (TablerIcons.Textures.TryGetValue(key, out var textureRef) && textureRef.TryGetTarget(out var texture) && GodotObject.IsInstanceValid(texture))
		{
			return texture;
		}

		texture = CreateTexture();
		TablerIcons.Textures[key] = new WeakReference<SvgTexture>(texture);
		return texture;
	}
}