// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.User

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.SqlParser.Metadata;



namespace BlackbirdSql.LanguageExtension.Model.SmoMetadataProvider;

// [SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "TBC")]
// [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "TBC")]


// =========================================================================================================
//
//											AbstractSmoMetaUser Class
//
/// <summary>
/// Impersonation of an SQL Server Smo User for providing metadata.
/// </summary>
// =========================================================================================================
internal abstract class AbstractSmoMetaUser : AbstractSmoMetaDatabasePrincipal<Microsoft.SqlServer.Management.Smo.User>, IUser, IDatabasePrincipal, IDatabaseOwnedObject, IDatabaseObject, IMetadataObject
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - AbstractSmoMetaUser
	// ---------------------------------------------------------------------------------


	public AbstractSmoMetaUser(Microsoft.SqlServer.Management.Smo.User smoMetadataObject, SmoMetaDatabase parent, Microsoft.SqlServer.Management.SqlParser.Metadata.UserType userType)
		: base(smoMetadataObject, parent)
	{
		_UserType = userType;
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - AbstractSmoMetaUser
	// =========================================================================================================


	private readonly Microsoft.SqlServer.Management.SqlParser.Metadata.UserType _UserType;

	private ISchema _DefaultSchema;

	private bool _DefaultSchemaIsSet;



	#endregion Fields





	// =========================================================================================================
	#region Property accessors - AbstractSmoMetaUser
	// =========================================================================================================


	public override int Id => _SmoMetadataObject.ID;

	public override bool IsSystemObject => _SmoMetadataObject.IsSystemObject;

	public Microsoft.SqlServer.Management.SqlParser.Metadata.UserType UserType => _UserType;

	public abstract IAsymmetricKey AsymmetricKey { get; }

	public abstract ICertificate Certificate { get; }

	public abstract ILogin Login { get; }

	public abstract string Password { get; }

	public ISchema DefaultSchema
	{
		get
		{
			if (!_DefaultSchemaIsSet)
			{
				Cmd.TryGetPropertyObject<string>(_SmoMetadataObject, "DefaultSchema", out var value);
				if (value != null)
				{
					_DefaultSchema = base.Database.Schemas[value];
				}
				_DefaultSchemaIsSet = true;
			}
			return _DefaultSchema;
		}
	}


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - AbstractSmoMetaUser
	// =========================================================================================================


	public override T Accept<T>(IDatabaseOwnedObjectVisitor<T> visitor)
	{
		if (visitor == null)
		{
			throw new ArgumentNullException("visitor");
		}
		return visitor.Visit(this);
	}

	public static AbstractSmoMetaUser CreateUser(Microsoft.SqlServer.Management.Smo.User smoMetadataObject, SmoMetaDatabase parent)
	{
		switch (smoMetadataObject.UserType)
		{
		case Microsoft.SqlServer.Management.Smo.UserType.AsymmetricKey:
			return new AsymmetricKeyUserI(smoMetadataObject, parent);
		case Microsoft.SqlServer.Management.Smo.UserType.Certificate:
			return new CertificateUserI(smoMetadataObject, parent);
		case Microsoft.SqlServer.Management.Smo.UserType.External:
			return new NoLoginUserI(smoMetadataObject, parent, Microsoft.SqlServer.Management.SqlParser.Metadata.UserType.External);
		case Microsoft.SqlServer.Management.Smo.UserType.NoLogin:
			return new NoLoginUserI(smoMetadataObject, parent);
		case Microsoft.SqlServer.Management.Smo.UserType.SqlLogin:
		{
			Cmd.TryGetPropertyValue((SqlSmoObject)smoMetadataObject, "AuthenticationType", out AuthenticationType? value);
			if (value.HasValue && value.Value == AuthenticationType.Database)
			{
				return new PasswordUserI(smoMetadataObject, parent);
			}
			return new SqlLoginUserI(smoMetadataObject, parent);
		}
		default:
			return null;
		}
	}

	protected override IEnumerable<string> GetMemberOfRoleNames()
	{
		return _SmoMetadataObject.EnumRoles().Cast<string>();
	}


	#endregion Methods





	// =========================================================================================================
	#region									Nested types - AbstractSmoMetaUser
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Original class: AsymmetricKeyUser.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private sealed class AsymmetricKeyUserI : AbstractSmoMetaUser
	{
		public AsymmetricKeyUserI(Microsoft.SqlServer.Management.Smo.User smoMetadataObject, SmoMetaDatabase parent)
			: base(smoMetadataObject, parent, Microsoft.SqlServer.Management.SqlParser.Metadata.UserType.AsymmetricKey)
		{
		}


		private IAsymmetricKey _AsymmetricKey;

		public override IAsymmetricKey AsymmetricKey
		{
			get
			{
				if (_AsymmetricKey == null)
				{
					string asymmetricKey = _SmoMetadataObject.AsymmetricKey;
					_AsymmetricKey = base.Database.AsymmetricKeys[asymmetricKey];
				}
				return _AsymmetricKey;
			}
		}

		public override ICertificate Certificate => null;

		public override ILogin Login => null;

		public override string Password => null;

	}




	private sealed class CertificateUserI : AbstractSmoMetaUser
	{
		public CertificateUserI(Microsoft.SqlServer.Management.Smo.User smoMetadataObject, SmoMetaDatabase parent)
			: base(smoMetadataObject, parent, Microsoft.SqlServer.Management.SqlParser.Metadata.UserType.Certificate)
		{
		}


		private ICertificate _Certificate;

		public override IAsymmetricKey AsymmetricKey => null;

		public override ICertificate Certificate
		{
			get
			{
				if (_Certificate == null)
				{
					string certificate = _SmoMetadataObject.Certificate;
					_Certificate = base.Database.Certificates[certificate];
				}
				return _Certificate;
			}
		}

		public override ILogin Login => null;

		public override string Password => null;

	}



	private sealed class NoLoginUserI : AbstractSmoMetaUser
	{
		public NoLoginUserI(Microsoft.SqlServer.Management.Smo.User smoMetadataObject, SmoMetaDatabase parent, Microsoft.SqlServer.Management.SqlParser.Metadata.UserType userType = Microsoft.SqlServer.Management.SqlParser.Metadata.UserType.NoLogin)
			: base(smoMetadataObject, parent, userType)
		{
		}


		public override IAsymmetricKey AsymmetricKey => null;

		public override ICertificate Certificate => null;

		public override ILogin Login => null;

		public override string Password => null;

	}




	private sealed class SqlLoginUserI : AbstractSmoMetaUser
	{
		public SqlLoginUserI(Microsoft.SqlServer.Management.Smo.User smoMetadataObject, SmoMetaDatabase parent)
			: base(smoMetadataObject, parent, Microsoft.SqlServer.Management.SqlParser.Metadata.UserType.SqlLogin)
		{
		}


		private ILogin _Login;

		private bool _LoginIsSet;

		public override IAsymmetricKey AsymmetricKey => null;

		public override ICertificate Certificate => null;

		public override ILogin Login
		{
			get
			{
				if (!_LoginIsSet)
				{
					string login = _SmoMetadataObject.Login;
					_Login = base.Database.Server.Logins[login];
					_LoginIsSet = true;
				}
				return _Login;
			}
		}

		public override string Password => null;

	}




	private sealed class PasswordUserI : AbstractSmoMetaUser
	{
		public PasswordUserI(Microsoft.SqlServer.Management.Smo.User smoMetadataObject, SmoMetaDatabase parent)
			: base(smoMetadataObject, parent, Microsoft.SqlServer.Management.SqlParser.Metadata.UserType.Password)
		{
		}


		public override IAsymmetricKey AsymmetricKey => null;

		public override ICertificate Certificate => null;

		public override ILogin Login => null;

		public override string Password => null;

	}


	#endregion Nested types

}
