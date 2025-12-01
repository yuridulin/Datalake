import { AppStore } from '@/store/appStore'
import { AppStoreContext } from '@/store/appStoreContext'
import { useContext } from 'react'

export const useAppStore = (): AppStore => useContext(AppStoreContext)
