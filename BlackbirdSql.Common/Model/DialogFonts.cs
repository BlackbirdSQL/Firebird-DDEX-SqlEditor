// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.WPF.DialogFonts

using System.ComponentModel;
using BlackbirdSql.Common.Enums;



namespace BlackbirdSql.Common.Model;


public class DialogFonts : INotifyPropertyChanged
{
	private static DialogFonts s_instance;

	private static DialogFontProvider s_fontProvider;

	public static DialogFonts Instance
	{
		get
		{
			s_instance ??= new DialogFonts();

			return s_instance;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static DialogFontProvider FontProvider
	{
		get
		{
			s_fontProvider ??= new DialogFontProvider("9pt");

			return s_fontProvider;
		}
		set
		{
			if (s_fontProvider == null || value == null || s_fontProvider.DefaultFontSize != value.DefaultFontSize)
			{
				s_fontProvider = value;
				Instance.OnPropertyChanged();
			}
		}
	}

	public double TitleFontSize => GetFontSize(EnResourceKeyId.TitleFontSize);

	public double SectionTitleFontSize => GetFontSize(EnResourceKeyId.SectionTitleFontSize);

	public double BodyFontSize => GetFontSize(EnResourceKeyId.BodyFontSize);

	public double EmphasizedFontSize => GetFontSize(EnResourceKeyId.EmphasizedFontSize);

	public double SubduedFontSize => GetFontSize(EnResourceKeyId.SubduedFontSize);

	public double NotificationFontSize => GetFontSize(EnResourceKeyId.NotificationFontSize);

	public double ActionLinkFontSize => GetFontSize(EnResourceKeyId.ActionLinkFontSize);

	public double DocumentHeadingFontSize => GetFontSize(EnResourceKeyId.DocumentHeadingFontSize);

	public double DocumentLinkFontSize => GetFontSize(EnResourceKeyId.DocumentLinkFontSize);

	public double DocumentSectionHeadingFontSize => GetFontSize(EnResourceKeyId.DocumentSectionHeadingFontSize);

	public double SubtitleFontSize => GetFontSize(EnResourceKeyId.SubtitleFontSize);

	public event PropertyChangedEventHandler PropertyChanged;

	protected DialogFonts()
	{
	}

	private double GetFontSize(EnResourceKeyId id)
	{
		return FontProvider.GetFontSize(id);
	}

	protected void OnPropertyChanged()
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
	}
}
