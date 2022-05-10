using GTANetworkAPI;
using WeiZed.Bussines;
using WeiZed.Systems;
using System;
using System.Globalization;

namespace WeiZed.Core
{
    class Commands : Script
    {
        #region testing cmds

        [Command("createoilcompany")]
        public void ADM_createoilcompany(Player sender, string type, int price = 1)
        {
            if (AdmFunctions.GetAdminLVL(sender) >= 8)
            {
                OilCompanyBase.CreateNewCompany(sender, type, price);
            }
        }

        [Command("t")]
        public void t(Player sender)
        {
            sender.TriggerEvent("render");
        }

        #endregion

        #region Admin Comands//////////////

        [Command("boost")] // Буст машины / ускорение (7 лвл)
        public static void CMD_SetTurboTorque(Player sender, float power = 100, float torque = 800, float maxSpeed = -1)
        {
            if (AdmFunctions.GetAdminLVL(sender) >= 3)
            {
                if (!sender.IsInVehicle)
                {
                    return;
                }
                sender.TriggerEvent("client_BoostVehicleParameters", power, torque, maxSpeed);
                sender.SendChatMessage($"Машине установлено {power} лс. и {torque} крутящего момента.");
            }
        }


        [Command("kill", "~o~Использовние: ~w~/kill [id] [Можно указать еще и просто хп,оно установится]")]
        public void ADM_kill(Player sender, ushort id, int hpValue = 0)
        {
            if (AdmFunctions.GetAdminLVL(sender) >= 3)
            {
                AdmFunctions.KillTarget(Functions.GetPlayerByID(id), hpValue);
            }
        }

        [Command("revive", "~o~Использовние: ~w~/revive [id]")]
        public void ADM_revive(Player sender, ushort id)
        {
            if (AdmFunctions.GetAdminLVL(sender) >= 3)
            {
                AdmFunctions.ReviveTarget(Functions.GetPlayerByID(id));
            }
        }

        [Command("kick", "~o~Использовние: ~w~/kick [id] [Причина]", GreedyArg = true)]
        public void ADM_kick(Player sender, ushort id, string reason = null)
        {
            if (AdmFunctions.GetAdminLVL(sender) >= 3)
            {
                AdmFunctions.KickPlayer(sender, id, reason);
            }
        }

        [Command("pos", "~o~Использовние: ~w~/pos")]
        public void ADM_pos(Player sender)
        {
            if (AdmFunctions.GetAdminLVL(sender) >= 3)
            {
                Vector3 pos = Functions.GetPosition(sender);
                sender.SendChatMessage($"Position: new Vector3(X: {pos.X}, Y: {pos.Y}, Z: {pos.Z})");
                Vector3 rot = Functions.GetRotation(sender);
                sender.SendChatMessage($"Rotation: new Vector3(X: {rot.X}, Y: {rot.Y}, Z: {rot.Z})");
            }
        }

        [Command("savepos", "~o~Использовние: ~w~/savepos [Название координат в файле]")]
        public void ADM_savepos(Player sender, string coordsNameInFile)
        {
            if (AdmFunctions.GetAdminLVL(sender) >= 8)
            {
                Functions.SavePlayerPos(sender, coordsNameInFile);
            }
        }

        [Command("saveveh", "~o~Использовние: ~w~/saveveh [Название координат в файле]")]
        public void ADM_saveveh(Player sender, string coordsNameInFile)
        {
            if (AdmFunctions.GetAdminLVL(sender) >= 8)
            {
                Functions.SaveVehiclePos(sender, coordsNameInFile);
            }
        }

        [Command("setskin", "~o~Использовние: ~w~/setskin [id] [skin]")]
        public void ADM_setskin(Player sender, ushort id, string pedModel)
        {
            if (AdmFunctions.GetAdminLVL(sender) >= 3)
            {
                Player target = Functions.GetPlayerByID(id);
                PedHash pedHash = NAPI.Util.PedNameToModel(pedModel);
                if (pedHash != 0)
                {
                    target.SetSkin(pedHash);
                    sender.SendChatMessage($"Вы сменили игроку {target.Name} внешность на ({pedModel})");
                }
                else
                {
                    sender.SendChatMessage("Внешности с таким названием не было найдено");
                    return;
                }
            }
        }

