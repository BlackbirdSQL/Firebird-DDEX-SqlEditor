# Entity Framework 6

### Setup procedure

* 1. Install Microsoft's EntityFramework 6 Nuget package.
* 2. Add a reference to 'EntityFramework.Blackbird.dll' located in the output
*		folder BlackbirdSql.EntityFramework\bin\Debug.
*		(We're still under development so this is a 2 step process. Once we have the
*		release version out the 'EntityFramework.Blackbird' Nuget package will perform
*		steps 1 and 2)
* 3. Register EntityFramework with the application (described below).
*		(There must be a way of automating step 3 so that's on the bucket list)
*

### EntityFramework application registration 

.NET Framework:
```xml
<configSections>
	<section name="entityFramework" type="System.Data.Entity.Internal.ConfigFile.EntityFrameworkSection, EntityFramework, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" requirePermission="false" />
</configSections>
<entityFramework>
	<defaultConnectionFactory type="BlackbirdSql.Data.Entity.ConnectionFactory, EntityFramework.Blackbird" />
	<providers>
		<provider invariantName="BlackbirdSql.Data.DslClient" type="BlackbirdSql.Data.Entity.ProviderServices, EntityFramework.Blackbird" />
	</providers>
</entityFramework>
```

.NET Core/.NET 5+:
```csharp
public class Conf : DbConfiguration
{
	public Conf()
	{
		SetProviderServices(ProviderServices.Invariant, ProviderServices.Instance);
	}
}
```

