namespace XBase;

/// <summary>
/// Defines an ammo type.
/// </summary>
[GameResource( "Ammo Type", "ammo", "Ammo", Icon = "📦", IconBgColor = "#f54248" )]
public class AmmoTypeResource : GameResource
{
	/// <summary>
	/// The type of ammo this resource represents
	/// </summary>
	[Property, Group( "Ammo" )]
	public string AmmoType { get; set; }

	/// <summary>
	/// The maximum amount of ammo that can be held
	/// </summary>
	[Property, Group( "Ammo" )]
	public int MaxAmount { get; set; }

	/// <summary>
	/// The icon for this ammo type.
	/// </summary>
	[Property, Group( "Visual" ), IconName]
	public string Icon { get; set; }

	/// <summary>
	/// The icon for this ammo type. (as SVG)
	/// </summary>
	[Property, Group( "Visual" ), TextArea]
	public string SvgIcon { get; set; }
}

