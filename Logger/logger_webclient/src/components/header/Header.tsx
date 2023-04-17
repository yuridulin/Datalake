import { Button } from "antd";
import React from "react";
import router from "../../router";
import './Header.css'
import { ArrowLeftOutlined } from "@ant-design/icons";
import MyIcon from "../myIcon/MyIcon";

type HeaderProps = {
	back?: string
	icon?: string
	iconTitle?: string
	onIconClick?: React.MouseEventHandler<HTMLAnchorElement> & React.MouseEventHandler<HTMLButtonElement>
	title: string
	children: React.ReactNode
}

export default function Header (props: HeaderProps) {
	return (
		<header className="header">
			{props.back && 
				<Button type="link" icon={<ArrowLeftOutlined />} onClick={() => router.navigate(props.back || '')}>Назад</Button>
			}
			{props.icon && (
				props.onIconClick
					? <Button type="link" onClick={props.onIconClick} title={props.iconTitle}>
						<MyIcon icon={props.icon} style={{ fontSize: '1.5em', marginRight: '.5em' }} />
					</Button>
					: <MyIcon icon={props.icon} style={{ fontSize: '1.5em', marginRight: '.5em' }} />
			)}
			<div className="header-title">
				{props.title}
			</div>
			<div className="header-buttons">{props.children}</div>
		</header>
	)
}