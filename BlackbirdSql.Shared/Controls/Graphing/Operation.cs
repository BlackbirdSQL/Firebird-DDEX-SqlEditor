// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.Operation
using System;
using System.Drawing;
using BlackbirdSql.Shared.Properties;

namespace BlackbirdSql.Shared.Controls.Graphing;

public sealed class Operation
{
	private readonly string name;

	private readonly string displayNameKey;

	private readonly string descriptionKey;

	private readonly string imageName;

	private readonly string helpKeyword;

	private readonly Type displayNodeType;

	private Image image;

	private string displayName;

	private string description;

	private static readonly Color transparentColor = Color.FromArgb(219, 64, 140);

	private static readonly Operation unknown = new Operation("", "Unknown", "UnknownDescription", "Result_32x.ico", ".swb.showplan.result.f1");

	public string Name => name;

	public string DisplayName
	{
		get
		{
			if (displayName == null && displayNameKey != null)
			{
				displayName = ControlsResources.ResourceManager.GetString(displayNameKey);
			}
			if (displayName == null)
			{
				return name;
			}
			return displayName;
		}
	}

	public string Description
	{
		get
		{
			if (description == null && descriptionKey != null)
			{
				description = ControlsResources.ResourceManager.GetString(descriptionKey);
			}
			return description;
		}
	}

	public Image Image
	{
		get
		{
			if (image == null && imageName != null)
			{
				int scaledImageSize = ControlUtils.GetScaledImageSize(32);
				if (imageName.ToLowerInvariant().EndsWith(".bmp", StringComparison.Ordinal))
				{
					Bitmap bitmap = new Bitmap(new Bitmap(typeof(Operation), imageName), scaledImageSize, scaledImageSize);
					bitmap.MakeTransparent(transparentColor);
					image = bitmap;
				}
				else if (imageName.ToLowerInvariant().EndsWith(".ico", StringComparison.Ordinal))
				{
					Icon icon = new Icon(new Icon(typeof(Operation), imageName), scaledImageSize, scaledImageSize);
					image = icon.ToBitmap();
				}
			}
			return image;
		}
	}

	public string HelpKeyword => helpKeyword;

	public Type DisplayNodeType => displayNodeType;

	public static Operation Unknown => unknown;

	public Operation(string name, string displayNameKey)
		: this(name, displayNameKey, null, null, null)
	{
	}

	public Operation(string name, string displayNameKey, string descriptionKey, string imageName, string helpKeyword)
		: this(name, displayNameKey, descriptionKey, imageName, helpKeyword, typeof(NodeDisplay))
	{
	}

	public Operation(string name, string displayNameKey, string descriptionKey, string imageName, string helpKeyword, Type displayNodeType)
	{
		this.name = name;
		this.displayNameKey = displayNameKey;
		this.descriptionKey = descriptionKey;
		this.imageName = imageName;
		this.helpKeyword = helpKeyword;
		this.displayNodeType = displayNodeType;
	}

	public static Operation CreateUnknown(string operationDisplayName, string iconName)
	{
		return new Operation(operationDisplayName, null, null, iconName, Unknown.HelpKeyword);
	}
}
