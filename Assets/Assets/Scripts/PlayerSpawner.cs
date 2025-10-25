using UnityEngine;
using Photon.Pun;

public class PlayerSpawner : MonoBehaviourPunCallbacks
{

    /*
    public override void OnJoinedRoom()
    {
        PhotonNetwork.Instantiate("Player",Vector3.zero, Quaternion.identity);
    }
    */
    private void Start()
    {
        Debug.Log("player spawned");
        PhotonNetwork.Instantiate("Player",Vector3.zero, Quaternion.identity);
    }
}
