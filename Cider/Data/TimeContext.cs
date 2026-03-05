using System;
using System.Collections.Generic;
using System.Text;

namespace Cider.Data
{
    public readonly record struct TimeContext(TimeSpan DeltaTime)
    {
    }
}
