import {
	AccessObject,
	AccessObjectToString,
} from '../../api/models/accessObject'

const withId = (route: string, id: string | number) =>
	route.replace(':id', String(id))

const routes = {
	settings: '/settings',
	users: {
		root: '/users',
		list: '/users/',
		create: '/users/create',
		view: '/users/:id',
		toUser(guid: string) {
			return withId(this.view, guid)
		},
	},
	userGroups: {
		root: '/user-groups',
		list: '/user-groups/',
		move: '/user-groups/move',
		view: '/user-groups/:id/view',
		edit: '/user-groups/:id/edit',
		access: {
			edit: '/user-groups/:id/edit/access',
		},
		toList() {
			return this.list
		},
		toUserGroup(guid: string) {
			return withId(this.view, guid)
		},
		toUserGroupEdit(guid: string) {
			return withId(this.edit, guid)
		},
		toUserGroupAccessForm(guid: string) {
			return withId(this.access.edit, guid)
		},
	},
	auth: {
		loginPage: '/login',
		energoId: '/energo-id',
	},
	viewer: {
		root: '/viewer',
		tagsViewer: '/tags',
	},
	tags: {
		root: '/tags',
		toTag(guid: string) {
			return this.root + '/' + guid
		},
	},
	blocks: {
		root: '/blocks',
		view: '/view',
		edit: '/edit',
		mover: '/mover',
		toViewBlock(id: number) {
			return `${this.root}${this.view}/${id}`
		},
		toEditBlock(id: number) {
			return `${this.root}${this.edit}/${id}`
		},
	},
	access: {
		root: '/access',
		form: '/form/:object?/:id?',
		toForm(object: AccessObject, id: string | number) {
			return (
				this.root +
				this.form
					.replace(':object?', AccessObjectToString(object))
					.replace(':id?', String(id))
			)
		},
	},
	sources: {
		root: '/sources',
		list: '/sources/',
		view: '/sources/:id',
		edit: '/sources/edit/:id',
		toViewSource(id: string | number) {
			return withId(this.view, id)
		},
		toEditSource(id: string | number) {
			return withId(this.edit, id)
		},
	},
	globalRoot: '/',
	offline: '/offline',
}

export default routes
