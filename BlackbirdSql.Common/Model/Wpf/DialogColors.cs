// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.WPF.DialogColors

using System;
using System.ComponentModel;
using System.Windows;
using BlackbirdSql.Common.Ctl.Enums;

namespace BlackbirdSql.Common.Model.Wpf;


public class DialogColors : INotifyPropertyChanged
{
	private static DialogColors s_instance;

	private DialogColorProvider m_colorProvider;

	public static DialogColors Instance => s_instance ??= new DialogColors();

	[EditorBrowsable(EditorBrowsableState.Never)]
	public DialogColorProvider ColorProvider
	{
		get
		{
			return m_colorProvider ??= new DialogColorProvider();
		}
		set
		{
			m_colorProvider = value;
		}
	}

	public System.Windows.Media.Color ActionLinkColor => GetColor(EnResourceKeyId.ActionLinkColor);

	public System.Windows.Media.Color ActionLinkDisabledColor => GetColor(EnResourceKeyId.ActionLinkDisabledColor);

	public System.Windows.Media.Color ActionLinkItemColor => GetColor(EnResourceKeyId.ActionLinkItemColor);

	public System.Windows.Media.Color ActionLinkItemDisabledColor => GetColor(EnResourceKeyId.ActionLinkItemDisabledColor);

	public System.Windows.Media.Color ActionLinkItemHoverColor => GetColor(EnResourceKeyId.ActionLinkItemHoverColor);

	public System.Windows.Media.Color ActionLinkItemSelectedColor => GetColor(EnResourceKeyId.ActionLinkItemSelectedColor);

	public System.Windows.Media.Color ActionLinkItemSelectedNotFocusedColor => GetColor(EnResourceKeyId.ActionLinkItemSelectedNotFocusedColor);

	public System.Windows.Media.Color ArrowGlyphColor => GetColor(EnResourceKeyId.ArrowGlyphColor);

	public System.Windows.Media.Color ArrowGlyphMouseOverColor => GetColor(EnResourceKeyId.ArrowGlyphMouseOverColor);

	public System.Windows.Media.Color BodyTextColor => GetColor(EnResourceKeyId.BodyTextColor);

	public System.Windows.Media.Color ButtonBackgroundColor => GetColor(EnResourceKeyId.ButtonBackgroundColor);

	public System.Windows.Media.Color ButtonBorderColor => GetColor(EnResourceKeyId.ButtonBorderColor);

	public System.Windows.Media.Color ButtonDisabledBackgroundColor => GetColor(EnResourceKeyId.ButtonDisabledBackgroundColor);

	public System.Windows.Media.Color ButtonDisabledBorderColor => GetColor(EnResourceKeyId.ButtonDisabledBorderColor);

	public System.Windows.Media.Color ButtonDisabledForegroundColor => GetColor(EnResourceKeyId.ButtonDisabledForegroundColor);

	public System.Windows.Media.Color ButtonForegroundColor => GetColor(EnResourceKeyId.ButtonForegroundColor);

	public System.Windows.Media.Color ButtonHoverBackgroundColor => GetColor(EnResourceKeyId.ButtonHoverBackgroundColor);

	public System.Windows.Media.Color ButtonHoverBorderColor => GetColor(EnResourceKeyId.ButtonHoverBorderColor);

	public System.Windows.Media.Color ButtonHoverForegroundColor => GetColor(EnResourceKeyId.ButtonHoverForegroundColor);

	public System.Windows.Media.Color ButtonPressedBackgroundColor => GetColor(EnResourceKeyId.ButtonPressedBackgroundColor);

	public System.Windows.Media.Color ButtonPressedBorderColor => GetColor(EnResourceKeyId.ButtonPressedBorderColor);

	public System.Windows.Media.Color ButtonPressedForegroundColor => GetColor(EnResourceKeyId.ButtonPressedForegroundColor);

