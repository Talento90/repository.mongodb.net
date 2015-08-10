using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repository
{
    public class EntityConflictException : EntityException
    {
        public override int InternalExceptionCode
        {
            get
            {
                return CoreException.EntityConflictExceptionCode;
            }
        }

        public EntityConflictException(object entity, string message) : base(entity, message)
        {
        }

        public EntityConflictException(object entity, string message, Exception inner) : base(entity, message, inner)
        {
        }
    }
}
