namespace RoadRegistry.BackOffice.Api.ZipArchiveWriters
{
    using System;
    using System.IO.Compression;
    using System.Threading;
    using System.Threading.Tasks;
    using Editor.Schema;
    using Microsoft.EntityFrameworkCore;

    public class CompositeZipArchiveWriter<TContext> : IZipArchivePathWriter<TContext> where TContext : DbContext
    {
        private readonly IZipArchivePathWriter<TContext>[] _writers;

        public CompositeZipArchiveWriter(params IZipArchivePathWriter<TContext>[] writers)
        {
            _writers = writers ?? throw new ArgumentNullException(nameof(writers));
        }

        public async Task WriteAsync(
            ZipArchive archive,
            ZipPath path,
            TContext context,
            CancellationToken cancellationToken)
        {
            if (archive == null) throw new ArgumentNullException(nameof(archive));
            if (context == null) throw new ArgumentNullException(nameof(context));
            foreach (var writer in _writers)
            {
                await writer.WriteAsync(archive, path, context, cancellationToken);
            }
        }
    }
}
