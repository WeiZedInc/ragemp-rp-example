using GTANetworkAPI;
using WeiZed.Core;
using WeiZed.MySQL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;

namespace WeiZed.Systems
{
    class GarageSystem
    {
        public static Dictionary<int, GarageSystem> AllGaragesDictionary = new Dictionary<int, GarageSystem>();  // Exmpl: <garage_ID, garageInstance>


        public int garage_ID { get; set; }
        public Vector3 garagePos { get; set; }
        public Vector3 garageRot { get; set; }
        public int garageSize { get; set; }
        public int carsCount { get; set; }
        public int garageInteriorID { get; set; }
        ColShape shape;
        public static Vector3[] garageInteriors =
        {
            new Vector3(173.2903, -1003.6, -99.65707), // 2car
            new Vector3(197.8153, -1002.293, -99.65749),// 6car
            new Vector3(229.9559, -981.7928, -99.66071) // 10car
        };


        public static void CreateNewGarage(Player sender, int forHouseWith_ID, int garageSize)
        {
            GarageSystem newGarage = new GarageSystem();



            newGarage.garage_ID = forHouseWith_ID;
            newGarage.garageSize = garageSize;
            newGarage.garagePos = (sender.Position - new Vector3(0, 0, 1.33));
            newGarage.garageRot = sender.Rotation;
            newGarage.carsCount = 0;
            switch (garageSize)
            {
                case 2:
                    newGarage.garageInteriorID = 0;
                    break;
                case 6:
                    newGarage.garageInteriorID = 1;
                    break;
                case 10:
                    newGarage.garageInteriorID = 2;
                    break;
                default:
                    newGarage.garageInteriorID = -1;
                    break;
            }

            #region MySQL Operations
            MySQLStatic.Query($"INSERT INTO `garages` (`id`, `garage_ID`, `garagePos`, `garageRot`, `garageSize`, `carsCount`)" +
                    $" VALUES(NULL, " +
                    $"'{newGarage.garage_ID}', " +
                    $"'{JsonConvert.SerializeObject(newGarage.garagePos)}', " +
                    $"'{JsonConvert.SerializeObject(newGarage.garageRot)}', " +
                    $"'{newGarage.garageSize}'," +
                    $"'{newGarage.carsCount}')");
            #endregion MySQL Operations

            /////////////////////////////////////////////
            int DictKey = newGarage.garage_ID;
            GarageSystem.AllGaragesDictionary.Add(DictKey, newGarage);
            /////////////////////////////////////////////
            newGarage.CreateColShapeForGarage();
        }//Для создания через команду
        public void CreateColShapeForGarage()
        {
            shape = NAPI.ColShape.CreateCylinderColShape(garagePos, 1.5f, 2, 0);
            shape.OnEntityEnterColShape += (colshape, player) =>
            {
                player.SetData("garage_ID", garage_ID);
                shape.SetSharedData("colshapeType", "GARAGE_COLSHAPE");
                shape.SetSharedData("object_ID", garage_ID);
            };
            shape.OnEntityExitColShape += (colshape, player) =>
            {
                player.ResetData("garage_ID");
                shape.ResetSharedData("colshapeType");
                shape.ResetSharedData("object_ID");
            };
            Color color = new Color(255, 95, 0);
            NAPI.TextLabel.CreateTextLabel($"Гараж для жилья #{garage_ID}", garagePos + new Vector3(0, 0, 1.5), 5f, 0.4f, 0, new Color(255, 222, 173), false, 0);
        }//Для создания через команду



