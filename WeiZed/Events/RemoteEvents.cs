using GTANetworkAPI;

namespace WeiZed.Events
{
    class RemoteEvents : Script
    {
        [RemoteEvent("server_PlayerSetData")]
        public void PlayerSetData(Player player, string type, object data)
        {
            player.SetData(type, data);
        }


    }
}
