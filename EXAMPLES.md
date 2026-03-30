# AiAgents Examples

This directory contains example projects demonstrating how to use the AiAgents system.

## Running the Examples

### Example 1: Basic Test Execution

To run tests on an external project:

```bash
# Navigate to the AiAgents root directory
cd /path/to/AiAgents

# Run the agent against a test project
dotnet run --project src/AiAgents.Core -- test /path/to/your/test/project
```

### Example 2: Using Configuration Options

```bash
# Run with Release configuration and detailed output
dotnet run --project src/AiAgents.Core -- test /path/to/project -c Release -v detailed

# Run specific test categories
dotnet run --project src/AiAgents.Core -- test /path/to/project -f "Category=Unit"

# Skip build and restore for faster execution
dotnet run --project src/AiAgents.Core -- test /path/to/project --no-build --no-restore
```

### Example 3: Programmatic Usage

You can also use the UnitTestAgent directly in your C# code:

```csharp
using AiAgents.Core.Agents;

// Create agent configuration
var config = new AgentConfiguration
{
    Configuration = "Release",
    Verbosity = "normal",
    Filter = "FullyQualifiedName~MyNamespace"
};

// Initialize the agent
var agent = new UnitTestAgent("/path/to/project", config);

// Run tests
var result = await agent.RunTestsAsync();

// Analyze results
var analysis = await agent.AnalyzeTestResultsAsync(result);

Console.WriteLine($"Tests run: {analysis.TotalTests}");
Console.WriteLine($"Passed: {analysis.PassedTests}");
Console.WriteLine($"Failed: {analysis.FailedTests}");
```

## Skills Configuration Examples

The `skills.json` file can be customized to modify agent behavior:

```json
{
  "configuration": {
    "defaultTimeout": 600,
    "maxConcurrentTests": 4,
    "autoDiscovery": true,
    "reportFormat": "json"
  }
}
```

## Common Use Cases

### 1. CI/CD Integration

```bash
# In your CI/CD pipeline
dotnet run --project src/AiAgents.Core -- test ./tests -c Release --no-restore
```

### 2. Pre-commit Hook

```bash
# Run quick tests before committing
dotnet run --project src/AiAgents.Core -- test ./tests -f "Category=Fast"
```

### 3. Directory Scanning

```bash
# Point to a directory, and the agent will find test projects
dotnet run --project src/AiAgents.Core -- test ./
```

## Expected Output

When you run the agent, you'll see output like:

```
=== AiAgents - Unit Test Runner ===

Initializing UnitTestAgent for: /path/to/project
[UnitTestAgent] Starting test execution for: /path/to/project
[UnitTestAgent] Found test project: /path/to/project/MyTests.csproj
[UnitTestAgent] Test execution completed. Success: True

=== Test Execution Results ===
Duration: 2.34 seconds
Success: True
Exit Code: 0

=== Analysis ===
Total Tests: 15
Passed: 15
Failed: 0
Skipped: 0

=== Recommendations ===
- All tests passed successfully!
```

## Troubleshooting

### Project Not Found

If the agent can't find your test project:
- Ensure the path is correct
- Check that .csproj files exist in the directory
- Try using the full path instead of a relative path

### Build Failures

If you see build errors:
- Run `dotnet build` on your test project first
- Check that all dependencies are restored
- Verify the .NET SDK version matches your project's target framework

### No Tests Discovered

If no tests are found:
- Verify your test project uses a supported framework (xUnit, NUnit, MSTest)
- Check that test classes and methods are properly decorated
- Ensure the project builds successfully
