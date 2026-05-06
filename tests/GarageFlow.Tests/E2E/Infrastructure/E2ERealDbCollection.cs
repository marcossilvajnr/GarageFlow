namespace GarageFlow.Tests.E2E.Infrastructure;

[CollectionDefinition("E2E Real DB", DisableParallelization = true)]
public sealed class E2ERealDbCollection : ICollectionFixture<E2ERealDbWebApplicationFactory>
{
}
