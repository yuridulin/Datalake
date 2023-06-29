export interface Block {
	Id: number
	ParentId: number
	Name: string
	Description: string
	Children: Block[]
	Tags: Tag[]
}