using AutoMapper;
using markit.Application.Common.Helpers;
using markit.Application.Features.Account.Commands.CreateAccount;
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
using markit.Application.Helpers;
using markit.Application.Models.Authentication;
using markit.Application.Models.Authentication.AppUser;
using markit.Application.Models.Authentication.Enums;
using markit.Domain.Entities;
using static markit.Application.Helpers.GeneralConstant;

namespace markit.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            #region Accounts
            CreateMap<ExternalUser, CreateAccountCommand>()
                .ForMember(
                    dest => dest.AccessType,
                    opt => opt.MapFrom(src => AccessType.External)
                )
                .ForMember(
                    dest => dest.Roles,
                    opt => opt.MapFrom(src => new string[] { Role.GENERAL_NAME })
                )
                .ForMember(
                    dest => dest.Enabled,
                    opt => opt.MapFrom(src => false)
                );
            #endregion

            #region Collections
            CreateMap<CreateCollectionCommand, Collection>();
            CreateMap<UpdateCollectionCommand, Collection>();
            CreateMap<Collection, CollectionViewModel>()
                .ForMember(dest => dest.Path, opt => opt.Ignore())
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
                    dest => dest.CreatedAt,
                    opt => opt.MapFrom(src => src.CreatedDate)
                )
                .ForMember(
                    dest => dest.Emoji,
                    opt => opt.MapFrom(src => src.Emoji)
                )
                .ForMember(
                    dest => dest.Preview,
                    opt => opt.MapFrom(src => src.Marks != null 
                    ? $"{ src.Marks.Count } marks" 
                    : GeneralConstant.Marks.COLLECTION_DEFAULT_PREVIEW)
                );
            #endregion

            #region Creators
            CreateMap<CreateCreatorCommand, Creator>();

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
            #endregion

            #region Blocks
            CreateMap<BlockViewModel, Block>();
            CreateMap<Block, BlockViewModel>();
            #endregion

            #region Marks
            CreateMap<CreateMarkCommand, Mark>()
                .ForMember(dest => dest.Blocks, opt => opt.Ignore())
                .AfterMap((src, dest) =>
                {
                    dest.Blocks = [];
                    for( int i = 0; i < src.Blocks.Count; i++ ) {
                        var sourceBlock = src.Blocks[i];

                        dest.Blocks?.Add(new Block
                        {
                            Title = sourceBlock.Title,
                            Content = sourceBlock.Content,
                            Order = i + 1
                        });
                    }
                });

            CreateMap<UpdateMarkCommand, Mark>()
                .ForMember(
                    dest => dest.NameLess,
                    opt  => opt.MapFrom(src => string.IsNullOrWhiteSpace(src.InputName))
                )
                .ForMember(
                    dest => dest.Name,
                    opt => opt.MapFrom(
                        src => string.IsNullOrWhiteSpace(src.InputName) 
                            ? GeneralConstant.Marks.MARK_PLACEHOLDER 
                            : src.InputName
                    )
                )
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

                            destinyBlock.Title = sourceBlock.Title;
                            destinyBlock.Content = sourceBlock.Content;
                            destinyBlock.Order = currentOrder;
                        }
                        // Otherwise, add the new block
                        else
                        {
                            dest.Blocks?.Add(new Block
                            {
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
                .ForMember(
                    dest => dest.Blocks,
                    opt => opt.MapFrom(
                        src => src.Blocks != null
                        ? src.Blocks.OrderBy(b => b.Order).ToList()
                        : new List<Block>()
                    )
                )
                .ForMember(
                    dest => dest.CreatorId,
                    opt => opt.MapFrom(src => src.Collection != null
                        ? src.Collection.CreatorId
                        : (int?)null
                    )
                )
                .ForMember(
                    dest => dest.CollectionName,
                    opt => opt.MapFrom(src => src.Collection != null
                        ? src.Collection.Name
                        : "Not found"
                    )
                )
                .ForMember(
                    dest => dest.InputName,
                    opt => opt.MapFrom(src => src.NameLess.Equals(true) ? "" : src.Name)
                );

            CreateMap<Mark, CollectionItem>()
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
                    opt => opt.MapFrom(src => src.CollectionId)
                )
                .ForMember(
                    dest => dest.Type,
                    opt => opt.MapFrom(src => CollectionItemType.Mark)
                )
                .ForMember(
                    dest => dest.TypeId,
                    opt => opt.MapFrom(src => src.Id)
                )
                .ForMember(
                    dest => dest.CreatedAt,
                    opt => opt.MapFrom(src => src.CreatedDate)
                )
                .ForMember(
                    dest => dest.UpdatedAt,
                    opt => opt.MapFrom( src => src.Blocks != null
                        ? src.Blocks.GetMostRecentBlockDate()
                        : src.UpdatedDate
                    )
                )
                .ForMember(
                    dest => dest.Emoji,
                    opt => opt.MapFrom(src => src.Emoji)
                )
                .ForMember(
                    dest => dest.Preview,
                    opt => opt.MapFrom(src => Utilities.CreateMarkPreview(src))
                );
            #endregion
        }
    }
}
