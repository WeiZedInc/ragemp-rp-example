using GTANetworkAPI;
using WeiZed.Core;
using WeiZed.Systems;
using System;

namespace WeiZed.Jobs
{
    class Truckers
    {
        public static Random random = new Random();
        public static DateTime CoolDown = DateTime.Now;
        private static ColShape OilBarreDropColShape;
        private static ColShape FuelBarrelDropColShape;
        private static ColShape FactoryBarrelDropColShape;
        private static Ped truckGiverNPC;
        private static Ped trailerGiverNPC;
        private static Ped oilLSFullBarrelGiverNPC;
        private static Ped fuelStorageBarrelGiverNPC;
        #region CoordinateArrays
        //factory
        private static Vector3[] TrucksSpawnPositionArray =
        {
            new Vector3(2676.3745, 1384.6184, 25.107624),
            new Vector3(2676.3745, 1378.6509, 25.107624),
            new Vector3(2676.3745, 1372.6783, 25.107624),
            new Vector3(2676.3745, 1366.89, 25.107624),
            new Vector3(2676.3745, 1360.326, 25.107624),
            new Vector3(2676.3745, 1353.691, 25.107624),
            new Vector3(2676.3745, 1347.4723, 25.107624),
            new Vector3(2676.3745, 1340.5367, 25.107624)
        };
        private static Vector3[] TrucksSpawnRotationArray =
        {
            new Vector3(-0.6909122, 0.4680496, -88.57175),
            new Vector3(-0.47908315, 0.04519373, -88.2739),
            new Vector3(-0.44637105, 0.072471425, -89.96374),
            new Vector3(-0.40224966, 0.040688697, -90.293686),
            new Vector3(-0.334299, 0.0067784004, -90.18134),
            new Vector3(-0.319776, 0.06462922, -88.504776),
            new Vector3(-0.29427814, 0.061832737, -89.18621),
            new Vector3(-0.3914053, 0.019505588, -89.295586)
        };
        private static bool[] SpawnPointsOccupiedStatus = new bool[8];
        //oilLS
        public static Vector3[] OilLSTruckerFullBarrelPos =
                    {
                        new Vector3(1703.1501, -1620.4283, 112.46879),
                        new Vector3(1734.4805, -1631.7347, 112.45141),
                        new Vector3(1729.1418, -1612.3289, 112.45248)
                    };
        public static Vector3[] OilLSTruckerFullBarrelRot =
        {
                        new Vector3(0, 0, -159.17299),
                        new Vector3(0, 0, 78.793564),
                        new Vector3(0, 0, 157.24295)
                    };
        private static bool[] OilLSSpawnPointsOccupiedStatus = new bool[3];
        //fuelLS
        public static Vector3[] FuelLSTruckerFullBarrelPos =
                    {
                        new Vector3(535.0045, -2735.5515, 6.056407),
                        new Vector3(538.79913, -2737.7908, 6.056425),
                        new Vector3(543.41394, -2739.9539, 6.0563464)
                    };
        public static Vector3[] FuelLSTruckerFullBarrelRot =
        {
                        new Vector3(0, 0, -32.354507),
                        new Vector3(0, 0, -35.981327),
                        new Vector3(0, 0, -30.320505)
                    };
        private static bool[] FuelLSSpawnPointsOccupiedStatus = new bool[3];
        #endregion

