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

/** Информация о разрешении пользователя или группы на доступ к какому-либо объекту */
export type AccessRightsInfo = AccessRightsForOneInfo & {
	/** Информация о группе пользователей */
	userGroup?: UserGroupSimpleInfo | null
	/** Информация о пользователе */
	user?: UserSimpleInfo | null
}

/** Базовая информация о группе пользователей */
export interface UserGroupSimpleInfo {
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
	/** Правило доступа */
	accessRule: AccessRuleInfo
}

/** Информация о уровне доступа с указанием на правило, на основе которого получен этот доступ */
export interface AccessRuleInfo {
	/**
	 * Идентификатор правила доступа
	 * @format int32
	 */
	ruleId: number
	/** Уровень доступа */
	accessType: AccessType
}

/**
 * Уровень доступа
 *
 * 0 = NoAccess
 * 5 = Viewer
 * 10 = Editor
 * 50 = Manager
 * 100 = Admin
 * -100 = NotSet
 */
export enum AccessType {
	NoAccess = 0,
	Viewer = 5,
	Editor = 10,
	Manager = 50,
	Admin = 100,
	NotSet = -100,
}

/** Базовая информация о пользователе */
export interface UserSimpleInfo {
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
	/** Правило доступа */
	accessRule: AccessRuleInfo
}

/** Информация о разрешении субьекта на доступ к объекту */
export type AccessRightsForOneInfo = AccessRightsSimpleInfo & {
	/** Тег, на который выдано разрешение */
	tag?: TagSimpleInfo | null
	/** Блок, на который выдано разрешение */
	block?: BlockSimpleInfo | null
	/** Источник, на который выдано разрешение */
	source?: SourceSimpleInfo | null
}

