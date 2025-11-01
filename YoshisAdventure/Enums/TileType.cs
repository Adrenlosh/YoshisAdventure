using System;

namespace YoshisAdventure.Enums
{
    [Flags]
    public enum TileType
    {
        None = 0,
        Platform = 1 << 0,        // 1
        Blocking = 1 << 1,        // 2
        Penetrable = 1 << 2,      // 4
        Interactive = 1 << 3,     // 8
        Breakable = 1 << 4,       // 16
        SteepSlopeLeft = 1 << 5,  // 32
        SteepSlopeRight = 1 << 6, // 64
        GentleSlopeLeft = 1 << 7, // 128
        GentleSlopeRight = 1 << 8, // 256
        MapPathway = 1 << 9    // 512
    }
}