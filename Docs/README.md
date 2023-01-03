# BlackbirdSQL DDEX 2.0 .NET Data Provider

This is a straight pull from FirebirdSQL.

The bulk of the FirebirdSQL code is behind lock and key so it became impossible to just hook into
the existing library which would have been the preferred route.
Also a lot of the functionality required by DDEX was moved into EF and had to be linked back to the
client library, so in the end it was a rename to BlackbirdSQL, but besides class name and namespace changes
very little is different from the original code.

Although the original DDEX 1.0 DataTools package was revived, the DDEX 2.0 package, BlackbirdSql.VisualStudio.Ddex
is click and go using VSIX and autoload, and requires no additional setup either in the app.config, csproj 
or machine.config.
Loading is asynchronous so the provider needs time to register and load. A database node in server explorer can simply be refreshed if an attempt was made to access it before it was fully loaded.

As it stands right now the code is littered with diagnostics calls with writes to a log file set to c:\bin\vsdiag.log.
Also, defaults in the connection dialog are set to my own test database.
There seems to be an issue with drag and drop on procedures and functions which I haven't looked at. It's likely something trivial.
The localized new query command is working but has not yet been placed in the views or function/procedures nodes of the SE.

If you're planning on using EF Core and/or .NET, VS does not have wizard support for either of these in the edmx or xsd which makes no sense to me.
This roadblock is easily overcome by creating a separate project using .NET Framework for your data models and then linking your .NET / EF Core projects to those datasets.
I have yet to transfer the converted EF Core project to the repository so you can do this simply because I haven't done any testing on it. That should happen in the next few days.

## Documentation

* [ADO.NET provider](docs/ado-net.md)
* [Entity Framework 6 provider](docs/entity-framework-6.md)
* [Entity Framework Core provider](docs/entity-framework-core.md)
* [Services - Backup](docs/services-backup.md)
* [Events](docs/events.md)
* [ADO.NET - Schema](docs/ado-net-schema.md)
* [Time zones](docs/time-zones.md)
* [DECFLOAT datatype](docs/decfloat.md)
* [INT128 datatype](docs/int128.md)
* [Batching](docs/batching.md)

## Packages

| NuGet | Version | Downloads |
|-------|---------|-----------|
| [BlackbirdSql.Data.DslClient](https://www.nuget.org/packages/BlackbirdSql.Data.DslClient) | ![BlackbirdSql.Data.DslClient](https://img.shields.io/nuget/v/BlackbirdSql.Data.DslClient.svg) | ![BlackbirdSql.Data.DslClient](https://img.shields.io/nuget/dt/BlackbirdSql.Data.DslClient.svg) |
| [EntityFramework.Blackbird](https://www.nuget.org/packages/EntityFramework.Blackbird) | ![EntityFramework.Blackbird](https://img.shields.io/nuget/v/EntityFramework.Blackbird.svg) | ![EntityFramework.Blackbird](https://img.shields.io/nuget/dt/EntityFramework.Blackbird.svg) |
| [BlackbirdSql.EntityFrameworkCore.Blackbird](https://www.nuget.org/packages/BlackbirdSql.EntityFrameworkCore.Blackbird) | ![BlackbirdSql.EntityFrameworkCore.Blackbird](https://img.shields.io/nuget/v/BlackbirdSql.EntityFrameworkCore.Blackbird.svg) | ![BlackbirdSql.EntityFrameworkCore.Blackbird](https://img.shields.io/nuget/dt/BlackbirdSql.EntityFrameworkCore.Blackbird.svg) |

## Resources

* [Downloads](https://github.com/BlackbirdSQL/NETProvider/releases)
* [Issue tracker](https://github.com/BlackbirdSQL/NETProvider/issues)
* [Development mailing list](https://groups.google.com/forum/#!forum/blackbird-net-provider)

## Builds

[![TeamCity](https://teamcity.jetbrains.com/app/rest/builds/buildType:(id:OpenSourceProjects_BlackbirdClient_CiBuild)/statusIcon.svg)](https://teamcity.jetbrains.com/project.html?projectId=OpenSourceProjects_BlackbirdClient)
[![GitHub Actions](https://github.com/BlackbirdSQL/NETProvider/workflows/CI/badge.svg)](https://github.com/BlackbirdSQL/NETProvider/actions)

## Misc

### Notable supporters


### 3rd party code

* For zlib compression the provider uses pieces from [DotNetZip](http://dotnetzip.codeplex.com/) library.
* For RC4 encryption the provider uses pieces from [Bouncy Castle](https://www.bouncycastle.org/csharp/index.html) library.
