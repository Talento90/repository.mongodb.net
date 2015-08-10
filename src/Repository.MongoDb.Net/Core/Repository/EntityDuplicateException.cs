using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repository
{
    [Serializable]
    public class EntityDuplicateException : EntityException
    {
        public override int InternalExceptionCode
        {
            get
            {
                return CoreException.EntityDuplicateExpectionCode;
            }
        }

        public EntityDuplicateException(object entity, string message) : base(entity, message)
        {
        }

        public EntityDuplicateException(object entity, string message, Exception inner) : base(entity, message, inner)
        {
        }
    }
}
