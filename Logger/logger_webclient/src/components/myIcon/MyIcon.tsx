type MyIconProps = {
	icon: string
	title?: string
	color?: 'done' | 'warning' | 'error'
	style?: React.CSSProperties
}

const colors = {
	'done': 'green',
	'warning': 'yellow',
	'error': '#ff4d4f',
}

export default function MyIcon(props: MyIconProps) {

	let style = props.style
	if (props.color) style = { ...style, color: colors[props.color] }

	return (
		<i
			title={props.title}
			style={style}
			className="material-icons"
		>{props.icon}</i>
	)
}