using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FrostweepGames.VoicePro.NetworkProviders.Mirror;
using FrostweepGames.VoicePro;

public class PlayerNetworkManager : NetworkManager
{
    public string playerEmail { get; set; }
    public bool isServerHost;

    public override void Start()
    {
        if (isServerHost)
        {
            playerEmail = "server@";
            StartHost();
        }
    }

    public override void OnStopClient()
    {
        if (mode == NetworkManagerMode.Offline)
            return;
        //Debug.Log(NetworkClient.connection.identity.gameObject.name);
       // Debug.Log(NetworkClient.localPlayer.gameObject.name);
      // NetworkClient.localPlayer.gameObject.GetComponent<IL3DN.ServerPlayer>().SetPlayerExist();

    }
    public override void OnStartHost()
    {
        base.OnStartHost();
        NetworkServer.RegisterHandler<TransportVoiceMessage>(NetworkEventReceivedHandler);

        Debug.Log("OnStartHost");
    }

    public override void OnStopHost()
    {
        base.OnStopHost();
        NetworkServer.UnregisterHandler<TransportVoiceMessage>();

        Debug.Log("OnStopHost");
    }

    private void NetworkEventReceivedHandler(NetworkConnection connection, TransportVoiceMessage message)
    {
        NetworkServer.SendToReady(message);
    }


    public struct CreatePlayerMessage : NetworkMessage
    {
        public string email;
    }

    public override void OnStartServer()
    {
        Debug.Log("OnStartServer");
        base.OnStartServer();
        NetworkServer.RegisterHandler<CreatePlayerMessage>(OnCreatePlayer);
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);

        Debug.Log("playerEmail" + playerEmail);
        // tell the server to create a player with this name
        conn.Send(new CreatePlayerMessage { email = playerEmail });
    }

    void OnCreatePlayer(NetworkConnection connection, CreatePlayerMessage createPlayerMessage)
    {
        Debug.Log("player");
        GameObject player = Instantiate(playerPrefab);
        player.name = createPlayerMessage.email;
        //player.GetComponent<ServerPlayer>().playerEmail = createPlayerMessage.email;
        NetworkServer.AddPlayerForConnection(connection, player);
    }
}
