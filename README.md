# eKIBRA

A software implementation of Spaced Repetition Method (SRM) for learners.

**Version:** 0.0.18  
**Author:** Guilherme Prado  
**License:** MIT  
**Live Demo:** https://ekibra-web.azurewebsites.net/

## Overview

eKIBRA allows learners to include study content for further use in solo practice sessions ruled by the 
Spaced Repetition Method. The application provides analysis at the end of each study session to assist 
learners in identifying weaknesses in their knowledge retention.

## Technology Stack

- **.NET 9 SDK** - [Download](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- **Razor Pages** - Server-side web framework
- **Entity Framework** - Object-relational mapping
- **Identity Server** - Authentication and authorization
- **SQL Server 2022** - Database
- **Bootstrap v5.3.3** - [Frontend framework](https://getbootstrap.com/)
- **jQuery v3.7.1** - [JavaScript library](https://jquery.com/)

### Testing & Quality Tools

- **xUnit** - Unit testing framework
- **Moq** - Mocking framework
- **FluentAssertions** - Assertion library
- **Coverlet** - [Code coverage tool](https://github.com/coverlet-coverage/coverlet)
- **ReportGenerator** - [Coverage report generator](https://reportgenerator.io/)

## Prerequisites

Before running the project locally, ensure you have the following installed:

1. **.NET 9 SDK** - [Download here](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
2. **Compatible IDE** (Visual Studio, VS Code, or JetBrains Rider)
3. **SQL Server 2022** or later
4. **ReportGenerator Global Tool** (for test coverage reports)

### Installing ReportGenerator

```bash
dotnet tool install --global dotnet-reportgenerator-globaltool --version 5.4.7
```

## Getting Started

### 1. Clone the Repository

```bash
git clone [your-repository-url]
cd eKIBRA
```

### 2. Database Setup

Ensure SQL Server is running and configure your connection string in `appsettings.json` or `appsettings.Development.json`.

### 3. Restore Dependencies and Update Database

```bash
# Restore NuGet packages
dotnet restore

# Install Entity Framework tools (if not already installed)
dotnet tool install --global dotnet-ef
dotnet tool restore

# Apply database migrations
dotnet ef database update --project eKIBRA.Web
```

### 4. Build and Run

Navigate to the root folder and run:

```bash
dotnet run
```

The application will start and be available at the configured port (typically `http://localhost:5106`).

### 5. Running Tests

To run the test suite:

```bash
dotnet test
```

### 6. Generate Test Coverage Report

Use the provided PowerShell script to run test and regenerate test coverage reports:

```bash
.\RegenerateTestCoverage.ps1
```
After running the script outputs the report results at `./TestResults/ReportGenerator/index.html`

## Project Structure

The project follows standard .NET web application conventions with Razor Pages architecture, incorporating 
Entity Framework for data access and Identity Server for authentication.

## Docker Support

The project includes a `Dockerfile` for containerized deployment, specifically configured for Azure deployment.

## Contributing

This project is intended for educational purposes focusing on the Spaced Repetition Method implementation. 
Contributions that enhance the learning experience or improve the SRM algorithm are welcome.

## License

This project is licensed under the MIT License—see the LICENSE file for details.

---

For questions or support, please contact Guilherme Prado.
