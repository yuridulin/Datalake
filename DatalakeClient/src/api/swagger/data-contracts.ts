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

export interface BlockInfo {
	/** @format int32 */
	id: number
	/** @minLength 1 */
	name: string
	description?: string | null
	parent?: BlockParentInfo | null
	children: BlockChildInfo[]
	properties: BlockPropertyInfo[]
	tags: BlockTagInfo[]
}

export type BlockParentInfo = BlockRelationInfo & object

export interface BlockRelationInfo {
	/** @format int32 */
	id: number
	/** @minLength 1 */
	name: string
}

export type BlockChildInfo = BlockRelationInfo & object

export type BlockPropertyInfo = BlockRelationInfo & {
	type: TagType
	/** @minLength 1 */
	value: string
}

export enum TagType {
	String = 'String',
	Number = 'Number',
	Boolean = 'Boolean',
}

export type BlockTagInfo = BlockRelationInfo & {
	tagType: BlockTagRelation
}

export enum BlockTagRelation {
	Static = 'Static',
	Input = 'Input',
	Output = 'Output',
}

export interface BlockSimpleInfo {
	/** @format int32 */
	id: number
	/** @minLength 1 */
	name: string
	description?: string | null
}

export interface BlockTreeInfo {
	/** @format int32 */
	id: number
	/** @minLength 1 */
	name: string
	description?: string | null
	children: BlockTreeInfo[]
}

export interface LogInfo {
	/** @format int64 */
	id: number
	/** @minLength 1 */
	dateString: string
	category: LogCategory
	type: LogType
	/** @minLength 1 */
	text: string
	refId?: string | null
}

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

export enum LogType {
	Trace = 'Trace',
	Information = 'Information',
	Success = 'Success',
	Warning = 'Warning',
	Error = 'Error',
}

export interface SourceInfo {
	/** @format int32 */
	id: number
	/** @minLength 1 */
	name: string
	description?: string | null
	address?: string | null
	type: SourceType
}

export enum SourceType {
	Inopc = 'Inopc',
	Datalake = 'Datalake',
	Unknown = 'Unknown',
	Custom = 'Custom',
}

export interface SourceItemInfo {
	/** @minLength 1 */
	path: string
	type: TagType
}

export interface SourceEntryInfo {
	itemInfo?: SourceItemInfo | null
	tagInfo?: SourceTagInfo | null
}

export interface SourceTagInfo {
	/** @format int32 */
	id: number
	/** @minLength 1 */
	name: string
	/** @minLength 1 */
	item: string
	type: TagType
}

export interface TagCreateRequest {
	name?: string | null
	tagType: TagType
	/** @format int32 */
	sourceId?: number | null
	sourceItem?: string | null
	/** @format int32 */
	blockId?: number | null
}

export interface TagInfo {
	/** @format int32 */
	id: number
	/** @minLength 1 */
	name: string
	description?: string | null
	type: TagType
	intervalInSeconds: number
	/** @format int32 */
	sourceId: number
	sourceType: SourceType
	sourceItem?: string | null
	sourceName?: string | null
	formula?: string | null
	isScaling: boolean
	/** @format float */
	minEu: number
	/** @format float */
	maxEu: number
	/** @format float */
	minRaw: number
	/** @format float */
	maxRaw: number
	formulaInputs: TagInputInfo[]
}

export interface TagInputInfo {
	/** @format int32 */
	id: number
	/** @minLength 1 */
	name: string
	/** @minLength 1 */
	variableName: string
}

export interface TagAsInputInfo {
	/** @format int32 */
	id: number
	/** @minLength 1 */
	name: string
	type: TagType
}

export interface TagUpdateRequest {
	/** @minLength 1 */
	name: string
	description?: string | null
	type: TagType
	intervalInSeconds: number
	/** @format int32 */
	sourceId: number
	sourceType: SourceType
	sourceItem?: string | null
	formula?: string | null
	isScaling: boolean
	/** @format float */
	minEu: number
	/** @format float */
	maxEu: number
	/** @format float */
	minRaw: number
	/** @format float */
	maxRaw: number
	formulaInputs: TagInputInfo[]
}

export interface CreateUserGroupRequest {
	/** @minLength 1 */
	name: string
	/** @format guid */
	parentGuid?: string | null
	description?: string | null
}

export interface UserGroupInfo {
	/**
	 * @format guid
	 * @minLength 1
	 */
	guid: string
	/** @minLength 1 */
	name: string
	description?: string | null
	/** @format guid */
	parentGroupGuid?: string | null
}

