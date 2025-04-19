using AutoMapper;
using markit.Application.Features.Blocks.Queries.ViewModels;
using markit.Application.Features.Collections.Commands.CreateCollectionCommand;
using markit.Application.Features.Collections.Commands.UpdateCollectionCommand;
using markit.Application.Features.Collections.Queries.ViewModels;
using markit.Application.Features.Creators.Commands.CreateCreator;
using markit.Application.Features.Creators.Commands.UpdateCreator;
using markit.Application.Features.Creators.Queries.ViewModels;
using markit.Application.Features.Marks.Commands.CreateMarkCommand;
using markit.Application.Features.Marks.Commands.UpdateMarkCommand;
using markit.Application.Features.Marks.Queries.ViewModels;
using markit.Application.Models.Authentication.AppUser;
using markit.Domain.Entities;

namespace markit.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Collections
            CreateMap<CreateCollectionCommand, Collection>();
            CreateMap<UpdateCollectionCommand, Collection>();
            CreateMap<Collection, CollectionViewModel>()
                .ForMember(dest => dest.CollectionItems, opt => opt.Ignore());

            CreateMap<Collection, CollectionItem>()
                .ForMember(
                    dest => dest.Id,
                    opt => opt.MapFrom(src => Guid.NewGuid().ToString())
                )
                .ForMember(
                    dest => dest.Name,
                    opt => opt.MapFrom(src => src.Name)
                )
                .ForMember(
                    dest => dest.CollectionId,
                    opt => opt.MapFrom(src => src.ParentId)
                )
                .ForMember(
                    dest => dest.Type,
                    opt => opt.MapFrom(src => CollectionItemType.Collection)
                )
                .ForMember(
                    dest => dest.TypeId,
                    opt => opt.MapFrom(src => src.Id)
                )
                .ForMember(
                    dest => dest.Preview,
                    opt => opt.MapFrom(src => src.Marks != null 
                    ? $"{ src.Marks.Count } marks" 
                    : "0 marks")
                );

            // Creators
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

            // Blocks
            CreateMap<BlockViewModel, Block>();
            CreateMap<Block, BlockViewModel>();

            // Marks
            CreateMap<CreateMarkCommand, Mark>()
                .ForMember(dest => dest.Blocks, opt =>
                    opt.MapFrom(src => src.Blocks));

            CreateMap<UpdateMarkCommand, Mark>()
                // Ignore Blocks initially due to AutoMapper replace the existent collection with the new one.
                // This could provocate data loss, it's better to mapping the collection manually.
                .ForMember(dest => dest.Blocks, opt => opt.Ignore())
                // Mapping blocks mannually
                .AfterMap((src, dest) =>
                {
                    List<Block> blocksToDelete = dest.Blocks?.ToList() ?? [];
                    int currentOrder = 1;

                    foreach (var sourceBlock in src.Blocks)
                    {
                        var destinyBlock = dest.Blocks?.FirstOrDefault(b => b.Id > 0 && b.Id == sourceBlock.Id);

                        // Update the existent block properties
                        if (destinyBlock != null)
                        {
                            blocksToDelete.Remove(destinyBlock);

                            destinyBlock.Cols = sourceBlock.Cols;
                            destinyBlock.Color = sourceBlock.Color;
                            destinyBlock.Title = sourceBlock.Title;
                            destinyBlock.Content = sourceBlock.Content;
                            destinyBlock.Order = currentOrder;
                        }
                        // Otherwise, add the new block
                        else
                        {
                            dest.Blocks?.Add(new Block
                            {
                                Cols = sourceBlock.Cols,
                                Color = sourceBlock.Color,
                                Title = sourceBlock.Title,
                                Content = sourceBlock.Content,
                                Order = currentOrder
                            });
                        }

                        currentOrder++;
                    }

                    // Delete the blocks that aren't in the src object, which means the user deleted them
                    foreach(var blockToDelete in blocksToDelete)
                    {
                        dest.Blocks?.Remove(blockToDelete);
                    }
                });

            CreateMap<Mark, MarkViewModel>()
                .ForMember(dest => dest.Blocks, opt =>
                    opt.MapFrom(src => src.Blocks));

        }
    }
}