	public System.Windows.Media.Color DefaultSelectedAnchorPointBorderColor => GetColor(EnResourceKeyId.DefaultSelectedAnchorPointBorderColor);

	public System.Windows.Media.Color DefaultSelectedAnchorPointColor => GetColor(EnResourceKeyId.DefaultSelectedAnchorPointColor);

	public System.Windows.Media.Color DefaultUnselectedAnchorPointBorderColor => GetColor(EnResourceKeyId.DefaultUnselectedAnchorPointBorderColor);

	public System.Windows.Media.Color DefaultUnselectedAnchorPointColor => GetColor(EnResourceKeyId.DefaultUnselectedAnchorPointColor);

	public System.Windows.Media.Color DefaultSelectedCommentAnchorBorderColor => GetColor(EnResourceKeyId.DefaultSelectedCommentAnchorBorderColor);

	public System.Windows.Media.Color DefaultSelectedCommentAnchorFillColor => GetColor(EnResourceKeyId.DefaultSelectedCommentAnchorFillColor);

	public System.Windows.Media.Color DefaultUnselectedCommentAnchorBorderColor => GetColor(EnResourceKeyId.DefaultUnselectedCommentAnchorBorderColor);

	public System.Windows.Media.Color DefaultUnselectedCommentAnchorFillColor => GetColor(EnResourceKeyId.DefaultUnselectedCommentAnchorFillColor);

	public System.Windows.Media.Color EmbeddedDialogBackgroundColor => GetColor(EnResourceKeyId.EmbeddedDialogBackgroundColor);

	public System.Windows.Media.Color CodeReviewDiscussionSelectedItemColor => GetColor(EnResourceKeyId.CodeReviewDiscussionSelectedItemColor);

	public System.Windows.Media.Color CodeReviewDiscussionSelectedItemColorTextColor => GetColor(EnResourceKeyId.CodeReviewDiscussionSelectedItemColorTextColor);

	public System.Windows.Media.Color CodeReviewDiscussionSelectedActionLinkColor => GetColor(EnResourceKeyId.CodeReviewDiscussionSelectedActionLinkColor);

	public System.Windows.Media.Color CodeReviewDiscussionDisabledActionLinkColor => GetColor(EnResourceKeyId.CodeReviewDiscussionDisabledActionLinkColor);

	public System.Windows.Media.Color ConnDlgSelectedTabUnderLineColor => GetColor(EnResourceKeyId.ConnDlgSelectedTabUnderLineColor);

	public System.Windows.Media.Color EmphasizedTextColor => GetColor(EnResourceKeyId.EmphasizedTextColor);

	public System.Windows.Media.Color IconActionFillColor => GetColor(EnResourceKeyId.IconActionFillColor);

	public System.Windows.Media.Color IconGeneralFillColor => GetColor(EnResourceKeyId.IconGeneralFillColor);

	public System.Windows.Media.Color IconGeneralStrokeColor => GetColor(EnResourceKeyId.IconGeneralStrokeColor);

	public System.Windows.Media.Color InnerTabActiveBackgroundColor => GetColor(EnResourceKeyId.InnerTabActiveBackgroundColor);

	public System.Windows.Media.Color InnerTabActiveTextColor => GetColor(EnResourceKeyId.InnerTabActiveTextColor);

	public System.Windows.Media.Color InnerTabHeaderBackgroundColor => GetColor(EnResourceKeyId.InnerTabHeaderBackgroundColor);

	public System.Windows.Media.Color InnerTabHeaderTextColor => GetColor(EnResourceKeyId.InnerTabHeaderTextColor);

	public System.Windows.Media.Color InnerTabHoverBackgroundColor => GetColor(EnResourceKeyId.InnerTabHoverBackgroundColor);

	public System.Windows.Media.Color InnerTabHoverTextColor => GetColor(EnResourceKeyId.InnerTabHoverTextColor);

	public System.Windows.Media.Color InnerTabInactiveBackgroundColor => GetColor(EnResourceKeyId.InnerTabInactiveBackgroundColor);

