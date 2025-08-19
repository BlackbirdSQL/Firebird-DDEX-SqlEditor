// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.Login


using System;
using Microsoft.SqlServer.Management.Smo;
// using Microsoft.SqlServer.Management.SmoMetadataProvider;
using Microsoft.SqlServer.Management.SqlParser.Metadata;



namespace BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider;

// [SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "TBC")]
// [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "TBC")]


// =========================================================================================================
//
//											AbstractSmoMetaLogin Class
//
/// <summary>
/// Impersonation of an SQL Server Smo Login for providing metadata.
/// </summary>
// =========================================================================================================
internal abstract class AbstractSmoMetaLogin : AbstractSmoMetaServerOwnedObject<Microsoft.SqlServer.Management.Smo.Login>, ILogin, IServerOwnedObject, IDatabaseObject, IMetadataObject
{
	private AbstractSmoMetaLogin(Microsoft.SqlServer.Management.Smo.Login smoMetadataObject, SmoMetaServer parent, Microsoft.SqlServer.Management.SqlParser.Metadata.LoginType loginType)
		: base(smoMetadataObject, parent)
	{
		_LoginType = loginType;
	}


	private readonly Microsoft.SqlServer.Management.SqlParser.Metadata.LoginType _LoginType;

	private ICredential _Credential;

	private IDatabase _DefaultDatabase;

	private bool _CredentialSet;

	private bool _DefaultDatabaseSet;

	public override int Id => _SmoMetadataObject.ID;

	public override bool IsSystemObject => _SmoMetadataObject.IsSystemObject;

	public Microsoft.SqlServer.Management.SqlParser.Metadata.LoginType LoginType => _LoginType;

	public abstract IAsymmetricKey AsymmetricKey { get; }

	public abstract ICertificate Certificate { get; }

	public abstract IPassword Password { get; }

	public ICredential Credential
	{
		get
		{
			if (!_CredentialSet)
			{
				Cmd.TryGetPropertyObject<string>(_SmoMetadataObject, "Credential", out string value);
				if (!string.IsNullOrEmpty(value))
				{
					_Credential = base.Server.Credentials[value];
				}
				_CredentialSet = true;
			}
			return _Credential;
		}
	}

	public IDatabase DefaultDatabase
	{
		get
		{
			if (!_DefaultDatabaseSet)
			{
				string defaultDatabase = _SmoMetadataObject.DefaultDatabase;
				if (!string.IsNullOrEmpty(defaultDatabase))
				{
					IServer parent = _Parent;
					_DefaultDatabase = SmoMetaServer.GetDatabase(parent, defaultDatabase);
				}
				_DefaultDatabaseSet = true;
			}
			return _DefaultDatabase;
		}
	}

	public string Language => Cmd.GetPropertyObject<string>(_SmoMetadataObject, "Language");