        public static void Init()
        {
            #region NPC, their blips & colshapes
            truckGiverNPC = NAPI.Ped.CreatePed(436345731, new Vector3(2681.1067, 1392.2058, 24.548098), -140, false, true, true, true, 0); // truck
            CreateColShapeAndBlipForNPC(truckGiverNPC, "TRUCK_GIVER_NPC", 477, 4, "Дальнобойщики");

            trailerGiverNPC = NAPI.Ped.CreatePed(1644266841, new Vector3(2683.57, 1454.8694, 24.528084), -92.86f, false, true, true, true, 0); // trailer
            CreateColShapeAndBlipForNPC(trailerGiverNPC, "Factory_BARREL_GIVER_NPC", 436, 1, "Пустая бочка", 0.7f, true, 100f);

            oilLSFullBarrelGiverNPC = NAPI.Ped.CreatePed(349680864, new Vector3(1731.4048, -1666.0907, 112.559616), 131.52f, false, true, true, true, 0);
            CreateColShapeAndBlipForNPC(oilLSFullBarrelGiverNPC, "OilLS_BARREL_GIVER_NPC", 436, 1, "Заполненная бочка", 0.7f, true, 100f);

            fuelStorageBarrelGiverNPC = NAPI.Ped.CreatePed(349680864, new Vector3(2747.5085, 1379.7738, 24.518074), -76.243f, false, true, true, true, 0); //new Vector3(556.98456, -2741.4124, 6.056066)
            CreateColShapeAndBlipForNPC(fuelStorageBarrelGiverNPC, "FuelLS_BARREL_GIVER_NPC", 436, 1, "Пустая бочка", 0.7f, true, 100f);
            #endregion

            #region Trailer drop points, their markers & colshapes
            //OIL_DROP
            NAPI.Marker.CreateMarker(39, new Vector3(1587.3228, -1717.4412, 88.68337), new Vector3(0, 0, 0), new Vector3(0, 0, 0), 4f, new Color(255, 218, 0), bobUpAndDown: true, dimension: 0); // barreldrop marker
            OilBarreDropColShape = NAPI.ColShape.CreatCircleColShape(1587.3228f, -1717.4412f, 10f, 0);
            OilBarreDropColShape.OnEntityEnterColShape += (shape, player) =>
            {
                ClientSideFunctions.AskForTrailerAttachedData(player);
                NAPI.Task.Run(() =>
                {
                    if (!player.HasData("FACTORY_TRAILER_INSTANCE")) return; // если у него нет Vehicle trailer
                    if (player.GetData<string>("MISSION_TYPE") != "OIL_LS")
                    {
                        player.SendChatMessage("Возможно вам не сюда?");
                        return; // если миссия не с этим колшейпом
                    }
                    if (player.GetData<int>("TrailerHandle_ID") != 0) //  если у него есть прикреплённый трейлер 
                    {
                        player.GetData<Vehicle>("FACTORY_TRAILER_INSTANCE").Delete();
                        player.SendChatMessage("Вы успешно сдали пустую бочку, отправляйтесь за полной!");
                        player.SetData("IS_FACTORY_TRAILER_DROPPED", true);
                        player.ResetData("FACTORY_TRAILER_INSTANCE");
                        player.ResetData("MISSION_TYPE");
                        ClientSideFunctions.SetWaypoint(player, 1731.4048f, -1666.0907f);
                    }
                    else
                    {
                        player.SendChatMessage("Это место сдачи пустой бочки для заполнения.");
                    }
                }, 500);
            };

            //FUEL_DROP
            NAPI.Marker.CreateMarker(39, new Vector3(2747.5085, 1379.7738, 24.518074), new Vector3(0, 0, 0), new Vector3(0, 0, 0), 4f, new Color(255, 218, 0), bobUpAndDown: true, dimension: 0); // barreldrop marker
            FuelBarrelDropColShape = NAPI.ColShape.CreatCircleColShape(2747.50f, 1379.77f, 10f, 0);
            FuelBarrelDropColShape.OnEntityEnterColShape += (shape, player) =>
            {
                ClientSideFunctions.AskForTrailerAttachedData(player);
                NAPI.Task.Run(() =>
                {
                    if (!player.HasData("FACTORY_TRAILER_INSTANCE")) return; // если у него нет Vehicle trailer
                    if (player.GetData<string>("MISSION_TYPE") != "FUEL_LS")
                    {
                        player.SendChatMessage("Возможно вам не сюда?");
                        return; // если миссия не с этим колшейпом
                    }
                    if (player.GetData<int>("TrailerHandle_ID") != 0) //  если у него есть прикреплённый трейлер 
                    {
                        player.GetData<Vehicle>("FACTORY_TRAILER_INSTANCE").Delete();
                        player.SendChatMessage("Вы успешно сдали пустую бочку, отправляйтесь за полной!");
                        player.SetData("IS_FACTORY_TRAILER_DROPPED", true);
                        player.ResetData("FACTORY_TRAILER_INSTANCE");
                        player.ResetData("MISSION_TYPE");
                        ClientSideFunctions.SetWaypoint(player, 556.984f, -2741.41f);
                    }
                    else
                    {
                        player.SendChatMessage("Это место сдачи пустой бочки для заполнения.");
                    }
                }, 500);
            };

            //FACTORY_DROP
            NAPI.Marker.CreateMarker(39, new Vector3(2791.6506, 1523.6624, 25.505562), new Vector3(0, 0, 0), new Vector3(0, 0, 0), 4f, new Color(255, 218, 0), bobUpAndDown: true, dimension: 0); // barreldrop marker
            FactoryBarrelDropColShape = NAPI.ColShape.CreatCircleColShape(2791.6506f, 1523.6624f, 10f, 0);
            FactoryBarrelDropColShape.OnEntityEnterColShape += (shape, player) =>
            {
                ClientSideFunctions.AskForTrailerAttachedData(player);
                NAPI.Task.Run(() =>
                {
                    if (!player.HasData("FACTORY_TRAILER_INSTANCE")) return; // если у него нет Vehicle trailer
                    if (player.GetData<string>("MISSION_TYPE") != "FACTORY") // если ему ещё не сюда
                    {
                        player.SendChatMessage("Возможно вам не сюда?");
                    }
                    if (player.GetData<int>("TrailerHandle_ID") != 0) // если есть трейлер
                    {
                        player.SendChatMessage("Вы успешно сдали полную бочку, отправляйтесь за новым заказом!");
                        player.GetData<Vehicle>("FACTORY_TRAILER_INSTANCE").Delete();
                        player.ResetData("FACTORY_TRAILER_INSTANCE");
                        player.ResetData("MISSION_TYPE");
                        player.ResetData("TrailerHandle_ID");
                    }
                    else
                    {
                        player.SendChatMessage("Это место сдачи полной бочки для переработки.");
                    }
                }, 500);
            };
            #endregion

            Console.WriteLine("Init truckers success");
        }

