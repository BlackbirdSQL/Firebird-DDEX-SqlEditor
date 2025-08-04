// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.DatabaseObjectBase

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BlackbirdSql.LanguageExtension.Ctl.Config;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Sdk.Sfc;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.SqlParser.Metadata;
using Microsoft.SqlServer.Management.SqlParser.MetadataProvider;



namespace BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider;


// =========================================================================================================
//
//											AbstractDatabaseObject Class
//
/// <summary>
/// Impersonation of an SQL Server Smo DatabaseObjectBase for providing metadata.
/// </summary>
// =========================================================================================================
internal abstract class AbstractDatabaseObject
{

	// =========================================================================================================
	#region									Nested types - AbstractDatabaseObject
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Original interface: IMetadataList<T>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public interface IMetadataListI<T> : IEnumerable<T>, IEnumerable where T : NamedSmoObject
	{
		int Count { get; }
	}




	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Original class: SmoCollectionMetadataList<T>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public sealed class SmoCollectionMetadataListI<T> : IMetadataListI<T>, IEnumerable<T>, IEnumerable where T : NamedSmoObject
	{
		public SmoCollectionMetadataListI(LsbMetadataServer server, SmoCollectionBase smoCollection)
		{
			SmoConfig.SmoInitFields initFields = SmoConfig.SmoInitFields.GetInitFields(typeof(T));
			server.TryRefreshSmoCollection(smoCollection, initFields);
			this.smoCollection = smoCollection;
			count = GetCount(smoCollection, server.IsConnected);
		}




		private readonly SmoCollectionBase smoCollection;

		private readonly int count;

		public int Count => count;


		private static int GetCount(SmoCollectionBase smoCollectionBase, bool isConnected)
		{
			try
			{
				return smoCollectionBase.Count;
			}
			catch (InvalidVersionEnumeratorException)
			{
				return 0;
			}
			catch (UnsupportedVersionException)
			{
				return 0;
			}
			catch (Exception)
			{
				if (isConnected)
				{
					return 0;
				}
				throw;
			}
		}

		private static IEnumerator<T> GetEmptyEnumerator()
		{
			yield break;
		}

		public IEnumerator<T> GetEnumerator()
		{
			if (count <= 0)
			{
				return GetEmptyEnumerator();
			}
			return smoCollection.Cast<T>().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			if (count <= 0)
			{
				return GetEmptyEnumerator();
			}
			return smoCollection.GetEnumerator();
		}
	}




	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Original class: EnumerableMetadataList<T>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public sealed class EnumerableMetadataListI<T> : IMetadataListI<T>, IEnumerable<T>, IEnumerable where T : NamedSmoObject
	{
		private readonly IEnumerable<T> collection;

		public int Count => collection.Count();

		public EnumerableMetadataListI(IEnumerable<T> collection)
		{
			this.collection = collection;
		}

		public IEnumerator<T> GetEnumerator()
		{
			return collection.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return collection.GetEnumerator();
		}
	}




	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Original class: CollectionHelperBase<T, V>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public abstract class CollectionHelperBaseI<T, V> where T : class, IMetadataObject where V : IMetadataCollection<T>
	{
		private readonly object m_syncRoot = new object();

		private V m_metadataCollection;

		public V MetadataCollection
		{
			get
			{
				if (m_metadataCollection == null)
				{
					CreateAndSetMetadataCollection();
				}
				return m_metadataCollection;
			}
		}

		protected abstract LsbMetadataServer Server { get; }

		private void CreateAndSetMetadataCollection()
		{
			if (m_metadataCollection != null)
			{
				return;
			}
			lock (m_syncRoot)
			{
				if (m_metadataCollection != null)
				{
					return;
				}
				try
				{
					m_metadataCollection = CreateMetadataCollection();
				}
				catch (SmoException ex)
				{
					// Microsoft.SqlServer.Management.SmoMetadataProvider.TraceHelper.TraceContext.TraceCatch(ex);
					Diag.Ex(ex);
					m_metadataCollection = GetEmptyCollection();
				}
				catch (ConnectionException ex2)
				{
					// Microsoft.SqlServer.Management.SmoMetadataProvider.TraceHelper.TraceContext.TraceCatch(ex2);
					Diag.Ex(ex2);
					m_metadataCollection = GetEmptyCollection();
				}
				catch (Exception ex3)
				{
					if (Server.IsConnected)
					{
						// Microsoft.SqlServer.Management.SmoMetadataProvider.TraceHelper.TraceContext.TraceCatch(ex3);
						Diag.Ex(ex3);
						m_metadataCollection = GetEmptyCollection();
						return;
					}
					throw;
				}
			}
		}

		protected abstract V GetEmptyCollection();

		protected abstract V CreateMetadataCollection();
	}




	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Original class: MetadataListCollectionHelperBase<T, U, V>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public abstract class MetadataListCollectionHelperBaseI<T, U, V> : CollectionHelperBaseI<T, V> where T : class, IMetadataObject where U : NamedSmoObject where V : IMetadataCollection<T>
	{
		protected sealed override V CreateMetadataCollection()
		{
			IMetadataListI<U> metadataList = RetrieveSmoMetadataList();
			int count = metadataList.Count;
			Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo collationInfo = GetCollationInfo();
			switch (count)
			{
				case 0:
					return GetEmptyCollection();
				case 1:
					{
						U smoObject3 = null;
						int num2 = 0;
						foreach (U item in metadataList)
						{
							smoObject3 = item;
							num2++;
						}
						return CreateOneElementCollection(collationInfo, CreateMetadataObject(smoObject3));
					}
				case 2:
					{
						U smoObject = null;
						U smoObject2 = null;
						int num = 0;
						foreach (U item2 in metadataList)
						{
							if (num == 0)
							{
								smoObject = item2;
							}
							else
							{
								smoObject2 = item2;
							}
							num++;
						}
						return CreateTwoElementsCollection(collationInfo, CreateMetadataObject(smoObject), CreateMetadataObject(smoObject2));
					}
				default:
					{
						IEnumerable<T> items = metadataList.Select(CreateMetadataObject);
						return CreateManyElementsCollection(collationInfo, items, count);
					}
			}
		}

		protected abstract Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo GetCollationInfo();

		protected abstract IMetadataListI<U> RetrieveSmoMetadataList();

		protected abstract V CreateOneElementCollection(Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo collationInfo, T item0);

		protected abstract V CreateTwoElementsCollection(Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo collationInfo, T item0, T item1);

		protected abstract V CreateManyElementsCollection(Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo collationInfo, IEnumerable<T> items, int count);

		protected abstract T CreateMetadataObject(U smoObject);
	}




	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Original class: UnorderedCollectionHelperBase<T, U>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public abstract class UnorderedCollectionHelperBaseI<T, U> : MetadataListCollectionHelperBaseI<T, U, IMetadataCollection<T>> where T : class, IMetadataObject where U : NamedSmoObject
	{
		protected sealed override IMetadataCollection<T> GetEmptyCollection()
		{
			return Collection<T>.Empty;
		}

		protected sealed override IMetadataCollection<T> CreateOneElementCollection(Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo collationInfo, T item0)
		{
			return Collection<T>.CreateOrderedCollection(collationInfo, item0);
		}

		protected sealed override IMetadataCollection<T> CreateTwoElementsCollection(Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo collationInfo, T item0, T item1)
		{
			return Collection<T>.CreateOrderedCollection(collationInfo, item0, item1);
		}

		protected sealed override IMetadataCollection<T> CreateManyElementsCollection(Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo collationInfo, IEnumerable<T> items, int count)
		{
			IMutableMetadataCollection<T> mutableMetadataCollection = CreateMutableCollection(count, collationInfo);
			mutableMetadataCollection.AddRange(items);
			return mutableMetadataCollection;
		}

		protected abstract IMutableMetadataCollection<T> CreateMutableCollection(int initialCapacity, Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo collationInfo);
	}




	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Original class: OrderedCollectionHelperBase<T, U>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public abstract class OrderedCollectionHelperBaseI<T, U> : MetadataListCollectionHelperBaseI<T, U, IMetadataOrderedCollection<T>> where T : class, IMetadataObject where U : NamedSmoObject
	{
		protected sealed override IMetadataOrderedCollection<T> GetEmptyCollection()
		{
			return Collection<T>.EmptyOrdered;
		}

		protected sealed override IMetadataOrderedCollection<T> CreateOneElementCollection(Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo collationInfo, T item0)
		{
			return Collection<T>.CreateOrderedCollection(collationInfo, item0);
		}

		protected sealed override IMetadataOrderedCollection<T> CreateTwoElementsCollection(Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo collationInfo, T item0, T item1)
		{
			return Collection<T>.CreateOrderedCollection(collationInfo, item0, item1);
		}

		protected sealed override IMetadataOrderedCollection<T> CreateManyElementsCollection(Microsoft.SqlServer.Management.SqlParser.Metadata.CollationInfo collationInfo, IEnumerable<T> items, int count)
		{
			return Collection<T>.CreateOrderedCollection(collationInfo, items.ToArray());
		}
	}


	#endregion Nested types
}
