var ws
var requests = {}
var clientId = Math.random()
var wsAddress = 'ws://' + location.hostname + ':82/'

function connectToServer() {
	ws = new WebSocket(wsAddress)
	ws.onopen = function () {
		Home()
	}
	ws.onclose = function () {
		setTimeout(connectToServer, 5000)
	}
	ws.onerror = function () {
		Offline()
	}
	ws.onmessage = function (message) {

		var parts = ('' + message.data).split('|')

		// Переданные значения
		var data = parts.length > 1
			? JSON.parse(parts[1])
			: {}

		var key = (parts[0]).split(':')

		// Имя метода
		var method = key[0]

		// Идентификатор
		var id = key.length > 1
			? (+key[1])
			: 0

		switch (method) {
			case 'tree':
				if (currentPage == 'first' || currentPage == 'login') return
				if (ID == 0) Home()
				else Tree()
				break
			case 'driver.logs':
				if (ID == id && ID != 0) DriverLogs(id)
				break
			case 'driver.devices':
				if (ID == id && ID != 0) DriverDevices(id)
				break
			case 'device.fields':
				//if (ID == id && ID != 0) DeviceFields(id)
				break
			case 'device.logs':
				if (ID == id && ID != 0) DeviceLog(id, data)
				break
        }
	}
}

/**
 * Обращение к серверу за конкретной информацией с авторизацией каждого запроса
 * @param {any} parameters Объект с данными, передаваемыми на сервер
 * @param {any} callback Обработчик, в который передается результат запроса
 */
function ask(parameters, callback) {
	var xhr = new XMLHttpRequest()
	xhr.open('POST', location.origin + '/' + parameters.method, true)
	xhr.setRequestHeader('Inopc-Access-Type', accessType)
	xhr.setRequestHeader('Inopc-Access-Token', ls('Inopc-Access-Token'))
	xhr.onreadystatechange = function () {

		// Ожидание ответа сервера
		if (xhr.readyState != 4) return
		if (xhr.status != 200) return console.log('ask err: xhr return ' + xhr.status + ' [' + xhr.statusText + ']')

		// Получение данных авторизации
		localStorage.setItem('Inopc-Access-Token', xhr.getResponseHeader('Inopc-Access-Token'))
		accessType = +(xhr.getResponseHeader('Inopc-Access-Type') || '0')
		login = xhr.getResponseHeader('Inopc-Login')
		AuthPanel()

		// Получение результата запроса
		var json = {}
		try { json = JSON.parse(xhr.responseText) } catch (e) { return console.log('ask err: not json [' + xhr.responseText + ']') }

		if (accessType != ACCESSTYPE.FIRST) {
			if (json.Error) console.log('Ошибка: ' + json.Error)
			if (json.Warning) console.log(json.Warning)
			if (json.Done) console.log(json.Done)
		}

		if (!callback) return
		callback.call(null, json)
	}
	xhr.send(JSON.stringify(parameters.body || {}))
}