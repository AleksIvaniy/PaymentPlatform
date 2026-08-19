# PaymentPlatform — ToDo / roadmap

Ниже не просто список технологий, а порядок, в котором мы будем развивать проект и одновременно разбирать production-подходы для .NET backend/highload.

## 1. Вынести FluentValidation из Controller

Сейчас в `PaymentsController` вручную вызывается:

```csharp
await validator.ValidateAsync(request, cancellationToken);
```

Что сделаем:

- создадим собственный async validation filter;
- автоматически будем получать `IValidator<T>` из DI;
- Controller перестанет знать о механике валидации;
- единообразно будем возвращать ошибки валидации.

Цель: понять filters, DI внутри pipeline и separation of concerns.

---

## 2. Global exception handling + ProblemDetails

Сделаем централизованную обработку ошибок через `IExceptionHandler` / `UseExceptionHandler`.

Разберём маппинг ошибок:

- validation error -> 400;
- not found -> 404;
- business/domain conflict -> 409;
- unexpected exception -> 500.

Ответы приведём к единому формату `ProblemDetails`.

Дополнительно подробно разберём внутреннюю механику `IProblemDetailsService`:

```text
GlobalExceptionHandler
-> ProblemDetailsContext
-> IProblemDetailsService.TryWriteAsync()
-> IEnumerable<IProblemDetailsWriter>
-> writer.CanWrite(context)
-> первый подходящий writer.WriteAsync(context)
-> true
```

Если ни один зарегистрированный `IProblemDetailsWriter` не подходит, `TryWriteAsync()` возвращает `false`. Поэтому рассмотрим defensive fallback через `WriteAsJsonAsync` и отдельно поймём, почему результат `TryWriteAsync()` и результат `IExceptionHandler.TryHandleAsync()` имеют разный смысл.

Сделаем собственный `IProblemDetailsWriter`, чтобы на практике увидеть:

- как writer регистрируется в DI;
- как `ProblemDetailsService` получает коллекцию `IEnumerable<IProblemDetailsWriter>`;
- как работает `CanWrite(ProblemDetailsContext)`;
- почему порядок writers имеет значение;
- почему слишком общий `CanWrite(...) => true` может перехватить все ответы;
- чем `TryWriteAsync` отличается от `WriteAsync`;
- как централизованно добавлять `traceId` и другие extensions в `ProblemDetails`.

Цель: убрать `try/catch` из controller/service, сделать предсказуемый API contract для ошибок и понимать не только использование `ProblemDetails`, но и то, как механизм writers устроен внутри ASP.NET Core.

---

## 3. Нормализовать Application DTO / Result

Сейчас `CreatePaymentResult` используется и для создания, и для чтения платежа.

Что сделаем:

- отделим `CreatePaymentResult` от общего `PaymentDto` / `PaymentResult`;
- API request/response оставим в `Api`;
- application-команды и результаты оставим в `Application`;
- обсудим границы между Request, Command, DTO, Result и Domain Entity.

---

## 4. Добавить операции изменения состояния Payment

Реализуем use cases:

- `MarkAsProcessing`;
- `Complete`;
- `Fail`;
- позже `Cancel`, если понадобится.

Для каждой операции добавим endpoint/use case и проверим domain invariants.

Цель: domain entity должна управлять состоянием сама, а не через публичные `set`.

---

## 5. Unit of Work и границы SaveChanges

Сейчас `PaymentRepository.AddAsync` сам вызывает `SaveChangesAsync`.

Переделаем, когда появится несколько изменений в одном use case:

```text
Payment
+
AuditRecord
+
OutboxMessage
-> один SaveChangesAsync
-> одна транзакция
```

Добавим абстракцию вроде `IUnitOfWork` или осознанно используем сам `DbContext` как Unit of Work.

Цель: понять transaction boundary и почему repository не всегда должен сам делать commit.

---

## 6. Транзакции PostgreSQL / EF Core

Смоделируем use case, где несколько изменений должны быть атомарными.

Разберём:

- implicit transaction в `SaveChanges`;
- explicit transaction;
- `BeginTransactionAsync`;
- commit / rollback;
- ACID на реальном коде;
- когда transaction scope действительно нужен.

---

## 7. Idempotency для POST /payments

Добавим `Idempotency-Key`.

Сценарий:

```text
клиент отправил POST
сервер выполнил операцию
клиент получил timeout
клиент повторил POST
```

Система не должна создать второй платёж.

Разберём:

