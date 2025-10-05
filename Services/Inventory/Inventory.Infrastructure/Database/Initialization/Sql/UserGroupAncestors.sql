-- �������� ������������������ ������������� ��� �������� ����� �������������
CREATE MATERIALIZED VIEW public."UserGroupAncestors" AS
WITH RECURSIVE GroupHierarchy AS (
    -- ������� ������: �������� � ������ ������
    SELECT 
        "Guid",
        "ParentGuid",
        ARRAY["Guid"] as "AncestorGuids", -- ������ ���������� � �������� GUID
        1 as "Depth"
    FROM public."UserGroups"
    WHERE "IsDeleted" = false
    
    UNION ALL
    
    -- ����������� ������: ����������� � ���������
    SELECT 
        gh."Guid",
        ug."ParentGuid",
        ug."Guid" || gh."AncestorGuids", -- ��������� �������� � ������ �������
        gh."Depth" + 1
    FROM GroupHierarchy gh
    INNER JOIN public."UserGroups" ug ON ug."Guid" = gh."ParentGuid"
    WHERE 
        ug."IsDeleted" = false
        AND gh."ParentGuid" IS NOT NULL
)
SELECT 
    "Guid" as "GroupGuid",
    "AncestorGuids" as "AncestorGuids" -- ������ �������� [����_������, ��������, �������, ...]
FROM GroupHierarchy
WHERE "ParentGuid" IS NULL -- ����� ������ ������ ������� �� �����
ORDER BY "Guid";

-- �������� �������� ��� �������� ������
CREATE UNIQUE INDEX "IX_UserGroupAncestors_GroupGuid" 
ON public."UserGroupAncestors" ("GroupGuid");

-- GIN ������ ��� ������ � ��������� UUID
CREATE INDEX "IX_UserGroupAncestors_AncestorGuids" 
ON public."UserGroupAncestors" USING GIN ("AncestorGuids");

-- ������� ��� ���������� ������������������ ������������� �����
CREATE OR REPLACE FUNCTION public.refresh_user_group_ancestors()
RETURNS trigger
LANGUAGE plpgsql
AS $$
BEGIN
    REFRESH MATERIALIZED VIEW CONCURRENTLY public."UserGroupAncestors";
    RETURN NULL;
END;
$$;

-- ������� �������� �� ������� UserGroups
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