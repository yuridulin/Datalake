using Datalake.PublicApi.Enums;

namespace Datalake.InventoryService.Application.Features.Blocks.Models;

public record BlockTagDto(
	int TagId,
	string LocalName,
	BlockTagRelation Relation = BlockTagRelation.Static);