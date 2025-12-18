using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace VaultGuard.Application.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void AddApplication_ShouldRegisterMediatR_And_Validators()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddApplication();

        // Assert – MediatR
        services.Should().Contain(
            sd => sd.ServiceType == typeof(IMediator));

        services.Should().Contain(
            sd => sd.ServiceType.IsGenericType &&
                  sd.ServiceType.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));

        // Assert – FluentValidation
        services.Should().Contain(
            sd => sd.ServiceType.IsGenericType &&
                  sd.ServiceType.GetGenericTypeDefinition() == typeof(IValidator<>));
    }

    [Fact]
    public void AddApplication_ShouldNotThrow()
    {
        var services = new ServiceCollection();

        Action act = () => services.AddApplication();

        act.Should().NotThrow();
    }
}

public record TestQuery : IRequest<string>;

public class TestQueryHandler : IRequestHandler<TestQuery, string>
{
    public Task<string> Handle(TestQuery request, CancellationToken cancellationToken) => Task.FromResult("OK");
}

public class TestQueryValidator : AbstractValidator<TestQuery>
{
    public TestQueryValidator()
    {
        RuleFor(x => x).NotNull();
    }
}
