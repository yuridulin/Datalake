import { createContext, useContext } from 'react'

export type UpdateContextType = {
	lastUpdate: Date
	setUpdate: (x: Date) => void
}

export const UpdateContext = createContext<UpdateContextType>({
	lastUpdate: new Date(),
	setUpdate: () => {},
})

export const useUpdateContext = () => useContext(UpdateContext)
