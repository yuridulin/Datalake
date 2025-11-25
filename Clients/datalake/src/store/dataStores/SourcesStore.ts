import { Api } from '@/generated/Api'
import { SourceActivityInfo, SourceInfo } from '@/generated/data-contracts'
import { makeObservable, observable, runInAction } from 'mobx'
import { BaseCacheStore } from './BaseCacheStore'

/**
 * Store для управления источниками с кэшированием
 */
export class SourcesStore extends BaseCacheStore {
	// Кэш списка источников
	private _sourcesCache = observable.box<SourceInfo[] | null>(null)

	// Кэш состояний активности источников (ключ: sourceId)
	private _activityCache = observable.map<number, SourceActivityInfo>()

	// TTL для разных типов данных (в миллисекундах)
	private readonly TTL_LIST = 5 * 60 * 1000 // 5 минут для списка
	private readonly TTL_ACTIVITY = 30 * 1000 // 30 секунд для состояний активности

	constructor(private api: Api) {
		super()
		makeObservable(this, {
			getSources: true,
			getActivity: true,
			isLoadingSources: true,
			invalidateSource: true,
			refreshSources: true,
			refreshActivity: true,
		})
	}

	/**
	 * Получает список всех источников
	 * @returns Список источников
	 */
	getSources(): SourceInfo[] {
		const cacheKey = 'sources_list'
		const cached = this._sourcesCache.get()

		if (cached && this.isCacheValid(cacheKey, this.TTL_LIST)) {
			if (this.shouldRefresh(cacheKey, this.TTL_LIST)) {
				this.loadSources().catch(console.error)
			}
			return cached
		}

		if (!this.isLoading(cacheKey)) {
			this.loadSources().catch(console.error)
		}

		return cached ?? []
	}

	/**
	 * Получает состояния активности для указанных источников
	 * @param sourceIds Массив идентификаторов источников
	 * @returns Объект с состояниями активности (ключ: sourceId)
	 */
	getActivity(sourceIds: number[]): Record<number, SourceActivityInfo> {
		if (sourceIds.length === 0) return {}

		const result: Record<number, SourceActivityInfo> = {}
		const needRefresh: number[] = []

		// Проверяем кэш для каждого источника
		for (const sourceId of sourceIds) {
			const cacheKey = `activity_${sourceId}`
			const cached = this._activityCache.get(sourceId)

			if (cached && this.isCacheValid(cacheKey, this.TTL_ACTIVITY)) {
				result[sourceId] = cached
				if (this.shouldRefresh(cacheKey, this.TTL_ACTIVITY)) {
					needRefresh.push(sourceId)
				}
			} else {
				needRefresh.push(sourceId)
			}
		}

		// Обновляем в фоне, если нужно
		if (needRefresh.length > 0 && !this.isLoading('activity')) {
			this.loadActivity(needRefresh).catch(console.error)
		}

		return result
	}

	/**
	 * Проверяет, идет ли загрузка источников
	 * @returns true, если идет загрузка
	 */
	isLoadingSources(): boolean {
		return this.isLoading('sources_list')
	}

	/**
	 * Инвалидирует кэш для конкретного источника
	 * @param id Идентификатор источника
	 */
	invalidateSource(id: number): void {
		runInAction(() => {
			this._activityCache.delete(id)
			// Инвалидируем список, так как он может содержать этот источник
			this.invalidateCache('sources_list')
		})
	}

	/**
	 * Принудительно обновляет список источников
	 */
	async refreshSources(): Promise<void> {
		this.invalidateCache('sources_list')
		await this.loadSources()
	}

	/**
	 * Принудительно обновляет состояния активности
	 * @param sourceIds Массив идентификаторов источников
	 */
	async refreshActivity(sourceIds: number[]): Promise<void> {
		for (const sourceId of sourceIds) {
			this.invalidateCache(`activity_${sourceId}`)
		}
		await this.loadActivity(sourceIds)
	}

	/**
	 * Загружает список источников с сервера
	 */
	private async loadSources(): Promise<void> {
		const cacheKey = 'sources_list'

		if (this.isLoading(cacheKey)) return

		this.setLoading(cacheKey, true)

		try {
			const response = await this.api.inventorySourcesGetAll({
				withCustom: true,
			})

			if (response.status === 200) {
				runInAction(() => {
					this._sourcesCache.set(response.data)
					this.setLastFetchTime(cacheKey)
				})
			}
		} catch (error) {
			console.error('Failed to load sources:', error)
		} finally {
			this.setLoading(cacheKey, false)
		}
	}

	/**
	 * Загружает состояния активности источников
	 * @param sourceIds Массив идентификаторов источников
	 */
	private async loadActivity(sourceIds: number[]): Promise<void> {
		if (sourceIds.length === 0) return

		const cacheKey = 'activity'

		if (this.isLoading(cacheKey)) return

		this.setLoading(cacheKey, true)

		try {
			const response = await this.api.dataSourcesGetActivity({
				sourceIds: sourceIds,
			})

			if (response.status === 200) {
				runInAction(() => {
					for (const activity of response.data) {
						this._activityCache.set(activity.sourceId, activity)
						this.setLastFetchTime(`activity_${activity.sourceId}`)
					}
				})
			}
		} catch (error) {
			console.error('Failed to load sources activity:', error)
		} finally {
			this.setLoading(cacheKey, false)
		}
	}
}
