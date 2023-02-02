/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/raw/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$Authors = Jiri Cincura (jiri@cincura.net)

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using BlackbirdSql.Common;
using FirebirdSql.Data.FirebirdClient;

namespace EntityFramework.Firebird.SqlGen;

internal class ExpressionTranslator : DbExpressionVisitor
{
	#region Fields

	private readonly StringBuilder _commandText;
	private readonly DbModificationCommandTree _commandTree;
	private readonly List<DbParameter> _parameters;
	private readonly Dictionary<EdmMember, List<DbParameter>> _memberValues;
	private readonly bool generateParameters;
	private int _parameterNameCount = 0;

	#endregion

	#region Internal Properties

	internal List<DbParameter> Parameters
	{
		get { return _parameters; }
	}

	internal Dictionary<EdmMember, List<DbParameter>> MemberValues
	{
		get { return _memberValues; }
	}

	#endregion

	#region Unsupported Visit Methods

	public override void Visit(DbApplyExpression expression)
	{
		NotSupportedException exbb = new("Visit(\"ApplyExpression\") is not supported.");
		Diag.Dug(exbb);
		throw exbb;
	}

	public override void Visit(DbArithmeticExpression expression)
	{
		NotSupportedException exbb = new("Visit(\"ArithmeticExpression\") is not supported.");
		Diag.Dug(exbb);
		throw exbb;
	}

	public override void Visit(DbCaseExpression expression)
	{
		NotSupportedException exbb = new("Visit(\"CaseExpression\") is not supported.");
		Diag.Dug(exbb);
		throw exbb;
	}

	public override void Visit(DbCastExpression expression)
	{
		NotSupportedException exbb = new("Visit(\"CastExpression\") is not supported.");
		Diag.Dug(exbb);
		throw exbb;
	}

	public override void Visit(DbCrossJoinExpression expression)
	{
		NotSupportedException exbb = new("Visit(\"CrossJoinExpression\") is not supported.");
		Diag.Dug(exbb);
		throw exbb;
	}

	public override void Visit(DbDerefExpression expression)
	{
		NotSupportedException exbb = new("Visit(\"DerefExpression\") is not supported.");
		Diag.Dug(exbb);
		throw exbb;
	}

	public override void Visit(DbDistinctExpression expression)
	{
		NotSupportedException exbb = new("Visit(\"DistinctExpression\") is not supported.");
		Diag.Dug(exbb);
		throw exbb;
	}

	public override void Visit(DbElementExpression expression)
	{
		NotSupportedException exbb = new("Visit(\"ElementExpression\") is not supported.");
		Diag.Dug(exbb);
		throw exbb;
	}

	public override void Visit(DbEntityRefExpression expression)
	{
		NotSupportedException exbb = new("Visit(\"EntityRefExpression\") is not supported.");
		Diag.Dug(exbb);
		throw exbb;
	}

	public override void Visit(DbExceptExpression expression)
	{
		NotSupportedException exbb = new("Visit(\"ExceptExpression\") is not supported.");
		Diag.Dug(exbb);
		throw exbb;
	}

	public override void Visit(DbExpression expression)
	{
		NotSupportedException exbb = new("Visit(\"Expression\") is not supported.");
		Diag.Dug(exbb);
		throw exbb;
	}

	public override void Visit(DbFilterExpression expression)
	{
		NotSupportedException exbb = new("Visit(\"FilterExpression\") is not supported.");
		Diag.Dug(exbb);
		throw exbb;
	}

	public override void Visit(DbFunctionExpression expression)
	{
		NotSupportedException exbb = new("Visit(\"FunctionExpression\") is not supported.");
		Diag.Dug(exbb);
		throw exbb;
	}

	public override void Visit(DbGroupByExpression expression)
	{
		NotSupportedException exbb = new("Visit(\"GroupByExpression\") is not supported.");
		Diag.Dug(exbb);
		throw exbb;
	}

	public override void Visit(DbIntersectExpression expression)
	{
		NotSupportedException exbb = new("Visit(\"IntersectExpression\") is not supported.");
		Diag.Dug(exbb);
		throw exbb;
	}

	public override void Visit(DbIsEmptyExpression expression)
	{
		NotSupportedException exbb = new("Visit(\"IsEmptyExpression\") is not supported.");
		Diag.Dug(exbb);
		throw exbb;
	}

