using Application.Features.ChatRoomInvites.DTOs;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappings
{
    public class ChatRoomInviteProfile : Profile
    {
        public ChatRoomInviteProfile()
        {
            CreateMap<ChatRoomInvite, ChatRoomInviteDto>()
                .ForMember(d => d.InviterUserName, opt => opt.MapFrom(s => s.InviterUser.UserName))
                .ForMember(d => d.InviterFileId, opt => opt.MapFrom(s => s.InviterUser.FileId))
                .ForMember(d => d.TargetRoomType, opt => opt.MapFrom(s => s.TargetRoomType.ToString()))
                .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.CreatedAt, opt => opt.MapFrom(s => s.CreatedDate));
        }
    }
}
