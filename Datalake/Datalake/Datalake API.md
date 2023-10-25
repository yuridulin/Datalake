# Типы данных, используемые в api Datalake

### Объект (в будущем "Сущность")
```ts
type Block = {
	Id: number,
	ParentId: number,
	Name: string,
	Description: string,
	PropertiesRaw: string,
	Properties: { [key: string]: string },
	Tags: Tag[],
	Children: Block[]
}
```

### Тег (единица представления данных)
```ts
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
```

### Часть истории значений тега
```ts
type HistoryResponse = {
	Id: number,
	TagName: string,
	Type: TagType,
	Func: AggFunc,
	Values: HistoryValue[]
}
```

### Значение истории по тегу
```ts
type HistoryValue = {
	Date: Date,
	Value: string | number | bool | null,
	Quality: TagQuality,
	Using: TagHistoryUse
}
```

### Объект дерева
```ts
type TreeItem = {
	Id: number,
	Name: string,
	FullName: string,
	Type: TreeType,
	Items: TreeItem[],
}
```

### Перечисления

### 
```ts
// достоверность значения
enum TagQuality = {
	Bad = 0,
	Bad_NoConnect = 4,
	Good = 192,
	Good_ManualWrite = 216,
	Unknown = -1
}

// способ получения значения
enum TagHistoryUse = {
	Initial = 0,
	Basic = 1
}

// тип значений тега
enum TagType = {
	String = 0,
	Number = 1,
	Boolean = 2,
	Computed = 3
}

// агрегирующая функция, применяемая к значениям при выводе истории
enum AggFunc = {
	List = 0,
	Sum = 1,
	Avg = 2,
	Min = 3,
	Max = 4
}

// тип элемента в дереве объектов
enum TreeType = {
	Source = 0,
	TagGgroup = 1,
	Tag = 2,
	Block = 3,
	Link = 4
}

// тип источника данных
enum SourceType = {
	Inopc = 0,
	Datalake = 1
}
```



# Список тегов

`POST http://<адрес сервера>:83/api/tags/list`

Список существующих в базе тегов

### Передаваемые данные
```ts
names?: string[]
```

### Получаемые данные
```ts
Tag[]
```



# Дерево объектов

`POST http://<адрес сервера>:83/api/console/tree`

Получение дерева объектов, первый уровень которого - источники, по которым сгруппированы теги. Теги собираются в древовидную структуру по имени, в качестве разделителя используется точка (OPC DA подход)

### Передаваемые данные
```ts
```

### Получаемые данные
```ts
TreeItem[]
```




# Текущие значения тегов

`POST /api/tags/live`

### Передаваемые данные
```ts
// Массив идентификаторов тегов
// Если он пустой, используется массив имён
id?: number[]

// Массив имён тегов
// Не используется, если указаны идентификаторы
// Если пустой - будут выбраны все теги
names: string[]
```

### Получаемые данные
```ts
HistoryResponse[]
```



# История значений тегов

`POST /api/tags/historyByNames`

Список значений из базы данных по группам. Каждая группа запрашивает значения по тегам из списка, используя указанный диапазон дат, шаг и агрегирующую функцию.

Список тегов предполагается как массив с именами тегов. Он может быть пустым, в этом случае будут выбраны все существующие в базе теги.

Диапазон времени выбирается как отрезок от Old включительно до Young включительно. Если Young не указывается, будет использована текущая дата. Если Old не указывается, используется Young с отброшенным временем. Если не указываются обе даты, в качестве ответа будут возвращены текущие значения.

Шаг измеряется в миллисекундах. Если он равен нулю, значения будут получены по изменению.

Доступные агрегирующие функции:
* List - возвращает список значений от старых записей к новым. Для остальных функций List выступает источником данных и выполняется в фоне
* Sum - сумма значений
* Avg - среднее значение
* Min - минимальное значение
* Max - максимальное значение

### Передаваемые данные
```ts
request: {
	tags: string[],
	old: Date,
	young: Date,
	resolution: number,
	func: AggFunc
}[]
```

### Получаемые данные
```ts
HistoryResponse[]
```



# Список объектов

`POST /api/blocks/list`

Передаваемые данные
```ts
```

Получаемые данные
```ts
Block[]
```

# Текущие значения тегов объекта

`POST /api/blocks/live`

Передаваемые данные
```ts
id: number
```

Получаемые данные
```ts
TagValue[]
```

# История значения тегов объекта

`POST /api/blocks/history`

Передаваемые данные
```ts
id: number,
old: Date,
young: Date,
resolution: number
```

Получаемые данные
```ts
TagValue[]
```

