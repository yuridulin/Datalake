export interface Log {
	Date: Date
	Module: string
	Message: string
	ProgramLogType: LogType;
}

export enum LogType {
	Trace = 0,
	Information = 1,
	Success = 2,
	Warning = 3,
	Error = 4,
}