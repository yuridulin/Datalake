import { createRoot } from 'react-dom/client'
import AppSetup from './app/AppSetup'

const container = document.getElementById('root')
const root = createRoot(container!)
root.render(<AppSetup />)