- unique constraint в PostgreSQL;
- почему `SELECT -> if not exists -> INSERT` недостаточно;
- race condition двух одинаковых запросов;
- сохранение результата идемпотентной операции.

---

## 8. Optimistic concurrency

Добавим версию записи / concurrency token.

Смоделируем два запроса, которые одновременно меняют один Payment или Account.

Разберём:

- lost update;
- optimistic concurrency;
- `DbUpdateConcurrencyException`;
- version column;
- HTTP 409 Conflict;
- retry vs отказ клиенту.

---

## 9. Добавить Account / Balance сценарий

Чтобы реально изучить банковскую конкурентность, одной сущности `Payment` мало.

Добавим Account:

```text
Account
- Id
- UserId
- Balance
- Version
```

Сценарий:

```text
Balance = 1000
Request A -> -800
Request B -> -800
```

Разберём:

- атомарное списание;
- optimistic concurrency;
- pessimistic locking;
- `SELECT ... FOR UPDATE`;
- isolation levels;
- когда лучше сделать один атомарный SQL UPDATE.

---

## 10. SQL и производительность EF Core

Специально создадим плохие запросы и будем их исправлять.

Разберём:

- N+1;
- `AsNoTracking`;
- projection через `Select`;
- `Include`;
- split query;
- индексы;
- composite indexes;
- generated SQL;
- execution plan;
- pagination;
- offset vs keyset pagination.

---

## 11. Redis

Добавим Redis как distributed cache.

Первый сценарий — чтение Payment:

```text
GET /payments/{id}
-> Redis
-> HIT: вернуть
-> MISS: PostgreSQL -> Redis
```

Разберём:

- cache-aside;
- TTL;
- invalidation;
- stale data;
- cache stampede;
- serialization;
- что нельзя бездумно кешировать в банковской системе.

---

## 12. Kafka

Добавим асинхронную обработку платежей.

После создания Payment API публикует событие:

```text
PaymentCreated
```

Разберём:

- producer;
- topic;
- partition;
- partition key;
- consumer;
- consumer group;
- offset;
- ordering;
- масштабирование consumers.

---

## 13. Outbox Pattern

Не будем делать опасный dual-write:

```text
SaveChanges в PostgreSQL
потом Publish в Kafka
```

Потому что приложение может упасть между этими операциями.

Сделаем:

```text
Payment
+
OutboxMessage
-> одна DB transaction
```

Background worker будет читать Outbox и отправлять события в Kafka.

Цель: понять надёжную публикацию integration events.

---

## 14. BackgroundService для Outbox

Создадим worker через `BackgroundService`.

Здесь отдельно разберём:

- почему hosted service обычно singleton;
- `IServiceScopeFactory`;
- создание scope;
- scoped `DbContext` внутри worker;
- graceful shutdown;
- `CancellationToken`.

Это будет реальный сценарий, где `IServiceScopeFactory` действительно нужен.

---

## 15. Безопасная конкурентная обработка Outbox

Запустим несколько экземпляров worker и разберём, как не отправить одно сообщение одновременно дважды.

Рассмотрим:

- optimistic update;
- status/locked_until;
- PostgreSQL `FOR UPDATE SKIP LOCKED`;
- distributed lock;
- почему distributed lock часто не лучший первый вариант.

---

## 16. Idempotent Kafka Consumer / Inbox

Kafka может доставить событие повторно.

Создадим таблицу обработанных сообщений / Inbox:

```text
MessageId UNIQUE
```

Consumer должен безопасно переживать повторную доставку.

Разберём:

- at-least-once delivery;
- effectively-once processing;
- offset commit;
- transaction boundary consumer + DB.

---

## 17. Retry + exponential backoff

Смоделируем временно недоступный внешний сервис.

Разделим ошибки на:

- transient: timeout, 502, 503;
- permanent/business: insufficient funds, invalid request.

Добавим retry с backoff и jitter.

Разберём, почему retry без idempotency может быть опасен.

---

## 18. Dead Letter Queue

После исчерпания retry сообщение должно попадать в DLQ.

Разберём:

- poison messages;
- диагностику;
- ручной replay;
- повторную обработку;
- хранение причины ошибки.

---

## 19. Correlation ID и structured logging

Добавим middleware для `CorrelationId`.

Один идентификатор должен пройти через:

```text
HTTP -> API -> Kafka event -> Worker -> external call
```

Используем structured logging:

```csharp
_logger.LogInformation(
    "Payment {PaymentId} processed for {UserId}",
    paymentId,
    userId);
```

