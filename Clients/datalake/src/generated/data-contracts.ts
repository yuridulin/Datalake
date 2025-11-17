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
 * 0 = UnknownError
 * 1 = Ok
 * 2 = NotFound
 * 3 = IsDeleted
 * 4 = NoAccess
 * 5 = NotManual
 * 6 = ValueNotFound
 * 7 = InternalError
 */
export enum ValueResult {
  UnknownError = 0,
  Ok = 1,
  NotFound = 2,
  IsDeleted = 3,
  NoAccess = 4,
  NotManual = 5,
  ValueNotFound = 6,
  InternalError = 7,
}

/**
 * Достоверность значения
 *
 * 0 = Unknown
 * 4 = Bad_NoConnect
 * 8 = Bad_NoValues
 * 12 = Bad_CalcError
 * 26 = Bad_ManualWrite
 * 100 = Bad_LOCF
 * 192 = Good
 * 200 = Good_LOCF
 * 216 = Good_ManualWrite
 */
export enum TagQuality {
  Unknown = 0,
  BadNoConnect = 4,
  BadNoValues = 8,
  BadCalcError = 12,
  BadManualWrite = 26,
  BadLOCF = 100,
  Good = 192,
  GoodLOCF = 200,
  GoodManualWrite = 216,
}

/**
 * Тип учётной записи
 *
 * 1 = Local
 * 3 = EnergoId
 */
export enum UserType {
  Local = 1,
  EnergoId = 3,
}

/**
 * Способ получения агрегированного значения
 *
 * 0 = None
 * 1 = Sum
 * 2 = Average
 * 3 = Min
 * 4 = Max
 */
