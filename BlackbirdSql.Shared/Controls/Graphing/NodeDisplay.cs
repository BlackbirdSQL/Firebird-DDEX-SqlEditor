// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.NodeDisplay
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.Windows.Forms;
using BlackbirdSql.Shared.Controls.Graphing.ComponentModel;
using BlackbirdSql.Shared.Controls.Graphing.Interfaces;
using BlackbirdSql.Shared.Properties;
using BlackbirdSql.Core.Ctl.ComponentModel;
using Microsoft.AnalysisServices.Graphing;


namespace BlackbirdSql.Shared.Controls.Graphing;

internal class NodeDisplay : Microsoft.AnalysisServices.Graphing.NodeDisplay, IRenderable, ICustomTypeDescriptor
{
	internal int MaxChildrenXPosition;

	private readonly NodeCollection<NodeDisplay> _Children;

	private static readonly NodeDisplayProperties DisplayPropertiesDefault;

	protected static readonly Size Margin;

	protected static readonly Size TextMargin;

	protected static readonly Size IconSize;

	private readonly int highlightBorderWidth = 5;

	private static readonly string[] ArgumentPropertyNames;

	private static Image parallelProcessingImage;

	private static Image warningImage;

	private static Image criticalWarningImage;

	private readonly Dictionary<PropertyDescriptor, bool> diffMap;

	internal Dictionary<PropertyDescriptor, bool> DiffMap => diffMap;

	public new NodeCollection<NodeDisplay> Children => _Children;

	internal NodeDisplay Parent
	{
		get
		{
			INodeEnumerator parents = Parents;
			if (parents.MoveNext())
			{
				return parents.Current as BlackbirdSql.Shared.Controls.Graphing.NodeDisplay;
			}
			return null;
		}
	}

	internal Image Image => NodeOriginal.Operation.Image;

	internal Color BackgroundColor { get; set; }

	internal bool UseBackgroundColor { get; set; }

	internal int GroupIndex { get; set; }

	internal Color TextColor { get; set; }

	internal StringFormat TextFormat { get; set; }

	internal virtual string DisplayName
	{
		get
		{
			if (NodeOriginal.Operation == Operation.Unknown)
			{
				return "";
			}
			string text = NodeOriginal["PhysicalOp"] as string;
			if (text == null)
			{
				if (NodeOriginal.Operation == null)
				{
					return "";
				}
				text = NodeOriginal.Operation.DisplayName;
			}
			if (NodeOriginal["PhysicalOperationKind"] is string text2)
			{
				text = "{0} {1}".Fmt(text, ControlsResources.Graphing_Parenthesis.Fmt(text2));
			}
			object obj = NodeOriginal["Object"];
			string text3;
			if (obj != null)
			{
				text3 = GetObjectNameForDisplay(obj);
			}
			else
			{
				text3 = NodeOriginal["LogicalOp"] as string;
				if (text3 != null)
				{
					text3 = !(text3 != text) ? null : ControlsResources.Parenthesis.Fmt(text3);
				}
			}
			if (text3 != null && text3.Length != 0)
			{
				return "{0}\n{1}".Fmt(text, text3);
			}
			return text;
		}
	}

	internal override string Name
	{
		get
		{
			string text = DisplayName.Replace("\n", "/");
			text = text.Replace("(", "");
			text = text.Replace(")", "");
			if (text.Length == 0)
			{
				return ControlsResources.Graphing_NodeDisplayPropertiesName1.Fmt(ID);
			}
			return ControlsResources.Graphing_NodeDisplayPropertiesName2.Fmt(text, ID);
		}
		set
		{
		}
	}

	[DisplayOrder(2)]
	[DisplayNameDescription("OperationDescriptionShort", "OperationDescription")]
	[Editor(typeof(DiffImageUITypeEditor), typeof(UITypeEditor))]
	internal string Description => NodeOriginal.Operation.Description;

