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
	}

	for (var i = 1; i < arguments.length; i++) {
		parseProp(arguments[i])
	}

	return elem
}

function mount(selector) {
	var el = document.querySelector(selector)
	if (!el) return console.error('Не найден объект по селектору "' + selector + '"')
	el.innerHTML = ''
	for (var i = 1; i < arguments.length; i++) {
		var x = arguments[i]
		if (isNode(x)) {
			el.appendChild(x)
		} else {
			el.insertAdjacentHTML('beforeEnd', String(x))
		}
	}
	
	
}

function ls(name, value) {
	if (value) {
		localStorage.setItem(name, value)
	}
	else {
		return localStorage.getItem(name)
	}
}

function cookie(name, value, options) {
	if (value) {
		options = options || {};
		var expires = options.expires;
		options.SameSite = "Lax"
		if (typeof expires == "number" && expires) {
			var d = new Date();
			d.setTime(d.getTime() + expires * 1000);
			expires = options.expires = d;
		}
		if (expires && expires.toUTCString) options.expires = expires.toUTCString();
		value = encodeURIComponent(value);
		var updatedCookie = name + "=" + value;
		for (var propName in options) {
			updatedCookie += "; " + propName;
			var propValue = options[propName];
			if (propValue !== true) updatedCookie += "=" + propValue;
		}
		document.cookie = updatedCookie;
	}
	else {
		var matches = document.cookie.match(new RegExp("(?:^|; )" + name.replace(/([\.$?*|{}\(\)\[\]\\\/\+^])/g, '\\$1') + "=([^;]*)"));
		return matches ? decodeURIComponent(matches[1]) : undefined;
	}
}

function $(selector) {
	return document.querySelector(selector)
}