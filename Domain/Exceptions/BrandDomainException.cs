namespace Domain.Exceptions
{
    public class BrandDomainException : DomainException
    {
        public BrandDomainException() { }
        public BrandDomainException(string message) : base(message) { }
        public BrandDomainException(string message, Exception innerException) : base(message, innerException) { }
    }
}
