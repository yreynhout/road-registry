namespace RoadRegistry.BackOffice.Api.ZipArchiveWriters
{
    using System.IO.Compression;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;

    public interface ITemplatedZipArchiveWriter<in TContext> where TContext : DbContext
    {
        Task WriteAsync(ZipArchive template, ZipArchive archive, TContext context, CancellationToken cancellationToken);
    }
}
