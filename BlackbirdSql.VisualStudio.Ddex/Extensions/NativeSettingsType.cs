#region Assembly Community.VisualStudio.Toolkit, Version=17.0.430.0, Culture=neutral, PublicKeyToken=79441d341a79572c
// C:\Users\GregChristos\.nuget\packages\community.visualstudio.toolkit.17\17.0.430\lib\net48\Community.VisualStudio.Toolkit.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

namespace BlackbirdSql.VisualStudio.Ddex.Extensions
{
	//
	// Summary:
	//     Data types of the properties that are stored inside the collections. This mostly
	//     mirror Microsoft.VisualStudio.Settings.SettingsType, but adds UInt32 and UInt64
	public enum NativeSettingsType
	{
		//
		// Summary:
		//     Data type used to store 4 byte (32 bits) properties which are Boolean and Int32.
		//     Note that Boolean is stored 1 byte in the .NET environment but as a property
		//     inside the SettingsStore, it is kept as 4 byte value and any value other than
		//     0 is converted to true and 0 is converted to false. NOTE: In .NET we need to
		//     explicitly use the unsigned methods to successfully store unsigned types. This
		//     enumeration adds Community.VisualStudio.Toolkit.NativeSettingsType.UInt32 for
		//     that purpose.
		Int32 = 1,
		//
		// Summary:
		//     Data type used to store 8 byte (64 bit) properties which are Int64. NOTE: In
		//     .NET we need to explicitly use the unsigned methods to successfully store unsigned
		//     types. This enumeration adds Community.VisualStudio.Toolkit.NativeSettingsType.UInt64
		//     for that purpose.
		Int64,
		//
		// Summary:
		//     Data type used to store the strings.
		String,
		//
		// Summary:
		//     Data type used to store byte streams (arrays).
		Binary,
		//
		// Summary:
		//     Data type used to store 4 byte (32 bits) properties which is UInt32. NOTE: This
		//     value is not in Microsoft.VisualStudio.Settings.SettingsType, but is necessary
		//     so we can use the appropriate methods to successfully store unsigned types.
		UInt32,
		//
		// Summary:
		//     Data type used to store 8 byte (64 bit) properties which is UInt64. NOTE: This
		//     value is not in Microsoft.VisualStudio.Settings.SettingsType, but is necessary
		//     so we can use the appropriate methods to successfully store unsigned types.
		UInt64
	}
}
#if false // Decompilation log
'213' items in cache
------------------
Resolve: 'mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Found single assembly: 'mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Load from: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\mscorlib.dll'
------------------
Resolve: 'Microsoft.VisualStudio.Interop, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'Microsoft.VisualStudio.Interop, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Users\GregChristos\.nuget\packages\microsoft.visualstudio.interop\17.0.31902.203\lib\net472\Microsoft.VisualStudio.Interop.dll'
------------------
Resolve: 'Microsoft.VisualStudio.Text.UI.Wpf, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'Microsoft.VisualStudio.Text.UI.Wpf, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Users\GregChristos\.nuget\packages\microsoft.visualstudio.text.ui.wpf\17.0.487\lib\net472\Microsoft.VisualStudio.Text.UI.Wpf.dll'
------------------
Resolve: 'Microsoft.VisualStudio.Threading, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'Microsoft.VisualStudio.Threading, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Users\GregChristos\.nuget\packages\microsoft.visualstudio.threading\17.0.64\lib\net472\Microsoft.VisualStudio.Threading.dll'
------------------
Resolve: 'Microsoft.VisualStudio.Shell.Framework, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'Microsoft.VisualStudio.Shell.Framework, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Users\GregChristos\.nuget\packages\microsoft.visualstudio.shell.framework\17.0.31902.203\lib\net472\Microsoft.VisualStudio.Shell.Framework.dll'
------------------
Resolve: 'Microsoft.VisualStudio.Text.Logic, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'Microsoft.VisualStudio.Text.Logic, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Users\GregChristos\.nuget\packages\microsoft.visualstudio.text.logic\17.0.487\lib\net472\Microsoft.VisualStudio.Text.Logic.dll'
------------------
Resolve: 'Microsoft.VisualStudio.Text.Data, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'Microsoft.VisualStudio.Text.Data, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Users\GregChristos\.nuget\packages\microsoft.visualstudio.text.data\17.0.487\lib\net472\Microsoft.VisualStudio.Text.Data.dll'
------------------
Resolve: 'Microsoft.VisualStudio.Text.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'Microsoft.VisualStudio.Text.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Users\GregChristos\.nuget\packages\microsoft.visualstudio.text.ui\17.0.487\lib\net472\Microsoft.VisualStudio.Text.UI.dll'
------------------
Resolve: 'Microsoft.VisualStudio.Shell.15.0, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'Microsoft.VisualStudio.Shell.15.0, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Users\GregChristos\.nuget\packages\microsoft.visualstudio.shell.15.0\17.0.31902.203\lib\net472\Microsoft.VisualStudio.Shell.15.0.dll'
------------------
Resolve: 'PresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
Could not find by name: 'PresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
------------------
Resolve: 'Microsoft.VisualStudio.Imaging.Interop.14.0.DesignTime, Version=14.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'Microsoft.VisualStudio.Imaging.Interop.14.0.DesignTime, Version=14.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Users\GregChristos\.nuget\packages\microsoft.visualstudio.imaging.interop.14.0.designtime\17.0.31902.203\lib\net472\Microsoft.VisualStudio.Imaging.Interop.14.0.DesignTime.dll'
------------------
Resolve: 'System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Could not find by name: 'System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
------------------
Resolve: 'PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
Could not find by name: 'PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
------------------
Resolve: 'System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Found single assembly: 'System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Load from: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\System.dll'
------------------
Resolve: 'System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Found single assembly: 'System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Load from: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\System.Core.dll'
------------------
Resolve: 'Microsoft.VisualStudio.Package.LanguageService.15.0, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'Microsoft.VisualStudio.Package.LanguageService.15.0, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Users\GregChristos\.nuget\packages\microsoft.visualstudio.package.languageservice.15.0\17.0.31902.203\lib\net45\Microsoft.VisualStudio.Package.LanguageService.15.0.dll'
------------------
Resolve: 'System.ComponentModel.Composition, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Found single assembly: 'System.ComponentModel.Composition, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Load from: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\System.ComponentModel.Composition.dll'
------------------
Resolve: 'Microsoft.VisualStudio.Language, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'Microsoft.VisualStudio.Language, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Users\GregChristos\.nuget\packages\microsoft.visualstudio.language\17.0.487\lib\net472\Microsoft.VisualStudio.Language.dll'
------------------
Resolve: 'System.Collections.Immutable, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Collections.Immutable, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Users\GregChristos\.nuget\packages\system.collections.immutable\5.0.0\lib\net461\System.Collections.Immutable.dll'
------------------
Resolve: 'System.Xml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Could not find by name: 'System.Xml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
------------------
Resolve: 'Microsoft.VisualStudio.ComponentModelHost, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'Microsoft.VisualStudio.ComponentModelHost, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Users\GregChristos\.nuget\packages\microsoft.visualstudio.componentmodelhost\17.0.487\lib\net472\Microsoft.VisualStudio.ComponentModelHost.dll'
------------------
Resolve: 'Microsoft.VisualStudio.CoreUtility, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'Microsoft.VisualStudio.CoreUtility, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Users\GregChristos\.nuget\packages\microsoft.visualstudio.coreutility\17.0.487\lib\net472\Microsoft.VisualStudio.CoreUtility.dll'
------------------
Resolve: 'WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
Could not find by name: 'WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
------------------
Resolve: 'Microsoft.VisualStudio.Editor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'Microsoft.VisualStudio.Editor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Users\GregChristos\.nuget\packages\microsoft.visualstudio.editor\17.0.487\lib\net472\Microsoft.VisualStudio.Editor.dll'
------------------
Resolve: 'Microsoft.VisualStudio.Validation, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'Microsoft.VisualStudio.Validation, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Users\GregChristos\.nuget\packages\microsoft.visualstudio.validation\17.0.28\lib\netstandard2.0\Microsoft.VisualStudio.Validation.dll'
------------------
Resolve: 'Microsoft.VisualStudio.Imaging, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'Microsoft.VisualStudio.Imaging, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Users\GregChristos\.nuget\packages\microsoft.visualstudio.imaging\17.0.31902.203\lib\net472\Microsoft.VisualStudio.Imaging.dll'
------------------
Resolve: 'Microsoft.VisualStudio.ImageCatalog, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'Microsoft.VisualStudio.ImageCatalog, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Users\GregChristos\.nuget\packages\microsoft.visualstudio.imagecatalog\17.0.31902.203\lib\net472\Microsoft.VisualStudio.ImageCatalog.dll'
------------------
Resolve: 'Microsoft.VisualStudio.Utilities, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'Microsoft.VisualStudio.Utilities, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Users\GregChristos\.nuget\packages\microsoft.visualstudio.utilities\17.0.31902.203\lib\net472\Microsoft.VisualStudio.Utilities.dll'
#endif
