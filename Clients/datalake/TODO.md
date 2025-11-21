# TODO: Рефакторинг работы с серверными данными

## Цель
Создать централизованный слой для работы с API, добавить кэширование данных с реактивным обновлением UI через MobX, устранить дублирование запросов и улучшить производительность приложения.

---

## 1. Создание слоя сервисов для работы с API

### Зачем нужно
- Абстракция над сгенерированным API-клиентом
- Централизованная обработка ошибок
- Переиспользование логики запросов
- Упрощение тестирования
- Готовность к добавлению кэширования

### Что нужно сделать

#### 1.1. Создать структуру директорий
```
src/
  services/
    api/
      tagsService.ts
      blocksService.ts
      sourcesService.ts
      valuesService.ts
      usersService.ts
      userGroupsService.ts
      auditService.ts
    types/
      serviceTypes.ts
```

#### 1.2. Реализовать базовый класс сервиса (опционально)
- Общая логика обработки ошибок
- Базовые методы для работы с API

#### 1.3. Создать сервисы для каждой сущности

**TagsService** (`services/api/tagsService.ts`):
- `getAll(sourceId?: SourceType): Promise<TagInfo[]>`
- `getById(id: number): Promise<TagInfo>`
- `create(data: TagCreateRequest): Promise<TagInfo>`
- `update(id: number, data: TagUpdateRequest): Promise<void>`
- `delete(id: number): Promise<void>`
- Централизованная обработка ошибок с понятными сообщениями

**BlocksService** (`services/api/blocksService.ts`):
- `getAll(): Promise<BlockWithTagsInfo[]>`
- `getTree(): Promise<BlockTreeInfo[]>`
- `getById(id: number): Promise<BlockFullInfo>`
- `create(data: BlockCreateRequest): Promise<BlockInfo>`
- `update(id: number, data: BlockUpdateRequest): Promise<void>`
- `delete(id: number): Promise<void>`
- `move(id: number, parentId: number | null): Promise<void>`

**SourcesService** (`services/api/sourcesService.ts`):
- `getAll(withCustom?: boolean): Promise<SourceInfo[]>`
- `getById(id: number): Promise<SourceFullInfo>`
- `getActivity(sourceIds: number[]): Promise<SourceActivityInfo[]>`
- `getItems(sourceId: number): Promise<SourceItemInfo[]>`
- `create(): Promise<SourceInfo>`
- `update(id: number, data: SourceUpdateRequest): Promise<void>`
- `delete(id: number): Promise<void>`

**ValuesService** (`services/api/valuesService.ts`):
- `get(requests: DataValuesRequest[]): Promise<DataValuesResponse[]>`
- `write(requests: DataValuesWriteRequest[]): Promise<void>`
- `getStatus(tagsId: number[]): Promise<Record<number, string>>`

**UsersService** (`services/api/usersService.ts`):
- `getAll(): Promise<UserInfo[]>`
- `get(userGuid: string): Promise<UserInfo[]>`
- `create(data: UserCreateRequest): Promise<UserInfo>`
- `update(userGuid: string, data: UserUpdateRequest): Promise<void>`
- `delete(userGuid: string): Promise<void>`

**UserGroupsService** (`services/api/userGroupsService.ts`):
- `getAll(): Promise<UserGroupInfo[]>`
- `getTree(): Promise<UserGroupTreeInfo[]>`
- `getById(guid: string): Promise<UserGroupFullInfo>`
- `create(data: UserGroupCreateRequest): Promise<UserGroupInfo>`
- `update(guid: string, data: UserGroupUpdateRequest): Promise<void>`
- `delete(guid: string): Promise<void>`
- `move(guid: string, parentGuid: string | null): Promise<void>`

**AuditService** (`services/api/auditService.ts`):
- `get(params: AuditGetParams): Promise<LogInfo[]>`

#### 1.4. Интегрировать сервисы в AppStore
- Добавить поля для каждого сервиса в `AppStore`
- Инициализировать сервисы в конструкторе `AppStore`
- Заменить прямое использование `store.api` на использование сервисов

---

## 2. Создание MobX stores для кэширования данных

### Зачем нужно
- Кэширование данных для мгновенного отклика UI
- Stale-while-revalidate паттерн (отдаем кэш сразу, обновляем в фоне)
- Реактивное обновление UI при изменении данных в кэше
- Синхронизация данных между компонентами
- Устранение дублирования запросов

### Что нужно сделать

#### 2.1. Создать структуру директорий
```
src/
  store/
    dataStores/
      tagsStore.ts
      blocksStore.ts
      sourcesStore.ts
      valuesStore.ts
      usersStore.ts
      userGroupsStore.ts
```

