﻿using System;

namespace Mapzen
{
    public struct TileAddress : IComparable, IEquatable<TileAddress>
    {
        public TileAddress(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static TileAddress FromLngLat(LngLat lngLat, int zoom)
        {
            double tileSize = Geo.EarthCircumferenceMeters / (1 << zoom);

            MercatorMeters meters = Geo.Project(lngLat);

            int tileX = (int)(meters.x / tileSize);
            int tileY = (int)(meters.y / tileSize);

            return new TileAddress(tileX, tileY, zoom);
        }

        public readonly int x;
        public readonly int y;
        public readonly int z;

        public TileAddress Wrapped()
        {
            int max = 1 << z;
            int tileAddressX = x % max;
            int tileAddressY = y % max;
            if (tileAddressX < 0)
            {
                tileAddressX += max;
            }
            if (tileAddressY < 0)
            {
                tileAddressY += max;
            }
            return new TileAddress(tileAddressX, tileAddressY, this.z);
        }

        public TileAddress GetParent()
        {
            return new TileAddress(x >> 1, y >> 1, z - 1);
        }

        public TileAddress[] GetChildren()
        {
            var children = new TileAddress[4];
            int cx = x << 1;
            int cy = y << 1;
            int cz = z + 1;
            children[0] = new TileAddress(cx + 0, cy + 0, cz);
            children[1] = new TileAddress(cx + 0, cy + 1, cz);
            children[2] = new TileAddress(cx + 1, cy + 0, cz);
            children[3] = new TileAddress(cx + 1, cy + 1, cz);
            return children;
        }

        public double GetSizeMercatorMeters()
        {
            return Geo.EarthCircumferenceMeters / (1 << z);
        }

        public MercatorMeters GetOriginMercatorMeters()
        {
            double metersPerTile = GetSizeMercatorMeters();
            return new MercatorMeters(x * metersPerTile, y * metersPerTile);
        }

        public int CompareTo(object obj)
        {
            if (!(obj is TileAddress))
            {
                return 1;
            }
            var other = (TileAddress)obj;
            if (z != other.z)
            {
                // Lower z precedes higher z.
                return z - other.z;
            }
            if (x != other.x)
            {
                // Lower x precedes higher x.
                return x - other.x;
            }
            if (y != other.y)
            {
                // Lower y precedes higher y.
                return y - other.y;
            }
            // The addresses are equal.
            return 0;
        }

        public bool Equals(TileAddress other)
        {
            return (z == other.z) && ((y == other.y) && (x == other.x));
        }

        public override bool Equals(object obj)
        {
            if (obj is TileAddress)
            {
                return Equals((TileAddress)obj);
            }
            return false;
        }

        public override int GetHashCode()
        {
            // TODO: Find a hash function with better distribution.
            return (z ^ x) ^ y;
        }


    }
}
