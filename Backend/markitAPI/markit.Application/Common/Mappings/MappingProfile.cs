using markit.Application.Common.Helpers;
using markit.Application.Common.Mappings;
using markit.Application.Features.Account.Commands.CreateAccount;
using markit.Application.Features.Blocks.Queries.ViewModels;
using markit.Application.Features.Collections.Commands.CreateCollectionCommand;
using markit.Application.Features.Collections.Commands.UpdateCollectionCommand;
using markit.Application.Features.Collections.Queries.ViewModels;
using markit.Application.Features.Notebooks.Commands.CreateNotebookCommand;
using markit.Application.Features.Notebooks.Commands.UpdateNotebookCommand;
using markit.Application.Features.Notebooks.Queries.ViewModels;
using markit.Application.Helpers;
using markit.Application.Models.Authentication;
using markit.Application.Models.Authentication.Enums;
using markit.Domain.Entities;
using static markit.Application.Helpers.GeneralConstant;

namespace markit.Application.Mappings
{
    public class MappingProfile : BaseProfile
    {
        public MappingProfile()
        {
            #region Accounts
            CreateBoundedMap<ExternalUser, CreateAccountCommand>()
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
                )
                .ForMember(
                    dest => dest.UserName,
                    opt => opt.MapFrom(src => src.Email)
                );
            #endregion

            #region Collections
            CreateBoundedMap<CreateCollectionCommand, Collection>();
            CreateBoundedMap<UpdateCollectionCommand, Collection>();
            CreateBoundedMap<Collection, CollectionViewModel>()
                .ForMember(dest => dest.Path, opt => opt.Ignore())
                .ForMember(dest => dest.CollectionItems, opt => opt.Ignore());

            CreateBoundedMap<Collection, CollectionItem>()
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
                    opt => opt.MapFrom(src => src.Notebooks != null 
                    ? $"{src.Notebooks.Count} notebooks" 
                    : GeneralConstant.Notebooks.COLLECTION_DEFAULT_PREVIEW)
                );
            #endregion

            #region Blocks
            CreateBoundedMap<BlockViewModel, Block>();
            CreateBoundedMap<Block, BlockViewModel>();
            #endregion

            #region Notebooks
            CreateBoundedMap<CreateNotebookCommand, Notebook>()
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

            CreateBoundedMap<UpdateNotebookCommand, Notebook>()
                .ForMember(
                    dest => dest.NameLess,
                    opt  => opt.MapFrom(src => string.IsNullOrWhiteSpace(src.InputName))
                )
                .ForMember(
                    dest => dest.Name,
                    opt => opt.MapFrom(
                        src => string.IsNullOrWhiteSpace(src.InputName) 
                            ? GeneralConstant.Notebooks.NOTEBOOK_PLACEHOLDER 
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

            CreateBoundedMap<Notebook, NotebookViewModel>()
                .ForMember(
                    dest => dest.Blocks,
                    opt => opt.MapFrom(
                        src => src.Blocks != null
                        ? src.Blocks.OrderBy(b => b.Order).ToList()
                        : new List<Block>()
                    )
                )
                .ForMember(
                    dest => dest.UserId,
                    opt => opt.MapFrom(src => src.Collection != null
                        ? src.Collection.UserId
                        : null
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

            CreateBoundedMap<Notebook, CollectionItem>()
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
                    opt => opt.MapFrom(src => CollectionItemType.Notebook)
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
                    opt => opt.MapFrom(src => src.ConstructPreview())
                );
            #endregion
        }
    }
}
