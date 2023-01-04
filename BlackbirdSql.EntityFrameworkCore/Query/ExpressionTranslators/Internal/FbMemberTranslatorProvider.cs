/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/BlackbirdSQL/NETProvider/raw/master/license.txt.
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
using System.Linq;
using BlackbirdSql.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.Query;

namespace BlackbirdSql.EntityFrameworkCore.Query.ExpressionTranslators.Internal;

public class FbMemberTranslatorProvider : RelationalMemberTranslatorProvider
{
	static readonly List<Type> Translators = TranslatorsHelper.GetTranslators<IMemberTranslator>().ToList();

	public FbMemberTranslatorProvider(RelationalMemberTranslatorProviderDependencies dependencies)
		: base(dependencies)
	{
		AddTranslators(Translators.Select(t => (IMemberTranslator)Activator.CreateInstance(t, dependencies.SqlExpressionFactory)));
	}
}