        public static void LoadGaragesFromDB()
        {
            int allGarageID = 0;
            string result = Convert.ToString(MySQLStatic.QueryRead($"SELECT `garage_ID` FROM `garages` ORDER BY `garage_ID` DESC LIMIT 1"));
            if (result != "")
            {
                allGarageID = Convert.ToInt32(result);
                NAPI.Util.ConsoleOutput("Last garage id in base - " + allGarageID);
                for (int i = 1; i <= allGarageID; i++)
                {
                    int DictKey = i;
                    GarageSystem.AllGaragesDictionary.Add(DictKey, CreateGarageFromBD(i));
                }
                NAPI.Util.ConsoleOutput("Garages added to garagesDictionary - " + GarageSystem.AllGaragesDictionary.Count.ToString());
            }
            else
            {
                NAPI.Util.ConsoleOutput("Not found garages in base!");
            }

        }//Для создания из бд
        public static GarageSystem CreateGarageFromBD(int garage_ID)//Для создания из бд
        {
            GarageSystem garage = new GarageSystem();
            garage.garage_ID = garage_ID;
            garage.garagePos = JsonConvert.DeserializeObject<Vector3>(MySQLStatic.QueryRead($"SELECT `garagePos` FROM `garages` WHERE `garage_ID` = {garage_ID}"));
            garage.garageRot = JsonConvert.DeserializeObject<Vector3>(MySQLStatic.QueryRead($"SELECT `garageRot` FROM `garages` WHERE `garage_ID` = {garage_ID}"));
            garage.garageSize = Convert.ToInt32(MySQLStatic.QueryRead($"SELECT `garageSize` FROM `garages` WHERE `garage_ID` = {garage_ID}"));
            garage.carsCount = Convert.ToInt32(MySQLStatic.QueryRead($"SELECT `carsCount` FROM `garages` WHERE `garage_ID` = {garage_ID}"));
            switch (garage.garageSize)
            {
                case 2:
                    garage.garageInteriorID = 0;
                    break;
                case 6:
                    garage.garageInteriorID = 1;
                    break;
                case 10:
                    garage.garageInteriorID = 2;
                    break;
                default:
                    garage.garageInteriorID = -1;
                    break;
            }


            garage.CreateColShapeForGarage();
            return garage;
        }




