type Formular = {
    Name: string
    Description: string
    Interval: number
    Formula: string
    Fields: { [key: string]: string }
}

type FormularValue = {
    Name: string
    Value: number
    Error: string
}