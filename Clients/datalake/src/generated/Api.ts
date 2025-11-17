/* eslint-disable */
/* tslint:disable */
// @ts-nocheck
/*
 * ---------------------------------------------------------------
 * ## THIS FILE WAS GENERATED VIA SWAGGER-TYPESCRIPT-API        ##
 * ##                                                           ##
 * ## AUTHOR: acacode                                           ##
 * ## SOURCE: https://github.com/acacode/swagger-typescript-api ##
 * ---------------------------------------------------------------
 */

import {
  AccessRightsInfo,
  AuthEnergoIdRequest,
  AuthLoginPassRequest,
  BlockCreateRequest,
  BlockFullInfo,
  BlockTreeInfo,
  BlockUpdateRequest,
  BlockWithTagsInfo,
  DataSourcesGetActivityPayload,
  DataValuesGetPayload,
  DataValuesWritePayload,
  InventoryAccessGetCalculatedAccessPayload,
  InventoryAccessSetBlockRulesPayload,
  InventoryAccessSetSourceRulesPayload,
  InventoryAccessSetTagRulesPayload,
  InventoryAccessSetUserGroupRulesPayload,
  InventoryAccessSetUserRulesPayload,
  LogCategory,
  LogInfo,
  LogType,
  SettingsInfo,
  SourceActivityInfo,
  SourceInfo,
  SourceItemInfo,
  SourceUpdateRequest,
  TagCreateRequest,
  TagFullInfo,
  TagInfo,
  TagMetricRequest,
  TagUpdateRequest,
  UserAccessValue,
  UserCreateRequest,
  UserEnergoIdInfo,
  UserGroupCreateRequest,
  UserGroupDetailedInfo,
  UserGroupInfo,
  UserGroupTreeInfo,
  UserGroupUpdateRequest,
  UserInfo,
  UserSessionWithAccessInfo,
  UsersGetActivityPayload,
  UserUpdateRequest,
  ValuesResponse,
  ValuesTagResponse,
} from "./data-contracts";
import { ContentType, HttpClient, RequestParams } from "./http-client";

