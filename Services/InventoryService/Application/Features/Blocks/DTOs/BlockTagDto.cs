using Datalake.PublicApi.Enums;

namespace Datalake.InventoryService.Application.Features.Blocks.DTOs;

public record BlockTagDto(
	int TagId,
	string LocalName,
	BlockTagRelation Relation = BlockTagRelation.Static);