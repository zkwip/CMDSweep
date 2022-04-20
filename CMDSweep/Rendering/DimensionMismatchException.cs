using System;
using System.Runtime.Serialization;

namespace CMDSweep.Rendering
{
    [Serializable]
    internal class DimensionMismatchException : Exception
    {
        public DimensionMismatchException()
        {

        }

        public DimensionMismatchException(string? message) : base(message)
        {

        }

        public DimensionMismatchException(string? message, Exception? innerException) : base(message, innerException)
        {

        }

        protected DimensionMismatchException(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }
    }
}