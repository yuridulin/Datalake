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

		// ���������� ��������
		var data = parts.length > 1
			? JSON.parse(parts[1])
			: {}

		var key = (parts[0]).split(':')

		// ��� ������
		var method = key[0]

		// �������������
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
 * ��������� � ������� �� ���������� ����������� � ������������ ������� �������
 * @param {any} parameters ������ � �������, ������������� �� ������
 * @param {any} callback ����������, � ������� ���������� ��������� �������
 */
function ask(parameters, callback) {
	var xhr = new XMLHttpRequest()
	xhr.open('POST', location.origin + '/' + parameters.method, true)
	xhr.setRequestHeader('Inopc-Access-Type', accessType)
	xhr.setRequestHeader('Inopc-Access-Token', ls('Inopc-Access-Token'))
	xhr.onreadystatechange = function () {

		// �������� ������ �������
		if (xhr.readyState != 4) return
		if (xhr.status != 200) return console.log('ask err: xhr return ' + xhr.status + ' [' + xhr.statusText + ']')

		// ��������� ������ �����������
		localStorage.setItem('Inopc-Access-Token', xhr.getResponseHeader('Inopc-Access-Token'))
		accessType = +(xhr.getResponseHeader('Inopc-Access-Type') || '0')
		login = xhr.getResponseHeader('Inopc-Login')
		AuthPanel()

		// ��������� ���������� �������
		var json = {}
		try { json = JSON.parse(xhr.responseText) } catch (e) { return console.log('ask err: not json [' + xhr.responseText + ']') }

		if (accessType != ACCESSTYPE.FIRST) {
			if (json.Error) console.log('������: ' + json.Error)
			if (json.Warning) console.log(json.Warning)
			if (json.Done) console.log(json.Done)
		}

		if (!callback) return
		callback.call(null, json)
	}
	xhr.send(JSON.stringify(parameters.body || {}))
}