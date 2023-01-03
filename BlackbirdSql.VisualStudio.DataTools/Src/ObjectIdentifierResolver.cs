/*
 *  Visual Studio DDEX Provider for BlackbirdSql DslClient
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
 *  Copyright (c) 2005 Carlos Guzman Alvarez
 *  All Rights Reserved.
 *   
 *  Contributors:
 *    Jiri Cincura (jiri@cincura.net)
 */

using System;
using Microsoft.VisualStudio.Data;

namespace BlackbirdSql.VisualStudio.DataTools;

internal class ObjectIdentifierResolver : DataObjectIdentifierResolver
{
	#region · Private Fields ·

#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable IDE0052 // Remove unread private members
	private DataConnection connection;
#pragma warning restore IDE0052 // Remove unread private members
#pragma warning restore IDE0044 // Add readonly modifier

	#endregion

	#region · Constructors ·

	public ObjectIdentifierResolver(DataConnection connection) : base()
	{
		// Diag.Dug();
		this.connection = connection;
	}

	#endregion

	#region · Methods ·

	protected override object[] QuickContractIdentifier(string typeName, object[] fullIdentifier)
	{
		// Diag.Dug("Indentifying: " + typeName);

		if (typeName == null)
		{
			throw new ArgumentNullException("typeName");
		}

		if (typeName == ObjectTypes.Root)
		{
			return base.QuickContractIdentifier(typeName, fullIdentifier);
		}

		int length = this.GetIdentifierLength(typeName);
		if (length == -1)
		{
			throw new NotSupportedException();
		}
		object[] identifier = new object[length];

		fullIdentifier?.CopyTo(identifier, length - fullIdentifier.Length);

		if (identifier.Length > 0)
		{
			identifier[0] = null;
		}

		if (identifier.Length > 1)
		{
			identifier[1] = null;
		}

		return identifier;
	}

	protected override object[] QuickExpandIdentifier(string typeName, object[] partialIdentifier)
	{
		// Diag.Dug("Resolving type: " + typeName);

		if (typeName == null)
		{
			throw new ArgumentNullException("typeName");
		}

		int length = this.GetIdentifierLength(typeName);
		if (length == -1)
		{
			throw new NotSupportedException();
		}
		// Create an identifier array of the correct full length based on
		// the object type
		object[] identifier = new object[length];

		// If the input identifier is not null, copy it to the full
		// identifier array.  If the input identifier's length is less
		// than the full length we assume the more specific parts are
		// specified and thus copy into the rightmost portion of the
		// full identifier array.
		if (partialIdentifier != null)
		{
			if (partialIdentifier.Length > length)
			{
				throw new InvalidOperationException();
			}

			partialIdentifier.CopyTo(identifier, length - partialIdentifier.Length);
		}

		if (length > 0)
		{
			identifier[0] = null;
		}

		if (length > 1)
		{
			identifier[1] = null;
		}

		return identifier;
	}

	#endregion

	#region · Private Methods ·

	private int GetIdentifierLength(string typeName)
	{
		// Diag.Dug("Type: " + typeName);

		switch (typeName)
		{
			case ObjectTypes.Root:
				return 0;

			case ObjectTypes.Table:
			case ObjectTypes.View:
			case ObjectTypes.StoredProcedure:
				return 3;

			case ObjectTypes.TableColumn:
			case ObjectTypes.ViewColumn:
			case ObjectTypes.StoredProcedureParameter:
				return 4;

			default:
				return -1;
		}
	}

	#endregion
}

