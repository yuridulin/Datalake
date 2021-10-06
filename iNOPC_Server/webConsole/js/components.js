var ID = 0
var accessType = 0
var login = null
var route = ''
var currentPage = ''

var reconnectTimeout = 0

var hash = ''

var routes = {
	'home': Home,
	'user': Login,
	'settings': Settings,
	'driver': function (args) {
		return Driver(args[1])
	},
	'device': function (args) {
		return Device(args[1])
	},
	'createdriver': DriverCreate
}

window.onload = function () {
	console.log('start checking connection')
	ask({ method: 'api/check ' }, function () {
		console.log('check done, go to first navigate')
		navigate()
	})
}

window.onhashchange = function () {
	navigate()
}

function navigate() {
	var new_hash = location.hash.replace('#', '')
	if (new_hash != hash) {
		console.log('navigate to ' + new_hash)
		hash = new_hash
		var parts = hash.split('/').filter(function (x) { return x != '' })
		if (routes[parts[0]]) {
			routes[parts[0]](parts)
		}
		Tree()
	} else if (hash == '') {
		console.log('navigate to home (default)')
		go('home')
		Tree()
	}
}

function go(route) {
	location.hash = '#/' + route
}

var LogTypes = {
	0: 'Информация',
	1: 'Детали',
	2: 'Внимание',
	3: 'Ошибка'
}

function Home() {
	// первый вход - перенаправление на страницу настроек
	if (accessType == ACCESSTYPE.FIRST || accessType == ACCESSTYPE.GUEST) return Login()

	ask({ method: 'api/tree' }, function (json) {
		mount('#view',
			h('table.datatable',
				h('thead',
					h('tr',
						h('th', { style: { width: '15em' }}, 'Драйвер'),
						h('th', 'Устройство')
					)
				),
				h('tbody',
					json.map(function (driver) {
						return driver.Devices.map(function (device) {
							return h('tr',
								h('td', driver.Name, {
									title: 'Нажмите для перехода к драйверу',
									onclick: function () {
										Driver(driver.Id)
									}
								}),
								h('td',
									h('i.ic.ic-' + (device.IsActive ? 'play' : 'pause'), AUTH() ? {
										title: 'Нажмите для ' + (device.IsActive ? 'завершения' : 'запуска') + ' опроса данных',
										onclick: function () {
											ask({ method: 'api/device.' + (device.IsActive ? 'stop' : 'start'), body: { Id: device.Id } }, Home)
										}
									} : {}),
									h('span', {
										innerHTML: device.Name,
										title: 'Нажмите для перехода к устройству',
										onclick: function () {
											Device(device.Id)
										}
									})
								)
							)
						})
					})
				)
			)
		)
	})
}

function Offline() {
	mount('#auth', '')
	mount('#tree', '')
	mount('#view', 'Нет связи с OPC сервером')
	clearInterval(deviceInverval)
	clearTimeout(reconnectTimeout)

	reconnectTimeout = setTimeout(function () {
		ask({ method: 'api/check' }, function () {
			navigate()
		})
	}, 5000)
}

function Tree() {
	if (accessType == ACCESSTYPE.FIRST || accessType == ACCESSTYPE.GUEST) return

	ask({ method: 'api/tree' }, function (json) {
		mount('#tree', h('div',
			accessType == ACCESSTYPE.FULL ?
				h('div.node',
					h('a.node-caption',
						{
							href: '#/settings/',
							title: 'Нажмите, чтобы перейти к настройке сервера'
						},
						h('i.ic.ic-menu'),
						h('span', 'Настройки')
					)
				) : '',

			accessType == ACCESSTYPE.READ || AUTH() ?
				json.map(function (driver) {
					return h('div.node' + (ls(driver.Id) == 'open' ? '.open' : ''),
						h('div.node-caption',
							h('i.ic.ic-expand-' + (ls(driver.Id) == 'open' ? 'less' : 'more'), {
								onclick: function () {
									if (this.className.indexOf('less') > -1) {
										ls(driver.Id, 'close')
										this.className = 'ic ic-expand-more'
										this.parentNode.parentNode.className = 'node'
									}
									else {
										ls(driver.Id, 'open')
										this.className = 'ic ic-expand-less'
										this.parentNode.parentNode.className = 'node open'
									}
								}
							}),
							h('a', { href: '#/driver/' + driver.Id }, driver.Name, (driver.HasError ? h('i.ic.ic-warning.ic-inline') : '')),
						),
						h('div.node-body',
							driver.Devices.map(function (device) {
								return h('div.node',
									h('div.node-caption',
										h('i.ic.' + (device.IsActive ? 'ic-play' : 'ic-pause'), AUTH() ? {
											onclick: function () {
												if (device.IsActive) ask({ method: 'api/device.stop', body: { Id: device.Id } })
												else ask({ method: 'api/device.start', body: { Id: device.Id } })
											}
										} : {}),
										h('a', { href: '#/device/' + device.Id }, device.Name, (device.HasError ? h('i.ic.ic-warning.ic-inline') : ''))
									)
								)
							})
						)
					)
				})
				: '',

			AUTH() ? h('div.node',
				h('div.node-caption',
					h('i.ic.ic-plus'),
					h('a', {
						innerHTML: 'Добавить драйвер',
						href: '#/createdriver'
					})
				)
			) : ''
		))
	})
}


