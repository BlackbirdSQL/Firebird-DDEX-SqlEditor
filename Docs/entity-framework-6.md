# Entity Framework 6

### Setup procedure

* 1. Install Microsoft's EntityFramework 6 Nuget package.
* 2. Add a reference to 'EntityFramework.Firebird.dll' located in the output
*		folder BlackbirdSql.EntityFramework\bin\Debug.
*		(We're still under development so this is a 2 step process. Once we have the
*		release version out the 'EntityFramework.Firebird' Nuget package will perform
*		steps 1 and 2)
* 3. Register EntityFramework with the application (described below).
*		(There must be a way of automating step 3 so that's on the bucket list)
*

### EntityFramework application registration 

.NET Framework:
```xml
<configSections>
	<!-- This will be automatically inserted - obsolete -->
</configSections>
<entityFramework>
	<!-- This will be automatically inserted - obsolete -->
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

