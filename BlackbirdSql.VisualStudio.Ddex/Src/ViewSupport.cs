/*
 *  Visual Studio DDEX Provider for FirebirdClient (BlackbirdSql)
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.blackbirdsql.org/index.php?op=doc&id=idpl
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2023 GA Christos
 *  All Rights Reserved.
 *   
 *  Contributors:
 *    GA Christos
 */

using System.Reflection;
using Microsoft.VisualStudio.Data.Framework;

using BlackbirdSql.Common;



namespace BlackbirdSql.VisualStudio.Ddex;

internal class ViewSupport : DataViewSupport
{
	#region · Constructors ·

	public ViewSupport(string resourceName, Assembly assembly) : base(resourceName, assembly)
//	public ViewSupport() : base("BlackbirdSql.VisualStudio.Ddex.ViewSupport", typeof(ViewSupport).Assembly)
	{
		Diag.Trace();
	}

	#endregion
}
