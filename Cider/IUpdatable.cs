using System;
using System.Collections.Generic;
using System.Text;

namespace Cider
{
    public interface IUpdatable
    {
        void Update(TimeSpan delta);
    }
}
