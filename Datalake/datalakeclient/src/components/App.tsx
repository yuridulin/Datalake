import { useState } from 'react';
import Tree from './left/Tree';
import { Link, Outlet } from 'react-router-dom';
import { UpdateContext } from '../context/updateContext';

export default function App() {

	const [ lastUpdate, setUpdate ] = useState<Date>(new Date())

	return (
		<UpdateContext.Provider value={{
			lastUpdate,
			setUpdate
		}}>
			<div className="left">
				<Link to="/" className="title">Datalake</Link>
				<Tree />
			</div>
			<div className="right">
				<Outlet />
			</div>
		</UpdateContext.Provider>
	)
}