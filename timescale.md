### Включаем timescale

```sql
CREATE EXTENSION IF NOT EXISTS timescaledb;
```

### Создание общей таблицы

```sql
CREATE TABLE public."TagsHistory" (
    "TagId" int NOT NULL,
    "Date" timestamp NOT NULL,
    "Text" text,
    "Number" float4,
    "Quality" int NOT NULL
);
```

### Создание гипертаблицы

```sql
SELECT create_hypertable('"TagsHistory"', by_range('Date', INTERVAL '1 day'));
```

### Смена даты

```sql
ALTER TABLE "TagsHistory"
ALTER COLUMN "Date" TYPE timestamptz
USING "Date" AT TIME ZONE 'Europe/Moscow';
```

### Перенос данных в гипертаблицу

```sql
DO $$
DECLARE
    tbl text;
BEGIN
    FOR tbl IN
        SELECT quote_ident(tablename)
        FROM pg_tables
        WHERE tablename ~ '^TagsHistory_\d{4}_\d{2}_\d{2}$'
    LOOP
        EXECUTE format(
            'INSERT INTO "TagsHistory" ("TagId", "Date", "Text", "Number", "Quality")
             SELECT "TagId", "Date" AT TIME ZONE ''Europe/Moscow'', "Text", "Number", "Quality"
             FROM %s',
            tbl
        );
    END LOOP;
END;
$$;
```