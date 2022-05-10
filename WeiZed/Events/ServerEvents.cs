using GTANetworkAPI;
using WeiZed.Bussines;
using WeiZed.Jobs;
using WeiZed.Systems;
using System;

namespace WeiZed.Core
{
    class ServerEvents : Script
    {
        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart()
        {
            NAPI.Server.SetGlobalServerChat(false);
            NAPI.Server.SetAutoRespawnAfterDeath(false);
            NAPI.Server.SetAutoSpawnOnConnect(false);
            NAPI.Server.SetCommandErrorMessage($"{Colors.ORANGE}Такой команды несуществует X﹏X");

            Colors.ColoredConsoleOutput("------------------------------НАЧАТА ИНИЦИАЛИЗАЦИЯ МОДА!---------------------------------", ConsoleColor.Red);
            GarageSystem.LoadGaragesFromDB();
            HouseSystem.LoadHousesFromDB();
            VehicleSystem.LoadAllNumberPlatesFromBD();
            Autorooms.LoadAutoroomsFromBD();
            OilCompanyBase.LoadOilCompaniesFromBD();
            Truckers.Init();
            Colors.ColoredConsoleOutput("------------------------------КОНЕЦ ИНИЦИАЛИЗАЦИИ МОДА!----------------------------------", ConsoleColor.Red);
        }

        [ServerEvent(Event.ChatMessage)] // Кастомный чат
        public void OnChatMessage(Player player, string message)
        {
            Functions.RPChat(player, $"{Functions.GetRPName(player)}[{player.Id}] сказал: {message}", 15, $"{Colors.WHITE}");
        }
    }
}

