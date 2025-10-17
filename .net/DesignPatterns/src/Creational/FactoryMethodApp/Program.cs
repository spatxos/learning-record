// See https://aka.ms/new-console-template for more information
using FactoryMethodApp;

// 使用工厂方法创建对象
VehicleFactory carFactory = new CarFactory();
IVehicle car = carFactory.CreateVehicle();
car.Drive();

VehicleFactory bikeFactory = new BikeFactory();
IVehicle bike = bikeFactory.CreateVehicle();
bike.Drive();


Console.WriteLine("Hello, World!");
