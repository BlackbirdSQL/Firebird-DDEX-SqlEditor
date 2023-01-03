# Blackbird .NET Data Provider

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

* Sean Leyne (Broadview Software)
* SMS-Timing

### 3rd party code

* For zlib compression the provider uses pieces from [DotNetZip](http://dotnetzip.codeplex.com/) library.
* For RC4 encryption the provider uses pieces from [Bouncy Castle](https://www.bouncycastle.org/csharp/index.html) library.
