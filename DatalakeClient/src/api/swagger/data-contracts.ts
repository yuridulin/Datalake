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

/** Информация о сущности */
export interface BlockInfo {
	/**
	 * Идентификатор
	 * @format int32
	 */
	id: number
	/**
	 * Наименование
	 * @minLength 1
	 */
	name: string
	/** Текстовое описание */
	description?: string | null
	/** Информация о родительской сущности */
	parent?: BlockParentInfo | null
	/** Список дочерних сущностей */
	children: BlockChildInfo[]
	/** Список статических свойств сущности */
	properties: BlockPropertyInfo[]
	/** Список прикреплённых тегов */
	tags: BlockTagInfo[]
}

/** Информация о родительской сущности */
export type BlockParentInfo = BlockRelationInfo & object

/** Связанный с блоком объект */
export interface BlockRelationInfo {
	/**
	 * Идентификатор
	 * @format int32
	 */
	id: number
	/**
	 * Наименование
	 * @minLength 1
	 */
	name: string
}

/** Информация о дочерней сущности */
export type BlockChildInfo = BlockRelationInfo & object

/** Информация о статическом свойстве сущности */
export type BlockPropertyInfo = BlockRelationInfo & {
	/** Тип значения свойства */
	type: TagType
	/**
	 * Значение свойства
	 * @minLength 1
	 */
	value: string
}

/** Тип данных */
export enum TagType {
	String = 'String',
	Number = 'Number',
	Boolean = 'Boolean',
}

/** Информация о закреплённом теге */
export type BlockTagInfo = BlockRelationInfo & {
	/** Тип значений тега */
	tagType: BlockTagRelation
}

/** Тип связи тега и блока */
export enum BlockTagRelation {
	Static = 'Static',
	Input = 'Input',
	Output = 'Output',
}

/** Информация о сущности */
export interface BlockSimpleInfo {
	/**
	 * Идентификатор
	 * @format int32
	 */
	id: number
	/**
	 * Наименование
	 * @minLength 1
	 */
	name: string
	/** Текстовое описание */
	description?: string | null
}

/** Информация о сущности в иерархическом представлении */
export interface BlockTreeInfo {
	/**
	 * Идентификатор
	 * @format int32
	 */
	id: number
	/**
	 * Наименование
	 * @minLength 1
	 */
	name: string
	/** Текстовое описание */
	description?: string | null
	/** Вложенные сущности, подчинённые этой */
	children: BlockTreeInfo[]
}

/** Запись собщения */
export interface LogInfo {
	/**
	 * Идентификатор записи
	 * @format int64
	 */
	id: number
	/**
	 * Дата формата DateFormats.Long
	 * @minLength 1
	 */
	dateString: string
	/** Категория сообщения (к какому объекту относится) */
	category: LogCategory
	/** Степень важности сообщения */
	type: LogType
	/**
	 * Текст сообщеня
	 * @minLength 1
	 */
	text: string
	/** Ссылка на конкретный объект в случае, если это подразумевает категория */
	refId?: string | null
}

/** Категория, к которой относится сообщение */
export enum LogCategory {
	Core = 'Core',
	Database = 'Database',
	Collector = 'Collector',
	Api = 'Api',
	Calc = 'Calc',
	Source = 'Source',
	Tag = 'Tag',
	Http = 'Http',
	Users = 'Users',
	UserGroups = 'UserGroups',
}

/** Степень важности сообщения */
export enum LogType {
	Trace = 'Trace',
	Information = 'Information',
	Success = 'Success',
	Warning = 'Warning',
	Error = 'Error',
}

/** Информация о источнике */
export interface SourceInfo {
	/**
	 * Идентификатор источника в базе данных
	 * @format int32
	 */
	id: number
	/**
	 * Название источника
	 * @minLength 1
	 */
	name: string
	/** Произвольное описание источника */
	description?: string | null
	/** Используемый для получения данных адрес */
	address?: string | null
	/** Тип протокола, по которому запрашиваются данные */
	type: SourceType
}

/** Тип получения данных с источника */
export enum SourceType {
	Inopc = 'Inopc',
	Datalake = 'Datalake',
	Unknown = 'Unknown',
	Custom = 'Custom',
}

/** Информация о удалённой записи с данными источника */
export interface SourceItemInfo {
	/**
	 * Путь к данным в источнике
	 * @minLength 1
	 */
	path: string
	/** Тип данных */
	type: TagType
}

/** Информация о сопоставлении данных в источнике и в базе */
export interface SourceEntryInfo {
	/** Сопоставленная запись в источнике */
	itemInfo?: SourceItemInfo | null
	/** Сопоставленный тег в базе */
	tagInfo?: SourceTagInfo | null
}

