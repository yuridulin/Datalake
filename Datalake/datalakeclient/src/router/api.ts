export const API = {
	tags: {
		getFlatList: 'tags/list',
		create: 'tags/create',
		getCalculatedTags: 'tags/calculatedList',
		getManualTags: 'tags/manualList',
		getLiveValues: 'tags/live',
		getHistoryValues: 'tags/history',
	},
	sources: {
		list: 'sources/list',
		readById: 'sources/read',
		create: 'sources/create',
		update: 'sources/update',
		del: 'sources/delete',
	},
	blocks: {
		list: 'blocks/list',
		create: 'blocks/create',
	},
	config: {
		stats: 'config/statistic'
	}
}