import { createContext, useContext } from 'react'

export type UpdateContextType = {
	lastUpdate: Date
	setUpdate: (x: Date) => void
	isDarkMode: boolean
}

export const UpdateContext = createContext<UpdateContextType>({
	lastUpdate: new Date(),
	setUpdate: () => {},
	isDarkMode: false,
})

export const useUpdateContext = () => useContext(UpdateContext)
