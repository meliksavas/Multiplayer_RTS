using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Room : MonoBehaviour
{
    public TMP_Text Name;

    public void JoinRoom() 
    {
        GameObject.Find("RoomManager").GetComponent<RoomManager>().JoinRoomInList(Name.text);
    }
}
