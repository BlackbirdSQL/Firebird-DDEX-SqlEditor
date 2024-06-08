// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ExecutionPlan.XmlPlanNodeBuilder
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using BlackbirdSql.Shared.Controls.Graphing.Enums;
using BlackbirdSql.Shared.Controls.Graphing.Gram;
using BlackbirdSql.Shared.Controls.Graphing.Interfaces;
using BlackbirdSql.Shared.Controls.Graphing.Parsers;
using BlackbirdSql.Shared.Properties;

namespace BlackbirdSql.Shared.Controls.Graphing;

public sealed class XmlPlanNodeBuilder : INodeBuilder, IXmlBatchParser
{
	private static readonly XmlSerializer Serializer = new XmlSerializer(typeof(ExecutionPlanXML));

	private readonly EnExecutionPlanType showPlanType;

	private int currentNodeId;

	public XmlPlanNodeBuilder(EnExecutionPlanType showPlanType)
	{
		this.showPlanType = showPlanType;
	}

	public ExecutionPlanGraph[] Execute(object dataSource)
	{
		if (dataSource is not ExecutionPlanXML showPlanXML)
		{
			showPlanXML = ReadXmlExecutionPlan(dataSource);
		}
		List<ExecutionPlanGraph> list = new List<ExecutionPlanGraph>();
		foreach (AbstractStmtInfoType item in EnumStatements(showPlanXML))
		{
			currentNodeId = 0;
			NodeBuilderContext nodeBuilderContext = new NodeBuilderContext(new ExecutionPlanGraph(), showPlanType, this);
			AbstractXmlPlanParser.Parse(item, null, null, nodeBuilderContext);
			list.Add(nodeBuilderContext.Graph);
		}
		return list.ToArray();
	}

	public string GetSingleStatementXml(object dataSource, int statementIndex)
	{
		StmtBlockType singleStatementObject = GetSingleStatementObject(dataSource, statementIndex);
		ExecutionPlanXML showPlanXML = ReadXmlExecutionPlan(dataSource);
		showPlanXML.BatchSequence = new StmtBlockType[1][] { new StmtBlockType[1] { singleStatementObject } };
		StringBuilder stringBuilder = new StringBuilder();
		Serializer.Serialize(new StringWriter(stringBuilder), showPlanXML);
		return stringBuilder.ToString();
	}

	public StmtBlockType GetSingleStatementObject(object dataSource, int statementIndex)
	{
		ExecutionPlanXML plan = ReadXmlExecutionPlan(dataSource);
		int num = 0;
		StmtBlockType stmtBlockType = new StmtBlockType();
		foreach (AbstractStmtInfoType item in EnumStatements(plan))
		{
			if (statementIndex == num++)
			{
				object[] items = new AbstractStmtInfoType[1] { item };
				stmtBlockType.Items = items;
				break;
			}
		}
		if (stmtBlockType.Items == null)
		{
			throw new ArgumentOutOfRangeException("statementIndex");
		}
		return stmtBlockType;
	}

	internal int GetCurrentNodeId()
	{
		return ++currentNodeId;
	}

	private ExecutionPlanXML ReadXmlExecutionPlan(object dataSource)
	{
		ExecutionPlanXML showPlanXML = null;
		if (dataSource is string s)
		{
			using StringReader textReader = new StringReader(s);
			showPlanXML = Serializer.Deserialize(textReader) as ExecutionPlanXML;
		}
		else if (dataSource is byte[] buffer)
		{
			using MemoryStream memoryStream = new MemoryStream(buffer);
			MethodInfo method = typeof(XmlReader).GetMethod("CreateSqlReader", BindingFlags.Static | BindingFlags.NonPublic);
			object[] parameters = new object[3] { memoryStream, null, null };
			using XmlReader xmlReader = (XmlReader)method.Invoke(null, parameters);
			showPlanXML = Serializer.Deserialize(xmlReader) as ExecutionPlanXML;
		}
		if (showPlanXML == null)
		{
			throw new ArgumentException(ControlsResources.UnknownExecutionPlanSource);
		}
		return showPlanXML;
	}

	private IEnumerable<AbstractStmtInfoType> EnumStatements(ExecutionPlanXML plan)
	{
		StmtBlockType[][] batchSequence = plan.BatchSequence;
		foreach (StmtBlockType[] array in batchSequence)
		{
			StmtBlockType[] array2 = array;
			foreach (StmtBlockType statementBlock in array2)
			{
				ExtractFunctions(statementBlock);
				if (showPlanType == EnExecutionPlanType.Live)
				{
					FlattenConditionClauses(statementBlock);
				}
				foreach (AbstractStmtInfoType item in EnumStatements(statementBlock))
				{
					yield return item;
				}
			}
		}
	}

