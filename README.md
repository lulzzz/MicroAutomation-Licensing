# Welcome to license manager 👋

> Software license management solution for to easily protect your applications.

## Project Status

[![Build](https://github.com/NetbootHome/MicroAutomation-Licensing/actions/workflows/build.yml/badge.svg)](https://github.com/NetbootHome/MicroAutomation-Licensing/actions/workflows/build.yml)
[![CodeQL](https://github.com/NetbootHome/MicroAutomation-Licensing/actions/workflows/codeql.yml/badge.svg)](https://github.com/NetbootHome/MicroAutomation-Licensing/actions/workflows/codeql.yml)
[![Test](https://github.com/NetbootHome/MicroAutomation-Licensing/actions/workflows/test.yml/badge.svg)](https://github.com/NetbootHome/MicroAutomation-Licensing/actions/workflows/test.yml)
[![Maintainability](https://api.codeclimate.com/v1/badges/c57c633412aafada9557/maintainability)](https://codeclimate.com/github/NetbootHome/MicroAutomation-Licensing/maintainability)

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

## How to Contribute

Everyone is welcome to contribute to this project! Feel free to contribute with pull requests, bug reports or enhancement suggestions.

## Bugs and Feedback

For bugs, questions and discussions please use the [GitHub Issues](https://github.com/NetbootHome/MicroAutomation-Licensing/issues).

## Credits

- [junian/Standard.Licensing](https://github.com/junian/Standard.Licensing) for the original work of the library.

## License

This project is licensed under [MIT License](https://github.com/NetbootHome/MicroAutomation-Licensing/blob/main/LICENSE).
