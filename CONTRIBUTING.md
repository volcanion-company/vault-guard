# Contributing to VaultGuard

First off, thank you for considering contributing to VaultGuard! It's people like you that make VaultGuard such a great tool.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Workflow](#development-workflow)
- [Coding Standards](#coding-standards)
- [Testing Requirements](#testing-requirements)
- [Pull Request Process](#pull-request-process)
- [Project Structure](#project-structure)
- [Reporting Bugs](#reporting-bugs)
- [Suggesting Enhancements](#suggesting-enhancements)

---

## Code of Conduct

### Our Pledge

We are committed to providing a welcoming and inspiring community for all. We expect all participants to:

- Use welcoming and inclusive language
- Be respectful of differing viewpoints and experiences
- Gracefully accept constructive criticism
- Focus on what is best for the community
- Show empathy towards other community members

### Unacceptable Behavior

- Harassment, trolling, or discriminatory comments
- Publishing others' private information
- Other conduct which could reasonably be considered inappropriate

---

## Getting Started

### Prerequisites

Before you begin, ensure you have:

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) installed
- [PostgreSQL 16+](https://www.postgresql.org/download/) running locally
- [Redis 7+](https://redis.io/download) running locally
- Git installed and configured
- Your preferred IDE (Visual Studio 2022, Rider, or VS Code)

### Fork and Clone

1. Fork the repository on GitHub
2. Clone your fork locally:
   ```bash
   git clone https://github.com/YOUR_USERNAME/vault-guard.git
   cd vault-guard
   ```
3. Add the upstream repository:
   ```bash
   git remote add upstream https://github.com/volcanion-company/vault-guard.git
   ```

### Setup Development Environment

1. **Install dependencies:**
   ```bash
   dotnet restore
   ```

2. **Setup databases:**
   ```sql
   CREATE DATABASE vaultguard_write;
   CREATE DATABASE vaultguard_read;
   ```

3. **Configure settings:**
   Copy `appsettings.Development.json` and update connection strings.

4. **Run migrations:**
   ```bash
   cd src/presentations/VaultGuard.Api
   dotnet ef database update --project ../../libs/VaultGuard.Persistence
   ```

5. **Run the application:**
   ```bash
   dotnet run
   ```

6. **Run tests:**
   ```bash
   dotnet test
   ```

---

## Development Workflow

### Branching Strategy

We use a simplified Git Flow:

- `main` - Production-ready code
- `develop` - Integration branch for features
- `feature/*` - New features
- `bugfix/*` - Bug fixes
- `hotfix/*` - Urgent production fixes

### Working on a Feature

1. **Update your local repository:**
   ```bash
   git checkout develop
   git pull upstream develop
   ```

2. **Create a feature branch:**
   ```bash
   git checkout -b feature/your-feature-name
   ```

3. **Make your changes:**
   - Write code following our [coding standards](#coding-standards)
   - Add tests for new functionality
   - Update documentation as needed

4. **Commit your changes:**
   ```bash
   git add .
   git commit -m "feat: add your feature description"
   ```

   Follow [Conventional Commits](https://www.conventionalcommits.org/):
   - `feat:` - New feature
   - `fix:` - Bug fix
   - `docs:` - Documentation changes
   - `test:` - Adding or updating tests
   - `refactor:` - Code refactoring
   - `perf:` - Performance improvements
   - `chore:` - Maintenance tasks

5. **Push to your fork:**
   ```bash
   git push origin feature/your-feature-name
   ```

6. **Open a Pull Request** on GitHub

---

## Coding Standards

### C# Conventions

Follow the [.NET coding conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions):

```csharp
// ✅ Good
public sealed class VaultService
{
    private readonly IVaultRepository _repository;
    private readonly ILogger<VaultService> _logger;

    public VaultService(IVaultRepository repository, ILogger<VaultService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Vault> CreateVaultAsync(string name, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating vault with name {VaultName}", name);
        
        var vault = new Vault(name, _currentUserId);
        await _repository.AddAsync(vault, cancellationToken);
        
        return vault;
    }
}
```

### Key Principles

1. **Clean Architecture** - Respect layer boundaries
   - Domain layer has NO external dependencies
   - Application layer depends only on Domain
   - Infrastructure depends on Application
   - API depends on Application and Infrastructure

2. **SOLID Principles**
   - Single Responsibility
   - Open/Closed
   - Liskov Substitution
   - Interface Segregation
   - Dependency Inversion

3. **DDD Patterns**
   - Aggregate roots enforce invariants
   - Value objects are immutable
   - Repositories work with aggregates
   - Domain events for side effects

4. **Async/Await**
   - Use async methods for I/O operations
   - Always pass `CancellationToken`
   - Avoid `async void` except for event handlers

5. **Naming Conventions**
   - `PascalCase` for classes, methods, properties
   - `camelCase` for local variables, parameters
   - `_camelCase` for private fields
   - Meaningful names over comments

### Code Organization

```csharp
// Order of class members:
public class Example
{
    // 1. Constants
    private const int MaxRetries = 3;

    // 2. Fields
    private readonly IDependency _dependency;

    // 3. Constructors
    public Example(IDependency dependency)
    {
        _dependency = dependency;
    }

    // 4. Properties
    public string Name { get; set; }

    // 5. Public methods
    public void DoSomething() { }

    // 6. Private methods
    private void HelperMethod() { }
}
```

---

## Testing Requirements

### Test Coverage

- Minimum **80%** code coverage required
- All new features must include tests
- Bug fixes should include regression tests

### Test Structure

Follow the **AAA pattern** (Arrange, Act, Assert):

```csharp
[Fact]
public async Task CreateVault_WithValidData_ShouldSucceed()
{
    // Arrange
    var repository = new Mock<IVaultRepository>();
    var handler = new CreateVaultCommandHandler(repository.Object);
    var command = new CreateVaultCommand("My Vault");

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Name.Should().Be("My Vault");
    repository.Verify(x => x.AddAsync(It.IsAny<Vault>(), It.IsAny<CancellationToken>()), Times.Once);
}
```

### Testing Guidelines

1. **Unit Tests**
   - Test one thing per test
   - Mock external dependencies
   - Fast execution (< 1 second per test)
   - No database or network calls

2. **Integration Tests**
   - Test component interactions
   - Use test database
   - Clean up after tests
   - Test realistic scenarios

3. **Test Naming**
   ```
   MethodName_Scenario_ExpectedBehavior
   ```
   Examples:
   - `CreateVault_WithValidData_ShouldSucceed`
   - `GetVault_WhenNotFound_ShouldReturnNull`
   - `DeleteVault_WhenNotOwner_ShouldThrowException`

4. **Use FluentAssertions**
   ```csharp
   // ✅ Good
   result.Should().NotBeNull();
   result.Name.Should().Be("Expected");
   
   // ❌ Avoid
   Assert.NotNull(result);
   Assert.Equal("Expected", result.Name);
   ```

### Running Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura

# Run specific test project
dotnet test test/libs/VaultGuard.Application.Tests

# Run tests matching a filter
dotnet test --filter "FullyQualifiedName~VaultTests"
```

---

## Pull Request Process

### Before Submitting

Ensure your PR:

- [ ] Follows the coding standards
- [ ] Includes appropriate tests
- [ ] All tests pass locally
- [ ] Code coverage meets requirements (80%+)
- [ ] Documentation is updated
- [ ] Commit messages follow Conventional Commits
- [ ] No merge conflicts with `develop` branch

### PR Template

When opening a PR, include:

```markdown
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix (non-breaking change fixing an issue)
- [ ] New feature (non-breaking change adding functionality)
- [ ] Breaking change (fix or feature causing existing functionality to change)
- [ ] Documentation update

## Testing
- [ ] Unit tests added/updated
- [ ] Integration tests added/updated
- [ ] All tests pass locally

## Checklist
- [ ] Code follows project coding standards
- [ ] Self-review completed
- [ ] Comments added for complex code
- [ ] Documentation updated
- [ ] No new warnings generated
```

### Review Process

1. **Automated Checks** - CI pipeline runs tests and linting
2. **Code Review** - At least one maintainer reviews the PR
3. **Feedback** - Address review comments
4. **Approval** - Maintainer approves the PR
5. **Merge** - Maintainer merges to `develop`

### After Merge

- Delete your feature branch
- Update your local repository:
  ```bash
  git checkout develop
  git pull upstream develop
  ```

---

## Project Structure

### Layer Responsibilities

```
src/
├── libs/
│   ├── VaultGuard.Domain/
│   │   ├── Entities/          # Aggregate roots, entities
│   │   ├── ValueObjects/      # Immutable value objects
│   │   ├── Events/            # Domain events
│   │   └── Repositories/      # Repository interfaces
│   │
│   ├── VaultGuard.Application/
│   │   ├── Features/
│   │   │   ├── Vaults/
│   │   │   │   ├── Commands/  # Write operations
│   │   │   │   └── Queries/   # Read operations
│   │   │   └── VaultItems/
│   │   ├── DTOs/              # Data Transfer Objects
│   │   └── Validators/        # FluentValidation validators
│   │
│   ├── VaultGuard.Persistence/
│   │   ├── Contexts/          # DbContext classes
│   │   ├── Configurations/    # EF Core configurations
│   │   └── Repositories/      # Repository implementations
│   │
│   └── VaultGuard.Infrastructure/
│       ├── Caching/           # Redis implementation
│       ├── Logging/           # Serilog configuration
│       └── Monitoring/        # OpenTelemetry setup
│
└── presentations/
    └── VaultGuard.Api/
        ├── Controllers/       # API endpoints
        ├── Middleware/        # Custom middleware
        └── Extensions/        # Service registrations
```

### Adding New Features

#### 1. Domain Layer

Add entities or value objects:

```csharp
// Domain/Entities/NewEntity.cs
public class NewEntity : BaseEntity
{
    public string Name { get; private set; }
    
    public NewEntity(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }
}
```

#### 2. Application Layer

Create Command/Query:

```csharp
// Application/Features/NewFeature/Commands/CreateCommand.cs
public record CreateCommand(string Name) : IRequest<ResultDto>;

// Application/Features/NewFeature/Commands/CreateCommandHandler.cs
public class CreateCommandHandler : IRequestHandler<CreateCommand, ResultDto>
{
    // Implementation
}

// Application/Features/NewFeature/Commands/CreateCommandValidator.cs
public class CreateCommandValidator : AbstractValidator<CreateCommand>
{
    public CreateCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
```

#### 3. API Layer

Add controller endpoint:

```csharp
// Api/Controllers/NewFeatureController.cs
[ApiController]
[Route("api/[controller]")]
public class NewFeatureController : ControllerBase
{
    private readonly IMediator _mediator;

    public NewFeatureController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
```

#### 4. Tests

Add comprehensive tests:

```csharp
// Tests/Application.Tests/Features/NewFeatureTests.cs
public class CreateCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_ShouldSucceed()
    {
        // Test implementation
    }
}
```

---

## Reporting Bugs

### Before Submitting a Bug Report

- Check existing issues to avoid duplicates
- Verify the bug with the latest version
- Collect relevant information (logs, screenshots, steps to reproduce)

### Bug Report Template

```markdown
## Description
Clear description of the bug

## Steps to Reproduce
1. Step 1
2. Step 2
3. Step 3

## Expected Behavior
What should happen

## Actual Behavior
What actually happens

## Environment
- OS: [e.g., Windows 11]
- .NET Version: [e.g., 10.0]
- Database: [e.g., PostgreSQL 16]

## Additional Context
Logs, screenshots, or other relevant information
```

---

## Suggesting Enhancements

### Enhancement Template

```markdown
## Summary
Brief description of the enhancement

## Motivation
Why is this enhancement needed?

## Proposed Solution
How should this be implemented?

## Alternatives Considered
Other approaches you've considered

## Additional Context
Any other relevant information
```

---

## Architecture Decision Records (ADRs)

When making significant architectural decisions, document them:

```markdown
# ADR-001: Use CQRS Pattern

## Status
Accepted

## Context
Need to optimize read and write operations separately

## Decision
Implement CQRS with separate read/write databases

## Consequences
- Increased complexity
- Better performance
- Improved scalability
```

---

## Questions?

If you have questions about contributing:

- Open a [GitHub Discussion](https://github.com/volcanion-company/vault-guard/discussions)
- Review the [ARCHITECTURE.md](ARCHITECTURE.md) documentation
- Ask in pull request comments

---

## Recognition

Contributors will be recognized in:
- GitHub contributors page
- Release notes for significant contributions
- Project documentation

---

Thank you for contributing to VaultGuard! 🎉

Your efforts help make this project better for everyone.
