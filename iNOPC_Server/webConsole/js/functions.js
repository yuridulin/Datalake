/**
 * Создание тега html с любым уровнем вложенности (реализация hyperscript)
 * @param {string} tagNameRaw Наименование html тега
 */
function h(tagNameRaw) {

	var className = [], id, tag = ''

	// разбор имени (в нем могут быть указаны классы, id и атрибуты)
	tagNameRaw.split('.').forEach((x, i) => {
		if (x.indexOf('#') > -1) {
			id = x.substring(x.indexOf('#') + 1)
			if (i == 0) tag = x.substring(0, x.indexOf('#'))
			else className.push(x.substring(0, x.indexOf('#')))
		} else {
			if (i == 0) tag = x
			else className.push(x)
		}
	})

	// создание html элемента и присвоение ему разобранных настроек
	var elem = document.createElement(tag)
	if (id) elem.id = id
	if (className.length > 0) elem.className = className.join(' ')

	function parseProp(prop) {
		if (prop === null || prop === undefined) {
			elem.innerHTML = 'NULL'
		}

		else if (isProps(prop)) {
			for (var key in prop) {
				var data = prop[key]
				if (key == 'style') {
					if (isProps(data)) {
						for (var styleProperty in data) {
							elem.style[styleProperty] = data[styleProperty]
						}
					} else elem.setAttribute('style', String(data))
				}
				else if (key == 'className') elem.className = data
				else if (key == 'innerHTML') elem.innerHTML = String(data)
				else if (key == 'value') elem.value = data
				else if (key == 'checked') elem.checked = !!data
				else if (typeof data == 'function') elem[key] = data
				else elem.setAttribute(key, String(data))
			}
		}

		else if (isArray(prop)) {
			prop.forEach(function (propChild) {
				parseProp(propChild)
			})
		}

		else if (isNode(prop)) {
			elem.appendChild(prop)
		}

		else if (isPrimitive(prop)) {
			elem.insertAdjacentHTML('beforeend', String(prop))
		}

		function isArray(obj) {
			return (Object.prototype.toString.call(obj) === '[object Array]')
		}

		function isNode(obj) {
			return (obj.tagName && typeof obj === 'object')
		}

		function isProps(obj) {
			return (!obj.length && obj.toString() === '[object Object]')
		}

		function isPrimitive(obj) {
			return (typeof obj === 'string' || typeof obj === 'number' || typeof obj === 'boolean')
		}
	}

	for (var i = 1; i < arguments.length; i++) {
		parseProp(arguments[i])
	}

	return elem
}

/**
 * Замещение содержимого тега по селектору на переданное в аргументах
 * @param {string} selector Селектор css
 */
function mount(selector) {
	var el = document.querySelector(selector)
	if (!el) return console.error('Не найден объект по селектору "' + selector + '"')
	el.innerHTML = ''
	for (var i = 1; i < arguments.length; i++) {
		var x = arguments[i]
		if (x.tagName) {
			el.appendChild(x)
		} else {
			el.insertAdjacentHTML('beforeEnd', String(x))
		}
	}
	
	
}

/**
 * Считывание/запись параметров через local Storage
 * @param {string} name
 * @param {string} value
 */
function ls(name, value) {
	if (value) {
		localStorage.setItem(name, value)
	}
	else {
		return localStorage.getItem(name)
	}
}

/**
 * Обращение к серверу за конкретной информацией с авторизацией каждого запроса
 * @param {{ method: string, body: object }} parameters Объект с данными, передаваемыми на сервер
 * @param {(json: object) => void} callback Обработчик, в который передается результат запроса
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