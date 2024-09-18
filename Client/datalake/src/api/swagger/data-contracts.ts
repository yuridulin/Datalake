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

/** Информация о блоке */
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
	/** Информация о родительском блоке */
	parent?: BlockParentInfo | null
	/** Список дочерних блоков */
	children: BlockChildInfo[]
	/** Список статических свойств блока */
	properties: BlockPropertyInfo[]
	/** Список прикреплённых тегов */
	tags: BlockTagInfo[]
}

/** Информация о родительском блоке */
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

/** Информация о дочернем блоке */
export type BlockChildInfo = BlockRelationInfo & object

/** Информация о статическом свойстве блока */
export type BlockPropertyInfo = BlockRelationInfo & {
	/** Тип значения свойства */
	type: TagType
	/**
	 * Значение свойства
	 * @minLength 1
	 */
	value: string
}

/**
 * Тип данных
 *
 * 0 = String
 * 1 = Number
 * 2 = Boolean
 */
export enum TagType {
	String = 0,
	Number = 1,
	Boolean = 2,
}

/** Информация о закреплённом теге */
export type BlockTagInfo = BlockRelationInfo & {
	/**
	 * Идентификатор тега
	 * @format guid
	 * @minLength 1
	 */
	guid: string
	/** Тип поля блока для этого тега */
	relation?: BlockTagRelation
	/** Тип значений тега */
	tagType?: TagType
	/** Свое имя тега в общем списке */
	tagName?: string
}

/**
 * Тип связи тега и блока
 *
 * 0 = Static
 * 1 = Input
 * 2 = Output
 */
