namespace RoadRegistry.BackOffice.Framework.Containers
{
    using Xunit;

    [CollectionDefinition(nameof(SqlServerCollection))]
    public class SqlServerCollection : ICollectionFixture<SqlServer>
    {
    }
}