export class Api<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  /**
   * No description
   *
   * @tags InventoryAccess
   * @name InventoryAccessGet
   * @summary Получение списка прямых (не глобальных) разрешений субъекта на объект
   * @request GET:/api/v1/inventory/access
   * @response `200` `(AccessRightsInfo)[]` Список разрешений
   */
  inventoryAccessGet = (
    query?: {
      /**
       * Идентификтатор пользователя
       * @format guid
       */
      user?: string | null;
      /**
       * Идентификатор группы пользователей
       * @format guid
       */
      userGroup?: string | null;
      /**
       * Идентификатор источника
       * @format int32
       */
      source?: number | null;
      /**
       * Идентификатор блока
       * @format int32
       */
      block?: number | null;
      /**
       * Идентификатор тега
       * @format int32
       */
      tag?: number | null;
    },
    params: RequestParams = {},
  ) =>
    this.request<AccessRightsInfo[], any>({
      path: `/api/v1/inventory/access`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags InventoryAccess
   * @name InventoryAccessGetCalculatedAccess
   * @summary Получение списка рассчитанных разрешений субъекта на объект для всех субъетов и всех объектов
   * @request POST:/api/v1/inventory/access/calculated
   * @response `200` `Record<string,UserAccessValue>`
   */
  inventoryAccessGetCalculatedAccess = (
    data: InventoryAccessGetCalculatedAccessPayload,
    params: RequestParams = {},
  ) =>
    this.request<Record<string, UserAccessValue>, any>({
      path: `/api/v1/inventory/access/calculated`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags InventoryAccess
   * @name InventoryAccessSetBlockRules
   * @summary Изменение разрешений для блок
   * @request PUT:/api/v1/inventory/access/block/{blockId}
   * @response `200` `File`
   */
  inventoryAccessSetBlockRules = (
    blockId: number,
    data: InventoryAccessSetBlockRulesPayload,
    params: RequestParams = {},
  ) =>
    this.request<File, any>({
      path: `/api/v1/inventory/access/block/${blockId}`,
      method: "PUT",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags InventoryAccess
   * @name InventoryAccessSetSourceRules
   * @summary Изменение разрешений на источник данных
   * @request PUT:/api/v1/inventory/access/source/{sourceId}
   * @response `200` `File`
   */
  inventoryAccessSetSourceRules = (
    sourceId: number,
    data: InventoryAccessSetSourceRulesPayload,
    params: RequestParams = {},
  ) =>
    this.request<File, any>({
      path: `/api/v1/inventory/access/source/${sourceId}`,
      method: "PUT",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags InventoryAccess
   * @name InventoryAccessSetTagRules
   * @summary Изменение разрешений для тега
   * @request PUT:/api/v1/inventory/access/tag/{tagId}
   * @response `200` `File`
   */
  inventoryAccessSetTagRules = (
    tagId: number,
    data: InventoryAccessSetTagRulesPayload,
    params: RequestParams = {},
  ) =>
    this.request<File, any>({
      path: `/api/v1/inventory/access/tag/${tagId}`,
      method: "PUT",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags InventoryAccess
   * @name InventoryAccessSetUserGroupRules
   * @summary Изменение разрешений для группы учетных записей
   * @request PUT:/api/v1/inventory/access/user-group/{userGroupGuid}
   * @response `200` `File`
   */
  inventoryAccessSetUserGroupRules = (
    userGroupGuid: string,
    data: InventoryAccessSetUserGroupRulesPayload,
    params: RequestParams = {},
  ) =>
    this.request<File, any>({
      path: `/api/v1/inventory/access/user-group/${userGroupGuid}`,
      method: "PUT",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags InventoryAccess
   * @name InventoryAccessSetUserRules
   * @summary Изменение разрешений для учетной записи
   * @request PUT:/api/v1/inventory/access/user/{userGuid}
   * @response `200` `File`
   */
  inventoryAccessSetUserRules = (
    userGuid: string,
    data: InventoryAccessSetUserRulesPayload,
    params: RequestParams = {},
  ) =>
    this.request<File, any>({
      path: `/api/v1/inventory/access/user/${userGuid}`,
      method: "PUT",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags InventoryAudit
   * @name InventoryAuditGet
   * @summary Получение списка сообщений аудита
   * @request GET:/api/v1/inventory/audit
   * @response `200` `(LogInfo)[]` Список сообщений аудита
   */
  inventoryAuditGet = (
    query?: {
      /**
       * Идентификатор последнего сообщения. Будут присланы только более поздние
       * @format int32
       */
      lastId?: number | null;
      /**
       * Идентификатор первого сообщения. Будут присланы только более ранние
       * @format int32
       */
      firstId?: number | null;
      /** @format int32 */
      take?: number | null;
      /** @format int32 */
      source?: number | null;
      /** @format int32 */
      block?: number | null;
      /** @format guid */
      tag?: string | null;
      /** @format guid */
      user?: string | null;
      /** @format guid */
      group?: string | null;
      "categories[]"?: LogCategory[] | null;
      "types[]"?: LogType[] | null;
      /** @format guid */
      author?: string | null;
    },
    params: RequestParams = {},
  ) =>
    this.request<LogInfo[], any>({
      path: `/api/v1/inventory/audit`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags InventoryBlocks
   * @name InventoryBlocksCreate
   * @summary Создание нового блока на основании переданной информации
   * @request POST:/api/v1/inventory/blocks
   * @response `200` `number` Идентификатор блока
   */
  inventoryBlocksCreate = (
    data: BlockCreateRequest,
    query?: {
      /**
       * Идентификатор родительского блока
       * @format int32
       */
      parentId?: number | null;
    },
    params: RequestParams = {},
  ) =>
    this.request<number, any>({
      path: `/api/v1/inventory/blocks`,
      method: "POST",
      query: query,
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags InventoryBlocks
   * @name InventoryBlocksGetAll
   * @summary Получение списка блоков с базовой информацией о них
   * @request GET:/api/v1/inventory/blocks
   * @response `200` `(BlockWithTagsInfo)[]` Список блоков
   */
  inventoryBlocksGetAll = (params: RequestParams = {}) =>
    this.request<BlockWithTagsInfo[], any>({
      path: `/api/v1/inventory/blocks`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags InventoryBlocks
   * @name InventoryBlocksGet
   * @summary Получение информации о выбранном блоке
   * @request GET:/api/v1/inventory/blocks/{blockId}
   * @response `200` `BlockFullInfo` Информация о блоке
   */
  inventoryBlocksGet = (blockId: number, params: RequestParams = {}) =>
    this.request<BlockFullInfo, any>({
      path: `/api/v1/inventory/blocks/${blockId}`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags InventoryBlocks
   * @name InventoryBlocksUpdate
   * @summary Изменение блока
   * @request PUT:/api/v1/inventory/blocks/{blockId}
   * @response `200` `File`
   */
  inventoryBlocksUpdate = (
    blockId: number,
    data: BlockUpdateRequest,
    params: RequestParams = {},
  ) =>
    this.request<File, any>({
      path: `/api/v1/inventory/blocks/${blockId}`,
      method: "PUT",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags InventoryBlocks
   * @name InventoryBlocksDelete
   * @summary Удаление блока
   * @request DELETE:/api/v1/inventory/blocks/{blockId}
   * @response `200` `File`
   */
  inventoryBlocksDelete = (blockId: number, params: RequestParams = {}) =>
    this.request<File, any>({
      path: `/api/v1/inventory/blocks/${blockId}`,
      method: "DELETE",
      ...params,
    });
  /**
   * No description
   *
   * @tags InventoryBlocks
   * @name InventoryBlocksGetTree
   * @summary Получение иерархической структуры всех блоков
   * @request GET:/api/v1/inventory/blocks/tree
   * @response `200` `(BlockTreeInfo)[]` Список обособленных блоков с вложенными блоками
   */
  inventoryBlocksGetTree = (params: RequestParams = {}) =>
    this.request<BlockTreeInfo[], any>({
      path: `/api/v1/inventory/blocks/tree`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags InventoryBlocks
   * @name InventoryBlocksMove
   * @summary Перемещение блока
   * @request PUT:/api/v1/inventory/blocks/{blockId}/move
   * @response `200` `File`
   */
  inventoryBlocksMove = (
    blockId: number,
    query?: {
      /**
       * Идентификатор родительского блока
       * @format int32
       */
      parentId?: number | null;
    },
    params: RequestParams = {},
  ) =>
    this.request<File, any>({
      path: `/api/v1/inventory/blocks/${blockId}/move`,
      method: "PUT",
      query: query,
      ...params,
    });
  /**
   * No description
   *
   * @tags InventoryEnergoId
   * @name InventoryEnergoIdGetEnergoId
   * @summary Получение списка пользователей, определенных на сервере EnergoId
   * @request GET:/api/v1/inventory/energo-id
   * @response `200` `(UserEnergoIdInfo)[]` Список учетных записей EnergoId с отметкой на каждой, за какой учетной записью приложения закреплена
   */
  inventoryEnergoIdGetEnergoId = (params: RequestParams = {}) =>
    this.request<UserEnergoIdInfo[], any>({
      path: `/api/v1/inventory/energo-id`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags InventoryEnergoId
   * @name InventoryEnergoIdUpdateEnergoId
   * @summary Обновление данных из EnergoId
   * @request PUT:/api/v1/inventory/energo-id
   * @response `200` `File`
   */
  inventoryEnergoIdUpdateEnergoId = (params: RequestParams = {}) =>
    this.request<File, any>({
      path: `/api/v1/inventory/energo-id`,
      method: "PUT",
      ...params,
    });
  /**
   * No description
   *
   * @tags InventorySources
   * @name InventorySourcesCreate
   * @summary Создание источника
   * @request POST:/api/v1/inventory/sources
   * @response `200` `number` Идентификатор источника
   */
  inventorySourcesCreate = (params: RequestParams = {}) =>
    this.request<number, any>({
      path: `/api/v1/inventory/sources`,
      method: "POST",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags InventorySources
   * @name InventorySourcesGetAll
   * @summary Получение списка источников
   * @request GET:/api/v1/inventory/sources
   * @response `200` `(SourceInfo)[]` Список источников
   */
  inventorySourcesGetAll = (
    query?: {
      /**
       * Включить ли в список системные источники
       * @default false
       */
      withCustom?: boolean;
    },
    params: RequestParams = {},
  ) =>
    this.request<SourceInfo[], any>({
      path: `/api/v1/inventory/sources`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags InventorySources
   * @name InventorySourcesGet
   * @summary Получение данных о источнике
   * @request GET:/api/v1/inventory/sources/{sourceId}
   * @response `200` `SourceInfo` Данные о источнике
   */
  inventorySourcesGet = (sourceId: number, params: RequestParams = {}) =>
    this.request<SourceInfo, any>({
      path: `/api/v1/inventory/sources/${sourceId}`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags InventorySources
   * @name InventorySourcesUpdate
   * @summary Изменение источника
   * @request PUT:/api/v1/inventory/sources/{sourceId}
   * @response `200` `File`
   */
  inventorySourcesUpdate = (
    sourceId: number,
    data: SourceUpdateRequest,
    params: RequestParams = {},
  ) =>
    this.request<File, any>({
      path: `/api/v1/inventory/sources/${sourceId}`,
      method: "PUT",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags InventorySources
   * @name InventorySourcesDelete
   * @summary Удаление источника
   * @request DELETE:/api/v1/inventory/sources/{sourceId}
   * @response `200` `File`
   */
  inventorySourcesDelete = (sourceId: number, params: RequestParams = {}) =>
    this.request<File, any>({
      path: `/api/v1/inventory/sources/${sourceId}`,
      method: "DELETE",
      ...params,
    });
  /**
   * No description
   *
   * @tags InventorySystem
   * @name InventorySystemGetSettings
   * @summary Получение информации о настройках сервера
   * @request GET:/api/v1/inventory/system/settings
   * @response `200` `SettingsInfo` Информация о настройках
   */
  inventorySystemGetSettings = (params: RequestParams = {}) =>
    this.request<SettingsInfo, any>({
      path: `/api/v1/inventory/system/settings`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags InventorySystem
   * @name InventorySystemUpdateSettings
   * @summary Изменение информации о настройках сервера
   * @request PUT:/api/v1/inventory/system/settings
   * @response `200` `File`
   */
  inventorySystemUpdateSettings = (
    data: SettingsInfo,
    params: RequestParams = {},
  ) =>
    this.request<File, any>({
      path: `/api/v1/inventory/system/settings`,
      method: "PUT",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags InventorySystem
   * @name InventorySystemRestartState
   * @summary Принудительная перезагрузка состояния БД в кэш
   * @request POST:/api/v1/inventory/system/cache
   * @response `200` `File`
   */
  inventorySystemRestartState = (params: RequestParams = {}) =>
    this.request<File, any>({
      path: `/api/v1/inventory/system/cache`,
      method: "POST",
      ...params,
    });
  /**
   * No description
   *
   * @tags InventoryTags
   * @name InventoryTagsCreate
   * @summary Создание нового тега
   * @request POST:/api/v1/inventory/tags
   * @response `200` `TagInfo` Идентификатор нового тега в локальной базе данных
   */
  inventoryTagsCreate = (data: TagCreateRequest, params: RequestParams = {}) =>
    this.request<TagInfo, any>({
      path: `/api/v1/inventory/tags`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags InventoryTags
   * @name InventoryTagsGetAll
   * @summary Получение списка тегов, включая информацию о источниках и настройках получения данных
   * @request GET:/api/v1/inventory/tags
   * @response `200` `(TagInfo)[]` Плоский список объектов информации о тегах
   */
  inventoryTagsGetAll = (
    query?: {
      /**
       * Идентификатор источника. Если указан, будут выбраны теги только этого источника
       * @format int32
       */
      sourceId?: number | null;
      /** Список локальных идентификаторов тегов */
      tagsId?: number[] | null;
      /** Список глобальных идентификаторов тегов */
      tagsGuid?: string[] | null;
    },
    params: RequestParams = {},
  ) =>
    this.request<TagInfo[], any>({
      path: `/api/v1/inventory/tags`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags InventoryTags
   * @name InventoryTagsGet
   * @summary Получение информации о конкретном теге, включая информацию о источнике и настройках получения данных
   * @request GET:/api/v1/inventory/tags/{tagId}
   * @response `200` `TagFullInfo` Объект информации о теге
   */
  inventoryTagsGet = (tagId: number, params: RequestParams = {}) =>
    this.request<TagFullInfo, any>({
      path: `/api/v1/inventory/tags/${tagId}`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags InventoryTags
   * @name InventoryTagsUpdate
   * @summary Изменение тега
   * @request PUT:/api/v1/inventory/tags/{tagId}
   * @response `200` `File`
   */
  inventoryTagsUpdate = (
    tagId: number,
    data: TagUpdateRequest,
    params: RequestParams = {},
  ) =>
    this.request<File, any>({
      path: `/api/v1/inventory/tags/${tagId}`,
      method: "PUT",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags InventoryTags
   * @name InventoryTagsDelete
   * @summary Удаление тега
   * @request DELETE:/api/v1/inventory/tags/{tagId}
   * @response `200` `File`
   */
  inventoryTagsDelete = (tagId: number, params: RequestParams = {}) =>
    this.request<File, any>({
      path: `/api/v1/inventory/tags/${tagId}`,
      method: "DELETE",
      ...params,
    });
  /**
   * No description
   *
   * @tags InventoryUserGroups
   * @name InventoryUserGroupsCreate
   * @summary Создание новой группы пользователей
   * @request POST:/api/v1/inventory/user-groups
   * @response `200` `string` Идентификатор новой группы пользователей
   */
  inventoryUserGroupsCreate = (
    data: UserGroupCreateRequest,
    params: RequestParams = {},
  ) =>
    this.request<string, any>({
      path: `/api/v1/inventory/user-groups`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags InventoryUserGroups
   * @name InventoryUserGroupsGetAll
   * @summary Получение плоского списка групп пользователей
   * @request GET:/api/v1/inventory/user-groups
   * @response `200` `(UserGroupInfo)[]` Список групп
   */
  inventoryUserGroupsGetAll = (params: RequestParams = {}) =>
    this.request<UserGroupInfo[], any>({
      path: `/api/v1/inventory/user-groups`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags InventoryUserGroups
   * @name InventoryUserGroupsGetTree
   * @summary Получение иерархической структуры всех групп пользователей
   * @request GET:/api/v1/inventory/user-groups/tree
   * @response `200` `(UserGroupTreeInfo)[]` Список обособленных групп с вложенными подгруппами
   */
  inventoryUserGroupsGetTree = (params: RequestParams = {}) =>
    this.request<UserGroupTreeInfo[], any>({
      path: `/api/v1/inventory/user-groups/tree`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags InventoryUserGroups
   * @name InventoryUserGroupsGet
   * @summary Получение информации о выбранной группе пользователей
   * @request GET:/api/v1/inventory/user-groups/{userGroupGuid}
   * @response `200` `UserGroupInfo` Информация о группе
   */
  inventoryUserGroupsGet = (
    userGroupGuid: string,
    params: RequestParams = {},
  ) =>
    this.request<UserGroupInfo, any>({
      path: `/api/v1/inventory/user-groups/${userGroupGuid}`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags InventoryUserGroups
   * @name InventoryUserGroupsUpdate
   * @summary Изменение группы пользователей
   * @request PUT:/api/v1/inventory/user-groups/{userGroupGuid}
   * @response `200` `File`
   */
  inventoryUserGroupsUpdate = (
    userGroupGuid: string,
    data: UserGroupUpdateRequest,
    params: RequestParams = {},
  ) =>
    this.request<File, any>({
      path: `/api/v1/inventory/user-groups/${userGroupGuid}`,
      method: "PUT",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags InventoryUserGroups
   * @name InventoryUserGroupsDelete
   * @summary Удаление группы пользователей
   * @request DELETE:/api/v1/inventory/user-groups/{userGroupGuid}
   * @response `200` `File`
   */
  inventoryUserGroupsDelete = (
    userGroupGuid: string,
    params: RequestParams = {},
  ) =>
    this.request<File, any>({
      path: `/api/v1/inventory/user-groups/${userGroupGuid}`,
      method: "DELETE",
      ...params,
    });
  /**
   * No description
   *
   * @tags InventoryUserGroups
   * @name InventoryUserGroupsGetWithDetails
   * @summary Получение детализированной информации о группе пользователей
   * @request GET:/api/v1/inventory/user-groups/{userGroupGuid}/details
   * @response `200` `UserGroupDetailedInfo` Информация о группе с подгруппами и списком пользователей
   */
  inventoryUserGroupsGetWithDetails = (
    userGroupGuid: string,
    params: RequestParams = {},
  ) =>
    this.request<UserGroupDetailedInfo, any>({
      path: `/api/v1/inventory/user-groups/${userGroupGuid}/details`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags InventoryUserGroups
   * @name InventoryUserGroupsMove
   * @summary Перемещение группы пользователей
   * @request PUT:/api/v1/inventory/user-groups/{groupGuid}/move
   * @response `200` `File`
   */
  inventoryUserGroupsMove = (
    groupGuid: string,
    query?: {
      /**
       * Идентификатор новой родительской группы
       * @format guid
       */
      parentGuid?: string | null;
    },
    params: RequestParams = {},
  ) =>
    this.request<File, any>({
      path: `/api/v1/inventory/user-groups/${groupGuid}/move`,
      method: "PUT",
      query: query,
      ...params,
    });
  /**
   * No description
   *
   * @tags InventoryUsers
   * @name InventoryUsersCreate
   * @summary Создание пользователя на основании переданных данных
   * @request POST:/api/v1/inventory/users
   * @response `200` `string` Идентификатор пользователя
   */
  inventoryUsersCreate = (
    data: UserCreateRequest,
    params: RequestParams = {},
  ) =>
    this.request<string, any>({
      path: `/api/v1/inventory/users`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags InventoryUsers
   * @name InventoryUsersGet
   * @summary Получение списка пользователей
   * @request GET:/api/v1/inventory/users
   * @response `200` `(UserInfo)[]` Список пользователей
   */
  inventoryUsersGet = (
    query?: {
      /**
       * Идентификатор запрошенного пользователя
       * @format guid
       */
      userGuid?: string | null;
    },
    params: RequestParams = {},
  ) =>
    this.request<UserInfo[], any>({
      path: `/api/v1/inventory/users`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags InventoryUsers
   * @name InventoryUsersGetWithDetails
   * @summary Получение детализированной информации о пользователе
   * @request GET:/api/v1/inventory/users/{userGuid}
   * @response `200` `UserInfo` Данные о пользователе
   */
  inventoryUsersGetWithDetails = (
    userGuid: string,
    params: RequestParams = {},
  ) =>
    this.request<UserInfo, any>({
      path: `/api/v1/inventory/users/${userGuid}`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags InventoryUsers
   * @name InventoryUsersUpdate
   * @summary Изменение пользователя
   * @request PUT:/api/v1/inventory/users/{userGuid}
   * @response `200` `File`
   */
  inventoryUsersUpdate = (
    userGuid: string,
    data: UserUpdateRequest,
    params: RequestParams = {},
  ) =>
    this.request<File, any>({
      path: `/api/v1/inventory/users/${userGuid}`,
      method: "PUT",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags InventoryUsers
   * @name InventoryUsersDelete
   * @summary Удаление пользователя
   * @request DELETE:/api/v1/inventory/users/{userGuid}
   * @response `200` `File`
   */
  inventoryUsersDelete = (userGuid: string, params: RequestParams = {}) =>
    this.request<File, any>({
      path: `/api/v1/inventory/users/${userGuid}`,
      method: "DELETE",
      ...params,
    });
  /**
   * No description
   *
   * @tags DataSources
   * @name DataSourcesGetActivity
   * @summary Получение данных о статистике сбора данных по источнику
   * @request POST:/api/v1/data/sources/activity
   * @response `200` `(SourceActivityInfo)[]`
   */
  dataSourcesGetActivity = (
    data: DataSourcesGetActivityPayload,
    params: RequestParams = {},
  ) =>
    this.request<SourceActivityInfo[], any>({
      path: `/api/v1/data/sources/activity`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags DataSources
   * @name DataSourcesGetItems
   * @summary Получение удаленных значений с источника
   * @request GET:/api/v1/data/sources/{sourceId}/items
   * @response `200` `(SourceItemInfo)[]`
   */
  dataSourcesGetItems = (sourceId: number, params: RequestParams = {}) =>
    this.request<SourceItemInfo[], any>({
      path: `/api/v1/data/sources/${sourceId}/items`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags DataSystem
   * @name DataSystemRestartCollection
   * @summary Перезапуск системы сбора данных
   * @request POST:/api/v1/data/system
   * @response `200` `File`
   */
  dataSystemRestartCollection = (params: RequestParams = {}) =>
    this.request<File, any>({
      path: `/api/v1/data/system`,
      method: "POST",
      ...params,
    });
  /**
   * No description
   *
   * @tags DataTags
   * @name DataTagsGetStatus
   * @summary Запись данных о состоянии последнего получения/вычисления значений тегов
   * @request POST:/api/v1/data/tags/status
   * @response `200` `Record<string,string>` Объект состояния последнего получениея/вычисления, сопоставленный с идентификаторами
   */
  dataTagsGetStatus = (data: TagMetricRequest, params: RequestParams = {}) =>
    this.request<Record<string, string>, any>({
      path: `/api/v1/data/tags/status`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags DataTags
   * @name DataTagsGetUsage
   * @summary Получение данных о использовании тегов
   * @request POST:/api/v1/data/tags/usage
   * @response `200` `Record<string,Record<string,string>>` Объект статистики использования, сопоставленный с идентификаторами
   */
  dataTagsGetUsage = (data: TagMetricRequest, params: RequestParams = {}) =>
    this.request<Record<string, Record<string, string>>, any>({
      path: `/api/v1/data/tags/usage`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags DataValues
   * @name DataValuesGet
   * @summary Получение значений на основании списка запросов
   * @request POST:/api/v1/data/values
   * @response `200` `(ValuesResponse)[]` Список ответов на запросы
   */
  dataValuesGet = (data: DataValuesGetPayload, params: RequestParams = {}) =>
    this.request<ValuesResponse[], any>({
      path: `/api/v1/data/values`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags DataValues
   * @name DataValuesWrite
   * @summary Запись значений на основании списка запросов
   * @request PUT:/api/v1/data/values
   * @response `200` `(ValuesTagResponse)[]` Список измененных значений
   */
  dataValuesWrite = (
    data: DataValuesWritePayload,
    params: RequestParams = {},
  ) =>
    this.request<ValuesTagResponse[], any>({
      path: `/api/v1/data/values`,
      method: "PUT",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags Auth
   * @name AuthAuthenticateLocal
   * @summary Аутентификация локального пользователя по связке "имя для входа/пароль"
   * @request POST:/api/v1/gateway/sessions/local
   * @response `200` `UserSessionWithAccessInfo` Данные о учетной записи
   */
  authAuthenticateLocal = (
    data: AuthLoginPassRequest,
    params: RequestParams = {},
  ) =>
    this.request<UserSessionWithAccessInfo, any>({
      path: `/api/v1/gateway/sessions/local`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags Auth
   * @name AuthAuthenticateEnergoIdUser
   * @summary Аутентификация пользователя, прошедшего проверку на сервере EnergoId
   * @request POST:/api/v1/gateway/sessions/energo-id
   * @response `200` `UserSessionWithAccessInfo` Данные о учетной записи
   */
  authAuthenticateEnergoIdUser = (
    data: AuthEnergoIdRequest,
    params: RequestParams = {},
  ) =>
    this.request<UserSessionWithAccessInfo, any>({
      path: `/api/v1/gateway/sessions/energo-id`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags Auth
   * @name AuthIdentify
   * @summary Получение информации о учетной записи на основе текущей сессии
   * @request GET:/api/v1/gateway/sessions/identify
   * @response `200` `UserSessionWithAccessInfo` Данные о учетной записи
   */
  authIdentify = (params: RequestParams = {}) =>
    this.request<UserSessionWithAccessInfo, any>({
      path: `/api/v1/gateway/sessions/identify`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags Auth
   * @name AuthLogout
   * @summary Delete: Закрытие уканной сессии пользователя
   * @request DELETE:/api/v1/gateway/sessions
   * @response `200` `File`
   */
  authLogout = (params: RequestParams = {}) =>
    this.request<File, any>({
      path: `/api/v1/gateway/sessions`,
      method: "DELETE",
      ...params,
    });
  /**
   * No description
   *
   * @tags Users
   * @name UsersGetActivity
   * @summary Получение времени последней активности пользователей
   * @request POST:/api/v1/gateway/users/activity
   * @response `200` `Record<string,string | null>`
   */
  usersGetActivity = (
    data: UsersGetActivityPayload,
    params: RequestParams = {},
  ) =>
    this.request<Record<string, string | null>, any>({
      path: `/api/v1/gateway/users/activity`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
}
