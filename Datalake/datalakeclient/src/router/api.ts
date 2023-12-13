export const API = {
	auth: {
		login: 'auth/login',
		logout: 'auth/logout',
		user: 'auth/userinfo',
		users: 'auth/users',
		create: 'auth/create',
		update: 'auth/update',
		delete: 'auth/delete',
	},
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
	console: {
		tree: 'console/tree',
	},
	config: {
		tree: 'config/tree',
		last: 'config/lastUpdate',
		stats: 'config/statistic',
	}
}