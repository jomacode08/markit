using AutoMapper;
using markit.Application.Features.Creators.Commands.CreateCreator;
using markit.Application.Features.Creators.Commands.UpdateCreator;
using markit.Application.Features.Creators.Queries.ViewModels;
using markit.Application.Models.Authentication.AppUser;
using markit.Domain.Entities;

namespace markit.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Creator
            CreateMap<CreateCreatorCommand, Creator>();
            CreateMap<CreateAppUserRequest, CreateCreatorCommand>();

            CreateMap<UpdateCreatorCommand, Creator>()
                .ForMember(
                    dest => dest.BirthDate, 
                    opt  => opt.MapFrom(src => DateOnly.Parse(src.BirthDate))
                );

            CreateMap<Creator, CreatorViewModel>()
                .ForMember(
                    dest => dest.BirthDate,
                    opt  => opt.MapFrom(src => src.BirthDate.ToString())
                );

        }
    }
}
