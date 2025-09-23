import { CheckOutlined, CopyOutlined } from '@ant-design/icons'
import { Button, message } from 'antd'
import { useState } from 'react'

type CopyableTextProps = {
	text: string
}

const CopyableText = ({ text }: CopyableTextProps) => {
	const [copied, setCopied] = useState(false)

	const handleCopy = async () => {
		try {
			await navigator.clipboard.writeText(text)
			setCopied(true)
			message.success('Текст скопирован!', 1.5)

			setTimeout(() => setCopied(false), 1000)
		} catch {
			message.error('Не удалось скопировать текст')
		}
	}

	return (
		<>
			{text}
			<Button
				size='small'
				icon={copied ? <CheckOutlined style={{ color: '#52c41a' }} /> : <CopyOutlined />}
				onClick={handleCopy}
				style={{ marginLeft: 8 }}
			/>
		</>
	)
}

export default CopyableText
