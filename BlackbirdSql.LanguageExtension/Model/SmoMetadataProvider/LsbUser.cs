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
//											LsbUser Class
//
/// <summary>
/// Impersonation of an SQL Server Smo User for providing metadata.
/// </summary>
// =========================================================================================================
internal abstract class LsbUser : LsbDatabasePrincipal<Microsoft.SqlServer.Management.Smo.User>, IUser, IDatabasePrincipal, IDatabaseOwnedObject, IDatabaseObject, IMetadataObject
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - LsbUser
	// ---------------------------------------------------------------------------------


	public LsbUser(Microsoft.SqlServer.Management.Smo.User smoMetadataObject, LsbDatabase parent, Microsoft.SqlServer.Management.SqlParser.Metadata.UserType userType)
		: base(smoMetadataObject, parent)
	{
		m_userType = userType;
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - LsbUser
	// =========================================================================================================


	private readonly Microsoft.SqlServer.Management.SqlParser.Metadata.UserType m_userType;

	private ISchema m_defaultSchema;

	private bool m_defaultSchemaIsSet;



	#endregion Fields





	// =========================================================================================================
	#region Property accessors - LsbUser
	// =========================================================================================================


	public override int Id => m_smoMetadataObject.ID;

	public override bool IsSystemObject => m_smoMetadataObject.IsSystemObject;

	public Microsoft.SqlServer.Management.SqlParser.Metadata.UserType UserType => m_userType;

	public abstract IAsymmetricKey AsymmetricKey { get; }

	public abstract ICertificate Certificate { get; }

	public abstract ILogin Login { get; }

	public abstract string Password { get; }

	public ISchema DefaultSchema
	{
		get
		{
			if (!m_defaultSchemaIsSet)
			{
				Cmd.TryGetPropertyObject<string>(m_smoMetadataObject, "DefaultSchema", out var value);
				if (value != null)
				{
					m_defaultSchema = base.Database.Schemas[value];
				}
				m_defaultSchemaIsSet = true;
			}
			return m_defaultSchema;
		}
	}


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - LsbUser
	// =========================================================================================================


	public override T Accept<T>(IDatabaseOwnedObjectVisitor<T> visitor)
	{
		if (visitor == null)
		{
			throw new ArgumentNullException("visitor");
		}
		return visitor.Visit(this);
	}

	public static LsbUser CreateUser(Microsoft.SqlServer.Management.Smo.User smoMetadataObject, LsbDatabase parent)
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
		return m_smoMetadataObject.EnumRoles().Cast<string>();
	}


	#endregion Methods





	// =========================================================================================================
	#region									Nested types - LsbUser
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Original class: AsymmetricKeyUser.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private sealed class AsymmetricKeyUserI : LsbUser
	{
		public AsymmetricKeyUserI(Microsoft.SqlServer.Management.Smo.User smoMetadataObject, LsbDatabase parent)
			: base(smoMetadataObject, parent, Microsoft.SqlServer.Management.SqlParser.Metadata.UserType.AsymmetricKey)
		{
		}


		private IAsymmetricKey m_asymmetricKey;

		public override IAsymmetricKey AsymmetricKey
		{
			get
			{
				if (m_asymmetricKey == null)
				{
					string asymmetricKey = m_smoMetadataObject.AsymmetricKey;
					m_asymmetricKey = base.Database.AsymmetricKeys[asymmetricKey];
				}
				return m_asymmetricKey;
			}
		}

		public override ICertificate Certificate => null;

		public override ILogin Login => null;

		public override string Password => null;

	}




	private sealed class CertificateUserI : LsbUser
	{
		public CertificateUserI(Microsoft.SqlServer.Management.Smo.User smoMetadataObject, LsbDatabase parent)
			: base(smoMetadataObject, parent, Microsoft.SqlServer.Management.SqlParser.Metadata.UserType.Certificate)
		{
		}


		private ICertificate m_certificate;

		public override IAsymmetricKey AsymmetricKey => null;

		public override ICertificate Certificate
		{
			get
			{
				if (m_certificate == null)
				{
					string certificate = m_smoMetadataObject.Certificate;
					m_certificate = base.Database.Certificates[certificate];
				}
				return m_certificate;
			}
		}

		public override ILogin Login => null;

		public override string Password => null;

	}



	private sealed class NoLoginUserI : LsbUser
	{
		public NoLoginUserI(Microsoft.SqlServer.Management.Smo.User smoMetadataObject, LsbDatabase parent, Microsoft.SqlServer.Management.SqlParser.Metadata.UserType userType = Microsoft.SqlServer.Management.SqlParser.Metadata.UserType.NoLogin)
			: base(smoMetadataObject, parent, userType)
		{
		}


		public override IAsymmetricKey AsymmetricKey => null;

		public override ICertificate Certificate => null;

		public override ILogin Login => null;

		public override string Password => null;

	}




	private sealed class SqlLoginUserI : LsbUser
	{
		public SqlLoginUserI(Microsoft.SqlServer.Management.Smo.User smoMetadataObject, LsbDatabase parent)
			: base(smoMetadataObject, parent, Microsoft.SqlServer.Management.SqlParser.Metadata.UserType.SqlLogin)
		{
		}


		private ILogin m_login;

		private bool m_loginIsSet;

		public override IAsymmetricKey AsymmetricKey => null;

		public override ICertificate Certificate => null;

		public override ILogin Login
		{
			get
			{
				if (!m_loginIsSet)
				{
					string login = m_smoMetadataObject.Login;
					m_login = base.Database.Server.Logins[login];
					m_loginIsSet = true;
				}
				return m_login;
			}
		}

		public override string Password => null;

	}




	private sealed class PasswordUserI : LsbUser
	{
		public PasswordUserI(Microsoft.SqlServer.Management.Smo.User smoMetadataObject, LsbDatabase parent)
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
