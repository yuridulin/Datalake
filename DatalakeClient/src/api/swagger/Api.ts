/* eslint-disable */
/* tslint:disable */
/*
 * ---------------------------------------------------------------
 * ## THIS FILE WAS GENERATED VIA SWAGGER-TYPESCRIPT-API        ##
 * ##                                                           ##
 * ## AUTHOR: acacode                                           ##
 * ## SOURCE: https://github.com/acacode/swagger-typescript-api ##
 * ---------------------------------------------------------------
 */

import {
	BlockInfo,
	BlockSimpleInfo,
	BlockTreeInfo,
	CreateUserGroupRequest,
	LogInfo,
	SourceEntryInfo,
	SourceInfo,
	SourceItemInfo,
	TagAsInputInfo,
	TagCreateRequest,
	TagInfo,
	TagUpdateRequest,
	UpdateUserGroupRequest,
	UserAuthInfo,
	UserCreateRequest,
	UserDetailInfo,
	UserGroupDetailedInfo,
	UserGroupInfo,
	UserGroupTreeInfo,
	UserInfo,
	UserLoginPass,
	UserUpdateRequest,
	ValuesGetPayload,
	ValuesResponse,
	ValuesWritePayload,
} from './data-contracts'
import { ContentType, HttpClient, RequestParams } from './http-client'

