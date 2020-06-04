namespace RoadRegistry.BackOffice.Api.ZipArchiveWriters
{
    using System;
    using System.Data;
    using System.IO.Compression;
    using System.Threading;
    using System.Threading.Tasks;
    using Editor.Schema;
    using Microsoft.EntityFrameworkCore;

    public class ReadCommittedZipArchiveWriter<TContext> : IZipArchivePathWriter<TContext> where TContext: DbContext
    {
        private readonly IZipArchivePathWriter<TContext> _writer;

        public ReadCommittedZipArchiveWriter(IZipArchivePathWriter<TContext> writer)
        {
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
        }

        public async Task WriteAsync(
            ZipArchive archive,
            ZipPath path,
            TContext context,
            CancellationToken cancellationToken)
        {
            if (archive == null) throw new ArgumentNullException(nameof(archive));
            if (context == null) throw new ArgumentNullException(nameof(context));

            using (await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken))
            {
                await _writer.WriteAsync(archive, path, context, cancellationToken);
            }
        }
    }
}
