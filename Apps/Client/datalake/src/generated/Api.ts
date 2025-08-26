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
  AccessRightsApplyRequest,
  AccessRightsInfo,
  BlockFullInfo,
  BlockTreeInfo,
  BlockUpdateRequest,
  BlockWithTagsInfo,
  KeyValuePairOfValuesRequestKeyAndValuesRequestUsageInfo,
  LogCategory,
  LogInfo,
  LogType,
  SettingsInfo,
  SourceEntryInfo,
  SourceInfo,
  SourceItemInfo,
  SourceStateInfo,
  SourceUpdateRequest,
  TagCreateRequest,
  TagFullInfo,
  TagInfo,
  TagsGetValuesPayload,
  TagsWriteValuesPayload,
  TagUpdateRequest,
  UserAuthInfo,
  UserCreateRequest,
  UserDetailInfo,
  UserEnergoIdInfo,
  UserGroupCreateRequest,
  UserGroupDetailedInfo,
  UserGroupInfo,
  UserGroupTreeInfo,
  UserGroupUpdateRequest,
  UserInfo,
  UserLoginPass,
  UserSessionInfo,
  UserUpdateRequest,
  ValuesGetPayload,
  ValuesResponse,
  ValuesTagResponse,
  ValuesWritePayload,
} from "./data-contracts";
import { ContentType, HttpClient, RequestParams } from "./http-client";

export class Api<
  SecurityDataType = unknown,