	public System.Windows.Media.Color InnerTabInactiveTextColor => GetColor(EnResourceKeyId.InnerTabInactiveTextColor);

	public System.Windows.Media.Color InnerTabPressedBackgroundColor => GetColor(EnResourceKeyId.InnerTabPressedBackgroundColor);

	public System.Windows.Media.Color InnerTabPressedTextColor => GetColor(EnResourceKeyId.InnerTabPressedTextColor);

	public System.Windows.Media.Color ItemBorderColor => GetColor(EnResourceKeyId.ItemBorderColor);

	public System.Windows.Media.Color ItemColor => GetColor(EnResourceKeyId.ItemColor);

	public System.Windows.Media.Color ItemHoverBorderColor => GetColor(EnResourceKeyId.ItemHoverBorderColor);

	public System.Windows.Media.Color ItemHoverColor => GetColor(EnResourceKeyId.ItemHoverColor);

	public System.Windows.Media.Color ItemHoverTextColor => GetColor(EnResourceKeyId.ItemHoverTextColor);

	public System.Windows.Media.Color ItemSelectedBorderColor => GetColor(EnResourceKeyId.ItemSelectedBorderColor);

	public System.Windows.Media.Color ItemSelectedBorderNotFocusedColor => GetColor(EnResourceKeyId.ItemSelectedBorderNotFocusedColor);

	public System.Windows.Media.Color ItemSelectedColor => GetColor(EnResourceKeyId.ItemSelectedColor);

	public System.Windows.Media.Color ItemSelectedNotFocusedColor => GetColor(EnResourceKeyId.ItemSelectedNotFocusedColor);

	public System.Windows.Media.Color ItemSelectedTextColor => GetColor(EnResourceKeyId.ItemSelectedTextColor);

	public System.Windows.Media.Color ItemSelectedTextNotFocusedColor => GetColor(EnResourceKeyId.ItemSelectedTextNotFocusedColor);

	public System.Windows.Media.Color ItemTextColor => GetColor(EnResourceKeyId.ItemTextColor);

	public System.Windows.Media.Color MenuBackgroundColor => GetColor(EnResourceKeyId.MenuBackgroundColor);

	public System.Windows.Media.Color MenuBorderColor => GetColor(EnResourceKeyId.MenuBorderColor);

	public System.Windows.Media.Color MenuHoverBackgroundColor => GetColor(EnResourceKeyId.MenuHoverBackgroundColor);

	public System.Windows.Media.Color MenuHoverTextColor => GetColor(EnResourceKeyId.MenuHoverTextColor);

	public System.Windows.Media.Color MenuSeparatorColor => GetColor(EnResourceKeyId.MenuSeparatorColor);

	public System.Windows.Media.Color MenuTextColor => GetColor(EnResourceKeyId.MenuTextColor);

	public System.Windows.Media.Color NotificationActionLinkColor => GetColor(EnResourceKeyId.NotificationActionLinkColor);

	public System.Windows.Media.Color NotificationColor => GetColor(EnResourceKeyId.NotificationColor);

	public System.Windows.Media.Color NotificationTextColor => GetColor(EnResourceKeyId.NotificationTextColor);

	public System.Windows.Media.Color ProgressBarBackgroundColor => GetColor(EnResourceKeyId.ProgressBarBackgroundColor);

	public System.Windows.Media.Color ProgressBarForegroundColor => GetColor(EnResourceKeyId.ProgressBarForegroundColor);

	public System.Windows.Media.Color RequiredTextBoxBorderColor => GetColor(EnResourceKeyId.RequiredTextBoxBorderColor);

	public System.Windows.Media.Color ScrollBarBackgroundColor => GetColor(EnResourceKeyId.ScrollBarBackgroundColor);

	public System.Windows.Media.Color SectionTitleTextColor => GetColor(EnResourceKeyId.SectionTitleTextColor);

