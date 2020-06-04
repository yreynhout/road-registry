namespace RoadRegistry.BackOffice.Api.ZipArchiveWriters
{
    using System.IO.Compression;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;

    public interface IZipArchivePathWriter<in TContext> where TContext : DbContext
    {
        Task WriteAsync(ZipArchive archive, ZipPath path, TContext context, CancellationToken cancellationToken);
    }
}
