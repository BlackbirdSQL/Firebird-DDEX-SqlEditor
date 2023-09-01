// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.WPF.DialogFontProvider

using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using BlackbirdSql.Common.Enums;
using BlackbirdSql.Core.Diagnostics;
using BlackbirdSql.Core.Diagnostics.Enums;



namespace BlackbirdSql.Common.Model;


[EditorBrowsable(EditorBrowsableState.Never)]
public class DialogFontProvider
{
	private readonly double m_defaultFontSize;

	public double DefaultFontSize => m_defaultFontSize;

	public DialogFontProvider(string defaultFontSize)
	{
		try
		{
			FontSizeConverter fontSizeConverter = new FontSizeConverter();
			m_defaultFontSize = (double)fontSizeConverter.ConvertFromInvariantString(defaultFontSize);
		}
		catch
		{
			m_defaultFontSize = 12.0;
		}
	}

	public DialogFontProvider(double defaultFontSize)
	{
		m_defaultFontSize = defaultFontSize;
	}

	private double CalculateRelativeFontSize(int desiredPoints, int relativeTo = 9)
	{
		if (relativeTo < 1 || relativeTo == desiredPoints)
		{
			return m_defaultFontSize;
		}
		double num = (double)desiredPoints / (double)relativeTo;
		return m_defaultFontSize * num;
	}

	public virtual double GetFontSize(EnResourceKeyId id)
	{
		switch (id)
		{
		case EnResourceKeyId.TitleFontSize:
			return CalculateRelativeFontSize(14);
		case EnResourceKeyId.SubtitleFontSize:
			return CalculateRelativeFontSize(11);
		case EnResourceKeyId.ActionLinkFontSize:
		case EnResourceKeyId.BodyFontSize:
		case EnResourceKeyId.EmphasizedFontSize:
		case EnResourceKeyId.SectionTitleFontSize:
		case EnResourceKeyId.SubduedFontSize:
			return CalculateRelativeFontSize(9);
		case EnResourceKeyId.NotificationFontSize:
			return CalculateRelativeFontSize(8);
		case EnResourceKeyId.DocumentHeadingFontSize:
			return CalculateRelativeFontSize(14, 8);
		case EnResourceKeyId.DocumentLinkFontSize:
			return CalculateRelativeFontSize(9, 8);
		case EnResourceKeyId.DocumentSectionHeadingFontSize:
			return CalculateRelativeFontSize(9, 8);
		default:
			UiTracer.TraceSource.AssertTraceEvent(condition: false, TraceEventType.Error, EnUiTraceId.UiInfra, "Unknown double requested: " + id);
			return m_defaultFontSize;
		}
	}
}