        public static void SpawnJobVehicle(Player player)
        {
            Random random = new Random();
            try
            {
                player.SetData("WORK_TYPE", "TRUCKER");
                int i = random.Next(0, 8); // max value должен быть больше на 1 чем рил макс значение
                if (SpawnPointsOccupiedStatus[i]) return;
                if (player.HasData("OWNED_TRUCK_INSTANCE"))
                {
                    player.SendChatMessage("У вас уже есть служебное авто!");
                    return;
                }
                SpawnPointsOccupiedStatus[i] = true;

                Vehicle truck = VehicleSystem.CreateJobVehicle("Truck-", TrucksSpawnPositionArray[i], TrucksSpawnRotationArray[i], "Packer", i, i);
                player.SetIntoVehicle(truck, 0);
                ClientSideFunctions.SetVehicleInvincibleAndTransparency(player, truck, 5);
                player.SendChatMessage("Во избежание неприятных ситуаций, рекомендуем отъехать с места выдачи в течении 5-ти секунд.");
                player.SendChatMessage("Человек, выдающий прицепы, отмечен на карте.");
                ClientSideFunctions.SetWaypoint(player, trailerGiverNPC.Position.X, trailerGiverNPC.Position.Y);

                truck.SetData("OWNER_PLAYER_INSTANCE", player);
                player.SetData("OWNED_TRUCK_INSTANCE", truck);
                player.SetData("JOB_VEHICLE", truck);
                NAPI.Task.Run(() =>
                {
                    SpawnPointsOccupiedStatus[i] = false;
                    if (player.Vehicle != truck)
                    {
                        truck.Delete();
                        player.ResetData("OWNED_TRUCK_INSTANCE");
                        player.ResetData("JOB_VEHICLE");
                        player.ResetData("WORK_TYPE");
                    }
                }
                , 5000);
            }
            catch (Exception e)
            {
                Console.WriteLine(e + "Err");
            }
        }