export enum TagAggregation {
  None = 0,
  Sum = 1,
  Average = 2,
  Min = 3,
  Max = 4,
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
 * Тип получения данных с источника
 *
 * 0 = System
 * 1 = Inopc
 * 3 = Datalake
 * -666 = Unset
 * -4 = Thresholds
 * -3 = Aggregated
 * -2 = Manual
 * -1 = Calculated
 */
export enum SourceType {
  /** Системные теги с данными о текущей работе различных частей приложения */
  System = 0,
  Inopc = 1,
  Datalake = 3,
  /** Источник без получения значений */
  Unset = -666,
  /** Теги, значения которых считаются на стороне БД как самое близкое по модулю среди пороговых значений */
  Thresholds = -4,
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
 * 0 = None
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
 * 13 = Year
 * 14 = Quarter
 */
export enum TagResolution {
  None = 0,
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
  Year = 13,
  Quarter = 14,
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
 * 0 = None
 * 1 = Viewer
 * 2 = Editor
 * 3 = Manager
 * 4 = Denied
 * 5 = Admin
 */
export enum AccessType {
  None = 0,
  Viewer = 1,
  Editor = 2,
  Manager = 3,
  Denied = 4,
  Admin = 5,
}

export type AccessRightsInfo = AccessRightsForOneInfo & {
  userGroup?: UserGroupSimpleInfo | null;
  user?: UserSimpleInfo | null;
};

export interface UserGroupSimpleInfo {
  /**
   * @format guid
   * @minLength 1
   */
  guid: string;
  /** @minLength 1 */
  name: string;
  accessRule: AccessRuleInfo;
}

export interface AccessRuleInfo {
  /** @format int32 */
  ruleId: number;
  /**
   * Уровень доступа
   *
   * 0 = None
   * 1 = Viewer
   * 2 = Editor
   * 3 = Manager
   * 4 = Denied
   * 5 = Admin
   */
  access: AccessType;
}

export interface UserSimpleInfo {
  /**
   * @format guid
   * @minLength 1
   */
  guid: string;
  /** @minLength 1 */
  fullName: string;
  accessRule: AccessRuleInfo;
}

export type AccessRightsForOneInfo = AccessRightsSimpleInfo & {
  tag?: TagSimpleInfo | null;
  block?: BlockSimpleInfo | null;
  source?: SourceSimpleInfo | null;
};

export interface TagSimpleInfo {
  /** @format int32 */
  id: number;
  /**
   * @format guid
   * @minLength 1
   */
  guid: string;
  /** @minLength 1 */
  name: string;
  /**
   * Тип данных
   *
   * 0 = String
   * 1 = Number
   * 2 = Boolean
   */
  type: TagType;
  /**
   * Частота записи/чтения значения
   *
   * 0 = None
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
   * 13 = Year
   * 14 = Quarter
   */
  resolution: TagResolution;
  /**
   * Тип получения данных с источника
   *
   * 0 = System
   * 1 = Inopc
   * 3 = Datalake
   * -666 = Unset
   * -4 = Thresholds
   * -3 = Aggregated
   * -2 = Manual
   * -1 = Calculated
   */
  sourceType: SourceType;
}

export interface BlockSimpleInfo {
  /** @format int32 */
  id: number;
  /**
   * @format guid
   * @minLength 1
   */
  guid: string;
  /** @minLength 1 */
  name: string;
}

export interface SourceSimpleInfo {
  /** @format int32 */
  id: number;
  /** @minLength 1 */
  name: string;
}

export interface AccessRightsSimpleInfo {
  /** @format int32 */
  id: number;
  /**
   * Уровень доступа
   *
   * 0 = None
   * 1 = Viewer
   * 2 = Editor
   * 3 = Manager
   * 4 = Denied
   * 5 = Admin
   */
  accessType: AccessType;
  isGlobal: boolean;
}

/** Состояние рассчитанных прав доступа учетной записи */
export interface UserAccessValue {
  /**
   * Идентификатор учетной записи
   * @format guid
   * @minLength 1
   */
  guid: string;
  /** Глобальный уровень доступа */
  rootRule: UserAccessRuleValue;
  /** Уровни доступа к группам учетных групп */
  groupsRules: Record<string, UserAccessRuleValue>;
  /** Уровни доступа к источникам данных */
  sourcesRules: Record<string, UserAccessRuleValue>;
  /** Уровни доступа к блокам */
  blocksRules: Record<string, UserAccessRuleValue>;
  /** Уровни доступа к тегам */
  tagsRules: Record<string, UserAccessRuleValue>;
}

/** Рассчитанный уровень доступа */
export interface UserAccessRuleValue {
  /**
   * Идентификатор правила, на основе которого выполнен расчет
   * @format int32
   */
  id: number;
  /** Уровень доступа */
  access: AccessType;
}

export interface AccessRuleForObjectRequest {
  /**
   * Уровень доступа
   *
   * 0 = None
   * 1 = Viewer
   * 2 = Editor
   * 3 = Manager
   * 4 = Denied
   * 5 = Admin
   */
  accessType?: AccessType;
  /** @format guid */
  userGuid?: string | null;
  /** @format guid */
  userGroupGuid?: string | null;
}

export interface AccessRuleForActorRequest {
  /**
   * Уровень доступа
   *
   * 0 = None
   * 1 = Viewer
   * 2 = Editor
   * 3 = Manager
   * 4 = Denied
   * 5 = Admin
   */
  accessType?: AccessType;
  /** @format int32 */
  blockId?: number | null;
  /** @format int32 */
  sourceId?: number | null;
  /** @format int32 */
  tagId?: number | null;
}

export interface LogInfo {
  /** @format int64 */
  id: number;
  /** @minLength 1 */
  dateString: string;
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
  category: LogCategory;
  /**
   * Степень важности сообщения
   *
   * 0 = Trace
   * 1 = Information
   * 2 = Success
   * 3 = Warning
   * 4 = Error
   */
  type: LogType;
  /** @minLength 1 */
  text: string;
  author?: UserSimpleInfo | null;
  affectedTag?: TagSimpleInfo | null;
  affectedSource?: SourceSimpleInfo | null;
  affectedBlock?: BlockSimpleInfo | null;
  affectedUser?: UserSimpleInfo | null;
  affectedUserGroup?: UserGroupSimpleInfo | null;
  details?: string | null;
}

export interface BlockCreateRequest {
  /** @format int32 */
  parentId?: number | null;
  name?: string | null;
  description?: string | null;
}

export type BlockWithTagsInfo = BlockSimpleInfo & {
  /** @format int32 */
  parentId?: number | null;
  description?: string | null;
  accessRule: AccessRuleInfo;
  tags: BlockNestedTagInfo[];
};

export type BlockNestedTagInfo = TagSimpleInfo & {
  /**
   * Тип связи тега и блока
   *
   * 0 = Static
   * 1 = Input
   * 2 = Output
   */
  relationType: BlockTagRelation;
  /** @minLength 1 */
  localName: string;
  /** @format int32 */
  sourceId: number;
};

export type BlockFullInfo = BlockWithTagsInfo & {
  parent?: BlockParentInfo | null;
  children: BlockChildInfo[];
  properties: BlockPropertyInfo[];
  accessRights: AccessRightsForObjectInfo[];
  adults: BlockTreeInfo[];
};

export type BlockParentInfo = BlockNestedItem & object;

export interface BlockNestedItem {
  /** @format int32 */
  id?: number;
  name?: string;
}

export type BlockChildInfo = BlockNestedItem & object;

export type BlockPropertyInfo = BlockNestedItem & {
  /**
   * Тип данных
   *
   * 0 = String
   * 1 = Number
   * 2 = Boolean
   */
  type: TagType;
  /** @minLength 1 */
  value: string;
};

export type AccessRightsForObjectInfo = AccessRightsSimpleInfo & {
  userGroup?: UserGroupSimpleInfo | null;
  user?: UserSimpleInfo | null;
};

export type BlockTreeInfo = BlockWithTagsInfo & {
  children?: BlockTreeInfo[] | null;
  /** @minLength 1 */
  fullName: string;
};

export interface BlockUpdateRequest {
  /** @minLength 1 */
  name: string;
  description?: string | null;
  tags: AttachedTag[];
  lastKnownVersion?: string;
}

export interface AttachedTag {
  /** @format int32 */
  id: number;
  /** @minLength 1 */
  name: string;
  /**
   * Тип связи тега и блока
   *
   * 0 = Static
   * 1 = Input
   * 2 = Output
   */
  relation: BlockTagRelation;
}

export interface UserEnergoIdInfo {
  /**
   * @format guid
   * @minLength 1
   */
  energoIdGuid: string;
  /** @format guid */
  userGuid?: string | null;
  /** @minLength 1 */
  email: string;
  /** @minLength 1 */
  fullName: string;
}

export type SourceInfo = SourceSimpleInfo & {
  description?: string | null;
  address?: string | null;
  /**
   * Тип получения данных с источника
   *
   * 0 = System
   * 1 = Inopc
   * 3 = Datalake
   * -666 = Unset
   * -4 = Thresholds
   * -3 = Aggregated
   * -2 = Manual
   * -1 = Calculated
   */
  type: SourceType;
  isDisabled: boolean;
  accessRule?: AccessRuleInfo;
};

export interface SourceUpdateRequest {
  /** @minLength 1 */
  name: string;
  description?: string | null;
  address?: string | null;
  /**
   * Тип получения данных с источника
   *
   * 0 = System
   * 1 = Inopc
   * 3 = Datalake
   * -666 = Unset
   * -4 = Thresholds
   * -3 = Aggregated
   * -2 = Manual
   * -1 = Calculated
   */
  type: SourceType;
  isDisabled: boolean;
}

export interface SettingsInfo {
  /** @minLength 1 */
  energoIdHost: string;
  /** @minLength 1 */
  energoIdClient: string;
  /** @minLength 1 */
  energoIdApi: string;
  /** @minLength 1 */
  instanceName: string;
}

export type TagInfo = TagSimpleInfo & {
  description?: string | null;
  /** @format int32 */
  sourceId: number;
  sourceItem?: string | null;
  sourceName?: string | null;
  formula?: string | null;
  thresholds?: TagThresholdInfo[] | null;
  thresholdSourceTag?: TagAsInputInfo | null;
  isScaling: boolean;
  /** @format float */
  minEu: number;
  /** @format float */
  maxEu: number;
  /** @format float */
  minRaw: number;
  /** @format float */
  maxRaw: number;
  formulaInputs: TagInputInfo[];
  aggregation?: TagAggregation | null;
  aggregationPeriod?: TagResolution | null;
  sourceTag?: TagAsInputInfo | null;
  accessRule?: AccessRuleInfo;
};

export interface TagThresholdInfo {
  /** @format float */
  threshold: number;
  /** @format float */
  result: number;
}

export type TagAsInputInfo = TagSimpleInfo & {
  /** @format int32 */
  blockId?: number | null;
  accessRule?: AccessRuleInfo;
};

export type TagInputInfo = TagSimpleInfo & {
  /** @minLength 1 */
  variableName: string;
  /** @format int32 */
  blockId?: number | null;
  accessRule?: AccessRuleInfo;
};

export interface TagCreateRequest {
  name?: string | null;
  /**
   * Тип данных
   *
   * 0 = String
   * 1 = Number
   * 2 = Boolean
   */
  tagType: TagType;
  /** @format int32 */
  sourceId?: number | null;
  sourceItem?: string | null;
  /** @format int32 */
  blockId?: number | null;
}

export type TagFullInfo = TagInfo & {
  blocks: TagBlockRelationInfo[];
};

export type TagBlockRelationInfo = BlockSimpleInfo & {
  /** @format int32 */
  relationId: number;
  localName?: string | null;
};

export interface TagUpdateRequest {
  /** @minLength 1 */
  name: string;
  description?: string | null;
  /**
   * Тип данных
   *
   * 0 = String
   * 1 = Number
   * 2 = Boolean
   */
  type: TagType;
  sourceItem?: string | null;
  isScaling: boolean;
  /** @format float */
  minEu: number;
  /** @format float */
  maxEu: number;
  /** @format float */
  minRaw: number;
  /** @format float */
  maxRaw: number;
  /** @format int32 */
  sourceId: number;
  /**
   * Частота записи/чтения значения
   *
   * 0 = None
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
   * 13 = Year
   * 14 = Quarter
   */
  resolution: TagResolution;
  formula?: string | null;
  thresholds?: TagThresholdInfo[];
  /** @format int32 */
  thresholdSourceTagId?: number | null;
  /** @format int32 */
  thresholdSourceTagBlockId?: number | null;
  formulaInputs: TagUpdateInputRequest[];
  aggregation?: TagAggregation | null;
  aggregationPeriod?: TagResolution | null;
  /** @format int32 */
  sourceTagId?: number | null;
  /** @format int32 */
  sourceTagBlockId?: number | null;
}

export interface TagUpdateInputRequest {
  /** @minLength 1 */
  variableName: string;
  /** @format int32 */
  tagId: number;
  /** @format int32 */
  blockId: number;
}

export interface UserGroupCreateRequest {
  /** @minLength 1 */
  name: string;
  /** @format guid */
  parentGuid?: string | null;
  description?: string | null;
}

export type UserGroupInfo = UserGroupSimpleInfo & {
  description?: string | null;
  /** @format guid */
  parentGroupGuid?: string | null;
  /**
   * Уровень доступа
   *
   * 0 = None
   * 1 = Viewer
   * 2 = Editor
   * 3 = Manager
   * 4 = Denied
   * 5 = Admin
   */
  globalAccessType: AccessType;
};

export type UserGroupTreeInfo = UserGroupInfo & {
  children: UserGroupTreeInfo[];
  /** @format guid */
  parentGuid?: string | null;
  parent?: UserGroupTreeInfo | null;
};

export type UserGroupDetailedInfo = UserGroupInfo & {
  users: UserGroupUsersInfo[];
  subgroups: UserGroupSimpleInfo[];
  accessRights: AccessRightsForOneInfo[];
};

export interface UserGroupUsersInfo {
  /**
   * @format guid
   * @minLength 1
   */
  guid: string;
  /**
   * Уровень доступа
   *
   * 0 = None
   * 1 = Viewer
   * 2 = Editor
   * 3 = Manager
   * 4 = Denied
   * 5 = Admin
   */
  accessType: AccessType;
  fullName?: string | null;
}

export type UserGroupUpdateRequest = UserGroupCreateRequest & {
  /**
   * Уровень доступа
   *
   * 0 = None
   * 1 = Viewer
   * 2 = Editor
   * 3 = Manager
   * 4 = Denied
   * 5 = Admin
   */
  accessType: AccessType;
  users: UserGroupUsersInfo[];
};

export interface UserCreateRequest {
  /**
   * Тип учётной записи
   *
   * 1 = Local
   * 3 = EnergoId
   */
  type: UserType;
  /** @format guid */
  energoIdGuid?: string | null;
  login?: string | null;
  password?: string | null;
  fullName?: string | null;
  email?: string | null;
  /**
   * Уровень доступа
   *
   * 0 = None
   * 1 = Viewer
   * 2 = Editor
   * 3 = Manager
   * 4 = Denied
   * 5 = Admin
   */
  accessType: AccessType;
}

export type UserInfo = UserSimpleInfo & {
  login?: string | null;
  /**
   * Уровень доступа
   *
   * 0 = None
   * 1 = Viewer
   * 2 = Editor
   * 3 = Manager
   * 4 = Denied
   * 5 = Admin
   */
  accessType: AccessType;
  /**
   * Тип учётной записи
   *
   * 1 = Local
   * 3 = EnergoId
   */
  type: UserType;
  /** @format guid */
  energoIdGuid?: string | null;
  userGroups: UserGroupSimpleInfo[];
};

export interface UserUpdateRequest {
  login?: string | null;
  staticHost?: string | null;
  password?: string | null;
  fullName?: string | null;
  email?: string | null;
  /**
   * Уровень доступа
   *
   * 0 = None
   * 1 = Viewer
   * 2 = Editor
   * 3 = Manager
   * 4 = Denied
   * 5 = Admin
   */
  accessType: AccessType;
  createNewStaticHash: boolean;
  /** @format guid */
  energoIdGuid?: string | null;
  /**
   * Тип учётной записи
   *
   * 1 = Local
   * 3 = EnergoId
   */
  type: UserType;
}

export interface SourceActivityInfo {
  /** @format int32 */
  sourceId: number;
  /**
   * @format date-time
   * @minLength 1
   */
  lastTry: string;
  /** @format date-time */
  lastConnection?: string | null;
  isConnected: boolean;
  /** @format int32 */
  valuesAll: number;
  /** @format int32 */
  valuesLastConnection: number;
  /** @format int32 */
  valuesLastHalfHour: number;
  /** @format int32 */
  valuesLastDay: number;
}

export interface SourceItemInfo {
  /** @minLength 1 */
  path: string;
  /**
   * Тип данных
   *
   * 0 = String
   * 1 = Number
   * 2 = Boolean
   */
  type: TagType;
  value?: any;
  /**
   * Достоверность значения
   *
   * 0 = Unknown
   * 4 = Bad_NoConnect
   * 8 = Bad_NoValues
   * 12 = Bad_CalcError
   * 26 = Bad_ManualWrite
   * 100 = Bad_LOCF
   * 192 = Good
   * 200 = Good_LOCF
   * 216 = Good_ManualWrite
   */
  quality: TagQuality;
}

export interface TagMetricRequest {
  tagsId?: number[] | null;
  tagsGuid?: string[] | null;
}

export interface ValuesResponse {
  /** @minLength 1 */
  requestKey: string;
  tags: ValuesTagResponse[];
}

export interface ValuesTagResponse {
  /** @format int32 */
  id: number;
  /**
   * @format guid
   * @minLength 1
   */
  guid: string;
  /** @minLength 1 */
  name: string;
  /**
   * Тип данных
   *
   * 0 = String
   * 1 = Number
   * 2 = Boolean
   */
  type: TagType;
  /**
   * Частота записи/чтения значения
   *
   * 0 = None
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
   * 13 = Year
   * 14 = Quarter
   */
  resolution: TagResolution;
  /**
   * Тип получения данных с источника
   *
   * 0 = System
   * 1 = Inopc
   * 3 = Datalake
   * -666 = Unset
   * -4 = Thresholds
   * -3 = Aggregated
   * -2 = Manual
   * -1 = Calculated
   */
  sourceType: SourceType;
  /**
   * 0 = UnknownError
   * 1 = Ok
   * 2 = NotFound
   * 3 = IsDeleted
   * 4 = NoAccess
   * 5 = NotManual
   * 6 = ValueNotFound
   * 7 = InternalError
   */
  result: ValueResult;
  values: ValueRecord[];
}

export interface ValueRecord {
  /**
   * @format date-time
   * @minLength 1
   */
  date: string;
  text?: string | null;
  /** @format float */
  number?: number | null;
  boolean?: boolean | null;
  /**
   * Достоверность значения
   *
   * 0 = Unknown
   * 4 = Bad_NoConnect
   * 8 = Bad_NoValues
   * 12 = Bad_CalcError
   * 26 = Bad_ManualWrite
   * 100 = Bad_LOCF
   * 192 = Good
   * 200 = Good_LOCF
   * 216 = Good_ManualWrite
   */
  quality: TagQuality;
}

export interface ValuesRequest {
  /** @minLength 1 */
  requestKey: string;
  tagsId?: number[] | null;
  tags?: string[] | null;
  /** @format date-time */
  old?: string | null;
  /** @format date-time */
  young?: string | null;
  /** @format date-time */
  exact?: string | null;
  resolution?: TagResolution | null;
  func?: TagAggregation | null;
}

export interface ValueWriteRequest {
  /** @format int32 */
  id?: number | null;
  /** @format guid */
  guid?: string | null;
  value?: any;
  /** @format date-time */
  date?: string | null;
  quality?: TagQuality | null;
}

export type UserSessionWithAccessInfo = SessionInfo & {
  access: AccessInfo;
};

export interface AccessInfo {
  rootRule: AccessRuleInfo;
  groups: Record<string, AccessRuleInfo>;
  sources: Record<string, AccessRuleInfo>;
  blocks: Record<string, AccessRuleInfo>;
  tags: Record<string, AccessRuleInfo>;
}

export interface SessionInfo {
  /** @minLength 1 */
  token: string;
  /**
   * @format guid
   * @minLength 1
   */
  userGuid: string;
  /**
   * @format date-time
   * @minLength 1
   */
  expirationTime: string;
  /**
   * Тип учётной записи
   *
   * 1 = Local
   * 3 = EnergoId
   */
  type: UserType;
}

export interface AuthLoginPassRequest {
  /** @minLength 1 */
  login: string;
  /** @minLength 1 */
  password: string;
}

export interface AuthEnergoIdRequest {
  /**
   * @format guid
   * @minLength 1
   */
  energoIdGuid: string;
  /** @minLength 1 */
  email: string;
  /** @minLength 1 */
  fullName: string;
}

export type InventoryAccessGetCalculatedAccessPayload = string[];

/** Список изменений */
export type InventoryAccessSetBlockRulesPayload = AccessRuleForObjectRequest[];

/** Список изменений */
export type InventoryAccessSetSourceRulesPayload = AccessRuleForObjectRequest[];

/** Список изменений */
export type InventoryAccessSetTagRulesPayload = AccessRuleForObjectRequest[];

/** Список изменений */
export type InventoryAccessSetUserGroupRulesPayload =
  AccessRuleForActorRequest[];

/** Список изменений */
export type InventoryAccessSetUserRulesPayload = AccessRuleForActorRequest[];

/** Список идентификаторов источников данных */
export type DataSourcesGetActivityPayload = number[];

/** Список запросов с настройками */
export type DataValuesGetPayload = ValuesRequest[];

/** Список запросов на изменение */
export type DataValuesWritePayload = ValueWriteRequest[];

/** Идентификаторы запрошенных пользователей */
export type UsersGetActivityPayload = string[];
