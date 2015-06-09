using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Utils
{
    public static class Guard
    {

        public static void ThrowIfNull(this object source, string parameterName)
        {
            if (source == null)
                throw new ArgumentNullException(parameterName);
        }
    }
}
