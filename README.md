# AiAgents

An AI-powered agent system for automating software development tasks, starting with intelligent unit test execution.

## Overview

AiAgents is a .NET-based framework that provides intelligent automation agents for common development workflows. The current implementation focuses on automated unit test discovery, execution, and analysis.

## Features

### Unit Test Agent

The Unit Test Agent provides automated testing capabilities:

- **Automatic Project Discovery**: Scans directories to find test projects
- **Intelligent Test Execution**: Runs tests using `dotnet test` with configurable parameters
- **Result Analysis**: Parses test output and provides actionable insights
- **Flexible Configuration**: Supports various test frameworks (xUnit, NUnit, MSTest)
- **Error Handling**: Distinguishes between build failures and test failures

## Getting Started

### Prerequisites

- .NET 9.0 SDK or later
- A .NET project with unit tests

### Installation

1. Clone the repository:
```bash
git clone https://github.com/RomikIT/AiAgents.git
cd AiAgents
```

2. Build the project:
```bash
dotnet build
```

3. Run the application:
```bash
dotnet run --project src/AiAgents.Core
```

## Usage

### Running Unit Tests

Basic usage:
```bash
dotnet run --project src/AiAgents.Core -- test /path/to/your/project
```

With configuration options:
```bash
dotnet run --project src/AiAgents.Core -- test /path/to/your/project -c Release -v detailed
```

With test filtering:
```bash
dotnet run --project src/AiAgents.Core -- test /path/to/your/project -f "Category=Unit"
```

### Command-Line Options

- `-c, --configuration <config>`: Build configuration (Debug/Release)
- `-v, --verbosity <level>`: Test output verbosity (quiet/minimal/normal/detailed/diagnostic)
- `-f, --filter <expression>`: Filter tests to run
- `--no-build`: Skip building the project
- `--no-restore`: Skip restoring dependencies

## Project Structure

```
AiAgents/
├── src/
│   └── AiAgents.Core/
│       ├── Agents/
│       │   └── UnitTestAgent.cs       # Main test agent implementation
│       ├── Program.cs                  # CLI entry point
│       └── AiAgents.Core.csproj       # Project file
├── skills.json                         # Agent skills configuration
├── AiAgents.slnx                      # Solution file
└── README.md
```

## Skills Configuration

The `skills.json` file defines the capabilities of each agent:

- **unit-test-runner**: Discovers and executes unit tests
- **test-analyzer**: Analyzes test results and provides recommendations
- **project-discovery**: Identifies test projects in directory structures
- **continuous-testing**: (Planned) Monitors and auto-runs tests on changes

## Architecture

### UnitTestAgent

The core agent responsible for test execution:

```csharp
var agent = new UnitTestAgent(projectPath, config);
var result = await agent.RunTestsAsync();
var analysis = await agent.AnalyzeTestResultsAsync(result);
```

### AgentConfiguration

Configure test execution parameters:

```csharp
var config = new AgentConfiguration
{
    Configuration = "Release",
    Verbosity = "detailed",
    Filter = "Category=Integration"
};
```

## Example Output

```
=== AiAgents - Unit Test Runner ===

Initializing UnitTestAgent for: /path/to/project
[UnitTestAgent] Starting test execution for: /path/to/project
[UnitTestAgent] Found test project: /path/to/project/Tests/MyProject.Tests.csproj
[UnitTestAgent] Test execution completed. Success: True

=== Test Execution Results ===
Duration: 3.45 seconds
Success: True
Exit Code: 0

=== Analysis ===
Total Tests: 42
Passed: 42
Failed: 0
Skipped: 0

=== Recommendations ===
- All tests passed successfully!
```

## Supported Frameworks

- .NET 9.0, 8.0, 7.0, 6.0
- Test frameworks: xUnit, NUnit, MSTest

## Contributing

Contributions are welcome! Please feel free to submit pull requests or open issues.

## Future Enhancements

- Continuous testing with file system watching
- Integration with CI/CD pipelines
- Test coverage analysis
- Performance benchmarking
- Multi-project test orchestration
- Custom reporting formats

## License

MIT License - feel free to use this project for any purpose.

## Author

Created for automated testing workflows in .NET projects.
