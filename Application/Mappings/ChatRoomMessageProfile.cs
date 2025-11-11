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
                           opt => opt.MapFrom(src => src.SenderUser.UserName));
        }
    }
}
