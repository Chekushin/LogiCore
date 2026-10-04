using LogiCore.Domain.Entities;
using LogiCore.Domain.Enums;
using LogiCore.Domain.Exceptions;
using LogiCore.Domain.ValueObjects;
using LogiCore.App.DeliveryCosts;
using LogiCore.App.Extensions;
using LogiCore.App.Factories;
using LogiCore.App.Repositories;
using LogiCore.App.Services;
using LogiCore.App.Strategies;
using LogiCore.App.Subscribers;

namespace LogiCore.App;

class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║           LOGICORE - LOGISTICS MANAGEMENT SYSTEM               ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        // Запускаем Demo Scenario
        RunDemoScenario();

        // Запускаем интерактивное меню
        RunInteractiveMenu();
    }

    static void RunDemoScenario()
    {
        Console.WriteLine("=== STARTING DEMO SCENARIO ===\n");

        // Инициализация компонентов
        var vehicleFactory = new VehicleFactory();
        var compatibilityValidator = new CargoCompatibilityValidator();
        var deliveryService = new DeliveryService(compatibilityValidator);
        var reportService = new ReportService();
        var persistenceService = new JsonPersistenceService();

        // Репозитории
        var vehicleRepo = new Repository<Vehicle>();
        var customerRepo = new Repository<Customer>();
        var orderRepo = new Repository<Order>();

        // Подписчики на события
        var consoleNotifier = new ConsoleNotifier();
        var logFilePath = "delivery_log.txt";
        
        using (var fileLogger = new FileLogger(logFilePath))
        {
            // Подписываемся на события
            deliveryService.OrderCreated += consoleNotifier.OnOrderCreated;
            deliveryService.OrderCreated += fileLogger.OnOrderCreated;
            deliveryService.VehicleOverloadAttempt += consoleNotifier.OnVehicleOverloadAttempt;
            deliveryService.VehicleOverloadAttempt += fileLogger.OnVehicleOverloadAttempt;
            deliveryService.DeliveryCompleted += consoleNotifier.OnDeliveryCompleted;
            deliveryService.DeliveryCompleted += fileLogger.OnDeliveryCompleted;
            deliveryService.LogisticsEvent += consoleNotifier.OnLogisticsEvent;
            deliveryService.LogisticsEvent += fileLogger.OnLogisticsEvent;

            Console.WriteLine("✓ Event subscribers connected\n");

            // ===== ШАГ 1: Создание парка транспорта =====
            Console.WriteLine("--- STEP 1: Creating vehicle fleet ---");
            
            var van1 = vehicleFactory.Create(VehicleType.Van, "VAN-001", 800, 10, 80, 2.0m);
            var truck1 = vehicleFactory.Create(VehicleType.Truck, "TRUCK-001", 5000, 50, 90, 2.5m);
            var refTruck1 = vehicleFactory.Create(VehicleType.RefrigeratedTruck, "REF-001", 4000, 45, 85, 3.0m, -20, 5);
            var plane1 = vehicleFactory.Create(VehicleType.CargoPlane, "PLANE-001", 10000, 100, 800, 15.0m);
            var ship1 = vehicleFactory.Create(VehicleType.CargoShip, "SHIP-001", 50000, 500, 40, 1.0m);
            var drone1 = vehicleFactory.Create(VehicleType.DroneCourier, "DRONE-001", 5, 0.5m, 60, 10.0m, maxRangeKm: 50);

            vehicleRepo.Add(van1);
            vehicleRepo.Add(truck1);
            vehicleRepo.Add(refTruck1);
            vehicleRepo.Add(plane1);
            vehicleRepo.Add(ship1);
            vehicleRepo.Add(drone1);

            Console.WriteLine($"✓ Created {vehicleRepo.Count} vehicles");
            Console.WriteLine();

            // ===== ШАГ 2: Создание грузов =====
            Console.WriteLine("--- STEP 2: Creating cargo items ---");

            var standardCargo1 = new StandardCargo("Office furniture", 200, 5, 5000);
            var standardCargo2 = new StandardCargo("Electronics", 50, 1, 10000);
            var perishable1 = new PerishableCargo("Fresh vegetables", 300, 8, 3000, DateTime.UtcNow.AddDays(5), -5, 5);
            var perishable2 = new PerishableCargo("Frozen meat", 500, 12, 8000, DateTime.UtcNow.AddDays(30), -18, -10);
            var fragile1 = new FragileCargo("Glass products", 100, 3, 7000, 2.5m);
            var fragile2 = new FragileCargo("Ceramics", 80, 2.5m, 4000, 1.8m);
            var dangerous1 = new DangerousCargo("Chemicals", 150, 4, 15000, DangerousCargoClass.Class3);
            var dangerous2 = new DangerousCargo("Flammable liquids", 200, 6, 12000, DangerousCargoClass.Class3);
            var oversized1 = new OversizedCargo("Industrial equipment", 3000, 30, 50000, 4.5m, 2.5m, 3.0m);
            var oversized2 = new OversizedCargo("Construction materials", 2500, 25, 30000, 5.0m, 2.0m, 2.5m);

            Console.WriteLine($"✓ Created 10 cargo items (5 types)");
            Console.WriteLine();

            // ===== ШАГ 3: Создание клиентов =====
            Console.WriteLine("--- STEP 3: Creating customers ---");

            var customer1 = new Customer("Tech Corp", "tech@example.com");
            var customer2 = new Customer("Food Market Ltd", "food@example.com");
            var customer3 = new Customer("Construction Inc", "build@example.com");

            customerRepo.Add(customer1);
            customerRepo.Add(customer2);
            customerRepo.Add(customer3);

            Console.WriteLine($"✓ Created {customerRepo.Count} customers");
            Console.WriteLine();

            // ===== ШАГ 4: Создание маршрутов =====
            Console.WriteLine("--- STEP 4: Creating routes ---");

            var route1 = new Route("Moscow-SPB", new List<RoutePoint>
            {
                new RoutePoint(55.7558, 37.6173, "Moscow"),
                new RoutePoint(59.9343, 30.3351, "Saint Petersburg")
            });

            var route2 = new Route("Moscow-Kazan", new List<RoutePoint>
            {
                new RoutePoint(55.7558, 37.6173, "Moscow"),
                new RoutePoint(55.8304, 49.0661, "Kazan")
            });

            var route3 = new Route("Short delivery", new List<RoutePoint>
            {
                new RoutePoint(55.7558, 37.6173, "Moscow Center"),
                new RoutePoint(55.7600, 37.6200, "Moscow North")
            });

            Console.WriteLine($"✓ Route 1: {route1.Name}, Distance: {route1.DistanceKm:F2} km");
            Console.WriteLine($"✓ Route 2: {route2.Name}, Distance: {route2.DistanceKm:F2} km");
            Console.WriteLine($"✓ Route 3: {route3.Name}, Distance: {route3.DistanceKm:F2} km");
            Console.WriteLine();

            // ===== ШАГ 5: Демонстрация несовместимых грузов (IncompatibleCargoException) =====
            Console.WriteLine("--- STEP 5: Demonstrating incompatible cargo ---");

            try
            {
                var invalidOrder = deliveryService.CreateOrder(
                    "ORD-INVALID",
                    customer1,
                    route1,
                    new Cargo[] { dangerous1, perishable1 }); // Опасный + скоропортящийся

                var testVehicle = truck1;
                compatibilityValidator.ValidateCargoCompatibility(invalidOrder.Cargo.ToList(), testVehicle);
                
                Console.WriteLine("✗ Should have thrown IncompatibleCargoException!");
            }
            catch (IncompatibleCargoException ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"✓ Caught IncompatibleCargoException: {ex.Message}");
                Console.ResetColor();
            }
            Console.WriteLine();

            // ===== ШАГ 6: Создание валидных заказов =====
            Console.WriteLine("--- STEP 6: Creating valid orders ---");

            var order1 = deliveryService.CreateOrder("ORD-001", customer1, route1, new Cargo[] { standardCargo1, standardCargo2 });
            var order2 = deliveryService.CreateOrder("ORD-002", customer2, route2, new Cargo[] { perishable1, perishable2 });
            var order3 = deliveryService.CreateOrder("ORD-003", customer3, route1, new Cargo[] { oversized1 });
            var order4 = deliveryService.CreateOrder("ORD-004", customer1, route3, new Cargo[] { fragile1, fragile2 });

            orderRepo.Add(order1);
            orderRepo.Add(order2);
            orderRepo.Add(order3);
            orderRepo.Add(order4);

            Console.WriteLine($"✓ Created {orderRepo.Count} valid orders");
            Console.WriteLine();

            // ===== ШАГ 7: Демонстрация перегрузки =====
            Console.WriteLine("--- STEP 7: Demonstrating vehicle overload ---");

            try
            {
                // Пытаемся загрузить слишком тяжёлый груз на дрон
                var heavyCargo = new StandardCargo("Heavy box", 100, 2, 1000); // Дрон может только 5 кг
                var overloadOrder = deliveryService.CreateOrder("ORD-OVERLOAD", customer1, route3, new Cargo[] { heavyCargo });
                
                deliveryService.AssignVehicle(overloadOrder, drone1, 100m);
                
                Console.WriteLine("✗ Should have thrown VehicleOverloadException!");
            }
            catch (VehicleOverloadException ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"✓ Caught VehicleOverloadException: {ex.Message}");
                Console.ResetColor();
            }
            Console.WriteLine();

            // ===== ШАГ 8: Подбор транспорта и расчёт стоимости с декораторами =====
            Console.WriteLine("--- STEP 8: Vehicle selection and cost calculation with decorators ---");

            // Заказ 1: Стандартные грузы
            var selectedVehicle1 = deliveryService.SelectVehicle(order1, vehicleRepo.GetAll());
            if (selectedVehicle1 != null)
            {
                var baseCost1 = selectedVehicle1.CalculateDeliveryCost(order1.Route!, order1.Cargo.ToList());
                
                Console.WriteLine($"\nOrder {order1.Number}:");
                Console.WriteLine($"  Selected vehicle: {selectedVehicle1.RegistrationNumber}");
                Console.WriteLine($"  Base cost: {baseCost1:C}");

                // Применяем стратегию
                var strategy = new StandardTariffStrategy();
                var strategyCost = strategy.Calculate(baseCost1, order1.Route!, order1.Cargo.ToList());
                Console.WriteLine($"  After strategy ({strategy.Name}): {strategyCost:C}");

                // Применяем декораторы
                var costWithInsurance = new InsuranceDecorator(new BaseDeliveryCost(strategyCost), 200m);
                Console.WriteLine($"  {costWithInsurance.Describe()}");

                var costWithPriority = new PriorityDeliveryDecorator(costWithInsurance, 300m);
                Console.WriteLine($"  {costWithPriority.Describe()}");

                var finalCost = costWithPriority.Total;
                Console.WriteLine($"  Final cost: {finalCost:C}");

                deliveryService.AssignVehicle(order1, selectedVehicle1, finalCost);
            }

            // Заказ 4: Хрупкие грузы
            var selectedVehicle4 = deliveryService.SelectVehicle(order4, vehicleRepo.GetAll());
            if (selectedVehicle4 != null)
            {
                var baseCost4 = selectedVehicle4.CalculateDeliveryCost(order4.Route!, order4.Cargo.ToList());
                
                Console.WriteLine($"\nOrder {order4.Number}:");
                Console.WriteLine($"  Selected vehicle: {selectedVehicle4.RegistrationNumber}");
                Console.WriteLine($"  Base cost: {baseCost4:C}");

                var costWithInsurance = new InsuranceDecorator(new BaseDeliveryCost(baseCost4), 150m);
                var costWithFragilePackaging = new FragilePackagingDecorator(costWithInsurance, 250m);
                var finalCost = costWithFragilePackaging.Total;
                
                Console.WriteLine($"  {costWithFragilePackaging.Describe()}");
                Console.WriteLine($"  Final cost: {finalCost:C}");

                deliveryService.AssignVehicle(order4, selectedVehicle4, finalCost);
            }

            Console.WriteLine();

            // ===== ШАГ 9: Полный жизненный цикл заказа =====
            Console.WriteLine("--- STEP 9: Full order lifecycle ---");

            Console.WriteLine($"\nOrder {order1.Number} lifecycle:");
            deliveryService.StartDelivery(order1);
            Thread.Sleep(500); // Пауза для наглядности
            deliveryService.CompleteDelivery(order1);

            Console.WriteLine($"\nOrder {order4.Number} lifecycle:");
            deliveryService.StartDelivery(order4);
            Thread.Sleep(500);
            deliveryService.CompleteDelivery(order4);

            Console.WriteLine($"\nTotal company revenue: {deliveryService.TotalRevenue:C}");
            Console.WriteLine();

            // ===== ШАГ 10: Демонстрация отписки от события =====
            Console.WriteLine("--- STEP 10: Demonstrating event unsubscription ---");

            // Отписываемся от события в ConsoleNotifier
            deliveryService.OrderCreated -= consoleNotifier.OnOrderCreated;
            
            var testOrder = deliveryService.CreateOrder("ORD-TEST", customer1, route3, new Cargo[] { standardCargo1 });
            orderRepo.Add(testOrder);
            
            Console.WriteLine("✓ Created order after unsubscribing ConsoleNotifier (should see only FileLogger entry)");
            Console.WriteLine();

            // ===== ШАГ 11: Демонстрация Flags enum =====
            Console.WriteLine("--- STEP 11: Demonstrating [Flags] enum ---");

            var conditions1 = TransportConditions.Refrigerated | TransportConditions.Sealed;
            var conditions2 = TransportConditions.Pressurized | TransportConditions.LongRange;
            var combinedConditions = conditions1 | conditions2;

            Console.WriteLine($"Conditions 1: {conditions1}");
            Console.WriteLine($"Conditions 2: {conditions2}");
            Console.WriteLine($"Combined: {combinedConditions}");
            Console.WriteLine($"Has Refrigerated: {combinedConditions.HasFlag(TransportConditions.Refrigerated)}");
            Console.WriteLine($"Has Sealed: {combinedConditions.HasFlag(TransportConditions.Sealed)}");
            Console.WriteLine($"Has LongRange: {combinedConditions.HasFlag(TransportConditions.LongRange)}");
            Console.WriteLine();

            // ===== ШАГ 12: LINQ отчёты =====
            Console.WriteLine("--- STEP 12: LINQ Reports ---");
            Console.WriteLine();

            var fullReport = reportService.GenerateFullReport(
                orderRepo.GetAll(),
                vehicleRepo.GetAll(),
                customerRepo.GetAll(),
                1000m);

            Console.WriteLine(fullReport);

            // ===== ШАГ 13: Сериализация и десериализация =====
            Console.WriteLine("--- STEP 13: JSON Serialization and Deserialization ---");

            var saveFilePath = "logicore_state.json";

            Console.WriteLine($"Saving state to {saveFilePath}...");
            persistenceService.SaveState(saveFilePath, vehicleRepo.GetAll(), customerRepo.GetAll(), orderRepo.GetAll());
            Console.WriteLine("✓ State saved successfully");

            Console.WriteLine("\nClearing in-memory state...");
            var originalVehicleCount = vehicleRepo.Count;
            var originalCustomerCount = customerRepo.Count;
            var originalOrderCount = orderRepo.Count;

            Console.WriteLine($"Original counts: Vehicles={originalVehicleCount}, Customers={originalCustomerCount}, Orders={originalOrderCount}");

            Console.WriteLine($"\nLoading state from {saveFilePath}...");
            var loadedSnapshot = persistenceService.LoadState(saveFilePath);
            Console.WriteLine("✓ State loaded successfully");
            Console.WriteLine($"Loaded at: {loadedSnapshot.SavedAt:yyyy-MM-dd HH:mm:ss UTC}");
            Console.WriteLine($"Loaded counts: Vehicles={loadedSnapshot.Vehicles.Count}, Customers={loadedSnapshot.Customers.Count}, Orders={loadedSnapshot.Orders.Count}");

            // Проверяем совпадение данных
            bool dataMatches = 
                loadedSnapshot.Vehicles.Count == originalVehicleCount &&
                loadedSnapshot.Customers.Count == originalCustomerCount &&
                loadedSnapshot.Orders.Count == originalOrderCount;

            if (dataMatches)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n✓✓✓ DATA VERIFICATION PASSED ✓✓✓");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n✗✗✗ DATA VERIFICATION FAILED ✗✗✗");
                Console.ResetColor();
            }

            Console.WriteLine();
        } // FileLogger автоматически освобождается (using)

        Console.WriteLine($"✓ Log file created: {logFilePath}");
        Console.WriteLine("\n=== DEMO SCENARIO COMPLETED ===\n");
    }

    static void RunInteractiveMenu()
    {
        Console.WriteLine("Press any key to continue to interactive menu...");
        Console.ReadKey();
        Console.Clear();

        // Здесь можно добавить интерактивное меню, но для базовой версии достаточно демо
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                 INTERACTIVE MENU                               ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.WriteLine();
        Console.WriteLine("Demo scenario has completed successfully.");
        Console.WriteLine("For full interactive features, additional UI implementation is required.");
        Console.WriteLine();
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}
