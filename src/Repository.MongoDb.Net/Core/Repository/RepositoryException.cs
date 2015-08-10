using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repository
{
    [Serializable]
    public class RepositoryException : CoreException
    {
        public override int InternalExceptionCode
        {
            get
            {
                return CoreException.RepositoryExpectionCode;
            }
        }

        public RepositoryException(string message)
            : base(message)
        {
            
        }
    }
}
