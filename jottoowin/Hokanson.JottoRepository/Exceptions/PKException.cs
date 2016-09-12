using System;

namespace Hokanson.JottoRepository.Exceptions
{
    public class PkException : Exception
    {
        public PkException()
            : base("primary key violation")
        { }

        public PkException(string message)
            : base(message)
        { }
    }
}