	[Browsable(false)]
	internal bool IsParallel
	{
		get
		{
			object obj = NodeOriginal["Parallel"];
			if (obj == null)
			{
				return false;
			}
			return (bool)obj;
		}
	}

	[Browsable(false)]
	internal bool HasWarnings => NodeOriginal["Warnings"] != null;

	private bool HasCriticalWarnings
	{
		get
		{
			if (NodeOriginal["Warnings"] != null)
			{
				ExpandableObjectWrapper expandableObjectWrapper = NodeOriginal["Warnings"] as ExpandableObjectWrapper;
				if (expandableObjectWrapper["NoJoinPredicate"] != null)
				{
					return (bool)expandableObjectWrapper["NoJoinPredicate"];
				}
			}
			return false;
		}
	}

	private bool HasPDWCost => NodeOriginal["PDWAccumulativeCost"] != null;

	[ShowInToolTip]
	[DisplayOrder(8)]
	[DisplayNameDescription("EstimatedOperatorCost", "EstimatedOperatorCostDescription")]
	[Editor(typeof(DiffImageUITypeEditor), typeof(UITypeEditor))]
	internal string DisplayCost
	{
		get
		{
			double num = NodeOriginal.RelativeCost * 100.0;
			if (HasPDWCost && num <= 0.0)
			{
				return "";
			}
			return ControlsResources.Graphing_OperatorDisplayCost.Fmt(NodeOriginal.Cost, (int)Math.Round(num));
		}
	}

	[ShowInToolTip]
	[DisplayOrder(9)]
	[DisplayNameDescription("EstimatedSubtreeCost", "EstimatedSubtreeCostDescription")]
	[TypeConverter(typeof(FloatTypeConverter))]
	[Editor(typeof(DiffImageUITypeEditor), typeof(UITypeEditor))]
	internal double SubtreeCost => NodeOriginal.SubtreeCost;

	internal string HelpKeyword => NodeOriginal.Operation.HelpKeyword;

	public new BlackbirdSql.Shared.Controls.Graphing.Node NodeOriginal => (BlackbirdSql.Shared.Controls.Graphing.Node)base.NodeOriginal;

	private NodeDisplayProperties DisplayProperties => _nodedisplayproperties;

	private Image ParallelProcessingImage
	{
		get
		{
			parallelProcessingImage ??= ControlsResources.IconParallelProcess.ToBitmap();
			return parallelProcessingImage;
		}
	}

	private Image WarningImage
	{
		get
		{
			warningImage ??= ControlsResources.IconWarning.ToBitmap();
			return warningImage;
		}
	}

	private Image CriticalWarningImage
	{
		get
		{
			criticalWarningImage ??= ControlsResources.IconErrorOffset32x.ToBitmap();
			return criticalWarningImage;
		}
	}

	public NodeDisplay(GraphCtrl graphctrl, IGraphModify igraphmodify, INodeDisplayProperties inodedisplaypropertiesDefault, IList ilistDisplayable, INode inodeOriginal)
		: base(graphctrl, igraphmodify, inodedisplaypropertiesDefault, ilistDisplayable, inodeOriginal)
	{
		UseBackgroundColor = false;
		_Children = new NodeCollection<NodeDisplay>(this);
		diffMap ??= new Dictionary<PropertyDescriptor, bool>();
		TextFormat = new()
		{
			Alignment = StringAlignment.Center,
			Trimming = StringTrimming.EllipsisCharacter
		};
	}

