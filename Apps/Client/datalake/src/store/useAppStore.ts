import { AppStore } from '@/store/appStore'
import { useContext } from 'react'
import { AppStoreContext } from './appStoreContext'

export const useAppStore = (): AppStore => useContext(AppStoreContext)
