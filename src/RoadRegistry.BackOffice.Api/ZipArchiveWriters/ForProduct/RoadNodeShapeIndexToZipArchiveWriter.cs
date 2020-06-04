namespace RoadRegistry.BackOffice.Api.ZipArchiveWriters.ForProduct
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.IO;
    using Product.Schema;

    public class RoadNodeShapeIndexToZipArchiveWriter : IZipArchivePathWriter<ProductContext>
    {
        private readonly RecyclableMemoryStreamManager _manager;
        private readonly Encoding _encoding;

        public RoadNodeShapeIndexToZipArchiveWriter(RecyclableMemoryStreamManager manager, Encoding encoding)
        {
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
        }

        public async Task WriteAsync(
            ZipArchive archive,
            ZipPath path,
            ProductContext context,
            CancellationToken cancellationToken)
        {
            if (archive == null) throw new ArgumentNullException(nameof(archive));
            if (context == null) throw new ArgumentNullException(nameof(context));

            var count = await context.RoadNodes.CountAsync(cancellationToken);
            
            var shpBoundingBox = count > 0
                ? (await context.RoadNodeBoundingBox.SingleAsync(cancellationToken)).ToBoundingBox3D()
                : BoundingBox3D.Empty;

            var info = await context.RoadNetworkInfo.SingleAsync(cancellationToken);

            var shpHeader = new ShapeFileHeader(
                new WordLength(info.TotalRoadNodeShapeLength),
                ShapeType.Point,
                shpBoundingBox);
            
            var shxEntry = archive.CreateEntry(path.Combine("Wegknoop.shx"));
            var shxHeader = shpHeader.ForIndex(new ShapeRecordCount(count));
            await using (var shxEntryStream = shxEntry.Open())
            using (var shxWriter =
                new ShapeIndexBinaryWriter(
                    shxHeader,
                    new BinaryWriter(shxEntryStream, _encoding, true)))
            {
                var offset = ShapeIndexRecord.InitialOffset;
                var number = RecordNumber.Initial;
                foreach (var data in context.RoadNodes.OrderBy(_ => _.Id).Select(_ => _.ShapeRecordContent))
                {
                    var shpRecord = ShapeContentFactory
                        .FromBytes(data, _manager, _encoding)
                        .RecordAs(number);
                    shxWriter.Write(shpRecord.IndexAt(offset));
                    number = number.Next();
                    offset = offset.Plus(shpRecord.Length);
                }
                shxWriter.Writer.Flush();
                await shxEntryStream.FlushAsync(cancellationToken);
            }
        }
    }
}
