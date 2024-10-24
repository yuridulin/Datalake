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
		edit: '/users/:id/edit',
		toList() {
			return this.list
		},
		toUserForm(guid: string) {
			return withId(this.edit, guid)
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
		tagsViewer: '/viewer/tags',
	},
	tags: {
		root: '/tags',
		list: '/tags/all',
		manual: '/tags/manual',
		calc: '/tags/calculated',
		edit: '/tags/:id/edit',
		toTagForm(guid: string) {
			return withId(this.edit, guid)
		},
	},
	blocks: {
		root: '/blocks',
		list: '/blocks/',
		mover: '/blocks/mover',
		view: '/blocks/:id/view',
		edit: '/blocks/:id/edit',
		access: {
			edit: '/blocks/:id/edit/access',
		},
		toList() {
			return this.list
		},
		toMoveForm() {
			return this.mover
		},
		toViewBlock(id: number) {
			return withId(this.view, id)
		},
		toEditBlock(id: number) {
			return withId(this.edit, id)
		},
		toBlockAccessForm(id: number) {
			return withId(this.access.edit, id)
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
		edit: '/sources/:id/edit',
		toEditSource(id: string | number) {
			return withId(this.edit, id)
		},
	},
	globalRoot: '/',
	offline: '/offline',
}

export default routes
