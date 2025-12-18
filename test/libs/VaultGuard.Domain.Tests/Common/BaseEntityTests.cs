using VaultGuard.Domain.Common;

namespace VaultGuard.Domain.Tests.Common;

public class BaseEntityTests
{
    // Test entity class for testing BaseEntity
    private class TestEntity : BaseEntity
    {
        public TestEntity() : base() { }
        public TestEntity(Guid id) : base(id) { }

        public void Update()
        {
            MarkAsUpdated();
        }
    }

    // Another test entity class for testing different types
    private class AnotherTestEntity : BaseEntity
    {
        public AnotherTestEntity() : base() { }
        public AnotherTestEntity(Guid id) : base(id) { }

        public void Update()
        {
            MarkAsUpdated();
        }
    }

    [Fact]
    public void BaseEntity_DefaultConstructor_ShouldGenerateNewId()
    {
        // Arrange & Act
        var entity = new TestEntity();

        // Assert
        Assert.NotEqual(Guid.Empty, entity.Id);
    }

    [Fact]
    public void BaseEntity_DefaultConstructor_ShouldSetCreatedAt()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;

        // Act
        var entity = new TestEntity();
        var afterCreation = DateTime.UtcNow;

        // Assert
        Assert.True(entity.CreatedAt >= beforeCreation);
        Assert.True(entity.CreatedAt <= afterCreation);
        Assert.Equal(DateTimeKind.Utc, entity.CreatedAt.Kind);
    }

    [Fact]
    public void BaseEntity_DefaultConstructor_ShouldNotSetUpdatedAt()
    {
        // Arrange & Act
        var entity = new TestEntity();

        // Assert
        Assert.Null(entity.UpdatedAt);
    }

    [Fact]
    public void BaseEntity_ConstructorWithId_ShouldUseProvidedId()
    {
        // Arrange
        var expectedId = Guid.NewGuid();

        // Act
        var entity = new TestEntity(expectedId);

        // Assert
        Assert.Equal(expectedId, entity.Id);
    }

    [Fact]
    public void BaseEntity_ConstructorWithId_ShouldSetCreatedAt()
    {
        // Arrange
        var id = Guid.NewGuid();
        var beforeCreation = DateTime.UtcNow;

        // Act
        var entity = new TestEntity(id);
        var afterCreation = DateTime.UtcNow;

        // Assert
        Assert.True(entity.CreatedAt >= beforeCreation);
        Assert.True(entity.CreatedAt <= afterCreation);
    }

    [Fact]
    public void BaseEntity_MarkAsUpdated_ShouldSetUpdatedAt()
    {
        // Arrange
        var entity = new TestEntity();
        var beforeUpdate = DateTime.UtcNow;

        // Act
        entity.Update();
        var afterUpdate = DateTime.UtcNow;

        // Assert
        Assert.NotNull(entity.UpdatedAt);
        Assert.True(entity.UpdatedAt >= beforeUpdate);
        Assert.True(entity.UpdatedAt <= afterUpdate);
        Assert.Equal(DateTimeKind.Utc, entity.UpdatedAt.Value.Kind);
    }

    [Fact]
    public void BaseEntity_MarkAsUpdated_ShouldUpdateUpdatedAtOnMultipleCalls()
    {
        // Arrange
        var entity = new TestEntity();
        entity.Update();
        var firstUpdate = entity.UpdatedAt;

        // Act
        System.Threading.Thread.Sleep(10);
        entity.Update();

        // Assert
        Assert.NotNull(entity.UpdatedAt);
        Assert.True(entity.UpdatedAt > firstUpdate);
    }

    [Fact]
    public void BaseEntity_Equals_SameReference_ShouldReturnTrue()
    {
        // Arrange
        var entity = new TestEntity();

        // Act
        var result = entity.Equals(entity);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void BaseEntity_Equals_SameId_ShouldReturnTrue()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity1 = new TestEntity(id);
        var entity2 = new TestEntity(id);

        // Act
        var result = entity1.Equals(entity2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void BaseEntity_Equals_DifferentId_ShouldReturnFalse()
    {
        // Arrange
        var entity1 = new TestEntity();
        var entity2 = new TestEntity();

        // Act
        var result = entity1.Equals(entity2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void BaseEntity_Equals_Null_ShouldReturnFalse()
    {
        // Arrange
        var entity = new TestEntity();

        // Act
        var result = entity.Equals(null);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void BaseEntity_Equals_DifferentType_ShouldReturnFalse()
    {
        // Arrange
        var entity = new TestEntity();
        var otherObject = new object();

        // Act
        var result = entity.Equals(otherObject);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void BaseEntity_Equals_DifferentEntityType_SameId_ShouldReturnFalse()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity1 = new TestEntity(id);
        var entity2 = new AnotherTestEntity(id);

        // Act
        var result = entity1.Equals(entity2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void BaseEntity_GetHashCode_SameId_ShouldReturnSameHashCode()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity1 = new TestEntity(id);
        var entity2 = new TestEntity(id);

        // Act
        var hashCode1 = entity1.GetHashCode();
        var hashCode2 = entity2.GetHashCode();

        // Assert
        Assert.Equal(hashCode1, hashCode2);
    }

    [Fact]
    public void BaseEntity_GetHashCode_DifferentId_ShouldReturnDifferentHashCode()
    {
        // Arrange
        var entity1 = new TestEntity();
        var entity2 = new TestEntity();

        // Act
        var hashCode1 = entity1.GetHashCode();
        var hashCode2 = entity2.GetHashCode();

        // Assert
        Assert.NotEqual(hashCode1, hashCode2);
    }

    [Fact]
    public void BaseEntity_EqualityOperator_SameId_ShouldReturnTrue()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity1 = new TestEntity(id);
        var entity2 = new TestEntity(id);

        // Act
        var result = entity1 == entity2;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void BaseEntity_EqualityOperator_DifferentId_ShouldReturnFalse()
    {
        // Arrange
        var entity1 = new TestEntity();
        var entity2 = new TestEntity();

        // Act
        var result = entity1 == entity2;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void BaseEntity_EqualityOperator_BothNull_ShouldReturnTrue()
    {
        // Arrange
        TestEntity? entity1 = null;
        TestEntity? entity2 = null;

        // Act
        var result = entity1 == entity2;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void BaseEntity_EqualityOperator_OneNull_ShouldReturnFalse()
    {
        // Arrange
        var entity1 = new TestEntity();
        TestEntity? entity2 = null;

        // Act
        var result1 = entity1 == entity2;
        var result2 = entity2 == entity1;

        // Assert
        Assert.False(result1);
        Assert.False(result2);
    }

    [Fact]
    public void BaseEntity_InequalityOperator_SameId_ShouldReturnFalse()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity1 = new TestEntity(id);
        var entity2 = new TestEntity(id);

        // Act
        var result = entity1 != entity2;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void BaseEntity_InequalityOperator_DifferentId_ShouldReturnTrue()
    {
        // Arrange
        var entity1 = new TestEntity();
        var entity2 = new TestEntity();

        // Act
        var result = entity1 != entity2;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void BaseEntity_InequalityOperator_BothNull_ShouldReturnFalse()
    {
        // Arrange
        TestEntity? entity1 = null;
        TestEntity? entity2 = null;

        // Act
        var result = entity1 != entity2;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void BaseEntity_InequalityOperator_OneNull_ShouldReturnTrue()
    {
        // Arrange
        var entity1 = new TestEntity();
        TestEntity? entity2 = null;

        // Act
        var result1 = entity1 != entity2;
        var result2 = entity2 != entity1;

        // Assert
        Assert.True(result1);
        Assert.True(result2);
    }

    [Fact]
    public void BaseEntity_MultipleInstances_ShouldHaveUniqueIds()
    {
        // Arrange & Act
        var entities = new List<TestEntity>();
        for (int i = 0; i < 100; i++)
        {
            entities.Add(new TestEntity());
        }

        // Assert
        var uniqueIds = entities.Select(e => e.Id).Distinct().ToList();
        Assert.Equal(100, uniqueIds.Count);
    }

    [Fact]
    public void BaseEntity_CreatedAt_ShouldNotChangeAfterCreation()
    {
        // Arrange
        var entity = new TestEntity();
        var originalCreatedAt = entity.CreatedAt;

        // Act
        System.Threading.Thread.Sleep(10);
        entity.Update();

        // Assert
        Assert.Equal(originalCreatedAt, entity.CreatedAt);
    }
}
