using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datalake.Inventory.Infrastructure.Database.Migrations
{
	/// <inheritdoc />
	public partial class MigrateDataFromPublic : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Sql(@"-- Users
				DO $$
				BEGIN
					IF EXISTS (
						SELECT FROM information_schema.TABLES
						WHERE table_schema = 'public'
						AND table_name = 'Users'
					)
					THEN
						INSERT INTO inventory.""Users""
							(""Guid"", ""Type"", ""IsDeleted"", ""Email"", ""Login"", ""PasswordHash"", ""FullName"")
						SELECT
							CASE WHEN ""Type"" = 3 THEN ""EnergoIdGuid"" ELSE ""Guid"" END AS ""Guid"",
							CASE WHEN ""Type"" = 1 THEN 'Local' WHEN ""Type"" = 3 THEN 'EnergoId' END AS ""Type"",
							""IsDeleted"",
							CASE WHEN ""Type"" = 3 THEN ""Login"" ELSE NULL END AS ""Email"",
							CASE WHEN ""Type"" = 1 THEN ""Login"" ELSE NULL END AS ""Login"",
							CASE WHEN ""Type"" = 1 THEN ""PasswordHash"" ELSE NULL END AS ""PasswordHash"",
							""FullName""
						FROM public.""Users""
						WHERE ""Type"" <> 2;
					END IF;
				END $$;", suppressTransaction: true);

			migrationBuilder.Sql(@"-- UserGroups
				DO $$
				BEGIN
					IF EXISTS (
						SELECT FROM information_schema.TABLES
						WHERE table_schema = 'public'
						AND table_name = 'UserGroups'
					)
					THEN
						INSERT INTO inventory.""UserGroups""
							(""Guid"", ""ParentGuid"", ""Name"", ""Description"", ""IsDeleted"")
						SELECT
							""Guid"", ""ParentGuid"", ""Name"", ""Description"", ""IsDeleted""
						FROM public.""UserGroups"";
					END IF;
				END $$;", suppressTransaction: true);

			migrationBuilder.Sql(@"-- UserGroupRelations
				DO $$
				BEGIN
					IF EXISTS (
						SELECT FROM information_schema.TABLES
						WHERE table_schema = 'public'
						AND table_name = 'UserGroupRelation'
					)
					THEN
						INSERT INTO inventory.""UserGroupRelations""
							(""UserGuid"", ""UserGroupGuid"", ""AccessType"")
						SELECT
							CASE WHEN u.""Type"" = 3 THEN u.""EnergoIdGuid"" ELSE u.""Guid"" END AS ""UserGuid"",
							""UserGroupGuid"",
							""AccessType""
						FROM public.""UserGroupRelation"" ugr
						INNER JOIN public.""Users"" u ON u.""Guid"" = ugr.""UserGuid"";
					END IF;
				END $$;", suppressTransaction: true);

			migrationBuilder.Sql(@"-- Sources
				DO $$
				BEGIN
					IF EXISTS (
						SELECT FROM information_schema.TABLES
						WHERE table_schema = 'public'
						AND table_name = 'Sources'
					)
					THEN
						INSERT INTO inventory.""Sources""
							(""Id"", ""Name"", ""Description"", ""Type"", ""Address"", ""IsDeleted"", ""IsDisabled"")
						SELECT
							""Id"", ""Name"", ""Description"", ""Type"", ""Address"", ""IsDeleted"", ""IsDisabled""
						FROM public.""Sources"";
					END IF;
					PERFORM setval(
							'inventory.""Sources_Id_seq""', 
							(SELECT COALESCE(MAX(""Id""), 0) FROM inventory.""Sources"")
					);
				END $$;", suppressTransaction: true);

			migrationBuilder.Sql(@"-- Blocks
				DO $$
				BEGIN
					IF EXISTS (
						SELECT FROM information_schema.TABLES
						WHERE table_schema = 'public'
						AND table_name = 'Blocks'
					)
					THEN
						INSERT INTO inventory.""Blocks""
							(""Id"", ""GlobalId"", ""ParentId"", ""Name"", ""Description"", ""IsDeleted"")
						SELECT
							""Id"", ""GlobalId"", ""ParentId"", ""Name"", ""Description"", ""IsDeleted""
						FROM public.""Blocks"";
					END IF;
					PERFORM setval(
						'inventory.""Blocks_Id_seq""', 
						(SELECT COALESCE(MAX(""Id""), 0) FROM inventory.""Blocks"")
					);
				END $$;", suppressTransaction: true);

			migrationBuilder.Sql(@"-- Tags
				DO $$
				BEGIN
					IF EXISTS (
						SELECT FROM information_schema.TABLES
						WHERE table_schema = 'public'
						AND table_name = 'Tags'
					)
					THEN
						INSERT INTO inventory.""Tags"" (
							""Id"", ""GlobalGuid"", ""Name"", ""Description"", ""Type"", ""Resolution"", ""Created"", ""IsDeleted"",
							""SourceId"", ""SourceItem"", ""IsScaling"", ""MinEu"", ""MaxEu"", ""MinRaw"", ""MaxRaw"",
							""Formula"", ""ThresholdSourceTagId"", ""ThresholdSourceTagBlockId"",
							""Aggregation"", ""AggregationPeriod"", ""SourceTagId"", ""SourceTagBlockId""
						)
						SELECT
							""Id"", ""GlobalGuid"", ""Name"", ""Description"", ""Type"", ""Resolution"", ""Created"", ""IsDeleted"",
							""SourceId"", ""SourceItem"", ""IsScaling"", ""MinEu"", ""MaxEu"", ""MinRaw"", ""MaxRaw"",
							""Formula"", ""ThresholdSourceTagId"", ""ThresholdSourceTagBlockId"",
							""Aggregation"", ""AggregationPeriod"", ""SourceTagId"", ""SourceTagBlockId""
						FROM public.""Tags"";
					END IF;
					PERFORM setval(
						'inventory.""Tags_Id_seq""', 
						(SELECT COALESCE(MAX(""Id""), 0) FROM inventory.""Tags"")
					);
				END $$;", suppressTransaction: true);

			migrationBuilder.Sql(@"-- TagsThresholds
				DO $$
				BEGIN
					IF EXISTS (
						SELECT FROM information_schema.TABLES
						WHERE table_schema = 'public'
						AND table_name = 'Tags'
					)
					THEN
						INSERT INTO inventory.""TagThresholds"" (""TagId"", ""InputValue"", ""OutputValue"")
						SELECT
								st.""Id"",
								(elem->>'Threshold')::float4 as ""InputValue"",
								(elem->>'Result')::float4 as ""OutputValue""
						FROM public.""Tags"" st
						CROSS JOIN LATERAL jsonb_array_elements(st.""Thresholds"") AS elem
						WHERE st.""Calculation"" = 2;
					END IF;
				END $$;", suppressTransaction: true);

			migrationBuilder.Sql(@"-- TagsInputs
				DO $$
				BEGIN
					IF EXISTS (
						SELECT FROM information_schema.TABLES
						WHERE table_schema = 'public'
						AND table_name = 'TagsInputs'
					)
					THEN
						INSERT INTO inventory.""TagsInputs"" (""TagId"", ""InputTagId"", ""InputBlockId"", ""VariableName"")
						SELECT
								""TagId"", ""InputTagId"", ""InputBlockId"", ""VariableName""
						FROM public.""TagsInputs"";
					END IF;
				END $$;", suppressTransaction: true);

			migrationBuilder.Sql(@"-- BlockTags
				DO $$
				BEGIN
					IF EXISTS (
						SELECT FROM information_schema.TABLES
						WHERE table_schema = 'public'
						AND table_name = 'BlockTags'
					)
					THEN
						INSERT INTO inventory.""BlockTags""
							(""BlockId"", ""TagId"", ""Name"", ""Relation"")
						SELECT DISTINCT ON (""BlockId"", ""TagId"")
							""BlockId"", ""TagId"", ""Name"", ""Relation""
						FROM public.""BlockTags"";
					END IF;
				END $$;", suppressTransaction: true);

			migrationBuilder.Sql(@"-- AccessRules
				DO $$
				BEGIN
					IF EXISTS (
						SELECT FROM information_schema.TABLES
						WHERE table_schema = 'public'
						AND table_name = 'AccessRights'
					) THEN
						INSERT INTO inventory.""AccessRules""
							(""UserGuid"", ""UserGroupGuid"", ""IsGlobal"", ""TagId"", ""SourceId"", ""BlockId"", ""AccessType"")
						SELECT
							CASE WHEN u.""Type"" = 3 THEN u.""EnergoIdGuid"" ELSE u.""Guid"" END AS ""UserGuid"",
							""UserGroupGuid"",
							""IsGlobal"",
							""TagId"",
							""SourceId"",
							""BlockId"",
							""AccessType""
						FROM public.""AccessRights"" ar
						INNER JOIN public.""Users"" u ON u.""Guid"" = ar.""UserGuid"" WHERE u.""Type"" <> 2;
					END IF;
				END $$;", suppressTransaction: true);

			migrationBuilder.Sql(@"-- Logs
				DO $$
				BEGIN
					IF EXISTS (
						SELECT FROM information_schema.TABLES
						WHERE table_schema = 'public'
						AND table_name = 'Logs'
					)
					THEN
						INSERT INTO inventory.""Logs""
							(""Date"", ""Category"", ""AffectedSourceId"", ""AffectedTagId"", ""AffectedBlockId"", ""AffectedUserGuid"", ""AffectedUserGroupGuid"", ""AffectedAccessRightsId"",
							""RefId"", ""AuthorGuid"", ""Type"", ""Text"", ""Details"")
						SELECT
							""Date"", ""Category"", ""AffectedSourceId"", ""AffectedTagId"", ""AffectedBlockId"",
							CASE WHEN u.""Type"" = 3 THEN u.""EnergoIdGuid"" ELSE u.""Guid"" END AS ""AffectedUserGuid"",
							""AffectedUserGroupGuid"", ""AffectedAccessRightsId"",
							""RefId"",
							CASE WHEN u2.""Type"" = 3 THEN u2.""EnergoIdGuid"" ELSE u2.""Guid"" END AS ""AuthorGuid"",
							l.""Type"", ""Text"", ""Details""
						FROM public.""Logs"" l
						INNER JOIN public.""Users"" u ON u.""Guid"" = l.""AffectedUserGuid"" AND u.""Type"" <> 2
						INNER JOIN public.""Users"" u2 ON u2.""Guid"" = l.""AuthorGuid"" AND u2.""Type"" <> 2;
					END IF;
				END $$;", suppressTransaction: true);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{

		}
	}
}
