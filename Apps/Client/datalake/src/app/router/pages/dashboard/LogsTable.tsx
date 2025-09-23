import LogsTableEl from '@/app/components/logsTable/LogsTableEl'
import useDatalakeTitle from '@/hooks/useDatalakeTitle'

const LogsTable = () => {
	useDatalakeTitle('Журнал')
	return <LogsTableEl />
}

export default LogsTable
