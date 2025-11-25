# Data Stores

Stores для кэширования данных с реактивным обновлением UI через MobX.

## Структура

- `BaseCacheStore` - базовый класс с общей логикой кэширования (TTL, валидация, инвалидация)
- `TagsStore` - управление тегами
- `BlocksStore` - управление блоками
- `SourcesStore` - управление источниками
- `ValuesStore` - управление значениями тегов
- `UsersStore` - управление пользователями
- `UserGroupsStore` - управление группами пользователей

## Использование

Все stores доступны через `AppStore`:

```typescript
import { useAppStore } from '@/store/useAppStore'

const MyComponent = () => {
  const store = useAppStore()

  // Получение данных (stale-while-revalidate)
  const tags = store.tagsStore.getTags(SourceType.Manual)
  const blocks = store.blocksStore.getBlocks()

  // Проверка состояния загрузки
  const isLoading = store.tagsStore.isLoadingTags()

  // Принудительное обновление
  await store.tagsStore.refreshTags()

  // Инвалидация кэша
  store.tagsStore.invalidateTag(tagId)
}
```

## Особенности

### Stale-while-revalidate паттерн
- Данные возвращаются из кэша мгновенно (если есть)
- Обновление происходит в фоне, если кэш устарел на 80% от TTL
- UI всегда показывает актуальные или устаревшие данные, но не пустоту

### TTL (Time To Live)
- **TagsStore**: 5 минут для списков, 10 минут для отдельного тега
- **BlocksStore**: 1 минута для списка/дерева, 5 минут для отдельного блока
- **SourcesStore**: 5 минут для списка, 30 секунд для активности
- **ValuesStore**: 30 секунд для значений, 10 секунд для статусов
- **UsersStore**: 5 минут для списка, 10 минут для отдельного пользователя
- **UserGroupsStore**: 5 минут для списка/дерева, 10 минут для отдельной группы

### Реактивность
Все stores используют MobX `makeObservable` с явными аннотациями для публичных методов, поэтому компоненты, обернутые в `observer()`, автоматически обновляются при изменении данных.

## Следующие шаги

После создания stores нужно:
1. Рефакторить компоненты для использования stores вместо прямых API-вызовов
2. Убрать локальные состояния для данных, которые теперь в stores
3. Использовать методы `refresh*()` для polling вместо прямых API-вызовов