	public override void Visit(DbIsOfExpression expression)
	{
		NotSupportedException exbb = new("Visit(\"IsOfExpression\") is not supported.");
		Diag.Dug(exbb);
		throw exbb;
	}

	public override void Visit(DbJoinExpression expression)
	{
		NotSupportedException exbb = new("Visit(\"JoinExpression\") is not supported.");
		Diag.Dug(exbb);
		throw exbb;
	}

	public override void Visit(DbLikeExpression expression)
	{
		NotSupportedException exbb = new("Visit(\"LikeExpression\") is not supported.");
		Diag.Dug(exbb);
		throw exbb;
	}

	public override void Visit(DbLimitExpression expression)
	{
		NotSupportedException exbb = new("Visit(\"LimitExpression\") is not supported.");
		Diag.Dug(exbb);
		throw exbb;
	}

	public override void Visit(DbOfTypeExpression expression)
	{
		NotSupportedException exbb = new("Visit(\"OfTypeExpression\") is not supported.");
		Diag.Dug(exbb);
		throw exbb;
	}

	public override void Visit(DbParameterReferenceExpression expression)
	{
		NotSupportedException exbb = new("Visit(\"ParameterReferenceExpression\") is not supported.");
		Diag.Dug(exbb);
		throw exbb;
	}

	public override void Visit(DbProjectExpression expression)
	{
		NotSupportedException exbb = new("Visit(\"ProjectExpression\") is not supported.");
		Diag.Dug(exbb);
		throw exbb;
	}

	public override void Visit(DbQuantifierExpression expression)
	{
		NotSupportedException exbb = new("Visit(\"QuantifierExpression\") is not supported.");
		Diag.Dug(exbb);
		throw exbb;
	}

	public override void Visit(DbRefExpression expression)
	{
		NotSupportedException exbb = new("Visit(\"RefExpression\") is not supported.");
		Diag.Dug(exbb);
		throw exbb;
	}

	public override void Visit(DbRefKeyExpression expression)
	{
		NotSupportedException exbb = new("Visit(\"RefKeyExpression\") is not supported.");
		Diag.Dug(exbb);
		throw exbb;
	}

	public override void Visit(DbRelationshipNavigationExpression expression)
	{
		NotSupportedException exbb = new("Visit(\"RelationshipNavigationExpression\") is not supported.");
		Diag.Dug(exbb);
		throw exbb;
	}

	public override void Visit(DbSkipExpression expression)
	{
		NotSupportedException exbb = new("Visit(\"SkipExpression\") is not supported.");
		Diag.Dug(exbb);
		throw exbb;
	}

	public override void Visit(DbSortExpression expression)
	{
		NotSupportedException exbb = new("Visit(\"SortExpression\") is not supported.");
		Diag.Dug(exbb);
		throw exbb;
	}

	public override void Visit(DbTreatExpression expression)
	{
		NotSupportedException exbb = new("Visit(\"TreatExpression\") is not supported.");
		Diag.Dug(exbb);
		throw exbb;
	}

	public override void Visit(DbUnionAllExpression expression)
	{
		NotSupportedException exbb = new("Visit(\"UnionAllExpression\") is not supported.");
		Diag.Dug(exbb);
		throw exbb;
	}

	public override void Visit(DbVariableReferenceExpression expression)
	{
		NotSupportedException exbb = new("Visit(\"VariableReferenceExpression\") is not supported.");
		Diag.Dug(exbb);
		throw exbb;
	}

	#endregion

	#region Methods

	public override void Visit(DbAndExpression expression)
	{
		VisitBinary(expression, " AND ");
	}

	public override void Visit(DbOrExpression expression)
	{
		VisitBinary(expression, " OR ");
	}

	public override void Visit(DbComparisonExpression expression)
	{
		Debug.Assert(expression.ExpressionKind == DbExpressionKind.Equals,
			"only equals comparison expressions are produced in DML command trees in V1");

		VisitBinary(expression, " = ");

		RegisterMemberValue(expression.Left, expression.Right);
	}

	public override void Visit(DbIsNullExpression expression)
	{
		expression.Argument.Accept(this);
		_commandText.Append(" IS NULL");
	}

