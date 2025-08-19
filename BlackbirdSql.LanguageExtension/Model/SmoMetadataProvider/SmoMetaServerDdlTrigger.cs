// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.ServerDdlTrigger

using System;
using Microsoft.SqlServer.Management.Smo;
// using Microsoft.SqlServer.Management.SmoMetadataProvider;
using Microsoft.SqlServer.Management.SqlParser.Metadata;



namespace BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider;


internal class SmoMetaServerDdlTrigger : AbstractSmoMetaServerOwnedObject<Microsoft.SqlServer.Management.Smo.ServerDdlTrigger>, IServerDdlTrigger, ITrigger, IMetadataObject, IServerOwnedObject, IDatabaseObject
{
	public SmoMetaServerDdlTrigger(Microsoft.SqlServer.Management.Smo.ServerDdlTrigger smoMetadataObject, SmoMetaServer parent)
		: base(smoMetadataObject, parent)
	{
	}



	private IExecutionContext _ExecutionContext;

	private TriggerEventTypeSet _ServerEventSet;

	public override int Id => _SmoMetadataObject.ID;

	public override bool IsSystemObject => _SmoMetadataObject.IsSystemObject;

	public ITriggerEventTypeSet ServerDdlEvents
	{
		get
		{
			if (_ServerEventSet == null)
			{
				_ServerEventSet ??= Cmd.DdlTriggerI.GetServerTriggerEvents(_SmoMetadataObject);
				return _ServerEventSet;
			}
			return _ServerEventSet;
		}
	}

	public bool IsQuotedIdentifierOn => _SmoMetadataObject.QuotedIdentifierStatus;

	public string BodyText => _SmoMetadataObject.TextBody;

	public bool IsEncrypted => _SmoMetadataObject.IsEncrypted;

	public bool IsEnabled => _SmoMetadataObject.IsEnabled;

	public bool IsSqlClr => _SmoMetadataObject.ImplementationType == ImplementationType.SqlClr;

	public IExecutionContext ExecutionContext
	{
		get
		{
			if (_ExecutionContext == null)
			{
				IServer server = base.Server;
				_ExecutionContext = Cmd.GetExecutionContext(server, _SmoMetadataObject);
			}
			return _ExecutionContext;
		}
	}


	public override T Accept<T>(IServerOwnedObjectVisitor<T> visitor)
	{
		if (visitor == null)
		{
			throw new ArgumentNullException("visitor");
		}
		return visitor.Visit(this);
	}
}
