import { Button, Col, Input, Radio, Row } from 'antd'
import { ViewModeState } from './utils/SourceItems.types'

type SourceItemsToolbarProps = {
	search: string
	onSearchChange: (value: string) => void
	viewMode: ViewModeState
	onViewModeChange: (mode: ViewModeState) => void
	onReload: () => void
}

export const SourceItemsToolbar = ({
	search,
	onSearchChange,
	viewMode,
	onViewModeChange,
	onReload,
}: SourceItemsToolbarProps) => {
	return (
		<Row>
			<Col flex='auto'>
				<Input.Search
					style={{ marginBottom: '1em', alignItems: 'center', justifyContent: 'space-between' }}
					placeholder='Введите запрос для поиска по значениям и тегам. Можно написать несколько запросов, разделив пробелами'
					value={search}
					onChange={(e) => onSearchChange(e.target.value)}
				/>
			</Col>
			<Col flex='14em'>
				&emsp;
				{/* Переключатель режимов просмотра */}
				<Radio.Group value={viewMode} onChange={(e) => onViewModeChange(e.target.value)} style={{ marginRight: 16 }}>
					<Radio.Button value='table'>Таблица</Radio.Button>
					<Radio.Button value='tree'>Дерево</Radio.Button>
				</Radio.Group>
			</Col>
			<Col flex='6em'>
				<Button onClick={onReload}>Обновить</Button>
			</Col>
		</Row>
	)
}
