import { createContext, useContext } from "react"

export type UpdateContextType = {
	lastUpdate: Date
	setUpdate: (x: Date) => void

	checkedTags: string[]
	setCheckedTags: (id: string[]) => void
}

export const UpdateContext = createContext<UpdateContextType>({ 
	lastUpdate: new Date(),
	setUpdate: () => {},
	checkedTags: [],
	setCheckedTags: () => {},
})

export const useUpdateContext = () => useContext(UpdateContext)