	public System.Windows.Media.Color SubduedBorderColor => GetColor(EnResourceKeyId.SubduedBorderColor);

	public System.Windows.Media.Color SubduedTextColor => GetColor(EnResourceKeyId.SubduedTextColor);

	public System.Windows.Media.Color SubtitleTextColor => GetColor(EnResourceKeyId.SubtitleTextColor);

	public System.Windows.Media.Color TextBoxBorderColor => GetColor(EnResourceKeyId.TextBoxBorderColor);

	public System.Windows.Media.Color TextBoxColor => GetColor(EnResourceKeyId.TextBoxColor);

	public System.Windows.Media.Color TextBoxHintTextColor => GetColor(EnResourceKeyId.TextBoxHintTextColor);

	public System.Windows.Media.Color TextBoxTextColor => GetColor(EnResourceKeyId.TextBoxTextColor);

	public System.Windows.Media.Color TitleBarInactiveColor => GetColor(EnResourceKeyId.TitleBarInactiveColor);

	public System.Windows.Media.Color TitleBarInactiveTextColor => GetColor(EnResourceKeyId.TitleBarInactiveTextColor);

	public System.Windows.Media.Color TitleTextColor => GetColor(EnResourceKeyId.TitleTextColor);

	public System.Windows.Media.Color ToolbarColor => GetColor(EnResourceKeyId.ToolbarColor);

	public System.Windows.Media.Color ToolbarSelectedBorderColor => GetColor(EnResourceKeyId.ToolbarSelectedBorderColor);

	public System.Windows.Media.Color ToolbarSelectedColor => GetColor(EnResourceKeyId.ToolbarSelectedColor);

	public System.Windows.Media.Color ToolbarSelectedTextColor => GetColor(EnResourceKeyId.ToolbarSelectedTextColor);

	public System.Windows.Media.Color ToolbarTextColor => GetColor(EnResourceKeyId.ToolbarTextColor);

	public System.Windows.Media.Color ToolWindowBackgroundColor => GetColor(EnResourceKeyId.ToolWindowBackgroundColor);

	public System.Windows.Media.Color ToolWindowBorderColor => GetColor(EnResourceKeyId.ToolWindowBorderColor);

	public System.Windows.Media.Color TeamExplorerHomeDefaultIconFillColor => GetColor(EnResourceKeyId.TeamExplorerHomeDefaultIconFillColor);

	public System.Windows.Media.Color TeamExplorerHomeWITFamilyIconFillColor => GetColor(EnResourceKeyId.TeamExplorerHomeWITFamilyIconFillColor);

	public System.Windows.Media.Color TeamExplorerHomeSCCFamilyIconFillColor => GetColor(EnResourceKeyId.TeamExplorerHomeSCCFamilyIconFillColor);

	public System.Windows.Media.Color TeamExplorerHomeBuildFamilyIconFillColor => GetColor(EnResourceKeyId.TeamExplorerHomeBuildFamilyIconFillColor);

	public System.Windows.Media.Color TeamExplorerHomeSCCGitFamilyIconFillColor => GetColor(EnResourceKeyId.TeamExplorerHomeSCCGitFamilyIconFillColor);

	public System.Windows.Media.Color TeamExplorerHomeDocumentsFamilyIconFillColor => GetColor(EnResourceKeyId.TeamExplorerHomeDocumentsFamilyIconFillColor);

	public System.Windows.Media.Color TeamExplorerHomeReportsFamilyIconFillColor => GetColor(EnResourceKeyId.TeamExplorerHomeReportsFamilyIconFillColor);

	public System.Windows.Media.Color TETileIconBackgroundColor => GetColor(EnResourceKeyId.TETileIconBackgroundColor);

	public System.Windows.Media.Color TETileIconBackgroundMouseOverColor => GetColor(EnResourceKeyId.TETileIconBackgroundMouseOverColor);