/** Информация о теге, берущем данные из этого источника */
export interface SourceTagInfo {
	/**
	 * Идентификатор тега в локальной базе
	 * @format int32
	 */
	id: number
	/**
	 * Глобальное наименование тега
	 * @minLength 1
	 */
	name: string
	/**
	 * Путь к данным в источнике
	 * @minLength 1
	 */
	item: string
	/** Тип данных тега */
	type: TagType
}

/** Необходимые данные для создания тега */
export interface TagCreateRequest {
	/** Наименование тега. Если не указать, будет составлено автоматически */
	name?: string | null
	/** Тип значений тега */
	tagType: TagType
	/**
	 * Идентификатор источника данных
	 * @format int32
	 */
	sourceId?: number | null
	/** Путь к данным при использовании удалённого источника */
	sourceItem?: string | null
	/**
	 * Идентификатор сущности, к которой будет привязан новый тег
	 * @format int32
	 */
	blockId?: number | null
}

/** Информация о теге */
export interface TagInfo {
	/**
	 * Идентификатор тега в локальной базе
	 * @format int32
	 */
	id: number
	/**
	 * Имя тега
	 * @minLength 1
	 */
	name: string
	/** Произвольное описание тега */
	description?: string | null
	/** Тип данных тега */
	type: TagType
	/** Интервал опроса источника для получения нового значения */
	intervalInSeconds: number
	/**
	 * Идентификатор источника данных
	 * @format int32
	 */
	sourceId: number
	/** Тип данных источника */
	sourceType: SourceType
	/** Путь к данным в источнике */
	sourceItem?: string | null
	/** Имя используемого источника данных */
	sourceName?: string | null
	/** Формула, на основе которой вычисляется значение */
	formula?: string | null
	/** Применяется ли приведение числового значения тега к другой шкале */
	isScaling: boolean
	/**
	 * Меньший предел итоговой шкалы
	 * @format float
	 */
	minEu: number
	/**
	 * Больший предел итоговой шкалы
	 * @format float
	 */
	maxEu: number
	/**
	 * Меньший предел шкалы исходного значения
	 * @format float
	 */
	minRaw: number
	/**
	 * Больший предел шкалы исходного значения
	 * @format float
	 */
	maxRaw: number
	/** Входные переменные для формулы, по которой рассчитывается значение */
	formulaInputs: TagInputInfo[]
}

/** Тег, используемый как входной параметр в формуле */
export interface TagInputInfo {
	/**
	 * Идентификатор тега в локальной базе
	 * @format int32
	 */
	id: number
	/**
	 * Имя тега
	 * @minLength 1
	 */
	name: string
	/**
	 * Имя переменной, используемое в формуле
	 * @minLength 1
	 */
	variableName: string
}

/** Информации о теге, выступающем в качестве входящей переменной при составлении формулы */
export interface TagAsInputInfo {
	/**
	 * Идентификатор тега в локальной базе
	 * @format int32
	 */
	id: number
	/**
	 * Имя тега
	 * @minLength 1
	 */
	name: string
	/** Тип данных тега */
	type: TagType
}

/** Данные запроса для изменение тега */
export interface TagUpdateRequest {
	/**
	 * Новое имя тега
	 * @minLength 1
	 */
	name: string
	/** Новое описание */
	description?: string | null
	/** Новый тип данных */
	type: TagType
	/** Новый интервал получения значения */
	intervalInSeconds: number
	/**
	 * Источник данных
	 * @format int32
	 */
	sourceId: number
	/** Тип источника данных */
	sourceType: SourceType
	/** Путь к данными в источнике */
	sourceItem?: string | null
	/** Формула, по которой рассчитывается значение */
	formula?: string | null
	/** Применяется ли изменение шкалы значения */
	isScaling: boolean
	/**
	 * Меньший предел итоговой шкалы
	 * @format float
	 */
	minEu: number
	/**
	 * Больший предел итоговой шкалы
	 * @format float
	 */
	maxEu: number
	/**
	 * Меньший предел шкалы исходного значения
	 * @format float
	 */
	minRaw: number
	/**
	 * Больший предел шкалы исходного значения
	 * @format float
	 */
	maxRaw: number
	/** Входные переменные для формулы, по которой рассчитывается значение */
	formulaInputs: TagInputInfo[]
}

/** Данные запроса для создания группы пользователей */
export interface UserGroupCreateRequest {
	/**
	 * Название. Не может повторяться в рамках родительской группы
	 * @minLength 1
	 */
	name: string
	/**
	 * Идентификатор родительской группы
	 * @format guid
	 */
	parentGuid?: string | null
	/** Описание */
	description?: string | null
}