        public static void SpawnFactoryJobTrailerAndGetMission(Player player, Vector3 pos, Vector3 rot)
        {
            try
            {
                if (InCoolDown())
                {
                    player.SendChatMessage("KD");
                    return;
                }
                if (player.HasData("FACTORY_TRAILER_INSTANCE"))
                {
                    player.SendChatMessage("Вы уже взяли прицеп!");
                    return;
                }
                CoolDown = DateTime.Now;
                CoolDown = CoolDown.AddSeconds(30);
                int i = 1;//random.Next(0, 2);
                string trailerName = "Tanker2";
                switch (i)
                {
                    case 0:
                        trailerName = "Tanker2";
                        break;
                    case 1:
                        trailerName = "Tanker";
                        break;
                    default:
                        break;
                } // tanker model
                Vehicle trailer = VehicleSystem.CreateJobVehicle("Tank-", pos, rot, trailerName, 1, 1);
                ClientSideFunctions.SetVehicleInvincible(player, trailer, true);

                player.SetData("FACTORY_TRAILER_INSTANCE", trailer); // записал на игрока экземпляр бочки с фабрики
                GetMissionAtFactory(player, trailer, i);
            }
            catch (Exception e)
            {
                Console.WriteLine(e + "Err");
            }
        }
        private static void GetMissionAtFactory(Player player, Vehicle factoryTrailer, int i)
        {
            int timeBeforeCheck = 20000; // ms
            switch (i)
            {
                case 0:
                    player.SendChatMessage($"У вас есть {timeBeforeCheck / 1000} секунд, чтобы безопасно присоединить прицеп, после чего вам будет выдано задание.");
                    NAPI.Task.Run(() =>
                    {
                        ClientSideFunctions.AskForTrailerAttachedData(player);
                    }, timeBeforeCheck - 500);
                    NAPI.Task.Run(() =>
                    {
                        if (player.GetData<int>("TrailerHandle_ID") == 0)// если он не успел присоединить трейлер
                        {
                            player.ResetData("FACTORY_TRAILER_INSTANCE");// удаляю дату с игрока о владении трейлером
                            if (factoryTrailer != null) // проверка, мб удалился выходом из рабочего авто
                            {
                                factoryTrailer.Delete();
                                player.SendChatMessage("Вы не успели.");
                            }
                            return;
                        }
                        else
                        {
                            ClientSideFunctions.SetWaypoint(player, 1587.3228f, -1717.4412f); // oilLS barreldrop
                            player.SendChatMessage("Метка сдачи отмечена на карте.");
                            player.SetData("MISSION_TYPE", "OIL_LS");
                        }
                    }, timeBeforeCheck);
                    break;
                case 1:
                    player.SendChatMessage($"У вас есть {timeBeforeCheck / 1000} секунд, чтобы безопасно присоединить прицеп, после чего вам будет выдано задание.");
                    NAPI.Task.Run(() =>
                    {
                        ClientSideFunctions.AskForTrailerAttachedData(player);
                    }, timeBeforeCheck - 500);
                    NAPI.Task.Run(() =>
                    {
                        if (player.GetData<int>("TrailerHandle_ID") == 0)// если он не успел присоединить трейлер
                        {
                            player.ResetData("FACTORY_TRAILER_INSTANCE");// удаляю дату с игрока о владении трейлером
                            if (factoryTrailer != null) // проверка, мб удалился выходом из рабочего авто
                            {
                                factoryTrailer.Delete();
                                player.SendChatMessage("Вы не успели.");
                            }
                            return;
                        }
                        else
                        {
                            ClientSideFunctions.SetWaypoint(player, 676.8957f, -2734.1487f); // FuelLS barreldrop
                            player.SendChatMessage("Метка сдачи отмечена на карте.");
                            player.SetData("MISSION_TYPE", "FUEL_LS");
                        }
                    }, timeBeforeCheck);
                    break;
                default:
                    break;
            }
        }

