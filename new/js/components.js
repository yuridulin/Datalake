var ID = 0
var accessType = 0
var login = null
var route = ''
var currentPage = ''

var LogTypes = {
	0: 'Информация',
	1: 'Детали',
	2: 'Внимание',
	3: 'Ошибка'
}

function Home() {

	if (currentPage == 'first' || currentPage == 'login') return
	patch(document.getElementById('view'),
		h('div', {}, [ text('подключаемся...') ])
	)
	ID = 0

	ask({ method: 'tree' }, function (json) {

		if (accessType == ACCESSTYPE.FIRST) return First()
		if (accessType == ACCESSTYPE.GUEST) return Login()

		BuildTree(json)

		patch(document.getElementById('view'),
			h('div', {}, [
				h('table', { class: 'datatable' }, [
					h('thead', {}, [
						h('tr', {}, [
							h('th', { style: 'width: 15em;' }, [ text('Драйвер') ]),
							h('th', {}, [ text('Устройство') ])
						])
					]),
					h('tbody', {},
						json.flatMap(function (driver) {
							return driver.Devices.map(function (device) {
								return h('tr', {}, [
									h('td', {
										title: 'Нажмите для перехода к драйверу',
										onclick: function () { Driver(driver.Id) }
									}, [
										text(driver.Name)
									]),
									h('td', {}, [
										h('i', AUTH() ? {
											class: 'ic ' + (device.IsActive ? 'ic-play' : 'ic-pause'),
											title: 'Нажмите для ' + (device.IsActive ? 'завершения' : 'запуска') + ' опроса данных',
											onclick: function () {
												ask({ method: 'device.' + (device.IsActive ? 'stop' : 'start'), body: { Id: device.Id } }, Home)
											}
										} : {}, []),
										h('span', {
											title: 'Нажмите для перехода к устройству',
											onclick: function () {
												Device(device.Id)
											}
										}, [
											text(device.Name)
										])
									])
								])
							})
						})
					)
				])
			])
		)
	})
}

function Offline() {
	ID = 0
	patch(document.getElementById('tree'), h('div', {}, []))
	patch(document.getElementById('view'), h('div', {}, [ text('Нет связи с OPC сервером') ]))
	clearInterval(timeout)
}

function Tree() {
	if (accessType == ACCESSTYPE.FIRST || accessType == ACCESSTYPE.GUEST) return
	ask({ method: 'tree' }, function (json) {
		console.log('get tree')
		BuildTree(json)
    })
}