> extends HttpClient<SecurityDataType> {
  /**
   * No description
   *
   * @tags Access
   * @name AccessGet
   * @summary Get: Получение списка прямых (не глобальных) разрешений субъекта на объект
   * @request GET:/api/access
   * @response `200` `(AccessRightsInfo)[]` Список разрешений
   */
  accessGet = (
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
      path: `/api/access`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags Access
   * @name AccessApplyChanges
   * @summary Post: Изменение разрешений для группы пользователей
   * @request POST:/api/access
   * @response `200` `File`
   */
  accessApplyChanges = (
    data: AccessRightsApplyRequest,
    params: RequestParams = {},
  ) =>
    this.request<File, any>({
      path: `/api/access`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags Auth
   * @name AuthAuthenticateLocal
   * @summary Post: Аутентификация локального пользователя по связке "имя для входа/пароль"
   * @request POST:/api/auth/local
   * @response `200` `UserSessionInfo` Данные о учетной записи
   */
  authAuthenticateLocal = (data: UserLoginPass, params: RequestParams = {}) =>
    this.request<UserSessionInfo, any>({
      path: `/api/auth/local`,
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
   * @summary Post: Аутентификация пользователя, прошедшего проверку на сервере EnergoId
   * @request POST:/api/auth/energo-id
   * @response `200` `UserSessionInfo` Данные о учетной записи
   */
  authAuthenticateEnergoIdUser = (
    data: UserEnergoIdInfo,
    params: RequestParams = {},
  ) =>
    this.request<UserSessionInfo, any>({
      path: `/api/auth/energo-id`,
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
   * @summary Get: Получение информации о учетной записи на основе текущей сессии
   * @request GET:/api/auth/identify
   * @response `200` `UserSessionInfo` Данные о учетной записи
   */
  authIdentify = (params: RequestParams = {}) =>
    this.request<UserSessionInfo, any>({
      path: `/api/auth/identify`,
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
   * @request DELETE:/api/auth/logout
   * @response `200` `File`
   */
  authLogout = (
    query: {
      /** Сессионный токен доступа */
      token: string;
    },
    params: RequestParams = {},
  ) =>
    this.request<File, any>({
      path: `/api/auth/logout`,
      method: "DELETE",
      query: query,
      ...params,
    });
  /**
   * No description
   *
   * @tags Blocks
   * @name BlocksCreate
   * @summary Post: Создание нового блока на основании переданной информации
   * @request POST:/api/blocks
   * @response `200` `BlockWithTagsInfo` Идентификатор блока
   */
  blocksCreate = (data: BlockFullInfo, params: RequestParams = {}) =>
    this.request<BlockWithTagsInfo, any>({
      path: `/api/blocks`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags Blocks
   * @name BlocksGetAll
   * @summary Get: Получение списка блоков с базовой информацией о них
   * @request GET:/api/blocks
   * @response `200` `(BlockWithTagsInfo)[]` Список блоков
   */
  blocksGetAll = (params: RequestParams = {}) =>
    this.request<BlockWithTagsInfo[], any>({
      path: `/api/blocks`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags Blocks
   * @name BlocksCreateEmpty
   * @summary Post: Создание нового отдельного блока с информацией по умолчанию
   * @request POST:/api/blocks/empty
   * @response `200` `BlockWithTagsInfo` Идентификатор блока
   */
  blocksCreateEmpty = (
    query?: {
      /**
       * Идентификатор родительского блока
       * @format int32
       */
      parentId?: number | null;
    },
    params: RequestParams = {},
  ) =>
    this.request<BlockWithTagsInfo, any>({
      path: `/api/blocks/empty`,
      method: "POST",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags Blocks
   * @name BlocksGet
   * @summary Get: Получение информации о выбранном блоке
   * @request GET:/api/blocks/{id}
   * @response `200` `BlockFullInfo` Информация о блоке
   */
  blocksGet = (id: number, params: RequestParams = {}) =>
    this.request<BlockFullInfo, any>({
      path: `/api/blocks/${id}`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags Blocks
   * @name BlocksUpdate
   * @summary Put: Изменение блока
   * @request PUT:/api/blocks/{id}
   * @response `200` `File`
   */
  blocksUpdate = (
    id: number,
    data: BlockUpdateRequest,
    params: RequestParams = {},
  ) =>
    this.request<File, any>({
      path: `/api/blocks/${id}`,
      method: "PUT",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags Blocks
   * @name BlocksDelete
   * @summary Delete: Удаление блока
   * @request DELETE:/api/blocks/{id}
   * @response `200` `File`
   */
  blocksDelete = (id: number, params: RequestParams = {}) =>
    this.request<File, any>({
      path: `/api/blocks/${id}`,
      method: "DELETE",
      ...params,
    });
  /**
   * No description
   *
   * @tags Blocks
   * @name BlocksGetTree
   * @summary Get: Получение иерархической структуры всех блоков
   * @request GET:/api/blocks/tree
   * @response `200` `(BlockTreeInfo)[]` Список обособленных блоков с вложенными блоками
   */
  blocksGetTree = (params: RequestParams = {}) =>
    this.request<BlockTreeInfo[], any>({
      path: `/api/blocks/tree`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags Blocks
   * @name BlocksMove
   * @summary Post: Перемещение блока
   * @request POST:/api/blocks/{id}/move
   * @response `200` `File`
   */
  blocksMove = (
    id: number,
    query?: {
      /**
       * Идентификатор нового родительского блока
       * @format int32
       */
      parentId?: number | null;
    },
    params: RequestParams = {},
  ) =>
    this.request<File, any>({
      path: `/api/blocks/${id}/move`,
      method: "POST",
      query: query,
      ...params,
    });
  /**
   * No description
   *
   * @tags Sources
   * @name SourcesCreateEmpty
   * @summary Post: Создание источника с информацией по умолчанию
   * @request POST:/api/sources/empty
   * @response `200` `SourceInfo` Идентификатор источника
   */
  sourcesCreateEmpty = (params: RequestParams = {}) =>
    this.request<SourceInfo, any>({
      path: `/api/sources/empty`,
      method: "POST",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags Sources
   * @name SourcesCreate
   * @summary Post: Создание источника на основе переданных данных
   * @request POST:/api/sources
   * @response `200` `SourceInfo` Идентификатор источника
   */
  sourcesCreate = (data: SourceInfo, params: RequestParams = {}) =>
    this.request<SourceInfo, any>({
      path: `/api/sources`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags Sources
   * @name SourcesGetAll
   * @summary Get: Получение списка источников
   * @request GET:/api/sources
   * @response `200` `(SourceInfo)[]` Список источников
   */
  sourcesGetAll = (
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
      path: `/api/sources`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags Sources
   * @name SourcesGet
   * @summary Get: Получение данных о источнике
   * @request GET:/api/sources/{id}
   * @response `200` `SourceInfo` Данные о источнике
   */
  sourcesGet = (id: number, params: RequestParams = {}) =>
    this.request<SourceInfo, any>({
      path: `/api/sources/${id}`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags Sources
   * @name SourcesUpdate
   * @summary Put: Изменение источника
   * @request PUT:/api/sources/{id}
   * @response `200` `File`
   */
  sourcesUpdate = (
    id: number,
    data: SourceUpdateRequest,
    params: RequestParams = {},
  ) =>
    this.request<File, any>({
      path: `/api/sources/${id}`,
      method: "PUT",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags Sources
   * @name SourcesDelete
   * @summary Delete: Удаление источника
   * @request DELETE:/api/sources/{id}
   * @response `200` `File`
   */
  sourcesDelete = (id: number, params: RequestParams = {}) =>
    this.request<File, any>({
      path: `/api/sources/${id}`,
      method: "DELETE",
      ...params,
    });
  /**
   * No description
   *
   * @tags Sources
   * @name SourcesGetItems
   * @summary Get: Получение доступных значений источника
   * @request GET:/api/sources/{id}/items
   * @response `200` `(SourceItemInfo)[]` Список данных источника
   */
  sourcesGetItems = (id: number, params: RequestParams = {}) =>
    this.request<SourceItemInfo[], any>({
      path: `/api/sources/${id}/items`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags Sources
   * @name SourcesGetItemsWithTags
   * @summary Get: Получение доступных значений и связанных тегов источника
   * @request GET:/api/sources/{id}/items-and-tags
   * @response `200` `(SourceEntryInfo)[]` Список данных источника
   */
  sourcesGetItemsWithTags = (id: number, params: RequestParams = {}) =>
    this.request<SourceEntryInfo[], any>({
      path: `/api/sources/${id}/items-and-tags`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags System
   * @name SystemGetLastUpdate
   * @summary Get: Получение даты последнего изменения структуры базы данных
   * @request GET:/api/system/last
   * @response `200` `string` Дата в строковом виде
   */
  systemGetLastUpdate = (params: RequestParams = {}) =>
    this.request<string, any>({
      path: `/api/system/last`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags System
   * @name SystemGetLogs
   * @summary Get: Получение списка сообщений
   * @request GET:/api/system/logs
   * @response `200` `(LogInfo)[]` Список сообщений
   */
  systemGetLogs = (
    query?: {
      /**
       * Идентификатор сообщения, с которого начать отсчёт количества в сторону более поздних
       * @format int32
       */
      lastId?: number | null;
      /**
       * Идентификатор сообщения, с которого начать отсчёт количества в сторону более ранних
       * @format int32
       */
      firstId?: number | null;
      /**
       * Сколько сообщений получить за этот запрос
       * @format int32
       */
      take?: number | null;
      /**
       * Идентификатор затронутого источника
       * @format int32
       */
      source?: number | null;
      /**
       * Идентификатор затронутого блока
       * @format int32
       */
      block?: number | null;
      /**
       * Идентификатор затронутого тега
       * @format guid
       */
      tag?: string | null;
      /**
       * Идентификатор затронутого пользователя
       * @format guid
       */
      user?: string | null;
      /**
       * Идентификатор затронутой группы пользователей
       * @format guid
       */
      group?: string | null;
      /** Выбранные категории сообщений */
      "categories[]"?: LogCategory[] | null;
      /** Выбранные типы сообщений */
      "types[]"?: LogType[] | null;
      /**
       * Идентификатор пользователя, создавшего сообщение
       * @format guid
       */
      author?: string | null;
    },
    params: RequestParams = {},
  ) =>
    this.request<LogInfo[], any>({
      path: `/api/system/logs`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags System
   * @name SystemGetVisits
   * @summary Get: Информация о визитах пользователей
   * @request GET:/api/system/visits
   * @response `200` `Record<string,string>` Даты визитов, сопоставленные с идентификаторами пользователей
   */
  systemGetVisits = (params: RequestParams = {}) =>
    this.request<Record<string, string>, any>({
      path: `/api/system/visits`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags System
   * @name SystemGetSourcesStates
   * @summary Get: Информация о подключении к источникам данных
   * @request GET:/api/system/sources
   * @response `200` `Record<string,SourceStateInfo>`
   */
  systemGetSourcesStates = (params: RequestParams = {}) =>
    this.request<Record<string, SourceStateInfo>, any>({
      path: `/api/system/sources`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags System
   * @name SystemGetTagsStates
   * @summary Get: Информация о подключении к источникам данных
   * @request GET:/api/system/tags
   * @response `200` `Record<string,Record<string,string>>`
   */
  systemGetTagsStates = (params: RequestParams = {}) =>
    this.request<Record<string, Record<string, string>>, any>({
      path: `/api/system/tags`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags System
   * @name SystemGetTagState
   * @summary Get: Информация о подключении к источникам данных
   * @request GET:/api/system/tags/{id}
   * @response `200` `Record<string,string>`
   */
  systemGetTagState = (id: number, params: RequestParams = {}) =>
    this.request<Record<string, string>, any>({
      path: `/api/system/tags/${id}`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags System
   * @name SystemGetSettings
   * @summary Get: Получение информации о настройках сервера
   * @request GET:/api/system/settings
   * @response `200` `SettingsInfo` Информация о настройках
   */
  systemGetSettings = (params: RequestParams = {}) =>
    this.request<SettingsInfo, any>({
      path: `/api/system/settings`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags System
   * @name SystemUpdateSettings
   * @summary Put: Изменение информации о настройках сервера
   * @request PUT:/api/system/settings
   * @response `200` `File`
   */
  systemUpdateSettings = (data: SettingsInfo, params: RequestParams = {}) =>
    this.request<File, any>({
      path: `/api/system/settings`,
      method: "PUT",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags System
   * @name SystemRestartState
   * @summary Put: Перестроение кэша
   * @request PUT:/api/system/restart/state
   * @response `200` `File`
   */
  systemRestartState = (params: RequestParams = {}) =>
    this.request<File, any>({
      path: `/api/system/restart/state`,
      method: "PUT",
      ...params,
    });
  /**
   * No description
   *
   * @tags System
   * @name SystemRestartValues
   * @summary Put: Перестроение кэша текущих (последних) значений
   * @request PUT:/api/system/restart/values
   * @response `200` `File`
   */
  systemRestartValues = (params: RequestParams = {}) =>
    this.request<File, any>({
      path: `/api/system/restart/values`,
      method: "PUT",
      ...params,
    });
  /**
   * No description
   *
   * @tags System
   * @name SystemGetAccess
   * @summary Get: Получение списка вычисленных прав доступа для каждого пользователя
   * @request GET:/api/system/access
   * @response `200` `Record<string,UserAuthInfo>`
   */
  systemGetAccess = (params: RequestParams = {}) =>
    this.request<Record<string, UserAuthInfo>, any>({
      path: `/api/system/access`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags System
   * @name SystemGetReadMetrics
   * @summary Get: Получение метрик запросов на чтение
   * @request GET:/api/system/reads
   * @response `200` `(KeyValuePairOfValuesRequestKeyAndValuesRequestUsageInfo)[]`
   */
  systemGetReadMetrics = (params: RequestParams = {}) =>
    this.request<
      KeyValuePairOfValuesRequestKeyAndValuesRequestUsageInfo[],
      any
    >({
      path: `/api/system/reads`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags Tags
   * @name TagsCreate
   * @summary Post: Создание нового тега
   * @request POST:/api/tags
   * @response `200` `TagInfo` Идентификатор нового тега в локальной базе данных
   */
  tagsCreate = (data: TagCreateRequest, params: RequestParams = {}) =>
    this.request<TagInfo, any>({
      path: `/api/tags`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags Tags
   * @name TagsGetAll
   * @summary Get: Получение списка тегов, включая информацию о источниках и настройках получения данных
   * @request GET:/api/tags
   * @response `200` `(TagInfo)[]` Плоский список объектов информации о тегах
   */
  tagsGetAll = (
    query?: {
      /**
       * Идентификатор источника. Если указан, будут выбраны теги только этого источника
       * @format int32
       */
      sourceId?: number | null;
      /** Список локальных идентификаторов тегов */
      id?: number[] | null;
      /** Список текущих наименований тегов */
      names?: string[] | null;
      /** Список глобальных идентификаторов тегов */
      guids?: string[] | null;
    },
    params: RequestParams = {},
  ) =>
    this.request<TagInfo[], any>({
      path: `/api/tags`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags Tags
   * @name TagsGet
   * @summary Get: Получение информации о конкретном теге, включая информацию о источнике и настройках получения данных
   * @request GET:/api/tags/{id}
   * @response `200` `TagFullInfo` Объект информации о теге
   */
  tagsGet = (id: number, params: RequestParams = {}) =>
    this.request<TagFullInfo, any>({
      path: `/api/tags/${id}`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags Tags
   * @name TagsUpdate
   * @summary Put: Изменение тега
   * @request PUT:/api/tags/{id}
   * @response `200` `File`
   */
  tagsUpdate = (
    id: number,
    data: TagUpdateRequest,
    params: RequestParams = {},
  ) =>
    this.request<File, any>({
      path: `/api/tags/${id}`,
      method: "PUT",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags Tags
   * @name TagsDelete
   * @summary Delete: Удаление тега
   * @request DELETE:/api/tags/{id}
   * @response `200` `File`
   */
  tagsDelete = (id: number, params: RequestParams = {}) =>
    this.request<File, any>({
      path: `/api/tags/${id}`,
      method: "DELETE",
      ...params,
    });
  /**
   * No description
   *
   * @tags Tags
   * @name TagsGetValues
   * @request POST:/api/tags/values
   * @response `200` `(ValuesResponse)[]`
   */
  tagsGetValues = (data: TagsGetValuesPayload, params: RequestParams = {}) =>
    this.request<ValuesResponse[], any>({
      path: `/api/tags/values`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags Tags
   * @name TagsWriteValues
   * @request PUT:/api/tags/values
   * @response `200` `(ValuesTagResponse)[]`
   */
  tagsWriteValues = (
    data: TagsWriteValuesPayload,
    params: RequestParams = {},
  ) =>
    this.request<ValuesTagResponse[], any>({
      path: `/api/tags/values`,
      method: "PUT",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags UserGroups
   * @name UserGroupsCreate
   * @summary Post: Создание новой группы пользователей
   * @request POST:/api/user-groups
   * @response `200` `UserGroupInfo` Идентификатор новой группы пользователей
   */
  userGroupsCreate = (
    data: UserGroupCreateRequest,
    params: RequestParams = {},
  ) =>
    this.request<UserGroupInfo, any>({
      path: `/api/user-groups`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags UserGroups
   * @name UserGroupsGetAll
   * @summary Get: Получение плоского списка групп пользователей
   * @request GET:/api/user-groups
   * @response `200` `(UserGroupInfo)[]` Список групп
   */
  userGroupsGetAll = (params: RequestParams = {}) =>
    this.request<UserGroupInfo[], any>({
      path: `/api/user-groups`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags UserGroups
   * @name UserGroupsGet
   * @summary Get: Получение информации о выбранной группе пользователей
   * @request GET:/api/user-groups/{groupGuid}
   * @response `200` `UserGroupInfo` Информация о группе
   */
  userGroupsGet = (groupGuid: string, params: RequestParams = {}) =>
    this.request<UserGroupInfo, any>({
      path: `/api/user-groups/${groupGuid}`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags UserGroups
   * @name UserGroupsUpdate
   * @summary Put: Изменение группы пользователей
   * @request PUT:/api/user-groups/{groupGuid}
   * @response `200` `File`
   */
  userGroupsUpdate = (
    groupGuid: string,
    data: UserGroupUpdateRequest,
    params: RequestParams = {},
  ) =>
    this.request<File, any>({
      path: `/api/user-groups/${groupGuid}`,
      method: "PUT",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags UserGroups
   * @name UserGroupsDelete
   * @summary Delete: Удаление группы пользователей
   * @request DELETE:/api/user-groups/{groupGuid}
   * @response `200` `File`
   */
  userGroupsDelete = (groupGuid: string, params: RequestParams = {}) =>
    this.request<File, any>({
      path: `/api/user-groups/${groupGuid}`,
      method: "DELETE",
      ...params,
    });
  /**
   * No description
   *
   * @tags UserGroups
   * @name UserGroupsGetTree
   * @summary Get: Получение иерархической структуры всех групп пользователей
   * @request GET:/api/user-groups/tree
   * @response `200` `(UserGroupTreeInfo)[]` Список обособленных групп с вложенными подгруппами
   */
  userGroupsGetTree = (params: RequestParams = {}) =>
    this.request<UserGroupTreeInfo[], any>({
      path: `/api/user-groups/tree`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags UserGroups
   * @name UserGroupsGetWithDetails
   * @summary Get: Получение детализированной информации о группе пользователей
   * @request GET:/api/user-groups/{groupGuid}/detailed
   * @response `200` `UserGroupDetailedInfo` Информация о группе с подгруппами и списком пользователей
   */
  userGroupsGetWithDetails = (groupGuid: string, params: RequestParams = {}) =>
    this.request<UserGroupDetailedInfo, any>({
      path: `/api/user-groups/${groupGuid}/detailed`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags UserGroups
   * @name UserGroupsMove
   * @summary Post: Перемещение группы пользователей
   * @request POST:/api/user-groups/{groupGuid}/move
   * @response `200` `File`
   */
  userGroupsMove = (
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
      path: `/api/user-groups/${groupGuid}/move`,
      method: "POST",
      query: query,
      ...params,
    });
  /**
   * No description
   *
   * @tags Users
   * @name UsersCreate
   * @summary Post: Создание пользователя на основании переданных данных
   * @request POST:/api/users
   * @response `200` `UserInfo` Идентификатор пользователя
   */
  usersCreate = (data: UserCreateRequest, params: RequestParams = {}) =>
    this.request<UserInfo, any>({
      path: `/api/users`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags Users
   * @name UsersGetAll
   * @summary Get: Получение списка пользователей
   * @request GET:/api/users
   * @response `200` `(UserInfo)[]` Список пользователей
   */
  usersGetAll = (params: RequestParams = {}) =>
    this.request<UserInfo[], any>({
      path: `/api/users`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags Users
   * @name UsersGet
   * @summary Get: Получение данных о пользователе
   * @request GET:/api/users/{userGuid}
   * @response `200` `UserInfo` Данные пользователя
   */
  usersGet = (userGuid: string, params: RequestParams = {}) =>
    this.request<UserInfo, any>({
      path: `/api/users/${userGuid}`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags Users
   * @name UsersUpdate
   * @summary Put: Изменение пользователя
   * @request PUT:/api/users/{userGuid}
   * @response `200` `File`
   */
  usersUpdate = (
    userGuid: string,
    data: UserUpdateRequest,
    params: RequestParams = {},
  ) =>
    this.request<File, any>({
      path: `/api/users/${userGuid}`,
      method: "PUT",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags Users
   * @name UsersDelete
   * @summary Delete: Удаление пользователя
   * @request DELETE:/api/users/{userGuid}
   * @response `200` `File`
   */
  usersDelete = (userGuid: string, params: RequestParams = {}) =>
    this.request<File, any>({
      path: `/api/users/${userGuid}`,
      method: "DELETE",
      ...params,
    });
  /**
   * No description
   *
   * @tags Users
   * @name UsersGetWithDetails
   * @summary Get: Получение детализированной информации о пользователе
   * @request GET:/api/users/{userGuid}/detailed
   * @response `200` `UserDetailInfo` Данные о пользователе
   */
  usersGetWithDetails = (userGuid: string, params: RequestParams = {}) =>
    this.request<UserDetailInfo, any>({
      path: `/api/users/${userGuid}/detailed`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags Users
   * @name UsersGetEnergoId
   * @summary Get: Получение списка пользователей, определенных на сервере EnergoId
   * @request GET:/api/users/energo-id
   * @response `200` `(UserEnergoIdInfo)[]` Список пользователей
   */
  usersGetEnergoId = (params: RequestParams = {}) =>
    this.request<UserEnergoIdInfo[], any>({
      path: `/api/users/energo-id`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags Users
   * @name UsersUpdateEnergoId
   * @summary Put: Обновление данных из EnergoId
   * @request PUT:/api/users/energo-id
   * @response `200` `File`
   */
  usersUpdateEnergoId = (params: RequestParams = {}) =>
    this.request<File, any>({
      path: `/api/users/energo-id`,
      method: "PUT",
      ...params,
    });
  /**
   * No description
   *
   * @tags Values
   * @name ValuesGet
   * @summary Post: Получение значений на основании списка запросов
   * @request POST:/api/values
   * @response `200` `(ValuesResponse)[]` Список ответов на запросы
   */
  valuesGet = (data: ValuesGetPayload, params: RequestParams = {}) =>
    this.request<ValuesResponse[], any>({
      path: `/api/values`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags Values
   * @name ValuesWrite
   * @summary Put: Запись значений на основании списка запросов
   * @request PUT:/api/values
   * @response `200` `(ValuesTagResponse)[]` Список измененных начений
   */
  valuesWrite = (data: ValuesWritePayload, params: RequestParams = {}) =>
    this.request<ValuesTagResponse[], any>({
      path: `/api/values`,
      method: "PUT",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
}
