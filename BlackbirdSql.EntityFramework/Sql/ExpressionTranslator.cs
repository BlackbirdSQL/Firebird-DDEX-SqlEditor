/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/BlackbirdSQL/NETProvider/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$OriginalAuthors = Jiri Cincura (jiri@cincura.net)

using System;
using System.Collections.Generic;
using System.Data.Common;

#if EF6 || NET
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Metadata.Edm;
#else
using System.Data.Metadata.Edm;
using System.Data.Common.CommandTrees;
using System.Data.Common.Utils;
using System.Data.Mapping.Update.Internal;
#endif

using System.Diagnostics;
using System.Globalization;
using System.Text;

using BlackbirdSql.Common;
using BlackbirdSql.Data.DslClient;



#if EF6 || NET
namespace BlackbirdSql.Data.Entity.Sql;
#else
namespace BlackbirdSql.Data.Sql;
#endif


internal class ExpressionTranslator : DbExpressionVisitor
{
	#region Fields

	private readonly StringBuilder _commandText;
	private DbModificationCommandTree _commandTree;
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
		Diag.Dug(true, "Visit(\"ApplyExpression\") is not supported.");
		throw new NotSupportedException("Visit(\"ApplyExpression\") is not supported.");
	}

	public override void Visit(DbArithmeticExpression expression)
	{
		Diag.Dug(true, "Visit(\"ArithmeticExpression\") is not supported.");
		throw new NotSupportedException("Visit(\"ArithmeticExpression\") is not supported.");
	}

	public override void Visit(DbCaseExpression expression)
	{
		Diag.Dug(true, "Visit(\"CaseExpression\") is not supported.");
		throw new NotSupportedException("Visit(\"CaseExpression\") is not supported.");
	}

	public override void Visit(DbCastExpression expression)
	{
		Diag.Dug(true, "Visit(\"CastExpression\") is not supported.");
		throw new NotSupportedException("Visit(\"CastExpression\") is not supported.");
	}

	public override void Visit(DbCrossJoinExpression expression)
	{
		Diag.Dug(true, "Visit(\"CrossJoinExpression\") is not supported.");
		throw new NotSupportedException("Visit(\"CrossJoinExpression\") is not supported.");
	}

	public override void Visit(DbDerefExpression expression)
	{
		Diag.Dug(true, "Visit(\"DerefExpression\") is not supported.");
		throw new NotSupportedException("Visit(\"DerefExpression\") is not supported.");
	}

	public override void Visit(DbDistinctExpression expression)
	{
		Diag.Dug(true, "Visit(\"DistinctExpression\") is not supported.");
		throw new NotSupportedException("Visit(\"DistinctExpression\") is not supported.");
	}

	public override void Visit(DbElementExpression expression)
	{
		Diag.Dug(true, "Visit(\"ElementExpression\") is not supported.");
		throw new NotSupportedException("Visit(\"ElementExpression\") is not supported.");
	}

	public override void Visit(DbEntityRefExpression expression)
	{
		Diag.Dug(true, "Visit(\"EntityRefExpression\") is not supported.");
		throw new NotSupportedException("Visit(\"EntityRefExpression\") is not supported.");
	}

	public override void Visit(DbExceptExpression expression)
	{
		Diag.Dug(true, "Visit(\"ExceptExpression\") is not supported.");
		throw new NotSupportedException("Visit(\"ExceptExpression\") is not supported.");
	}

	public override void Visit(DbExpression expression)
	{
		Diag.Dug(true, "Visit(\"Expression\") is not supported.");
		throw new NotSupportedException("Visit(\"Expression\") is not supported.");
	}

	public override void Visit(DbFilterExpression expression)
	{
		Diag.Dug(true, "Visit(\"FilterExpression\") is not supported.");
		throw new NotSupportedException("Visit(\"FilterExpression\") is not supported.");
	}

	public override void Visit(DbFunctionExpression expression)
	{
		Diag.Dug(true, "Visit(\"FunctionExpression\") is not supported.");
		throw new NotSupportedException("Visit(\"FunctionExpression\") is not supported.");
	}

	public override void Visit(DbGroupByExpression expression)
	{
		Diag.Dug(true, "Visit(\"GroupByExpression\") is not supported.");
		throw new NotSupportedException("Visit(\"GroupByExpression\") is not supported.");
	}

	public override void Visit(DbIntersectExpression expression)
	{
		Diag.Dug(true, "Visit(\"IntersectExpression\") is not supported.");
		throw new NotSupportedException("Visit(\"IntersectExpression\") is not supported.");
	}

	public override void Visit(DbIsEmptyExpression expression)
	{
		Diag.Dug(true, "Visit(\"IsEmptyExpression\") is not supported.");
		throw new NotSupportedException("Visit(\"IsEmptyExpression\") is not supported.");
	}

	public override void Visit(DbIsOfExpression expression)
	{
		Diag.Dug(true, "Visit(\"IsOfExpression\") is not supported.");
		throw new NotSupportedException("Visit(\"IsOfExpression\") is not supported.");
	}

	public override void Visit(DbJoinExpression expression)
	{
		Diag.Dug(true, "Visit(\"JoinExpression\") is not supported.");
		throw new NotSupportedException("Visit(\"JoinExpression\") is not supported.");
	}

	public override void Visit(DbLikeExpression expression)
	{
		Diag.Dug(true, "Visit(\"LikeExpression\") is not supported.");
		throw new NotSupportedException("Visit(\"LikeExpression\") is not supported.");
	}

	public override void Visit(DbLimitExpression expression)
	{
		Diag.Dug(true, "Visit(\"LimitExpression\") is not supported.");
		throw new NotSupportedException("Visit(\"LimitExpression\") is not supported.");
	}

	public override void Visit(DbOfTypeExpression expression)
	{
		Diag.Dug(true, "Visit(\"OfTypeExpression\") is not supported.");
		throw new NotSupportedException("Visit(\"OfTypeExpression\") is not supported.");
	}

	public override void Visit(DbParameterReferenceExpression expression)
	{
		Diag.Dug(true, "Visit(\"ParameterReferenceExpression\") is not supported.");
		throw new NotSupportedException("Visit(\"ParameterReferenceExpression\") is not supported.");
	}

	public override void Visit(DbProjectExpression expression)
	{
		Diag.Dug(true, "Visit(\"ProjectExpression\") is not supported.");
		throw new NotSupportedException("Visit(\"ProjectExpression\") is not supported.");
	}

	public override void Visit(DbQuantifierExpression expression)
	{
		Diag.Dug(true, "Visit(\"QuantifierExpression\") is not supported.");
		throw new NotSupportedException("Visit(\"QuantifierExpression\") is not supported.");
	}

	public override void Visit(DbRefExpression expression)
	{
		Diag.Dug(true, "Visit(\"RefExpression\") is not supported.");
		throw new NotSupportedException("Visit(\"RefExpression\") is not supported.");
	}

	public override void Visit(DbRefKeyExpression expression)
	{
		Diag.Dug(true, "Visit(\"RefKeyExpression\") is not supported.");
		throw new NotSupportedException("Visit(\"RefKeyExpression\") is not supported.");
	}

	public override void Visit(DbRelationshipNavigationExpression expression)
	{
		Diag.Dug(true, "Visit(\"RelationshipNavigationExpression\") is not supported.");
		throw new NotSupportedException("Visit(\"RelationshipNavigationExpression\") is not supported.");
	}

	public override void Visit(DbSkipExpression expression)
	{
		Diag.Dug(true, "Visit(\"SkipExpression\") is not supported.");
		throw new NotSupportedException("Visit(\"SkipExpression\") is not supported.");
	}

	public override void Visit(DbSortExpression expression)
	{
		Diag.Dug(true, "Visit(\"SortExpression\") is not supported.");
		throw new NotSupportedException("Visit(\"SortExpression\") is not supported.");
	}

	public override void Visit(DbTreatExpression expression)
	{
		Diag.Dug(true, "Visit(\"TreatExpression\") is not supported.");
		throw new NotSupportedException("Visit(\"TreatExpression\") is not supported.");
	}

	public override void Visit(DbUnionAllExpression expression)
	{
		Diag.Dug(true, "Visit(\"UnionAllExpression\") is not supported.");
		throw new NotSupportedException("Visit(\"UnionAllExpression\") is not supported.");
	}

	public override void Visit(DbVariableReferenceExpression expression)
	{
		Diag.Dug(true, "Visit(\"VariableReferenceExpression\") is not supported.");
		throw new NotSupportedException("Visit(\"VariableReferenceExpression\") is not supported.");
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
		_commandText.Append(')');
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
	internal DslParameter CreateParameter(object value, TypeUsage type)
	{
		var parameterName = string.Concat("@p", _parameterNameCount.ToString(CultureInfo.InvariantCulture));
		_parameterNameCount++;

		var parameter = ProviderServices.CreateSqlParameter(parameterName, type, ParameterMode.In, value);

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
				var p = _parameters[^1];
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
		_commandText.Append('(');
		expression.Left.Accept(this);
		_commandText.Append(separator);
		expression.Right.Accept(this);
		_commandText.Append(')');
	}

	#endregion
}
