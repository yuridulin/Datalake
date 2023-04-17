export type LogEntry = {
	Id: number
	MachineName: string
	JournalName: string
	Category: string
	Type: number
	EventId: number
	Message: string
	Source: string
	TimeGenerated: Date
	Username: string
	FilterId: number
}