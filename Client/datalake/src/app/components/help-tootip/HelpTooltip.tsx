import { QuestionCircleOutlined } from '@ant-design/icons'
import { Popover } from 'antd'
import React, { useState } from 'react'

interface HelpTooltipProps {
	/**
	 * Задержка в секундах перед появлением справки при наведении.
	 * По умолчанию — 1 секунда.
	 */
	delay?: number
	/**
	 * Дополнительное оформление иконки.
	 */
	iconStyle?: React.CSSProperties
	/**
	 * Содержимое подсказки/справки, задаваемое через children.
	 */
	children: React.ReactNode
}

const HelpTooltip: React.FC<HelpTooltipProps> = ({
	delay = 1,
	iconStyle,
	children,
}) => {
	// Состояние: видимость поповера и флаг, открыт ли он по клику
	const [visible, setVisible] = useState(false)
	const [isClickTriggered, setIsClickTriggered] = useState(false)

	// Обработчик для изменений видимости при hover;
	// если поповер открыт по клику, игнорируем автоматическое скрытие.
	const handleVisibleChange = (newVisible: boolean) => {
		if (!isClickTriggered) {
			setVisible(newVisible)
		}
	}

	// По клику открываем поповер (без задержки) и отмечаем, что он открыт по клику.
	const handleIconClick = (e: React.MouseEvent) => {
		// Предотвращаем всплытие событий клика, чтобы не конфликтовать с hover-режимом.
		e.stopPropagation()
		e.preventDefault()
		setVisible(true)
		setIsClickTriggered(true)
	}

	// Закрытие поповера по клику на крестик
	const closePopover = (e: React.MouseEvent) => {
		e.stopPropagation()
		setVisible(false)
		setIsClickTriggered(false)
	}

	// Формирование контента справки с крестиком для закрытия (если открыт по клику)
	const popoverContent = (
		<div
			style={{
				position: 'relative',
				paddingRight: isClickTriggered ? 20 : undefined,
			}}
		>
			{children}
			{isClickTriggered && (
				<button
					onClick={closePopover}
					style={{
						position: 'absolute',
						top: 0,
						right: 0,
						border: 'none',
						background: 'transparent',
						fontSize: '12px',
						cursor: 'pointer',
					}}
					aria-label='Close'
				>
					&#x2715;
				</button>
			)}
		</div>
	)

	return (
		<Popover
			content={popoverContent}
			// Можно задать title, если он нужен, или оставить его null
			title={null}
			trigger='hover' // по умолчанию открытие происходит по hover
			open={visible}
			onOpenChange={handleVisibleChange}
			mouseEnterDelay={delay}
		>
			&ensp;
			<span
				onClick={handleIconClick}
				style={{
					display: 'inline-flex',
					alignItems: 'center',
					cursor: 'pointer',
				}}
			>
				<QuestionCircleOutlined
					style={{
						fontSize: '14px', // иконка меньше
						verticalAlign: 'middle',
						...iconStyle,
					}}
				/>
			</span>
		</Popover>
	)
}

export default HelpTooltip
