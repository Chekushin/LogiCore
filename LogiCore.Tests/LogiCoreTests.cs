using LogiCore.Domain.Entities;
using LogiCore.Domain.Enums;
using LogiCore.Domain.Exceptions;
using LogiCore.Domain.ValueObjects;
using LogiCore.Domain.Events;
using LogiCore.App.Repositories;
using LogiCore.App.DeliveryCosts;
using LogiCore.App.Strategies;
using LogiCore.App.Factories;
using LogiCore.App.Services;
using LogiCore.App.Adapters;

namespace LogiCore.Tests;

public class LogiCoreTests
{
    // ===== ТЕСТЫ ТРАНСПОРТА =====

    [Fact]
    public void Vehicle_ShouldCalculateCostCorrectly_ForEachType()
    {
        // Arrange
        var route = CreateTestRoute(100m);
        var cargo = new List<Cargo> { CreateStandardCargo(100, 5) };

        var van = new Van("VAN-001", 1000, 10, 80, 2.0m);
        var truck = new Truck("TRUCK-001", 5000, 50, 90, 2.5m, 1.15m);
        var refTruck = new RefrigeratedTruck("REF-001", 4000, 45, 85, 3.0m, -20, 5);

        // Act & Assert
        Assert.True(van.CalculateDeliveryCost(route, cargo) > 0);
        Assert.True(truck.CalculateDeliveryCost(route, cargo) > 0);
        Assert.True(refTruck.CalculateDeliveryCost(route, cargo) > 0);
    }

    [Fact]
    public void Vehicle_ShouldCalculateZeroCost_ForZeroDistance()
    {
        // Arrange
        var route = CreateTestRoute(0m);
        var cargo = new List<Cargo> { CreateStandardCargo(100, 5) };
        var van = new Van("VAN-001", 1000, 10, 80, 2.0m);

        // Act
        var cost = van.CalculateDeliveryCost(route, cargo);

        // Assert
        Assert.Equal(0m, cost);
    }