function Driver(id) {
	clearInterval(inverval)
	ID = id

	ask({ method: 'api/driver', body: { Id: id } }, function (driver) {
		if (driver.Warning) return mount('#view', driver.Warning)
		if (!driver.Name) return mount('#view', 'Ошибка получения данных с сервера')
		
		var name, path
		mount('#view',
			h('div.container',

				h('span', 'Имя'),

				AUTH()
					? name = h('input', { type: 'text', value: driver.Name })
					: h('span.value', driver.Name),

				h('span', 'Тип'),

				AUTH()
					? path = h('select', { disabled: true },
						driver.Dlls.map(function (x) {
							return x == driver.Path ? h('option', x, { selected: true }) : ''
						})
					)
					: h('span.value', driver.Path),

				AUTH()
					? h('button', {
						innerHTML: 'Перезагрузить',
						onclick: function () {
							ask({ method: 'api/driver.reload', body: { Id: id } })
						}
					})
					: '',
				AUTH()
					? h('button', {
						innerHTML: 'Сохранить',
						onclick: function () {
							ask({ method: 'api/driver.update', body: { Id: id, Name: name.value, Path: path.value } })
						}
					})
					: '',
				AUTH()
					? h('button', {
						innerHTML: 'Удалить',
						onclick: function () {
							if (!confirm('Драйвер будет удален из конфигурации без возможности восстановления. Продолжить?')) return
							ask({ method: 'api/driver.delete', body: { Id: id } }, Home)
						}
					})
					: ''
			),

			h('div.container',
				h('div.container-caption', 'Логи'),
				h('table',
					h('tr',
						h('th', { style: { width: '10em' }}, 'Дата'),
						h('th', { style: { width: '7.5em' }}, 'Тип'),
						h('th', 'Сообщение'),
					)
				),
				h('div#driver-logs.sub'),
			),

			h('div.container',
				h('div.container-caption', 'Устройства'),
				h('div#driver-devices.sub'),
			)
		)
		DriverLogs(id)
		DriverDevices(id)
	})
}

function DriverLogs(id) {
	ask({ method: 'api/driver.logs', body: { Id: id } }, function (logs) {
		mount('#driver-logs',
			h('table', logs
				.map(function (x) {
					return h('tr',
						h('td', { style: { width: '10em' } }, x.Date),
						h('td.type' + x.Type, { style: { width: '7.5em' } }, LogTypes[x.Type]),
						h('td', x.Text)
					)
				})
			)
		)
    })
}

function DriverDevices(id) {
	ask({ method: 'api/driver.devices', body: { Id: id } }, function (devices) {
		mount('#driver-devices',
			h('div.devices', devices.map(function (device) {
				return h('div',
					h('i.ic.ic-' + (device.IsActive ? 'play' : 'pause'), AUTH() ? {
						onclick: function () {
							if (!device.IsActive) {
								ask({ method: 'api/device.start', body: { Id: device.Id } })
							} else {
								ask({ method: 'api/device.stop', body: { Id: device.Id } })
							}
						}
					} : { }),
					h('span', {
						innerHTML: device.Name,
						onclick: function () {
							Device(device.Id)
						}
					})
				)
			})),
			AUTH() ? h('button', {
				innerHTML: 'Добавить устройство',
				onclick: function () {
					ask({ method: 'api/device.create', body: { Id: id } }, function () {
						DriverDevices(id)
					})
				}
			}) : ''
		)
	})
}