#### 2.2. Реализовать базовый класс для кэширования (опционально)
- Общая логика TTL (Time To Live)
- Методы проверки валидности кэша
- Методы инвалидации кэша

#### 2.3. Создать MobX stores для каждой сущности

**TagsStore** (`store/dataStores/tagsStore.ts`):
- **Кэш данных:**
  - `_tagsCache: Map<string, TagInfo[]>` - кэш списков тегов (ключ: 'all' | 'source_{id}')
  - `_tagsByIdCache: Map<number, TagInfo>` - кэш отдельных тегов по ID
- **Состояния:**
  - `_loadingStates: Map<string, boolean>` - состояния загрузки
  - `_lastFetchTime: Map<string, number>` - время последнего запроса
- **TTL:**
  - Список всех тегов: 5 минут
  - Список по источнику: 5 минут
  - Конкретный тег: 10 минут
- **Публичные методы:**
  - `getTags(sourceId?: SourceType): TagInfo[]` - получение с stale-while-revalidate
  - `getTagById(id: number): TagInfo | undefined` - получение конкретного тега
  - `isLoading(sourceId?: SourceType): boolean` - проверка состояния загрузки
  - `invalidateTag(id: number): void` - инвалидация кэша при изменении
  - `refreshTags(sourceId?: SourceType): Promise<void>` - принудительное обновление
- **Приватные методы:**
  - `loadTags(sourceId?: SourceType): Promise<void>` - синхронная загрузка
  - `refreshTags(sourceId?: SourceType): Promise<void>` - асинхронное обновление в фоне
  - `isCacheValid(cacheKey: string, type: string): boolean` - проверка валидности
  - `shouldRefresh(cacheKey: string, type: string): boolean` - нужно ли обновление (80% TTL)

**BlocksStore** (`store/dataStores/blocksStore.ts`):
- **Кэш данных:**
  - `_blocksCache: BlockWithTagsInfo[] | null` - кэш всех блоков
  - `_blocksTreeCache: BlockTreeInfo[] | null` - кэш дерева блоков
  - `_blocksByIdCache: Map<number, BlockFullInfo>` - кэш отдельных блоков
- **TTL:**
  - Список блоков: 1 минута (обновляется через polling)
  - Дерево блоков: 1 минута
  - Конкретный блок: 5 минут
- **Публичные методы:**
  - `getBlocks(): BlockWithTagsInfo[]`
  - `getTree(): BlockTreeInfo[]`
  - `getBlockById(id: number): BlockFullInfo | undefined`
  - `invalidateBlock(id: number): void`
  - `refreshBlocks(): Promise<void>`

**SourcesStore** (`store/dataStores/sourcesStore.ts`):
- **Кэш данных:**
  - `_sourcesCache: SourceInfo[] | null`
  - `_sourcesByIdCache: Map<number, SourceFullInfo>`
  - `_activityCache: Map<number, SourceActivityInfo>` - кэш состояний активности
- **TTL:**
  - Список источников: 5 минут
  - Состояния активности: 30 секунд (часто обновляются)
  - Конкретный источник: 10 минут
- **Публичные методы:**
  - `getSources(): SourceInfo[]`
  - `getSourceById(id: number): SourceFullInfo | undefined`
  - `getActivity(sourceIds: number[]): Record<number, SourceActivityInfo>`
  - `invalidateSource(id: number): void`
  - `refreshActivity(sourceIds: number[]): Promise<void>`

**ValuesStore** (`store/dataStores/valuesStore.ts`):
- **Особенности:**
  - Короткий TTL (30 секунд) из-за частых обновлений
  - Кэш по ключу запроса (tagsId + timeSettings)
  - Поддержка polling для live-режима
- **Кэш данных:**
  - `_valuesCache: Map<string, ValueRecord[]>` - кэш значений по ключу запроса
  - `_statusCache: Map<number, string>` - кэш статусов тегов
- **TTL:**
  - Значения: 30 секунд
  - Статусы: 10 секунд
- **Публичные методы:**
  - `getValues(request: DataValuesRequest): ValueRecord[]`
  - `getStatus(tagsId: number[]): Record<number, string>`
  - `invalidateValues(tagsId: number[]): void`

**UsersStore** (`store/dataStores/usersStore.ts`):
- **Кэш данных:**
  - `_usersCache: UserInfo[] | null`
  - `_usersByGuidCache: Map<string, UserInfo>`
- **TTL:**
  - Список пользователей: 5 минут
  - Конкретный пользователь: 10 минут