	internal void CompareAndUpdateDiffMaps(NodeDisplay ndCompareTo)
	{
		PropertyDescriptorCollection propertyDescriptorCollection = InitDiffMap(initialDiff: true);
		PropertyDescriptorCollection propertyDescriptorCollection2 = ndCompareTo.InitDiffMap(initialDiff: true);
		for (int i = 0; i < propertyDescriptorCollection.Count; i++)
		{
			if (!DiffMap.ContainsKey(propertyDescriptorCollection[i]) || !ndCompareTo.DiffMap.ContainsKey(propertyDescriptorCollection[i]))
			{
				continue;
			}
			int num = propertyDescriptorCollection2.IndexOf(propertyDescriptorCollection[i]);
			if (num < 0 || num >= propertyDescriptorCollection2.Count || propertyDescriptorCollection[i] is not PropertyValue || propertyDescriptorCollection2[num] is not PropertyValue)
			{
				continue;
			}
			PropertyValue propertyValue = (PropertyValue)propertyDescriptorCollection[i];
			PropertyValue propertyValue2 = (PropertyValue)propertyDescriptorCollection2[num];
			if (propertyValue.Value == null || propertyValue2.Value == null)
			{
				if (propertyValue.Value == null && propertyValue2.Value == null)
				{
					DiffMap[propertyDescriptorCollection[i]] = false;
					ndCompareTo.DiffMap[propertyDescriptorCollection2[num]] = false;
				}
			}
			else if (string.Compare(propertyValue.Value.ToString(), propertyValue2.Value.ToString()) == 0)
			{
				DiffMap[propertyDescriptorCollection[i]] = false;
				ndCompareTo.DiffMap[propertyDescriptorCollection2[num]] = false;
			}
		}
	}

	internal PropertyDescriptor GetArgumentProperty()
	{
		PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(this);
		string[] argumentPropertyNames = ArgumentPropertyNames;
		foreach (string name in argumentPropertyNames)
		{
			PropertyDescriptor propertyDescriptor = properties[name];
			if (propertyDescriptor != null)
			{
				return propertyDescriptor;
			}
		}
		return null;
	}

	void IRenderable.Render(Graphics graphics)
	{
		DoRender(graphics);
	}

	protected virtual void DoRender(Graphics graphics)
	{
		GraphControl graphControl = (GraphControl)ParentGraph;
		Rectangle drawRectangle = GetDrawRectangle();
		int width = (drawRectangle.Width - IconSize.Width) / 2;
		Rectangle rectangle = new Rectangle(drawRectangle.Location + new Size(width, 0), IconSize);
		graphics.DrawImage(Image, rectangle);
		Image overlayImage = GetOverlayImage();
		if (overlayImage != null)
		{
			Rectangle rect = rectangle;
			rect.Offset(3, 3);
			graphics.DrawImage(overlayImage, rect);
		}
		Rectangle rectangle2 = Rectangle.FromLTRB(drawRectangle.Left, rectangle.Bottom + TextMargin.Height, drawRectangle.Right, drawRectangle.Bottom);
		rectangle2.Inflate(0, -TextMargin.Height);
		bool flag = Selected && graphControl.IsActive;
		bool flag2 = false;
		SolidBrush solidBrush = new SolidBrush(SystemColors.Highlight);
		int num = rectangle2.Width + 2 * highlightBorderWidth;
		_ = BackgroundColor;
		Brush brush;
		if (BackgroundColor.IsEmpty || !UseBackgroundColor)
		{
			brush = !flag ? new SolidBrush(SystemColors.WindowText) : new SolidBrush(SystemColors.HighlightText);
		}
		else
		{
			flag2 = true;
			if (flag)
			{
				int height = rectangle2.Height + 2 * highlightBorderWidth;
				Rectangle rect2 = new Rectangle(rectangle2.X - highlightBorderWidth, rectangle2.Y - highlightBorderWidth, num, height);
				graphics.FillRectangle(solidBrush, rect2);
			}
			_ = TextColor;
			brush = TextColor.IsEmpty ? new SolidBrush(SystemColors.WindowText) : new SolidBrush(TextColor);
		}
		Font font = graphControl.Font;
		RectangleF rectangleF = rectangle2;
		SolidBrush solidBrush2 = new SolidBrush(BackgroundColor);
		string[] displayLinesOfText = GetDisplayLinesOfText();
		foreach (string text in displayLinesOfText)
		{
			if (text.Length <= 0)
			{
				continue;
			}
			if (flag2)
			{
				if (flag)
				{
					float height2 = rectangleF.Height + highlightBorderWidth;
					RectangleF rect3 = new RectangleF(rectangleF.X - highlightBorderWidth, rectangleF.Y, num, height2);
					graphics.FillRectangle(solidBrush, rect3);
				}
				graphics.FillRectangle(solidBrush2, rectangleF);
			}
			else if (flag)
			{
				graphics.FillRectangle(solidBrush, rectangleF);
			}
			rectangleF.Height = (float)Math.Ceiling(graphics.MeasureString(text, font).Height);
			graphics.DrawString(text, font, brush, rectangleF, TextFormat);
			rectangleF.Y = rectangleF.Bottom;
		}
		brush?.Dispose();
		solidBrush?.Dispose();
		solidBrush2.Dispose();
	}

