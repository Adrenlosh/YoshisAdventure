using System;

namespace YoshisAdventure.Enums
{
    [Flags]
    public enum TransitionType
    {
        In = 0,
        Out = 1 << 0,
    }
}