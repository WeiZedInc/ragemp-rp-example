using GTANetworkAPI;
using WeiZed.Core;
using WeiZed.Systems;
using System;

namespace WeiZed.Events
{
    class KeyAPIServerSide : Script
    {
        Random random = new Random();


        [RemoteEvent("server_SwitchColshapeTypeAndExecuteInteractions")]
        public void SwitchColshapeTypeAndExecuteInteractions(Player player, string type)
        {
            switch (type) // colshapeType
            {
                case "house":
                    HouseSystem.EnterHouse(player);
                    break;
                case "INSIDE_HOUSE":
                    HouseSystem.ExitHouse(player);
                    break;
                //
                case "garage":
                    GarageSystem.EnterGarage(player);
                    break;
                case "INSIDE_GARAGE":
                    GarageSystem.ExitGarage(player);
                    break;
                //
                case "autoroom":
                    Autorooms.EnterAutoroom(player);
                    break;
                case "INSIDE_AUTOROOM":
                    Autorooms.ExitAutoroom(player);
                    break;
                //
                case "oilcompany":
                    Autorooms.EnterAutoroom(player);
                    break;
                case "INSIDE_OILCOMPANY":
                    Autorooms.ExitAutoroom(player);
                    break;

                //Trucker
                case "TRUCK_GIVER_NPC":
                    Jobs.Truckers.SpawnJobVehicle(player);
                    break;
                //
                case "Factory_BARREL_GIVER_NPC":
                    Jobs.Truckers.SpawnFactoryJobTrailerAndGetMission(player, new Vector3(2675.3857, 1450.5676, 24.500807), new Vector3(0, 0, -3.483164));
                    break;
                //
                case "OilLS_BARREL_GIVER_NPC":
                    Jobs.Truckers.SpawnOilJobTrailerAndGetMission(player);
                    break;
                //
                case "FuelLS_BARREL_GIVER_NPC":
                    Jobs.Truckers.SpawnFuelJobTrailerAndGetMission(player);
                    break;




                default:
                    player.SendChatMessage("Err in remote event");
                    break;
            }
        }
    }
}