	AttributeCollection ICustomTypeDescriptor.GetAttributes()
	{
		return TypeDescriptor.GetAttributes(GetType());
	}

	EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
	{
		return TypeDescriptor.GetDefaultEvent(GetType());
	}

	PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
	{
		return TypeDescriptor.GetDefaultProperty(GetType());
	}

	object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
	{
		return TypeDescriptor.GetEditor(GetType(), editorBaseType);
	}

	EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
	{
		return TypeDescriptor.GetEvents(GetType());
	}

	EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
	{
		return TypeDescriptor.GetEvents(GetType(), attributes);
	}

	object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
	{
		return this;
	}

	PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
	{
		return InternalGetProperties(TypeDescriptor.GetProperties(GetType()));
	}

	PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
	{
		return InternalGetProperties(TypeDescriptor.GetProperties(GetType(), attributes));
	}

	string ICustomTypeDescriptor.GetComponentName()
	{
		return DisplayName.Split('\n')[0];
	}

	TypeConverter ICustomTypeDescriptor.GetConverter()
	{
		return TypeDescriptor.GetConverter(GetType());
	}

	string ICustomTypeDescriptor.GetClassName()
	{
		return "";
	}

	internal override void UpdateBoundingRectangle(Graphics graphics)
	{
		if (DisplayProperties.FixedRectangleSize == DisplayPropertiesDefault.FixedRectangleSize)
		{
			DisplayProperties.FixedRectangleSize = MeasureSize(graphics);
			DisplayProperties.FixedSizeBoundingRectangle = true;
		}
		base.UpdateBoundingRectangle(graphics);
	}

	internal override Point ConnectionPoint(Point sourcePoint)
	{
		return new Point(sourcePoint.X < BoundingRect.Left ? BoundingRect.Left : BoundingRect.Right, (BoundingRect.Top + BoundingRect.Bottom) / 2);
	}

	internal Rectangle GetDrawRectangle()
	{
		return Rectangle.Inflate(BoundingRect, -1, -Margin.Height);
	}

	private bool IsParentHierarchyTreeStructure(BlackbirdSql.Shared.Controls.Graphing.NodeDisplay node)
	{
		while (node != null)
		{
			if (node.Children.Count >= 2)
			{
				return true;
			}
			node = node.Parent;
		}
		return false;
	}

