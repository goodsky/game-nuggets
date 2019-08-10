using Common;

namespace Campus
{
    /// <summary>
    /// Keeper of Parking Lot information.
    /// </summary>
    public class ParkingLot
    {
        public ParkingLot(Rectangle footprint)
        {
            Footprint = footprint;
            LotLines = GenerateLotLines(footprint);
        }

        public Rectangle Footprint { get; private set; }
        public bool[,] LotLines { get; private set; }
        public int LotCount { get; private set; }

        public ParkingInfo ToParkingInfo()
        {
            return new ParkingInfo()
            {
                Id = Footprint.Start.GetHashCode() + ((long)(Footprint.End.GetHashCode()) << 32),
                ParkingSpots = LotCount,
                IsConnectedToRoad = false,
            };
        }

        private bool[,] GenerateLotLines(Rectangle footprint)
        {
            bool[,] lotSpaces = new bool[footprint.SizeX, footprint.SizeZ];

            AxisAlignment alignment = footprint.SizeX > footprint.SizeZ ? AxisAlignment.XAxis : AxisAlignment.ZAxis;

            int minorAxisSize, majorAxisSize;
            if (alignment == AxisAlignment.XAxis)
            {
                minorAxisSize = footprint.SizeZ;
                majorAxisSize = footprint.SizeX;
            }
            else
            {
                minorAxisSize = footprint.SizeX;
                majorAxisSize = footprint.SizeZ;
            }

            // adjust the blank space between even and odd size lots
            int adjustBlankSpaces = (minorAxisSize % 2);
            for (int i = 0; i < minorAxisSize; ++i)
            {
                if ((i + adjustBlankSpaces) % 3 == 0)
                {
                    continue; // leave a blank space between parking lots
                }

                for (int j = 0; j < majorAxisSize; ++j)
                {
                    ++LotCount;
                    if (alignment == AxisAlignment.XAxis)
                    {
                        lotSpaces[j, i] = true;
                    }
                    else
                    {
                        lotSpaces[i, j] = true;
                    }
                }
            }

            return lotSpaces;
        }
    }
}
