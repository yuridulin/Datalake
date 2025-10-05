-- Создание материализованного представления для иерархии групп пользователей
CREATE MATERIALIZED VIEW public."UserGroupAncestors" AS
WITH RECURSIVE GroupHierarchy AS (
    -- Базовый случай: начинаем с каждой группы
    SELECT 
        "Guid",
        "ParentGuid",
        ARRAY["Guid"] as "AncestorGuids", -- Массив начинается с текущего GUID
        1 as "Depth"
    FROM public."UserGroups"
    WHERE "IsDeleted" = false
    
    UNION ALL
    
    -- Рекурсивный случай: поднимаемся к родителям
    SELECT 
        gh."Guid",
        ug."ParentGuid",
        ug."Guid" || gh."AncestorGuids", -- Добавляем родителя в начало массива
        gh."Depth" + 1
    FROM GroupHierarchy gh
    INNER JOIN public."UserGroups" ug ON ug."Guid" = gh."ParentGuid"
    WHERE 
        ug."IsDeleted" = false
        AND gh."ParentGuid" IS NOT NULL
)
SELECT 
    "Guid" as "GroupGuid",
    "AncestorGuids" as "AncestorGuids" -- Массив содержит [сама_группа, родитель, дедушка, ...]
FROM GroupHierarchy
WHERE "ParentGuid" IS NULL -- Берем только полные цепочки до корня
ORDER BY "Guid";

-- Создание индексов для быстрого поиска
CREATE UNIQUE INDEX "IX_UserGroupAncestors_GroupGuid" 
ON public."UserGroupAncestors" ("GroupGuid");

-- GIN индекс для работы с массивами UUID
CREATE INDEX "IX_UserGroupAncestors_AncestorGuids" 
ON public."UserGroupAncestors" USING GIN ("AncestorGuids");

-- Функция для обновления материализованного представления групп
CREATE OR REPLACE FUNCTION public.refresh_user_group_ancestors()
RETURNS trigger
LANGUAGE plpgsql
AS $$
BEGIN
    REFRESH MATERIALIZED VIEW CONCURRENTLY public."UserGroupAncestors";
    RETURN NULL;
END;
$$;

-- Создаем триггеры на таблице UserGroups
CREATE TRIGGER trigger_refresh_user_group_ancestors_insert
    AFTER INSERT ON public."UserGroups"
    FOR EACH STATEMENT
    EXECUTE FUNCTION public.refresh_user_group_ancestors();

CREATE TRIGGER trigger_refresh_user_group_ancestors_update
    AFTER UPDATE ON public."UserGroups"
    FOR EACH STATEMENT
    EXECUTE FUNCTION public.refresh_user_group_ancestors();

CREATE TRIGGER trigger_refresh_user_group_ancestors_delete
    AFTER DELETE ON public."UserGroups"
    FOR EACH STATEMENT
    EXECUTE FUNCTION public.refresh_user_group_ancestors();