- **Публичные методы:**
  - `getUsers(): UserInfo[]`
  - `getUserByGuid(guid: string): UserInfo | undefined`
  - `invalidateUser(guid: string): void`

**UserGroupsStore** (`store/dataStores/userGroupsStore.ts`):
- **Кэш данных:**
  - `_groupsCache: UserGroupInfo[] | null`
  - `_groupsTreeCache: UserGroupTreeInfo[] | null`
  - `_groupsByGuidCache: Map<string, UserGroupFullInfo>`
- **TTL:**
  - Список групп: 5 минут
  - Дерево групп: 5 минут
  - Конкретная группа: 10 минут
- **Публичные методы:**
  - `getGroups(): UserGroupInfo[]`
  - `getTree(): UserGroupTreeInfo[]`
  - `getGroupByGuid(guid: string): UserGroupFullInfo | undefined`
  - `invalidateGroup(guid: string): void`

#### 2.4. Интегрировать stores в AppStore
- Добавить поля для каждого store в `AppStore`
- Инициализировать stores в конструкторе `AppStore`
- Убедиться, что stores используют `makeAutoObservable` для реактивности

---

## 3. Интеграция нового подхода в компоненты

### Зачем нужно
- Использовать кэшированные данные вместо прямых запросов
- Получить реактивное обновление UI через MobX
- Устранить дублирование запросов
- Упростить код компонентов

### Что нужно сделать

#### 3.1. Рефакторинг компонентов списков

**TagsList.tsx:**
- Удалить локальное состояние `const [tags, setTags] = useState<TagInfo[]>([])`
- Удалить `useEffect` с запросом данных
- Заменить на: `const tags = store.tagsStore.getTags()` (внутри `observer`)
- Убрать `useRef` для предотвращения множественных запросов (это теперь в store)

**TagsManualList.tsx:**
- Аналогично `TagsList.tsx`
- Использовать: `const tags = store.tagsStore.getTags(SourceType.Manual)`
- При создании тега вызывать `store.tagsStore.invalidateTag()` и `store.tagsStore.refreshTags()`

**TagsCalculatedList.tsx:**
- Аналогично `TagsManualList.tsx`
- Использовать: `const tags = store.tagsStore.getTags(SourceType.Calculated)`

**TagsAggregatedList.tsx:**
- Аналогично `TagsManualList.tsx`
- Использовать: `const tags = store.tagsStore.getTags(SourceType.Aggregated)`

**BlocksTree.tsx:**
- Заменить `const [data, setData] = useState<BlockWithTagsInfo[]>([])` на `const blocks = store.blocksStore.getBlocks()`
- Удалить `loadBlocks` функцию или заменить на `store.blocksStore.refreshBlocks()`
- Убрать `PollingLoader` с `loadBlocks`, использовать `store.blocksStore.refreshBlocks()` напрямую
- При создании блока вызывать `store.blocksStore.invalidateBlock()` и `store.blocksStore.refreshBlocks()`

**SourcesList.tsx:**
- Заменить `const [sources, setSources] = useState<DataCell[]>([])` на использование `store.sourcesStore.getSources()`
- Заменить `getStates` на `store.sourcesStore.getActivity(sourceIds)`
- Убрать локальную логику загрузки, использовать store

**UsersList.tsx:**
- Заменить локальное состояние на `store.usersStore.getUsers()`
- Убрать дублирование запросов

**UserGroupsTreeList.tsx:**
- Заменить локальное состояние на `store.userGroupsStore.getTree()`

#### 3.2. Рефакторинг компонентов форм и просмотра

**TagForm.tsx:**
- Заменить `store.api.inventoryTagsGetAll()` на `store.tagsStore.getTags()`
- Заменить `store.api.inventoryTagsGet(id)` на `store.tagsStore.getTagById(id)`
- При сохранении вызывать `store.tagsStore.updateTag()` вместо прямого API-вызова
- После успешного обновления вызывать `store.tagsStore.invalidateTag(id)`

**TagView.tsx:**
- Использовать `store.tagsStore.getTagById(id)` вместо прямого запроса
- UI автоматически обновится при изменении данных в store

**BlockForm.tsx:**
- Аналогично `TagForm.tsx`, использовать `store.blocksStore`
- При перемещении блока вызывать `store.blocksStore.invalidateBlock()`

**SourceForm.tsx:**
- Использовать `store.sourcesStore` вместо прямых API-вызовов

**UserForm.tsx, UserGroupForm.tsx:**
- Использовать соответствующие stores

#### 3.3. Рефакторинг компонентов работы со значениями

**TagsValuesViewer.tsx:**
- Заменить `store.api.dataValuesGet()` на `store.valuesStore.getValues()`
- Использовать кэшированные данные для мгновенного отклика
- Обновление через polling будет работать через store

