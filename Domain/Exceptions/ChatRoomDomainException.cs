namespace Domain.Exceptions
{
    public class ChatRoomDomainException : DomainException
    {
        public ChatRoomDomainException() { }
        public ChatRoomDomainException(string message) : base(message) { }
        public ChatRoomDomainException(string message, Exception innerException) : base(message, innerException) { }
    }
}
