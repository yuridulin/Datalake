using Datalake.Contracts.Models.Blocks;
using Datalake.Contracts.Models.Sources;
using Datalake.Contracts.Models.Tags;
using Datalake.Contracts.Models.UserGroups;
using Datalake.Contracts.Models.Users;
using Datalake.Domain.Entities;
using Datalake.Domain.Enums;
using LinqToDB;

namespace Datalake.Inventory.Infrastructure.Database.Extensions;

public static class SimpleInfoQueryExtensions
{
	extension(ITable<Block> blocks)
	{
		public IQueryable<BlockSimpleInfo> AsSimpleInfo() =>
			from block in blocks
			select new BlockSimpleInfo
			{
				Id = block.Id,
				Guid = block.GlobalId,
				Name = block.Name,
				Description = block.Description,
				ParentBlockId = block.ParentId,
			};
	}

	extension(ITable<Source> sources)
	{
		public IQueryable<SourceSimpleInfo> AsSimpleInfo() =>
			from source in sources
			select new SourceSimpleInfo
			{
				Id = source.Id,
				Name = source.Name,
			};
	}

	extension(ITable<Tag> tags)
	{
		public IQueryable<TagSimpleInfo> AsSimpleInfo(ITable<Source> sources) =>
			from tag in tags
			from source in sources.InnerJoin(x => x.Id == tag.SourceId)
			select new TagSimpleInfo
			{
				Id = tag.Id,
				Guid = tag.GlobalGuid,
				Name = tag.Name,
				Resolution = tag.Resolution,
				Type = tag.Type,
				SourceType = source.Type,
			};
	}

	extension(ITable<User> users)
	{
		public IQueryable<UserSimpleInfo> AsSimpleInfo(ITable<EnergoId> energoId) =>
			from user in users
			from energo in energoId.LeftJoin(x => x.Guid == user.Guid)
			select new UserSimpleInfo
			{
				Guid = user.Guid,
				Type = user.Type,
				FullName = user.Type == UserType.EnergoId
					? (energo == null ? "Имя не найдено в EnergoId" : $"{energo.LastName} {energo.FirstName} {energo.MiddleName}")
					: (user.FullName ?? user.Login ?? "Имя не найдено в системе")
			};
	}

	extension(ITable<UserGroup> userGroups)
	{
		public IQueryable<UserGroupSimpleInfo> AsSimpleInfo() =>
			from userGroup in userGroups
			select new UserGroupSimpleInfo
			{
				Guid = userGroup.Guid,
				Name = userGroup.Name,
			};
	}
}