function DriverCreate() {
	clearInterval(inverval)
	if (!AUTH()) return

	ask({ method: 'api/driver.createform' }, function (data) {
		ID = 0
		var name, path
		mount('#view',
			h('div.container',

				h('span', 'Dll сборка'),
				path = h('select',
					data.map(function (x) {
						return h('option', x)
					})
				),

				h('span', 'Наименование'),
				name = h('input'),
				h('br'),
				h('br'),
				h('div',
					h('button', {
						innerHTML: 'Добавить',
						onclick: function () {
							ask({ method: 'api/driver.create', body: { Name: name.value, Path: path.value } }, function (data) {
								if (data.Error) alert(data.Error)
								else if (data.Id) Driver(data.Id);
							});
						}
					}),
					h('button', {
						innerHTML: 'Закрыть',
						onclick: Home
					})
				)
			)
		)
	})
}

var deviceName
var deviceAutoStart
var inverval = 0
var deviceInverval = 0

function Device(id) {

	lastLog = ''
	ID = id

	var x1, x2, x3, logsDetailed, logsWarnings

	if (ls(id + '.logs') == null) {
		ls(id + '.logs', 'open')
	}

	ls('device.' + id + '.last', undefined)

	ask({ method: 'api/device', body: { Id: id } }, function (device) {
		if (!device.Name) return mount('#view', 'Ошибка получения данных с сервера')
		
		mount('#view',
			h('div.container',
				h('span', 'Имя'),
				deviceName = AUTH()
					? h('input', { value: device.Name })
					: h('span.value', device.Name),

				AUTH()
					? h('span', 'Автостарт')
					: '',

				deviceAutoStart = AUTH()
					? h('input', device.AutoStart ? { type: 'checkbox', checked: true } : { type: 'checkbox' })
					: '',

				AUTH()
					? h('button', {
						innerHTML: 'Старт',
						onclick: function () {
							ask({ method: 'api/device.start', body: { Id: id } }, function () {
								Device(id)
							})
						}
					})
					: '',
				AUTH()
					? h('button', {
						innerHTML: 'Стоп',
						onclick: function () {
							ask({ method: 'api/device.stop', body: { Id: id } }, function () {
								Device(id)
							})
						}
					})
					: '',
				AUTH()
					? h('button', {
						innerHTML: 'Сохранить',
						onclick: function () {
							DeviceSave(id)
						}
					})
					: '',
				AUTH()
					? h('button', {
						innerHTML: 'Удалить',
						onclick: function () {
							if (!confirm('Устройство будет удалено из конфигурации без возможности восстановления. Продолжить?')) return
							ask({ method: 'api/device.delete', body: { Id: id } }, Home)
						}
					})
					: ''
			),

			AUTH() ? h('div.container',
				h('div',
					h('span.container-expand-button',
						{
							onclick: function () {
								x1.classList.toggle('closed')
								this.querySelector('i').classList.toggle('ic-expand-more')
								this.querySelector('i').classList.toggle('ic-expand-less')
								ls(id + '.settings', x1.classList.contains('closed') ? 'close' : 'open')
							}
						},
						h('i.ic.ic-expand-' + (ls(id + '.settings') == 'open' ? 'more' : 'less')),
						h('span.container-expand-caption', 'Настройки')
					)
				),
				x1 = h(ls(id + '.settings') == 'open' ? 'div.container-data' : 'div.container-data.closed',
					{
						style: { height: ls(id + '.settings.h') },
						onclick: function () {
							ls(id + '.settings.h', this.style.height)
						}
					},
					h('div#device-configuration.sub')
				)
			) : '',

			h('div.container',
				h('div',
					h('span.container-expand-button',
						{
							onclick: function () {
								x2.classList.toggle('closed')
								this.querySelector('i').classList.toggle('ic-expand-more')
								this.querySelector('i').classList.toggle('ic-expand-less')
								ls(id + '.logs', x2.classList.contains('closed') ? 'close' : 'open')
							}
						},
						h('i.ic.ic-expand-' + (ls(id + '.logs') == 'open' ? 'more' : 'less')),
						h('span.container-expand-caption', 'Логи')
					)
				),
				x2 = h(ls(id + '.logs') == 'open' ? 'div.container-data' : 'div.container-data.closed',
					{
						style: { height: ls(id + '.logs.h') },
						onclick: function () {
							ls(id + '.logs.h', this.style.height)
						}
					},
					h('label',
						logsDetailed = h('input', { 
							type: 'checkbox',
							onchange: function () {
								ls(id + '.logs.detailed', this.checked ? 'true' : 'false')
								DeviceLogs(id, true)
							}
						}),
						'детали'
					),
					h('label',
						logsWarnings = h('input', { 
							type: 'checkbox',
							onchange: function () {
								ls(id + '.logs.warnings', this.checked ? 'true' : 'false')
								DeviceLogs(id, true)
							}
						}),
						'предупреждения'
					),
					h('button', {
						innerHTML: 'Очистить',
						onclick: function () {
							mount('#device-logs', h('table'))
                        }
					}),
					h('table',
						h('tr',
							h('th', { style: { width: '14em' }}, 'Дата'),
							h('th', { style: { width: '8em' }}, 'Тип'),
							h('th', 'Сообщение'),
						)
					),
					h('div#device-logs.sub', h('table'))
				)
			),

			h('div.container',
				h('div',
					h('span.container-expand-button',
						{
							onclick: function () {
								x3.classList.toggle('closed')
								this.querySelector('i').classList.toggle('ic-expand-more')
								this.querySelector('i').classList.toggle('ic-expand-less')
								ls(id + '.fields', x3.classList.contains('closed') ? 'close' : 'open')
							}
						},
						h('i.ic.ic-expand-' + (ls(id + '.fields') == 'open' ? 'more' : 'less')),
						h('span.container-expand-caption', 'Опрашиваемые параметры')
					)
				),
				x3 = h(ls(id + '.fields') == 'open' ? 'div.container-data' : 'div.container-data.closed',
					{
						style: { height: ls(id + '.fields.h') },
						onclick: function () {
							ls(id + '.fields.h', this.style.height)
						}
					},
					h('table',
						h('tr',
							h('th', { style: { width: '12em' }}, 'Параметр'),
							h('th', { style: { width: '18em' }}, 'Значение'),
							h('th', 'Качество'),
						)
					),
					h('div#device-fields.sub')
				)
			)
		)

		if (ls(id + '.logs.detailed') == 'true') {
			logsDetailed.checked = true
		}
		if (ls(id + '.logs.warnings') == 'true') {
			logsWarnings.checked = true
		}

		DeviceConfiguration(id)
		DeviceLogs(id)
		DeviceFields(id)

		clearInterval(deviceInverval)
		deviceInverval = setInterval(function () {
			DeviceLogs(id)
			DeviceFields(id)
		}, 1000)
	})
}

