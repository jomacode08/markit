using AutoMapper;
using markit.Application.Features.Creators.Commands.CreateCreator;
using markit.Application.Models.Authentication;
using markit.Domain.Entities;

namespace markit.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Creator
            CreateMap<CreateCreatorCommand, Creator>();
            CreateMap<UserViewModel, CreateCreatorCommand>();
        }
    }
}
