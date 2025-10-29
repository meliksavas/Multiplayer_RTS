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
        PhotonNetwork.Instantiate("Player",new Vector3(Random.Range(0,10), Random.Range(0,10), 0), Quaternion.identity);
    }
}