	internal int InitializeEdges()
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		float num4 = 0f;
		IEdgeEnumerator edgesRelated = EdgesRelated;
		while (edgesRelated.MoveNext())
		{
			BlackbirdSql.Shared.Controls.Graphing.EdgeDisplay edgeDisplay = edgesRelated.Current as BlackbirdSql.Shared.Controls.Graphing.EdgeDisplay;
			if (edgeDisplay.NodeFrom == this)
			{
				num += edgeDisplay.ExtendedWidth;
				num2 = Math.Max(num2, edgeDisplay.ExtendedWidth);
				num3 = Math.Max(num3, edgeDisplay.NodeTo.BoundingRect.Width);
				num4 = Math.Max(num4, edgeDisplay.ArrowWidth);
			}
		}
		int minimalMidpointOffsetFromTheEnd = num3 / 2 + num2;
		int num5 = BoundingRect.Width / 2 + (int)num4 + num + num3 / 2;
		if (Children.Count > 1)
		{
			int num6 = -(num / 2);
			int num7 = 0;
			edgesRelated.Reset();
			while (edgesRelated.MoveNext())
			{
				BlackbirdSql.Shared.Controls.Graphing.EdgeDisplay edgeDisplay2 = edgesRelated.Current as BlackbirdSql.Shared.Controls.Graphing.EdgeDisplay;
				if (edgeDisplay2.NodeFrom == this)
				{
					edgeDisplay2.ConnectionOffset = new Size(0, num6 + edgeDisplay2.ExtendedWidth / 2);
					edgeDisplay2.MinimalMidpointOffsetFromTheEnd = minimalMidpointOffsetFromTheEnd;
					num6 += edgeDisplay2.ExtendedWidth;
					num7 = Math.Max(num7, edgeDisplay2.NodeTo.BoundingRect.Width);
				}
			}
			if (IsParentHierarchyTreeStructure(Parent))
			{
				num5 += Math.Max(num7 - 200, 0);
			}
		}
		return num5;
	}

	private PropertyDescriptorCollection InternalGetProperties(PropertyDescriptorCollection properties)
	{
		List<PropertyValue> list = new List<PropertyValue>();
		foreach (PropertyDescriptor property in properties)
		{
			foreach (Attribute attribute in property.Attributes)
			{
				if (attribute is DisplayNameDescriptionAttribute)
				{
					object value = property.GetValue(this);
					if (value != null)
					{
						list.Add(new PropertyValue(property, value));
					}
				}
			}
		}
		foreach (PropertyValue property2 in NodeOriginal.Properties)
		{
			if (property2.IsBrowsable)
			{
				list.Add(property2);
			}
		}
		PropertyDescriptor[] properties2 = list.ToArray();
		return new PropertyDescriptorCollection(properties2).Sort(PropertyValue.OrderComparer.Default);
	}

	protected Size MeasureSize(Graphics graphics)
	{
		int val = IconSize.Width;
		int num = 0;
		Font font = (ParentGraph as Control).Font;
		string[] displayLinesOfText = GetDisplayLinesOfText();
		foreach (string text in displayLinesOfText)
		{
			if (text.Length > 0)
			{
				SizeF sizeF = graphics.MeasureString(text, font);
				num += (int)Math.Ceiling(sizeF.Height);
				val = Math.Max(val, (int)Math.Ceiling(sizeF.Width));
			}
		}
		val = Math.Min(val, 300);
		return new Size(val, IconSize.Height + num) + Margin + Margin + TextMargin + TextMargin;
	}

	private string GetElapsedTimeDisplayString()
	{
		string result = null;
		if (NodeOriginal["ActualTimeStatistics"] is ExpandableObjectWrapper expandableObjectWrapper && expandableObjectWrapper["ActualElapsedms"] is RunTimeCounters runTimeCounters)
		{
			long num = (long)(runTimeCounters.MaxCounter * 10000);
			DateTime dateTime = new DateTime(num);
			result = num >= 600000000 ? num / 36000000000L + dateTime.ToString(":mm:ss") : dateTime.ToString("s.fff") + "s";
		}
		return result;
	}

	private string GetRowStatisticsDisplayString()
	{
		ulong? actualRows = NodeOriginal[NodeBuilderConstants.ActualRows] is RunTimeCounters runTimeCounters ? new ulong?(runTimeCounters.TotalCounters) : null;
		double? num = NodeOriginal[NodeBuilderConstants.EstimateRows] as double?;
		double? num2 = NodeOriginal[NodeBuilderConstants.EstimateExecutions] as double?;
		if (num.HasValue)
		{
			if (num2.HasValue)
			{
				num *= num2;
			}
			num = Math.Round(num.Value);
		}
		return GetRowStatisticsDisplayString(actualRows, num);
	}

	private string GetRowStatisticsDisplayString(ulong? actualRows, double? estimateRows)
	{
		if (!actualRows.HasValue || !estimateRows.HasValue)
		{
			return null;
		}
		estimateRows = estimateRows > 0.0 ? estimateRows : new double?(1.0);
		string text = actualRows.Value.ToString();
		string text2 = estimateRows.Value.ToString();
		int num = 100;
		if (estimateRows > 0.0)
		{
			num = (int)(100.0 * (actualRows.Value / estimateRows)).Value;
		}
		text = text.PadLeft(text2.Length);
		text2 = text2.PadLeft(text.Length);
		return text.Fmt(text2, num);
	}

	protected virtual string[] GetDisplayLinesOfText()
	{
		string text = DisplayName;
		double num = NodeOriginal.RelativeCost * 100.0;
		if (!HasPDWCost || num > 0.0)
		{
			string text2 = ControlsResources.Graphing_CostFormat.Fmt((int)Math.Round(num));
			text = text + "\n" + text2;
		}
		string elapsedTimeDisplayString = GetElapsedTimeDisplayString();
		if (!string.IsNullOrEmpty(elapsedTimeDisplayString))
		{
			text = text + "\n" + elapsedTimeDisplayString;
		}
		string rowStatisticsDisplayString = GetRowStatisticsDisplayString();
		if (!string.IsNullOrEmpty(rowStatisticsDisplayString))
		{
			text = text + "\n" + rowStatisticsDisplayString;
		}
		return text.Split('\n');
	}

	private Image GetOverlayImage()
	{
		if (HasCriticalWarnings)
		{
			return CriticalWarningImage;
		}
		if (HasWarnings)
		{
			return WarningImage;
		}
		if (IsParallel)
		{
			return ParallelProcessingImage;
		}
		return null;
	}

	private string GetObjectNameForDisplay(object objectProperty)
	{
		string result = "";
		if (objectProperty != null)
		{
			result = objectProperty.ToString();
			if (objectProperty is ExpandableObjectWrapper expandableObjectWrapper)
			{
				result = ObjectWrapperTypeConverter.MergeString(".", expandableObjectWrapper["Table"], expandableObjectWrapper["Index"]);
				result = ObjectWrapperTypeConverter.MergeString(" ", result, expandableObjectWrapper["Alias"]);
			}
		}
		return result;
	}

	private void AddPropertyToDiffMap(PropertyDescriptor pD, bool isDiff)
	{
		if (!diffMap.ContainsKey(pD))
		{
			diffMap.Add(pD, isDiff);
		}
	}

	private PropertyDescriptorCollection InitDiffMap(bool initialDiff)
	{
		diffMap.Clear();
		PropertyDescriptorCollection properties = ((ICustomTypeDescriptor)this).GetProperties();
		for (int i = 0; i < properties.Count; i++)
		{
			AddPropertyToDiffMap(properties[i], initialDiff);
		}
		return properties;
	}

	static NodeDisplay()
	{
		DisplayPropertiesDefault = new NodeDisplayProperties();
		Margin = new Size(2, 4);
		TextMargin = new Size(2, 2);
		ArgumentPropertyNames = new string[4] { "Warnings", "Argument", "Object", "StatementText" };
		DisplayPropertiesDefault.ENodeStyle = ENodeStyle.Rectangle;
		DisplayPropertiesDefault.FixedSizeBoundingRectangle = true;
		int scaledImageSize = ControlUtils.GetScaledImageSize(32);
		IconSize = new Size(scaledImageSize, scaledImageSize);
	}
}
