using GTANetworkAPI;

namespace ClientSide
{
    public class CarMenu : Script
    {


        [RemoteEvent("CmenuFix")]
        public void OnCmenuFix(Player player)
        {
            Vehicle car = NAPI.Player.GetPlayerVehicle(player);
            NAPI.Vehicle.RepairVehicle(car);
        }


        [RemoteEvent("CmenuOkay")]
        public void OnCmenuOkay(Player player)
        {
            NAPI.ClientEvent.TriggerClientEvent(player, "cmenuDone");
        }

        [RemoteEvent("CmenuColor")]
        public void OnCmenuColor(Player player, int red, int green, int blue)
        {
            Vehicle car = NAPI.Player.GetPlayerVehicle(player);
            NAPI.Vehicle.SetVehicleCustomPrimaryColor(car, red, green, blue);
            NAPI.Vehicle.SetVehicleCustomSecondaryColor(car, red, green, blue);
        }



        [Command("cursor")]
        public void cmenuCursor(Player player)
        {
            NAPI.ClientEvent.TriggerClientEvent(player, "cmenuCursor");
        }

        [Command("cmenu")]
        public void cmenuCommand(Player player)
        {
            if (!NAPI.Player.IsPlayerInAnyVehicle(player))
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "[ERROR]: Not in any vehicle!");
            }
            else
            {
                NAPI.ClientEvent.TriggerClientEvent(player, "cmenuActive");
            }

        }
    }
}
