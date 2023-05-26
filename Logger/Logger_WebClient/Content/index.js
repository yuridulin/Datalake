function getCookie(name) {
	var matches = document.cookie.match(new RegExp("(?:^|; )" + name.replace(/([\.$?*|{}\(\)\[\]\\\/\+^])/g, '\\$1') + "=([^;]*)"))
	return matches ? decodeURIComponent(matches[1]) : undefined
}

function setCookie(name, value, options) {
	options = options || { expites: 99999990 }
	var expires = options.expires || 99999990
	options.SameSite = "Lax"
	if (typeof expires == "number" && expires) {
		var d = new Date()
		d.setTime(d.getTime() + expires * 1000)
		expires = options.expires = d
	}
	if (expires && expires.toUTCString) options.expires = expires.toUTCString()
	value = encodeURIComponent(value)
	var updatedCookie = name + "=" + value
	for (var propName in options) {
		updatedCookie += "; " + propName
		var propValue = options[propName]
		if (propValue !== true) updatedCookie += "=" + propValue
	}
	document.cookie = updatedCookie
}

function toggleTreeSub(el, id) {
	var icon = el
	if (icon.className != 'material-icons') {
		icon = el.parentNode.querySelector('.material-icons')
	}
	var treeSub = document.querySelector('[data-toggle="' + id + '"]')

	if (icon.innerHTML == 'chevron_right') {
		setCookie(id, 'more')
		treeSub.className = 'tree-sub tree-sub-opened'
		icon.innerHTML = 'expand_more'
	}
	else {
		setCookie(id, 'less')
		treeSub.className = 'tree-sub'
		icon.innerHTML = 'chevron_right'
	}
}


window.onload = window.onhashchange = function () {
	load(location.hash)
}

function load(currentHash) {
	console.log('hash before check', currentHash)
	if (location.hash != currentHash) {

		console.log('location.hash not match', location.hash)
		location.hash = currentHash
		return
	}
	if (currentHash[0] == '#') currentHash = currentHash.substring(1)
	if (currentHash[0] == '/') currentHash = currentHash.substring(1)

	console.log('hash after check', currentHash)

	var previousLink = document.querySelector('.tree-active')
	if (previousLink) {
		previousLink.classList.remove('tree-active')
	}

	var futureLink = currentHash
	do {
		var link = document.querySelector('.tree [href="#/' + futureLink + '"]')
		if (link) {
			if (link.parentNode.classList.contains('tree-item')) {
				link = link.parentNode
			}
			link.classList.add('tree-active')
			break
		}
		else {
			if (futureLink.length == 0) break
			futureLink = futureLink.substring(0, futureLink.lastIndexOf('/'))
		}
	}
	while (true)

	var view = document.querySelector('.view')
	if (currentHash == '') {
		currentHash = 'logs'
	}

	fetch(host + currentHash)
		.then(res => {
			if (!res.ok) {
				throw new Error();
			}
			return res.text()
		})
		.then(text => {
			view.innerHTML = text
		})
		.catch(() => {
			view.innerHTML = 'Произошла ошибка при загрузке контента'
		})
}

function go(route) {
	if (route[0] == '#') route = route.substring(1)
	if (route[0] == '/') route = route.substring(1)
	load('#/' + route)
}

function submit(selector) {
	var el = document.querySelector(selector || '[form]')
	if (!el) return alert('Нет формы для отправки')

	var form = new FormData()
	el.querySelectorAll('input[name],select[name],textarea[name]').forEach(x => {
		if (!x.classList.contains('excluded')) form.append(x.name, x.type == 'checkbox' ? x.checked : x.value)
	})

	fetch(el.getAttribute('form'), { method: 'POST', body: form })
		.then(res => {
			if (res.ok) return res.json()
			else throw new Error('Ошибка ' + res.statusText)
		})
		.then(json => {
			if (json.Done) {
				alert(json.Done)
			}
			if (json.Error) {
				alert('Ошибка: ' + json.Error)
			}
			if (json.UpdateView) {
				go(location.hash)
			}
			if (json.Link) {
				go(json.Link)
			}
		})
		.catch(err => {
			alert(err)
		})
}

function action(url) {
	fetch(url, { method: 'POST' })
		.then(res => {
			if (res.ok) return res.json()
			else throw new Error('Ошибка ' + res.statusText)
		})
		.then(json => {
			if (json.Done) {
				alert(json.Done)
			}
			if (json.Error) {
				alert('Ошибка: ' + json.Error)
			}
			if (json.UpdateView) {
				go(location.hash)
			}
			if (json.Link) {
				go(json.Link)
			}
		})
		.catch(err => {
			alert(err)
		})
}

function modal(selector) {
	var modalGroup = document.querySelector('.modal-group')
	if (!modalGroup) return

	modalGroup.style.display = 'none'

	if (!selector) return

	var modal = document.querySelector(selector)
	if (!modal) return

	modal.style.display = 'block'
}

function changeSelection(div) {
	div.classList.toggle('excluded')
	var input = div.querySelector('input')
	if (input) input.classList.toggle('excluded')
}

function addComparer() {
	var el = document.getElementById('comparersTemplate').cloneNode(true)
	el.removeAttribute('id')
	el.style.display = 'block'
	document.getElementById('comparers').appendChild(el)
}

function removeComparer(button) {
	button.parentNode.parentNode.removeChild(button.parentNode)
}

function computeComparers() { 
	var json = Array
		.from(document.getElementById('comparers').querySelectorAll('.comparer'))
		.map(x => {
			var obj = {}
			x.querySelectorAll('input,select,textarea').forEach(k => obj[k.name] = k.value)
			return obj
		})
	document.getElementById('comparersJson').value = JSON.stringify(json)
}
