# LogiCore — Транспортно-логистическая система

**Дополнительное задание по дисциплине «Объектно-ориентированное программирование»**

**Платформа:** .NET 8  
**Язык:** C#

## Описание проекта

LogiCore — это ядро информационной системы транспортно-логистической компании. Система управляет доставкой грузов различных категорий (обычные, скоропортящиеся, хрупкие, опасные, негабаритные) с использованием собственного парка транспорта (фургоны, фуры, рефрижераторы, самолёты, суда, дроны).

Проект демонстрирует:
- Проектирование объектно-ориентированной архитектуры
- Применение паттернов проектирования
- Использование механизмов ООП (наследование, полиморфизм, инкапсуляция)
- Работу с событиями и делегатами
- LINQ-запросы для аналитики
- Сериализацию состояния в JSON
- Тестирование бизнес-логики

## Структура решения

```
LogiCore.sln
├── LogiCore.Domain          # Доменная модель (не зависит от консоли)
│   ├── Entities             # Сущности: Vehicle, Cargo, Order, Customer, Route
│   ├── Enums                # Перечисления: VehicleState, OrderStatus, TransportConditions
│   ├── Events               # EventArgs классы и делегаты
│   ├── Exceptions           # Иерархия исключений от LogisticsException
│   ├── Interfaces           # Интерфейсы: IEntity, ITariffStrategy, IInsurable и др.
│   └── ValueObjects         # Структуры: RoutePoint
├── LogiCore.App             # Консольное приложение
│   ├── DeliveryCosts        # Декораторы для расчёта стоимости
│   ├── Extensions           # Extension methods для коллекций
│   ├── Factories            # VehicleFactory (Factory Method)
│   ├── Repositories         # Repository<T> с обобщениями
│   ├── Services             # DeliveryService, ReportService, JsonPersistenceService
│   ├── Strategies           # Тарифные стратегии (Strategy)
│   ├── Subscribers          # ConsoleNotifier, FileLogger (Observer)
│   └── Program.cs           # Demo Scenario и меню
└── LogiCore.Tests           # Юнит-тесты (xUnit)
```

## Архитектура

### Иерархия транспорта

```
Vehicle (abstract)
├── Van                      # Фургон для лёгких грузов
├── Truck                    # Фура с коэффициентом платных дорог
├── RefrigeratedTruck        # Рефрижератор с температурным контролем
├── CargoPlane               # Самолёт с надбавкой за вес
├── CargoShip                # Судно с надбавкой за негабарит
└── DroneCourier (sealed)    # Дрон с ограничением дальности
```

### Иерархия грузов

```
Cargo (abstract)
├── StandardCargo            # Обычный груз (IStackable)
├── PerishableCargo          # Скоропортящийся (ITemperatureSensitive, IInsurable)
├── FragileCargo             # Хрупкий (IInsurable)
├── DangerousCargo           # Опасный с классом 1-9
└── OversizedCargo           # Негабаритный
```

### Машина состояний заказа

```
Created → Assigned → InTransit → Delivered
   ↓          ↓
Cancelled  Cancelled
```

## Таблица соответствия требованиям (T1–T10)

