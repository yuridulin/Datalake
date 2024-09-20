import { createContext, useContext } from 'react'

export type UpdateContextType = {
	lastUpdate: Date
	setUpdate: (x: Date) => void
	isDarkMode: boolean
	setDarkMode: (x: boolean) => void
}

export const UpdateContext = createContext<UpdateContextType>({
	lastUpdate: new Date(),
	setUpdate: () => {},
	isDarkMode: false,
	setDarkMode: () => {},
})

export const useUpdateContext = () => useContext(UpdateContext)