        public static void SpawnOilJobTrailerAndGetMission(Player player, string trailerName = "Tanker")
        {
            try
            {
                int i = random.Next(0, 3);
                if (player.GetData<string>("WORK_TYPE") != "TRUCKER") return; // если не на работе
                if (!player.GetData<bool>("IS_FACTORY_TRAILER_DROPPED")) return;// если не привез бочку с фабрики
                if (OilLSSpawnPointsOccupiedStatus[i]) return; // если занята точка спавна
                if (player.HasData("FACTORY_TRAILER_INSTANCE")) // если уже взял прицеп
                {
                    player.SendChatMessage("Вы уже взяли прицеп!");
                    return;
                }
                OilLSSpawnPointsOccupiedStatus[i] = true;
                Vehicle trailer = VehicleSystem.CreateJobVehicle("Tank-", OilLSTruckerFullBarrelPos[i], OilLSTruckerFullBarrelRot[i], trailerName, 1, 1);
                ClientSideFunctions.SetVehicleInvincible(player, trailer, true);
                player.SetData("FACTORY_TRAILER_INSTANCE", trailer);
                ClientSideFunctions.SetWaypoint(player, trailer.Position.X, trailer.Position.Y);
                GetMissionAtOilLS(player, trailer);
                NAPI.Task.Run(() => OilLSSpawnPointsOccupiedStatus[i] = false, 5000);
            }
            catch (Exception e)
            {
                Console.WriteLine(e + "Err");
            }
        }
        private static void GetMissionAtOilLS(Player player, Vehicle trailer)
        {
            player.SendChatMessage("Комапания `Oil LS` выдала заказ.");
            player.SendChatMessage("У вас есть 30 секунд, чтобы безопасно присоединить прицеп, после чего вам будет выдано задание.");
            NAPI.Task.Run(() =>
            {
                ClientSideFunctions.AskForTrailerAttachedData(player);
            }, 29500);
            NAPI.Task.Run(() =>
            {
                if (player.GetData<int>("TrailerHandle_ID") == 0) // если не успел присоединить бочку
                {
                    trailer.Delete();
                    player.ResetData("FACTORY_TRAILER_INSTANCE");
                    player.SendChatMessage("Вы не успели.");
                    return;
                }
                else
                {
                    // barreldrop
                    ClientSideFunctions.SetWaypoint(player, 2791.6506f, 1523.6624f);
                    player.SetData("MISSION_TYPE", "FACTORY");
                    player.SendChatMessage("Метка сдачи отмечена на карте.");
                }
            }, 30000);
        }

        public static void SpawnFuelJobTrailerAndGetMission(Player player, string trailerName = "Tanker2")
        {
            try
            {
                int i = random.Next(0, 3);
                if (player.GetData<string>("WORK_TYPE") != "TRUCKER") return; // если не на работе
                if (!player.GetData<bool>("IS_FACTORY_TRAILER_DROPPED")) return;// если не привез бочку с фабрики
                if (FuelLSSpawnPointsOccupiedStatus[i]) return; // если занята точка спавн
                if (player.HasData("FACTORY_TRAILER_INSTANCE")) // если уже взял прицеп
                {
                    player.SendChatMessage("Вы уже взяли прицеп!");
                    return;
                }
                FuelLSSpawnPointsOccupiedStatus[i] = true;
                //Vehicle jobTanker = VehicleSystem.CreateJobVehicle("Tank-", FuelLSTruckerFullBarrelPos[i], FuelLSTruckerFullBarrelRot[i], trailerName, 1, 1);
                Vehicle trailer = VehicleSystem.CreateJobVehicle("Tank-", new Vector3(2731.0098, 1379.614, 24.51478), new Vector3(0, 0, 87.06708), trailerName, 1, 1);
                player.SetData("FACTORY_TRAILER_INSTANCE", trailer);
                ClientSideFunctions.SetWaypoint(player, trailer.Position.X, trailer.Position.Y);
                GetMissionAtFuelStorage(player, trailer);
                NAPI.Task.Run(() => FuelLSSpawnPointsOccupiedStatus[i] = false, 20000);
            }
            catch (Exception e)
            {
                Console.WriteLine(e + "Err");
            }
        }
        private static void GetMissionAtFuelStorage(Player player, Vehicle trailer)
        {
            player.SendChatMessage("Комапания `Fuel LS` выдала заказ.");
            player.SendChatMessage("У вас есть 20 секунд, чтобы безопасно присоединить прицеп, после чего вам будет выдано задание.");
            NAPI.Task.Run(() =>
            {
                ClientSideFunctions.AskForTrailerAttachedData(player);
            }, 19500);
            NAPI.Task.Run(() =>
            {
                if (player.GetData<int>("TrailerHandle_ID") == 0) // если не успел присоединить бочку
                {
                    trailer.Delete();
                    player.ResetData("FACTORY_TRAILER_INSTANCE");
                    player.SendChatMessage("Вы не успели.");
                    return;
                }
                else
                {
                    // factory barreldrop
                    ClientSideFunctions.SetWaypoint(player, 2791.6506f, 1523.6624f);
                    player.SetData("MISSION_TYPE", "FACTORY");
                    player.SendChatMessage("Метка сдачи отмечена на карте.");
                }
            }, 20000);
        }

