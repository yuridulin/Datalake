var ID = 0
var accessType = 0
var login = null
var route = ''


var LogTypes = {
	0: 'Информация',
	1: 'Детали',
	2: 'Внимание',
	3: 'Ошибка'
}

function Home() {

	mount('#view', 'подключаемся...')

	ask({ method: 'tree' }, function (json) {

		// первый вход - перенаправление на страницу настроек
		if (accessType == ACCESSTYPE.FIRST) return First()

		if (accessType == ACCESSTYPE.GUEST) return Login()

		BuildTree(json)
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
											ask({ method: 'device.' + (device.IsActive ? 'stop' : 'start'), body: { Id: device.Id } }, Home)
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
	ID = 0
	mount('#tree', '')
	mount('#view', 'Нет связи с OPC сервером')
	clearInterval(timeout)
}

function Tree() {
	ask({ method: 'tree' }, function (json) {
		BuildTree(json)
    })
}

function BuildTree(json) {
	console.log(accessType, )
	mount('#tree', h('div',
		accessType == ACCESSTYPE.FULL ? 
			h('div.node', { route: 'settings' },
				h('div.node-caption',
					{
						title: 'Нажмите, чтобы перейти к настройке сервера',
						onclick: function () {
							TreeSetActive('settings')
							Settings()
						}
					},
					h('i.ic.ic-menu'),
					h('span', 'Настройки')
				)
			) : '',

		accessType == ACCESSTYPE.READ || AUTH() ?
			json.map(function (driver) {
				return h('div.node' + (ls(driver.Id) == 'open' ? '.open' : ''), { route: 'driver|' + driver.Id },
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
						h('span', driver.Name, (driver.HasError ? h('i.ic.ic-warning.ic-inline') : ''), {
							onclick: function () {
								TreeSetActive('driver|' + driver.Id)
								Driver(driver.Id)
							}
						}),
					),
					h('div.node-body',
						driver.Devices.map(function (device) {
							return h('div.node', { route: 'device|' + device.Id },
								h('div.node-caption',
									h('i.ic.' + (device.IsActive ? 'ic-play' : 'ic-pause'), AUTH() ? {
										onclick: function () {
											if (device.IsActive) ask({ method: 'device.stop', body: { Id: device.Id } })
											else ask({ method: 'device.start', body: { Id: device.Id } })
										}
									} : { }),
									h('span', device.Name, (device.HasError ? h('i.ic.ic-warning.ic-inline') : ''), {
										onclick: function () {
											TreeSetActive('device|' + device.Id)
											Device(device.Id)
										}
									})
								)
							)
						})
					)
				)
			})
			: '',

		AUTH() ? h('div.node', { route: 'driver-create' },
			h('div.node-caption',
				h('i.ic.ic-plus'),
				h('span', {
					innerHTML: 'Добавить драйвер',
					onclick: function () {
						TreeSetActive('driver-create')
						DriverCreate()
					}
				})
			)
		) : ''
	))

	TreeSetActive(route)
}

function TreeSetActive(r) {

	var old = document.querySelector('.active')
	if (old) old.classList.remove('active')

	route = r
	var el = document.querySelector('[route="' + r + '"]')
	if (el) el.classList.add('active')
}


function Driver(id) {
	clearInterval(timeout)
	mount('#view', 'выполняется запрос...')
	ask({ method: 'driver', body: { Id: id } }, function (driver) {
		if (!driver.Name) return mount('#view', 'Ошибка получения данных с сервера')
		ID = id
		var name, path
		mount('#view',
			h('div.container',

				h('span', 'Имя'),

				AUTH()
					? h('input', { type: 'text', value: driver.Name })
					: h('span.value', driver.Name),

				h('span', 'Тип'),

				AUTH()
					? h('select',
						driver.Dlls.map(function (x) {
							return x == driver.Path ? h('option', x, { selected: true }) : h('option', x)
						})
					)
					: h('span.value', driver.Path),

				AUTH()
					? h('button', {
						innerHTML: 'Перезагрузить',
						onclick: function () {
							ask({ method: 'driver.reload', body: { Id: id } })
						}
					})
					: '',
				AUTH()
					? h('button', {
						innerHTML: 'Сохранить',
						onclick: function () {
							ask({ method: 'driver.update', body: { Id: id, Name: name.value, Path: path.value } })
						}
					})
					: '',
				AUTH()
					? h('button', {
						innerHTML: 'Удалить',
						onclick: function () {
							if (!confirm('Драйвер будет удален из конфигурации без возможности восстановления. Продолжить?')) return
							ask({ method: 'driver.delete', body: { Id: id } }, Home)
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
	ask({ method: 'driver.logs', body: { Id: id } }, function (logs) {
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
	ask({ method: 'driver.devices', body: { Id: id } }, function (devices) {
		mount('#driver-devices',
			h('div.devices', devices.map(function (device) {
				return h('div',
					h('i.ic.ic-' + (device.IsActive ? 'play' : 'pause'), AUTH() ? {
						onclick: function () {
							if (!device.IsActive) {
								ask({ method: 'device.start', body: { Id: device.Id } })
							} else {
								ask({ method: 'device.stop', body: { Id: device.Id } })
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
					ask({ method: 'device.create', body: { Id: id } }, function () {
						DriverDevices(id)
					})
				}
			}) : ''
		)
	})
}

function DriverCreate() {
	clearInterval(timeout)
	if (!AUTH()) return

	ask({ method: 'driver.createform' }, function (data) {
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
							ask({ method: 'driver.create', body: { Name: name.value, Path: path.value } }, function (data) {
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
var timeout = 0

function Device(id) {

	lastLog = ''

	var x1, x2, x3, logsDetailed, logsWarnings

	if (ls(id + '.logs') == null) {
		ls(id + '.logs', 'open')
	}

	mount('#view', 'выполняется запрос...')
	ask({ method: 'device', body: { Id: id } }, function (device) {
		if (!device.Name) return mount('#view', 'Ошибка получения данных с сервера')
		ID = id
		
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
							ask({ method: 'device.start', body: { Id: id } }, function () {
								timeout = setInterval(function () {
									DeviceFields(id)
								}, 1000)
							})
						}
					})
					: '',
				AUTH()
					? h('button', {
						innerHTML: 'Стоп',
						onclick: function () {
							ask({ method: 'device.stop', body: { Id: id } }, function () {
								clearInterval(timeout)
								DeviceFields(id)
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
							ask({ method: 'device.delete', body: { Id: id } }, Home)
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
							h('th', { style: { width: '10em' }}, 'Дата'),
							h('th', { style: { width: '7.5em' }}, 'Тип'),
							h('th', 'Сообщение'),
						)
					),
					h('div#device-logs.sub')
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
							h('th', 'Значение'),
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

		clearInterval(timeout)
		timeout = setInterval(function () {
			DeviceFields(id)
		}, 1000)
	})
}

function DeviceConfiguration(id) {
	if (!AUTH()) return
	ask({ method: 'device.configuration', body: { Id: id } }, function (page) {
		if (!AUTH()) return
		mount('#device-configuration', h('div.form', page))
		$('#device-configuration').querySelectorAll('script').forEach(function (el) {
			(1, eval)(el.innerHTML)
		})
	})
}

function DeviceLogs(id) {
	ask({ method: 'device.logs', body: { Id: id } }, function (logs) {
		mount('#device-logs', h('table'))
		logs.forEach(function (x) {
			DeviceLog(id, x)
        })
	})
}

function DeviceLog(id, log) {
	if (ls(id + '.logs.detailed') == 'false' && log.Type == 1) return
	if (ls(id + '.logs.warnings') == 'false' && log.Type == 2) return

	var table = document.getElementById('device-logs').querySelector('table')
	if (!table) return

	table.appendChild(
		h('tr',
			h('td', { style: { width: '10em' } }, log.Date),
			h('td.type' + log.Type, { style: { width: '7.5em' } }, LogTypes[log.Type]),
			h('td', log.Text)
		)
	)
}

function DeviceFields(id) {
	ask({ method: 'device.fields', body: { Id: id } }, function (fields) {
		if (ID != id) return
		mount('#device-fields', 
			h('table',
				Object.keys(fields).map(function (key) {
					return h('tr',
						h('td', { style: { width: '12em' } }, key),
						h('td', fields[key])
					)
				})
			)
		)
	})
	
}

function DeviceSave(id) {
	if (!AUTH()) return
	var config = {}
	$('#device-configuration .form').querySelectorAll('div[type]').forEach(function (el) {

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
	ask({ method: 'device.update', body: form }, function () {
		alert('Устройство сохранено')
	})
}


function Settings() {

	ID = 0
	clearInterval(timeout)

	if (accessType == ACCESSTYPE.GUEST || accessType == ACCESSTYPE.READ) {
		return mount('#view', h('div.container', 'Нет доступа'))
	}

	mount('#view',
		h('div.container',
			h('button', {
				innerHTML: 'Создать службу и инициализировать DCOM',
				onclick: function () {
					ask({ method: 'opc.dcom' }, function (data) {
						if (data) alert('Инициализация DCOM выполнена')
					})
				}
			}),
			h('button', {
				innerHTML: 'Реинициализация OPC тегов',
				onclick: function () {
					ask({ method: 'opc.clean' }, function (data) {
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

	if (accessType < ACCESSTYPE.FULL) return ''

	var login, pass, access

	ask({ method: 'users' }, function (json) {
		if (!$('#users')) return
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
										ask({ method: 'user.delete', body: { Login: user.Login } }, function (json) {
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

									ask({ method: 'user.create', body: body }, function (json) {
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


function First() {
	var _pass
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
					ask({ method: 'user.create', body: _body }, function (json) {
						if (json.Done) {
							ask({ method: 'login', body: _body }, function (json) {
								if (json.Done) location.reload()
							})
						}
					})
				}
			})
		)
	)
}

function AuthPanel() {
	mount('#auth',
		accessType == ACCESSTYPE.FIRST || accessType == ACCESSTYPE.GUEST ? '' :
			h('div.panel-el',
				{
					route: 'auth',
					title: 'Нажмите, чтобы выйти из учётной записи',
					onclick: function () {
						TreeSetActive('auth')
						Login()
					}
				},
				h('i.ic.ic-person'),
				h('span', login)
			)
	)
}

function Login() {
	var _login, _pass
	mount('#view',
		accessType > ACCESSTYPE.GUEST
			? h('div.container',
				h('p', 'Вы вошли как ' + login),
				h('button', 'Выйти', {
					onclick: function () {
						ask({ method: 'logout', body: { Token: ls('Inopc-Access-Token') } }, function (json) {
							if (json.Done) {
								localStorage.removeItem('Inopc-Access-Token')
								location.reload()
							}
						})
					}
				})
			)
			: h('div.container',
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
						ask({ method: 'login', body: _body }, function (json) {
							if (json.Done) location.reload()
						})
					}
				})
			)
	)
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