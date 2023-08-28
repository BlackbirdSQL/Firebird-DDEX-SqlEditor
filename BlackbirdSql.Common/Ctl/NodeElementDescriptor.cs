// Microsoft.Data.Tools.Schema.Sql, Version=16.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.Data.Tools.Schema.Sql.SchemaModel.SqlElementDescriptor
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Enums;
using BlackbirdSql.Core.Providers;
using BlackbirdSql.Common.Enums;

namespace BlackbirdSql.Common.Ctl;

public class NodeElementDescriptor
{
	private EnModelObjectType _Type;

	private readonly List<string> _IdentifierList;

	private readonly List<string> _ExternalReferenceList;

	private readonly EnNodeElementDescriptorVolatility _Volatility;

	public bool IsEmpty => Identifiers.Count == 0;

	public bool HasUsableName
	{
		get
		{
			if (!IsEmpty)
			{
				return !IgnoreName;
			}
			return false;
		}
	}

	public bool IgnoreName { get; set; }

	public EnModelObjectType ElementType
	{
		get
		{
			return _Type;
		}
		set
		{
			_Type = value;
		}
	}

	// public SqlColumnGraphType GraphType { get; set; }

	/*
	public ModelElementClass ElementClass
	{
		get
		{
			ModelElementClass result = null;
			IList<ModelElementClass> implementingElementClasses = SqlSchemaModel.ModelSchema.GetImplementingElementClasses(ElementType);
			Tracer.AssertTraceEvent(implementingElementClasses.Count == 1, TraceEventType.Warning, TraceId.CoreServices, "Type " + ElementType?.ToString() + " was expected to map to one element class but mapped to " + implementingElementClasses.Count);
			if (implementingElementClasses.Count > 0)
			{
				result = implementingElementClasses[0];
			}
			return result;
		}
	}
	*/

	public EnNodeElementDescriptorVolatility Volatility => _Volatility;

	public bool IsVolatile => _Volatility != EnNodeElementDescriptorVolatility.None;

	public ReadOnlyCollection<string> Identifiers => _IdentifierList.AsReadOnly();

	public ReadOnlyCollection<string> ExternalReferenceList => _ExternalReferenceList.AsReadOnly();

	public NodeElementDescriptor(EnModelObjectType elementType, params string[] identifiers)
		: this(identifiers, elementType, EnNodeElementDescriptorVolatility.None)
	{
	}

	public NodeElementDescriptor(EnModelObjectType elementType, IEnumerable<string> identifiers)
		: this(identifiers, elementType, EnNodeElementDescriptorVolatility.None)
	{
	}

	public NodeElementDescriptor(NodeElementDescriptor rhs)
	{
		_ExternalReferenceList = rhs._ExternalReferenceList;
		_IdentifierList = rhs._IdentifierList;
		_Type = rhs._Type;
		_Volatility = rhs._Volatility;
	}

	public NodeElementDescriptor(EnModelObjectType elementType, EnNodeElementDescriptorVolatility volatility, params string[] identifiers)
		: this(identifiers, elementType, volatility)
	{
	}

	public NodeElementDescriptor(params string[] identifiers)
		: this(identifiers, EnModelObjectType.Unknown, EnNodeElementDescriptorVolatility.None)
	{
	}

	public NodeElementDescriptor(EnModelObjectType elementType, EnNodeElementDescriptorVolatility volatility, IEnumerable<string> identifiers)
		: this(identifiers, elementType, volatility)
	{
	}

	private NodeElementDescriptor(IEnumerable<string> identifierList, EnModelObjectType elementType, EnNodeElementDescriptorVolatility volatility)
	{
		_Type = elementType;
		_Volatility = volatility;
		_IdentifierList = new List<string>();
		if (identifierList != null)
		{
			_IdentifierList.AddRange(identifierList);
		}
		_ExternalReferenceList = new List<string>();
		IgnoreName = false;
		// GraphType = SqlColumnGraphType.None;
	}

	public void AddExternalNameParts(params string[] names)
	{
		if (names != null)
		{
			_ExternalReferenceList.AddRange(names);
		}
	}

	public void AddExternalNameParts(IEnumerable<string> names)
	{
		if (names != null)
		{
			_ExternalReferenceList.AddRange(names);
		}
	}

	private static void BuildEscapedIdentifier(EnQuoteType quoteType, StringBuilder builder, IEnumerable<string> parts, bool append = false)
	{
		foreach (string part in parts)
		{
			if (append)
			{
				builder.Append(".");
			}
			builder.Append(EncodeIdentifier(part, quoteType));
			append = true;
		}
	}

	public static string EncodeIdentifier(string identifier, EnQuoteType quoteType)
	{
		StringBuilder stringBuilder = new StringBuilder();
		switch (quoteType)
		{
			case EnQuoteType.NotQuoted:
				return identifier;
			case EnQuoteType.SquareBracket:
				stringBuilder.Append("[");
				stringBuilder.Append(identifier.Replace("]", "]]"));
				stringBuilder.Append("]");
				break;
			case EnQuoteType.DoubleQuote:
				stringBuilder.Append("\"");
				stringBuilder.Append(identifier.Replace("\"", "\"\""));
				stringBuilder.Append("\"");
				break;
			default:
				ArgumentOutOfRangeException ex = new("quoteType");
				Diag.Dug(ex);
				throw ex;
		}
		return stringBuilder.ToString();
	}

	/*
	public virtual string ToStringInternal()
	{
		return HashLog.FormatObjectName(GetFullyQualifiedName());
	}
	*/

	public virtual string GetFullyQualifiedName()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (Identifiers.Count == 0)
		{
			stringBuilder.Append("[#EMPTY#]");
		}
		else
		{
			BuildEscapedIdentifier(EnQuoteType.NotQuoted, stringBuilder, ExternalReferenceList);
			BuildEscapedIdentifier(EnQuoteType.NotQuoted, stringBuilder, Identifiers, stringBuilder.Length > 0);
		}
		return stringBuilder.ToString();
	}

	public static int GetHashCode(NodeElementDescriptor obj)
	{
		int num = 0;
		if (obj._Type != EnModelObjectType.Unknown)
		{
			num = obj._Type.GetHashCode();
		}
		num ^= obj._Volatility.GetHashCode();
		num ^= obj.IgnoreName.GetHashCode();
		if (obj._ExternalReferenceList != null && obj._ExternalReferenceList.Count > 0)
		{
			foreach (string externalNamePart in obj._ExternalReferenceList)
			{
				num ^= externalNamePart.GetHashCode();
			}
		}
		if (obj._IdentifierList != null && obj._IdentifierList.Count > 0)
		{
			foreach (string identifier in obj._IdentifierList)
			{
				num ^= identifier.GetHashCode();
			}
		}
		return num;
	}
}
