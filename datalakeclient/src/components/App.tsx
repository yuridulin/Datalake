import { useState } from 'react';
import Tree from './left/Tree';
import { Outlet } from 'react-router-dom';
import { UpdateContext } from '../context/updateContext';
import Title from './left/Title';

function App() {

	const [ lastUpdate, setUpdate ] = useState<Date>(new Date())

	return (
		<UpdateContext.Provider value={{
			lastUpdate,
			setUpdate
		}}>
			<div className="left">
				<Title />
				<Tree />
			</div>
			<div className="right">
				<Outlet />
			</div>
		</UpdateContext.Provider>
	);
}

export default App
