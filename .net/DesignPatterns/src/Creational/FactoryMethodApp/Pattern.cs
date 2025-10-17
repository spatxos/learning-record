using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryMethodApp
{

//例子场景：我们要生产不同类型的“产品”（比如：Car、Bike），但不希望在主程序中直接 new 具体的对象，而是通过“工厂”来统一创建。

//IVehicle 是抽象产品，定义了产品共有的方法。

//Car、Bike 是具体产品。

//VehicleFactory 是抽象工厂，声明了 CreateVehicle 方法。

//CarFactory、BikeFactory 是具体工厂，负责创建对应的具体产品。

//Program 是客户端，只依赖于抽象工厂和抽象产品，不依赖具体实现。

//优点：

//解耦了“对象的创建”和“使用”。

//新增产品时，只需新增一个具体工厂和产品类，不影响已有代码。

    // 1. 抽象产品
    public interface IVehicle
    {
        void Drive();
    }

    // 2. 具体产品 - 汽车
    public class Car : IVehicle
    {
        public void Drive()
        {
            Console.WriteLine("汽车正在行驶！");
        }
    }

    // 3. 具体产品 - 自行车
    public class Bike : IVehicle
    {
        public void Drive()
        {
            Console.WriteLine("自行车正在行驶！");
        }
    }

    // 4. 抽象工厂
    public abstract class VehicleFactory
    {
        public abstract IVehicle CreateVehicle();
    }

    // 5. 具体工厂 - 汽车工厂
    public class CarFactory : VehicleFactory
    {
        public override IVehicle CreateVehicle()
        {
            return new Car();
        }
    }

    // 6. 具体工厂 - 自行车工厂
    public class BikeFactory : VehicleFactory
    {
        public override IVehicle CreateVehicle()
        {
            return new Bike();
        }
    }

}
