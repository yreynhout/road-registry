namespace RoadRegistry.BackOffice.Api.ZipArchiveWriters.ForEditor
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Editor.Schema;
    using Editor.Schema.RoadSegments;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.IO;

    public class RoadSegmentEuropeanRoadAttributesToZipArchiveWriter : IZipArchivePathWriter<EditorContext>
    {
        private readonly RecyclableMemoryStreamManager _manager;
        private readonly Encoding _encoding;

        public RoadSegmentEuropeanRoadAttributesToZipArchiveWriter(RecyclableMemoryStreamManager manager,
            Encoding encoding)
        {
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
        }

        public async Task WriteAsync(
            ZipArchive archive,
            ZipPath path,
            EditorContext context,
            CancellationToken cancellationToken)
        {
            if (archive == null) throw new ArgumentNullException(nameof(archive));
            if (context == null) throw new ArgumentNullException(nameof(context));

            var count = await context.RoadSegmentEuropeanRoadAttributes.CountAsync(cancellationToken);
            var dbfEntry = archive.CreateEntry(path.Combine("AttEuropweg.dbf"));
            var dbfHeader = new DbaseFileHeader(
                DateTime.Now,
                DbaseCodePage.Western_European_ANSI,
                new DbaseRecordCount(count),
                RoadSegmentEuropeanRoadAttributeDbaseRecord.Schema
            );
            await using (var dbfEntryStream = dbfEntry.Open())
            using (var dbfWriter =
                new DbaseBinaryWriter(
                    dbfHeader,
                    new BinaryWriter(dbfEntryStream, _encoding, true)))
            {
                var dbfRecord = new RoadSegmentEuropeanRoadAttributeDbaseRecord();
                foreach (var data in context.RoadSegmentEuropeanRoadAttributes.OrderBy(_ => _.Id).Select(_ => _.DbaseRecord))
                {
                    dbfRecord.FromBytes(data, _manager, _encoding);
                    dbfWriter.Write(dbfRecord);
                }

                dbfWriter.Writer.Flush();
                await dbfEntryStream.FlushAsync(cancellationToken);
            }
        }
    }
}
