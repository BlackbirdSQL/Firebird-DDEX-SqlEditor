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

// Guids.cs
// MUST match guids.h

using System;
using System.Runtime.InteropServices;

namespace BlackbirdSql.VisualStudio.Ddex.Configuration;

static class PackageData
{
	// Non language based required constant strings
	public const string ServiceName = "BlackbirdSql DDEX 2.0 Provider Object Factory";

	// public const string CodeBase = "%ProgramFiles%\\BlackbirdSql\\BlackbirdDDEX.9.1.0\\BlackbirdSql.VisualStudio.Ddex.dll";

	// Guids
	public const string AssemblyGuid = "D5A9B07D-5302-42A5-9509-F877DEC4BEDB";

	public const string PackageGuid = "c21e1c58-3772-4572-88e9-0f2188268741";
	public const string PackageGuideNET = "7787981E-E42A-412F-A42B-9AD07A7DE169";

	public const string EdmxUIContextRuleGuid = "{e000c7e5-dba5-4682-abe0-7f6ce57b236d}";
	public const string ShellInitializedContextRuleGuid = "{E80EF1CB-6D64-4609-8FAA-FEACFD3BC89F}";

	public const string ProviderGuid = "43015F6E-757F-408B-966E-C2BCE34686BA";
	public const string ProviderGuidNET = "66F5BB69-4C70-4319-8947-A2E0643A4CE";

	public const string ObjectFactoryServiceGuid = "B0640FC7-F798-4CC0-81F9-2587762D4957";
	public const string ObjectFactoryServiceGuidNET = "AE2CB68C-0AA2-46A7-910A-CBDA1464DCB0";



	// Security tokens

	public const string PublicTokenString = "d39a163eb11ac91a";
	public const string PublicTokenStringNET = "";

	public const string PublicHashString = "002400000480000094000000060200000024000052534131000400000100010099b99763c990a25eb0fad128c99cefa4dd9716e5edd609fcc245d0e19fdbcc5b4ac8b1f33349a0a231cc5d0e7702e8289e29d6f6e28074e3e844b24726c7368151dcfa97d109de847521febfead7937cae2933418583cc97630263d849425645721ef381de3c33ef27d3d01c805a8082721f94d5e664c09390f3a3fbf9faa9ca";
	public const string PublicHashStringNET = "";


};