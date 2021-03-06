namespace RoadRegistry.BackOffice.Uploads
{
    using System;
    using System.IO.Compression;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Core;
    using Schema;

    public class TransactionZoneDbaseRecordsTranslator : IZipArchiveDbaseRecordsTranslator<TransactionZoneDbaseRecord>
    {
        public TranslatedChanges Translate(ZipArchiveEntry entry, IDbaseRecordEnumerator<TransactionZoneDbaseRecord> records, TranslatedChanges changes)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            if (records == null) throw new ArgumentNullException(nameof(records));
            if (changes == null) throw new ArgumentNullException(nameof(changes));

            if (records.MoveNext() && records.Current != null)
            {
                return changes
                    .WithReason(new Reason(records.Current.BESCHRIJV.Value))
                    .WithOperatorName(new OperatorName(records.Current.OPERATOR.Value))
                    .WithOrganization(new OrganizationId(records.Current.ORG.Value));
            }
            return changes;
        }
    }
}
