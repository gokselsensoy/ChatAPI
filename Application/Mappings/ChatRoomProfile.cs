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
                           opt => opt.MapFrom(src => src.ChatRoomUserMaps.Count))
                .ForMember(dest => dest.LastMessagePreview, opt => opt.Ignore())
                .ForMember(dest => dest.LastMessageAt, opt => opt.Ignore())
                .ForMember(dest => dest.LastMessageSenderUserId, opt => opt.Ignore())
                .ForMember(dest => dest.HasNew, opt => opt.Ignore())
                .ForMember(dest => dest.UnreadCount, opt => opt.Ignore());
        }
    }
}