	public override void Visit(DbNotExpression expression)
	{
		_commandText.Append("NOT (");
		expression.Accept(this);
		_commandText.Append(")");
	}

	public override void Visit(DbConstantExpression expression)
	{
		if (generateParameters)
		{
			var parameter = CreateParameter(expression.Value, expression.ResultType);
			_commandText.Append(parameter.ParameterName);
		}
		else
		{
			using (var writer = new SqlWriter(_commandText))
			{
				var sqlGenerator = new SqlGenerator();
				sqlGenerator.WriteSql(writer, expression.Accept(sqlGenerator));
			}
		}
	}

	public override void Visit(DbScanExpression expression)
	{
		_commandText.Append(SqlGenerator.GetTargetSql(expression.Target));
	}

	public override void Visit(DbPropertyExpression expression)
	{
		_commandText.Append(DmlSqlGenerator.GenerateMemberSql(expression.Property));
	}

	public override void Visit(DbNullExpression expression)
	{
		_commandText.Append("NULL");
	}

	public override void Visit(DbNewInstanceExpression expression)
	{
		// assumes all arguments are self-describing (no need to use aliases
		// because no renames are ever used in the projection)
		var first = true;

		foreach (var argument in expression.Arguments)
		{
			if (first)
			{
				first = false;
			}
			else
			{
				_commandText.Append(", ");
			}
			argument.Accept(this);
		}
	}

	#endregion

	#region Internal Methods

	/// <summary>
	/// Initialize a new expression translator populating the given string builder
	/// with command text. Command text builder and command tree must not be null.
	/// </summary>
	/// <param name="commandText">Command text with which to populate commands</param>
	/// <param name="commandTree">Command tree generating SQL</param>
	/// <param name="preserveMemberValues">Indicates whether the translator should preserve
	/// member values while compiling t-SQL (only needed for server generation)</param>
	internal ExpressionTranslator(
		StringBuilder commandText,
		DbModificationCommandTree commandTree,
		bool preserveMemberValues,
		bool generateParameters)
	{
		Debug.Assert(null != commandText);
		Debug.Assert(null != commandTree);

		_commandText = commandText;
		_commandTree = commandTree;
		_parameters = new List<DbParameter>();
		_memberValues = preserveMemberValues ? new Dictionary<EdmMember, List<DbParameter>>() : null;
		this.generateParameters = generateParameters;
	}

	// generate parameter (name based on parameter ordinal)
	internal FbParameter CreateParameter(object value, TypeUsage type)
	{
		var parameterName = string.Concat("@p", _parameterNameCount.ToString(CultureInfo.InvariantCulture));
		_parameterNameCount++;

		var parameter = FbProviderServices.CreateSqlParameter(parameterName, type, ParameterMode.In, value);

		_parameters.Add(parameter);

		return parameter;
	}

	/// <summary>
	/// Call this method to register a property value pair so the translator "remembers"
	/// the values for members of the row being modified. These values can then be used
	/// to form a predicate for server-generation (based on the key of the row)
	/// </summary>
	/// <param name="propertyExpression">Expression containing the column reference (property expression).</param>
	/// <param name="value">Expression containing the value of the column.</param>
	internal void RegisterMemberValue(DbExpression propertyExpression, DbExpression value)
	{
		if (null != _memberValues)
		{
			// register the value for this property
			Debug.Assert(propertyExpression.ExpressionKind == DbExpressionKind.Property,
						 "DML predicates and setters must be of the form property = value");

			// get name of left property
			var property = ((DbPropertyExpression)propertyExpression).Property;

			// don't track null values
			if (value.ExpressionKind != DbExpressionKind.Null)
			{
				Debug.Assert(value.ExpressionKind == DbExpressionKind.Constant, "value must either constant or null");

				// retrieve the last parameter added (which describes the parameter)
				var p = _parameters[_parameters.Count - 1];
				if (!_memberValues.ContainsKey(property))
					_memberValues.Add(property, new List<DbParameter>(new[] { p }));
				else
					_memberValues[property].Add(p);
			}
		}
	}

	#endregion

	#region Private Methods

	private void VisitBinary(DbBinaryExpression expression, string separator)
	{
		_commandText.Append("(");
		expression.Left.Accept(this);
		_commandText.Append(separator);
		expression.Right.Accept(this);
		_commandText.Append(")");
	}

	#endregion
}