| Требование | Реализация | Файлы |
|------------|------------|-------|
| **T1. Инкапсуляция** | Нет публичных полей, проверка инвариантов в конструкторах, readonly/init, коллекции через IReadOnlyCollection<T> | `Vehicle.cs`, `Cargo.cs`, `Order.cs`, `Customer.cs` |
| **T2. Наследование и полиморфизм** | abstract/virtual/override, sealed класс DroneCourier, вызовы base.CanCarry() | `Vehicle.cs` и наследники, `Cargo.cs` и наследники |
| **T3. Интерфейсы и вариантность** | 8 собственных интерфейсов, explicit implementation в PerishableCargo, IReadOnlyRepository<out T>, IValidator<in T> | `ITemperatureSensitive.cs`, `IInsurable.cs`, `IStackable.cs`, `IEntity.cs`, `ITariffStrategy.cs`, `IDeliveryCost.cs`, `IReadOnlyRepository.cs`, `IValidator.cs`; `PerishableCargo.cs` (строки 48-56) |
| **T4. Обобщения** | Repository<T> where T : class, IEntity с Add, Remove, FindAll(Predicate<T>), индексатором по Guid, IEnumerable<T> через yield return | `Repository.cs`; Extension method: `EnumerableExtensions.cs` |
| **T5. Делегаты и события** | 4 события (OrderCreated, OrderStatusChanged, VehicleOverloadAttempt, DeliveryCompleted) с EventArgs, собственный delegate LogisticsEventHandler, 2 подписчика с using для файлов, демонстрация отписки | `LogisticsEventHandler.cs` (delegate), `*EventArgs.cs`, `ConsoleNotifier.cs`, `FileLogger.cs`, `DeliveryService.cs`, `Program.cs` (строка 272) |
| **T6. Исключения** | Иерархия от LogisticsException, фильтры when, throw;, finally/using | `LogisticsException.cs`, `CargoValidationException.cs`, `IncompatibleCargoException.cs`, `VehicleOverloadException.cs`, `RouteNotFoundException.cs`, `InvalidOrderStateException.cs`; `JsonPersistenceService.cs` (when), `FileLogger.cs` (using) |
| **T7. Паттерны** | Strategy (3 стратегии тарифов), Decorator (3 декоратора услуг), Factory (VehicleFactory), Observer (события), Singleton (TariffStrategyRegistry через Lazy<T>) | Strategy: `StandardTariffStrategy.cs`, `ExpressTariffStrategy.cs`, `HeavyCargoTariffStrategy.cs`; Decorator: `InsuranceDecorator.cs`, `PriorityDeliveryDecorator.cs`, `FragilePackagingDecorator.cs`; Factory: `VehicleFactory.cs`; Singleton: `TariffStrategyRegistry.cs` |
| **T8. LINQ** | 7 отчётов: Where, Select, OrderBy(Descending), GroupBy, Join, Sum/Average/Count, ToDictionary/ToLookup, query syntax | `ReportService.cs` (методы GetTopVehiclesByRevenue, GetOrdersByStatus, GetAverageLoadByVehicleType, GetCustomersAboveThreshold, GetCargoToCustomerReport с query syntax, GetDangerousCargoByClass, GetCargoTypeStatistics) |
| **T9. Сериализация** | JSON через System.Text.Json, обработка отсутствующего/повреждённого файла, DTO-снимок | `JsonPersistenceService.cs`, `Program.cs` (строки 313-340) |
| **T10. Enum и структуры** | [Flags] enum TransportConditions с побитовыми операциями, struct RoutePoint с перегрузкой оператора - и explicit operator string | `TransportConditions.cs`, `RoutePoint.cs`, `Program.cs` (строки 284-293) |

## Используемые паттерны проектирования

### 1. Strategy (Стратегия)
**Где:** Тарифные стратегии (`ITariffStrategy`)  
**Зачем:** Позволяет динамически выбирать алгоритм расчёта стоимости доставки  
**Реализация:**
- `StandardTariffStrategy` — базовый тариф
- `ExpressTariffStrategy` — экспресс-доставка (×1.5)
- `HeavyCargoTariffStrategy` — надбавка за тяжёлые грузы

### 2. Decorator (Декоратор)
**Где:** Дополнительные услуги (`IDeliveryCost`)  
**Зачем:** Динамическое добавление опций к базовой стоимости  
**Реализация:**
- `InsuranceDecorator` — страхование груза
- `PriorityDeliveryDecorator` — срочная доставка
- `FragilePackagingDecorator` — упаковка хрупкого груза

### 3. Factory Method (Фабричный метод)
**Где:** `VehicleFactory`  
**Зачем:** Инкапсуляция создания транспорта разных типов  
**Реализация:** Создание экземпляров Van, Truck, RefrigeratedTruck, CargoPlane, CargoShip, DroneCourier по типу

### 4. Observer (Наблюдатель)
**Где:** События C# (`event`)  
**Зачем:** Уведомление подписчиков о событиях системы  
**Реализация:** `DeliveryService` публикует события, `ConsoleNotifier` и `FileLogger` подписываются

### 5. Singleton (Одиночка)
**Где:** `TariffStrategyRegistry`  
**Зачем:** Единственный экземпляр реестра стратегий  
**Реализация:** Потокобезопасный Singleton через `Lazy<T>`