	public System.Windows.Media.Color TETileIconBackgroundSelectedColor => GetColor(EnResourceKeyId.TETileIconBackgroundSelectedColor);

	public System.Windows.Media.Color TETileListBackgroundColor => GetColor(EnResourceKeyId.TETileListBackgroundColor);

	public System.Windows.Media.Color TETileListBackgroundMouseOverColor => GetColor(EnResourceKeyId.TETileListBackgroundMouseOverColor);

	public System.Windows.Media.Color TETileListBackgroundSelectedColor => GetColor(EnResourceKeyId.TETileListBackgroundSelectedColor);

	public System.Windows.Media.Color TETileListForegroundColor => GetColor(EnResourceKeyId.TETileListForegroundColor);

	public System.Windows.Media.Color TETileListForegroundMouseOverColor => GetColor(EnResourceKeyId.TETileListForegroundMouseOverColor);

	public System.Windows.Media.Color TETileListForegroundSelectedColor => GetColor(EnResourceKeyId.TETileListForegroundSelectedColor);

	public System.Windows.Media.Color TETileBorderColor => GetColor(EnResourceKeyId.TETileBorderColor);

	public System.Windows.Media.Color VSLogoIconFillColor => GetColor(EnResourceKeyId.VSLogoIconFillColor);

	public System.Windows.Media.Color WorkItemActiveControlBackground => GetColor(EnResourceKeyId.WorkItemActiveControlBackground);

	public System.Windows.Media.Color WorkItemActiveControlForeground => GetColor(EnResourceKeyId.WorkItemActiveControlForeground);

	public System.Windows.Media.Color WorkItemActiveControlBorder => GetColor(EnResourceKeyId.WorkItemActiveControlBorder);

	public System.Windows.Media.Color WorkItemButtonSelectedOverride => GetColor(EnResourceKeyId.WorkItemButtonSelectedOverride);

	public System.Windows.Media.Color WorkItemDefaultControlBackground => GetColor(EnResourceKeyId.WorkItemDefaultControlBackground);

	public System.Windows.Media.Color WorkItemDefaultControlForeground => GetColor(EnResourceKeyId.WorkItemDefaultControlForeground);

	public System.Windows.Media.Color WorkItemDefaultControlBorder => GetColor(EnResourceKeyId.WorkItemDefaultControlBorder);

	public System.Windows.Media.Color WorkItemDropDownHoverBackground => GetColor(EnResourceKeyId.WorkItemDropDownHoverBackground);

	public System.Windows.Media.Color WorkItemDropDownHoverForeground => GetColor(EnResourceKeyId.WorkItemDropDownHoverForeground);

	public System.Windows.Media.Color WorkItemDropDownPressedBackground => GetColor(EnResourceKeyId.WorkItemDropDownPressedBackground);

	public System.Windows.Media.Color WorkItemDropDownPressedForeground => GetColor(EnResourceKeyId.WorkItemDropDownPressedForeground);

	public System.Windows.Media.Color WorkItemDropDownInactiveBackground => GetColor(EnResourceKeyId.WorkItemDropDownInactiveBackground);

	public System.Windows.Media.Color WorkItemDropDownInactiveForeground => GetColor(EnResourceKeyId.WorkItemDropDownInactiveForeground);

	public System.Windows.Media.Color WorkItemErrorControlBackground => GetColor(EnResourceKeyId.WorkItemErrorControlBackground);

	public System.Windows.Media.Color WorkItemErrorControlForeground => GetColor(EnResourceKeyId.WorkItemErrorControlForeground);

	public System.Windows.Media.Color WorkItemErrorControlBorder => GetColor(EnResourceKeyId.WorkItemErrorControlBorder);

	public System.Windows.Media.Color WorkItemFormBackground => GetColor(EnResourceKeyId.WorkItemFormBackground);

	public System.Windows.Media.Color WorkItemFormForeground => GetColor(EnResourceKeyId.WorkItemFormForeground);

