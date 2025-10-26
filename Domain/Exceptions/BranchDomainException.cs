namespace Domain.Exceptions
{
    public class BranchDomainException : DomainException
    {
        public BranchDomainException() { }
        public BranchDomainException(string message) : base(message) { }
        public BranchDomainException(string message, Exception innerException) : base(message, innerException) { }
    }
}
