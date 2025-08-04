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
//											LsbDmlTrigger Class
//
/// <summary>
/// Impersonation of an SQL Server Smo DmlTrigger for providing metadata.
/// </summary>
// =========================================================================================================
internal class LsbDmlTrigger : IDmlTrigger, ITrigger, IMetadataObject, BlackbirdSql.LanguageExtension.Interfaces.IBsSmoDatabaseObject
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - LsbDmlTrigger
	// ---------------------------------------------------------------------------------


	public LsbDmlTrigger(ITableViewBase parent, Trigger smoTrigger)
	{
		m_smoTrigger = smoTrigger;
		m_parent = parent;
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - LsbDmlTrigger
	// =========================================================================================================


	private readonly ITableViewBase m_parent;

	private readonly Trigger m_smoTrigger;

	private IExecutionContext m_executionContext;

	private string bodyText;

	private bool isBodyTextSet;



	#endregion Fields





	// =========================================================================================================
	#region Property accessors - LsbDmlTrigger
	// =========================================================================================================


	public ITableViewBase Parent => m_parent;

	public bool NotForReplication => m_smoTrigger.NotForReplication;

	public bool InsteadOf => m_smoTrigger.InsteadOf;

	public bool Delete => m_smoTrigger.Delete;

	public bool Insert => m_smoTrigger.Insert;

	public bool Update => m_smoTrigger.Update;

	public Microsoft.SqlServer.Management.SqlParser.Metadata.ActivationOrder DeleteActivationOrder => GetActivationOrder(m_smoTrigger.DeleteOrder);

	public Microsoft.SqlServer.Management.SqlParser.Metadata.ActivationOrder InsertActivationOrder => GetActivationOrder(m_smoTrigger.InsertOrder);

	public Microsoft.SqlServer.Management.SqlParser.Metadata.ActivationOrder UpdateActivationOrder => GetActivationOrder(m_smoTrigger.UpdateOrder);

	public bool IsQuotedIdentifierOn
	{
		get
		{
			if (!IsSqlClr)
			{
				return m_smoTrigger.QuotedIdentifierStatus;
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
				if (Cmd.TryGetPropertyObject<string>(m_smoTrigger, "Text", out var value))
				{
					bodyText = LsbDmlTrigger.RetrieveTriggerBody(value, IsQuotedIdentifierOn);
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

	public bool IsEncrypted => m_smoTrigger.IsEncrypted;

	public bool IsEnabled => m_smoTrigger.IsEnabled;

	public bool IsSqlClr => m_smoTrigger.ImplementationType == ImplementationType.SqlClr;

	public IExecutionContext ExecutionContext
	{
		get
		{
			if (m_executionContext == null)
			{
				IDatabase database = m_parent.Schema.Database;
				m_executionContext = Cmd.GetExecutionContext(database, m_smoTrigger);
			}
			return m_executionContext;
		}
	}

	public string Name => m_smoTrigger.Name;

	public SqlSmoObject SmoObject => m_smoTrigger;


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - LsbDmlTrigger
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
	#region									Nested types - LsbDmlTrigger
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Original class: Utils.DmlTriggerCollectionHelper.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public class CollectionHelperI : Cmd.OrderedCollectionHelperI<IDmlTrigger, Trigger>
	{
		public CollectionHelperI(LsbDatabase database, ITableViewBase dbTable, TriggerCollection smoCollection)
			: base(database)
		{
			this.dbTable = dbTable;
			this.smoCollection = smoCollection;
		}


		private readonly TriggerCollection smoCollection;

		private readonly ITableViewBase dbTable;


		protected override AbstractDatabaseObject.IMetadataListI<Trigger> RetrieveSmoMetadataList()
		{
			return new AbstractDatabaseObject.SmoCollectionMetadataListI<Trigger>(m_database.Server, smoCollection);
		}

		protected override IDmlTrigger CreateMetadataObject(Trigger smoObject)
		{
			return new LsbDmlTrigger(dbTable, smoObject);
		}
	}


	#endregion Nested types

}
