namespace Application.Abstractions.Services
{
    public interface IPhotoUploader
    {
        Task UploadOrderPhotoAsync(Guid orderId, string photoBase64, Guid customerId);
    }
}