/** Базовая информация о теге, достаточная, чтобы на него сослаться */
export interface TagSimpleInfo {
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
	/** Тип данных тега */
	type: TagType
	/** Частота записи тега */
	frequency: TagFrequency
	/** Тип данных источника */
	sourceType: SourceType
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

/**
 * Частота записи значения
 *
 * 0 = NotSet
 * 1 = ByMinute
 * 2 = ByHour
 * 3 = ByDay
 */
export enum TagFrequency {
	NotSet = 0,
	ByMinute = 1,
	ByHour = 2,
	ByDay = 3,
}

/**
 * Тип получения данных с источника
 *
 * 0 = System
 * 1 = Inopc
 * 2 = Datalake
 * 3 = Datalake_v2
 * -666 = NotSet
 * -2 = Manual
 * -1 = Calculated
 */
export enum SourceType {
	System = 0,
	Inopc = 1,
	Datalake = 2,
	DatalakeV2 = 3,
	NotSet = -666,
	Manual = -2,
	Calculated = -1,
}

/** Базовая информация о блоке, достаточная, чтобы на него сослаться */
export interface BlockSimpleInfo {
	/**
	 * Идентификатор
	 * @format int32
	 */
	id: number
	/**
	 * Глобальный идентификатор
	 * @format guid
	 * @minLength 1
	 */
	guid: string
	/**
	 * Наименование
	 * @minLength 1
	 */
	name: string
}

/** Базовая информация о источнике, достаточная, чтобы на него сослаться */
export interface SourceSimpleInfo {
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
}

/** Общая информация о разрешении */
export interface AccessRightsSimpleInfo {
	/**
	 * Идентификатор разрешения
	 * @format int32
	 */
	id: number
	/** Тип доступа */
	accessType: AccessType
	/** Является ли разрешение глобальным */
	isGlobal: boolean
}

/** Измененное разрешение, которое нужно обновить в БД */
export interface AccessRightsApplyRequest {
	/**
	 * Идентификатор пользователя, которому выдается разрешение
	 * @format guid
	 */
	userGuid?: string | null
	/**
	 * Идентификатор группы пользователей, которой выдается разрешение
	 * @format guid
	 */
	userGroupGuid?: string | null
	/**
	 * Идентификатор источника, на который выдается разрешение
	 * @format int32
	 */
	sourceId?: number | null
	/**
	 * Идентификатор блока, на который выдается разрешение
	 * @format int32
	 */
	blockId?: number | null
	/**
	 * Идентификатор тега, на который выдается разрешение
	 * @format int32
	 */
	tagId?: number | null
	/** Список прав доступа */
	rights: AccessRightsIdInfo[]
}

/** Измененное разрешение, которое нужно обновить в БД */
export interface AccessRightsIdInfo {
	/**
	 * Идентификатор существующего разрешения
	 * @format int32
	 */
	id?: number | null
	/** Уровень доступа */
	accessType: AccessType
	/**
	 * Идентификатор пользователя, которому выдается разрешение
	 * @format guid
	 */
	userGuid?: string | null
	/**
	 * Идентификатор группы пользователей, которой выдается разрешение
	 * @format guid
	 */
	userGroupGuid?: string | null
	/**
	 * Идентификатор источника, на который выдается разрешение
	 * @format int32
	 */
	sourceId?: number | null
	/**
	 * Идентификатор блока, на который выдается разрешение
	 * @format int32
	 */
	blockId?: number | null
	/**
	 * Идентификатор тега, на который выдается разрешение
	 * @format int32
	 */
	tagId?: number | null
}

/** Информация о блоке */
export type BlockFullInfo = BlockWithTagsInfo & {
	/** Информация о родительском блоке */
	parent?: BlockParentInfo | null
	/** Список дочерних блоков */
	children: BlockChildInfo[]
	/** Список статических свойств блока */
	properties: BlockPropertyInfo[]
	/** Список прав доступа, которые действуют на этот блок */
	accessRights: AccessRightsForObjectInfo[]
	/** Список родительских блоков */
	adults: BlockTreeInfo[]
}

/** Информация о родительском блоке */
export type BlockParentInfo = BlockNestedItem & object

/** Информация о вложенном объекте */
export interface BlockNestedItem {
	/**
	 * Идентификатор
	 * @format int32
	 */
	id?: number
	/** Наименование */
	name?: string
}

/** Информация о дочернем блоке */
export type BlockChildInfo = BlockNestedItem & object

/** Информация о статическом свойстве блока */
export type BlockPropertyInfo = BlockNestedItem & {
	/** Тип значения свойства */
	type: TagType
	/**
	 * Значение свойства
	 * @minLength 1
	 */
	value: string
}

/** Информация о разрешении на объект для субьекта */
export type AccessRightsForObjectInfo = AccessRightsSimpleInfo & {
	/** Информация о группе пользователей */
	userGroup?: UserGroupSimpleInfo | null
	/** Информация о пользователе */
	user?: UserSimpleInfo | null
}

/** Информация о блоке в иерархическом представлении */
export type BlockTreeInfo = BlockWithTagsInfo & {
	/** Вложенные блоки */
	children: BlockTreeInfo[]
	/**
	 * Полное имя блока, включающее имена всех родительских блоков по иерархии через "."
	 * @minLength 1
	 */
	fullName: string
}

/** Информация о блоке */
export type BlockWithTagsInfo = BlockSimpleInfo & {
	/**
	 * Идентификатор родительского блока
	 * @format int32
	 */
	parentId?: number | null
	/** Текстовое описание */
	description?: string | null
	/** Уровень доступа к блоку */
	accessRule: AccessRuleInfo
	/** Список прикреплённых тегов */
	tags: BlockNestedTagInfo[]
}

/** Информация о закреплённом теге */
export type BlockNestedTagInfo = TagSimpleInfo & {
	/** Тип поля блока для этого тега */
	relation: BlockTagRelation
	/**
	 * Свое имя тега в общем списке
	 * @minLength 1
	 */
	localName: string
	/**
	 * Идентификатор источника данных
	 * @format int32
	 */
	sourceId: number
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

/** Новая информация о блоке */
export interface BlockUpdateRequest {
	/**
	 * Новое название
	 * @minLength 1
	 */
	name: string
	/** Новое описание */
	description?: string | null
	/** Новый список закрепленных тегов */
	tags: AttachedTag[]
}

/** Информация о закрепленном теге */
export interface AttachedTag {
	/**
	 * Локальный идентификатор тега
	 * @format int32
	 */
	id: number
	/**
	 * Название поля в блоке, которому соответствует тег
	 * @minLength 1
	 */
	name: string
	/** Тип поля блока */
	relation: BlockTagRelation
}

/** Информация о источнике */
export type SourceInfo = SourceSimpleInfo & {
	/** Произвольное описание источника */
	description?: string | null
	/** Используемый для получения данных адрес */
	address?: string | null
	/** Тип протокола, по которому запрашиваются данные */
	type: SourceType
	/** Правило доступа */
	accessRule?: AccessRuleInfo
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
 * 100 = Bad_LOCF
 * 192 = Good
 * 200 = Good_LOCF
 * 216 = Good_ManualWrite
 * -1 = Unknown
 */
export enum TagQuality {
	Bad = 0,
	BadNoConnect = 4,
	BadNoValues = 8,
	BadManualWrite = 26,
	BadLOCF = 100,
	Good = 192,
	GoodLOCF = 200,
	GoodManualWrite = 216,
	Unknown = -1,
}

/** Информация о сопоставлении данных в источнике и в базе */
export interface SourceEntryInfo {
	/** Сопоставленная запись в источнике */
	itemInfo?: SourceItemInfo | null
	/** Сопоставленный тег в базе */
	tagInfo?: SourceTagInfo | null
}

/** Информация о теге, берущем данные из этого источника */
export type SourceTagInfo = TagSimpleInfo & {
	/**
	 * Путь к данным в источнике
	 * @minLength 1
	 */
	item: string
	/** Правило доступа */
	accessRule?: AccessRuleInfo
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
	/**
	 * Ссылка на конкретный объект в случае, если это подразумевает категория
	 *
	 * Теги, пользователи, группы пользователей: Guid
	 * Источники, блоки: int
	 */
	refId?: string | null
	/** Информация об авторе сообщения */
	author?: UserSimpleInfo | null
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
 * 100 = Blocks
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
	Blocks = 100,
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

/** Объект состояния источника */
export interface SourceState {
	/**
	 * Идентификатор источника
	 * @format int32
	 */
	sourceId: number
	/**
	 * Дата последней попытки подключиться
	 * @format date-time
	 * @minLength 1
	 */
	lastTry: string
	/**
	 * Дата последнего удачного подключения
	 * @format date-time
	 */
	lastConnection?: string | null
	/** Было ли соединение при последнем подключении */
	isConnected: boolean
}

/** Информация о настройках приложения, задаваемых через UI */
export interface SettingsInfo {
	/**
	 * Адрес сервера EnergoId, к которому выполняются подключения, включая порт при необходимости
	 *
	 * Протокол будет выбран на основе того, какой используется в клиенте в данный момент
	 * @minLength 1
	 */
	energoIdHost: string
	/**
	 * Название клиента EnergoId, через который идет аутентификация
	 * @minLength 1
	 */
	energoIdClient: string
	/**
	 * Конечная точка сервиса, который отдает информацию о пользователях EnergoId
	 * @minLength 1
	 */
	energoIdApi: string
	/**
	 * Пользовательское название базы данных
	 * @minLength 1
	 */
	instanceName: string
}

/** Информация о аутентифицированном пользователе */
export type UserAuthInfo = UserSimpleInfo & {
	/**
	 * Идентификатор сессии
	 * @minLength 1
	 */
	token: string
	/** Глобальный уровень доступа */
	globalAccessType: AccessType
	/** Список всех блоков с указанием доступа к ним */
	groups: Record<string, AccessRuleInfo>
	/** Список всех блоков с указанием доступа к ним */
	sources: Record<string, AccessRuleInfo>
	/** Список всех блоков с указанием доступа к ним */
	blocks: Record<string, AccessRuleInfo>
	/** Список всех тегов с указанием доступа к ним */
	tags: Record<string, AccessRuleInfo>
	/**
	 * Идентификатор пользователя внешнего приложения, который передается через промежуточную учетную запись
	 * @format guid
	 */
	underlyingUserGuid?: string | null
	/**
	 * Идентификатор пользователя в системе EnergoId
	 * @format guid
	 */
	energoId?: string | null
}

/** Информация о теге */
export type TagInfo = TagSimpleInfo & {
	/** Произвольное описание тега */
	description?: string | null
	/**
	 * Идентификатор источника данных
	 * @format int32
	 */
	sourceId: number
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
	/** Правило доступа */
	accessRule?: AccessRuleInfo
}

/** Тег, используемый как входной параметр в формуле */
export type TagInputInfo = TagSimpleInfo & {
	/**
	 * Имя переменной, используемое в формуле
	 * @minLength 1
	 */
	variableName: string
	/** Правило доступа */
	accessRule?: AccessRuleInfo
}

/** Необходимые данные для создания тега */
export interface TagCreateRequest {
	/** Наименование тега. Если не указать, будет составлено автоматически */
	name?: string | null
	/** Тип значений тега */
	tagType: TagType
	/** Частота записи тега */
	frequency: TagFrequency
	/**
	 * Идентификатор источника данных
	 * @format int32
	 */
	sourceId?: number | null
	/** Путь к данным при использовании удалённого источника */
	sourceItem?: string | null
	/**
	 * Идентификатор блока, к которому будет привязан новый тег
	 * @format int32
	 */
	blockId?: number | null
}

/** Информации о теге, выступающем в качестве входящей переменной при составлении формулы */
export type TagAsInputInfo = TagSimpleInfo & {
	/** Правило доступа */
	accessRule?: AccessRuleInfo
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
	/** Путь к данными в источнике */
	sourceItem?: string | null
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
	/**
	 * Источник данных
	 * @format int32
	 */
	sourceId: number
	/** Новый интервал получения значения */
	frequency: TagFrequency
	/** Формула, по которой рассчитывается значение */
	formula?: string | null
	/** Входные переменные для формулы, по которой рассчитывается значение */
	formulaInputs: TagUpdateInputRequest[]
}

/** Необходимая информация для привязки тега в качестве входного для  */
export interface TagUpdateInputRequest {
	/**
	 * Название переменной
	 * @minLength 1
	 */
	variableName: string
	/**
	 * Идентификатор закрепленного тега
	 * @format int32
	 */
	tagId: number
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
export type UserGroupInfo = UserGroupSimpleInfo & {
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
	/** Общий уровень доступа для всех участников группы */
	globalAccessType: AccessType
	/** Список пользователей этой группы */
	users: UserGroupUsersInfo[]
	/** Список подгрупп этой группы */
	subgroups: UserGroupSimpleInfo[]
	/** Разрешения, выданные на эту группу */
	accessRights: AccessRightsForOneInfo[]
}

/** Информация о пользователей данной группы */
export interface UserGroupUsersInfo {
	/**
	 * Идентификатор пользователя
	 * @format guid
	 * @minLength 1
	 */
	guid: string
	/** Уровень доступа пользователя в группе */
	accessType: AccessType
	/** Полное имя пользователя */
	fullName?: string | null
}

/** Данные запроса для изменения группы пользователей */
export type UserGroupUpdateRequest = UserGroupCreateRequest & {
	/** Базовый уровень доступа участников и под-групп */
	accessType: AccessType
	/** Список пользователей, которые включены в эту группу */
	users: UserGroupUsersInfo[]
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
export type UserInfo = UserSimpleInfo & {
	/** Имя для входа */
	login?: string | null
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
	userGroups: UserGroupSimpleInfo[]
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
export type ValuesTagResponse = TagSimpleInfo & {
	/** Список значений */
	values: ValueRecord[]
	/** Флаг, говорящий о недостаточности доступа для записи у пользователя */
	noAccess?: boolean | null
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
	 * Глобальный идентификатор тега
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
