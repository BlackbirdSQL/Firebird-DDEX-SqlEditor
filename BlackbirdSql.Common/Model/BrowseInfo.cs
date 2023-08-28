#region Assembly Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using BlackbirdSql.Core;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Common.Properties;



namespace BlackbirdSql.Common.Model
{
	public class BrowseInfo : AbstractDispatcherConnection
	{

		// private AuthenticationTypes _defaultAuthenticationType;

		/*
		public AuthenticationTypes DefaultDefaultAuthenticationType
		{
			get
			{
				return _defaultAuthenticationType;
			}
			set
			{
				SetAndRaisePropertyChanged(ref _defaultAuthenticationType, value, "DefaultDefaultAuthenticationType");
			}
		}
		*/



		public bool HasShortName => !string.IsNullOrWhiteSpace(Dataset);

		public string ListItemAutomationName
		{
			get
			{
				if (!HasShortName)
				{
					return DataSource;
				}

				return string.Format(SharedResx.ListViewItemAutomationProperty, Dataset, DataSource);
			}
		}

		protected BrowseInfo(BrowseInfo rhs, bool generateNewId) : base(rhs, generateNewId)
		{
		}

		protected BrowseInfo() : this(null, true)
		{
		}


		public override IBPropertyAgent Copy()
		{
			return new BrowseInfo(this, true);
		}



		protected static new void CreateAndPopulatePropertySet(DescriberDictionary describers = null)
		{
			// This class does not have private descriptors. Just pass request down.
			AbstractModelPropertyAgent.CreateAndPopulatePropertySet(describers);
		}


	}
}
