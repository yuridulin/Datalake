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

function ask(parameters, callback) {
	var xhr = new XMLHttpRequest()
	xhr.open('POST', location.origin + '/' + parameters.method, true)
	xhr.onreadystatechange = function () {
		if (xhr.readyState != 4) return
		if (xhr.status != 200) return console.log('ask err: xhr return ' + xhr.status + ' [' + xhr.statusText + ']')
		var json = {}
		try { json = JSON.parse(xhr.responseText) } catch (e) { console.log('ask err: not json [' + xhr.responseText + ']') }
		if (!callback) return
		callback.call(null, json)
	}
	xhr.send(JSON.stringify(parameters.body || {}))
}