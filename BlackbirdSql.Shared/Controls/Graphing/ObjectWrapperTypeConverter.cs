// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.ObjectWrapperTypeConverter
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using BlackbirdSql.Shared.Controls.Graphing.ComponentModel;
using BlackbirdSql.Shared.Controls.Graphing.Enums;
using BlackbirdSql.Shared.Controls.Graphing.Gram;
using BlackbirdSql.Shared.Properties;

namespace BlackbirdSql.Shared.Controls.Graphing;

internal class ObjectWrapperTypeConverter : ExpandableObjectConverter
{
	public static ObjectWrapperTypeConverter Default;

	private static readonly Dictionary<Type, MethodInfo> convertMethods;

	public static ExpandableObjectWrapper ConvertToWrapperObject(object item)
	{
		ExpandableObjectWrapper expandableObjectWrapper = new ExpandableObjectWrapper(item);
		expandableObjectWrapper.DisplayName = MakeDisplayNameFromObjectNamesAndValues(expandableObjectWrapper);
		return expandableObjectWrapper;
	}

	public static ExpandableObjectWrapper Convert(ColumnReferenceType item)
	{
		string displayName = MergeString(".", item.Database, item.Schema, item.Table, item.Column);
		return new ExpandableObjectWrapper(item, "Column", displayName);
	}

	public static ExpandableObjectWrapper Convert(GroupingSetReferenceType item)
	{
		string value = item.Value;
		return new ExpandableObjectWrapper(item, "GroupingSet", value);
	}

	public static ExpandableObjectWrapper Convert(ObjectType item)
	{
		string text = MergeString(".", item.Server, item.Database, item.Schema, item.Table, item.Index);
		text = MergeString(" ", text, item.Alias);
		if (item.CloneAccessScopeSpecified)
		{
			string text2 = Convert(item.CloneAccessScope);
			text = MergeString(" ", text, text2);
		}
		return new ExpandableObjectWrapper(item, "Index", text);
	}

	public static ExpandableObjectWrapper Convert(SingleColumnReferenceType item)
	{
		return Convert(item.ColumnReference);
	}

