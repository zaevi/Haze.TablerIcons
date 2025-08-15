# Haze.TablerIcons

[![NuGet](https://img.shields.io/nuget/v/Haze.TablerIcons.svg)](https://www.nuget.org/packages/Haze.TablerIcons/)
[![License](https://img.shields.io/github/license/tabler/tabler-icons.svg)](https://github.com/tabler/tabler-icons/blob/master/LICENSE)

A Godot library that provides direct access to [Tabler Icons](https://github.com/tabler/tabler-icons) as embedded SVG resources.

>  Since `SvgTexture` was introduced in Godot 4.5, this library can only be used with Godot 4.5 or later.

## Usage

```csharp
using Haze;

// Access icons by category and name
var icon = TablerIcons.Design.LayoutGridAdd;

// Get or create a cached texture (recommended for performance)
Texture2D texture1 = icon.GetTexture();

// Implicit conversion support (uses the same cached instance as above)
Texture2D texture2 = icon;

// Create a new texture instance
Texture2D texture3 = icon.CreateTexture();

// Use with custom scale
SvgTexture scaledTexture = icon.GetTexture(2.0f);
```

## License

MIT
