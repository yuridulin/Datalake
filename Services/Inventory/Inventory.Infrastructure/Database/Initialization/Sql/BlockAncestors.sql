-- �������� ������������������ ������������� ��� �������� ������
CREATE MATERIALIZED VIEW public."BlockAncestors" AS
WITH RECURSIVE BlockHierarchy AS (
    -- ������� ������: �������� � ������� �����
    SELECT 
        "Id",
        "ParentId",
        ARRAY["Id"] as "AncestorIds", -- ������ ���������� � �������� ID
        1 as "Depth"
    FROM public."Blocks"
    WHERE "IsDeleted" = false
    
    UNION ALL
    
    -- ����������� ������: ����������� � ���������
    SELECT 
        bh."Id",
        b."ParentId",
        b."Id" || bh."AncestorIds", -- ��������� �������� � ������ �������
        bh."Depth" + 1
    FROM BlockHierarchy bh
    INNER JOIN public."Blocks" b ON b."Id" = bh."ParentId"
    WHERE 
        b."IsDeleted" = false
        AND bh."ParentId" IS NOT NULL
)
SELECT 
    "Id" as "BlockId",
    "AncestorIds" as "AncestorIds" -- ������ �������� [���_����, ��������, �������, ...]
FROM BlockHierarchy
WHERE "ParentId" IS NULL -- ����� ������ ������ ������� �� �����
ORDER BY "Id";

-- �������� �������� ��� �������� ������
CREATE UNIQUE INDEX "IX_BlockAncestors_BlockId" 
ON public."BlockAncestors" ("BlockId");

-- ������ ��� ������ �� ������� (GIN ������ ��� ������ � ���������)
CREATE INDEX "IX_BlockAncestors_AncestorIds" 
ON public."BlockAncestors" USING GIN ("AncestorIds");

-- ������ ��� ������ ����������� ������ � �������
CREATE INDEX "IX_BlockAncestors_AncestorIds_Contains" 
ON public."BlockAncestors" USING GIN ("AncestorIds" array_ops);

-- ������� ���������� ������������� ��� ���������
CREATE OR REPLACE FUNCTION public.refresh_block_ancestors()
RETURNS trigger
LANGUAGE plpgsql
AS $$
BEGIN
    REFRESH MATERIALIZED VIEW CONCURRENTLY public."BlockAncestors";
    RETURN NULL;
END;
$$;

-- ������� ���������� ��� ������� �����
CREATE TRIGGER trigger_refresh_block_ancestors_insert
    AFTER INSERT ON public."Blocks"
    FOR EACH STATEMENT
    EXECUTE FUNCTION public.refresh_block_ancestors();
    
-- ������� ���������� ��� ��������� �����
CREATE TRIGGER trigger_refresh_block_ancestors_update
    AFTER UPDATE ON public."Blocks"
    FOR EACH STATEMENT
    EXECUTE FUNCTION public.refresh_block_ancestors();
    
-- ������� ���������� ��� �������� �����
CREATE TRIGGER trigger_refresh_block_ancestors_delete
    AFTER DELETE ON public."Blocks"
    FOR EACH STATEMENT
    EXECUTE FUNCTION public.refresh_block_ancestors();