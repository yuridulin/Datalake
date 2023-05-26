var ID = 0
var accessType = 0
var loginName = null
var route = ''
var currentPage = ''
var licenseMode = 0

var deviceName
var deviceAutoStart
var interval = 0

var unsetTimers = function () {
	ID = 0
	clearInterval(interval)
}



function Logo() {
	if (licenseMode == LICENSEMODE.LICENSED) {
		mount('#logo', 'iNOPC webConsole', h('sup', '&ensp;'))
	}
	else if (licenseMode == LICENSEMODE.ACTIVETRIAL) {
		mount('#logo', 'iNOPC webConsole', h('sup', 'trial'))
	}
	else if (licenseMode == LICENSEMODE.DEBUG) {
		mount('#logo', 'iNOPC webConsole', h('sup', 'debug'))
	}
	else {
		mount('#logo', 'iNOPC webConsole', h('sup.error', 'trial'))
	}
}

function Home() {

	unsetTimers()
	if (currentPage == 'first' || currentPage == 'login') return
	mount('#view', 'подключаемся...')

	ask({ method: 'settings/tree' }, function (json) {

		// первый вход - перенаправление на страницу настроек
		if (accessType == ACCESSTYPE.FIRST) return First()
		if (accessType == ACCESSTYPE.GUEST) return Login()

		BuildTree(json)
		mount('#view',
			h('table.datatable',
				h('thead',
					h('tr',
						h('th', { style: { width: '15em' } }, 'Драйвер'),
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
											ask({ method: 'devices/' + (device.IsActive ? 'stop' : 'start'), body: { Id: device.Id } }, Home)
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
	unsetTimers()
	mount('#tree', '')
	mount('#view', 'Нет связи с OPC сервером')
}

function Tree() {
	if (accessType == ACCESSTYPE.FIRST || accessType == ACCESSTYPE.GUEST) {
		mount('#view', '')
	}
	else {
		ask({ method: 'settings/tree' }, function (json) {
			BuildTree(json)
		})
	}
}

function BuildTree(json) {
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

		h('div.node', { route: 'calc' },
			h('div.node-caption', {
				title: 'Нажмите, чтобы перейти к списку тегов, рассчитываемых по математическим формулам',
			},
				h('i.ic.ic-calc'),
				h('span', {
					innerHTML: 'Вычисляемые теги',
					onclick: function () {
						TreeSetActive('calc'),
						CalcPage()
					}
				})
			)
		),

		h('div.node', { route: 'values' },
			h('div.node-caption', {
				title: 'Нажмите для перехода к просмотру дерева тегов сервера'
			},
				h('i.ic.ic-values'),
				h('span', {
					innerHTML: 'Значения',
					onclick: function () {
						TreeSetActive('values')
						Values()
					}
				})
			)
		),

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
											if (device.IsActive) ask({ method: 'devices/stop', body: { Id: device.Id } })
											else ask({ method: 'devices/start', body: { Id: device.Id } })
										}
									} : {}),
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
			h('div.node-caption', {
				title: 'Нажмите, чтобы перейти к добавлению нового драйвера',
			},
				h('i.ic.ic-plus'),
				h('span', {
					innerHTML: 'Добавить драйвер',
					onclick: function () {
						TreeSetActive('driver-create')
						DriverCreate()
					}
				})
			)
		) : '',

		h('div.node', { route: 'driver-create' },
			h('div.node-caption', {
				title: 'Нажмите, чтобы перейти к добавлению нового драйвера',
			},
				h('i.ic.ic-help'),
				h('span', {
					innerHTML: 'Справка',
					onclick: function () {
						var a = document.createElement('a')
						a.style.display = 'none'
						a.href = './help.html'
						a.target = '_blank'
						document.body.appendChild(a)
						a.click()
					}
				})
			)
		)
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

	unsetTimers()

	mount('#view', 'выполняется запрос...')
	
	ID = id

	ask({ method: 'drivers/read', body: { Id: id } }, function (driver) {
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
					? path = h('select',
						h('option', { value: '' }, 'не определён'),
						driver.Dlls.map(function (x) {
							return x == driver.Path ? h('option', x, { selected: true }) : h('option', x)
						})
					)
					: h('span.value', driver.Path),

				AUTH()
					? h('button', {
						innerHTML: 'Перезагрузить',
						onclick: function () {
							ask({ method: 'drivers/reload', body: { Id: id } })
						}
					})
					: '',
				AUTH()
					? h('button', {
						innerHTML: 'Сохранить',
						onclick: function () {
							ask({ method: 'drivers/update', body: { Id: id, Name: name.value, Path: path.value } })
						}
					})
					: '',
				AUTH()
					? h('button', {
						innerHTML: 'Удалить',
						onclick: function () {
							if (!confirm('Драйвер будет удален из конфигурации без возможности восстановления. Продолжить?')) return
							ask({ method: 'drivers/delete', body: { Id: id } }, Home)
						}
					})
					: ''
			),

			h('div.container',
				h('div.container-caption', 'Логи'),
				h('table',
					h('tr',
						h('th', { style: { width: '10em' } }, 'Дата'),
						h('th', { style: { width: '7.5em' } }, 'Тип'),
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
	ask({ method: 'drivers/logs', body: { Id: id } }, function (logs) {
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
	ask({ method: 'drivers/devices', body: { Id: id } }, function (devices) {
		mount('#driver-devices',
			h('div.devices', devices.map(function (device) {
				return h('div',
					h('i.ic.ic-' + (device.IsActive ? 'play' : 'pause'), AUTH() ? {
						onclick: function () {
							if (!device.IsActive) {
								ask({ method: 'devices/start', body: { Id: device.Id } })
							} else {
								ask({ method: 'devices/stop', body: { Id: device.Id } })
							}
						}
					} : {}),
					h('span', {
						innerHTML: device.Name,
						onclick: function () {
							Device(device.Id)
						}
					}),
					h('button', {
						innerHTML: 'Копировать',
						onclick: function () {
							ask({ method: 'devices/copy', body: { Id: device.Id } }, function () {
								DriverDevices(id)
							})
						}
					})
				)
			})),
			AUTH() ? h('button', {
				innerHTML: 'Добавить устройство',
				onclick: function () {
					ask({ method: 'devices/create', body: { Id: id } }, function () {
						DriverDevices(id)
					})
				}
			}) : ''
		)
	})
}

function DriverCreate() {

	unsetTimers()
	if (!AUTH()) return

	ask({ method: 'drivers/createform' }, function (data) {
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
							ask({ method: 'drivers/create', body: { Name: name.value, Path: path.value } }, function (data) {
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


function Device(id) {

	unsetTimers()
	ID = id
	var x1, x2, x3, logsDetailed, logsWarnings

	if (ls(id + '.logs') == null) {
		ls(id + '.logs', 'open')
	}

	mount('#view', 'выполняется запрос...')
	ask({ method: 'devices/read', body: { Id: id } }, function (device) {
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
							ask({ method: 'devices/start', body: { Id: id } }, function () {
								interval = setInterval(function () {
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
							ask({ method: 'devices/stop', body: { Id: id } }, function () {
								clearInterval(interval)
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
							ask({ method: 'devices/delete', body: { Id: id } }, Home)
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
							h('th', { style: { width: '10em' } }, 'Дата'),
							h('th', { style: { width: '7.5em' } }, 'Тип'),
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
							h('th', 'Параметр'),
							h('th', { style: { width: '18em' } }, 'Значение'),
							h('th', { style: { width: '12em' } }, 'Качество'),
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

		clearInterval(interval)
		interval = setInterval(function () {
			DeviceLogsClean()
			DeviceFields(id)
		}, 1000)
	})
}

function DeviceConfiguration(id) {
	if (!AUTH()) return
	ask({ method: 'devices/configuration', body: { Id: id } }, function (page) {
		if (!AUTH()) return
		mount('#device-configuration', h('div.form', page))
		document.getElementById('device-configuration').querySelectorAll('script').forEach(function (el) {
			(1, eval)(el.innerHTML)
		})
	})
}

function DeviceLogs(id) {
	ask({ method: 'devices/logs', body: { Id: id } }, function (logs) {
		mount('#device-logs', h('table'))
		logs.forEach(function (x) {
			DeviceLog(id, x)
		})
	})
}

function DeviceLog(id, log) {
	if (ls(id + '.logs.detailed') == 'false' && log.Type == 1) return
	if (ls(id + '.logs.warnings') == 'false' && log.Type == 2) return

	var div = document.getElementById('device-logs')
	var table = div.querySelector('table')
	if (!table) return

	var needScroll = div.scrollHeight < (div.getBoundingClientRect().height + div.scrollTop)

	table.appendChild(
		h('tr',
			h('td', { style: { width: '10em' } }, log.Date),
			h('td.type' + log.Type, { style: { width: '7.5em' } }, LogTypes[log.Type]),
			h('td', log.Text)
		)
	)

	if (needScroll) div.scrollTop = div.scrollHeight
}

function DeviceLogsClean() {
	var div = document.getElementById('device-logs')
	var table = div.querySelector('table')
	var rows = table.getElementsByTagName('tr')

	if (rows.length >= 100) {
		var i = rows.length - 100
		while (i >= 0) {
			table.removeChild(rows[i])
			i--
		}
	}
}

function DeviceFields(id) {
	ask({ method: 'devices/fields', body: { Id: id } }, function (fields) {
		if (ID != id) return
		mount('#device-fields',
			h('table',
				Object.keys(fields).map(function (key) {
					return h('tr',
						h('td', key),
						h('td', { style: { width: '18em' } }, fields[key].Value),
						h('td', { style: { width: '12em' } }, fields[key].Quality)
					)
				})
			)
		)
	})

}

function DeviceSave(id) {
	if (!AUTH()) return
	var config = {}
	var form = document.querySelector('#device-configuration .form')
	form.querySelectorAll('div[type]').forEach(function (el) {

		if (el.getAttribute('type') == 'value') {
			var input = el.querySelector('input,select,textarea')
			config[input.name] = input.type == 'checkbox' ? input.checked : input.value
		}

		else if (el.getAttribute('type') == 'array') {
			var arr = []
			var parts = el.querySelectorAll('p')
			parts.forEach(function (p) {
				var x = {}
				p.querySelectorAll('input,select,textarea').forEach(function (i) {
					x[i.name] = i.type == 'checkbox' ? i.checked : i.value
				})
				arr.push(x)
			})
			config[el.getAttribute('name')] = arr
		}
	})

	form.querySelectorAll('input[v]').forEach(function (input) {
		config[input.name] = input.type == 'checkbox' ? input.checked : input.value
	})

	var body = {
		Id: id,
		Name: deviceName.value,
		AutoStart: deviceAutoStart.checked,
		Configuration: JSON.stringify(config)
	}

	// оправка формы на сервер
	ask({ method: 'devices/update', body: body }, function () {
		alert('Устройство сохранено')
	})
}


function Settings() {

	unsetTimers()
	if (currentPage == 'settings') return
	currentPage = 'settings'

	if (accessType == ACCESSTYPE.GUEST || accessType == ACCESSTYPE.READ) {
		return mount('#view', h('div.container', 'Нет доступа'))
	}

	var _key

	ask({ method: 'settings/settings' }, function (json) {
		mount('#view',
			h('div.container',
				h('h3', 'Версия сервера'),
				h('span', 'v.' + json.Version)
			),
			licenseMode == LICENSEMODE.DEBUG
				? h('div.container',
					h('h3', 'Лицензирование'),
					h('span', 'Состояние: ' + json.LicenseStatus)
				)
				: h('div.container',
					h('h3', 'Лицензирование'),
					h('span', 'Состояние: ' + json.LicenseStatus),
					h('br'),
					h('span', { style: { display: 'inline-block', width: '16em' } }, 'Идентификатор сервера: '),
					h('input', { disabled: true, style: { width: '40em' }, value: json.LicenseId }),
					h('br'),
					h('span', { style: { display: 'inline-block', width: '16em' } }, 'Лицензионный ключ: '),
					_key = h('input', { value: json.LicenseKey, style: { width: '40em' } }),
					h('br'),
					h('button', {
						innerHTML: 'Сохранить ключ',
						onclick: function () {
							ask({ method: 'settings/OpcLicense', body: { key: _key.value } }, function (data) {
								if (data.Done) alert('Ключ сохранён')
								Settings()
							})
						}
					})
				),
			
			h('div.container',
				h('h3', 'DCOM'),
				h('span', 'Состояние: ' + json.OpcStatus),
				h('br'),
				h('button', {
					innerHTML: 'Зарегистрировать сервер в DCOM',
					onclick: function () {
						ask({ method: 'settings/OpcIstallDcom' }, function (data) {
							if (data.Done) alert('Регистрация DCOM выполнена')
							Settings()
						})
					}
				}),
				h('button', {
					innerHTML: 'Отменить регистрацию сервера в DCOM',
					onclick: function () {
						ask({ method: 'settings/OpcUninstallDcom' }, function (data) {
							if (data.Done) alert('Регистрация DCOM отменена')
							Settings()
						})
					}
				}),
				h('button', {
					innerHTML: 'Пересоздать OPC теги',
					onclick: function () {
						ask({ method: 'settings/OpcClean' }, function (data) {
							if (data.Done) alert('Реинициализация OPC тегов выполнена')
							Settings()
						})
					}
				})
			),
			h('div.container',
				h('h3', 'Служба'),
				h('span', 'Состояние: ' + json.ServiceStatus, { title: 'Путь к исполняемому файлу: ' + json.ServicePath }),
				h('br'),
				h('button', {
					innerHTML: 'Создать службу',
					onclick: function () {
						ask({ method: 'settings/ServiceCreate' }, function (data) {
							if (data.Done) alert(data.Done)
							Settings()
						})
					}
				}),
				h('button', {
					innerHTML: 'Удалить службу',
					onclick: function () {
						ask({ method: 'settings/ServiceRemove' }, function (data) {
							if (data.Done) alert(data.Done)
							Settings()
						})
					}
				})
			),
			h('div.container',
				h('h3', 'Учётные записи'),
				h('div#users')
			)
		)

		UsersTable()
	})
}

function UsersTable() {

	unsetTimers()
	if (currentPage == 'users') return
	currentPage = 'users'

	if (accessType < ACCESSTYPE.FULL) return ''
	var login, pass, access

	ask({ method: 'auth/users' }, function (json) {
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
										ask({ method: 'auth/delete', body: { Login: user.Login } }, function (json) {
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

									ask({ method: 'auth/create', body: body }, function (json) {
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

	unsetTimers()
	if (currentPage == 'first') return
	currentPage = 'first'

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
					ask({ method: 'auth/create', body: _body }, function (json) {
						if (json.Done) {
							ask({ method: 'auth/login', body: _body }, function (json) {
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
				h('span', loginName)
			)
	)
}

function Login() {

	unsetTimers()
	if (currentPage == 'login') return
	currentPage = 'login'

	var _login, _pass

	mount('#view',
		accessType > ACCESSTYPE.GUEST
			? h('div.container',
				h('p', 'Вы вошли как ' + loginName),
				h('button', 'Выйти', {
					onclick: function () {
						ask({ method: 'auth/logout', body: { Token: ls('Inopc-Access-Token') } }, function (json) {
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
						ask({ method: 'auth/login', body: _body }, function (json) {
							if (json.Done) location.reload()
						})
					}
				})
			)
	)
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


function CalcPage() {
	unsetTimers()
	if (accessType == ACCESSTYPE.GUEST || accessType == ACCESSTYPE.FIRST) return mount('#view', h('div.container', 'Нет доступа'))
	
	ask({ method: 'formulars/fields' }, function (/**@type{Formular[]}*/json) {
		mount('#view',
			h('div', { className: 'container' },
				h('h3', 'Список вычисляемых тегов'),
				h('div', { className: 'fields' },
					h('div',
						h('b', 'Тег', { style: { width: '10em' } }),
						h('b', 'Значение', { style: { width: '10em' } }),
						h('b', 'Ошибки', { style: { width: '30em' } }),
						h('b', { style: { width: '10em' } },
							accessType == ACCESSTYPE.WRITE || accessType == ACCESSTYPE.FULL
								? h('button', {
									innerHTML: 'Добавить тег',
									onclick: function () {
										ask({ method: 'formulars/create' }, function (json) {
											if (json.Error) alert(json.Error)
											if (json.Done) {
												CalcPage()
											}
										})
									}
								})
								: ''
						)
					),
					json.map(formular => h('div', { 'data-id': formular.Name },
						h('span', formular.Name),
						h('span'),
						h('span'),
						h('span',
							 h('button', {
								innerHTML: accessType == ACCESSTYPE.READ ? 'Просмотреть' : 'Изменить',
								onclick: function () {
									Formular(formular)
								}
							})
						)
					))
				)
			)
		)

		interval = setInterval(function () {
			CalcValues()
		}, 1000)

		CalcValues()
	})
}

function CalcValues() {
	ask({ method: 'formulars/values' }, function (/**@type{FormularValue[]}*/json) {
		json.forEach(x => {
			var el = document.querySelector('[data-id="' + x.Name + '"]')
			if (el) {
				var cells = el.querySelectorAll('span')
				try {
					cells[1].innerHTML = x.Value.toFixed(2)
				}
				catch (e) {
					cells[1].innerHTML = x.Value
				}
				cells[2].innerHTML = x.Error
			}
		})
	})
}

/**
 * @param {Formular} field
 */
function Formular(field) {
	unsetTimers()
	if (accessType == ACCESSTYPE.GUEST || accessType == ACCESSTYPE.FIRST) return mount('#view', h('div.container', 'Нет доступа'))
	ask({ method: 'formulars/form', body: { Name: field.Name } }, function (/**@type{string[]}*/json) {
		mount('#view',
			accessType == ACCESSTYPE.WRITE || accessType == ACCESSTYPE.FULL
				? h('div', { className: 'container', id: 'calc-tag' },
					h('table', { className: 'form' },
						h('tr',
							h('td', { style: { width: '14em' } }, h('span', 'Тег')),
							h('td', h('input', { id: 'calc-tag-name', value: field.Name }))
						),
						h('tr',
							h('td', h('span', 'Интервал расчёта, с')),
							h('td', h('input', { id: 'calc-tag-interval', type: 'number', min: 1, max: 604800, step: 1, value: field.Interval }))
						),
						h('tr',
							h('td', h('span', 'Формула')),
							h('td', h('input', { id: 'calc-tag-formula', value: field.Formula }))
						),
						h('tr',
							h('td', h('span', 'Описание')),
							h('td', h('textarea', {
								id: 'calc-tag-desc', oninput: function () {
									this.style.height = 0
									this.style.height = this.scrollHeight + 'px'
								}}, field.Description))
						),
						h('tr',
							h('td', { colspan: 2 }, 
								h('button', {
									innerHTML: 'Удалить',
									onclick: function () {
										if (!confirm('Подтвердите удаление тега')) return
										ask({ method: 'formulars/delete', body: { name: field.Name } }, function (json) {
											if (json.Error) alert(json.Error)
											if (json.Done) {
												alert('Тег успешно удалён')
												CalcPage()
											}
										})
									}
								}),
								h('button', {
									innerHTML: 'Сохранить',
									onclick: function () {
										/**@type{Formular}*/
										var form = {
											OldName: field.Name,
											Name: document.getElementById('calc-tag-name').value,
											Interval: document.getElementById('calc-tag-interval').value,
											Description: document.getElementById('calc-tag-desc').value,
											Formula: document.getElementById('calc-tag-formula').value,
											Fields: {}
										}

										document.getElementById('calc-tag-fields').querySelectorAll('div').forEach(el => {
											if (!el.querySelector('input')) return
											form.Fields[el.querySelector('input').value] = el.querySelector('select').value
										})

										ask({ method: 'formulars/update', body: form }, function (json) {
											if (json.Error) alert(json.Error)
											if (json.Done) {
												alert('Тег успешно обновлён')
												CalcPage()
											}
										})
									}
								}),
								h('button', {
									innerHTML: 'Закрыть',
									onclick: function () {
										CalcPage()
									}
								})
							)
						)
					)
				)
				: h('div', { className: 'container', },
					h('table', { className: 'form' },
						h('tr',
							h('td', { style: { width: '14em' } }, h('span', 'Тег')),
							h('td', field.Name)
						),
						h('tr',
							h('td', h('span', 'Интервал расчёта')),
							h('td', field.Interval + 'c')
						),
						h('tr',
							h('td', h('span', 'Формула')),
							h('td', h('pre', field.Formula))
						),
						h('tr',
							h('td', h('span', 'Описание')),
							h('td', field.Description)
						),
						h('tr',
							h('td', { colspan: 2 },
								h('button', {
									innerHTML: 'Закрыть',
									onclick: function () {
										CalcPage()
									}
								})
							)
						)
					)
				),
			accessType == ACCESSTYPE.WRITE || accessType == ACCESSTYPE.FULL
				? h('div', { className: 'container' },
					h('div', { className: 'fields', id: 'calc-tag-fields' },
						h('div',
							h('b', { style: { width: '15em' } }, 'Наименование переменной'),
							h('b', { style: { width: '25em' } }, 'Привязанный OPC тег'),
							h('b', { style: { width: '8em' } },
								h('button', {
									innerHTML: 'Добавить',
									onclick: function () {
										document.getElementById('calc-tag-fields').insertAdjacentElement('beforeend', 
											h('div',
												h('span',
													h('input', { value: 'x' + Math.random() })
												),
												h('span',
													h('select', json.map(s => h('option', s, { value: s })))
												),
												h('span',
													h('button', {
														innerHTML: 'Удалить',
														onclick: function () {
															this.parentNode.parentNode.remove()
														}
													})
												)
											)
										)
									}
								})
							)
						),
						Object.keys(field.Fields).map(x =>
							h('div',
								h('span',
									h('input', { value: x })
								),
								h('span',
									h('select', json
										.map(s => h('option', s, s == field.Fields[x] ? { selected: true, value: s } : { value: s }))
									)
								),
								h('span',
									h('button', {
										innerHTML: 'Удалить',
										onclick: function () {
											this.parentNode.parentNode.remove()
										}
									})
								)
							)
						)
					)
					)
				: h('div', { className: 'container' },
					h('div', { className: 'fields' },
						h('div',
							h('b', { style: { width: '15em' } }, 'Наименование переменной'),
							h('b', { style: { width: '25em' } }, 'Привязанный OPC тег')
						),
						Object.keys(field.Fields).map(x =>
							h('div',
								h('span', x),
								h('span', field.Fields[x])
							)
						)
					)
				)
		)

		try { document.getElementById('calc-tag-desc').oninput() } catch (e) { }
	})
}


function Values() {

	unsetTimers()
	if (accessType == ACCESSTYPE.GUEST || accessType == ACCESSTYPE.FIRST) return mount('#view', h('div.container', 'Нет доступа'))

	mount('#view',
		h('table', { style: { width: '100%' } },
			h('thead',
				h('tr',
					h('th', 'Name'),
					h('th', 'Value', { style: { width: '25em' } }),
					h('th', 'Quality', { style: { width: '10em' } }),
				),
			),
			h('tbody#values', ValuesTick())
		)
	)

	function ValuesTick() {
		ask({ method: 'storage/read', body: { Tags: [] } }, json => {
			mount('#values',
				json.Tags.map(x => h('tr',
					h('td', x.Name),
					h('td', x.Value),
					h('td', x.Quality)
				))
			)
		})
	}

	interval = setInterval(ValuesTick, 5000)
}



// enum 

var ACCESSTYPE = {
	GUEST: 0,
	READ: 1,
	WRITE: 2,
	FULL: 3,
	FIRST: 4
}

var LICENSEMODE = {
	ACTIVETRIAL: 0,
	EXPIREDTRIAL: 1,
	LICENSED: 2,
	DEBUG: 3,
}

var LogTypes = {
	0: 'Информация',
	1: 'Детали',
	2: 'Внимание',
	3: 'Ошибка'
}

var MathTypes = {
	'SUM': 'Сложение',
	'DIFF': 'Вычитание',
	'MULT': 'Умножение',
	'DIV': 'Деление',
	'CONST': 'Константа',
	'AVG': 'Среднее арифм.'
}