export type UserGroupTreeInfo = UserGroupInfo & {
	children: UserGroupTreeInfo[]
	/** @format guid */
	parentGuid?: string | null
	parent?: UserGroupTreeInfo | null
}

export type UserGroupDetailedInfo = UserGroupInfo & {
	users: UserGroupUsersInfo[]
	subgroups: UserGroupInfo[]
}

export interface UserGroupUsersInfo {
	/** @format guid */
	guid?: string
	accessType?: AccessType
	fullName?: string | null
}

export enum AccessType {
	NoAccess = 'NoAccess',
	Viewer = 'Viewer',
	User = 'User',
	Admin = 'Admin',
	NotSet = 'NotSet',
}

export type UpdateUserGroupRequest = CreateUserGroupRequest & {
	accessType?: AccessType
	users: UserGroupUsersInfo[]
	groups: UserGroupInfo[]
}

export interface UserAuthInfo {
	/**
	 * @format guid
	 * @minLength 1
	 */
	guid: string
	/** @minLength 1 */
	login: string
	/** @minLength 1 */
	token: string
	globalAccessType: AccessType
	rights: UserAccessRightsInfo[]
}

export interface UserAccessRightsInfo {
	isGlobal?: boolean
	/** @format int32 */
	tagId?: number | null
	/** @format int32 */
	sourceId?: number | null
	/** @format int32 */
	blockId?: number | null
	accessType: AccessType
}

export interface UserKeycloakInfo {
	/**
	 * @format guid
	 * @minLength 1
	 */
	keycloakGuid: string
	/** @minLength 1 */
	login: string
	/** @minLength 1 */
	fullName: string
}

export interface UserLoginPass {
	/** @minLength 1 */
	login: string
	/** @minLength 1 */
	password: string
}

export interface UserCreateRequest {
	/** @minLength 1 */
	loginName: string
	fullName?: string | null
	password?: string | null
	staticHost?: string | null
	accessType: AccessType
}

export interface UserInfo {
	/**
	 * @format guid
	 * @minLength 1
	 */
	guid: string
	/** @minLength 1 */
	login: string
	fullName?: string | null
	accessType: AccessType
	isStatic: boolean
	userGroups: UserGroupsInfo[]
}

export interface UserGroupsInfo {
	/** @minLength 1 */
	guid: string
	/** @minLength 1 */
	name: string
}

export type UserDetailInfo = UserInfo & {
	/** @minLength 1 */
	hash: string
	staticHost?: string | null
}

export interface UserUpdateRequest {
	/** @minLength 1 */
	loginName: string
	staticHost?: string | null
	password?: string | null
	fullName?: string | null
	accessType: AccessType
	createNewStaticHash: boolean
}

export interface ValuesResponse {
	/** @format int32 */
	id: number
	/** @minLength 1 */
	tagName: string
	type: TagType
	func: AggregationFunc
	values: ValueRecord[]
}

export enum AggregationFunc {
	List = 'List',
	Sum = 'Sum',
	Avg = 'Avg',
	Min = 'Min',
	Max = 'Max',
}

export interface ValueRecord {
	/**
	 * @format date-time
	 * @minLength 1
	 */
	date: string
	/** @minLength 1 */
	dateString: string
	value: any
	quality: TagQuality
	using: TagUsing
}

export enum TagQuality {
	Bad = 'Bad',
	BadNoConnect = 'Bad_NoConnect',
	BadNoValues = 'Bad_NoValues',
	BadManualWrite = 'Bad_ManualWrite',
	Good = 'Good',
	GoodManualWrite = 'Good_ManualWrite',
	Unknown = 'Unknown',
}

export enum TagUsing {
	Initial = 'Initial',
	Basic = 'Basic',
	Aggregated = 'Aggregated',
	Continuous = 'Continuous',
	Outdated = 'Outdated',
	NotFound = 'NotFound',
}

export interface ValuesRequest {
	tags?: number[] | null
	tagNames?: string[] | null
	/** @format date-time */
	old?: string | null
	/** @format date-time */
	young?: string | null
	/** @format date-time */
	exact?: string | null
	/** @format int32 */
	resolution?: number | null
	func?: AggregationFunc | null
}

export interface ValueWriteRequest {
	/** @format int32 */
	tagId?: number | null
	tagName?: string | null
	value?: any
	/** @format date-time */
	date?: string | null
	tagQuality?: TagQuality | null
}

export type ValuesGetPayload = ValuesRequest[]

export type ValuesWritePayload = ValueWriteRequest[]
