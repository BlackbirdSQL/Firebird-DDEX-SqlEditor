// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.DmlTrigger

using System;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Smo.Agent;
using Microsoft.SqlServer.Management.SqlParser.Metadata;



namespace BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider;

// [SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "TBC")]
// [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "TBC")]


// =========================================================================================================
//
//											SmoMetaDmlTrigger Class
//
/// <summary>
/// Impersonation of an SQL Server Smo DmlTrigger for providing metadata.
/// </summary>
// =========================================================================================================
internal class SmoMetaDmlTrigger : IDmlTrigger, ITrigger, IMetadataObject, BlackbirdSql.LanguageExtension.Interfaces.IBsSmoDatabaseObject
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - SmoMetaDmlTrigger
	// ---------------------------------------------------------------------------------


	public SmoMetaDmlTrigger(ITableViewBase parent, Trigger smoTrigger)
	{
		_SmoTrigger = smoTrigger;
		_Parent = parent;
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - SmoMetaDmlTrigger
	// =========================================================================================================


	private readonly ITableViewBase _Parent;

	private readonly Trigger _SmoTrigger;

	private IExecutionContext _ExecutionContext;

	private string bodyText;

	private bool isBodyTextSet;



	#endregion Fields





	// =========================================================================================================
	#region Property accessors - SmoMetaDmlTrigger
	// =========================================================================================================


	public ITableViewBase Parent => _Parent;

	public bool NotForReplication => _SmoTrigger.NotForReplication;

	public bool InsteadOf => _SmoTrigger.InsteadOf;

	public bool Delete => _SmoTrigger.Delete;

	public bool Insert => _SmoTrigger.Insert;

	public bool Update => _SmoTrigger.Update;

	public Microsoft.SqlServer.Management.SqlParser.Metadata.ActivationOrder DeleteActivationOrder => GetActivationOrder(_SmoTrigger.DeleteOrder);

	public Microsoft.SqlServer.Management.SqlParser.Metadata.ActivationOrder InsertActivationOrder => GetActivationOrder(_SmoTrigger.InsertOrder);

	public Microsoft.SqlServer.Management.SqlParser.Metadata.ActivationOrder UpdateActivationOrder => GetActivationOrder(_SmoTrigger.UpdateOrder);

	public bool IsQuotedIdentifierOn
	{
		get
		{
			if (!IsSqlClr)
			{
				return _SmoTrigger.QuotedIdentifierStatus;
			}
			return false;
		}
	}

	public string BodyText
	{
		get
		{
			if (HasBodyText() && !isBodyTextSet)
			{
				if (Cmd.TryGetPropertyObject<string>(_SmoTrigger, "Text", out var value))
				{
					bodyText = SmoMetaDmlTrigger.RetrieveTriggerBody(value, IsQuotedIdentifierOn);
				}
				else
				{
					bodyText = null;
				}
				isBodyTextSet = true;
			}
			return bodyText;
		}
	}

	public bool IsEncrypted => _SmoTrigger.IsEncrypted;

	public bool IsEnabled => _SmoTrigger.IsEnabled;

	public bool IsSqlClr => _SmoTrigger.ImplementationType == ImplementationType.SqlClr;

	public IExecutionContext ExecutionContext
	{
		get
		{
			if (_ExecutionContext == null)
			{
				IDatabase database = _Parent.Schema.Database;
				_ExecutionContext = Cmd.GetExecutionContext(database, _SmoTrigger);
			}
			return _ExecutionContext;
		}
	}

	public string Name => _SmoTrigger.Name;

	public SqlSmoObject SmoObject => _SmoTrigger;


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - SmoMetaDmlTrigger
	// =========================================================================================================


	public T Accept<T>(IMetadataObjectVisitor<T> visitor)
	{
		if (visitor == null)
		{
			throw new ArgumentNullException("visitor");
		}
		return visitor.Visit(this);
	}

	private static Microsoft.SqlServer.Management.SqlParser.Metadata.ActivationOrder GetActivationOrder(Microsoft.SqlServer.Management.Smo.Agent.ActivationOrder smoActivationOrder)
	{
		return smoActivationOrder switch
		{
			Microsoft.SqlServer.Management.Smo.Agent.ActivationOrder.None => Microsoft.SqlServer.Management.SqlParser.Metadata.ActivationOrder.None, 
			Microsoft.SqlServer.Management.Smo.Agent.ActivationOrder.First => Microsoft.SqlServer.Management.SqlParser.Metadata.ActivationOrder.First, 
			Microsoft.SqlServer.Management.Smo.Agent.ActivationOrder.Last => Microsoft.SqlServer.Management.SqlParser.Metadata.ActivationOrder.Last, 
			_ => Microsoft.SqlServer.Management.SqlParser.Metadata.ActivationOrder.None, 
		};
	}

	private bool HasBodyText()
	{
		if (!IsSqlClr)
		{
			return !IsEncrypted;
		}
		return false;
	}


	public static string RetrieveTriggerBody(string sql, bool isQuotedIdentifierOn)
	{
		return Cmd.RetrieveModuleBody(sql, isQuotedIdentifierOn, isTrigger: true);
	}


	#endregion Methods





	// =========================================================================================================
	#region									Nested types - SmoMetaDmlTrigger
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Original class: Utils.DmlTriggerCollectionHelper.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public class CollectionHelperI : Cmd.OrderedCollectionHelperI<IDmlTrigger, Trigger>
	{
		public CollectionHelperI(SmoMetaDatabase database, ITableViewBase dbTable, TriggerCollection smoCollection)
			: base(database)
		{
			this.dbTable = dbTable;
			this.smoCollection = smoCollection;
		}


		private readonly TriggerCollection smoCollection;

		private readonly ITableViewBase dbTable;


		protected override AbstractSmoMetaDatabaseObjectBase.IMetadataListI<Trigger> RetrieveSmoMetadataList()
		{
			return new AbstractSmoMetaDatabaseObjectBase.SmoCollectionMetadataListI<Trigger>(_Database.Server, smoCollection);
		}

		protected override IDmlTrigger CreateMetadataObject(Trigger smoObject)
		{
			return new SmoMetaDmlTrigger(dbTable, smoObject);
		}
	}


	#endregion Nested types

}
