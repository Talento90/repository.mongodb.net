using System;

namespace Core.Repository
{
    public class EntityException : CoreException
    {
        public object Entity { get; private set; }

        public override int InternalExceptionCode
        {
            get
            {
                return CoreException.EntityExpectionCode;
            }
        }

        public EntityException(object entity, string message) : base(message)
        {
            this.Entity = entity;
        }

        public EntityException(object entity, string message, Exception inner) : base(message, inner)
        {
            this.Entity = entity;
        }

    }
}
