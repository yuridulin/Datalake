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
	EnergoIdInfo,
	LogInfo,
	SettingsInfo,
	SourceEntryInfo,
	SourceInfo,
	SourceItemInfo,
	TagAsInputInfo,
	TagCreateRequest,
	TagInfo,
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
	UserUpdateRequest,
	ValuesGetPayload,
	ValuesResponse,
	ValuesTagResponse,
	ValuesWritePayload,
} from './data-contracts'
import { ContentType, HttpClient, RequestParams } from './http-client'

export class Api<SecurityDataType = unknown> extends HttpClient<SecurityDataType> {
	/**
	 * No description
	 *
	 * @tags Blocks
	 * @name BlocksCreate
	 * @summary Создание нового блока на основании переданной информации
	 * @request POST:/api/Blocks
	 * @response `200` `number` Идентификатор блока
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
	 * @summary Получение списка блоков с базовой информацией о них
	 * @request GET:/api/Blocks
	 * @response `200` `(BlockSimpleInfo)[]` Список блоков
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
	 * @summary Создание нового отдельного блока с информацией по умолчанию
	 * @request POST:/api/Blocks/empty
	 * @response `200` `number` Идентификатор блока
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
	 * @summary Получение информации о выбранном блоке
	 * @request GET:/api/Blocks/{id}
	 * @response `200` `BlockInfo` Информация о блоке
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
	 * @summary Изменение блока
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
	 * @summary Удаление блока
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
	 * @summary Получение иерархической структуры всех блоков
	 * @request GET:/api/Blocks/tree
	 * @response `200` `(BlockTreeInfo)[]` Список обособленных блоков с вложенными блоками
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
	 * @summary Получение даты последнего изменения структуры базы данных
	 * @request GET:/api/Config/last
	 * @response `200` `string` Дата в строковом виде
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
	 * @summary Получение списка сообщений
	 * @request GET:/api/Config/logs
	 * @response `200` `(LogInfo)[]`
	 */
	configGetLogs = (
		query?: {
			/**
			 * Сколько сообщений получить за этот запрос
			 * @format int32
			 */
			take?: number | null
			/**
			 * Идентификатор сообщения, с которого начать отсчёт количества
			 * @format int32
			 */
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
	 * @tags Config
	 * @name ConfigGetSettings
	 * @summary Получение информации о настройках сервера
	 * @request GET:/api/Config/settings
	 * @response `200` `SettingsInfo` Информация о настройках
	 */
	configGetSettings = (params: RequestParams = {}) =>
		this.request<SettingsInfo, any>({
			path: `/api/Config/settings`,
			method: 'GET',
			format: 'json',
			...params,
		})
	/**
	 * No description
	 *
	 * @tags Config
	 * @name ConfigUpdateSettings
	 * @summary Изменение информации о настройках сервера
	 * @request PUT:/api/Config/settings
	 * @response `200` `File`
	 */
	configUpdateSettings = (data: SettingsInfo, params: RequestParams = {}) =>
		this.request<File, any>({
			path: `/api/Config/settings`,
			method: 'PUT',
			body: data,
			type: ContentType.Json,
			...params,
		})
	/**
	 * No description
	 *
	 * @tags Sources
	 * @name SourcesCreate
	 * @summary Создание источника с информацией по умолчанию
	 * @request POST:/api/Sources/empty
	 * @response `200` `number` Идентификатор источника
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
	 * @summary Создание источника на основе переданных данных
	 * @request POST:/api/Sources
	 * @response `200` `number` Идентификатор источника
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
	 * @summary Получение списка источников
	 * @request GET:/api/Sources
	 * @response `200` `(SourceInfo)[]` Список источников
	 */
	sourcesReadAll = (
		query?: {
			/**
			 * Включить ли в список системные источники
			 * @default false
			 */
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
	 * @summary Получение данных о источнике
	 * @request GET:/api/Sources/{id}
	 * @response `200` `SourceInfo` Данные о источнике
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
	 * @summary Изменение источника
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
	 * @summary Удаление источника
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
	 * @summary Получение доступных значений источника
	 * @request GET:/api/Sources/{id}/items
	 * @response `200` `(SourceItemInfo)[]` Список данных источника
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
	 * @summary Получение доступных значений и связанных тегов источника
	 * @request GET:/api/Sources/{id}/items-and-tags
	 * @response `200` `(SourceEntryInfo)[]` Список данных источника
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
			/** Список локальных идентификаторов тегов */
			id?: number[] | null
			/** Список текущих наименований тегов */
			names?: string[] | null
			/** Список глобальных идентификаторов тегов */
			guids?: string[] | null
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
	 * @request GET:/api/Tags/{guid}
	 * @response `200` `TagInfo` Объект информации о теге
	 */
	tagsRead = (guid: string, params: RequestParams = {}) =>
		this.request<TagInfo, any>({
			path: `/api/Tags/${guid}`,
			method: 'GET',
			format: 'json',
			...params,
		})
	/**
	 * No description
	 *
	 * @tags Tags
	 * @name TagsUpdate
	 * @summary Изменение тега
	 * @request PUT:/api/Tags/{guid}
	 * @response `200` `File`
	 */
	tagsUpdate = (guid: string, data: TagUpdateRequest, params: RequestParams = {}) =>
		this.request<File, any>({
			path: `/api/Tags/${guid}`,
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
	 * @summary Удаление тега
	 * @request DELETE:/api/Tags/{guid}
	 * @response `200` `File`
	 */
	tagsDelete = (guid: string, params: RequestParams = {}) =>
		this.request<File, any>({
			path: `/api/Tags/${guid}`,
			method: 'DELETE',
			...params,
		})
	/**
	 * No description
	 *
	 * @tags Tags
	 * @name TagsReadPossibleInputs
	 * @summary Получение списка тегов, подходящих для использования в формулах
	 * @request GET:/api/Tags/inputs
	 * @response `200` `(TagAsInputInfo)[]` Список тегов
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
	 * @summary Создание новой группы пользователей
	 * @request POST:/api/UserGroups
	 * @response `200` `string` Идентификатор новой группы пользователей
	 */
	userGroupsCreate = (data: UserGroupCreateRequest, params: RequestParams = {}) =>
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
	 * @summary Получение плоского списка групп пользователей
	 * @request GET:/api/UserGroups
	 * @response `200` `(UserGroupInfo)[]` Список групп
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
	 * @summary Получение информации о выбранной группе пользователей
	 * @request GET:/api/UserGroups/{groupGuid}
	 * @response `200` `UserGroupInfo` Информация о группе
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
	 * @summary Изменение группы пользователей
	 * @request PUT:/api/UserGroups/{groupGuid}
	 * @response `200` `File`
	 */
	userGroupsUpdate = (groupGuid: string, data: UserGroupUpdateRequest, params: RequestParams = {}) =>
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
	 * @summary Удаление группы пользователей
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
	 * @summary Получение иерархической структуры всех групп пользователей
	 * @request GET:/api/UserGroups/tree
	 * @response `200` `(UserGroupTreeInfo)[]` Список обособленных групп с вложенными подгруппами
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
	 * @summary Получение детализированной информации о группе пользователей
	 * @request GET:/api/UserGroups/{groupGuid}/detailed
	 * @response `200` `UserGroupDetailedInfo` Информация о группе с подгруппами и списком пользователей
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
	 * @name UsersGetEnergoIdList
	 * @summary Получение списка пользователей, определенных на сервере EnergoId
	 * @request GET:/api/Users/energo-id
	 * @response `200` `EnergoIdInfo` Список пользователей
	 */
	usersGetEnergoIdList = (
		query?: {
			/** @format guid */
			currentUserGuid?: string | null
		},
		params: RequestParams = {},
	) =>
		this.request<EnergoIdInfo, any>({
			path: `/api/Users/energo-id`,
			method: 'GET',
			query: query,
			format: 'json',
			...params,
		})
	/**
	 * No description
	 *
	 * @tags Users
	 * @name UsersAuthenticateEnergoIdUser
	 * @summary Аутентификация пользователя, прошедшего проверку на сервере EnergoId
	 * @request POST:/api/Users/energo-id
	 * @response `200` `UserAuthInfo` Данные о учетной записи
	 */
	usersAuthenticateEnergoIdUser = (data: UserEnergoIdInfo, params: RequestParams = {}) =>
		this.request<UserAuthInfo, any>({
			path: `/api/Users/energo-id`,
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
	 * @name UsersAuthenticate
	 * @summary Аутентификация локального пользователя по связке "имя для входа/пароль"
	 * @request POST:/api/Users/auth
	 * @response `200` `UserAuthInfo` Данные о учетной записи
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
	 * @name UsersIdentify
	 * @summary Получение информации о учетной записи на основе текущей сессии
	 * @request GET:/api/Users/identify
	 * @response `200` `UserAuthInfo` Данные о учетной записи
	 */
	usersIdentify = (params: RequestParams = {}) =>
		this.request<UserAuthInfo, any>({
			path: `/api/Users/identify`,
			method: 'GET',
			format: 'json',
			...params,
		})
	/**
	 * No description
	 *
	 * @tags Users
	 * @name UsersCreate
	 * @summary Создание пользователя на основании переданных данных
	 * @request POST:/api/Users
	 * @response `200` `string` Идентификатор пользователя
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
	 * @summary Получение списка пользователей
	 * @request GET:/api/Users
	 * @response `200` `(UserInfo)[]` Список пользователей
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
	 * @summary Получение данных о пользователе
	 * @request GET:/api/Users/{userGuid}
	 * @response `200` `UserInfo` Данные пользователя
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
	 * @summary Изменение пользователя
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
	 * @summary Удаление пользователя
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
	 * @summary Получение детализированной информации о пользователе
	 * @request GET:/api/Users/{userGuid}/detailed
	 * @response `200` `UserDetailInfo` Данные о пользователе
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
	 * @summary Получение значений на основании списка запросов
	 * @request POST:/api/Tags/Values
	 * @response `200` `(ValuesResponse)[]` Список ответов на запросы
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
	 * @summary Запись значений на основании списка запросов
	 * @request PUT:/api/Tags/Values
	 * @response `200` `(ValuesTagResponse)[]` Список измененных начений
	 */
	valuesWrite = (data: ValuesWritePayload, params: RequestParams = {}) =>
		this.request<ValuesTagResponse[], any>({
			path: `/api/Tags/Values`,
			method: 'PUT',
			body: data,
			type: ContentType.Json,
			format: 'json',
			...params,
		})
}