	public static ExpandableObjectWrapper Convert(DefinedValuesListTypeDefinedValue[] definedValues)
	{
		ExpandableObjectWrapper expandableObjectWrapper = new ExpandableObjectWrapper();
		StringBuilder stringBuilder = new StringBuilder();
		foreach (DefinedValuesListTypeDefinedValue definedValuesListTypeDefinedValue in definedValues)
		{
			if (definedValuesListTypeDefinedValue.Item == null)
			{
				continue;
			}
			string text = Default.ConvertFrom(definedValuesListTypeDefinedValue.Item).ToString();
			if (text.Length == 0 || expandableObjectWrapper[text] != null)
			{
				continue;
			}
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(CultureInfo.CurrentCulture.TextInfo.ListSeparator);
				stringBuilder.Append(" ");
			}
			if (definedValuesListTypeDefinedValue.Items == null || definedValuesListTypeDefinedValue.Items.Length == 0)
			{
				stringBuilder.Append(text);
				expandableObjectWrapper[text] = string.Empty;
				continue;
			}
			object obj = ConvertToObjectWrapper(definedValuesListTypeDefinedValue.Items);
			if (definedValuesListTypeDefinedValue.Items.Length > 1)
			{
				stringBuilder.AppendFormat(CultureInfo.CurrentCulture, "[{0}] = ({1})", text, obj);
			}
			else
			{
				stringBuilder.AppendFormat(CultureInfo.CurrentCulture, "[{0}] = {1}", text, obj);
			}
			expandableObjectWrapper[text] = obj;
		}
		expandableObjectWrapper.DisplayName = stringBuilder.ToString();
		return expandableObjectWrapper;
	}

	public static ExpandableObjectWrapper Convert(SetOptionsType item)
	{
		return ConvertToWrapperObject(item);
	}

	public static ExpandableObjectWrapper Convert(RollupLevelType item)
	{
		return new ExpandableObjectWrapper(item, ControlsResources.Level, item.Level.ToString());
	}

	public static ExpandableObjectWrapper Convert(WarningsType item)
	{
		string displayName = string.Empty;
		ExpandableObjectWrapper expandableObjectWrapper = new ExpandableObjectWrapper(item);
		ProcessSpillOccurred(expandableObjectWrapper, ref displayName);
		ProcessColumnWithNoStatistics(expandableObjectWrapper, ref displayName);
		ProcessNoJoinPredicate(expandableObjectWrapper, ref displayName);
		ProcessSpillToTempDb(expandableObjectWrapper, ref displayName);
		ProcessHashSpillDetails(expandableObjectWrapper, ref displayName);
		ProcessSortSpillDetails(expandableObjectWrapper, ref displayName);
		ProcessWaits(expandableObjectWrapper, ref displayName);
		ProcessPlanAffectingConvert(expandableObjectWrapper, ref displayName);
		ProcessMemoryGrantWarning(expandableObjectWrapper, ref displayName);
		ProcessFullUpdateForOnlineIndexBuild(expandableObjectWrapper, ref displayName);
		if (expandableObjectWrapper["FullUpdateForOnlineIndexBuild"] != null)
		{
			displayName = MergeString(CultureInfo.CurrentCulture.TextInfo.ListSeparator + " ", displayName, ControlsResources.FullUpdateForOnlineIndexBuild);
		}
		expandableObjectWrapper.DisplayName = displayName;
		return expandableObjectWrapper;
	}

	private static void ProcessWaits(ExpandableObjectWrapper wrapper, ref string displayName)
	{
		if (wrapper["Wait"] == null)
		{
			return;
		}
		List<ExpandableObjectWrapper> propertyList = GetPropertyList(wrapper, "Wait");
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		foreach (ExpandableObjectWrapper item in propertyList)
		{
			string s = (item.Properties["WaitTime"] as PropertyValue).Value.ToString();
			string key = (item.Properties["WaitType"] as PropertyValue).Value.ToString();
			if (dictionary.ContainsKey(key))
			{
				dictionary[key] += int.Parse(s);
			}
			else
			{
				dictionary.Add(key, int.Parse(s));
			}
		}
		foreach (KeyValuePair<string, int> item2 in dictionary)
		{
			string text = string.Format(ControlsResources.Wait, item2.Value, item2.Key);
			displayName = MergeString(CultureInfo.CurrentCulture.TextInfo.ListSeparator + " ", displayName, text);
		}
	}

	private static void ProcessSpillToTempDb(ExpandableObjectWrapper wrapper, ref string displayName)
	{
		if (wrapper["SpillToTempDb"] == null)
		{
			return;
		}
		foreach (ExpandableObjectWrapper property in GetPropertyList(wrapper, "SpillToTempDb"))
		{
			PropertyValue propertyValue = property.Properties["SpillLevel"] as PropertyValue;
			string arg = propertyValue.Value.ToString();
			string text = ((property.Properties["SpilledThreadCount"] is PropertyValue propertyValue2) ? string.Format(CultureInfo.CurrentCulture, ControlsResources.SpillToTempDb, arg, propertyValue2.Value.ToString()) : string.Format(CultureInfo.CurrentCulture, ControlsResources.SpillToTempDbOld, arg));
			displayName = MergeString(CultureInfo.CurrentCulture.TextInfo.ListSeparator + " ", displayName, text);
		}
	}

	private static void GetCommonSpillDetails(ExpandableObjectWrapper eow, out string grantedMemory, out string usedMemory, out string writes, out string reads)
	{
		PropertyValue propertyValue = eow.Properties["GrantedMemoryKb"] as PropertyValue;
		grantedMemory = propertyValue.Value.ToString();
		propertyValue = eow.Properties["UsedMemoryKb"] as PropertyValue;
		usedMemory = propertyValue.Value.ToString();
		propertyValue = eow.Properties["WritesToTempDb"] as PropertyValue;
		writes = propertyValue.Value.ToString();
		propertyValue = eow.Properties["ReadsFromTempDb"] as PropertyValue;
		reads = propertyValue.Value.ToString();
	}

	private static void ProcessHashSpillDetails(ExpandableObjectWrapper wrapper, ref string displayName)
	{
		if (wrapper["HashSpillDetails"] == null)
		{
			return;
		}
		foreach (ExpandableObjectWrapper property in GetPropertyList(wrapper, "HashSpillDetails"))
		{
			GetCommonSpillDetails(property, out var grantedMemory, out var usedMemory, out var writes, out var reads);
			string text = string.Format(ControlsResources.HashSpillDetails, writes, reads, grantedMemory, usedMemory);
			displayName = MergeString(CultureInfo.CurrentCulture.TextInfo.ListSeparator + " ", displayName, text);
		}
	}

	private static void ProcessSortSpillDetails(ExpandableObjectWrapper wrapper, ref string displayName)
	{
		if (wrapper["SortSpillDetails"] == null)
		{
			return;
		}
		foreach (ExpandableObjectWrapper property in GetPropertyList(wrapper, "SortSpillDetails"))
		{
			GetCommonSpillDetails(property, out var grantedMemory, out var usedMemory, out var writes, out var reads);
			string text = string.Format(ControlsResources.SortSpillDetails, writes, reads, grantedMemory, usedMemory);
			displayName = MergeString(CultureInfo.CurrentCulture.TextInfo.ListSeparator + " ", displayName, text);
		}
	}

	private static void ProcessSpillOccurred(ExpandableObjectWrapper wrapper, ref string displayName)
	{
		if (wrapper["SpillOccurred"] != null)
		{
			displayName = MergeString(CultureInfo.CurrentCulture.TextInfo.ListSeparator + " ", displayName, ControlsResources.SpillOccurredDisplayString);
		}
	}

	private static void ProcessNoJoinPredicate(ExpandableObjectWrapper wrapper, ref string displayName)
	{
		if (object.Equals(wrapper["NoJoinPredicate"], true))
		{
			displayName = MergeString(CultureInfo.CurrentCulture.TextInfo.ListSeparator + " ", displayName, ControlsResources.NoJoinPredicate);
		}
	}

	private static void ProcessColumnWithNoStatistics(ExpandableObjectWrapper wrapper, ref string displayName)
	{
		if (wrapper["ColumnsWithNoStatistics"] != null)
		{
			string value = ((wrapper["ColumnsWithNoStatistics"] as ExpandableObjectWrapper).Properties["ColumnReference"] as PropertyValue).Value.ToString();
			displayName = string.Format(ControlsResources.NameValuePair, ControlsResources.ColumnsWithNoStatistics, value);
		}
	}

	private static void ProcessFullUpdateForOnlineIndexBuild(ExpandableObjectWrapper wrapper, ref string displayName)
	{
		if (wrapper["FullUpdateForOnlineIndexBuild"] != null)
		{
			displayName = MergeString(CultureInfo.CurrentCulture.TextInfo.ListSeparator + " ", displayName, ControlsResources.FullUpdateForOnlineIndexBuild);
		}
	}

	private static void ProcessPlanAffectingConvert(ExpandableObjectWrapper wrapper, ref string displayName)
	{
		if (wrapper["PlanAffectingConvert"] == null)
		{
			return;
		}
		foreach (ExpandableObjectWrapper property in GetPropertyList(wrapper, "PlanAffectingConvert"))
		{
			string arg = (property.Properties["ConvertIssue"] as PropertyValue).Value.ToString();
			string arg2 = (property.Properties["Expression"] as PropertyValue).Value.ToString();
			string text = string.Format(ControlsResources.PlanAffectingConvert, arg2, arg);
			displayName = MergeString(CultureInfo.CurrentCulture.TextInfo.ListSeparator + " ", displayName, text);
		}
	}

	private static void ProcessMemoryGrantWarning(ExpandableObjectWrapper wrapper, ref string displayName)
	{
		if (wrapper["MemoryGrantWarning"] == null)
		{
			return;
		}
		foreach (ExpandableObjectWrapper property in GetPropertyList(wrapper, "MemoryGrantWarning"))
		{
			if (property.Properties["GrantWarningKind"] is PropertyValue propertyValue
				&& property.Properties["GrantedMemory"] is PropertyValue propertyValue3
				&& property.Properties["RequestedMemory"] is PropertyValue propertyValue2
				&& property.Properties["MaxUsedMemory"] is PropertyValue propertyValue4)
			{
				string text = string.Format(ControlsResources.MemoryGrantWarning, propertyValue.Value.ToString(), propertyValue2.Value.ToString(), propertyValue3.Value.ToString(), propertyValue4.Value.ToString());
				displayName = MergeString(CultureInfo.CurrentCulture.TextInfo.ListSeparator + " ", displayName, text);
			}
		}
	}

	private static List<ExpandableObjectWrapper> GetPropertyList(ExpandableObjectWrapper wrapper, string propertyName)
	{
		List<ExpandableObjectWrapper> list = new List<ExpandableObjectWrapper>();
		foreach (PropertyDescriptor property in wrapper.Properties)
		{
			if (property.Name == propertyName)
			{
				PropertyValue propertyValue = property as PropertyValue;
				list.Add(propertyValue.Value as ExpandableObjectWrapper);
			}
		}
		return list;
	}

	public static ExpandableObjectWrapper Convert(MemoryFractionsType item)
	{
		return ConvertToWrapperObject(item);
	}

	public static ExpandableObjectWrapper Convert(ScalarType[][] items)
	{
		ExpandableObjectWrapper expandableObjectWrapper = new ExpandableArrayWrapper(items);
		string value = CultureInfo.CurrentCulture.TextInfo.ListSeparator + " ";
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < items.Length; i++)
		{
			if (i != 0)
			{
				stringBuilder.Append(value);
			}
			stringBuilder.Append("(");
			for (int j = 0; j < items[i].Length; j++)
			{
				if (j != 0)
				{
					stringBuilder.Append(value);
				}
				stringBuilder.Append(Convert(items[i][j]));
			}
			stringBuilder.Append(")");
		}
		expandableObjectWrapper.DisplayName = stringBuilder.ToString();
		return expandableObjectWrapper;
	}

	public static ExpandableObjectWrapper Convert(ScalarType item)
	{
		ExpandableObjectWrapper expandableObjectWrapper = new ExpandableObjectWrapper(item);
		if (!string.IsNullOrEmpty(item.ScalarString))
		{
			expandableObjectWrapper.DisplayName = string.Format(CultureInfo.CurrentCulture, "{0}({1})", ControlsResources.ScalarOperator, item.ScalarString);
		}
		else
		{
			expandableObjectWrapper.DisplayName = ControlsResources.ScalarOperator;
		}
		return expandableObjectWrapper;
	}

	public static string Convert(ScalarExpressionType item)
	{
		if (item.ScalarOperator == null)
		{
			return string.Empty;
		}
		return item.ScalarOperator.ScalarString;
	}

	public static ExpandableObjectWrapper Convert(CompareType item)
	{
		ExpandableObjectWrapper expandableObjectWrapper = new ExpandableObjectWrapper(item);
		object obj = expandableObjectWrapper["ScalarOperator"];
		if (obj != null)
		{
			expandableObjectWrapper.DisplayName = item.CompareOp.ToString();
		}
		else
		{
			expandableObjectWrapper.DisplayName = string.Format(CultureInfo.CurrentCulture, "{0}({1})", item.CompareOp, obj);
		}
		return expandableObjectWrapper;
	}

	public static ExpandableObjectWrapper Convert(OrderByTypeOrderByColumn item)
	{
		ExpandableObjectWrapper expandableObjectWrapper = new ExpandableObjectWrapper(item);
		expandableObjectWrapper.DisplayName = string.Format(CultureInfo.CurrentCulture, "{0} {1}", expandableObjectWrapper["ColumnReference"], item.Ascending ? ControlsResources.Ascending : ControlsResources.Descending);
		return expandableObjectWrapper;
	}

	public static ExpandableObjectWrapper Convert(ScanRangeType item)
	{
		ExpandableObjectWrapper expandableObjectWrapper = new ExpandableObjectWrapper(item);
		object obj = expandableObjectWrapper["RangeColumns"];
		object obj2 = expandableObjectWrapper["RangeExpressions"];
		if (obj != null && obj2 != null)
		{
			string text = string.Empty;
			switch (item.ScanType)
			{
			case EnCompareOpType.EQ:
				text = "=";
				break;
			case EnCompareOpType.GE:
				text = ">=";
				break;
			case EnCompareOpType.GT:
				text = ">";
				break;
			case EnCompareOpType.LE:
				text = "<=";
				break;
			case EnCompareOpType.LT:
				text = "<";
				break;
			case EnCompareOpType.NE:
				text = "<>";
				break;
			}
			if (text.Length > 0)
			{
				expandableObjectWrapper.DisplayName = string.Format(CultureInfo.CurrentCulture, "{0} {1} {2}", obj, text, obj2);
			}
			else
			{
				expandableObjectWrapper.DisplayName = string.Format(CultureInfo.CurrentCulture, "{0}({1})", item.ScanType, MergeString(",", obj, obj2));
			}
		}
		return expandableObjectWrapper;
	}

	public static ExpandableObjectWrapper Convert(SeekPredicateType item)
	{
		ExpandableObjectWrapper expandableObjectWrapper = new ExpandableObjectWrapper(item);
		expandableObjectWrapper.DisplayName = MakeDisplayNameFromObjectNamesAndValues(expandableObjectWrapper, "Prefix", "StartRange", "EndRange");
		return expandableObjectWrapper;
	}

	public static ExpandableObjectWrapper Convert(SeekPredicatesType item)
	{
		ExpandableObjectWrapper expandableObjectWrapper;
		if (item.Items.Length != 0 && item.Items[0] is SeekPredicateNewType)
		{
			expandableObjectWrapper = new ExpandableArrayWrapper(item.Items);
			string value = CultureInfo.CurrentCulture.TextInfo.ListSeparator + " ";
			PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(expandableObjectWrapper);
			if (properties.Count > 1)
			{
				StringBuilder stringBuilder = new StringBuilder();
				for (int i = 0; i < properties.Count; i++)
				{
					if (i != 0)
					{
						stringBuilder.Append(value);
					}
					stringBuilder.Append(string.Format(CultureInfo.CurrentCulture, "{0} {1}", properties[i].DisplayName, properties[i].GetValue(expandableObjectWrapper).ToString()));
				}
				expandableObjectWrapper.DisplayName = stringBuilder.ToString();
			}
		}
		else
		{
			expandableObjectWrapper = new ExpandableArrayWrapper(item.Items);
		}
		return expandableObjectWrapper;
	}

	public static ExpandableObjectWrapper Convert(SeekPredicateNewType item)
	{
		ExpandableObjectWrapper expandableObjectWrapper = new ExpandableArrayWrapper(item.SeekKeys);
		string value = CultureInfo.CurrentCulture.TextInfo.ListSeparator + " ";
		StringBuilder stringBuilder = new StringBuilder();
		PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(expandableObjectWrapper);
		for (int i = 0; i < properties.Count; i++)
		{
			if (i != 0)
			{
				stringBuilder.Append(value);
			}
			stringBuilder.Append(string.Format(CultureInfo.CurrentCulture, "{0}[{1}]: {2}", ControlsResources.SeekKeys, i + 1, properties[i].GetValue(expandableObjectWrapper).ToString()));
		}
		expandableObjectWrapper.DisplayName = stringBuilder.ToString();
		return expandableObjectWrapper;
	}

	public static ExpandableObjectWrapper Convert(SeekPredicatePartType item)
	{
		ExpandableObjectWrapper expandableObjectWrapper = new ExpandableArrayWrapper(item.Items);
		string value = CultureInfo.CurrentCulture.TextInfo.ListSeparator + " ";
		PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(expandableObjectWrapper);
		if (properties.Count > 1)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < properties.Count; i++)
			{
				if (i != 0)
				{
					stringBuilder.Append(value);
				}
				stringBuilder.Append(string.Format(CultureInfo.CurrentCulture, "{0} {1}", properties[i].DisplayName, properties[i].GetValue(expandableObjectWrapper).ToString()));
			}
			expandableObjectWrapper.DisplayName = stringBuilder.ToString();
		}
		return expandableObjectWrapper;
	}

	public static ExpandableObjectWrapper Convert(MergeColumns item)
	{
		ExpandableObjectWrapper expandableObjectWrapper = new ExpandableObjectWrapper(item);
		object obj = expandableObjectWrapper["InnerSideJoinColumns"];
		object obj2 = expandableObjectWrapper["OuterSideJoinColumns"];
		if (obj != null && obj2 != null)
		{
			expandableObjectWrapper.DisplayName = string.Format(CultureInfo.CurrentCulture, "({0}) = ({1})", obj, obj2);
		}
		return expandableObjectWrapper;
	}

	public static string Convert(EnStmtInfoTypeStatementOptmEarlyAbortReason item)
	{
		return item switch
		{
			EnStmtInfoTypeStatementOptmEarlyAbortReason.TimeOut => ControlsResources.TimeOut, 
			EnStmtInfoTypeStatementOptmEarlyAbortReason.MemoryLimitExceeded => ControlsResources.MemoryLimitExceeded, 
			EnStmtInfoTypeStatementOptmEarlyAbortReason.GoodEnoughPlanFound => ControlsResources.GoodEnoughPlanFound, 
			_ => item.ToString(), 
		};
	}

	public static string Convert(EnCloneAccessScopeType item)
	{
		return item switch
		{
			EnCloneAccessScopeType.Primary => ControlsResources.PrimaryClones, 
			EnCloneAccessScopeType.Secondary => ControlsResources.SecondaryClones, 
			EnCloneAccessScopeType.Both => ControlsResources.BothClones, 
			EnCloneAccessScopeType.Either => ControlsResources.EitherClones, 
			EnCloneAccessScopeType.ExactMatch => ControlsResources.ExactMatchClones, 
			_ => item.ToString(), 
		};
	}

	public static object Convert(InternalInfoType item)
	{
		ExpandableObjectWrapper expandableObjectWrapper = new ExpandableObjectWrapper();
		StringBuilder stringBuilder = new StringBuilder();
		using (XmlTextWriter xmlTextWriter = new XmlTextWriter(new StringWriter(stringBuilder, CultureInfo.InvariantCulture)))
		{
			xmlTextWriter.WriteStartElement("InternalInfo");
			if (item.AnyAttr != null)
			{
				XmlAttribute[] anyAttr = item.AnyAttr;
				foreach (XmlAttribute xmlAttribute in anyAttr)
				{
					object obj = Convert(xmlAttribute);
					if (obj != null)
					{
						expandableObjectWrapper[xmlAttribute.Name] = obj;
						xmlTextWriter.WriteAttributeString(XmlConvert.EncodeLocalName(xmlAttribute.Name), obj.ToString());
					}
				}
			}
			if (item.Any != null)
			{
				XmlElement[] any = item.Any;
				foreach (XmlElement xmlElement in any)
				{
					object obj2 = Convert(xmlElement);
					if (obj2 != null)
					{
						expandableObjectWrapper[xmlElement.Name] = obj2;
						xmlTextWriter.WriteRaw(Convert(xmlElement).ToString());
					}
				}
			}
			xmlTextWriter.WriteEndElement();
		}
		expandableObjectWrapper.DisplayName = stringBuilder.ToString();
		return expandableObjectWrapper;
	}

	public static object Convert(XmlElement item)
	{
		ExpandableObjectWrapper expandableObjectWrapper = new ExpandableObjectWrapper();
		StringBuilder stringBuilder = new StringBuilder();
		using (XmlTextWriter xmlTextWriter = new XmlTextWriter(new StringWriter(stringBuilder, CultureInfo.InvariantCulture)))
		{
			xmlTextWriter.WriteStartElement(XmlConvert.EncodeLocalName(item.Name));
			foreach (XmlAttribute attribute in item.Attributes)
			{
				object obj = Convert(attribute);
				if (obj != null)
				{
					expandableObjectWrapper[attribute.Name] = obj;
					xmlTextWriter.WriteAttributeString(XmlConvert.EncodeLocalName(attribute.Name), obj.ToString());
				}
			}
			foreach (XmlElement childNode in item.ChildNodes)
			{
				object obj2 = Convert(childNode);
				if (obj2 != null)
				{
					expandableObjectWrapper[childNode.Name] = obj2;
					xmlTextWriter.WriteRaw(Convert(childNode).ToString());
				}
			}
			xmlTextWriter.WriteEndElement();
		}
		expandableObjectWrapper.DisplayName = stringBuilder.ToString();
		return expandableObjectWrapper;
	}

	public static object Convert(XmlAttribute item)
	{
		return item.Value;
	}

	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (Type.GetTypeCode(sourceType) != TypeCode.Object && !sourceType.IsArray)
		{
			return convertMethods.ContainsKey(sourceType);
		}
		return true;
	}

	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (convertMethods.TryGetValue(value.GetType(), out var value2))
		{
			return value2.Invoke(null, BindingFlags.Static | BindingFlags.Public, null, new object[1] { value }, CultureInfo.CurrentCulture);
		}
		return ConvertToObjectWrapper(value);
	}

	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destType)
	{
		if (convertMethods.TryGetValue(value.GetType(), out var value2) && value2.ReturnType == destType)
		{
			return value2.Invoke(this, new object[1] { value });
		}
		return ConvertToObjectWrapper(value);
	}

	private static object ConvertToObjectWrapper(object item)
	{
		if (item is ICollection collection)
		{
			if (collection.Count == 1)
			{
				IEnumerator enumerator = collection.GetEnumerator();
				enumerator.MoveNext();
				return Default.ConvertFrom(enumerator.Current);
			}
			return new ExpandableArrayWrapper(collection);
		}
		return new ExpandableObjectWrapper(item);
	}

	static ObjectWrapperTypeConverter()
	{
		Default = new ObjectWrapperTypeConverter();
		convertMethods = new Dictionary<Type, MethodInfo>();
		MethodInfo[] methods = typeof(ObjectWrapperTypeConverter).GetMethods(BindingFlags.Static | BindingFlags.Public);
		foreach (MethodInfo methodInfo in methods)
		{
			if (methodInfo.Name == "Convert")
			{
				ParameterInfo[] parameters = methodInfo.GetParameters();
				if (parameters.Length == 1)
				{
					convertMethods.Add(parameters[0].ParameterType, methodInfo);
				}
			}
		}
	}

	internal static string MergeString(string separator, params object[] items)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (object obj in items)
		{
			if (obj == null)
			{
				continue;
			}
			string text = obj.ToString();
			if (text.Length > 0)
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append(separator);
				}
				stringBuilder.Append(text);
			}
		}
		return stringBuilder.ToString();
	}

	private static string MakeDisplayNameFromObjectNamesAndValues(object item)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(item))
		{
			AppendPropertyNameValuePair(stringBuilder, item, property);
		}
		return stringBuilder.ToString();
	}

	private static string MakeDisplayNameFromObjectNamesAndValues(object item, params string[] propertyNames)
	{
		StringBuilder stringBuilder = new StringBuilder();
		PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(item);
		foreach (string name in propertyNames)
		{
			PropertyDescriptor propertyDescriptor = properties[name];
			if (propertyDescriptor != null)
			{
				AppendPropertyNameValuePair(stringBuilder, item, propertyDescriptor);
			}
		}
		return stringBuilder.ToString();
	}

	private static void AppendPropertyNameValuePair(StringBuilder stringBuilder, object item, PropertyDescriptor property)
	{
		object value = property.GetValue(item);
		if (value != null)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(CultureInfo.CurrentCulture.TextInfo.ListSeparator);
				stringBuilder.Append(" ");
			}
			stringBuilder.Append(string.Format(ControlsResources.NameValuePair, property.DisplayName, value.ToString()));
		}
	}
}
