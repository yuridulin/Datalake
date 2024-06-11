import { createRoot } from 'react-dom/client'
import Layout from './components/Layout'

const container = document.getElementById('root') as HTMLElement
const root = createRoot(container)
root.render(<Layout />)
