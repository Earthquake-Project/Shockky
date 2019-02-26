﻿using System;

namespace Shockky.Chunks.Enum
{
    [Flags]
    public enum BitmapFlags : byte
    {
        None,
        Dither                  = 1 << 0,
        CenterRegistrationPoint = 1 << 5,
        TrimWhitespace          = 1 << 7,
    }
}
