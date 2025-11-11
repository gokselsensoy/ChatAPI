using Application.Features.ChatRooms.DTOs;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappings
{
    public class ChatRoomProfile : Profile
    {
        public ChatRoomProfile()
        {
            CreateMap<ChatRoom, ChatRoomDto>()
                .ForMember(dest => dest.MemberCount,
                           opt => opt.MapFrom(src => src.ChatRoomUserMaps.Count));
        }
    }
}