/** Информация о группе пользователей */
export interface UserGroupInfo {
	/**
	 * Идентификатор группы
	 * @format guid
	 * @minLength 1
	 */
	guid: string
	/**
	 * Название группы
	 * @minLength 1
	 */
	name: string
	/** Произвольное описание группы */
	description?: string | null
	/**
	 * Идентификатор группы, в которой располагается эта группа
	 * @format guid
	 */
	parentGroupGuid?: string | null
}

/** Информация о группе пользователей в иерархическом представлении */
export type UserGroupTreeInfo = UserGroupInfo & {
	/** Список подгрупп */
	children: UserGroupTreeInfo[]
	/**
	 * Идентификатор родительской группы
	 * @format guid
	 */
	parentGuid?: string | null
	/** Информация о родительской группе */
	parent?: UserGroupTreeInfo | null
}

/** Расширенная информация о группе пользователей, включающая вложенные группы и список пользователей */
export type UserGroupDetailedInfo = UserGroupInfo & {
	/** Список пользователей этой группы */
	users: UserGroupUsersInfo[]
	/** Список подгрупп этой группы */
	subgroups: UserGroupInfo[]
}

/** Информация о пользователей данной группы */
export interface UserGroupUsersInfo {
	/**
	 * Идентификатор пользователя
	 * @format guid
	 */
	guid?: string
	/** Уровень доступа пользователя в группе */
	accessType?: AccessType
	/** Полное имя пользователя */
	fullName?: string | null
}

/** Уровень доступа */
export enum AccessType {
	NoAccess = 'NoAccess',
	Viewer = 'Viewer',
	User = 'User',
	Admin = 'Admin',
	NotSet = 'NotSet',
}

/** Данные запроса для изменения группы пользователей */
export type UserGroupUpdateRequest = UserGroupCreateRequest & {
	/** Базовый уровень доступа участников и под-групп */
	accessType?: AccessType
	/** Список пользователей, которые включены в эту группу */
	users: UserGroupUsersInfo[]
	/** Список групп, которые включены в эту группу */
	groups: UserGroupInfo[]
}

/** Информация о пользователе, взятая из Keycloak */
export interface UserKeycloakInfo {
	/**
	 * Идентификатор пользователя в сервере Keycloak
	 * @format guid
	 * @minLength 1
	 */
	keycloakGuid: string
	/**
	 * Имя для входа
	 * @minLength 1
	 */
	login: string
	/**
	 * Полное имя пользователя
	 * @minLength 1
	 */
	fullName: string
}

/** Информация о аутентифицированном пользователе */
export interface UserAuthInfo {
	/**
	 * Идентификатор пользователя
	 * @format guid
	 * @minLength 1
	 */
	guid: string
	/**
	 * Имя для входа
	 * @minLength 1
	 */
	login: string
	/**
	 * Идентификатор сессии
	 * @minLength 1
	 */
	token: string
	/** Глобальный уровень доступа */
	globalAccessType: AccessType
	/** Список правил доступа */
	rights: UserAccessRightsInfo[]
}

/** Правило доступа пользователя */
export interface UserAccessRightsInfo {
	/** Является ли это правило глобальным */
	isGlobal: boolean
	/**
	 * Идентификатор тега, на который распространяется это правило
	 * @format int32
	 */
	tagId?: number | null
	/**
	 * Идентификатор источника, на который распространяется это правило
	 * @format int32
	 */
	sourceId?: number | null
	/**
	 * Идентификатор блока, на который распространяется это правило
	 * @format int32
	 */
	blockId?: number | null
	/** Уровень доступа на основе этого правила */
	accessType: AccessType
}

/** Информация при аутентификации локальной учетной записи */
export interface UserLoginPass {
	/**
	 * Имя для входа
	 * @minLength 1
	 */
	login: string
	/**
	 * Пароль
	 * @minLength 1
	 */
	password: string
}

/** Данные запроса на создание пользователя */
export interface UserCreateRequest {
	/**
	 * Имя для входа
	 * @minLength 1
	 */
	login: string
	/** Полное имя пользователя */
	fullName?: string | null
	/** Используемый пароль */
	password?: string | null
	/** Адрес статической точки, откуда будет осуществляться доступ */
	staticHost?: string | null
	/** Глобальный уровень доступа */
	accessType: AccessType
}

/** Информация о пользователе */
export interface UserInfo {
	/**
	 * Идентификатор пользователя
	 * @format guid
	 * @minLength 1
	 */
	guid: string
	/**
	 * Имя для входа
	 * @minLength 1
	 */
	login: string
	/** Полное имя */
	fullName?: string | null
	/** Глобальные уровень доступа */
	accessType: AccessType
	/** Тип учётной записи */
	type: UserType
	/**
	 * Идентификатор пользователя в сервере Keycloak
	 * @format guid
	 */
	keycloakGuid?: string | null
	/** Список групп, в которые входит пользователь */
	userGroups: UserGroupsInfo[]
}

