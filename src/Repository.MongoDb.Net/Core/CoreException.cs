using System;

namespace Core
{
    [Serializable]
    public class CoreException : Exception
    {
        public const int CoreExceptionCode = 1000;
        public const int RepositoryExpectionCode = 2000;
        public const int EntityExpectionCode = 2100;
        public const int EntityDuplicateExpectionCode = 2200;
        public const int EntityConflictExceptionCode = 2300;

        public virtual int InternalExceptionCode
        {
            get
            {
                return CoreExceptionCode;
            }
        }

        public CoreException(string message): base(message)
        {

        }

        public CoreException(string message, Exception ex): base(message, ex)
        {

        }
    }
}
