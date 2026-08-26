using Application.Features.ChatRooms.DTOs;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappings
{
    public class ChatRoomMessageProfile : Profile
    {
        public ChatRoomMessageProfile()
        {
            CreateMap<ChatRoomMessage, ChatRoomMessageDto>()
                .ForMember(dest => dest.SenderUserName,
                           opt => opt.MapFrom(src => src.SenderUser.UserName))
                .ForMember(dest => dest.SenderRole, opt => opt.Ignore())
                .ForMember(dest => dest.IsMine, opt => opt.Ignore())
                .ForMember(dest => dest.ReplyToMessageId,
                           opt => opt.MapFrom(src => src.ReplyToMessageId))
                .ForMember(dest => dest.ReplyToSenderUserId,
                           opt => opt.MapFrom(src => src.ReplyToMessage != null
                               ? (Guid?)src.ReplyToMessage.SenderUserId
                               : null))
                .ForMember(dest => dest.ReplyToSenderUserName,
                           opt => opt.MapFrom(src => src.ReplyToMessage != null
                               ? src.ReplyToMessage.SenderUser.UserName
                               : null))
                .ForMember(dest => dest.ReplyToMessage,
                           opt => opt.MapFrom(src => src.ReplyToMessage != null
                               ? src.ReplyToMessage.Message
                               : null))
                .ForMember(dest => dest.ReplyToIsMine, opt => opt.Ignore());
        }
    }
}
