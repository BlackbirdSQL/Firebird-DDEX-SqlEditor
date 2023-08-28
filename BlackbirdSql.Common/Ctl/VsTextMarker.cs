#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLEditor\Microsoft.VisualStudio.Data.Tools.SqlEditor.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Runtime.InteropServices;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.TextManager.Interop;




namespace BlackbirdSql.Common.Ctl;


[ComVisible(false)]
public class VsTextMarker : IVsPackageDefinedTextMarkerType, IVsMergeableUIItem
{
	private readonly uint visualFlags;

	private readonly COLORINDEX foreground;

	private readonly COLORINDEX background;

	private readonly COLORINDEX lineColor;

	private readonly LINESTYLE lineStyle;

	private readonly uint fontFlags;

	private readonly uint behaviorFlag;

	private readonly int priorityIndex;

	private readonly string nameLocalized;

	private readonly string description;

	private readonly string nameEnglish;

	public VsTextMarker(uint visualFlags, COLORINDEX foreground, COLORINDEX background, string nameLocalized, string description, string nameEnglish)
	{
		this.visualFlags = visualFlags;
		this.foreground = foreground;
		this.background = background;
		lineColor = COLORINDEX.CI_USERTEXT_BK;
		lineStyle = LINESTYLE.LI_NONE;
		fontFlags = 0u;
		behaviorFlag = 0u;
		priorityIndex = 0;
		this.nameLocalized = nameLocalized;
		this.description = description;
		this.nameEnglish = nameEnglish;
	}

	public VsTextMarker(uint visualFlags, COLORINDEX foreground, COLORINDEX background, COLORINDEX lineColor, LINESTYLE lineStyle, uint fontFlags, uint behaviorFlag, int priorityIndex, string nameLocalized, string description, string nameEnglish)
	{
		this.visualFlags = visualFlags;
		this.foreground = foreground;
		this.background = background;
		this.lineColor = lineColor;
		this.lineStyle = lineStyle;
		this.fontFlags = fontFlags;
		this.behaviorFlag = behaviorFlag;
		this.priorityIndex = priorityIndex;
		this.nameLocalized = nameLocalized;
		this.description = description;
		this.nameEnglish = nameEnglish;
	}

	int IVsPackageDefinedTextMarkerType.GetVisualStyle(out uint visualFlags)
	{
		visualFlags = this.visualFlags;
		return VSConstants.S_OK;
	}

	int IVsPackageDefinedTextMarkerType.GetDefaultColors(COLORINDEX[] foreground, COLORINDEX[] background)
	{
		foreground[0] = this.foreground;
		background[0] = this.background;
		return VSConstants.S_OK;
	}

	int IVsPackageDefinedTextMarkerType.GetDefaultLineStyle(COLORINDEX[] lineColor, LINESTYLE[] lineStyle)
	{
		lineColor[0] = this.lineColor;
		lineStyle[0] = this.lineStyle;
		return VSConstants.S_OK;
	}

	int IVsPackageDefinedTextMarkerType.GetDefaultFontFlags(out uint fontFlags)
	{
		fontFlags = this.fontFlags;
		return VSConstants.S_OK;
	}

	int IVsPackageDefinedTextMarkerType.DrawGlyphWithColors(IntPtr hdc, RECT[] rect, int markerType, IVsTextMarkerColorSet markerColors, uint glyphDrawFlags, int lineHeight)
	{
		return VSConstants.S_OK;
	}

	int IVsPackageDefinedTextMarkerType.GetBehaviorFlags(out uint behaviorFlag)
	{
		behaviorFlag = this.behaviorFlag;
		return VSConstants.S_OK;
	}

	int IVsPackageDefinedTextMarkerType.GetPriorityIndex(out int priorityIndex)
	{
		priorityIndex = this.priorityIndex;
		return VSConstants.S_OK;
	}

	int IVsMergeableUIItem.GetMergingPriority(out int mergingPriority)
	{
		mergingPriority = 0;
		return VSConstants.S_OK;
	}

	int IVsMergeableUIItem.GetDisplayName(out string displayName)
	{
		displayName = nameLocalized;
		return VSConstants.S_OK;
	}

	int IVsMergeableUIItem.GetDescription(out string description)
	{
		description = this.description;
		return VSConstants.S_OK;
	}

	int IVsMergeableUIItem.GetCanonicalName(out string nonLocalizeName)
	{
		nonLocalizeName = nameEnglish;
		return VSConstants.S_OK;
	}
}
