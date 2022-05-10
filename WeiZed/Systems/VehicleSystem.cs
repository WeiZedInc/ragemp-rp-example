using GTANetworkAPI;
using WeiZed.MySQL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;

namespace WeiZed.Systems
{
    static class VehicleSystem
    {
        public static Dictionary<string, Vehicle> PersonalVehiclesDictionary = new Dictionary<string, Vehicle>();  // Exmpl: <vehicleNumberPlate, vehicleInstance>

        public static List<string> ListOfAllNumberPlates = new List<string>();

        public static void LoadAllNumberPlatesFromBD()
        {
            try
            {
                DataTable dataTable = MySQLStatic.QueryReadTable($"SELECT `numberPlate` FROM `personal_vehicles`");
                if (dataTable != null && dataTable.Rows.Count != 0)
                {
                    string numberPlate;
                    foreach (DataRow Row in dataTable.Rows)
                    {
                        numberPlate = Convert.ToString(Row["numberPlate"]);
                        ListOfAllNumberPlates.Add(numberPlate);
                    }
                }
                Console.WriteLine("Номера машин подгружены");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static string GenerateVehicleNumber()
        {
            Random random = new Random();
            string number;
            do
            {
                number = "";
                number += (char)random.Next(0x0041, 0x005A);
                for (int i = 0; i < 3; i++)
                    number += (char)random.Next(0x0030, 0x0039);
                number += (char)random.Next(0x0041, 0x005A);

            } while (ListOfAllNumberPlates.Contains(number));
            return number;
        }

        public static void CreatePesonalVehicle(Player buyer, string carModelName, int color1, int color2, Vector3 position, Vector3 rotation, string ownerSocialName, int garage_ID = 0, int fuel = 100, int price = 100)
        {
            VehicleHash vehHash = (VehicleHash)NAPI.Util.GetHashKey(carModelName);
            Vehicle vehicle = NAPI.Vehicle.CreateVehicle(vehHash, position, rotation, color1, color2);
            vehicle.SetData("CAR_ACCESS", "PERSONAL_VEHICLE");
            vehicle.SetData("OWNER", buyer.SocialClubName);
            vehicle.EngineStatus = true;
            vehicle.NumberPlate = VehicleSystem.GenerateVehicleNumber();
            buyer.SetIntoVehicle(vehicle, 0);
            VehicleSystem.PersonalVehiclesDictionary.Add(vehicle.NumberPlate, vehicle);
            string tune = null;
            string itemsInside = null;
            MySQLStatic.Query($"INSERT INTO `personal_vehicles` (`id`, `numberPlate`, `carModelName`, `color1`, `color2`, `ownerSocialName`, `fuel`, `price`, `tune`," +
                              $"`itemsInside`, `position`, `rotation`,`garage_ID`)" +
                    $" VALUES(NULL, " +
                    $"'{vehicle.NumberPlate}', " +
                    $"'{carModelName}', " +
                    $"'{color1}', " +
                    $"'{color2}'," +
                    $"'{ownerSocialName}'," +
                    $"'{fuel}'," +
                    $"'{price}'," +
                    $"'{JsonConvert.SerializeObject(tune)}'," +
                    $"'{JsonConvert.SerializeObject(itemsInside)}'," +
                    $"'{JsonConvert.SerializeObject(position)}'," +
                    $"'{JsonConvert.SerializeObject(rotation)}'," +
                    $"'{garage_ID}')");
            NAPI.Chat.SendChatMessageToAll($"В базу добавлена машина с номером {vehicle.NumberPlate}");
        }
        public static void CreateAdminVehicle(Player sender, string carModelName, int color1, int color2, string numberPlate)
        {
            VehicleHash vehHash = (VehicleHash)NAPI.Util.GetHashKey(carModelName);
            Vehicle vehicle = NAPI.Vehicle.CreateVehicle(vehHash, sender.Position, sender.Rotation, color1, color2, dimension: sender.Dimension);
            vehicle.SetData("CAR_ACCESS", "ADMIN");
            vehicle.SetData("СREATED_BY", sender.SocialClubName);
            vehicle.EngineStatus = true;
            sender.SetIntoVehicle(vehicle, 0);
            sender.SendChatMessage($"Вы создали тс с ID: { Convert.ToString(vehicle.Id)}");
            if (numberPlate == "")
            {
                vehicle.NumberPlate = "ADMIN" + vehicle.Id;
            }
            else
            {
                vehicle.NumberPlate = numberPlate;
            }
        }

        public static Vehicle CreateJobVehicle(string nameOfJob, Vector3 position, Vector3 rotation, string carModelName, int color1, int color2)
        {
            VehicleHash vehHash = (VehicleHash)NAPI.Util.GetHashKey(carModelName);
            Vehicle vehicle = NAPI.Vehicle.CreateVehicle(vehHash, position, rotation, color1, color2, dimension: 0);
            vehicle.SetData("CAR_ACCESS", "JOB");
            vehicle.EngineStatus = true;
            vehicle.NumberPlate = nameOfJob + vehicle.Id;
            return vehicle;
        }

        public static Vehicle CreatePesonalVehicleFromBD(string numberPlate, string carModelName, int color1, int color2, string ownerSocialName,
                                                         int fuel, int price, string tune, string itemsInside, Vector3 pos, Vector3 rot)
        {
            VehicleHash vehHash = (VehicleHash)NAPI.Util.GetHashKey(carModelName);
            Vehicle vehicle = NAPI.Vehicle.CreateVehicle(vehHash, pos, rot, color1, color2);
            vehicle.SetData("CAR_ACCESS", "PERSONAL_VEHICLE");
            vehicle.SetData("CAR_OWNER", $"{ownerSocialName}");
            vehicle.SetData<int>("CAR_PRICE", price);
            vehicle.SetData<int>("CAR_FUEL", fuel);
            vehicle.SetData("CAR_TUNE", $"{tune}");
            vehicle.SetData("CAR_ITEMS", $"{itemsInside}");
            vehicle.NumberPlate = numberPlate;
            vehicle.EngineStatus = false;
            NAPI.Chat.SendChatMessageToAll($"Из базы создана машина с номером {vehicle.NumberPlate}");
            return vehicle;
        }

        public static void SaveVehicleToBD(Player player)
        {
            foreach (Vehicle car in NAPI.Pools.GetAllVehicles())
            {
                if (car.GetData<string>("CAR_OWNER") == player.SocialClubName)
                {
                    MySQLStatic.Query($"UPDATE `personal_vehicles` SET `position`='{JsonConvert.SerializeObject(car.Position)}' WHERE `carModelName`='{car.DisplayName}'");
                    MySQLStatic.Query($"UPDATE `personal_vehicles` SET `rotation`='{JsonConvert.SerializeObject(car.Rotation)}' WHERE `carModelName`='{car.DisplayName}'");
                    MySQLStatic.Query($"UPDATE `personal_vehicles` SET `fuel`='{car.GetData<int>("CAR_FUEL")}' WHERE `carModelName`='{car.DisplayName}'");
                    MySQLStatic.Query($"UPDATE `personal_vehicles` SET `tune`='{JsonConvert.SerializeObject(car.GetData<string>("CAR_TUNE"))}' WHERE `carModelName`='{car.DisplayName}'");
                    MySQLStatic.Query($"UPDATE `personal_vehicles` SET `itemsInside`='{JsonConvert.SerializeObject(car.GetData<string>("CAR_ITEMS"))}' WHERE `carModelName`='{car.DisplayName}'");
                    VehicleSystem.PersonalVehiclesDictionary.Remove(car.NumberPlate);
                    car.Delete();
                }
            }
        }


        public static void Tuning(Vehicle vehicle)
        {
            NAPI.Util.ConsoleOutput("CustomTires " + vehicle.CustomTires.ToString());
            NAPI.Util.ConsoleOutput("DashboardColor " + vehicle.DashboardColor.ToString());
            NAPI.Util.ConsoleOutput("EngineStatus " + vehicle.EngineStatus.ToString());
            NAPI.Util.ConsoleOutput("Heading " + vehicle.Heading.ToString());
            NAPI.Util.ConsoleOutput("Health " + vehicle.Health.ToString());
            NAPI.Util.ConsoleOutput("locked " + vehicle.Locked.ToString());
            NAPI.Util.ConsoleOutput("MaxAcceleration " + vehicle.MaxAcceleration.ToString());
            NAPI.Util.ConsoleOutput("MaxBraking " + vehicle.MaxBraking.ToString());
            NAPI.Util.ConsoleOutput("MaxSpeed " + vehicle.MaxSpeed.ToString());
            NAPI.Util.ConsoleOutput("MaxTraction " + vehicle.MaxTraction.ToString());
            NAPI.Util.ConsoleOutput("Neons " + vehicle.Neons.ToString());
            NAPI.Util.ConsoleOutput("NeonColor " + vehicle.NeonColor.ToString());
            NAPI.Util.ConsoleOutput("NumberPlate " + vehicle.NumberPlate.ToString());
            NAPI.Util.ConsoleOutput("PrimaryColor " + vehicle.PrimaryColor.ToString());
            NAPI.Util.ConsoleOutput("SecondaryColor " + vehicle.SecondaryColor.ToString());
            NAPI.Util.ConsoleOutput("Siren " + vehicle.Siren.ToString());
            NAPI.Util.ConsoleOutput("SpecialLight " + vehicle.SpecialLight.ToString());
            NAPI.Util.ConsoleOutput("WheelColor " + vehicle.WheelColor.ToString());
            NAPI.Util.ConsoleOutput("WheelType " + vehicle.WheelType.ToString());
            NAPI.Util.ConsoleOutput("WindowTint " + vehicle.WindowTint.ToString());
        }
    }
}
