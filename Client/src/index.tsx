import { createRoot } from 'react-dom/client'
import Layout from './components/Layout'

const container = document.getElementById('root')
const root = createRoot(container!)
root.render(<Layout />)