	public System.Windows.Media.Color WorkItemHtmlControlBackground => GetColor(EnResourceKeyId.WorkItemHtmlControlBackground);

	public System.Windows.Media.Color WorkItemHtmlControlForeground => GetColor(EnResourceKeyId.WorkItemHtmlControlForeground);

	public System.Windows.Media.Color WorkItemHtmlControlHyperlink => GetColor(EnResourceKeyId.WorkItemHtmlControlHyperlink);

	public System.Windows.Media.Color WorkItemHtmlControlSecondaryText => GetColor(EnResourceKeyId.WorkItemHtmlControlSecondaryText);

	public System.Windows.Media.Color WorkItemHtmlScrollbarArrow => GetColor(EnResourceKeyId.WorkItemHtmlScrollbarArrow);

	public System.Windows.Media.Color WorkItemHtmlScrollbarTrack => GetColor(EnResourceKeyId.WorkItemHtmlScrollbarTrack);

	public System.Windows.Media.Color WorkItemHtmlScrollbarThumb => GetColor(EnResourceKeyId.WorkItemHtmlScrollbarThumb);

	public System.Windows.Media.Color WorkItemInvalidControlBackground => GetColor(EnResourceKeyId.WorkItemInvalidControlBackground);

	public System.Windows.Media.Color WorkItemInvalidControlForeground => GetColor(EnResourceKeyId.WorkItemInvalidControlForeground);

	public System.Windows.Media.Color WorkItemInvalidControlBorder => GetColor(EnResourceKeyId.WorkItemInvalidControlBorder);

	public System.Windows.Media.Color WorkItemGroupBoxBackground => GetColor(EnResourceKeyId.WorkItemGroupBoxBackground);

	public System.Windows.Media.Color WorkItemGroupBoxForeground => GetColor(EnResourceKeyId.WorkItemGroupBoxForeground);

	public System.Windows.Media.Color WorkItemLabelBackground => GetColor(EnResourceKeyId.WorkItemLabelBackground);

	public System.Windows.Media.Color WorkItemLabelForeground => GetColor(EnResourceKeyId.WorkItemLabelForeground);

	public System.Windows.Media.Color WorkItemPromptText => GetColor(EnResourceKeyId.WorkItemPromptText);

	public System.Windows.Media.Color WorkItemReadOnlyControlBackground => GetColor(EnResourceKeyId.WorkItemReadOnlyControlBackground);

	public System.Windows.Media.Color WorkItemReadOnlyControlForeground => GetColor(EnResourceKeyId.WorkItemReadOnlyControlForeground);

	public System.Windows.Media.Color WorkItemReadOnlyControlBorder => GetColor(EnResourceKeyId.WorkItemReadOnlyControlBorder);

	public System.Windows.Media.Color WorkItemTabItemBackground => GetColor(EnResourceKeyId.WorkItemTabItemBackground);

	public System.Windows.Media.Color WorkItemTabItemForeground => GetColor(EnResourceKeyId.WorkItemTabItemForeground);

	public System.Windows.Media.Color WorkItemTabHeaderBackground => GetColor(EnResourceKeyId.WorkItemTabHeaderBackground);

	public System.Windows.Media.Color WorkItemTabHeaderForeground => GetColor(EnResourceKeyId.WorkItemTabHeaderForeground);

	public System.Windows.Media.Color WorkItemTabHeaderActiveForeground => GetColor(EnResourceKeyId.WorkItemTabHeaderActiveForeground);

	public System.Windows.Media.Color WorkItemTabHeaderHoverForeground => GetColor(EnResourceKeyId.WorkItemTabHeaderHoverForeground);

	public System.Windows.Media.Color WorkItemGridBackground => GetColor(EnResourceKeyId.WorkItemGridBackground);

	public System.Windows.Media.Color WorkItemGridForeground => GetColor(EnResourceKeyId.WorkItemGridForeground);

	public System.Windows.Media.Color WorkItemGridBorder => GetColor(EnResourceKeyId.WorkItemGridBorder);