        public static void CreateColShapeAndBlipForNPC(Ped npc, string colshapeType, int blipID, byte blipColor, string blipName, float size = 1f, bool isShortRange = false, float drawDistance = 0)
        {
            ColShape colShape = NAPI.ColShape.CreateCylinderColShape(npc.Position, 3f, 2f, 0);
            colShape.OnEntityEnterColShape += (colshape, player) =>
            {
                colShape.SetSharedData("colshapeType", colshapeType);
            };
            colShape.OnEntityExitColShape += (colshape, player) =>
            {
                colShape.ResetSharedData("colshapeType");
            };
            Blip blip = NAPI.Blip.CreateBlip(blipID, npc.Position, size, blipColor, name: blipName, dimension: NAPI.GlobalDimension, shortRange: isShortRange, drawDistance: drawDistance);
        }
        public static bool InCoolDown()
        {
            if (CoolDown > DateTime.Now)
            {
                return true; // inCool
            }
            else
            {
                return false; // notInCool
            }

        }



        [ServerEvent(Event.PlayerExitVehicle)]
        public void OnPlayerExitVehicle(Player player, Vehicle vehicle)
        {
            player.ResetData("CLIENT_SIDE_TrailerID"); // удаляю трейлер (возможно лишняя херь)
            if (vehicle == player.GetData<Vehicle>("JOB_VEHICLE")) // если рабочаяя машина
            {
                player.SendChatMessage("Служебное авто будет уничтожено через 60 секунд.");
                NAPI.Task.Run(() =>
                {
                    if (player.Vehicle == player.GetData<Vehicle>("JOB_VEHICLE")) return;
                    if (player.GetData<string>("WORK_TYPE") == "TRUCKER")
                    {
                        player.GetData<Vehicle>("OWNED_TRUCK_INSTANCE")?.Delete();
                        player.ResetData("OWNED_TRUCK_INSTANCE");
                        player.GetData<Vehicle>("FACTORY_TRAILER_INSTANCE")?.Delete();
                        player.ResetData("FACTORY_TRAILER_INSTANCE");
                    }
                    player.ResetData("WORK_TYPE");
                    player.ResetData("JOB_VEHICLE");
                    player.SendChatMessage("Служебное авто уничтожено, Вы уволены с работы.");
                }, 15000);
            }
        }

        [ServerEvent(Event.PlayerEnterVehicle)]
        public void OnPlayerEnterVehicle(Player player, Vehicle vehicle, sbyte seatID)
        {
            if (vehicle == player.GetData<Vehicle>("JOB_VEHICLE"))
            {
                ClientSideFunctions.AskForTrailerAttachedData(player);
            }
        }
    }
}