function BuildTree(json) {
	console.log('build tree')
	patch(document.getElementById('tree'),
		h('div', {}, [
			accessType == ACCESSTYPE.FULL ? 
				h('div', { class: 'node', route: 'settings' }, [
					h('div',
						{
							class: 'node-caption',
							title: 'Нажмите, чтобы перейти к настройке сервера',
							onclick: function () {
								TreeSetActive('settings')
								Settings()
							}
						}, [
						h('i', { class: 'ic ic-menu' }, []),
						h('span', {}, [ text('Настройки') ])
					])
				]) : text(''),

			h('div', {}, accessType == ACCESSTYPE.READ || AUTH()
				? json.map(function (driver) {
					return h('div', {
						class: 'node' + (localStorage.getItem(driver.Id) == 'open' ? ' open' : ''),
						route: 'driver|' + driver.Id
					}, [
						h('div', { class: 'node-caption' }, [
							h('i', {
								class: 'ic ' + (localStorage.getItem(driver.Id) == 'open' ? 'ic-expand-less' : 'ic-expand-more'),
								onclick: function (e) {
									if (e.target.className.indexOf('less') > -1) {
										localStorage.setItem(driver.Id, 'close')
										e.target.className = 'ic ic-expand-more'
										e.target.parentNode.parentNode.className = 'node'
									}
									else {
										localStorage.setItem(driver.Id, 'open')
										e.target.className = 'ic ic-expand-less'
										e.target.parentNode.parentNode.className = 'node open'
									}
								}
							}, []),
							h('span', {
								onclick: function () {
									TreeSetActive('driver|' + driver.Id)
									Driver(driver.Id)
								}
							}, [
								text(driver.Name),
								driver.HasError
									? h('i', { class: 'ic ic-warning ic-inline' }, [])
									: text(''),
							]),
						]),
						h('div', { class: 'node-body' },
							driver.Devices.map(function (device) {
								return h('div', { class: 'node', route: 'device|' + device.Id }, [
									h('div', { class: 'node-caption' }, [
										h('i', {
											class: 'ic ' + (device.IsActive ? 'ic-play' : 'ic-pause'),
											onclick: AUTH()
												? function () {
													if (device.IsActive) ask({ method: 'device.stop', body: { Id: device.Id } })
													else ask({ method: 'device.start', body: { Id: device.Id } })
												}
												: function () { }
										}, []),
										h('span', {
											onclick: function () {
												TreeSetActive('device|' + device.Id)
												Device(device.Id)
											}
										}, [
											text(device.Name),
											device.HasError
												? h('i', { class: 'ic ic-warning ic-inline' }, [])
												: text('')
										])
									])
								])
							})
						)
					])
				})
				: []
			),

			AUTH() ? h('div', { class: 'node', route: 'driver-create' }, [
				h('div', { class: 'node-caption' }, [
					h('i', { class: 'ic ic-plus' }),
					h('span', {
						onclick: function () {
							TreeSetActive('driver-create')
							DriverCreate()
						}
					}, [ text('Добавить драйвер') ])
				])
			]) : text('')
		])
	)

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
	patch(document.getElementById('view'), h('div', {}, [ text('выполняется запрос...') ]))
	ID = id

	ask({ method: 'driver', body: { Id: id } }, function (driver) {
		if (!driver.Name) return patch(document.getElementById('view'), h('div', {}, [text('Ошибка получения данных с сервера')]))
		
		var name, path

		patch(document.getElementById('view'),
			h('div', {}, [
				h('div', { class: 'ontainer' }, [

					h('span', 'Имя'),

					AUTH()
						? h('input', { type: 'text', value: driver.Name })
						: h('span.value', driver.Name),

					h('span', 'Тип'),

					AUTH()
						? h('select', { disabled: true },
							driver.Dlls.map(function (x) {
								return x == driver.Path ? h('option', x, { selected: true }) : ''
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
				]),

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
			])
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
	ID = id

	var x1, x2, x3, logsDetailed, logsWarnings

	if (localStorage.getItem(id + '.logs') == null) {
		localStorage.setItem(id + '.logs', 'open')
	}

	patch(document.getElementById('view'),
		h('div', {}, [text('выполняется запрос...')])
	)
	ask({ method: 'device', body: { Id: id } }, function (device) {
		if (!device.Name) return patch(document.getElementById('view'),
			h('div', {}, [text('Ошибка получения данных с сервера')])
		)
		
		patch(document.getElementById('view'),
			h('div', { class: 'container' }, [
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
			]),

			AUTH() ? h('div.container',
				h('div',
					h('span.container-expand-button',
						{
							onclick: function () {
								x1.classList.toggle('closed')
								this.querySelector('i').classList.toggle('ic-expand-more')
								this.querySelector('i').classList.toggle('ic-expand-less')
								localStorage.setItem(id + '.settings', x1.classList.contains('closed') ? 'close' : 'open')
							}
						},
						h('i.ic.ic-expand-' + (localStorage.getItem(id + '.settings') == 'open' ? 'more' : 'less')),
						h('span.container-expand-caption', 'Настройки')
					)
				),
				x1 = h(localStorage.getItem(id + '.settings') == 'open' ? 'div.container-data' : 'div.container-data.closed',
					{
						style: { height: localStorage.getItem(id + '.settings.h') },
						onclick: function () {
							localStorage.setItem(id + '.settings.h', this.style.height)
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
								localStorage.setItem(id + '.logs', x2.classList.contains('closed') ? 'close' : 'open')
							}
						},
						h('i.ic.ic-expand-' + (localStorage.getItem(id + '.logs') == 'open' ? 'more' : 'less')),
						h('span.container-expand-caption', 'Логи')
					)
				),
				x2 = h(localStorage.getItem(id + '.logs') == 'open' ? 'div.container-data' : 'div.container-data.closed',
					{
						style: { height: localStorage.getItem(id + '.logs.h') },
						onclick: function () {
							localStorage.setItem(id + '.logs.h', this.style.height)
						}
					},
					h('label',
						logsDetailed = h('input', { 
							type: 'checkbox',
							onchange: function () {
								localStorage.setItem(id + '.logs.detailed', this.checked ? 'true' : 'false')
								DeviceLogs(id, true)
							}
						}),
						'детали'
					),
					h('label',
						logsWarnings = h('input', { 
							type: 'checkbox',
							onchange: function () {
								localStorage.setItem(id + '.logs.warnings', this.checked ? 'true' : 'false')
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
								localStorage.setItem(id + '.fields', x3.classList.contains('closed') ? 'close' : 'open')
							}
						},
						h('i.ic.ic-expand-' + (localStorage.setItem(id + '.fields') == 'open' ? 'more' : 'less')),
						h('span.container-expand-caption', 'Опрашиваемые параметры')
					)
				),
				x3 = h(localStorage.getItem(id + '.fields') == 'open' ? 'div.container-data' : 'div.container-data.closed',
					{
						style: { height: localStorage.getItem(id + '.fields.h') },
						onclick: function () {
							localStorage.setItem(id + '.fields.h', this.style.height)
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

		if (localStorage.getItem(id + '.logs.detailed') == 'true') {
			logsDetailed.checked = true
		}
		if (localStorage.getItem(id + '.logs.warnings') == 'true') {
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
		document.getElementById('device-configuration').querySelectorAll('script').forEach(function (el) {
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
	if (localStorage.getItem(id + '.logs.detailed') == 'false' && log.Type == 1) return
	if (localStorage.getItem(id + '.logs.warnings') == 'false' && log.Type == 2) return

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
	ask({ method: 'device.update', body: form }, function () {
		alert('Устройство сохранено')
	})
}


function Settings() {

	if (currentPage == 'settings') return
	currentPage = 'settings'

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

	if (currentPage == 'users') return
	currentPage = 'users'

	ID = 0
	if (accessType < ACCESSTYPE.FULL) return ''
	var login, pass, access

	ask({ method: 'users' }, function (json) {
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

	if (currentPage == 'first') return
	currentPage = 'first'

	ID = 0

	patch(document.getElementById('view'),
		h('container', {}, [
			h('h3', {}, [ text('Первоначальная настройка') ]),
			h('p', {}, [ text('Введите пароль для создания первой учётной записи с полным доступом') ]),
			h('input', {
				id: '_pass',
				type: 'password',
				placeholder: 'пароль...'
			}, []),
			h('button', {
				onclick: function () {
					var _body = {
						Login: 'admin',
						Password: document.getElementById('_pass').value,
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
			}, [ text('Сохранить') ])
		])
	)
}

function AuthPanel() {
	patch(document.getElementById('auth'),
		accessType == ACCESSTYPE.FIRST || accessType == ACCESSTYPE.GUEST
			? h('div', {}, [])
			: h('div',
				{
					class: 'panel-el',
					route: 'auth',
					title: 'Нажмите, чтобы выйти из учётной записи',
					onclick: function () {
						TreeSetActive('auth')
						Login()
					}
				}, [
					h('i', { class: 'ic ic-person' }, []),
					h('span', {}, [ text(login) ])
			])
	)
}

function Login() {

	if (currentPage == 'login') return
	currentPage = 'login'

	clearInterval(timeout)
	ID = 0

	patch(document.getElementById('view'),
		accessType > ACCESSTYPE.GUEST
			? h('div.container', {}, [
				h('p', {}, [ text('Вы вошли как ' + login) ]),
				h('button', {
					onclick: function () {
						ask({ method: 'logout', body: { Token: localStorage.getItem('Inopc-Access-Token') } }, function (json) {
							if (json.Done) {
								localStorage.removeItem('Inopc-Access-Token')
								location.reload()
							}
						})
					}
				}, [ text('Выйти') ])
			])
			: h('div.container', {}, [
				h('p', {}, [ text('Вы не вошли в учетную запись. Выполните вход:') ]),

				h('span', {}, [ text('Имя учётной записи') ]),
				h('input', { id: '_login', type: 'text', name: 'login' }, []),

				h('span', {}, [ text('Пароль') ]),
				h('input', { id: '_pass', type: 'password', name: 'password' }, []),

				h('button', {
					onclick: function () {
						var _body = {
							Login: document.getElementById('_login').value,
							Password: document.getElementById('_pass').value
						}
						ask({ method: 'login', body: _body }, function (json) {
							if (json.Done) location.reload()
						})
					}
				}, [ text('Вход') ])
			])
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