	public System.Windows.Media.Color WorkItemGridRowHeaderBackground => GetColor(EnResourceKeyId.WorkItemGridRowHeaderBackground);

	public System.Windows.Media.Color WorkItemGridRowHeaderForeground => GetColor(EnResourceKeyId.WorkItemGridRowHeaderForeground);

	public System.Windows.Media.Color WorkItemGridColumnHeaderBackground => GetColor(EnResourceKeyId.WorkItemGridColumnHeaderBackground);

	public System.Windows.Media.Color WorkItemGridColumnHeaderForeground => GetColor(EnResourceKeyId.WorkItemGridColumnHeaderForeground);

	public System.Windows.Media.Color WorkItemGridColumnHeaderHoverBackground => GetColor(EnResourceKeyId.WorkItemGridColumnHeaderHoverBackground);

	public System.Windows.Media.Color WorkItemGridColumnHeaderHoverForeground => GetColor(EnResourceKeyId.WorkItemGridColumnHeaderHoverForeground);

	public System.Windows.Media.Color WorkItemGridColumnHeaderPressedBackground => GetColor(EnResourceKeyId.WorkItemGridColumnHeaderPressedBackground);

	public System.Windows.Media.Color WorkItemGridColumnHeaderPressedForeground => GetColor(EnResourceKeyId.WorkItemGridColumnHeaderPressedForeground);

	public System.Windows.Media.Color WorkItemGridActiveRowHeaderBackground => GetColor(EnResourceKeyId.WorkItemGridActiveRowHeaderBackground);

	public System.Windows.Media.Color WorkItemGridActiveRowHeaderForeground => GetColor(EnResourceKeyId.WorkItemGridActiveRowHeaderForeground);

	public System.Windows.Media.Color WorkItemGridActiveColumnHeaderBackground => GetColor(EnResourceKeyId.WorkItemGridActiveColumnHeaderBackground);

	public System.Windows.Media.Color WorkItemGridActiveColumnHeaderForeground => GetColor(EnResourceKeyId.WorkItemGridActiveColumnHeaderForeground);

	public System.Windows.Media.Color WorkItemGridCellBackground => GetColor(EnResourceKeyId.WorkItemGridCellBackground);

	public System.Windows.Media.Color WorkItemGridCellForeground => GetColor(EnResourceKeyId.WorkItemGridCellForeground);

	public System.Windows.Media.Color WorkItemGridActiveCellBackground => GetColor(EnResourceKeyId.WorkItemGridActiveCellBackground);

	public System.Windows.Media.Color WorkItemGridActiveCellForeground => GetColor(EnResourceKeyId.WorkItemGridActiveCellForeground);

	public System.Windows.Media.Color WorkItemGridInactiveCellBackground => GetColor(EnResourceKeyId.WorkItemGridInactiveCellBackground);

	public System.Windows.Media.Color WorkItemGridInactiveCellForeground => GetColor(EnResourceKeyId.WorkItemGridInactiveCellForeground);

	public System.Windows.Media.Color WorkItemGridSortedCellBackground => GetColor(EnResourceKeyId.WorkItemGridSortedCellBackground);

	public System.Windows.Media.Color WorkItemGridSortedCellForeground => GetColor(EnResourceKeyId.WorkItemGridSortedCellForeground);

	public System.Windows.Media.Color WorkItemTrackingInfobarBackground => GetColor(EnResourceKeyId.WorkItemTrackingInfobarBackground);

	public System.Windows.Media.Color WorkItemTagForeground => GetColor(EnResourceKeyId.WorkItemTagForeground);

	public System.Windows.Media.Color WorkItemTagBackground => GetColor(EnResourceKeyId.WorkItemTagBackground);

	public System.Windows.Media.Color WorkItemTagActiveForeground => GetColor(EnResourceKeyId.WorkItemTagActiveForeground);

