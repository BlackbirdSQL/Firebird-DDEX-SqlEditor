#region Assembly Microsoft.Cosmos.ClientTools.Common.ColorService, Version=2.6.5000.0, Culture=neutral, PublicKeyToken=f300afd708cefcd3
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\ADL Tools\2.6.5000.0\Microsoft.Cosmos.ClientTools.Common.ColorService.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Windows.Media;

using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using BlackbirdSql.Core;
using BlackbirdSql.LanguageExtension.Interfaces;


// using Microsoft.Cosmos.ClientTools.Common.ColorService;
// using Ns = Microsoft.Cosmos.ClientTools.Common.ColorService;


// namespace Microsoft.Cosmos.ClientTools.Common.ColorService
namespace BlackbirdSql.LanguageExtension.ColorService
{
	/// <summary>
	/// Plagiarized off of Microsoft.Cosmos.ClientTools.Common.ColorService.ColorService
	/// </summary>
	public abstract class AbstractColorService : IBColorService
	{
		protected static IBColorService _Instance;
		protected IVsUIShell5 _Shell;

		public static readonly Guid Category = new Guid(ServiceData.ColorServiceCategoryGuid);

		public static IBColorService Instance
		{
			get
			{
				if (_Instance == null)
				{
					try
					{
						_Instance = Package.GetGlobalService(typeof(IBColorService)) as IBColorService;
					}
					catch (Exception)
					{
					}
				}

				return _Instance;
			}
		}

		protected AbstractColorService(IVsUIShell5 uiShell)
		{
			_Shell = uiShell;
		}

		public System.Windows.Media.Color GetColor(Guid category, string key, int type)
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			return _Shell.GetThemedWPFColor(DoGetKey(category, key, type));
		}

		public System.Windows.Media.Color GetColor(object key)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			System.Windows.Media.Color result = Colors.Transparent;
			if (key is ThemeResourceKey themeResourceKey)
			{
				try
				{
					result = _Shell.GetThemedWPFColor(themeResourceKey);
					return result;
				}
				catch
				{
					return result;
				}
			}

			return result;
		}

		public System.Drawing.Color GetGDIColor(object key)
		{
			System.Drawing.Color result = System.Drawing.Color.Transparent;
			if (key is ThemeResourceKey themeResourceKey)
			{
				try
				{
					result = _Shell.GetThemedGDIColor(themeResourceKey);
					return result;
				}
				catch
				{
					return result;
				}
			}

			return result;
		}

		public object GetKey(Guid category, string key, int type)
		{
			return DoGetKey(category, key, type);
		}

		private ThemeResourceKey DoGetKey(Guid category, string key, int type)
		{
			return new ThemeResourceKey(category, key, (ThemeResourceKeyType)type);
		}

		public System.Windows.Media.Color GetToolWindowBackgroundColor()
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			return _Shell.GetThemedWPFColor(EnvironmentColors.ToolWindowBackgroundColorKey);
		}

		public System.Windows.Media.Color GetToolWindowTextColor()
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			return _Shell.GetThemedWPFColor(EnvironmentColors.ToolWindowTextColorKey);
		}
	}
}