export class Api<SecurityDataType = unknown> extends HttpClient<SecurityDataType> {
	/**
	 * No description
	 *
	 * @tags Blocks
	 * @name BlocksCreate
	 * @request POST:/api/Blocks
	 * @response `200` `number`
	 */
	blocksCreate = (data: BlockInfo, params: RequestParams = {}) =>
		this.request<number, any>({
			path: `/api/Blocks`,
			method: 'POST',
			body: data,
			type: ContentType.Json,
			format: 'json',
			...params,
		})
	/**
	 * No description
	 *
	 * @tags Blocks
	 * @name BlocksReadAll
	 * @request GET:/api/Blocks
	 * @response `200` `(BlockSimpleInfo)[]`
	 */
	blocksReadAll = (params: RequestParams = {}) =>
		this.request<BlockSimpleInfo[], any>({
			path: `/api/Blocks`,
			method: 'GET',
			format: 'json',
			...params,
		})
	/**
	 * No description
	 *
	 * @tags Blocks
	 * @name BlocksCreateEmpty
	 * @request POST:/api/Blocks/empty
	 * @response `200` `number`
	 */
	blocksCreateEmpty = (params: RequestParams = {}) =>
		this.request<number, any>({
			path: `/api/Blocks/empty`,
			method: 'POST',
			format: 'json',
			...params,
		})
	/**
	 * No description
	 *
	 * @tags Blocks
	 * @name BlocksRead
	 * @request GET:/api/Blocks/{id}
	 * @response `200` `BlockInfo`
	 */
	blocksRead = (id: number, params: RequestParams = {}) =>
		this.request<BlockInfo, any>({
			path: `/api/Blocks/${id}`,
			method: 'GET',
			format: 'json',
			...params,
		})
	/**
	 * No description
	 *
	 * @tags Blocks
	 * @name BlocksUpdate
	 * @request PUT:/api/Blocks/{id}
	 * @response `200` `File`
	 */
	blocksUpdate = (id: number, data: BlockInfo, params: RequestParams = {}) =>
		this.request<File, any>({
			path: `/api/Blocks/${id}`,
			method: 'PUT',
			body: data,
			type: ContentType.Json,
			...params,
		})
	/**
	 * No description
	 *
	 * @tags Blocks
	 * @name BlocksDelete
	 * @request DELETE:/api/Blocks/{id}
	 * @response `200` `File`
	 */
	blocksDelete = (id: number, params: RequestParams = {}) =>
		this.request<File, any>({
			path: `/api/Blocks/${id}`,
			method: 'DELETE',
			...params,
		})
	/**
	 * No description
	 *
	 * @tags Blocks
	 * @name BlocksReadAsTree
	 * @request GET:/api/Blocks/tree
	 * @response `200` `(BlockTreeInfo)[]`
	 */
	blocksReadAsTree = (params: RequestParams = {}) =>
		this.request<BlockTreeInfo[], any>({
			path: `/api/Blocks/tree`,
			method: 'GET',
			format: 'json',
			...params,
		})
	/**
	 * No description
	 *
	 * @tags Config
	 * @name ConfigGetLastUpdate
	 * @request GET:/api/Config/last
	 * @response `200` `string`
	 */
	configGetLastUpdate = (params: RequestParams = {}) =>
		this.request<string, any>({
			path: `/api/Config/last`,
			method: 'GET',
			format: 'json',
			...params,
		})
	/**
	 * No description
	 *
	 * @tags Config
	 * @name ConfigGetLogs
	 * @request GET:/api/Config/logs
	 * @response `200` `(LogInfo)[]`
	 */
	configGetLogs = (
		query?: {
			/** @format int32 */
			take?: number | null
			/** @format int32 */
			lastId?: number | null
		},
		params: RequestParams = {},
	) =>
		this.request<LogInfo[], any>({
			path: `/api/Config/logs`,
			method: 'GET',
			query: query,
			format: 'json',
			...params,
		})
	/**
	 * No description
	 *
	 * @tags Sources
	 * @name SourcesCreate
	 * @request POST:/api/Sources/empty
	 * @response `200` `number`
	 */
	sourcesCreate = (params: RequestParams = {}) =>
		this.request<number, any>({
			path: `/api/Sources/empty`,
			method: 'POST',
			format: 'json',
			...params,
		})
	/**
	 * No description
	 *
	 * @tags Sources
	 * @name SourcesCreate2
	 * @request POST:/api/Sources
	 * @response `200` `number`
	 */
	sourcesCreate2 = (data: SourceInfo, params: RequestParams = {}) =>
		this.request<number, any>({
			path: `/api/Sources`,
			method: 'POST',
			body: data,
			type: ContentType.Json,
			format: 'json',
			...params,
		})
	/**
	 * No description
	 *
	 * @tags Sources
	 * @name SourcesReadAll
	 * @request GET:/api/Sources
	 * @response `200` `(SourceInfo)[]`
	 */
	sourcesReadAll = (
		query?: {
			/** @default false */
			withCustom?: boolean
		},
		params: RequestParams = {},
	) =>
		this.request<SourceInfo[], any>({
			path: `/api/Sources`,
			method: 'GET',
			query: query,
			format: 'json',
			...params,
		})
	/**
	 * No description
	 *
	 * @tags Sources
	 * @name SourcesRead
	 * @request GET:/api/Sources/{id}
	 * @response `200` `SourceInfo`
	 */
	sourcesRead = (id: number, params: RequestParams = {}) =>
		this.request<SourceInfo, any>({
			path: `/api/Sources/${id}`,
			method: 'GET',
			format: 'json',
			...params,
		})
	/**
	 * No description
	 *
	 * @tags Sources
	 * @name SourcesUpdate
	 * @request PUT:/api/Sources/{id}
	 * @response `200` `File`
	 */
	sourcesUpdate = (id: number, data: SourceInfo, params: RequestParams = {}) =>
		this.request<File, any>({
			path: `/api/Sources/${id}`,
			method: 'PUT',
			body: data,
			type: ContentType.Json,
			...params,
		})
	/**
	 * No description
	 *
	 * @tags Sources
	 * @name SourcesDelete
	 * @request DELETE:/api/Sources/{id}
	 * @response `200` `File`
	 */
	sourcesDelete = (id: number, params: RequestParams = {}) =>
		this.request<File, any>({
			path: `/api/Sources/${id}`,
			method: 'DELETE',
			...params,
		})
	/**
	 * No description
	 *
	 * @tags Sources
	 * @name SourcesGetItems
	 * @request GET:/api/Sources/{id}/items
	 * @response `200` `(SourceItemInfo)[]`
	 */
	sourcesGetItems = (id: number, params: RequestParams = {}) =>
		this.request<SourceItemInfo[], any>({
			path: `/api/Sources/${id}/items`,
			method: 'GET',
			format: 'json',
			...params,
		})
	/**
	 * No description
	 *
	 * @tags Sources
	 * @name SourcesGetItemsWithTags
	 * @request GET:/api/Sources/{id}/items-and-tags
	 * @response `200` `(SourceEntryInfo)[]`
	 */
	sourcesGetItemsWithTags = (id: number, params: RequestParams = {}) =>
		this.request<SourceEntryInfo[], any>({
			path: `/api/Sources/${id}/items-and-tags`,
			method: 'GET',
			format: 'json',
			...params,
		})
	/**
	 * No description
	 *
	 * @tags Tags
	 * @name TagsCreate
	 * @summary Создание нового тега
	 * @request POST:/api/Tags
	 * @response `200` `number` Идентификатор нового тега в локальной базе данных
	 */
	tagsCreate = (data: TagCreateRequest, params: RequestParams = {}) =>
		this.request<number, any>({
			path: `/api/Tags`,
			method: 'POST',
			body: data,
			type: ContentType.Json,
			format: 'json',
			...params,
		})
	/**
	 * No description
	 *
	 * @tags Tags
	 * @name TagsReadAll
	 * @summary Получение списка тегов, включая информацию о источниках и настройках получения данных
	 * @request GET:/api/Tags
	 * @response `200` `(TagInfo)[]` Плоский список объектов информации о тегах
	 */
	tagsReadAll = (
		query?: {
			/**
			 * Идентификатор источника. Если указан, будут выбраны теги только этого источника
			 * @format int32
			 */
			sourceId?: number | null
		},
		params: RequestParams = {},
	) =>
		this.request<TagInfo[], any>({
			path: `/api/Tags`,
			method: 'GET',
			query: query,
			format: 'json',
			...params,
		})
	/**
	 * No description
	 *
	 * @tags Tags
	 * @name TagsRead
	 * @summary Получение информации о конкретном теге, включая информацию о источнике и настройках получения данных
	 * @request GET:/api/Tags/{id}
	 * @response `200` `TagInfo` Объект информации о теге
	 */
	tagsRead = (id: number, params: RequestParams = {}) =>
		this.request<TagInfo, any>({
			path: `/api/Tags/${id}`,
			method: 'GET',
			format: 'json',
			...params,
		})
	/**
	 * No description
	 *
	 * @tags Tags
	 * @name TagsUpdate
	 * @request PUT:/api/Tags/{id}
	 * @response `200` `File`
	 */
	tagsUpdate = (id: number, data: TagUpdateRequest, params: RequestParams = {}) =>
		this.request<File, any>({
			path: `/api/Tags/${id}`,
			method: 'PUT',
			body: data,
			type: ContentType.Json,
			...params,
		})
	/**
	 * No description
	 *
	 * @tags Tags
	 * @name TagsDelete
	 * @request DELETE:/api/Tags/{id}
	 * @response `200` `File`
	 */
	tagsDelete = (id: number, params: RequestParams = {}) =>
		this.request<File, any>({
			path: `/api/Tags/${id}`,
			method: 'DELETE',
			...params,
		})
	/**
	 * No description
	 *
	 * @tags Tags
	 * @name TagsReadPossibleInputs
	 * @request GET:/api/Tags/inputs
	 * @response `200` `(TagAsInputInfo)[]`
	 */
	tagsReadPossibleInputs = (params: RequestParams = {}) =>
		this.request<TagAsInputInfo[], any>({
			path: `/api/Tags/inputs`,
			method: 'GET',
			format: 'json',
			...params,
		})
	/**
	 * No description
	 *
	 * @tags UserGroups
	 * @name UserGroupsCreate
	 * @request POST:/api/UserGroups
	 * @response `200` `string`
	 */
	userGroupsCreate = (data: CreateUserGroupRequest, params: RequestParams = {}) =>
		this.request<string, any>({
			path: `/api/UserGroups`,
			method: 'POST',
			body: data,
			type: ContentType.Json,
			format: 'json',
			...params,
		})
	/**
	 * No description
	 *
	 * @tags UserGroups
	 * @name UserGroupsReadAll
	 * @request GET:/api/UserGroups
	 * @response `200` `(UserGroupInfo)[]`
	 */
	userGroupsReadAll = (params: RequestParams = {}) =>
		this.request<UserGroupInfo[], any>({
			path: `/api/UserGroups`,
			method: 'GET',
			format: 'json',
			...params,
		})
	/**
	 * No description
	 *
	 * @tags UserGroups
	 * @name UserGroupsRead
	 * @request GET:/api/UserGroups/{groupGuid}
	 * @response `200` `UserGroupInfo`
	 */
	userGroupsRead = (groupGuid: string, params: RequestParams = {}) =>
		this.request<UserGroupInfo, any>({
			path: `/api/UserGroups/${groupGuid}`,
			method: 'GET',
			format: 'json',
			...params,
		})
	/**
	 * No description
	 *
	 * @tags UserGroups
	 * @name UserGroupsUpdate
	 * @request PUT:/api/UserGroups/{groupGuid}
	 * @response `200` `File`
	 */
	userGroupsUpdate = (groupGuid: string, data: UpdateUserGroupRequest, params: RequestParams = {}) =>
		this.request<File, any>({
			path: `/api/UserGroups/${groupGuid}`,
			method: 'PUT',
			body: data,
			type: ContentType.Json,
			...params,
		})
	/**
	 * No description
	 *
	 * @tags UserGroups
	 * @name UserGroupsDelete
	 * @request DELETE:/api/UserGroups/{groupGuid}
	 * @response `200` `File`
	 */
	userGroupsDelete = (groupGuid: string, params: RequestParams = {}) =>
		this.request<File, any>({
			path: `/api/UserGroups/${groupGuid}`,
			method: 'DELETE',
			...params,
		})
	/**
	 * No description
	 *
	 * @tags UserGroups
	 * @name UserGroupsReadAsTree
	 * @request GET:/api/UserGroups/tree
	 * @response `200` `(UserGroupTreeInfo)[]`
	 */
	userGroupsReadAsTree = (params: RequestParams = {}) =>
		this.request<UserGroupTreeInfo[], any>({
			path: `/api/UserGroups/tree`,
			method: 'GET',
			format: 'json',
			...params,
		})
	/**
	 * No description
	 *
	 * @tags UserGroups
	 * @name UserGroupsReadWithDetails
	 * @request GET:/api/UserGroups/{groupGuid}/detailed
	 * @response `200` `UserGroupDetailedInfo`
	 */
	userGroupsReadWithDetails = (groupGuid: string, params: RequestParams = {}) =>
		this.request<UserGroupDetailedInfo, any>({
			path: `/api/UserGroups/${groupGuid}/detailed`,
			method: 'GET',
			format: 'json',
			...params,
		})
	/**
	 * No description
	 *
	 * @tags Users
	 * @name UsersAuthenticate
	 * @request POST:/api/Users/auth
	 * @response `200` `UserAuthInfo`
	 */
	usersAuthenticate = (data: UserLoginPass, params: RequestParams = {}) =>
		this.request<UserAuthInfo, any>({
			path: `/api/Users/auth`,
			method: 'POST',
			body: data,
			type: ContentType.Json,
			format: 'json',
			...params,
		})
	/**
	 * No description
	 *
	 * @tags Users
	 * @name UsersCreate
	 * @request POST:/api/Users
	 * @response `200` `string`
	 */
	usersCreate = (data: UserCreateRequest, params: RequestParams = {}) =>
		this.request<string, any>({
			path: `/api/Users`,
			method: 'POST',
			body: data,
			type: ContentType.Json,
			format: 'json',
			...params,
		})
	/**
	 * No description
	 *
	 * @tags Users
	 * @name UsersReadAll
	 * @request GET:/api/Users
	 * @response `200` `(UserInfo)[]`
	 */
	usersReadAll = (params: RequestParams = {}) =>
		this.request<UserInfo[], any>({
			path: `/api/Users`,
			method: 'GET',
			format: 'json',
			...params,
		})
	/**
	 * No description
	 *
	 * @tags Users
	 * @name UsersRead
	 * @request GET:/api/Users/{userGuid}
	 * @response `200` `UserInfo`
	 */
	usersRead = (userGuid: string, params: RequestParams = {}) =>
		this.request<UserInfo, any>({
			path: `/api/Users/${userGuid}`,
			method: 'GET',
			format: 'json',
			...params,
		})
	/**
	 * No description
	 *
	 * @tags Users
	 * @name UsersUpdate
	 * @request PUT:/api/Users/{userGuid}
	 * @response `200` `File`
	 */
	usersUpdate = (userGuid: string, data: UserUpdateRequest, params: RequestParams = {}) =>
		this.request<File, any>({
			path: `/api/Users/${userGuid}`,
			method: 'PUT',
			body: data,
			type: ContentType.Json,
			...params,
		})
	/**
	 * No description
	 *
	 * @tags Users
	 * @name UsersDelete
	 * @request DELETE:/api/Users/{userGuid}
	 * @response `200` `File`
	 */
	usersDelete = (userGuid: string, params: RequestParams = {}) =>
		this.request<File, any>({
			path: `/api/Users/${userGuid}`,
			method: 'DELETE',
			...params,
		})
	/**
	 * No description
	 *
	 * @tags Users
	 * @name UsersReadWithDetails
	 * @request GET:/api/Users/{userGuid}/detailed
	 * @response `200` `UserDetailInfo`
	 */
	usersReadWithDetails = (userGuid: string, params: RequestParams = {}) =>
		this.request<UserDetailInfo, any>({
			path: `/api/Users/${userGuid}/detailed`,
			method: 'GET',
			format: 'json',
			...params,
		})
	/**
	 * No description
	 *
	 * @tags Values
	 * @name ValuesGet
	 * @request POST:/api/Tags/Values
	 * @response `200` `(ValuesResponse)[]`
	 */
	valuesGet = (data: ValuesGetPayload, params: RequestParams = {}) =>
		this.request<ValuesResponse[], any>({
			path: `/api/Tags/Values`,
			method: 'POST',
			body: data,
			type: ContentType.Json,
			format: 'json',
			...params,
		})
	/**
	 * No description
	 *
	 * @tags Values
	 * @name ValuesWrite
	 * @request PUT:/api/Tags/Values
	 * @response `200` `(ValuesResponse)[]`
	 */
	valuesWrite = (data: ValuesWritePayload, params: RequestParams = {}) =>
		this.request<ValuesResponse[], any>({
			path: `/api/Tags/Values`,
			method: 'PUT',
			body: data,
			type: ContentType.Json,
			format: 'json',
			...params,
		})
}
