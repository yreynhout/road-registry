namespace RoadRegistry.BackOffice.Framework.Testing.Projections
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector.Testing;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using KellermanSoftware.CompareNetObjects;
    using Microsoft.EntityFrameworkCore;
    using Schema;
    using Xunit.Sdk;

    internal class MemoryBackOfficeContext : BackOfficeContext {
        public MemoryBackOfficeContext(DbContextOptions<BackOfficeContext> options) : base(options) { }

        protected override void OnModelQueryTypes(ModelBuilder builder)
        {
            builder
                .Entity<RoadNodeBoundingBox2D>()
                .HasNoKey()
                .ToQuery(() =>
                    from node in RoadNodes
                    group node by node.Id
                    into nodes
                    select new RoadNodeBoundingBox2D
                    {
                        MinimumX = nodes.Min(n => n.BoundingBox.MinimumX),
                        MaximumX = nodes.Max(n => n.BoundingBox.MaximumX),
                        MinimumY = nodes.Min(n => n.BoundingBox.MinimumY),
                        MaximumY = nodes.Max(n => n.BoundingBox.MaximumY)
                    }
                );
            builder
                .Entity<RoadSegmentBoundingBox3D>()
                .HasNoKey()
                .ToQuery(() =>
                    from segment in RoadSegments
                    group segment by segment.Id
                    into segments
                    select new RoadSegmentBoundingBox3D
                    {
                        MinimumX = segments.Min(n => n.BoundingBox.MinimumX),
                        MaximumX = segments.Max(n => n.BoundingBox.MaximumX),
                        MinimumY = segments.Min(n => n.BoundingBox.MinimumY),
                        MaximumY = segments.Max(n => n.BoundingBox.MaximumY),
                        MinimumM = segments.Min(n => n.BoundingBox.MinimumM),
                        MaximumM = segments.Max(n => n.BoundingBox.MaximumM)
                    }
                );
        }
    }

    public static class BackOfficeContextScenarioExtensions
    {
        public static ConnectedProjectionScenario<BackOfficeContext> Scenario(this ConnectedProjection<BackOfficeContext> projection) =>
            new ConnectedProjectionScenario<BackOfficeContext>(Resolve.WhenEqualToHandlerMessageType(projection.Handlers));

        public static async Task ExpectNone(this ConnectedProjectionScenario<BackOfficeContext> scenario)
        {
            var database = Guid.NewGuid().ToString("N");

            var specification = scenario.Verify(async context =>
            {
                var actualRecords = await context.AllRecords();
                return actualRecords.Length == 0
                    ? VerificationResult.Pass()
                    : VerificationResult.Fail($"Expected 0 records but found {actualRecords.Length}.");
            });

            using (var context = CreateContextFor(database))
            {
                var projector = new ConnectedProjector<BackOfficeContext>(specification.Resolver);
                foreach (var message in specification.Messages)
                {
                    var envelope = new Envelope(message, new Dictionary<string, object>()).ToGenericEnvelope();
                    await projector.ProjectAsync(context, envelope);
                }
                await context.SaveChangesAsync();
            }

            using (var context = CreateContextFor(database))
            {
                var result = await specification.Verification(context, CancellationToken.None);

                if (result.Failed)
                    throw specification.CreateFailedScenarioExceptionFor(result);
            }
        }

        public static Task Expect(
            this ConnectedProjectionScenario<BackOfficeContext> scenario,
            IEnumerable<object> records)
        {
            return scenario.Expect(records.ToArray());
        }

        public static async Task Expect(
            this ConnectedProjectionScenario<BackOfficeContext> scenario,
            params object[] records)
        {
            var database = Guid.NewGuid().ToString("N");

            var specification = scenario.Verify(async context =>
            {
                var comparisonConfig = new ComparisonConfig { MaxDifferences = 5};
                var comparer = new CompareLogic(comparisonConfig);
                var actualRecords = await context.AllRecords();
                var result = comparer.Compare(
                    actualRecords,
                    records
                );

                return result.AreEqual
                    ? VerificationResult.Pass()
                    : VerificationResult.Fail(result.CreateDifferenceMessage(actualRecords, records));
            });

            using (var context = CreateContextFor(database))
            {
                var projector = new ConnectedProjector<BackOfficeContext>(specification.Resolver);
                var position = 0L;
                foreach (var message in specification.Messages)
                {
                    var envelope = new Envelope(message, new Dictionary<string, object> { { "Position", position }}).ToGenericEnvelope();
                    await projector.ProjectAsync(context, envelope);
                    position++;
                }

                await context.SaveChangesAsync();
            }

            using (var context = CreateContextFor(database))
            {
                var result = await specification.Verification(context, CancellationToken.None);

                if (result.Failed)
                    throw specification.CreateFailedScenarioExceptionFor(result);
            }
        }

        private static async Task<object[]> AllRecords(this BackOfficeContext context)
        {
            var records = new List<object>();
            records.AddRange(await context.RoadNodes.ToArrayAsync());
            records.AddRange(await context.RoadSegments.ToArrayAsync());
            records.AddRange(await context.RoadSegmentLaneAttributes.ToArrayAsync());
            records.AddRange(await context.RoadSegmentWidthAttributes.ToArrayAsync());
            records.AddRange(await context.RoadSegmentSurfaceAttributes.ToArrayAsync());
            records.AddRange(await context.RoadSegmentEuropeanRoadAttributes.ToArrayAsync());
            records.AddRange(await context.RoadSegmentNationalRoadAttributes.ToArrayAsync());
            records.AddRange(await context.RoadSegmentNumberedRoadAttributes.ToArrayAsync());
            records.AddRange(await context.GradeSeparatedJunctions.ToArrayAsync());
            records.AddRange(await context.Organizations.ToArrayAsync());
            records.AddRange(await context.RoadNetworkInfo.ToArrayAsync());
            records.AddRange(await context.RoadNetworkChanges.ToArrayAsync());
            return records.ToArray();
        }

        private static BackOfficeContext CreateContextFor(string database)
        {
            var options = new DbContextOptionsBuilder<BackOfficeContext>()
                .UseInMemoryDatabase(database)
                .EnableSensitiveDataLogging()
                .Options;

            return new MemoryBackOfficeContext(options);
        }

        private static XunitException CreateFailedScenarioExceptionFor(this ConnectedProjectionTestSpecification<BackOfficeContext> specification, VerificationResult result)
        {
            var title = string.Empty;
            var exceptionMessage = new StringBuilder()
                .AppendLine(title)
                .AppendTitleBlock("Given", specification.Messages, Formatters.NamedJsonMessage)
                .Append(result.Message);

            return new XunitException(exceptionMessage.ToString());
        }
    }
}