    [Fact]
    public void Vehicle_CanCarry_ShouldReturnFalse_WhenExceedingCapacity()
    {
        // Arrange
        var van = new Van("VAN-001", 100, 10, 80, 2.0m);
        var heavyCargo = CreateStandardCargo(200, 5);

        // Act
        var result = van.CanCarry(heavyCargo);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Vehicle_ShouldImplementEquals_ById()
    {
        // Arrange
        var van1 = new Van("VAN-001", 1000, 10, 80, 2.0m);
        var van2 = new Van("VAN-002", 1000, 10, 80, 2.0m);

        // Act & Assert
        Assert.NotEqual(van1, van2);
        Assert.Equal(van1, van1);
    }

    [Fact]
    public void DroneCourier_IsSealed()
    {
        // Assert
        Assert.True(typeof(DroneCourier).IsSealed);
    }

    [Fact]
    public void CargoPlane_ShouldCalculateCost_WithWeightSurcharge()
    {
        // Arrange
        var route = CreateTestRoute(100m);
        var cargo = new List<Cargo> { CreateStandardCargo(500, 10) };
        var plane = new CargoPlane("PLANE-001", 10000, 100, 800, 15.0m, 5.0m);

        // Act
        var cost = plane.CalculateDeliveryCost(route, cargo);

        // Assert - базовая стоимость (100 * 15) + надбавка за вес (500 * 5)
        Assert.Equal(4000m, cost);
    }

    [Fact]
    public void CargoPlane_CannotCarry_DangerousClass1()
    {
        // Arrange
        var plane = new CargoPlane("PLANE-001", 10000, 100, 800, 15.0m, 5.0m);
        var explosives = new DangerousCargo("Explosives", 100, 5, 1000, DangerousCargoClass.Class1);

        // Act
        var result = plane.CanCarry(explosives);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CargoShip_ShouldAccept_OversizedCargo()
    {
        // Arrange
        var ship = new CargoShip("SHIP-001", 50000, 500, 40, 1.0m, 5000m);
        var oversized = new OversizedCargo("Heavy machinery", 20000, 200, 50000, 15, 8, 5);

        // Act
        var canCarry = ship.CanCarry(oversized);

        // Assert
        Assert.True(canCarry);
    }

    [Fact]
    public void DroneCourier_ShouldNotReach_DistantRoute()
    {
        // Arrange
        var drone = new DroneCourier("DRONE-001", 5, 0.5m, 60, 5.0m, 30);
        var longRoute = CreateTestRoute(50m);

        // Act
        var canReach = drone.CanReach(longRoute);

        // Assert
        Assert.False(canReach);
    }

    [Fact]
    public void DroneCourier_ShouldReach_NearbyRoute()
    {
        // Arrange
        var drone = new DroneCourier("DRONE-001", 5, 0.5m, 60, 5.0m, 30);
        var shortRoute = CreateTestRoute(20m);

        // Act
        var canReach = drone.CanReach(shortRoute);

        // Assert
        Assert.True(canReach);
    }

    [Fact]
    public void Vehicle_ShouldLoadAndUnloadCargo()
    {
        // Arrange
        var van = new Van("VAN-001", 1000, 10, 80, 2.0m);
        var cargo = CreateStandardCargo(100, 2);

        // Act
        van.LoadCargo(cargo);
        Assert.Equal(100m, van.CurrentLoad);
        
        van.UnloadCargo(cargo);

        // Assert
        Assert.Equal(0m, van.CurrentLoad);
    }

    [Fact]
    public void Vehicle_ShouldThrow_WhenLoadingExceedsCapacity()
    {
        // Arrange
        var van = new Van("VAN-001", 100, 10, 80, 2.0m);
        var heavyCargo = CreateStandardCargo(150, 2);

        // Act & Assert
        Assert.Throws<VehicleOverloadException>(() => van.LoadCargo(heavyCargo));
    }

    // ===== ТЕСТЫ СОВМЕСТИМОСТИ ГРУЗОВ =====

    [Fact]
    public void CargoCompatibility_ShouldThrow_WhenDangerousAndPerishableTogether()
    {
        // Arrange
        var validator = new CargoCompatibilityValidator();
        var vehicle = new Truck("TRUCK-001", 5000, 50, 90, 2.5m);
        var dangerous = new DangerousCargo("Chemicals", 100, 5, 1000, DangerousCargoClass.Class3);
        var perishable = new PerishableCargo("Food", 100, 5, 1000, DateTime.UtcNow.AddDays(5), -5, 5);
        var cargo = new List<Cargo> { dangerous, perishable };

        // Act & Assert
        Assert.Throws<IncompatibleCargoException>(() =>
            validator.ValidateCargoCompatibility(cargo, vehicle));
    }

    [Fact]
    public void CargoCompatibility_ShouldThrow_WhenPerishableWithoutRefrigeration()
    {
        // Arrange
        var validator = new CargoCompatibilityValidator();
        var vehicle = new Truck("TRUCK-001", 5000, 50, 90, 2.5m);
        var perishable = new PerishableCargo("Food", 100, 5, 1000, DateTime.UtcNow.AddDays(5), -5, 5);
        var cargo = new List<Cargo> { perishable };

        // Act & Assert
        Assert.Throws<IncompatibleCargoException>(() =>
            validator.ValidateCargoCompatibility(cargo, vehicle));
    }

    [Fact]
    public void CargoCompatibility_ShouldThrow_WhenExceedingWeight()
    {
        // Arrange
        var validator = new CargoCompatibilityValidator();
        var vehicle = new Van("VAN-001", 100, 10, 80, 2.0m);
        var heavyCargo = CreateStandardCargo(200, 5);
        var cargo = new List<Cargo> { heavyCargo };

        // Act & Assert
        Assert.Throws<VehicleOverloadException>(() =>
            validator.ValidateCargoCompatibility(cargo, vehicle));
    }

    [Fact]
    public void CargoCompatibility_ShouldThrow_WhenExpiredPerishable()
    {
        // Arrange
        var validator = new CargoCompatibilityValidator();
        var vehicle = new RefrigeratedTruck("REF-001", 4000, 45, 85, 3.0m, -20, 5);
        var expired = new PerishableCargo("Expired food", 100, 5, 1000, DateTime.UtcNow.AddDays(-1), -5, 5);
        var cargo = new List<Cargo> { expired };

        // Act & Assert
        Assert.Throws<CargoValidationException>(() =>
            validator.ValidateCargoCompatibility(cargo, vehicle));
    }

    [Fact]
    public void CargoCompatibility_ShouldPass_WithRefrigeratedTruckAndPerishable()
    {
        // Arrange
        var validator = new CargoCompatibilityValidator();
        var vehicle = new RefrigeratedTruck("REF-001", 4000, 45, 85, 3.0m, -20, 5);
        var perishable = new PerishableCargo("Food", 100, 5, 1000, DateTime.UtcNow.AddDays(5), -5, 5);
        var cargo = new List<Cargo> { perishable };

        // Act & Assert - не должно быть исключений
        validator.ValidateCargoCompatibility(cargo, vehicle);
    }

    [Fact]
    public void FragileCargo_ShouldRequire_SpecialHandling()
    {
        // Arrange
        var fragile = new FragileCargo("Glass items", 50, 2, 500, 3);

        // Act & Assert
        Assert.Equal(3, fragile.FragilityLevel);
        Assert.True(fragile.WeightKg > 0);
    }

    // ===== ТЕСТЫ МАШИНЫ СОСТОЯНИЙ ЗАКАЗА =====

    [Fact]
    public void Order_ShouldTransition_FromCreatedToAssigned()
    {
        // Arrange
        var customer = new Customer("Test Customer", "test@example.com");
        var order = new Order("ORD-001", customer);
        var vehicle = new Van("VAN-001", 1000, 10, 80, 2.0m);

        // Act
        order.Assign(vehicle, 1000m);

        // Assert
        Assert.Equal(OrderStatus.Assigned, order.Status);
        Assert.Equal(vehicle, order.AssignedVehicle);
        Assert.Equal(1000m, order.TotalCost);
    }

    [Fact]
    public void Order_ShouldTransition_FromAssignedToInTransit()
    {
        // Arrange
        var customer = new Customer("Test Customer", "test@example.com");
        var order = new Order("ORD-001", customer);
        var vehicle = new Van("VAN-001", 1000, 10, 80, 2.0m);
        order.Assign(vehicle, 1000m);

        // Act
        order.StartDelivery();

        // Assert
        Assert.Equal(OrderStatus.InTransit, order.Status);
    }

    [Fact]
    public void Order_ShouldTransition_FromInTransitToDelivered()
    {
        // Arrange
        var customer = new Customer("Test Customer", "test@example.com");
        var order = new Order("ORD-001", customer);
        var vehicle = new Van("VAN-001", 1000, 10, 80, 2.0m);
        order.Assign(vehicle, 1000m);
        order.StartDelivery();

        // Act
        order.Complete();

        // Assert
        Assert.Equal(OrderStatus.Delivered, order.Status);
    }

    [Fact]
    public void Order_ShouldThrow_OnInvalidTransition()
    {
        // Arrange
        var customer = new Customer("Test Customer", "test@example.com");
        var order = new Order("ORD-001", customer);

        // Act & Assert
        Assert.Throws<InvalidOrderStateException>(() => order.Complete());
    }

    [Fact]
    public void Order_ShouldAllowCancel_FromCreatedOrAssigned()
    {
        // Arrange
        var customer = new Customer("Test Customer", "test@example.com");
        var order = new Order("ORD-001", customer);

        // Act
        order.Cancel();

        // Assert
        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public void Order_ShouldRaiseEvent_OnStatusChange()
    {
        // Arrange
        var customer = new Customer("Test Customer", "test@example.com");
        var order = new Order("ORD-001", customer);
        var vehicle = new Van("VAN-001", 1000, 10, 80, 2.0m);
        var eventRaised = false;
        OrderStatus? oldStatus = null;
        OrderStatus? newStatus = null;

        order.StatusChanged += (sender, e) =>
        {
            eventRaised = true;
            oldStatus = e.OldStatus;
            newStatus = e.NewStatus;
        };

        // Act
        order.Assign(vehicle, 1000m);

        // Assert
        Assert.True(eventRaised);
        Assert.Equal(OrderStatus.Created, oldStatus);
        Assert.Equal(OrderStatus.Assigned, newStatus);
    }

    [Fact]
    public void Order_ShouldAddCargo_OnlyInCreatedStatus()
    {
        // Arrange
        var customer = new Customer("Test Customer", "test@example.com");
        var order = new Order("ORD-001", customer);
        var cargo = CreateStandardCargo(100, 5);
        var vehicle = new Van("VAN-001", 1000, 10, 80, 2.0m);

        // Act
        order.AddCargo(cargo);
        Assert.Single(order.Cargo);

        order.Assign(vehicle, 1000m);

        // Assert
        Assert.Throws<InvalidOrderStateException>(() => 
            order.AddCargo(CreateStandardCargo(50, 2)));
    }

    [Fact]
    public void Order_ShouldNotAllowCancel_AfterInTransit()
    {
        // Arrange
        var customer = new Customer("Test Customer", "test@example.com");
        var order = new Order("ORD-001", customer);
        var vehicle = new Van("VAN-001", 1000, 10, 80, 2.0m);
        order.Assign(vehicle, 1000m);
        order.StartDelivery();

        // Act & Assert
        Assert.Throws<InvalidOrderStateException>(() => order.Cancel());
    }

    // ===== ТЕСТЫ REPOSITORY =====

    [Fact]
    public void Repository_ShouldAddAndRetrieve()
    {
        // Arrange
        var repo = new Repository<Vehicle>();
        var van = new Van("VAN-001", 1000, 10, 80, 2.0m);

        // Act
        repo.Add(van);
        var retrieved = repo[van.Id];

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(van.Id, retrieved.Id);
    }

    [Fact]
    public void Repository_ShouldRemove()
    {
        // Arrange
        var repo = new Repository<Vehicle>();
        var van = new Van("VAN-001", 1000, 10, 80, 2.0m);
        repo.Add(van);

        // Act
        var removed = repo.Remove(van.Id);
        var retrieved = repo[van.Id];

        // Assert
        Assert.True(removed);
        Assert.Null(retrieved);
    }

    [Fact]
    public void Repository_FindAll_ShouldFilterByPredicate()
    {
        // Arrange
        var repo = new Repository<Vehicle>();
        repo.Add(new Van("VAN-001", 1000, 10, 80, 2.0m));
        repo.Add(new Truck("TRUCK-001", 5000, 50, 90, 2.5m));
        repo.Add(new Van("VAN-002", 1000, 10, 80, 2.0m));

        // Act
        var vans = repo.FindAll(v => v is Van).ToList();

        // Assert
        Assert.Equal(2, vans.Count);
    }

    [Fact]
    public void Repository_ShouldSupportIteration()
    {
        // Arrange
        var repo = new Repository<Vehicle>();
        repo.Add(new Van("VAN-001", 1000, 10, 80, 2.0m));
        repo.Add(new Truck("TRUCK-001", 5000, 50, 90, 2.5m));

        // Act
        var count = 0;
        foreach (var vehicle in repo)
        {
            count++;
        }

        // Assert
        Assert.Equal(2, count);
    }

    [Fact]
    public void Repository_Indexer_ShouldReturnNull_ForNonExistentId()
    {
        // Arrange
        var repo = new Repository<Vehicle>();

        // Act
        var result = repo[Guid.NewGuid()];

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Repository_ShouldUpdate_ExistingEntity()
    {
        // Arrange
        var repo = new Repository<Vehicle>();
        var van = new Van("VAN-001", 1000, 10, 80, 2.0m);
        repo.Add(van);

        // Act
        van.SetState(VehicleState.InTransit);
        var updated = repo.Update(van);
        var retrieved = repo[van.Id];

        // Assert
        Assert.True(updated);
        Assert.Equal(VehicleState.InTransit, retrieved?.State);
    }

    [Fact]
    public void Repository_ShouldReturnAll_Entities()
    {
        // Arrange
        var repo = new Repository<Vehicle>();
        repo.Add(new Van("VAN-001", 1000, 10, 80, 2.0m));
        repo.Add(new Truck("TRUCK-001", 5000, 50, 90, 2.5m));
        repo.Add(new Van("VAN-002", 1000, 10, 80, 2.0m));

        // Act
        var all = repo.GetAll().ToList();

        // Assert
        Assert.Equal(3, all.Count);
    }

    // ===== ТЕСТЫ ДЕКОРАТОРОВ =====

    [Fact]
    public void Decorator_Insurance_ShouldAddCost()
    {
        // Arrange
        var baseCost = new BaseDeliveryCost(1000m);
        var withInsurance = new InsuranceDecorator(baseCost, 200m);

        // Act
        var total = withInsurance.Total;

        // Assert
        Assert.Equal(1200m, total);
    }

    [Fact]
    public void Decorator_Priority_ShouldAddCost()
    {
        // Arrange
        var baseCost = new BaseDeliveryCost(1000m);
        var withPriority = new PriorityDeliveryDecorator(baseCost, 300m);

        // Act
        var total = withPriority.Total;

        // Assert
        Assert.Equal(1300m, total);
    }

    [Fact]
    public void Decorator_ShouldChain()
    {
        // Arrange
        var baseCost = new BaseDeliveryCost(1000m);
        var withInsurance = new InsuranceDecorator(baseCost, 200m);
        var withPriority = new PriorityDeliveryDecorator(withInsurance, 300m);
        var withPackaging = new FragilePackagingDecorator(withPriority, 150m);

        // Act
        var total = withPackaging.Total;

        // Assert
        Assert.Equal(1650m, total);
    }

    [Fact]
    public void Decorator_OrderMatters()
    {
        // Arrange
        var baseCost1 = new BaseDeliveryCost(1000m);
        var chain1 = new PriorityDeliveryDecorator(
            new InsuranceDecorator(baseCost1, 200m), 300m);

        var baseCost2 = new BaseDeliveryCost(1000m);
        var chain2 = new InsuranceDecorator(
            new PriorityDeliveryDecorator(baseCost2, 300m), 200m);

        // Act & Assert
        Assert.Equal(chain1.Total, chain2.Total);
        Assert.NotEqual(chain1.Describe(), chain2.Describe());
    }

    [Fact]
    public void Decorator_FragilePackaging_ShouldDescribeCorrectly()
    {
        // Arrange
        var baseCost = new BaseDeliveryCost(1000m);
        var withPackaging = new FragilePackagingDecorator(baseCost, 150m);

        // Act
        var description = withPackaging.Describe();

        // Assert
        Assert.Contains("Fragile", description);
        Assert.Contains("150", description);
    }

    [Fact]
    public void Decorator_ShouldStack_MultipleOfSameType()
    {
        // Arrange
        var baseCost = new BaseDeliveryCost(1000m);
        var insurance1 = new InsuranceDecorator(baseCost, 100m);
        var insurance2 = new InsuranceDecorator(insurance1, 150m);

        // Act
        var total = insurance2.Total;

        // Assert
        Assert.Equal(1250m, total);
    }

    // ===== ТЕСТЫ ФАБРИКИ ТРАНСПОРТА =====

    [Fact]
    public void VehicleFactory_ShouldCreate_Van()
    {
        // Arrange
        var factory = new VehicleFactory();

        // Act
        var vehicle = factory.Create(VehicleType.Van, "VAN-001", 1000, 10, 80, 2.0m);

        // Assert
        Assert.IsType<Van>(vehicle);
        Assert.Equal("VAN-001", vehicle.RegistrationNumber);
    }

    [Fact]
    public void VehicleFactory_ShouldCreate_AllVehicleTypes()
    {
        // Arrange
        var factory = new VehicleFactory();

        // Act & Assert
        Assert.IsType<Van>(factory.Create(VehicleType.Van, "VAN-001", 1000, 10, 80, 2.0m));
        Assert.IsType<Truck>(factory.Create(VehicleType.Truck, "TRUCK-001", 5000, 50, 90, 2.5m));
        Assert.IsType<RefrigeratedTruck>(factory.Create(VehicleType.RefrigeratedTruck, "REF-001", 4000, 45, 85, 3.0m, -20, 5));
        Assert.IsType<CargoPlane>(factory.Create(VehicleType.CargoPlane, "PLANE-001", 10000, 100, 800, 15.0m));
        Assert.IsType<CargoShip>(factory.Create(VehicleType.CargoShip, "SHIP-001", 50000, 500, 40, 1.0m));
        Assert.IsType<DroneCourier>(factory.Create(VehicleType.DroneCourier, "DRONE-001", 5, 0.5m, 60, 5.0m));
    }

    // ===== ТЕСТЫ DELIVERY SERVICE =====

    [Fact]
    public void DeliveryService_ShouldCreateOrder()
    {
        // Arrange
        var validator = new CargoCompatibilityValidator();
        var service = new DeliveryService(validator);
        var customer = new Customer("Test Customer", "test@example.com");
        var route = CreateTestRoute(100m);
        var cargo = new List<Cargo> { CreateStandardCargo(100, 5) };

        // Act
        var order = service.CreateOrder("ORD-001", customer, route, cargo);

        // Assert
        Assert.NotNull(order);
        Assert.Equal("ORD-001", order.Number);
        Assert.Single(order.Cargo);
        Assert.Equal(OrderStatus.Created, order.Status);
    }

    [Fact]
    public void DeliveryService_ShouldSelectBestVehicle()
    {
        // Arrange
        var validator = new CargoCompatibilityValidator();
        var service = new DeliveryService(validator);
        var customer = new Customer("Test Customer", "test@example.com");
        var route = CreateTestRoute(100m);
        var cargo = new List<Cargo> { CreateStandardCargo(100, 5) };
        var order = service.CreateOrder("ORD-001", customer, route, cargo);

        var vehicles = new List<Vehicle>
        {
            new Van("VAN-001", 1000, 10, 80, 2.0m),
            new Truck("TRUCK-001", 5000, 50, 90, 5.0m)
        };

        // Act
        var selected = service.SelectVehicle(order, vehicles);

        // Assert
        Assert.NotNull(selected);
        Assert.IsType<Van>(selected);
    }

    [Fact]
    public void DeliveryService_ShouldAssignVehicle_AndUpdateStatus()
    {
        // Arrange
        var validator = new CargoCompatibilityValidator();
        var service = new DeliveryService(validator);
        var customer = new Customer("Test Customer", "test@example.com");
        var route = CreateTestRoute(100m);
        var cargo = new List<Cargo> { CreateStandardCargo(100, 5) };
        var order = service.CreateOrder("ORD-001", customer, route, cargo);
        var vehicle = new Van("VAN-001", 1000, 10, 80, 2.0m);

        // Act
        service.AssignVehicle(order, vehicle, 500m);

        // Assert
        Assert.Equal(OrderStatus.Assigned, order.Status);
        Assert.Equal(vehicle, order.AssignedVehicle);
        Assert.Equal(VehicleState.InTransit, vehicle.State);
    }

    [Fact]
    public void DeliveryService_ShouldCompleteDelivery_AndUpdateRevenue()
    {
        // Arrange
        var validator = new CargoCompatibilityValidator();
        var service = new DeliveryService(validator);
        var customer = new Customer("Test Customer", "test@example.com");
        var route = CreateTestRoute(100m);
        var cargo = new List<Cargo> { CreateStandardCargo(100, 5) };
        var order = service.CreateOrder("ORD-001", customer, route, cargo);
        var vehicle = new Van("VAN-001", 1000, 10, 80, 2.0m);

        service.AssignVehicle(order, vehicle, 500m);
        service.StartDelivery(order);

        // Act
        service.CompleteDelivery(order);

        // Assert
        Assert.Equal(OrderStatus.Delivered, order.Status);
        Assert.Equal(VehicleState.Free, vehicle.State);
        Assert.Equal(500m, service.TotalRevenue);
    }

    [Fact]
    public void DeliveryService_ShouldRaiseEvents()
    {
        // Arrange
        var validator = new CargoCompatibilityValidator();
        var service = new DeliveryService(validator);
        var customer = new Customer("Test Customer", "test@example.com");
        var route = CreateTestRoute(100m);
        var cargo = new List<Cargo> { CreateStandardCargo(100, 5) };
        
        var orderCreatedRaised = false;
        var deliveryCompletedRaised = false;

        service.OrderCreated += (sender, e) => orderCreatedRaised = true;
        service.DeliveryCompleted += (sender, e) => deliveryCompletedRaised = true;

        // Act
        var order = service.CreateOrder("ORD-001", customer, route, cargo);
        var vehicle = new Van("VAN-001", 1000, 10, 80, 2.0m);
        service.AssignVehicle(order, vehicle, 500m);
        service.StartDelivery(order);
        service.CompleteDelivery(order);

        // Assert
        Assert.True(orderCreatedRaised);
        Assert.True(deliveryCompletedRaised);
    }

    [Fact]
    public void DeliveryService_ShouldNotSelectVehicle_WhenAllBusy()
    {
        // Arrange
        var validator = new CargoCompatibilityValidator();
        var service = new DeliveryService(validator);
        var customer = new Customer("Test Customer", "test@example.com");
        var route = CreateTestRoute(100m);
        var cargo = new List<Cargo> { CreateStandardCargo(100, 5) };
        var order = service.CreateOrder("ORD-001", customer, route, cargo);

        var vehicle = new Van("VAN-001", 1000, 10, 80, 2.0m);
        vehicle.SetState(VehicleState.InTransit);
        var vehicles = new List<Vehicle> { vehicle };

        // Act
        var selected = service.SelectVehicle(order, vehicles);

        // Assert
        Assert.Null(selected);
    }

    // ===== ТЕСТЫ ТАРИФНЫХ СТРАТЕГИЙ =====

    [Fact]
    public void TariffStrategyRegistry_ShouldRegisterAndRetrieve()
    {
        // Arrange
        var registry = TariffStrategyRegistry.Instance;
        var strategy = new StandardTariffStrategy();

        // Act
        registry.Register(strategy);
        var retrieved = registry.Get(strategy.Name);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(strategy.Name, retrieved.Name);
    }

    [Fact]
    public void TariffStrategyRegistry_ShouldReturnNull_ForUnknownStrategy()
    {
        // Arrange
        var registry = TariffStrategyRegistry.Instance;

        // Act
        var retrieved = registry.Get("NonExistentStrategy_" + Guid.NewGuid());

        // Assert
        Assert.Null(retrieved);
    }

    // ===== ТЕСТЫ АДАПТЕРА =====

    [Fact]
    public void ExternalDeliveryPriceAdapter_ShouldAdaptCorrectly()
    {
        // Arrange
        var externalPrice = new ExternalDeliveryPrice { Amount = 1500m, Currency = "RUB" };
        var adapter = new ExternalDeliveryPriceAdapter(externalPrice);

        // Act
        var total = adapter.Total;
        var description = adapter.Describe();

        // Assert
        Assert.Equal(1500m, total);
        Assert.Contains("External", description);
    }

    // ===== ТЕСТЫ СЕРИАЛИЗАЦИИ =====

    [Fact]
    public void JsonPersistence_ShouldSaveAndLoad()
    {
        // Arrange
        var service = new JsonPersistenceService();
        var vehicles = new List<Vehicle>
        {
            new Van("VAN-001", 1000, 10, 80, 2.0m),
            new Truck("TRUCK-001", 5000, 50, 90, 2.5m)
        };
        var customers = new List<Customer>
        {
            new Customer("Customer 1", "c1@test.com")
        };
        var orders = new List<Order>();

        var tempFile = Path.GetTempFileName();

        try
        {
            // Act
            service.SaveState(tempFile, vehicles, customers, orders);
            var loaded = service.LoadState(tempFile);

            // Assert
            Assert.Equal(2, loaded.Vehicles.Count);
            Assert.Equal(1, loaded.Customers.Count);
            Assert.Equal(0, loaded.Orders.Count);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public void JsonPersistence_ShouldThrow_ForMissingFile()
    {
        // Arrange
        var service = new JsonPersistenceService();

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() =>
            service.LoadState("nonexistent_file.json"));
    }

    // ===== ТЕСТЫ VALUE OBJECTS =====

    [Fact]
    public void RoutePoint_ShouldCalculateDistance()
    {
        // Arrange
        var point1 = new RoutePoint(55.7558, 37.6173, "Moscow");
        var point2 = new RoutePoint(59.9343, 30.3351, "Saint Petersburg");

        // Act
        var distance = point1.DistanceTo(point2);

        // Assert
        Assert.True(distance > 600);
        Assert.True(distance < 700);
    }

    [Fact]
    public void Route_ShouldCalculateTotalDistance()
    {
        // Arrange
        var route = CreateTestRoute(100m);

        // Act
        var distance = route.DistanceKm;

        // Assert
        Assert.True(distance > 99 && distance < 101);
    }

    // ===== ТЕСТЫ CUSTOMER =====

    [Fact]
    public void Customer_ShouldAddOrder()
    {
        // Arrange
        var customer = new Customer("Test Customer", "test@example.com");
        var order = new Order("ORD-001", customer);

        // Act
        customer.AddOrder(order);

        // Assert
        Assert.Single(customer.Orders);
        Assert.Contains(order, customer.Orders);
    }

    [Fact]
    public void Customer_ShouldCalculateTotalSpent()
    {
        // Arrange
        var customer = new Customer("Test Customer", "test@example.com");
        var order1 = new Order("ORD-001", customer);
        var order2 = new Order("ORD-002", customer);
        var vehicle = new Van("VAN-001", 1000, 10, 80, 2.0m);

        order1.Assign(vehicle, 500m);
        order2.Assign(vehicle, 300m);
        customer.AddOrder(order1);
        customer.AddOrder(order2);

        // Act
        var totalSpent = customer.TotalSpent;

        // Assert
        Assert.Equal(800m, totalSpent);
    }

    // ===== ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ =====

    private Route CreateTestRoute(decimal distanceKm)
    {
        var point1 = new RoutePoint(55.7558, 37.6173, "Point A");
        var point2 = new RoutePoint(55.7558 + (double)distanceKm / 111.0, 37.6173, "Point B");
        return new Route("Test Route", new List<RoutePoint> { point1, point2 });
    }

    private StandardCargo CreateStandardCargo(decimal weightKg, decimal volumeM3)
    {
        return new StandardCargo("Test Cargo", weightKg, volumeM3, 1000m);
    }
}
