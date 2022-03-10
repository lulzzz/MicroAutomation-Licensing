# Welcome to license manager 👋

> Software license management solution for to easily protect your applications.

## Project Status

[![Build](https://github.com/NetbootCompany/MicroAutomation-Licensing/actions/workflows/build.yml/badge.svg)](https://github.com/NetbootCompany/MicroAutomation-Licensing/actions/workflows/build.yml)
[![CodeQL](https://github.com/NetbootCompany/MicroAutomation-Licensing/actions/workflows/codeql.yml/badge.svg)](https://github.com/NetbootCompany/MicroAutomation-Licensing/actions/workflows/codeql.yml)
[![Test](https://github.com/NetbootCompany/MicroAutomation-Licensing/actions/workflows/test.yml/badge.svg)](https://github.com/NetbootCompany/MicroAutomation-Licensing/actions/workflows/test.yml)
[![Maintainability](https://api.codeclimate.com/v1/badges/e07fec22c07068410b0a/maintainability)](https://codeclimate.com/github/NetbootCompany/MicroAutomation-Licensing/maintainability)

The solution is written in the **Asp.Net Core MVC - using .NET 6**

## Requirements

- [Install](https://www.microsoft.com/net/download/windows#/current) the latest .NET 6 SDK (using older versions may lead to 502.5 errors when hosted on IIS or application exiting immediately after starting when self-hosted)

## EF Core & Data Access

- The solution uses these `DbContexts`:
  - `DataStoreDbContext`: for Licensing configuration store.
  - `DataProtectionDbContext`: for Microsoft Data Protection.

### Run entity framework migrations:

> NOTE: Initial migrations are a part of the repository.

- It is possible to use powershell script in folder `build/add-migrations.ps1`.
- This script take two arguments:
  - --migration (migration name)
  - --migrationProviderName (provider type - available choices: All, SqlServer, PostgreSQL)
- For example: `.\add-migrations.ps1 -migration DbInit -migrationProviderName SqlServer`

### Available database providers:

- SqlServer
- PostgreSQL

> It is possible to switch the database provider via `appsettings.json`:

```json
"DatabaseProviderConfiguration": {
    "ProviderType": "SqlServer" 
}
```

## Validate the license in your application

The easiest way to assert the license is in the entry point of your application.

First load the license from a file or resource:

```csharp
var license = License.Load(...);
```

Then you can assert the license:

```csharp
var validation = license.Validate()  
    .ExpirationDate()  
    .When(lic => lic.Type == LicenseType.Trial)  
    .And()  
    .Signature(publicKey)  
    .AssertValidLicense();
```

Licensing class will not throw any Exception and just return an enumeration of validation failures.

Now you can iterate over possible validation failures:

```csharp
foreach (var failure in validationFailures)
     Console.WriteLine(failure.GetType().Name + ": " + failure.Message + " - " + failure.HowToResolve);
```

Or simply check if there is any failure:

```csharp
if (validationFailures.Any())
   // ...
```

Make sure to call `validationFailures.ToList()` or `validationFailures.ToArray()` before using the result multiple times.

## How to Contribute

Everyone is welcome to contribute to this project! Feel free to contribute with pull requests, bug reports or enhancement suggestions.

## Bugs and Feedback

For bugs, questions and discussions please use the [GitHub Issues](https://github.com/NetbootCompany/MicroAutomation-Licensing/issues).

## Credits

- [junian/Standard.Licensing](https://github.com/junian/Standard.Licensing) for the original work of the library.

## License

This project is licensed under [MIT License](https://github.com/NetbootCompany/MicroAutomation-Licensing/blob/main/LICENSE).