	private void FlattenConditionClauses(StmtBlockType statementBlock)
	{
		if (statementBlock != null && statementBlock.Items != null)
		{
			ArrayList arrayList = new ArrayList();
			object[] items = statementBlock.Items;
			for (int i = 0; i < items.Length; i++)
			{
				AbstractStmtInfoType baseStmtInfoType = (AbstractStmtInfoType)items[i];
				arrayList.Add(baseStmtInfoType);
				FlattenConditionClauses(baseStmtInfoType, arrayList);
			}
			items = new AbstractStmtInfoType[arrayList.Count];
			statementBlock.Items = items;
			arrayList.CopyTo(statementBlock.Items);
		}
	}

	private void FlattenConditionClauses(AbstractStmtInfoType statement, ArrayList targetStatementList)
	{
		foreach (object child in XmlPlanParserFactory.GetParser(statement.GetType()).GetChildren(statement))
		{
			if (child is StmtCondTypeThen stmtCondTypeThen)
			{
				if (stmtCondTypeThen.Statements != null && stmtCondTypeThen.Statements.Items != null)
				{
					object[] items = stmtCondTypeThen.Statements.Items;
					for (int i = 0; i < items.Length; i++)
					{
						AbstractStmtInfoType baseStmtInfoType = (AbstractStmtInfoType)items[i];
						targetStatementList.Add(baseStmtInfoType);
						FlattenConditionClauses(baseStmtInfoType, targetStatementList);
					}
				}
			}
			else if (child is StmtCondTypeElse stmtCondTypeElse && stmtCondTypeElse.Statements != null && stmtCondTypeElse.Statements.Items != null)
			{
				object[] items = stmtCondTypeElse.Statements.Items;
				for (int i = 0; i < items.Length; i++)
				{
					AbstractStmtInfoType baseStmtInfoType2 = (AbstractStmtInfoType)items[i];
					targetStatementList.Add(baseStmtInfoType2);
					FlattenConditionClauses(baseStmtInfoType2, targetStatementList);
				}
			}
		}
	}

	private void ExtractFunctions(StmtBlockType statementBlock)
	{
		if (statementBlock != null && statementBlock.Items != null)
		{
			ArrayList arrayList = new ArrayList();
			object[] items = statementBlock.Items;
			for (int i = 0; i < items.Length; i++)
			{
				AbstractStmtInfoType baseStmtInfoType = (AbstractStmtInfoType)items[i];
				arrayList.Add(baseStmtInfoType);
				ExtractFunctions(baseStmtInfoType, arrayList);
			}
			items = new AbstractStmtInfoType[arrayList.Count];
			statementBlock.Items = items;
			arrayList.CopyTo(statementBlock.Items);
		}
	}

	private void ExtractFunctions(AbstractStmtInfoType statement, ArrayList targetStatementList)
	{
		foreach (FunctionTypeItem item in XmlPlanParserFactory.GetParser(statement.GetType()).ExtractFunctions(statement))
		{
			StmtSimpleType stmtSimpleType = null;
			if (item.Type == FunctionTypeItem.EnItemType.StoredProcedure)
			{
				stmtSimpleType = new StmtSimpleType
				{
					StoredProc = item.Function
				};
			}
			else if (item.Type == FunctionTypeItem.EnItemType.Udf)
			{
				stmtSimpleType = new StmtSimpleType
				{
					UDF = new FunctionType[1] { item.Function }
				};
			}
			if (stmtSimpleType == null)
			{
				continue;
			}
			targetStatementList.Add(stmtSimpleType);
			if (item.Function.Statements != null && item.Function.Statements.Items != null)
			{
				object[] items = item.Function.Statements.Items;
				for (int i = 0; i < items.Length; i++)
				{
					AbstractStmtInfoType statement2 = (AbstractStmtInfoType)items[i];
					ExtractFunctions(statement2, targetStatementList);
				}
			}
		}
	}

	private IEnumerable<AbstractStmtInfoType> EnumStatements(StmtBlockType statementBlock)
	{
		if (statementBlock != null && statementBlock.Items != null)
		{
			object[] items = statementBlock.Items;
			for (int i = 0; i < items.Length; i++)
			{
				yield return (AbstractStmtInfoType)items[i];
			}
		}
	}
}
