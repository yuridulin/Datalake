import { Rel_Block_Type } from "./Rel_Block_Tag"

export interface BlockType {
	Id: number
	ParentId: number
	Name: string
	Description: string
	Properties: { [key: string]: string }
	Tags: Rel_Block_Type[]
	Children: BlockType[]
}