// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.Description
using System;
using System.Collections;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;


namespace BlackbirdSql.Shared.Controls.Graphing;

public class DescriptionControl : UserControl
{
	private string title = string.Empty;

	private string queryText = string.Empty;

	private string toolTipQueryText = string.Empty;

	private string clusteredMode = string.Empty;

	private bool isClusteredMode;

	private bool hasMissingIndex;

	private string missingIndexCaption = string.Empty;

	private string missingIndexQueryText = string.Empty;

	private string missingIndexImpact = string.Empty;

	private string missingIndexDatabase = string.Empty;

	private int missingIndexBorder;

	private ToolTip toolTip = new ToolTip();

	private const string C_NewLine = "\r\n";

	private const int C_MaxTooltipLength = 1000;

	private static readonly Size Margins = new Size(2, 2);

	private static readonly Regex whitespaceExpression = new Regex("[\n\r\t ]+", RegexOptions.Compiled);

	public string Title
	{
		get
		{
			return title;
		}
		set
		{
			title = value.Trim().Replace(C_NewLine, " ");
			UpdateHeight();
			UpdateAccDescription();
		}
	}

	public string QueryText
	{
		get
		{
			return queryText;
		}
		set
		{
			string text = value.Trim();
			queryText = text.Replace(C_NewLine, " ");
			toolTipQueryText = MakeTruncatedTooltip(queryText);
			if (toolTipQueryText.Length > 0)
			{
				toolTip.SetToolTip(this, toolTipQueryText);
			}
			UpdateHeight();
			UpdateAccDescription();
		}
	}

	public string ClusteredMode
	{
		get
		{
			return clusteredMode;
		}
		set
		{
			clusteredMode = value.Trim().Replace(C_NewLine, " ");
			UpdateHeight();
			UpdateAccDescription();
		}
	}

	public bool IsClusteredMode
	{
		set
		{
			isClusteredMode = value;
			UpdateAccDescription();
		}
	}

	public bool HasMissingIndex => hasMissingIndex;

	public string MissingIndexQueryText => missingIndexQueryText;

	public string MissingIndexImpact => missingIndexImpact;

	public string MissingIndexDatabase => missingIndexDatabase;

	internal StringFormat DescriptionFormat => new StringFormat
	{
		Alignment = StringAlignment.Near,
		LineAlignment = StringAlignment.Center,
		Trimming = StringTrimming.EllipsisCharacter,
		FormatFlags = StringFormatFlags.NoWrap
	};

	public DescriptionControl()
	{
		SetStyle(ControlStyles.Opaque, value: true);
	}

	public void SetOptionalMissingIndex(string caption, string queryText, string impact, string database)
	{
		hasMissingIndex = true;
		missingIndexCaption = caption;
		missingIndexQueryText = queryText;
		missingIndexImpact = impact;
		missingIndexDatabase = database;
		UpdateAccDescription();
	}

	public static string MakeTruncatedTooltip(string strSource)
	{
		if (string.IsNullOrEmpty(strSource))
		{
			return string.Empty;
		}
		if (strSource.Length <= C_MaxTooltipLength)
		{
			return strSource;
		}
		return strSource[..C_MaxTooltipLength] + "...";
	}

	protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
	{
		if ((specified & BoundsSpecified.Height) != 0)
		{
			using Graphics graphics = CreateGraphics();
			int num = 0;
			int num2 = 0;
			string[] linesOfText = GetLinesOfText();
			foreach (string text in linesOfText)
			{
				num = Math.Max(num, (int)Math.Ceiling(graphics.MeasureString(text, Font).Height));
				num2++;
			}
			height = 2 * Margins.Height + num * num2;
		}
		base.SetBoundsCore(x, y, width, height, specified);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		StringFormat descriptionFormat = DescriptionFormat;
		e.Graphics.FillRectangle(SystemBrushes.Window, Rectangle.Inflate(ClientRectangle, 0, -1));
		Rectangle rectangle = Rectangle.Inflate(ClientRectangle, -Margins.Width, -Margins.Height);
		float num = rectangle.Top;
		string[] linesOfText = GetLinesOfText();
		float num2 = rectangle.Height / linesOfText.Length;
		int num3 = 0;
		int num4 = hasMissingIndex ? linesOfText.Length : -1;
		string[] array = linesOfText;
		foreach (string s in array)
		{
			RectangleF layoutRectangle = new RectangleF(rectangle.Left, num, rectangle.Width, num2);
			if (hasMissingIndex && ++num3 == num4)
			{
				missingIndexBorder = (int)num;
				using SolidBrush brush = new SolidBrush(Color.Green);
				e.Graphics.DrawString(s, Font, brush, layoutRectangle, descriptionFormat);
			}
			else
			{
				e.Graphics.DrawString(s, Font, SystemBrushes.WindowText, layoutRectangle, descriptionFormat);
			}
			num += num2;
		}
		e.Graphics.DrawLine(SystemPens.WindowText, 0, 0, Width - 1, 0);
		e.Graphics.DrawLine(SystemPens.WindowText, 0, Height - 1, Width - 1, Height - 1);
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		if (hasMissingIndex)
		{
			string strSource = toolTipQueryText;
			if (e.Y >= missingIndexBorder)
			{
				strSource = missingIndexQueryText;
			}
			strSource = MakeTruncatedTooltip(strSource);
			if (toolTip.GetToolTip(this) != strSource)
			{
				toolTip.SetToolTip(this, strSource);
			}
		}
		base.OnMouseMove(e);
	}

	protected override void OnResize(EventArgs e)
	{
		base.OnResize(e);
		Invalidate();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && toolTip != null)
		{
			toolTip.Dispose();
			toolTip = null;
		}
		base.Dispose(disposing);
	}

	private string[] GetLinesOfText()
	{
		ArrayList arrayList = new ArrayList();
		if (isClusteredMode)
		{
			arrayList.Add(ClusteredMode);
		}
		arrayList.Add(Title);
		arrayList.Add(RemoveWhitespaces(queryText));
		if (hasMissingIndex)
		{
			arrayList.Add(RemoveWhitespaces(missingIndexCaption));
		}
		return (string[])arrayList.ToArray(typeof(string));
	}

	private static string RemoveWhitespaces(string line)
	{
		if (line == null)
		{
			return string.Empty;
		}
		return whitespaceExpression.Replace(line, " ");
	}

	private void UpdateHeight()
	{
		Height = 0;
	}

	private void UpdateAccDescription()
	{
		string[] linesOfText = GetLinesOfText();
		AccessibleDescription = string.Join(Environment.NewLine, linesOfText);
	}
}
