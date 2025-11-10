using Datalake.Contracts.Public.Enums;
using Datalake.Data.Application.Interfaces.Repositories;
using Datalake.Data.Application.Models.Sources;
using Datalake.Data.Application.Models.Tags;
using Datalake.Data.Infrastructure.Database;
using Datalake.Shared.Application.Attributes;
using Microsoft.EntityFrameworkCore;

namespace Datalake.Data.Infrastructure.Database.Repositories;

[Scoped]
public class SourcesSettingsRepository(DataDbContext context) : ISourcesSettingsRepository
{
	public async Task<IEnumerable<SourceSettingsDto>> GetAllAsync(CancellationToken cancellationToken)
	{
		return await context.Sources
			.Include(x => x.Tags).ThenInclude(x => x.Inputs).ThenInclude(x => x.InputTag)
			.Include(x => x.Tags).ThenInclude(x => x.Thresholds)
			.Include(x => x.Tags).ThenInclude(x => x.ThresholdsSourceTag)
			.Include(x => x.Tags).ThenInclude(x => x.AggregationSourceTag)
			.AsSplitQuery()
			.Where(x => !x.IsDeleted)
			.AsNoTracking()
			.Select(source => new SourceSettingsDto
			{
				SourceId = source.Id,
				SourceName = source.Name,
				SourceType = source.Type,
				IsDisabled = source.IsDisabled,
				RemoteSettings = source.Type != SourceType.Inopc ? null :
					new SourceRemoteSettingsDto
					{
						RemoteHost = source.Address ?? string.Empty,
						RemotePort = 81, // TODO: временная заглушка
					},
				Tags = source.Tags.Select(tag => new TagSettingsDto
				{
					TagId = tag.Id,
					TagGuid = tag.Guid,
					TagType = tag.Type,
					TagResolution = tag.Resolution,
					TagName = tag.Name,
					IsDeleted = tag.IsDeleted,
					SourceId = source.Id,
					SourceType = source.Type,
					ScaleSettings = !tag.IsScaling ? null : new TagScaleSettings
					{
						MinEu = tag.MinEu,
						MaxEu = tag.MaxEu,
						MaxRaw = tag.MaxRaw,
						MinRaw = tag.MinRaw,
					},
					InopcSettings = source.Type != SourceType.Inopc ? null : new TagInopcSettingsDto
					{
						RemoteItem = tag.SourceItem ?? string.Empty,
					},
					AggregationSettings =
						source.Type != SourceType.Aggregated ? null :
						tag.AggregationSourceTag == null ? null :
						tag.Aggregation == null ? null :
						tag.AggregationPeriod == null ? null :
						new TagAggregationSettingsDto
						{
							SourceTagId = tag.AggregationSourceTag.Id,
							SourceTagType = tag.AggregationSourceTag.Type,
							AggregateFunction = tag.Aggregation.Value,
							AggregatePeriod = tag.AggregationPeriod.Value,
						},
					CalculationSettings =
						source.Type != SourceType.Calculated ? null :
						new TagCalculationSettingsDto
						{
							ExpressionFormula = tag.Formula ?? string.Empty,
							ExpressionVariables = tag.Inputs.Select(input => new TagCalculationSettingsDto.TagCalculationInputDto
							{
								SourceTagId = input.InputTag != null ? input.InputTag.Id : 0,
								SourceTagType = input.InputTag != null ? input.InputTag.Type : TagType.String,
								VariableName = input.VariableName,
							})
						},
					ThresholdsSettings =
						source.Type != SourceType.Thresholds ? null :
						tag.ThresholdsSourceTag == null ? null :
						new TagThresholdsSettingsDto
						{
							SourceTagId = tag.ThresholdsSourceTag.Id,
							SourceTagType = tag.ThresholdsSourceTag.Type,
							Thresholds = tag.Thresholds.Select(threshold => new TagThresholdsSettingsDto.TagThresholdDto
							{
								InputValue = threshold.InputValue,
								OutputValue = threshold.OutputValue,
							})
						}
				})
			})
			.ToArrayAsync(cancellationToken);
	}
}
