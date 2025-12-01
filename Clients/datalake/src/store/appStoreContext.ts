import { createContext } from 'react'
import { AppStore } from './appStore'

export const AppStoreContext = createContext<AppStore>({} as AppStore)
export const AppStoreProvider = AppStoreContext.Provider
