# AutoServiceApp

AutoServiceApp is a cross-platform training desktop application for managing an auto service shop. It is built with C# .NET 8 and Avalonia UI, and stores data in JSON files instead of a database.

## Requirements

- .NET SDK 8.0 or newer
- macOS, Linux, or Windows

## Project Structure

- `AutoServiceApp/` - Avalonia desktop application
- `AutoServiceApp.Tests/` - xUnit test project with code coverage support

## Run the Application

From the repository root:

```bash
cd AutoServiceApp
dotnet restore
dotnet build
dotnet run
```

The application saves JSON data files in the user's application data folder under `AutoServiceApp`.

## Run Tests

From the repository root:

```bash
dotnet restore AutoServiceApp.Tests/AutoServiceApp.Tests.csproj
dotnet test AutoServiceApp.Tests/AutoServiceApp.Tests.csproj
```

## Run Tests with Coverage

From the repository root:

```bash
dotnet test AutoServiceApp.Tests/AutoServiceApp.Tests.csproj --collect:"XPlat Code Coverage"
```

Coverage reports are written to:

```text
AutoServiceApp.Tests/TestResults/
```

## Notes

This project is intentionally written as a realistic training codebase with accumulated technical debt and object-oriented design issues. The goal is to support refactoring and code-smell analysis exercises while still keeping the application buildable and runnable.
