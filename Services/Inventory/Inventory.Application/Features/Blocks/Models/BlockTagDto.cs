using Datalake.Contracts.Public.Enums;

namespace Datalake.Inventory.Application.Features.Blocks.Models;

public record BlockTagDto(
	int TagId,
	string LocalName,
	BlockTagRelation Relation = BlockTagRelation.Static);