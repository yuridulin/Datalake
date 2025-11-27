# Информация о вычисляемых данных для Computed свойств

Этот документ содержит информацию о потенциальных computed свойствах, которые могут быть полезны для сторов, но пока не реализованы.

## 1. Сопоставление Тег + Блок для компонента значений

### Описание
Компоненты значений (`TagsValuesViewer`, `TagsValuesWriter`) используют пары `[blockId, tagId]` для идентификации связей между блоками и тегами. Эти пары кодируются в строки через `encodeBlockTagPair` и используются как ключи в маппингах.

### Текущее использование
- **Файлы**:
  - `src/app/components/values/TagsValuesViewer.tsx`
  - `src/app/components/values/TagsValuesWriter.tsx`
  - `src/app/router/pages/blocks/block/BlockView.tsx`
  - `src/app/components/tagTreeSelect/treeSelectShared.tsx`

### Потенциальные computed свойства

#### В BlocksStore:
```typescript
/**
 * Маппинг blockId -> массив связанных тегов
 * Используется для быстрого получения всех тегов блока
 */
get blockTagsMap(): Map<number, BlockTagRelationInfo[]>

/**
 * Маппинг tagId -> массив блоков, в которых используется тег
 * Обратная связь для поиска блоков по тегу
 */
get tagBlocksMap(): Map<number, number[]>
```

#### В TagsStore:
```typescript
/**
 * Маппинг tagId -> массив блоков, где используется тег
 * На основе данных из BlockDetailedInfo.tags
 */
get tagsBlocksMap(): Map<number, number[]>

/**
 * Группировка тегов по блокам
 * Полезно для компонентов, отображающих теги в контексте блоков
 */
get tagsByBlocks(): Map<number, TagSimpleInfo[]>
```

#### Комбинированный computed (можно добавить в AppStore или отдельный CrossStore):
```typescript
/**
 * Полный маппинг пар [blockId, tagId] -> информация о связи
 * Ключ: encodeBlockTagPair(blockId, tagId)
 * Значение: { blockId, tagId, blockName, tagName, relationType, localName }
 */
get blockTagPairsMap(): Map<string, BlockTagPairInfo>
```

## 2. Группировка тегов по источникам

### Статус
✅ **Реализовано** в `TagsStore.tagsBySource`

## 3. Статистика по связям Тег-Блок

### Описание
Полезная статистика для анализа использования тегов в блоках.

### Потенциальные computed свойства

#### В BlocksStore:
```typescript
/**
 * Статистика по использованию тегов в блоках
 */
get tagUsageStatistics() {
  return computed(() => {
    const blocks = this.blocksCache.get()
    const tagUsage = new Map<number, { count: number, blocks: number[] }>()

    blocks.forEach(block => {
      block.tags?.forEach(tag => {
        const tagId = tag.tag?.id ?? tag.tagId ?? 0
        if (!tagUsage.has(tagId)) {
          tagUsage.set(tagId, { count: 0, blocks: [] })
        }
        const usage = tagUsage.get(tagId)!
        usage.count++
        if (!usage.blocks.includes(block.id)) {
          usage.blocks.push(block.id)
        }
      })
    })

    return {
      totalRelations: Array.from(tagUsage.values()).reduce((sum, u) => sum + u.count, 0),
      uniqueTagsInBlocks: tagUsage.size,
      tagsUsedInMultipleBlocks: Array.from(tagUsage.values()).filter(u => u.blocks.length > 1).length,
      byTag: Object.fromEntries(tagUsage)
    }
  }).get()
}
```

## 4. Маппинг источников с их тегами

### Описание
Быстрый доступ к тегам источника.

### Потенциальные computed свойства

#### В SourcesStore:
```typescript
/**
 * Маппинг sourceId -> массив тегов источника
 * На основе данных из SourceWithSettingsAndTagsInfo
 */
get sourcesTagsMap(): Map<number, TagSimpleInfo[]>
```

#### В TagsStore (уже частично реализовано через tagsBySource):
```typescript
/**
 * Обратный маппинг: tagId -> sourceId
 * Для быстрого определения источника тега
 */
get tagsSourceMap(): Map<number, SourceType>
```