function DeviceConfiguration(id) {
	if (!AUTH()) return
	ask({ method: 'api/device.configuration', body: { Id: id } }, function (page) {
		if (!AUTH()) return
		mount('#device-configuration', h('div.form', page))
		document.getElementById('device-configuration').querySelectorAll('script').forEach(function (el) {
			(1, eval)(el.innerHTML)
		})
	})
}

function DeviceLogs(id) {
	ask({ method: 'api/device.logs', body: { Id: id, Last: ls("device." + id + ".last") } }, function (logs) {
		if (logs.length == 0) return
		ls("device." + id + ".last", logs[logs.length - 1].Id)
		logs.forEach(function (x) {
			DeviceLog(id, x)
		})
		DeviceLogsClean()
	})
}

function DeviceLog(id, log) {
	if ((ls(id + '.logs.detailed') == 'false' || ls(id + '.logs.detailed') == null) && log.Type == 1) return
	if ((ls(id + '.logs.warnings') == 'false' || ls(id + '.logs.warnings') == null) && log.Type == 2) return

	var div = document.getElementById('device-logs')
	var table = div.querySelector('table')
	if (!table) return

	var needScroll = div.scrollHeight < (div.getBoundingClientRect().height + div.scrollTop)

	table.appendChild(
		h('tr',
			h('td', { style: { width: '14em' } }, log.Date),
			h('td.type' + log.Type, { style: { width: '8em' } }, LogTypes[log.Type]),
			h('td', log.Text)
		)
	)

	if (needScroll) div.scrollTop = div.scrollHeight
}

