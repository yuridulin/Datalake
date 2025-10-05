-- ћатериализованное представление действующих на пользователей групп
CREATE MATERIALIZED VIEW public."UserEffectiveGroups" AS
SELECT DISTINCT
    ugr."UserGuid",
    unnest(uga."AncestorGuids") AS "GroupGuid"
FROM public."UserGroupRelations" ugr
JOIN public."Users" u ON u."Guid" = ugr."UserGuid"
JOIN public."UserGroupAncestors" uga ON uga."GroupGuid" = ugr."UserGroupGuid"
WHERE u."IsDeleted" = false;
 
-- —оздаем индексы дл€ быстрого доступа
CREATE UNIQUE INDEX "IX_UserEffectiveGroups_UserGuid_GroupGuid" 
ON public."UserEffectiveGroups" ("UserGuid", "GroupGuid");

CREATE INDEX "IX_UserEffectiveGroups_GroupGuid" 
ON public."UserEffectiveGroups" ("GroupGuid");