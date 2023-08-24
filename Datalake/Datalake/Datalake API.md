# Типы данных, используемые в api

```ts
type Block = {
	Id: number,
	ParentId: number,
	Name: string,
	Description: string,
	PropertiesRaw: string,
	Properties: { [key: string]: string },
	Tags: [] as Tag[],
	Children: [] as Block[]
}

type Tag = {
	Id: number,
	Name: string,
	Type: TagType,
	Description: string,
	SourceId: number,
	SourceItem: string,
	Interval: number,
	Source: string
}

type TagValue = {
	TagId: number,
	TagName: string,
	Date: string,
	Quality: TagQuality,
	Type: TagType,
	Using: TagHistoryUse,
	Value: string | number | bool | null,
}

enum TagQuality = {
	Bad = 0,
	Bad_NoConnect = 4,
	Good = 192,
	Good_ManualWrite = 216,
	Unknown = -1,
}

enum TagHistoryUse = {
	Initial = 0,
	Basic = 1,
}

enum TagType = {
	String = 0,
	Number = 1,
	Boolean = 2,
	Computed = 3,
}
```

# Начало URL 

`http://<адрес сервера>:83`

# Список тегов

`POST /api/tags/list`

Передаваемые данные
```ts
```

Получаемые данные
```ts
[] as Tag[]
```

# Список текущих значений по имени тега

`POST /api/tags/liveByNames`

Передаваемые данные
```ts
{
	"tags": string[]
}
```

Получаемые данные
```ts
[] as TagValue[]
```

# Список архивных значений по имени тега

`POST /api/tags/historyByNames`

Передаваемые данные
```ts
{
	"tags": string[],
	"old": Date,
	"young": Date,
	"resolution": number
}
```

Получаемые данные
```ts
[] as TagValue[]
```

# Список объектов

`POST /api/blocks/list`

Передаваемые данные
```ts
{
	"tags": string[],
	"old": Date,
	"young": Date,
	"resolution": number
}
```

Получаемые данные
```ts
[] as Block[]
```

# Текущие значения тегов объекта

`POST /api/blocks/live`

Передаваемые данные
```ts
{
	"id": number,
}
```

Получаемые данные
```ts
[] as TagValue[]
```

# Архивные значения тегов объекта

`POST /api/blocks/history`

Передаваемые данные
```ts
{
	"id": number,
	"old": Date,
	"young": Date,
	"resolution": number
}
```

Получаемые данные
```ts
[] as TagValue[]
```