function DeviceLogsClean() {
	var div = document.getElementById('device-logs')
	var table = div.querySelector('table')
	var rows = table.getElementsByTagName('tr')
	console.log(rows.length)

	if (rows.length >= 100) {
		var i = rows.length - 100
		while (i >= 0) {
			table.removeChild(rows[i])
			i--
		}
	}
}

function DeviceFields(id) {
	ask({ method: 'api/device.fields', body: { Id: id } }, function (fields) {
		if (ID != id) return
		mount('#device-fields', 
			h('table',
				Object.keys(fields).map(function (key) {
					return h('tr',
						h('td', { style: { width: '12em' }}, key),
						h('td', { style: { width: '18em' }}, fields[key].Value),
						h('td', fields[key].Quality)
					)
				})
			)
		)
	})
}

function DeviceSave(id) {
	if (!AUTH()) return
	var config = {}
	document.querySelector('#device-configuration .form').querySelectorAll('div[type]').forEach(function (el) {

		if (el.getAttribute('type') == 'value') {
			var input = el.querySelector('input')
			config[input.name] = input.type == 'checkbox' ? input.checked : input.value
		}

		else if (el.getAttribute('type') == 'array') {
			var arr = []
			var parts = el.querySelectorAll('p')
			parts.forEach(function (p) {
				var x = {}
				p.querySelectorAll('input,select').forEach(function (i) {
					x[i.name] = i.type == 'checkbox' ? i.checked : i.value
				})
				arr.push(x)
			})
			config[el.getAttribute('name')] = arr
		}
	})

	var form = {
		Id: id,
		Name: deviceName.value,
		AutoStart: deviceAutoStart.checked,
		Configuration: JSON.stringify(config)
	}

	// оправка формы на сервер
	ask({ method: 'api/device.update', body: form }, function () {
		alert('Устройство сохранено')
	})
}


function Settings() {

	if (currentPage == 'settings') return
	currentPage = 'settings'

	ID = 0
	clearInterval(inverval)

	if (accessType == ACCESSTYPE.GUEST || accessType == ACCESSTYPE.READ) {
		return mount('#view', h('div.container', 'Нет доступа'))
	}

	mount('#view',
		h('div.container',
			h('button', {
				innerHTML: 'Создать службу и инициализировать DCOM',
				onclick: function () {
					ask({ method: 'api/opc.dcom' }, function (data) {
						if (data) alert('Инициализация DCOM выполнена')
					})
				}
			}),
			h('button', {
				innerHTML: 'Реинициализация OPC тегов',
				onclick: function () {
					ask({ method: 'api/opc.clean' }, function (data) {
						if (data) alert('Реинициализация OPC тегов выполнена')
					})
				}
			})
		),
		h('div#users')
	)

	UsersTable()
}

function UsersTable() {

	if (currentPage == 'users') return
	currentPage = 'users'

	ID = 0
	if (accessType < ACCESSTYPE.FULL) return ''
	var login, pass, access

	ask({ method: 'api/users' }, function (json) {
		if (!document.getElementById('users')) return
		mount('#users',
			h('div.container',
				h('table',
					h('tr',
						h('th', 'Учётная запись'),
						h('th', 'Доступ'),
						h('th', { colspan: 2 }, 'Управление')
					),
					json.map(function (user) {
						return h('tr',
							h('td', user.Login),
							h('td', AccessType(user.AccessType)),
							h('td',
								h('button', {
									innerHTML: 'Удалить',
									onclick: function () {
										ask({ method: 'api/user.delete', body: { Login: user.Login } }, function (json) {
											if (json.Done) UsersTable()
										})
									}
								}),
							)
						)
					}),
					h('tr',
						h('td',
							login = h('input', {
								type: 'text',
								placeholder: 'логин...',
								readonly: true,
								onfocus: function () {
									this.removeAttribute('readonly')
								}
							})
						),
						h('td',
							pass = h('input', {
								type: 'password',
								placeholder: 'пароль...',
								readonly: true,
								onfocus: function () {
									this.removeAttribute('readonly')
								}
							})
						),
						h('td',
							access = h('select',
								h('option', { innerHTML: AccessType(ACCESSTYPE.READ), value: ACCESSTYPE.READ }),
								h('option', { innerHTML: AccessType(ACCESSTYPE.WRITE), value: ACCESSTYPE.WRITE }),
								h('option', { innerHTML: AccessType(ACCESSTYPE.FULL), value: ACCESSTYPE.FULL }),
							)
						),
						h('td',
							h('button', {
								innerHTML: 'Добавить',
								onclick: function () {
									var body = {
										Login: login.value,
										Password: pass.value,
										AccessType: access.value
									}

									ask({ method: 'api/user.create', body: body }, function (json) {
										if (json.Done) UsersTable()
									})
								}
							})
						)
					)
				)
			)
		)
	})
}


