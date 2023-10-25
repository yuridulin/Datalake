import { TreeType } from "./enums/treeType"

export interface TreeItem {
	Id: number
	Name: string
	FullName: string
	Type: TreeType
	Items: TreeItem[]
	IsChecked: boolean
}