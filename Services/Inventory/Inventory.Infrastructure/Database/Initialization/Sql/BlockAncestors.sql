-- Создание материализованного представления для иерархии блоков
CREATE MATERIALIZED VIEW public."BlockAncestors" AS
WITH RECURSIVE BlockHierarchy AS (
    -- Базовый случай: начинаем с каждого блока
    SELECT 
        "Id",
        "ParentId",
        ARRAY["Id"] as "AncestorIds", -- Массив начинается с текущего ID
        1 as "Depth"
    FROM public."Blocks"
    WHERE "IsDeleted" = false
    
    UNION ALL
    
    -- Рекурсивный случай: поднимаемся к родителям
    SELECT 
        bh."Id",
        b."ParentId",
        b."Id" || bh."AncestorIds", -- Добавляем родителя в начало массива
        bh."Depth" + 1
    FROM BlockHierarchy bh
    INNER JOIN public."Blocks" b ON b."Id" = bh."ParentId"
    WHERE 
        b."IsDeleted" = false
        AND bh."ParentId" IS NOT NULL
)
SELECT 
    "Id" as "BlockId",
    "AncestorIds" as "AncestorIds" -- Массив содержит [сам_блок, родитель, дедушка, ...]
FROM BlockHierarchy
WHERE "ParentId" IS NULL -- Берем только полные цепочки до корня
ORDER BY "Id";

-- Создание индексов для быстрого поиска
CREATE UNIQUE INDEX "IX_BlockAncestors_BlockId" 
ON public."BlockAncestors" ("BlockId");

-- Индекс для поиска по предкам (GIN индекс для работы с массивами)
CREATE INDEX "IX_BlockAncestors_AncestorIds" 
ON public."BlockAncestors" USING GIN ("AncestorIds");

-- Индекс для поиска конкретного предка в массиве
CREATE INDEX "IX_BlockAncestors_AncestorIds_Contains" 
ON public."BlockAncestors" USING GIN ("AncestorIds" array_ops);

-- Функция обновления представления для триггеров
CREATE OR REPLACE FUNCTION public.refresh_block_ancestors()
RETURNS trigger
LANGUAGE plpgsql
AS $$
BEGIN
    REFRESH MATERIALIZED VIEW CONCURRENTLY public."BlockAncestors";
    RETURN NULL;
END;
$$;

-- Триггер обновления при вставке блока
CREATE TRIGGER trigger_refresh_block_ancestors_insert
    AFTER INSERT ON public."Blocks"
    FOR EACH STATEMENT
    EXECUTE FUNCTION public.refresh_block_ancestors();
    
-- Триггер обновления при изменении блока
CREATE TRIGGER trigger_refresh_block_ancestors_update
    AFTER UPDATE ON public."Blocks"
    FOR EACH STATEMENT
    EXECUTE FUNCTION public.refresh_block_ancestors();
    
-- Триггер обновления при удалении блока
CREATE TRIGGER trigger_refresh_block_ancestors_delete
    AFTER DELETE ON public."Blocks"
    FOR EACH STATEMENT
    EXECUTE FUNCTION public.refresh_block_ancestors();