        public static void EnterGarage(Player player)
        {
            if (player.HasData("garage_ID"))
            {
                int garage_ID = player.GetData<int>("garage_ID");
                CharacterData playerData = DBFunctions.GetPlayerInstanceFromDictionary(player);
                HouseSystem house;
                HouseSystem.AllHousesDictionary.TryGetValue(garage_ID, out house);
                GarageSystem garage;
                GarageSystem.AllGaragesDictionary.TryGetValue(garage_ID, out garage);
                // для купленного дома , для владельца
                if (playerData.ownedHouse_ID == garage_ID)
                {
                    if (garage.garageInteriorID != -1)
                    {
                        if (player.IsInVehicle)
                        {
                            ClientSideFunctions.ScreenFadeEffect(player, 0, 400);
                            //Часть машины
                            Vehicle vehicle = player.Vehicle;
                            garage.SpawnVehiclesOnEnter();
                            GarageSystem.EnterWithVehicle(vehicle, garage, house); // static бо лень переписывать) 
                            player.SetIntoVehicle(vehicle, 0);


                            //Часть игрока
                            NAPI.Entity.SetEntityDimension(player, (uint)garage_ID);
                            player.SetData("INSIDE_GARAGE_ID", garage_ID);
                            player.SetSharedData("INSIDE_TYPE", "INSIDE_GARAGE");
                            return;
                        }
                        else
                        {
                            garage.SpawnVehiclesOnEnter();
                            ClientSideFunctions.ScreenFadeEffect(player, 0, 400);
                            NAPI.Entity.SetEntityPosition(player, garageInteriors[garage.garageInteriorID]);
                            NAPI.Entity.SetEntityDimension(player, (uint)garage_ID);
                            player.SetData("INSIDE_GARAGE_ID", garage_ID);
                            player.SetSharedData("INSIDE_TYPE", "INSIDE_GARAGE");
                            return;
                        }
                    }
                    else
                    {
                        player.SendChatMessage("Гараж на одну машину,при входе должен спавнить одну машину,дописать");
                        return;
                    }
                }
                // для некупленого/купленного закрытого дома 
                else if (garage.garageInteriorID != -1)
                {
                    if (playerData.ownedHouse_ID != garage_ID && house.isLocked == true)
                    {
                        player.SendChatMessage($"{Colors.RED}Гараж закрыт.");
                        return;
                    }
                    else
                    {
                        if (player.IsInVehicle)
                        {
                            player.SendChatMessage($"{Colors.RED}Покиньте транспорт.");
                            return;
                        }
                        else
                        {
                            ClientSideFunctions.ScreenFadeEffect(player, 0, 400);
                            NAPI.Entity.SetEntityPosition(player, garageInteriors[garage.garageInteriorID]);
                            NAPI.Entity.SetEntityDimension(player, (uint)garage_ID);
                            player.SetData("INSIDE_GARAGE_ID", garage_ID);
                            player.SetSharedData("INSIDE_TYPE", "INSIDE_GARAGE");
                            return;
                        }
                    }
                }
                else
                {
                    player.SendChatMessage("Гараж на одну машину,при входе должен спавнить одну машину,дописать");
                    return;
                }

            }
        }
        public static void ExitGarage(Player player)
        {
            if (player.HasData("INSIDE_GARAGE_ID"))
            {
                int garage_ID = player.GetData<int>("INSIDE_GARAGE_ID");
                CharacterData playerData = DBFunctions.GetPlayerInstanceFromDictionary(player);
                HouseSystem house;
                HouseSystem.AllHousesDictionary.TryGetValue(garage_ID, out house);
                GarageSystem garage;
                GarageSystem.AllGaragesDictionary.TryGetValue(garage_ID, out garage);
                // если в машине
                if (player.IsInVehicle)
                {
                    ClientSideFunctions.ScreenFadeEffect(player, 0, 400);
                    //часть машинки
                    Vehicle vehicle = player.Vehicle;
                    GarageSystem.ExitWithVehicle(vehicle, garage);
                    garage.DeleteVehiclesOnExit(player);
                    player.SetIntoVehicle(vehicle, 0);


                    // чась игрока
                    NAPI.Entity.SetEntityDimension(player, 0);
                    player.ResetData("INSIDE_GARAGE_ID");
                    player.ResetSharedData("INSIDE_TYPE");
                    vehicle.ResetData("INSIDE_GARAGE_ID");
                }
                else
                {
                    garage.DeleteVehiclesOnExit(player);
                    ClientSideFunctions.ScreenFadeEffect(player, 0, 400);
                    NAPI.Entity.SetEntityPosition(player, (garage.garagePos + new Vector3(0, 0, 1)));
                    NAPI.Entity.SetEntityDimension(player, 0);
                    player.ResetData("INSIDE_GARAGE_ID");
                    player.ResetSharedData("INSIDE_TYPE");
                }
            }
            else
            {
                player.SendChatMessage($"{Colors.ORANGE}Вы не в гараже.");
            }
        }
        public static void EnterWithVehicle(Vehicle vehicle, GarageSystem garage, HouseSystem house)
        {
            vehicle.Dimension = (uint)house.houseGarage_ID;
            vehicle.Position = garageInteriors[garage.garageInteriorID];
            vehicle.SetData("INSIDE_GARAGE_ID", garage.garage_ID);
            string garageVehiclesNumberPlates = MySQLStatic.QueryRead($"SELECT `garageVehiclesNumberPlates` FROM `garages` WHERE `garage_ID`='{garage.garage_ID}'");
            #region Запись номеров машин
            string newNumberPlatesToBD;
            if (garageVehiclesNumberPlates != "") // если garageVehiclesNumberPlates не пустой то прибавляем к имеющемуся
            {
                newNumberPlatesToBD = garageVehiclesNumberPlates + ", " + vehicle.NumberPlate;
                MySQLStatic.Query($"UPDATE `garages` SET `garageVehiclesNumberPlates`= '{newNumberPlatesToBD}' WHERE `garage_ID`='{garage.garage_ID}'");
                MySQLStatic.Query($"UPDATE `personal_vehicles` SET `garage_ID` = '{garage.garage_ID}' WHERE `numberPlate`='{vehicle.NumberPlate}'");
            }
            else // если пустой
            {
                MySQLStatic.Query($"UPDATE `garages` SET `garageVehiclesNumberPlates`= '{vehicle.NumberPlate}' WHERE `garage_ID`='{garage.garage_ID}'");
                MySQLStatic.Query($"UPDATE `personal_vehicles` SET `garage_ID` = '{garage.garage_ID}' WHERE `numberPlate`='{vehicle.NumberPlate}'");
            }
            #endregion



        }
        public static void ExitWithVehicle(Vehicle vehicle, GarageSystem garage)
        {
            string garageVehiclesNumberPlates = MySQLStatic.QueryRead($"SELECT `garageVehiclesNumberPlates` FROM `garages` WHERE `garage_ID`='{garage.garage_ID}'");
            #region Убираю номерной знак с базы при выезде на тачке и вытягиваю тачку
            vehicle.Position = (garage.garagePos + new Vector3(0, 0, 1));
            vehicle.Dimension = 0;
            if (garageVehiclesNumberPlates.Contains(", " + vehicle.NumberPlate))
            {
                string newNumberPlatesToBD = garageVehiclesNumberPlates.Replace(", " + vehicle.NumberPlate, "");
                //NAPI.Chat.SendChatMessageToAll("/" + newNumberPlatesToBD + "/");
                MySQLStatic.Query($"UPDATE `garages` SET `garageVehiclesNumberPlates`= '{newNumberPlatesToBD}' WHERE `garage_ID`='{garage.garage_ID}'");
                MySQLStatic.Query($"UPDATE `personal_vehicles` SET `garage_ID` = 0 WHERE `numberPlate`='{vehicle.NumberPlate}'");
                return;
            }
            else if (garageVehiclesNumberPlates.Contains(vehicle.NumberPlate + ", "))
            {
                string newNumberPlatesToBD = garageVehiclesNumberPlates.Replace(vehicle.NumberPlate + ", ", "");
                MySQLStatic.Query($"UPDATE `garages` SET `garageVehiclesNumberPlates`= '{newNumberPlatesToBD}' WHERE `garage_ID`='{garage.garage_ID}'");
                MySQLStatic.Query($"UPDATE `personal_vehicles` SET `garage_ID` = 0 WHERE `numberPlate`='{vehicle.NumberPlate}'");
                return;
            }
            else
            {
                MySQLStatic.Query($"UPDATE `garages` SET `garageVehiclesNumberPlates`= NULL WHERE `garage_ID`='{garage.garage_ID}'");
                MySQLStatic.Query($"UPDATE `personal_vehicles` SET `garage_ID` = 0 WHERE `numberPlate`='{vehicle.NumberPlate}'");
            }
            #endregion

        }
        public void SpawnVehiclesOnEnter()
        {
            string garageVehiclesNumberPlates = MySQLStatic.QueryRead($"SELECT `garageVehiclesNumberPlates` FROM `garages` WHERE `garage_ID`='{garage_ID}'");
            if (garageVehiclesNumberPlates != "")
            {
                if (garageVehiclesNumberPlates.Contains(", ")) // если несколько машин стоит в гараже
                {
                    string[] numberPlates = garageVehiclesNumberPlates.Split(", ");
                    foreach (var numberPlate in numberPlates)
                    {
                        Vehicle createdVeh = CreateVehicleFromDB(numberPlate);
                        createdVeh.SetData("INSIDE_GARAGE_ID", garage_ID);
                    }
                }
                else // если одна машина в гараже
                {
                    Vehicle createdVeh = CreateVehicleFromDB(garageVehiclesNumberPlates);
                    createdVeh.SetData("INSIDE_GARAGE_ID", garage_ID);
                }
            }
        }
        public void DeleteVehiclesOnExit(Player player)
        {
            foreach (Vehicle vehicle in NAPI.Pools.GetAllVehicles())
            {
                if (player.Vehicle != vehicle)
                {
                    if (vehicle.GetData<int>("INSIDE_GARAGE_ID") == garage_ID)
                    {
                        MySQLStatic.Query($"UPDATE `personal_vehicles` SET `position`='{JsonConvert.SerializeObject(vehicle.Position)}' WHERE `carModelName`='{vehicle.DisplayName}'");
                        MySQLStatic.Query($"UPDATE `personal_vehicles` SET `rotation`='{JsonConvert.SerializeObject(vehicle.Rotation)}' WHERE `carModelName`='{vehicle.DisplayName}'");
                        MySQLStatic.Query($"UPDATE `personal_vehicles` SET `fuel`='{vehicle.GetData<int>("CAR_FUEL")}' WHERE `carModelName`='{vehicle.DisplayName}'");
                        MySQLStatic.Query($"UPDATE `personal_vehicles` SET `tune`='{JsonConvert.SerializeObject(vehicle.GetData<string>("CAR_TUNE"))}' WHERE `carModelName`='{vehicle.DisplayName}'");
                        MySQLStatic.Query($"UPDATE `personal_vehicles` SET `itemsInside`='{JsonConvert.SerializeObject(vehicle.GetData<string>("CAR_ITEMS"))}' WHERE `carModelName`='{vehicle.DisplayName}'");
                        VehicleSystem.PersonalVehiclesDictionary.Remove(vehicle.NumberPlate);
                        NAPI.Chat.SendChatMessageToAll("Машина удалена" + vehicle.NumberPlate);
                        vehicle.Delete();
                    }
                    else
                    {
                        continue;
                    }
                }
            }
        }
        public Vehicle CreateVehicleFromDB(string numberPlate)
        {
            DataTable dataTable = MySQLStatic.QueryReadTable($"SELECT * FROM `personal_vehicles` WHERE `numberPlate`='{numberPlate}'");
            foreach (DataRow Row in dataTable.Rows)
            {
                int garage_ID = Convert.ToInt32(Row["garage_ID"]);
                string carModelName = Convert.ToString(Row["carModelName"]);
                int color1 = Convert.ToInt32(Row["color1"]);
                int color2 = Convert.ToInt32(Row["color2"]);
                string ownerSocialName = Convert.ToString(Row["ownerSocialName"]);
                int fuel = Convert.ToInt32(Row["fuel"]);
                int price = Convert.ToInt32(Row["price"]);
                string tune = JsonConvert.DeserializeObject<string>(Row["tune"].ToString());
                string itemsInside = JsonConvert.DeserializeObject<string>(Row["itemsInside"].ToString());
                Vector3 position = JsonConvert.DeserializeObject<Vector3>(Row["position"].ToString());
                Vector3 rotation = JsonConvert.DeserializeObject<Vector3>(Row["rotation"].ToString());
                VehicleHash vehHash = (VehicleHash)NAPI.Util.GetHashKey(carModelName);
                Vehicle newVeh = NAPI.Vehicle.CreateVehicle(vehHash, position, rotation, color1, color2, numberPlate, dimension: (uint)garage_ID);
                newVeh.SetData("CAR_ACCESS", "PERSONAL_VEHICLE");
                newVeh.SetData("CAR_OWNER", $"{ownerSocialName}");
                newVeh.SetData<int>("CAR_PRICE", price);
                newVeh.SetData<int>("CAR_FUEL", fuel);
                newVeh.SetData("CAR_TUNE", $"{tune}");
                newVeh.SetData("CAR_ITEMS", $"{itemsInside}");
                VehicleSystem.PersonalVehiclesDictionary.Add(numberPlate, newVeh);
                return newVeh;
            }
            return null;
        }
    }
}