function AuthPanel() {
	mount('#auth',
		accessType == ACCESSTYPE.FIRST || accessType == ACCESSTYPE.GUEST
			? h('a.panel-el',
				{
					href: '#/user',
					title: 'Нажмите, чтобы зайти под учётной записью',
				},
				h('span', 'Вход не выполнен'),
				h('button', 'Войти')
			)
			: h('a.panel-el',
				{
					href: '#/user',
					title: 'Нажмите, чтобы выйти из учётной записи',
				},
				h('i.ic.ic-person'),
				h('span', login)
			)
	)
}

function Login() {

	var _login, _pass

	if (accessType == ACCESSTYPE.FIRST) {
		mount('#view',
			h('container',
				h('h3', 'Первоначальная настройка'),
				h('p', 'Введите пароль для создания первой учётной записи с полным доступом'),
				_pass = h('input', {
					type: 'password',
					placeholder: 'пароль...'
				}),
				h('button', 'Сохранить', {
					onclick: function () {
						var _body = {
							Login: 'admin',
							Password: _pass.value,
							AccessType: ACCESSTYPE.FULL
						}
						ask({ method: 'api/user.create', body: _body }, function (json) {
							if (json.Done) {
								ask({ method: 'api/login', body: _body }, function (json) {
									if (json.Done) location.reload()
								})
							}
						})
					}
				})
			)
		)
	}
	else if (accessType == ACCESSTYPE.GUEST) {
		mount('#view',
			h('div.container',
				h('p', 'Вы не вошли в учетную запись. Выполните вход:'),

				h('span', 'Имя учётной записи'),
				_login = h('input', { type: 'text', name: 'login' }),
				h('span', 'Пароль'),
				_pass = h('input', { type: 'password', name: 'password' }),

				h('button', {
					innerHTML: 'Вход',
					onclick: function () {
						var _body = {
							Login: _login.value,
							Password: _pass.value
						}
						ask({ method: 'api/login', body: _body }, function (json) {
							if (json.Done) location.reload()
						})
					}
				})
			)
		)
	}
	else {
		mount('#view',
			h('div.container',
				h('p', 'Вы вошли как ' + login),
				h('button', 'Выйти', {
					onclick: function () {
						ask({ method: 'api/logout', body: { Token: ls('Inopc-Access-Token') } }, function (json) {
							if (json.Done) {
								localStorage.removeItem('Inopc-Access-Token')
								location.reload()
							}
						})
					}
				})
			)
		)
	}
}

var ACCESSTYPE = {
	GUEST: 0,
	READ: 1,
	WRITE: 2,
	FULL: 3,
	FIRST: 4,
}

function AccessType(t) {
	switch (t) {
		case ACCESSTYPE.FIRST: return 'Первый запуск'
		case ACCESSTYPE.GUEST: return 'Гость'
		case ACCESSTYPE.READ: return 'Доступ на чтение'
		case ACCESSTYPE.WRITE: return 'Доступ на запись'
		case ACCESSTYPE.FULL: return 'Полный доступ'
		default: return 'Тип не найден'
	}
}

function AUTH() {
	return accessType == ACCESSTYPE.WRITE || accessType == ACCESSTYPE.FULL
}