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
	name: string
	description?: string
	parent?: BlockParentInfo
	children: BlockChildInfo[]
	properties: BlockPropertyInfo[]
	tags: BlockTagInfo[]
}

export type BlockParentInfo = BlockRelationInfo & object

export interface BlockRelationInfo {
	/** @format int32 */
	id: number
	name: string
}

export type BlockChildInfo = BlockRelationInfo & object

export type BlockPropertyInfo = BlockRelationInfo & object

export type BlockTagInfo = BlockRelationInfo & object

export interface BlockSimpleInfo {
	/** @format int32 */
	id: number
	name: string
	description?: string
}

export interface BlockTreeInfo {
	/** @format int32 */
	id: number
	name: string
	description?: string
	children: BlockTreeInfo[]
}

export interface SourceInfo {
	/** @format int32 */
	id: number
	name: string
	description?: string
	address?: string
	type: SourceType
}

export enum SourceType {
	Inopc = 'Inopc',
	Datalake = 'Datalake',
	Unknown = 'Unknown',
	Custom = 'Custom',
}

export interface SourceEntryInfo {
	itemInfo?: SourceItemInfo
	tagInfo?: SourceTagInfo
}

export interface SourceItemInfo {
	path: string
	type: TagType
}

export enum TagType {
	String = 'String',
	Number = 'Number',
	Boolean = 'Boolean',
}

export interface SourceTagInfo {
	/** @format int32 */
	id: number
	name: string
	item: string
	type: TagType
}

export interface TagInfo {
	/** @format int32 */
	id?: number
	name: string
	description?: string
	type: TagType
	intervalInSeconds?: number
	sourceInfo: TagSourceInfo
	mathInfo?: TagMathInfo
	calcInfo?: TagCalcInfo
}

export interface TagSourceInfo {
	/** @format int32 */
	id: number
	type: SourceType
	item?: string
	name: string
}

export interface TagMathInfo {
	/** @format float */
	minEu: number
	/** @format float */
	maxEu: number
	/** @format float */
	minRaw: number
	/** @format float */
	maxRaw: number
}

export interface TagCalcInfo {
	formula: string
	inputs: Record<string, string>
}

export interface UserAuthInfo {
	userName: string
	accessType: AccessType
}

export enum AccessType {
	NOT = 'NOT',
	USER = 'USER',
	ADMIN = 'ADMIN',
	FIRST = 'FIRST',
}

export interface UserLoginPass {
	name: string
	password: string
}

export interface UserAuthRequest {
	loginName: string
	fullName?: string
	password?: string
	staticHost?: string
	accessType: AccessType
}

export interface UserInfo {
	loginName: string
	fullName?: string
	accessType: AccessType
	isStatic: boolean
}

export interface UserUpdateRequest {
	loginName: string
	staticHost?: string
	password?: string
	fullName?: string
	accessType: AccessType
	createNewStaticHash: boolean
}

export interface ValuesResponse {
	/** @format int32 */
	id: number
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
	/** @format date-time */
	date: string
	value?: any
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
	tags: number[]
	tagNames: string[]
	/** @format date-time */
	old?: string
	/** @format date-time */
	young?: string
	/** @format date-time */
	exact?: string
	/** @format int32 */
	resolution: number
	func: AggregationFunc
}

export interface ValueWriteRequest {
	/** @format int32 */
	tagId?: number
	tagName?: string
	value?: any
	/** @format date-time */
	date?: string
	tagQuality?: TagQuality
}

export type ValuesGetValuesPayload = ValuesRequest[]

export type ValuesWriteValuesPayload = ValueWriteRequest[]
