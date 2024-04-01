import { AccessType } from "./enums/AccessType"

export interface User {
	Name: string
	FullName: string
	AccessType: AccessType
	Password: string
	Hash: string
	StaticHost: string | null
}