        [Command("setdim", "~o~Использовние: ~w~/setdim [id] [dim]")]
        public void ADM_givemoney(Player sender, ushort id, uint dimension)
        {
            if (AdmFunctions.GetAdminLVL(sender) >= 3)
            {
                NAPI.Entity.SetEntityDimension(Functions.GetPlayerByID(id), dimension);
                sender.SendChatMessage("Dimension set to - " + dimension);
            }
        }





        [Command("spawn")]
        public void ADM_spawn(Player sender)
        {
            if (AdmFunctions.GetAdminLVL(sender) >= 3)
            {
                sender.TriggerEvent("client_ScreenFade", 0, 300);
                NAPI.Entity.SetEntityPosition(sender, new Vector3(-40.298965, -1114.0304, 26.147163));
                NAPI.Entity.SetEntityDimension(sender, 0);
            }
        }

        [Command("tp")]
        public void ADM_tp(Player sender, ushort id)
        {
            if (AdmFunctions.GetAdminLVL(sender) >= 3)
            {
                AdmFunctions.TeleportPlayerToTarget(sender, Functions.GetPlayerByID(id));
            }
        }

        [Command("tpveh")]
        public void ADM_tpveh(Player sender, ushort id)
        {
            if (AdmFunctions.GetAdminLVL(sender) >= 3)
            {
                AdmFunctions.TeleportPlayerWithVehicleToTarget(sender, Functions.GetPlayerByID(id));
            }
        }

        [Command("tptome")]
        public void ADM_tptome(Player sender, ushort id)
        {
            if (AdmFunctions.GetAdminLVL(sender) >= 3)
            {
                Player target = Functions.GetPlayerByID(id);
                AdmFunctions.TeleportTargetToPlayer(sender, target);
            }
        }

        [Command("tptomeveh")]
        public void ADM_tptomeveh(Player sender, ushort id)
        {
            if (AdmFunctions.GetAdminLVL(sender) >= 3)
            {
                AdmFunctions.TeleportTargetWithVehicleToPlayer(sender, Functions.GetPlayerByID(id));
            }
        }

        [Command("tpcords")]
        public void ADM_tpcords(Player sender, string cords)
        {
            if (AdmFunctions.GetAdminLVL(sender) >= 3)
            {
                float x, y, z;
                if (cords.Contains(','))
                {
                    cords = cords.Replace(',', ' ');
                    string[] subs = cords.Split(' ');
                    x = float.Parse(subs[0], CultureInfo.InvariantCulture.NumberFormat);
                    y = float.Parse(subs[2], CultureInfo.InvariantCulture.NumberFormat);
                    z = float.Parse(subs[4], CultureInfo.InvariantCulture.NumberFormat);
                    // subs[1] = пробел
                    // subs[3] = пробел
                }
                else
                {
                    string[] subs2 = cords.Split(' ');
                    x = float.Parse(subs2[0], CultureInfo.InvariantCulture.NumberFormat);
                    y = float.Parse(subs2[1], CultureInfo.InvariantCulture.NumberFormat);
                    z = float.Parse(subs2[2], CultureInfo.InvariantCulture.NumberFormat);
                }
                NAPI.Entity.SetEntityPosition(sender, new Vector3(x, y, z));
            }
        }




        [Command("givemoney", "~o~Использовние: ~w~/givemoney [id] [money]")]
        public void ADM_givemoney(Player sender, ushort id, int money)
        {
            if (AdmFunctions.GetAdminLVL(sender) >= 8)
            {
                DBFunctions.GetPlayerInstanceFromDictionary(Functions.GetPlayerByID(id)).cashMoney += money;
            }
        }