## 5. Комбинированные данные для управления доступом

### Описание
Компоненты управления доступом (`AccessSettings`, `UserGroupAccessForm`, `BlockAccessForm`) часто работают с комбинациями пользователей, групп, источников, блоков и тегов.

### Текущее использование
- **Файлы**:
  - `src/app/router/pages/admin/tabs/AccessSettings.tsx`
  - `src/app/router/pages/usergroups/usergroup/access/UserGroupAccessForm.tsx`

### Потенциальные computed свойства

#### В AppStore или отдельном AccessStore:
```typescript
/**
 * Полный маппинг объектов доступа с их именами
 * Объединяет данные из всех сторов для удобного доступа
 */
get accessObjectsMap() {
  return computed(() => {
    return {
      users: this.usersStore.usersMap,
      userGroups: this.userGroupsStore.groupsMap,
      sources: this.sourcesStore.sourcesMap,
      blocks: this.blocksStore.flatTreeMap, // или отдельный маппинг
      tags: new Map(this.tagsStore.getTags().map(t => [t.guid, t]))
    }
  }).get()
}

/**
 * Иерархия доступа: пользователь -> группы -> объекты
 * Полезно для отображения дерева прав доступа
 */
get accessHierarchy() {
  return computed(() => {
    // Логика построения иерархии
  }).get()
}
```

## 6. Поиск и фильтрация

### Описание
Многие компоненты используют поиск по тегам, блокам, источникам.

### Потенциальные computed свойства

#### В TagsStore:
```typescript
/**
 * Индексированный поиск по тегам
 * Поддерживает быстрый поиск по имени, GUID, источнику
 */
get searchIndex(): {
  byName: Map<string, TagSimpleInfo[]>
  byGuid: Map<string, TagSimpleInfo>
  bySource: Map<SourceType, TagSimpleInfo[]>
}
```

#### В BlocksStore (уже реализовано):
✅ `searchBlocks(searchTerm: string): BlockTreeInfo[]`

## 7. Статистика использования тегов в значениях

### Описание
Информация о том, какие теги активно используются для получения значений.

### Потенциальные computed свойства

#### В ValuesStore или комбинированном:
```typescript
/**
 * Статистика использования тегов
 * На основе данных из кэша запросов значений
 */
get tagsUsageStatistics() {
  return computed(() => {
    // Анализ запросов в valuesCache
    // Подсчет частоты запросов по tagId
  }).get()
}
```

## 8. Дерево блоков с тегами для TreeSelect

### Описание
Компонент `TagTreeSelect` использует комбинацию блоков и тегов для построения дерева выбора.

### Текущее использование
- **Файлы**:
  - `src/app/components/tagTreeSelect/TagTreeSelect.tsx`
  - `src/app/components/tagTreeSelect/treeSelectShared.tsx`

### Потенциальные computed свойства

#### В BlocksStore:
```typescript
/**
 * Дерево блоков с вложенными тегами в формате для TreeSelect
 * Готовые данные для компонента TagTreeSelect
 */
get treeWithTagsForSelect(): DataNode[]
```

## Рекомендации по реализации

1. **Приоритет 1 (высокий)**:
   - `blockTagsMap` и `tagBlocksMap` в BlocksStore
   - `tagsBlocksMap` в TagsStore
   - `blockTagPairsMap` (комбинированный)

2. **Приоритет 2 (средний)**:
   - Статистика по связям тег-блок
   - Маппинг источников с тегами
   - Поисковые индексы

3. **Приоритет 3 (низкий)**:
   - Комбинированные данные для доступа
   - Статистика использования в значениях
   - Готовые данные для TreeSelect

## Примечания

- Все computed свойства должны использовать `computed()` из MobX для реактивности
- При работе с данными из нескольких сторов, можно создать отдельный `CrossStore` или добавить computed в `AppStore`
- Важно учитывать производительность при построении сложных индексов
- Некоторые computed могут требовать дополнительных API запросов для полной информации
