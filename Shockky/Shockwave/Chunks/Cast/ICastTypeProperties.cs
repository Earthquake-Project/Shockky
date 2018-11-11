﻿using System.Drawing;

using Shockky.IO;

namespace Shockky.Shockwave.Chunks.Cast
{
    public interface ICastTypeProperties
    {
        Rectangle Rectangle { get; set; }

        int GetBodySize();
        void WriteTo(ShockwaveWriter output);
    }
}