        [Command("takemoney", "~o~Использовние: ~w~/takemoney [id] [money]")]
        public void ADM_takemoney(Player sender, ushort id, int money)
        {
            if (AdmFunctions.GetAdminLVL(sender) >= 8)
            {
                DBFunctions.GetPlayerInstanceFromDictionary(Functions.GetPlayerByID(id)).cashMoney -= money;
            }
        }

        [Command("setmoney", "~o~Использовние: ~w~/setmoney [id] [money]")]
        public void ADM_setmoney(Player sender, ushort id, int money)
        {
            if (AdmFunctions.GetAdminLVL(sender) >= 8)
            {
                DBFunctions.GetPlayerInstanceFromDictionary(Functions.GetPlayerByID(id)).cashMoney = money;
            }
        }

        [Command("getmoney", "~o~Использовние: ~w~/getmoney [id]")]
        public void ADM_getmoney(Player sender, ushort id)
        {
            if (AdmFunctions.GetAdminLVL(sender) >= 8)
            {
                sender.SendChatMessage("Количество денег у игрока: " + Convert.ToString(DBFunctions.GetPlayerInstanceFromDictionary(Functions.GetPlayerByID(id)).cashMoney));
            }
        }




        [Command("veh", "~o~Использовние: ~w~/veh [carModelName]")]
        public void ADM_veh(Player sender, string carModelName = "Adder", int color1 = 1, int color2 = 2, string numberPlate = "")
        {
            if (AdmFunctions.GetAdminLVL(sender) >= 3)
            {
                VehicleSystem.CreateAdminVehicle(sender, carModelName, color1, color2, numberPlate);
            }
        }

        [Command("getvehbyid", "~o~Использовние: ~w~/getvehbyid [vehId]")]
        public void ADM_getvehbyid(Player sender, ushort vehId)
        {
            if (AdmFunctions.GetAdminLVL(sender) >= 3)
            {
                Vehicle vehicle = sender.Vehicle;
                foreach (Vehicle vehToFind in NAPI.Pools.GetAllVehicles())
                {
                    if (vehToFind.Id == vehId)
                    {
                        NAPI.Entity.SetEntityPosition(vehToFind, sender.Position);
                        NAPI.Entity.SetEntityRotation(vehToFind, sender.Rotation);
                        NAPI.Entity.SetEntityDimension(vehToFind, sender.Dimension);
                        sender.SendChatMessage($"Вы телепортировли тс с ID: {vehId}");
                    }
                    else
                    {
                        sender.SendChatMessage($"Тс с ID: {vehId} не найдено");
                    }
                }
            }
        }

        [Command("dveh")]
        public void ADM_dveh(Player sender)
        {
            if (AdmFunctions.GetAdminLVL(sender) >= 3)
            {
                if (sender.IsInVehicle)
                {
                    Vehicle vehicle = sender.Vehicle;
                    if (vehicle.GetData<string>("СREATED_BY") == sender.SocialClubName)
                    {
                        vehicle.Delete();
                        sender.SendChatMessage($"Вы удалили созданное вами тс.");
                    }
                }
                else if (sender.GetData<Vehicle>("LAST_VEHICLE").GetData<string>("СREATED_BY") == sender.SocialClubName)
                {
                    sender.GetData<Vehicle>("LAST_VEHICLE").Delete();
                }
                else
                {
                    sender.SendChatMessage($"{Colors.RED}Не обнаружена последняя созданная машина, сядьте в неё для удаления, либо удалите все свои машины.");
                }
            }
        }

        [Command("dvehs")]
        public void ADM_dvehs(Player sender)
        {
            if (AdmFunctions.GetAdminLVL(sender) >= 3)
            {
                Vehicle vehicle = sender.Vehicle;
                int deletedCount = 0;
                foreach (Vehicle vehToDel in NAPI.Pools.GetAllVehicles())
                {
                    if (vehToDel.GetData<string>("СREATED_BY") == DBFunctions.GetPlayerInstanceFromDictionary(sender).SocialClubName)
                    {
                        deletedCount++;
                        vehToDel.Delete();
                    }
                }
                sender.SendChatMessage($"Вы удалили {deletedCount} созданное вами тс.");
            }
        }

