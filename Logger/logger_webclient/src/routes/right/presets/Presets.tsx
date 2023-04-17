import axios from "axios";
import { useEffect } from "react";

export default function Presets() {

	function load() {
		axios.post('agents/list')
			.then(res => {
				if (res.data) console.log('data')
			})
	}

	useEffect(() => {
		load()
		console.log('i fire once');
	}, [])

	return (
		<div>presets</div>
	)
}