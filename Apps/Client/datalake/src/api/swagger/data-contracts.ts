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
 * Период, за который берутся необходимые для расчета агрегированных значений данные
 *
 * 1 = Minute
 * 2 = Hour
 * 3 = Day
 */
export enum AggregationPeriod {
  Minute = 1,
  Hour = 2,
  Day = 3,
}

/**
 * Способ получения агрегированного значения
 *
 * 1 = Sum
 * 2 = Average
 */
export enum TagAggregation {
  Sum = 1,
  Average = 2,
}

/**
 * Способ вычисления значения
 *
 * 1 = Formula
 * 2 = Thresholds
 */
export enum TagCalculation {
  Formula = 1,
  Thresholds = 2,
}

/**
 * Достоверность значения
 *
 * 0 = Bad
 * 4 = Bad_NoConnect
 * 8 = Bad_NoValues
 * 12 = Bad_CalcError
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
  BadCalcError = 12,
  BadManualWrite = 26,
  BadLOCF = 100,
  Good = 192,
  GoodLOCF = 200,
  GoodManualWrite = 216,
  Unknown = -1,
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

/**
 * Тип получения данных с источника
 *
 * 0 = System
 * 1 = Inopc
 * 2 = Datalake
 * 3 = Datalake_v2
 * -666 = NotSet
 * -3 = Aggregated
 * -2 = Manual
 * -1 = Calculated
 */
export enum SourceType {
  /** Системные теги с данными о текущей работе различных частей приложения */
  System = 0,
  Inopc = 1,
  Datalake = 2,
  DatalakeV2 = 3,
  /** Заглушка для неопределённого источника */
  NotSet = -666,
  /** Теги, значения которых считаются на стороне БД как агрегированные значения тега-источника за прошедший период */
  Aggregated = -3,
  /** Пользовательские теги с ручным вводом значений в произвольный момент времени */
  Manual = -2,
  /** Пользовательские теги, значения которых вычисляются по формулам на основе значений других тегов */
  Calculated = -1,
}

/**
 * Частота записи/чтения значения
 *
 * 0 = NotSet
 * 1 = Minute
 * 2 = Hour
 * 3 = Day
 * 4 = HalfHour
 * 5 = Week
 * 6 = Month
 * 7 = Second
 * 8 = Minute3
 * 9 = Minute5
 * 10 = Minute10
 * 11 = Minute15
 * 12 = Minute20
 */