        [Command("deladmvehs")]
        public void ADM_deladmvehs(Player sender)
        {
            if (AdmFunctions.GetAdminLVL(sender) >= 3)
            {
                Vehicle vehicle = sender.Vehicle;
                int deletedCount = 0;
                foreach (Vehicle vehToDel in NAPI.Pools.GetAllVehicles())
                {
                    if (vehToDel.GetData<string>("CAR_ACCESS") == "ADMIN")
                    {
                        deletedCount++;
                        vehToDel.Delete();
                    }
                }
                sender.SendChatMessage($"Вы удалили {deletedCount} созданное всеми админами тс.");
            }
        }




        [Command("ch")]
        public void ADM_CreateNewHouse(Player sender, string type, int price = 10, bool withGarage = true)
        {
            if (AdmFunctions.GetAdminLVL(sender) >= 8)
            {
                HouseSystem.CreateNewHouse(sender, type, price, withGarage);
            }
        }

        [Command("cg")]
        public void ADM_CreateNewGarage(Player sender, int houseid, int size)
        {
            if (AdmFunctions.GetAdminLVL(sender) >= 8)
            {
                GarageSystem.CreateNewGarage(sender, houseid, size);
            }
        }

        [Command("ca")]
        public void ADM_CreateNewAutoroom(Player sender, string type, int price = 10)
        {
            if (AdmFunctions.GetAdminLVL(sender) >= 8)
            {
                Autorooms.CreateNewAutoroom(sender, type, price);
            }
        }





        #endregion Admin Comands///////////





        #region Players Commands
        [Command("money")]
        public void Money(Player sender)
        {
            sender.SendChatMessage(Convert.ToString(DBFunctions.GetPlayerInstanceFromDictionary(sender).cashMoney));
        }

        [Command("me", "~o~Использовние: ~w~/me *действие/состояние*", GreedyArg = true)]
        public void Me(Player sender, string message)
        {
            // Функция берет всех ближайших игроков в радиусе и отправляет им сообщение
            // Передаю форматированный под /me месседж
            Functions.SendChatMessageToPlayersInRadius(sender, 10, $"{Colors.MAGENTA}{sender.Name} {message}");
        }

        [Command("q")]
        public void Quit(Player sender)
        {
            sender.SendChatMessage($"{Colors.GOLD}Как жаль,что вы покидаете нас. Но мы всегда будем ждать вашего возвращения!");
            NAPI.Task.Run(() =>
            {
                sender.Kick();
            }, delayTime: 200);
        }



        #region HOUSE,GARAGE,SHOPS,AUTOROOMS
        //house
        [Command("bh")]
        public void BuyHouse(Player sender)
        {
            HouseSystem.BuyHouse(sender);
        }

        [Command("sh")]
        public void SellHouse(Player sender)
        {
            HouseSystem.SellHouse(sender);
        }

        [Command("lh")]
        public void OpenHouse(Player sender)
        {
            HouseSystem.OpenHouse(sender);
        }

        [Command("addroommate")]
        public void AddRoommate(Player sender, ushort id)
        {
            HouseSystem.AddRoommate(sender, id);
        }

        [Command("kickroommate")]
        public void RemoveRoommate(Player sender, ushort id)
        {
            HouseSystem.RemoveRoommate(sender, id);
        }
        //house



        //autoroom
        [Command("bcar")]
        public void BuyCar(Player sender, string carModel)
        {
            Autorooms.BuyCar(sender, carModel);
        }

        //дописать команду на продажу машины

        [Command("ba")]
        public void BuyAutoroom(Player sender)
        {
            Autorooms.BuyAutoroom(sender);
        }

        [Command("sa")]
        public void SellAutoroom(Player sender)
        {
            Autorooms.SellAutoroom(sender);
        }
        //autoroom

        #endregion HOUSE,GARAGE,SHOPS,AUTOROOMS


        #endregion Players Commands
    }
}
