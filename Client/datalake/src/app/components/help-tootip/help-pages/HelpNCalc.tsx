import HelpTooltip from '@/app/components/help-tootip/HelpTooltip'

export default function HelpNCalc() {
	return (
		<HelpTooltip>
			<div>
				<p>
					Для расчета используется библиотека NCalc. Основные правила:
				</p>
				<br />
				<ul style={{ paddingLeft: '1em' }}>
					<li>
						<strong>Арифметические операторы: </strong>
						поддерживаются <code>+</code>, <code>-</code>,{' '}
						<code>*</code>, <code>/</code> и <code>%</code>.
					</li>
					<li>
						<strong>Операторы сравнения: </strong>
						можно использовать <code>==</code>, <code>!=</code>,{' '}
						<code>&lt;</code>, <code>&gt;</code>, <code>&lt;=</code>{' '}
						и <code>&gt;=</code>.
					</li>
					<li>
						<strong>Логические операторы: </strong>
						<code>and</code>, <code>or</code>, <code>not</code>{' '}
						позволяют создавать логические выражения.
					</li>
					<li>
						<strong>Функции: </strong>
						встроенные функции, такие как <code>Sin(x)</code>,{' '}
						<code>Cos(x)</code>, <code>Sqrt(x)</code> и другие.
						Полный список функций доступен{' '}
						<a href='https://ncalc.github.io/ncalc/articles/functions.html#built-in-functions'>
							здесь
						</a>
						.
					</li>
					<li>
						<strong>Параметры: </strong>
						используются для передачи значений в выражение. Параметр
						можно указать просто по имени, а при выполнении значения
						подставляются через словарь.
					</li>
					<li>
						<strong>Скобки: </strong>
						круглые скобки задают порядок вычислений.
					</li>
				</ul>
				<br />
				<p>
					Для использования тегов при расчете нужно указать
					необходимые теги во входных параметрах формулы, присвоив им
					короткие имена (первый столбец).
				</p>
				<p>
					Короткие имена можно использовать при составлении формулы
					как переменные.
				</p>
				<p>
					Чтобы избегать неправильного прочтения парсером переменных,
					рекомендуется брать их в квадратные скобки.
				</p>
				<br />
				<p>
					Деление на ноль вызывает ошибку вычисления, рекомендуется
					делать проверку через <code>if</code>.
				</p>
				<br />
				<p>
					Пример:
					<br />
					Формула:
					<div>
						<input
							style={{ padding: '3px', width: '13em' }}
							value='([input1] + [input2]) * 101'
						/>
					</div>
					Входные параметры:
					<div
						style={{
							marginBottom: '.25em',
							display: 'grid',
							gridTemplateColumns: '1fr 2fr',
							width: '20em',
						}}
					>
						<input value='input1' />
						<select>
							<option selected value='Tag1'>
								Tag1
							</option>
							<option value='Tag2'>Tag2</option>
						</select>
					</div>
					<div
						style={{
							marginBottom: '.25em',
							display: 'grid',
							gridTemplateColumns: '1fr 2fr',
							width: '20em',
						}}
					>
						<input value='input2' />
						<select>
							<option value='Tag1'>Tag1</option>
							<option selected value='Tag2'>
								Tag2
							</option>
						</select>
					</div>
				</p>
			</div>
		</HelpTooltip>
	)
}