export enum TagResolution {
  NotSet = 0,
  Minute = 1,
  Hour = 2,
  Day = 3,
  HalfHour = 4,
  Week = 5,
  Month = 6,
  Second = 7,
  Minute3 = 8,
  Minute5 = 9,
  Minute10 = 10,
  Minute15 = 11,
  Minute20 = 12,
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
 * Уровень доступа
 *
 * 0 = NotSet
 * 1 = Viewer
 * 2 = Editor
 * 3 = Manager
 * 4 = NoAccess
 * 5 = Admin
 */
export enum AccessType {
  NotSet = 0,
  Viewer = 1,
  Editor = 2,
  Manager = 3,
  NoAccess = 4,
  Admin = 5,
}

/** Информация о разрешении пользователя или группы на доступ к какому-либо объекту */
export type AccessRightsInfo = AccessRightsForOneInfo & {
  /** Информация о группе пользователей */
  userGroup?: UserGroupSimpleInfo | null;
  /** Информация о пользователе */
  user?: UserSimpleInfo | null;
};

/** Базовая информация о группе пользователей */
export interface UserGroupSimpleInfo {
  /**
   * Идентификатор группы
   * @format guid
   * @minLength 1
   */
  guid: string;
  /**
   * Название группы
   * @minLength 1
   */
  name: string;
  /** Правило доступа */
  accessRule: AccessRuleInfo;
}

/** Информация о уровне доступа с указанием на правило, на основе которого получен этот доступ */
export interface AccessRuleInfo {
  /**
   * Идентификатор правила доступа
   * @format int32
   */
  ruleId: number;
  /** Уровень доступа */
  access: AccessType;
}

/** Базовая информация о пользователе */
export interface UserSimpleInfo {
  /**
   * Идентификатор пользователя
   * @format guid
   * @minLength 1
   */
  guid: string;
  /**
   * Имя пользователя
   * @minLength 1
   */
  fullName: string;
  /** Правило доступа */
  accessRule: AccessRuleInfo;
}

/** Информация о разрешении субьекта на доступ к объекту */
export type AccessRightsForOneInfo = AccessRightsSimpleInfo & {
  /** Тег, на который выдано разрешение */
  tag?: TagSimpleInfo | null;
  /** Блок, на который выдано разрешение */
  block?: BlockSimpleInfo | null;
  /** Источник, на который выдано разрешение */
  source?: SourceSimpleInfo | null;
};

/** Базовая информация о теге, достаточная, чтобы на него сослаться */
export interface TagSimpleInfo {
  /**
   * Идентификатор тега в локальной базе
   * @format int32
   */
  id: number;
  /**
   * Глобальный идентификатор тега
   * @format guid
   * @minLength 1
   */
  guid: string;
  /**
   * Имя тега
   * @minLength 1
   */
  name: string;
  /** Тип данных тега */
  type: TagType;
  /** Частота записи тега */
  resolution: TagResolution;
  /** Тип данных источника */
  sourceType: SourceType;
}

/** Базовая информация о блоке, достаточная, чтобы на него сослаться */
export interface BlockSimpleInfo {
  /**
   * Идентификатор
   * @format int32
   */
  id: number;
  /**
   * Глобальный идентификатор
   * @format guid
   * @minLength 1
   */
  guid: string;
  /**
   * Наименование
   * @minLength 1
   */
  name: string;
}

/** Базовая информация о источнике, достаточная, чтобы на него сослаться */
export interface SourceSimpleInfo {
  /**
   * Идентификатор источника в базе данных
   * @format int32
   */
  id: number;
  /**
   * Название источника
   * @minLength 1
   */
  name: string;
}

/** Общая информация о разрешении */
export interface AccessRightsSimpleInfo {
  /**
   * Идентификатор разрешения
   * @format int32
   */
  id: number;
  /** Тип доступа */
  accessType: AccessType;
  /** Является ли разрешение глобальным */
  isGlobal: boolean;
}

/** Измененное разрешение, которое нужно обновить в БД */
export interface AccessRightsApplyRequest {
  /**
   * Идентификатор пользователя, которому выдается разрешение
   * @format guid
   */
  userGuid?: string | null;
  /**
   * Идентификатор группы пользователей, которой выдается разрешение
   * @format guid
   */
  userGroupGuid?: string | null;
  /**
   * Идентификатор источника, на который выдается разрешение
   * @format int32
   */
  sourceId?: number | null;
  /**
   * Идентификатор блока, на который выдается разрешение
   * @format int32
   */
  blockId?: number | null;
  /**
   * Идентификатор тега, на который выдается разрешение
   * @format int32
   */
  tagId?: number | null;
  /** Список прав доступа */
  rights: AccessRightsIdInfo[];
}

/** Измененное разрешение, которое нужно обновить в БД */
export interface AccessRightsIdInfo {
  /**
   * Идентификатор существующего разрешения
   * @format int32
   */
  id?: number | null;
  /** Уровень доступа */
  accessType: AccessType;
  /**
   * Идентификатор пользователя, которому выдается разрешение
   * @format guid
   */
  userGuid?: string | null;
  /**
   * Идентификатор группы пользователей, которой выдается разрешение
   * @format guid
   */
  userGroupGuid?: string | null;
  /**
   * Идентификатор источника, на который выдается разрешение
   * @format int32
   */
  sourceId?: number | null;
  /**
   * Идентификатор блока, на который выдается разрешение
   * @format int32
   */
  blockId?: number | null;
  /**
   * Идентификатор тега, на который выдается разрешение
   * @format int32
   */
  tagId?: number | null;
}

/** Информация о блоке */
export type BlockWithTagsInfo = BlockSimpleInfo & {
  /**
   * Идентификатор родительского блока
   * @format int32
   */
  parentId?: number | null;
  /** Текстовое описание */
  description?: string | null;
  /** Уровень доступа к блоку */
  accessRule: AccessRuleInfo;
  /** Список прикреплённых тегов */
  tags: BlockNestedTagInfo[];
};

/** Информация о закреплённом теге */
export type BlockNestedTagInfo = TagSimpleInfo & {
  /**
   * Идентификатор связи
   * @format int32
   */
  relationId: number;
  /** Тип поля блока для этого тега */
  relationType: BlockTagRelation;
  /**
   * Свое имя тега в общем списке
   * @minLength 1
   */
  localName: string;
  /**
   * Идентификатор источника данных
   * @format int32
   */
  sourceId: number;
};

/** Информация о блоке */
export type BlockFullInfo = BlockWithTagsInfo & {
  /** Информация о родительском блоке */
  parent?: BlockParentInfo | null;
  /** Список дочерних блоков */
  children: BlockChildInfo[];
  /** Список статических свойств блока */
  properties: BlockPropertyInfo[];
  /** Список прав доступа, которые действуют на этот блок */
  accessRights: AccessRightsForObjectInfo[];
  /** Список родительских блоков */
  adults: BlockTreeInfo[];
};

/** Информация о родительском блоке */
export type BlockParentInfo = BlockNestedItem & object;

/** Информация о вложенном объекте */
export interface BlockNestedItem {
  /**
   * Идентификатор
   * @format int32
   */
  id?: number;
  /** Наименование */
  name?: string;
}

/** Информация о дочернем блоке */
export type BlockChildInfo = BlockNestedItem & object;

/** Информация о статическом свойстве блока */
export type BlockPropertyInfo = BlockNestedItem & {
  /** Тип значения свойства */
  type: TagType;
  /**
   * Значение свойства
   * @minLength 1
   */
  value: string;
};

/** Информация о разрешении на объект для субьекта */
export type AccessRightsForObjectInfo = AccessRightsSimpleInfo & {
  /** Информация о группе пользователей */
  userGroup?: UserGroupSimpleInfo | null;
  /** Информация о пользователе */
  user?: UserSimpleInfo | null;
};

/** Информация о блоке в иерархическом представлении */
export type BlockTreeInfo = BlockWithTagsInfo & {
  /** Вложенные блоки */
  children?: BlockTreeInfo[] | null;
  /**
   * Полное имя блока, включающее имена всех родительских блоков по иерархии через "."
   * @minLength 1
   */
  fullName: string;
};

/** Новая информация о блоке */
export interface BlockUpdateRequest {
  /**
   * Новое название
   * @minLength 1
   */
  name: string;
  /** Новое описание */
  description?: string | null;
  /** Новый список закрепленных тегов */
  tags: AttachedTag[];
  /** Версия данных, на основе которых сделан запрос */
  lastKnownVersion?: string;
}

/** Информация о закрепленном теге */
export interface AttachedTag {
  /**
   * Локальный идентификатор тега
   * @format int32
   */
  id: number;
  /**
   * Название поля в блоке, которому соответствует тег
   * @minLength 1
   */
  name: string;
  /** Тип поля блока */
  relation: BlockTagRelation;
}

/** Информация о источнике */
export type SourceInfo = SourceSimpleInfo & {
  /** Произвольное описание источника */
  description?: string | null;
  /** Используемый для получения данных адрес */
  address?: string | null;
  /** Тип протокола, по которому запрашиваются данные */
  type: SourceType;
  /** Источник отмечен как удаленный */
  isDisabled: boolean;
  /** Правило доступа */
  accessRule?: AccessRuleInfo;
};

/** Данные для изменения источника данных */
export interface SourceUpdateRequest {
  /**
   * Название источника
   * @minLength 1
   */
  name: string;
  /** Произвольное описание источника */
  description?: string | null;
  /** Используемый для получения данных адрес */
  address?: string | null;
  /** Тип протокола, по которому запрашиваются данные */
  type: SourceType;
  /** Источник отмечен как удаленный */
  isDisabled: boolean;
}

/** Информация о удалённой записи с данными источника */
export interface SourceItemInfo {
  /**
   * Путь к данным в источнике
   * @minLength 1
   */
  path: string;
  /** Тип данных */
  type: TagType;
  /** Значение, прочитанное с источника при опросе */
  value?: any;
  /** Достоверность значения */
  quality: TagQuality;
}

/** Информация о сопоставлении данных в источнике и в базе */
export interface SourceEntryInfo {
  /** Сопоставленная запись в источнике */
  itemInfo?: SourceItemInfo | null;
  /** Сопоставленный тег в базе */
  tagInfo?: SourceTagInfo | null;
  /** Используется ли тег в запросах */
  isTagInUse?: boolean;
}

/** Информация о теге, берущем данные из этого источника */
export type SourceTagInfo = TagSimpleInfo & {
  /**
   * Путь к данным в источнике
   * @minLength 1
   */
  item: string;
  /** Используемый тип вычисления */
  calculation?: TagCalculation | null;
  /** Формула, на основе которой вычисляется значение */
  formula?: string | null;
  /** Пороговые значения, по которым выбирается итоговое значение */
  thresholds?: TagThresholdInfo[] | null;
  /** Входной тег, по значениям которого выбирается значение из пороговой таблицы */
  thresholdSourceTag?: TagInputMinimalInfo | null;
  /** Входные переменные для формулы, по которой рассчитывается значение */
  formulaInputs: TagInputMinimalInfo[];
  /** Входной тег, по значениям которого считается агрегированное значение */
  sourceTag?: TagInputMinimalInfo | null;
  /** Тип агрегации */
  aggregation?: TagAggregation | null;
  /** Временное окно для расчета агрегированного значения */
  aggregationPeriod?: AggregationPeriod | null;
  /** Правило доступа */
  accessRule?: AccessRuleInfo;
};

/** Соответствие входного и выходного значения по таблице пороговых уставок */
export interface TagThresholdInfo {
  /**
   * Пороговое значение
   * @format float
   */
  threshold: number;
  /**
   * Итоговое значение
   * @format float
   */
  result: number;
}

/** Минимальная информация о переменных для расчета значений по формуле */
export interface TagInputMinimalInfo {
  /**
   * Идентификатор входного тега
   * @format int32
   */
  inputTagId?: number;
  /** Тип данных входного тега */
  inputTagType?: TagType;
  /** Имя переменной */
  variableName?: string;
}

/** Запись собщения */
export interface LogInfo {
  /**
   * Идентификатор записи
   * @format int64
   */
  id: number;
  /**
   * Дата формата DateFormats.Long
   * @minLength 1
   */
  dateString: string;
  /** Категория сообщения (к какому объекту относится) */
  category: LogCategory;
  /** Степень важности сообщения */
  type: LogType;
  /**
   * Текст сообщеня
   * @minLength 1
   */
  text: string;
  /** Информация об авторе сообщения */
  author?: UserSimpleInfo | null;
  /** Информация о затронутом тэге */
  affectedTag?: TagSimpleInfo | null;
  /** Информация о затронутом источнике */
  affectedSource?: SourceSimpleInfo | null;
  /** Информация о затронутом блоке */
  affectedBlock?: BlockSimpleInfo | null;
  /** Информация о затронутой учетной записи */
  affectedUser?: UserSimpleInfo | null;
  /** Информация о затронутом группе учетных записей */
  affectedUserGroup?: UserGroupSimpleInfo | null;
  /** Пояснения и дополнительная информация */
  details?: string | null;
}

/** Объект состояния источника */
export interface SourceState {
  /**
   * Идентификатор источника
   * @format int32
   */
  sourceId: number;
  /**
   * Дата последней попытки подключиться
   * @format date-time
   * @minLength 1
   */
  lastTry: string;
  /**
   * Дата последнего удачного подключения
   * @format date-time
   */
  lastConnection?: string | null;
  /** Было ли соединение при последнем подключении */
  isConnected: boolean;
  /** Была ли попытка установить соединение */
  isTryConnected: boolean;
  /**
   * Список количества тегов этого источника
   * @format int32
   */
  valuesAll: number;
  /**
   * Список количества тегов, которые обновлены за последние полчаса
   * @format int32
   */
  valuesLastHalfHour: number;
  /**
   * Список количества тегов, которые обновлены за последние сутки
   * @format int32
   */
  valuesLastDay: number;
}

/** Информация о настройках приложения, задаваемых через UI */
export interface SettingsInfo {
  /**
   * Адрес сервера EnergoId, к которому выполняются подключения, включая порт при необходимости
   *
   * Протокол будет выбран на основе того, какой используется в клиенте в данный момент
   * @minLength 1
   */
  energoIdHost: string;
  /**
   * Название клиента EnergoId, через который идет аутентификация
   * @minLength 1
   */
  energoIdClient: string;
  /**
   * Конечная точка сервиса, который отдает информацию о пользователях EnergoId
   * @minLength 1
   */
  energoIdApi: string;
  /**
   * Пользовательское название базы данных
   * @minLength 1
   */
  instanceName: string;
}

/** Информация о аутентифицированном пользователе */
export type UserAuthInfo = UserSimpleInfo & {
  /**
   * Идентификатор сессии
   * @minLength 1
   */
  token: string;
  /** Глобальный уровень доступа */
  rootRule: AccessRuleInfo;
  /** Список всех блоков с указанием доступа к ним */
  groups: Record<string, AccessRuleInfo>;
  /** Список всех блоков с указанием доступа к ним */
  sources: Record<string, AccessRuleInfo>;
  /** Список всех блоков с указанием доступа к ним */
  blocks: Record<string, AccessRuleInfo>;
  /** Список всех тегов с указанием доступа к ним */
  tags: Record<string, AccessRuleInfo>;
  /** Идентификатор пользователя внешнего приложения, который передается через промежуточную учетную запись */
  underlyingUser?: UserAuthInfo | null;
  /**
   * Идентификатор пользователя в системе EnergoId
   * @format guid
   */
  energoId?: string | null;
};

export interface KeyValuePairOfValuesRequestKeyAndValuesRequestUsage {
  /** Уникальная подпись для метрики запроса к данным */
  key?: ValuesRequestKey;
  value?: ValuesRequestUsage | null;
}

/** Уникальная подпись для метрики запроса к данным */
export interface ValuesRequestKey {
  /** Ключ запроса */
  requestKey?: string;
  /** Список идентификаторов тегов */
  tags?: string[];
  /** Список глобальных идентификаторов тегов */
  tagsId?: number[] | null;
  /**
   * Дата начала диапазона
   * @format date-time
   */
  old?: string | null;
  /**
   * Дата конца диапазона
   * @format date-time
   */
  young?: string | null;
  /**
   * Дата среза
   * @format date-time
   */
  exact?: string | null;
  /** Шаг */
  resolution?: TagResolution;
  /** Агрегирующая функция */
  func?: AggregationFunc;
}

/** Метрика запроса на чтение данных */
export interface ValuesRequestUsage {
  /**
   * Время последнего выполнения
   * @format duration
   */
  lastExecutionTime?: string;
  /**
   * Время последнего завершения выполнения
   * @format date-time
   */
  lastExecutedAt?: string;
  /**
   * Количество значений в последнем запросе
   * @format int32
   */
  lastValuesCount?: number;
  /**
   * Подсчет количества запросов за последние сутки
   * @format int32
   */
  requestsLast24h?: number;
}

/** Информация о теге */
export type TagInfo = TagSimpleInfo & {
  /** Произвольное описание тега */
  description?: string | null;
  /**
   * Идентификатор источника данных
   * @format int32
   */
  sourceId: number;
  /** Путь к данным в источнике */
  sourceItem?: string | null;
  /** Имя используемого источника данных */
  sourceName?: string | null;
  /** Используемый тип вычисления */
  calculation?: TagCalculation | null;
  /** Формула, на основе которой вычисляется значение */
  formula?: string | null;
  /** Пороговые значения, по которым выбирается итоговое значение */
  thresholds?: TagThresholdInfo[] | null;
  /** Входной тег, по значениям которого выбирается значение из пороговой таблицы */
  thresholdSourceTag?: TagAsInputInfo | null;
  /** Применяется ли приведение числового значения тега к другой шкале */
  isScaling: boolean;
  /**
   * Меньший предел итоговой шкалы
   * @format float
   */
  minEu: number;
  /**
   * Больший предел итоговой шкалы
   * @format float
   */
  maxEu: number;
  /**
   * Меньший предел шкалы исходного значения
   * @format float
   */
  minRaw: number;
  /**
   * Больший предел шкалы исходного значения
   * @format float
   */
  maxRaw: number;
  /** Входные переменные для формулы, по которой рассчитывается значение */
  formulaInputs: TagInputInfo[];
  /** Тип агрегации */
  aggregation?: TagAggregation | null;
  /** Временное окно для расчета агрегированного значения */
  aggregationPeriod?: AggregationPeriod | null;
  /** Идентификатор тега, который будет источником данных для расчета агрегированного значения */
  sourceTag?: TagAsInputInfo | null;
  /** Правило доступа */
  accessRule?: AccessRuleInfo;
};

/** Информации о теге, выступающем в качестве входящей переменной при составлении формулы */
export type TagAsInputInfo = TagSimpleInfo & {
  /**
   * Идентификатор связи, по которой тег был выбран
   * @format int32
   */
  relationId?: number | null;
  /** Правило доступа */
  accessRule?: AccessRuleInfo;
};

/** Тег, используемый как входной параметр в формуле */
export type TagInputInfo = TagSimpleInfo & {
  /**
   * Имя переменной, используемое в формуле
   * @minLength 1
   */
  variableName: string;
  /**
   * Идентификатор связи
   * @format int32
   */
  relationId?: number | null;
  /** Правило доступа */
  accessRule?: AccessRuleInfo;
};

/** Необходимые данные для создания тега */
export interface TagCreateRequest {
  /** Наименование тега. Если не указать, будет составлено автоматически */
  name?: string | null;
  /** Тип значений тега */
  tagType: TagType;
  /** Частота записи тега */
  resolution: TagResolution;
  /**
   * Идентификатор источника данных
   * @format int32
   */
  sourceId?: number | null;
  /** Путь к данным при использовании удалённого источника */
  sourceItem?: string | null;
  /**
   * Идентификатор блока, к которому будет привязан новый тег
   * @format int32
   */
  blockId?: number | null;
}

/** Полная информация о теге */
export type TagFullInfo = TagInfo & {
  /** Список блоков, в которых используется этот тег */
  blocks: TagBlockRelationInfo[];
};

/** Краткая информация о блоке, имеющем связь с тегом, включая локальное имя тега в блоке */
export type TagBlockRelationInfo = BlockSimpleInfo & {
  /**
   * Идентификатор связи
   * @format int32
   */
  relationId: number;
  /** Локальное имя тега в блоке */
  localName?: string | null;
};

/** Данные запроса для изменение тега */
export interface TagUpdateRequest {
  /**
   * Новое имя тега
   * @minLength 1
   */
  name: string;
  /** Новое описание */
  description?: string | null;
  /** Новый тип данных */
  type: TagType;
  /** Путь к данными в источнике */
  sourceItem?: string | null;
  /** Применяется ли изменение шкалы значения */
  isScaling: boolean;
  /**
   * Меньший предел итоговой шкалы
   * @format float
   */
  minEu: number;
  /**
   * Больший предел итоговой шкалы
   * @format float
   */
  maxEu: number;
  /**
   * Меньший предел шкалы исходного значения
   * @format float
   */
  minRaw: number;
  /**
   * Больший предел шкалы исходного значения
   * @format float
   */
  maxRaw: number;
  /**
   * Источник данных
   * @format int32
   */
  sourceId: number;
  /** Новый интервал получения значения */
  resolution: TagResolution;
  /** Используемый тип вычисления */
  calculation?: TagCalculation | null;
  /** Формула, на основе которой вычисляется значение */
  formula?: string | null;
  /** Пороговые значения, по которым выбирается итоговое значение */
  thresholds?: TagThresholdInfo[] | null;
  /**
   * Идентификатор тега, который будет источником данных для выбора из пороговой таблицы
   * @format int32
   */
  thresholdSourceTagId?: number | null;
  /**
   * Идентификатор связи, по которой выбран тег-источник данных для выбора из пороговой таблицы
   * @format int32
   */
  thresholdSourceTagRelationId?: number | null;
  /** Входные переменные для формулы, по которой рассчитывается значение */
  formulaInputs: TagUpdateInputRequest[];
  /** Тип агрегации */
  aggregation?: TagAggregation | null;
  /** Временное окно для расчета агрегированного значения */
  aggregationPeriod?: AggregationPeriod | null;
  /**
   * Идентификатор тега, который будет источником данных для расчета агрегированного значения
   * @format int32
   */
  sourceTagId?: number | null;
  /**
   * Идентификатор связи, по которой выбран тег-источник данных для расчета агрегированного значения
   * @format int32
   */
  sourceTagRelationId?: number | null;
}

/** Необходимая информация для привязки тега в качестве входного для  */
export interface TagUpdateInputRequest {
  /**
   * Название переменной
   * @minLength 1
   */
  variableName: string;
  /**
   * Идентификатор закрепленного тега
   * @format int32
   */
  tagId: number;
  /**
   * Идентификатор связи, по которой выбран закрепленный тег
   * @format int32
   */
  tagRelationId: number;
}

/** Информация о группе пользователей */
export type UserGroupInfo = UserGroupSimpleInfo & {
  /** Произвольное описание группы */
  description?: string | null;
  /**
   * Идентификатор группы, в которой располагается эта группа
   * @format guid
   */
  parentGroupGuid?: string | null;
  /** Общий уровень доступа для всех участников группы */
  globalAccessType: AccessType;
};

/** Данные запроса для создания группы пользователей */
export interface UserGroupCreateRequest {
  /**
   * Название. Не может повторяться в рамках родительской группы
   * @minLength 1
   */
  name: string;
  /**
   * Идентификатор родительской группы
   * @format guid
   */
  parentGuid?: string | null;
  /** Описание */
  description?: string | null;
}

/** Информация о группе пользователей в иерархическом представлении */
export type UserGroupTreeInfo = UserGroupInfo & {
  /** Список подгрупп */
  children: UserGroupTreeInfo[];
  /**
   * Идентификатор родительской группы
   * @format guid
   */
  parentGuid?: string | null;
  /** Информация о родительской группе */
  parent?: UserGroupTreeInfo | null;
};

/** Расширенная информация о группе пользователей, включающая вложенные группы и список пользователей */
export type UserGroupDetailedInfo = UserGroupInfo & {
  /** Список пользователей этой группы */
  users: UserGroupUsersInfo[];
  /** Список подгрупп этой группы */
  subgroups: UserGroupSimpleInfo[];
  /** Разрешения, выданные на эту группу */
  accessRights: AccessRightsForOneInfo[];
};

/** Информация о пользователей данной группы */
export interface UserGroupUsersInfo {
  /**
   * Идентификатор пользователя
   * @format guid
   * @minLength 1
   */
  guid: string;
  /** Уровень доступа пользователя в группе */
  accessType: AccessType;
  /** Полное имя пользователя */
  fullName?: string | null;
}

/** Данные запроса для изменения группы пользователей */
export type UserGroupUpdateRequest = UserGroupCreateRequest & {
  /** Базовый уровень доступа участников и под-групп */
  accessType: AccessType;
  /** Список пользователей, которые включены в эту группу */
  users: UserGroupUsersInfo[];
};

/** Данные с сервиса "EnergoID" */
export interface EnergoIdInfo {
  /** Список пользователей */
  energoIdUsers: UserEnergoIdInfo[];
  /** Есть ли связь с сервисом "EnergoID" */
  connected: boolean;
}

/** Информация о пользователе, взятая из Keycloak */
export interface UserEnergoIdInfo {
  /**
   * Идентификатор пользователя в сервере Keycloak
   * @format guid
   * @minLength 1
   */
  energoIdGuid: string;
  /**
   * Имя для входа
   * @minLength 1
   */
  login: string;
  /**
   * Полное имя пользователя
   * @minLength 1
   */
  fullName: string;
}

/** Информация при аутентификации локальной учетной записи */
export interface UserLoginPass {
  /**
   * Имя для входа
   * @minLength 1
   */
  login: string;
  /**
   * Пароль
   * @minLength 1
   */
  password: string;
}

/** Информация о пользователе */
export type UserInfo = UserSimpleInfo & {
  /** Имя для входа */
  login?: string | null;
  /** Глобальный уровень доступа */
  accessType: AccessType;
  /** Тип учётной записи */
  type: UserType;
  /**
   * Идентификатор пользователя в сервере EnergoId
   * @format guid
   */
  energoIdGuid?: string | null;
  /** Список групп, в которые входит пользователь */
  userGroups: UserGroupSimpleInfo[];
};

/** Данные запроса на создание пользователя */
export interface UserCreateRequest {
  /** Имя для входа */
  login?: string | null;
  /** Полное имя пользователя */
  fullName?: string | null;
  /** Глобальный уровень доступа */
  accessType: AccessType;
  /** Тип учетной записи */
  type: UserType;
  /** Используемый пароль */
  password?: string | null;
  /** Адрес статической точки, откуда будет осуществляться доступ */
  staticHost?: string | null;
  /**
   * Идентификатор связанной учетной записи в сервере EnergoId
   * @format guid
   */
  energoIdGuid?: string | null;
}

/** Расширенная информация о пользователе, включающая данные для аутентификации */
export type UserDetailInfo = UserInfo & {
  /** Хэш, по которому проверяется доступ */
  hash?: string | null;
  /** Адрес статического узла, с которого идёт доступ */
  staticHost?: string | null;
};

/** Данные запроса для изменения учетной записи */
export interface UserUpdateRequest {
  /** Новое имя для входа */
  login?: string | null;
  /** Новый адрес статической точки, из которой будет разрешен доступ */
  staticHost?: string | null;
  /** Новый пароль */
  password?: string | null;
  /** Новое полное имя */
  fullName?: string | null;
  /** Новый глобальный уровень доступа */
  accessType: AccessType;
  /** Нужно ли создать новый ключ для статичной учетной записи */
  createNewStaticHash: boolean;
  /**
   * Идентификатор пользователя в сервере EnergoId
   * @format guid
   */
  energoIdGuid?: string | null;
  /** Тип учетной записи */
  type: UserType;
}

/** Ответ на запрос для получения значений, включающий обработанные теги и идентификатор запроса */
export interface ValuesResponse {
  /**
   * Идентификатор запроса, который будет передан в соответствующий объект ответа
   * @minLength 1
   */
  requestKey: string;
  /** Список глобальных идентификаторов тегов */
  tags: ValuesTagResponse[];
}

/** Ответ на запрос для получения значений, характеризующий запрошенный тег и его значения */
export type ValuesTagResponse = TagSimpleInfo & {
  /** Список значений */
  values: ValueRecord[];
  /** Флаг, говорящий о недостаточности доступа для записи у пользователя */
  noAccess?: boolean | null;
};

/** Запись о значении */
export interface ValueRecord {
  /**
   * Дата, на которую значение актуально
   * @format date-time
   * @minLength 1
   */
  date: string;
  /**
   * Строковое представление даты
   * @minLength 1
   */
  dateString: string;
  /** Значение */
  value?: any;
  /** Достоверность значения */
  quality: TagQuality;
}

/** Данные запроса для получения значений */
export interface ValuesRequest {
  /**
   * Идентификатор запроса, который будет передан в соответствующий объект ответа
   * @minLength 1
   */
  requestKey: string;
  /** Список глобальных идентификаторов тегов */
  tags?: string[] | null;
  /** Список локальных идентификаторов тегов */
  tagsId?: number[] | null;
  /**
   * Дата, с которой (включительно) нужно получить значения. По умолчанию - начало текущих суток
   * @format date-time
   */
  old?: string | null;
  /**
   * Дата, по которую (включительно) нужно получить значения. По умолчанию - текущая дата
   * @format date-time
   */
  young?: string | null;
  /**
   * Дата, на которую (по точному соответствию) нужно получить значения. По умолчанию - не используется
   * @format date-time
   */
  exact?: string | null;
  /** Шаг времени, по которому нужно разбить значения. Если не задан, будут оставлены записи о изменениях значений */
  resolution?: TagResolution | null;
  /** Тип агрегирования значений, который нужно применить к этому запросу. По умолчанию - список */
  func?: AggregationFunc | null;
}

/** Данные запроса на ввод значения */
export interface ValueWriteRequest {
  /**
   * Глобальный идентификатор тега
   * @format guid
   */
  guid?: string | null;
  /**
   * Идентификатор тега в локальной базе
   * @format int32
   */
  id?: number | null;
  /** Наименование тега */
  name?: string | null;
  /** Новое значение */
  value?: any;
  /**
   * Дата, на которую будет записано значение
   * @format date-time
   */
  date?: string | null;
  /** Флаг достоверности нового значения */
  quality?: TagQuality | null;
}

/** Список запросов с настройками */
export type ValuesGetPayload = ValuesRequest[];

/** Список запросов на изменение */
export type ValuesWritePayload = ValueWriteRequest[];
