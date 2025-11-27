import { AppStore } from '@/store/AppStore'
import { useContext } from 'react'
import { AppStoreContext } from './AppStoreContext'

export const useAppStore = (): AppStore => useContext(AppStoreContext)