/** Тип учётной записи */
export enum UserType {
	Local = 'Local',
	Static = 'Static',
	Keycloak = 'Keycloak',
}

/** Информация о принадлежности пользователя к группе */
export interface UserGroupsInfo {
	/**
	 * Идентификатор группы
	 * @format guid
	 * @minLength 1
	 */
	guid: string
	/**
	 * Имя группы
	 * @minLength 1
	 */
	name: string
}

/** Расширенная информация о пользователе, включающая данные для аутентификации */
export type UserDetailInfo = UserInfo & {
	/** Хэш, по которому проверяется доступ */
	hash?: string | null
	/** Адрес статического узла, с которого идёт доступ */
	staticHost?: string | null
}

/** Данные запроса для изменения учетной записи */
export interface UserUpdateRequest {
	/**
	 * Новое имя для входа
	 * @minLength 1
	 */
	login: string
	/** Новый адрес статической точки, из которой будет разрешен доступ */
	staticHost?: string | null
	/** Новый пароль */
	password?: string | null
	/** Новое полное имя */
	fullName?: string | null
	/** Новый глобальный уровень доступа */
	accessType: AccessType
	/** Нужно ли создать новый ключ для статичной учетной записи */
	createNewStaticHash: boolean
	/**
	 * Идентификатор пользователя в сервере Keycloak
	 * @format guid
	 */
	keycloakGuid?: string | null
}

/** Ответ на запрос для получения значений, характеризующий запрошенный тег и его значения */
export interface ValuesResponse {
	/**
	 * Идентификатор тега в локальной базе
	 * @format int32
	 */
	id: number
	/**
	 * Имя тега
	 * @minLength 1
	 */
	tagName: string
	/** Тип данных */
	type: TagType
	/** Применённый тип агрегирования */
	func: AggregationFunc
	/** Список значений */
	values: ValueRecord[]
}

/** Тип агрегирования данных */
export enum AggregationFunc {
	List = 'List',
	Sum = 'Sum',
	Avg = 'Avg',
	Min = 'Min',
	Max = 'Max',
}

/** Запись о значении */
export interface ValueRecord {
	/**
	 * Дата, на которую значение актуально
	 * @format date-time
	 * @minLength 1
	 */
	date: string
	/**
	 * Строковое представление даты
	 * @minLength 1
	 */
	dateString: string
	/** Значение */
	value: any
	/** Достоверность значения */
	quality: TagQuality
	/** Характеристика хранения значения */
	using: TagUsing
}

/** Достоверность значения */
export enum TagQuality {
	Bad = 'Bad',
	BadNoConnect = 'Bad_NoConnect',
	BadNoValues = 'Bad_NoValues',
	BadManualWrite = 'Bad_ManualWrite',
	Good = 'Good',
	GoodManualWrite = 'Good_ManualWrite',
	Unknown = 'Unknown',
}

/** Характеристика значения */
export enum TagUsing {
	Initial = 'Initial',
	Basic = 'Basic',
	Aggregated = 'Aggregated',
	Continuous = 'Continuous',
	Outdated = 'Outdated',
	NotFound = 'NotFound',
}

/** Данные запроса для получения значений */
export interface ValuesRequest {
	/** Список локальных идентификаторов тегов */
	tags?: number[] | null
	/** Список имён тегов */
	tagNames?: string[] | null
	/**
	 * Дата, с которой (включительно) нужно получить значения. По умолчанию - начало текущих суток
	 * @format date-time
	 */
	old?: string | null
	/**
	 * Дата, по которую (включительно) нужно получить значения. По умолчанию - текущая дата
	 * @format date-time
	 */
	young?: string | null
	/**
	 * Дата, на которую (по точному соответствию) нужно получить значения. По умолчанию - не используется
	 * @format date-time
	 */
	exact?: string | null
	/**
	 * Шаг времени, по которому нужно разбить значения. Если не задан, будут оставлены записи о изменениях значений
	 * @format int32
	 */
	resolution?: number | null
	/** Тип агрегирования значений, который нужно применить к этому запросу. По умолчанию - список */
	func?: AggregationFunc | null
}

/** Данные запроса на ввод значения */
export interface ValueWriteRequest {
	/**
	 * Идентификатор тега в локальной базе
	 * @format int32
	 */
	tagId?: number | null
	/** Имя тега */
	tagName?: string | null
	/** Новое значение */
	value?: any
	/**
	 * Дата, на которую будет записано значение
	 * @format date-time
	 */
	date?: string | null
	/** Флаг достоверности нового значения */
	tagQuality?: TagQuality | null
}

export type ValuesGetPayload = ValuesRequest[]

export type ValuesWritePayload = ValueWriteRequest[]
