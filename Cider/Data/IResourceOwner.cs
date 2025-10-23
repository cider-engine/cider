using System;
using System.Collections.Generic;
using System.Text;

namespace Cider.Data
{
    public interface IResourceOwner
    {
        bool UnloadResourceWhenUnreachable { get; set; }
    }
}