Разберём scopes и контекст логирования.

---

## 20. Health Checks

Добавим:

- `/health/live`;
- `/health/ready`.

Проверим зависимости:

- PostgreSQL;
- Redis;
- Kafka.

Разберём liveness vs readiness и зачем это Kubernetes / orchestrator.

---

## 21. Authentication / Authorization

Добавим JWT authentication.

Разберём:

- access token;
- claims;
- roles;
- policies;
- authentication vs authorization;
- `[Authorize]`;
- ownership check: пользователь может читать только свой Payment.

Позже обсудим OpenID Connect и внешний Identity Provider.

---

## 22. Rate Limiting

Добавим ограничения на API.

Например:

```text
100 requests / minute / client
```

Разберём:

- fixed window;
- sliding window;
- token bucket;
- concurrency limiter;
- локальный limiter vs distributed limiter.

---

## 23. HttpClientFactory + resilience

Добавим mock внешнего банковского/платёжного сервиса.

Разберём:

- `IHttpClientFactory`;
- typed client;
- connection reuse;
- timeout;
- retry;
- circuit breaker;
- cancellation;
- propagation correlation ID.

---

## 24. Unit tests

Создадим отдельный test project.

Покроем:

- создание Payment;
- domain transitions;
- invalid states;
- PaymentService;
- validators.

Разберём:

- Arrange / Act / Assert;
- mock / stub / fake;
- когда mock repository полезен, а когда мешает.

---

## 25. Integration tests + Testcontainers

Поднимем настоящий PostgreSQL в Docker для тестов.

Проверим полный flow:

```text
POST /payments
-> PostgreSQL
-> GET /payments/{id}
```

Также проверим:

- migrations;
- idempotency;
- concurrency;
- repository queries.

Позже добавим Redis/Kafka containers.

---

## 26. Docker Compose

Соберём локальную инфраструктуру:

```text
PaymentPlatform.Api
PostgreSQL
Redis
Kafka
Worker
```

Проект должен подниматься воспроизводимо одной командой.

Разберём:

- Dockerfile;
- multi-stage build;
- environment variables;
- ports;
- volumes;
- networks.

---

## 27. Observability

Добавим базовую наблюдаемость:

- logs;
- metrics;
- traces.

Разберём OpenTelemetry и distributed tracing.

Хотим видеть один запрос сквозь несколько компонентов системы.

---

## 28. Нагрузочное тестирование

Проверим приложение под нагрузкой.

Сценарии:

- много параллельных GET;
- много POST;
- конкурентное списание;
- медленная PostgreSQL;
- Redis недоступен;
- consumer отстаёт.

Будем смотреть latency, throughput, error rate и bottlenecks.

---

## 29. Graceful degradation / fault scenarios

Специально будем ломать зависимости:

- PostgreSQL временно недоступен;
- Redis недоступен;
- Kafka недоступна;
- consumer падает;
- внешний API отвечает медленно.

Для каждого случая определим ожидаемое поведение системы.

Цель: думать не только happy-path, но и production failure modes.

---

## 30. Security / configuration cleanup

Перед финальной версией:

- убрать реальные пароли из repository;
- environment variables / user-secrets;
- не логировать чувствительные данные;
- ограничить Swagger по environment;
- проверить CORS;
- HTTPS;
- минимизировать выдачу внутренних ошибок клиенту.

---

## 31. Архитектурный рефакторинг после появления реальной сложности

Только после того, как проект станет достаточно большим, оценим, нужен ли переход от общего `PaymentService` к отдельным use-case handlers:

```text
CreatePaymentHandler
GetPaymentHandler
CompletePaymentHandler
```

Тогда осознанно разберём CQRS, а не будем добавлять его только ради паттерна.

Также сравним:

- Repository vs direct DbContext;
- Domain entity == EF entity vs отдельная persistence entity;
- Service approach vs Command/Handler;
- Clean Architecture trade-offs.

---

## Итоговая цель проекта

К концу проекта нужно не просто уметь написать API, а уметь объяснить полный жизненный цикл финансовой операции:

```text
HTTP request
-> validation
-> application use case
-> domain rules
-> transaction
-> PostgreSQL
-> Outbox
-> Kafka
-> Consumer
-> idempotency
-> Redis
-> logging/tracing
-> retry/failure handling
```

И для каждого механизма отвечать на три вопроса:

1. Как это работает?
2. Какую проблему это решает?
3. Что сломается или ухудшится, если этого механизма не будет?
