export interface ProgramLog {
	Id: number
	Module: string
	Timestamp: Date
	Message: string
	ProgramLogType: ProgramLogType;
}

export enum ProgramLogType {
	Trace = 0,
	Information = 1,
	Success = 2,
	Warning = 3,
	Error = 4,
}