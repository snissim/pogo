using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeProse.Pogo
{
    public interface IPogoQueryCustomization
    {
        IPogoQueryCustomization WaitForNonStaleResults();
    }
}
