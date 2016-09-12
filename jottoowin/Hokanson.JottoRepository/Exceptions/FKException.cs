using System;

namespace Hokanson.JottoRepository.Exceptions
{
    public class FkException : Exception
    {
        public FkException()
            : base("foreign key violation")
        { }

        public FkException(string message)
            : base(message)
        { }
    }
}