## SOLID принципы

### S — Single Responsibility (Единственная ответственность)
- `CargoCompatibilityValidator` — только проверка совместимости грузов
- `DeliveryService` — управление доставками
- `ReportService` — генерация отчётов
- `JsonPersistenceService` — сериализация

### O — Open/Closed (Открыт для расширения, закрыт для изменения)
- Добавление нового типа транспорта не требует изменения Vehicle
- Новые тарифные стратегии добавляются без изменения существующих
- Новые декораторы услуг не меняют базовый расчёт

### L — Liskov Substitution (Подстановка Лискова)
- Все наследники Vehicle могут использоваться через базовый класс
- RefrigeratedTruck расширяет, а не заменяет логику Vehicle.CanCarry()

### I — Interface Segregation (Разделение интерфейсов)
- `ITemperatureSensitive` — только для грузов с температурными требованиями
- `IInsurable` — только для страхуемых грузов
- `IStackable` — только для штабелируемых грузов

### D — Dependency Inversion (Инверсия зависимостей)
- `DeliveryService` зависит от `CargoCompatibilityValidator` (абстракция)
- Стратегии тарифов используются через `ITariffStrategy`

## Запуск проекта

### Требования
- .NET 8 SDK или новее

### Сборка
```bash
cd LogiCore
dotnet build LogiCore.sln
```

### Запуск
```bash
cd LogiCore.App
dotnet run
```

### Запуск тестов
```bash
cd LogiCore.Tests
dotnet test
```

## Demo Scenario

При запуске приложение автоматически выполняет демонстрационный сценарий:

1. **Создание парка** — 6 транспортных средств всех типов
2. **Создание грузов** — 10 грузов (5 типов)
3. **Создание клиентов** — 3 клиента
4. **Создание маршрутов** — 3 маршрута с расчётом расстояния
5. **Демонстрация IncompatibleCargoException** — попытка совместить опасный и скоропортящийся груз
6. **Создание заказов** — 5 валидных заказов
7. **Демонстрация VehicleOverloadException** — попытка перегрузки дрона
8. **Подбор транспорта и расчёт стоимости** — применение стратегий и цепочки декораторов
9. **Полный жизненный цикл заказа** — от Created до Delivered с событиями
10. **Демонстрация отписки** — отписка от события и проверка
11. **Демонстрация [Flags] enum** — побитовые операции с TransportConditions
12. **LINQ отчёты** — 7 различных отчётов
13. **JSON сериализация** — сохранение → очистка → загрузка → проверка

## Вывод событий

- **Консоль** — цветной вывод с временными метками
- **Файл** — `delivery_log.txt` с полным журналом событий

## Ключевые особенности реализации

### Explicit Interface Implementation
```csharp
// PerishableCargo.cs
decimal ITemperatureSensitive.MinTemperature => RequiredMinTemperatureC;
decimal IInsurable.InsuranceValue => DeclaredValue;
```
Используется для избежания конфликта имён и разделения публичного API класса от интерфейсных контрактов.

### Собственный итератор с yield return
```csharp
// Repository.cs
public IEnumerator<T> GetEnumerator()
{
    foreach (var item in _items)
    {
        yield return item;  // Ленивая итерация
    }
}
```

### Собственный делегат
```csharp
// LogisticsEventHandler.cs
public delegate void LogisticsEventHandler(object sender, LogisticsEventArgs e);
```

### Перегрузка оператора
```csharp
// RoutePoint.cs
public static decimal operator -(RoutePoint pointA, RoutePoint pointB)
{
    // Формула гаверсинусов для расчёта расстояния
}
```

### Using для автоматического освобождения ресурсов
```csharp
// Program.cs, FileLogger.cs
using (var fileLogger = new FileLogger(logFilePath))
{
    // Автоматический вызов Dispose при выходе из блока
}
```

## Тестирование

Проект содержит 25+ юнит-тестов, покрывающих:
- Расчёт стоимости для каждого типа транспорта
- Валидацию совместимости грузов (4 правила)
- Машину состояний заказа (допустимые/недопустимые переходы)
- Repository<T> (Add, Remove, FindAll, итерация, индексатор)
- Композицию декораторов
- События и отписку
- Ковариантность IReadOnlyRepository
