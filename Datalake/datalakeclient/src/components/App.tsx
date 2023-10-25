import { useState } from 'react';
import TreeContainer from './left/TreeContainer';
import { Link, Outlet } from 'react-router-dom';
import { UpdateContext } from '../context/updateContext';

export default function App() {

	const [ lastUpdate, setUpdate ] = useState<Date>(new Date())
	const [ checkedTags, setCheckedTags ] = useState<string[]>([])

	return (
		<UpdateContext.Provider value={{
			lastUpdate,
			setUpdate,
			checkedTags,
			setCheckedTags,
		}}>
			<div className="left">
				<Link to="/" className="title">Datalake</Link>
				<TreeContainer />
			</div>
			<div className="right">
				<Outlet />
			</div>
		</UpdateContext.Provider>
	)
}