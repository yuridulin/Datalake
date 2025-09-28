using Datalake.InventoryService.Application.Interfaces;
using Datalake.PrivateApi.Entities;
using Datalake.PublicApi.Enums;
using static Datalake.InventoryService.Application.Features.Blocks.Commands.UpdateBlockTags.UpdateBlockTagsCommand;

namespace Datalake.InventoryService.Application.Features.Blocks.Commands.UpdateBlockTags;

public record UpdateBlockTagsCommand(
	UserAccessEntity User,
	int BlockId,
	BlockTagDto[] Tags) : ICommand
{
	public record BlockTagDto(
		int TagId,
		string LocalName,
		BlockTagRelation Relation = BlockTagRelation.Static);
}