**TagsValuesWriter.tsx:**
- Использовать `store.valuesStore` для получения текущих значений
- После записи вызывать `store.valuesStore.invalidateValues(tagsId)`

**TagsTable.tsx:**
- Заменить `loadValues` на использование `store.valuesStore.getValues()` и `store.valuesStore.getStatus()`
- Убрать локальное состояние для значений и статусов

#### 3.4. Рефакторинг компонентов выбора

**QueryTreeSelect.tsx:**
- Заменить `store.api.inventoryBlocksGetTree()` на `store.blocksStore.getTree()`
- Заменить `store.api.inventoryTagsGetAll()` на `store.tagsStore.getTags()`
- Данные будут автоматически обновляться через MobX

**TagTreeSelect.tsx:**
- Аналогично `QueryTreeSelect.tsx`

#### 3.5. Обновление компонентов с polling

**Компоненты с PollingLoader:**
- Оставить `PollingLoader`, но изменить функции polling
- Вместо прямых API-вызовов использовать методы `refresh*()` из stores
- Например: `<PollingLoader pollingFunction={() => store.tagsStore.refreshTags()} interval={60000} />`

**Компоненты:**
- `BlocksTree.tsx` - использовать `store.blocksStore.refreshBlocks()`
- `SourcesList.tsx` - использовать `store.sourcesStore.refreshActivity()`
- `TagsValuesViewer.tsx` - использовать `store.valuesStore.refreshValues()`
- `TagsTable.tsx` - использовать `store.valuesStore.refreshStatus()`

#### 3.6. Убедиться, что компоненты обернуты в observer
- Проверить все компоненты, использующие stores
- Обернуть в `observer()` из `mobx-react-lite` для реактивного обновления
- Пример: `export default observer(TagsList)`

#### 3.7. Удалить устаревший код
- Удалить все прямые вызовы `store.api.*` в компонентах
- Удалить локальные состояния для данных, которые теперь в stores
- Удалить дублирующиеся `useEffect` с запросами
- Удалить `useRef` для предотвращения множественных запросов (теперь это в stores)

---

## 4. Дополнительные улучшения (опционально)

### 4.1. Оптимистичные обновления
- При создании/обновлении сущности сразу обновлять кэш
- В случае ошибки откатывать изменения

### 4.2. Умная инвалидация кэша
- При обновлении тега инвалидировать все списки, содержащие этот тег
- При обновлении блока инвалидировать дерево блоков

### 4.3. Дебаунсинг запросов
- Если несколько компонентов запрашивают одни и те же данные одновременно, объединять запросы

### 4.4. Обработка офлайн-режима
- Использовать кэш при отсутствии соединения
- Показывать индикатор, что данные могут быть устаревшими

### 4.5. Метрики и мониторинг
- Логирование использования кэша (hit/miss)
- Отслеживание времени загрузки данных

---

## Порядок выполнения

1. **Этап 1: Сервисы** (1-2 дня)
   - Создать структуру директорий
   - Реализовать все сервисы
   - Интегрировать в AppStore
   - Протестировать базовую функциональность

2. **Этап 2: Stores** (2-3 дня)
   - Создать базовую структуру stores
   - Реализовать TagsStore (как пример)
   - Протестировать кэширование и реактивность
   - Реализовать остальные stores

3. **Этап 3: Интеграция** (3-4 дня)
   - Рефакторинг компонентов списков
   - Рефакторинг компонентов форм
   - Рефакторинг компонентов работы со значениями
   - Тестирование и исправление багов

4. **Этап 4: Оптимизация** (1-2 дня)
   - Оптимистичные обновления
   - Умная инвалидация кэша
   - Финальное тестирование

---

## Критерии готовности

- ✅ Все компоненты используют stores вместо прямых API-вызовов
- ✅ Нет дублирования запросов при переходе между страницами
- ✅ UI мгновенно отображает данные из кэша
- ✅ Данные автоматически обновляются в фоне
- ✅ Все компоненты реактивно обновляются при изменении данных
- ✅ Кэш правильно инвалидируется при изменениях
- ✅ Нет утечек памяти (правильная очистка кэша)
- ✅ Обработка ошибок централизована и понятна

---

## Примечания

- При реализации stores важно правильно настроить `makeAutoObservable` с исключением приватных полей
- Использовать `runInAction` для всех асинхронных обновлений состояния
- TTL можно настраивать в зависимости от частоты изменений данных
- Для часто изменяемых данных (значения тегов) использовать короткий TTL
- Для редко изменяемых данных (справочники) использовать длинный TTL
