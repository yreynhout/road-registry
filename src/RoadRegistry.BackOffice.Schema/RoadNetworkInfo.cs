﻿namespace RoadRegistry.BackOffice.Schema
{
    using System;

    public class RoadNetworkInfo
    {
        public int Id { get; set; } = 0;
        public bool CompletedImport { get; set; }
        public int OrganizationCount { get; set; }
        public int RoadNodeCount { get; set; }
        public int TotalRoadNodeShapeLength { get; set; }
        public int RoadSegmentCount { get; set; }
        public int TotalRoadSegmentShapeLength { get; set; }
        public int RoadSegmentEuropeanRoadAttributeCount { get; set; }
        public int RoadSegmentNumberedRoadAttributeCount { get; set; }
        public int RoadSegmentNationalRoadAttributeCount { get; set; }
        public int RoadSegmentLaneAttributeCount { get; set; }
        public int RoadSegmentWidthAttributeCount { get; set; }
        public int RoadSegmentSurfaceAttributeCount { get; set; }
        public int GradeSeparatedJunctionCount { get; set; }
    }
}