export enum BlockTagRelation {
	Static = 0,
	Input = 1,
	Output = 2,
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

/**
 * Категория, к которой относится сообщение
 *
 * 0 = Core
 * 10 = Database
 * 20 = Collector
 * 30 = Api
 * 40 = Calc
 * 50 = Source
 * 60 = Tag
 * 70 = Http
 * 80 = Users
 * 90 = UserGroups
 */
export enum LogCategory {
	Core = 0,
	Database = 10,
	Collector = 20,
	Api = 30,
	Calc = 40,
	Source = 50,
	Tag = 60,
	Http = 70,
	Users = 80,
	UserGroups = 90,
}

/**
 * Степень важности сообщения
 *
 * 0 = Trace
 * 1 = Information
 * 2 = Success
 * 3 = Warning
 * 4 = Error
 */
export enum LogType {
	Trace = 0,
	Information = 1,
	Success = 2,
	Warning = 3,
	Error = 4,
}

/** Информация о настройках приложения, задаваемых через UI */
export interface SettingsInfo {
	/**
	 * Адрес сервера EnergoId, к которому выполняются подключения, включая порт при необходимости
	 *
	 * Протокол будет выбран на основе того, какой используется в клиенте в данный момент
	 */
	energoIdHost?: string
	/** Название клиента EnergoId, через который идет аутентификация */
	energoIdClient?: string
	/** Конечная точка сервиса, который отдает информацию о пользователях EnergoId */
	energoIdApi?: string
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

/**
 * Тип получения данных с источника
 *
 * 0 = Inopc
 * 1 = Datalake
 * 2 = DatalakeCore_v1
 * -100 = Unknown
 * -1 = Custom
 */
export enum SourceType {
	Inopc = 0,
	Datalake = 1,
	DatalakeCoreV1 = 2,
	Unknown = -100,
	Custom = -1,
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
	/** Значение, прочитанное с источника при опросе */
	value?: any
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
	 * Идентификатор тега
	 * @format guid
	 * @minLength 1
	 */
	guid: string
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
	 * Глобальный идентификатор тега
	 * @format guid
	 * @minLength 1
	 */
	guid: string
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
	 * Идентификатор тега
	 * @format guid
	 * @minLength 1
	 */
	guid: string
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
	 * Идентификатор тега
	 * @format guid
	 * @minLength 1
	 */
	guid: string
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

/**
 * Уровень доступа
 *
 * 0 = NoAccess
 * 5 = Viewer
 * 10 = User
 * 100 = Admin
 * -100 = NotSet
 */
export enum AccessType {
	NoAccess = 0,
	Viewer = 5,
	User = 10,
	Admin = 100,
	NotSet = -100,
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

/** Данные с сервиса "EnergoID" */
export interface EnergoIdInfo {
	/** Список пользователей */
	energoIdUsers: UserEnergoIdInfo[]
	/** Есть ли связь с сервисом "EnergoID" */
	connected: boolean
}

/** Информация о пользователе, взятая из Keycloak */
export interface UserEnergoIdInfo {
	/**
	 * Идентификатор пользователя в сервере Keycloak
	 * @format guid
	 * @minLength 1
	 */
	energoIdGuid: string
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
	 * Имя пользователя
	 * @minLength 1
	 */
	fullName: string
	/**
	 * Идентификатор сессии
	 * @minLength 1
	 */
	token: string
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
	/** Имя для входа */
	login?: string | null
	/** Полное имя пользователя */
	fullName?: string | null
	/** Глобальный уровень доступа */
	accessType: AccessType
	/** Тип учетной записи */
	type: UserType
	/** Используемый пароль */
	password?: string | null
	/** Адрес статической точки, откуда будет осуществляться доступ */
	staticHost?: string | null
	/**
	 * Идентификатор связанной учетной записи в сервере EnergoId
	 * @format guid
	 */
	energoIdGuid?: string | null
}

/**
 * Тип учётной записи
 *
 * 1 = Local
 * 2 = Static
 * 3 = EnergoId
 */
export enum UserType {
	Local = 1,
	Static = 2,
	EnergoId = 3,
}

/** Информация о пользователе */
export interface UserInfo {
	/**
	 * Идентификатор пользователя
	 * @format guid
	 * @minLength 1
	 */
	guid: string
	/** Имя для входа */
	login?: string | null
	/** Полное имя */
	fullName?: string | null
	/** Глобальный уровень доступа */
	accessType: AccessType
	/** Тип учётной записи */
	type: UserType
	/**
	 * Идентификатор пользователя в сервере EnergoId
	 * @format guid
	 */
	energoIdGuid?: string | null
	/** Список групп, в которые входит пользователь */
	userGroups: UserGroupsInfo[]
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
	/** Новое имя для входа */
	login?: string | null
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
	 * Идентификатор пользователя в сервере EnergoId
	 * @format guid
	 */
	energoIdGuid?: string | null
	/** Тип учетной записи */
	type: UserType
}

/** Ответ на запрос для получения значений, включающий обработанные теги и идентификатор запроса */
export interface ValuesResponse {
	/**
	 * Идентификатор запроса, который будет передан в соответствующий объект ответа
	 * @minLength 1
	 */
	requestKey: string
	/** Список глобальных идентификаторов тегов */
	tags: ValuesTagResponse[]
}

/** Ответ на запрос для получения значений, характеризующий запрошенный тег и его значения */
export interface ValuesTagResponse {
	/**
	 * Идентификатор тега в локальной базе
	 * @format int32
	 */
	id: number
	/**
	 * Глобальный идентификатор тега
	 * @format guid
	 * @minLength 1
	 */
	guid: string
	/**
	 * Полное наименование тега
	 * @minLength 1
	 */
	name: string
	/** Тип данных */
	type: TagType
	/** Список значений */
	values: ValueRecord[]
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
	value?: any
	/** Достоверность значения */
	quality: TagQuality
}

/**
 * Достоверность значения
 *
 * 0 = Bad
 * 4 = Bad_NoConnect
 * 8 = Bad_NoValues
 * 26 = Bad_ManualWrite
 * 192 = Good
 * 216 = Good_ManualWrite
 * -1 = Unknown
 */
export enum TagQuality {
	Bad = 0,
	BadNoConnect = 4,
	BadNoValues = 8,
	BadManualWrite = 26,
	Good = 192,
	GoodManualWrite = 216,
	Unknown = -1,
}

/** Данные запроса для получения значений */
export interface ValuesRequest {
	/**
	 * Идентификатор запроса, который будет передан в соответствующий объект ответа
	 * @minLength 1
	 */
	requestKey: string
	/** Список глобальных идентификаторов тегов */
	tags?: string[] | null
	/** Список локальных идентификаторов тегов */
	tagsId?: number[] | null
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

/**
 * Тип агрегирования данных
 *
 * 0 = List
 * 1 = Sum
 * 2 = Avg
 * 3 = Min
 * 4 = Max
 */
export enum AggregationFunc {
	List = 0,
	Sum = 1,
	Avg = 2,
	Min = 3,
	Max = 4,
}

/** Данные запроса на ввод значения */
export interface ValueWriteRequest {
	/**
	 * Глобальные идентификатор тега
	 * @format guid
	 */
	guid?: string | null
	/**
	 * Идентификатор тега в локальной базе
	 * @format int32
	 */
	id?: number | null
	/** Наименование тега */
	name?: string | null
	/** Новое значение */
	value?: any
	/**
	 * Дата, на которую будет записано значение
	 * @format date-time
	 */
	date?: string | null
	/** Флаг достоверности нового значения */
	quality?: TagQuality | null
}

export type ValuesGetPayload = ValuesRequest[]

export type ValuesWritePayload = ValueWriteRequest[]