	public System.Windows.Media.Color WorkItemTagActiveBackground => GetColor(EnResourceKeyId.WorkItemTagActiveBackground);

	public System.Windows.Media.Color WorkItemTagHoverForeground => GetColor(EnResourceKeyId.WorkItemTagHoverForeground);

	public System.Windows.Media.Color WorkItemTagHoverBackground => GetColor(EnResourceKeyId.WorkItemTagHoverBackground);

	public System.Windows.Media.Color WorkItemTagActiveGlyphHoverForeground => GetColor(EnResourceKeyId.WorkItemTagActiveGlyphHoverForeground);

	public System.Windows.Media.Color WorkItemTagActiveGlyphHoverBackground => GetColor(EnResourceKeyId.WorkItemTagActiveGlyphHoverBackground);

	public System.Windows.Media.Color WorkItemTagActiveGlyphHoverBorder => GetColor(EnResourceKeyId.WorkItemTagActiveGlyphHoverBorder);

	public System.Windows.Media.Color WorkItemTagActiveGlyphPressedForeground => GetColor(EnResourceKeyId.WorkItemTagActiveGlyphPressedForeground);

	public System.Windows.Media.Color WorkItemTagActiveGlyphPressedBackground => GetColor(EnResourceKeyId.WorkItemTagActiveGlyphPressedBackground);

	public System.Windows.Media.Color WorkItemTagActiveGlyphPressedBorder => GetColor(EnResourceKeyId.WorkItemTagActiveGlyphPressedBorder);

	public System.Windows.Media.Color WorkItemTagHoverGlyphHoverForeground => GetColor(EnResourceKeyId.WorkItemTagHoverGlyphHoverForeground);

	public System.Windows.Media.Color WorkItemTagHoverGlyphHoverBackground => GetColor(EnResourceKeyId.WorkItemTagHoverGlyphHoverBackground);

	public System.Windows.Media.Color WorkItemTagHoverGlyphHoverBorder => GetColor(EnResourceKeyId.WorkItemTagHoverGlyphHoverBorder);

	public System.Windows.Media.Color WorkItemTagHoverGlyphPressedForeground => GetColor(EnResourceKeyId.WorkItemTagHoverGlyphPressedForeground);

	public System.Windows.Media.Color WorkItemTagHoverGlyphPressedBackground => GetColor(EnResourceKeyId.WorkItemTagHoverGlyphPressedBackground);

	public System.Windows.Media.Color WorkItemTagHoverGlyphPressedBorder => GetColor(EnResourceKeyId.WorkItemTagHoverGlyphPressedBorder);

	public System.Windows.Media.Color VersionControlAnnotateRegionBackgroundColor => GetColor(EnResourceKeyId.VersionControlAnnotateRegionBackgroundColor);

	public System.Windows.Media.Color VersionControlAnnotateRegionForegroundColor => GetColor(EnResourceKeyId.VersionControlAnnotateRegionForegroundColor);

	public System.Windows.Media.Color VersionControlAnnotateRegionSelectedBackgroundColor => GetColor(EnResourceKeyId.VersionControlAnnotateRegionSelectedBackgroundColor);

	public System.Windows.Media.Color VersionControlAnnotateRegionSelectedForegroundColor => GetColor(EnResourceKeyId.VersionControlAnnotateRegionSelectedForegroundColor);

	public event PropertyChangedEventHandler PropertyChanged;

	private DialogColors()
	{
	}

	private void UIHost_ColorChanged(object sender, EventArgs e)
	{
		OnPropertyChanged(null);
	}

	private System.Windows.Media.Color GetColor(EnResourceKeyId id)
	{
		if (SystemParameters.HighContrast)
		{
			return ColorProvider.GetHighContrastColor(id);
		}
		return ColorProvider.GetColor(id);
	}

	public static System.Drawing.Color ConvertToWinFormsColor(System.Windows.Media.Color color)
	{
		return Cmd.ConvertColor(color);
	}

	private void OnPropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
