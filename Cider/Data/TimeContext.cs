using System;
using System.Collections.Generic;
using System.Text;

namespace Cider.Data
{
    public readonly struct TimeContext(TimeSpan deltaTime)
    {
        public readonly TimeSpan DeltaTime = deltaTime;
    }
}