	public byte[] Sid
	{
		get
		{
			if (_LoginType == Microsoft.SqlServer.Management.SqlParser.Metadata.LoginType.Sql)
			{
				return Cmd.GetPropertyObject<byte[]>(_SmoMetadataObject, "Sid");
			}
			return null;
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

	public static AbstractSmoMetaLogin CreateLogin(Microsoft.SqlServer.Management.Smo.Login smoMetadataObject, SmoMetaServer parent)
	{
		return smoMetadataObject.LoginType switch
		{
			Microsoft.SqlServer.Management.Smo.LoginType.AsymmetricKey => new AsymmetricKeyLogin(smoMetadataObject, parent), 
			Microsoft.SqlServer.Management.Smo.LoginType.Certificate => new CertificateLogin(smoMetadataObject, parent), 
			Microsoft.SqlServer.Management.Smo.LoginType.SqlLogin => new SqlLogin(smoMetadataObject, parent), 
			Microsoft.SqlServer.Management.Smo.LoginType.WindowsGroup => new WindowsLogin(smoMetadataObject, parent), 
			Microsoft.SqlServer.Management.Smo.LoginType.WindowsUser => new WindowsLogin(smoMetadataObject, parent), 
			Microsoft.SqlServer.Management.Smo.LoginType.ExternalGroup => new ExternalLogin(smoMetadataObject, parent), 
			Microsoft.SqlServer.Management.Smo.LoginType.ExternalUser => new ExternalLogin(smoMetadataObject, parent), 
			_ => null, 
		};
	}



	private sealed class LoginPassword : IPassword
	{
		private readonly Microsoft.SqlServer.Management.Smo.Login _SmoMetadataObject;

		public string Value => null;

		public bool IsHashed => false;

		public bool MustChange
		{
			get
			{
				Cmd.TryGetPropertyValue((SqlSmoObject)_SmoMetadataObject, "MustChangePassword", out bool? value);
				return value.GetValueOrDefault();
			}
		}

		public bool CheckPolicy
		{
			get
			{
				Cmd.TryGetPropertyValue((SqlSmoObject)_SmoMetadataObject, "PasswordPolicyEnforced", out bool? value);
				return value.GetValueOrDefault();
			}
		}

		public bool CheckExpiration
		{
			get
			{
				Cmd.TryGetPropertyValue((SqlSmoObject)_SmoMetadataObject, "PasswordExpirationEnabled", out bool? value);
				return value.GetValueOrDefault();
			}
		}

		public LoginPassword(Microsoft.SqlServer.Management.Smo.Login smoMetadataObject)
		{
			_SmoMetadataObject = smoMetadataObject;
		}
	}

	private sealed class AsymmetricKeyLogin : AbstractSmoMetaLogin
	{
		private IAsymmetricKey _AsymmetricKey;

		public override IAsymmetricKey AsymmetricKey
		{
			get
			{
				if (_AsymmetricKey == null)
				{
					string asymmetricKey = _SmoMetadataObject.AsymmetricKey;
					_AsymmetricKey = _Parent.MasterDatabase.AsymmetricKeys[asymmetricKey];
				}
				return _AsymmetricKey;
			}
		}

		public override ICertificate Certificate => null;

		public override IPassword Password => null;

		public AsymmetricKeyLogin(Microsoft.SqlServer.Management.Smo.Login smoMetadataObject, SmoMetaServer parent)
			: base(smoMetadataObject, parent, Microsoft.SqlServer.Management.SqlParser.Metadata.LoginType.AsymmetricKey)
		{
		}
	}

	private sealed class CertificateLogin : AbstractSmoMetaLogin
	{
		private ICertificate _Certificate;

		public override IAsymmetricKey AsymmetricKey => null;

		public override ICertificate Certificate
		{
			get
			{
				if (_Certificate == null)
				{
					string certificate = _SmoMetadataObject.Certificate;
					_Certificate = _Parent.MasterDatabase.Certificates[certificate];
				}
				return _Certificate;
			}
		}

		public override IPassword Password => null;

		public CertificateLogin(Microsoft.SqlServer.Management.Smo.Login smoMetadataObject, SmoMetaServer parent)
			: base(smoMetadataObject, parent, Microsoft.SqlServer.Management.SqlParser.Metadata.LoginType.Certificate)
		{
		}
	}

	private sealed class SqlLogin : AbstractSmoMetaLogin
	{
		private IPassword _Password;

		public override IAsymmetricKey AsymmetricKey => null;

		public override ICertificate Certificate => null;

		public override IPassword Password
		{
			get
			{
				_Password ??= new LoginPassword(_SmoMetadataObject);
				return _Password;
			}
		}

		public SqlLogin(Microsoft.SqlServer.Management.Smo.Login smoMetadataObject, SmoMetaServer parent)
			: base(smoMetadataObject, parent, Microsoft.SqlServer.Management.SqlParser.Metadata.LoginType.Sql)
		{
		}
	}

	private sealed class WindowsLogin : AbstractSmoMetaLogin
	{
		public override IAsymmetricKey AsymmetricKey => null;

		public override ICertificate Certificate => null;

		public override IPassword Password => null;

		public WindowsLogin(Microsoft.SqlServer.Management.Smo.Login smoMetadataObject, SmoMetaServer parent)
			: base(smoMetadataObject, parent, Microsoft.SqlServer.Management.SqlParser.Metadata.LoginType.Windows)
		{
		}
	}

	private sealed class ExternalLogin : AbstractSmoMetaLogin
	{
		public override IAsymmetricKey AsymmetricKey => null;

		public override ICertificate Certificate => null;

		public override IPassword Password => null;

		public ExternalLogin(Microsoft.SqlServer.Management.Smo.Login smoMetadataObject, SmoMetaServer parent)
			: base(smoMetadataObject, parent, Microsoft.SqlServer.Management.SqlParser.Metadata.LoginType.External)
		{
		}
	}
}
