using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using System.Collections.Generic;

public class RoomList : MonoBehaviourPunCallbacks
{
    public GameObject RoomPrefab; // Unity UI Prefab'ý (Button)
    public Transform contentParent; // Room objelerinin dizileceði alan (Content veya bu objenin kendisi)

    // ÖNEMLÝ: Lobiye girdiðimizde listeyi temizleyelim
    public override void OnJoinedLobby()
    {
        Debug.Log("Lobiye Katýlýndý.");
        // Lobiye ilk giriþte liste boþ gelir, Update beklenecek.
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("Oda listesi güncelleniyor. Oda sayýsý: " + roomList.Count);

        // Önce listedeki eski butonlarý temizle
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // Yeni listeyi oluþtur
        for (int i = 0; i < roomList.Count; i++)
        {
            // Kapanmamýþ, görünür ve silinmemiþ odalarý göster
            if (roomList[i].IsOpen && roomList[i].IsVisible && !roomList[i].RemovedFromList)
            {
                // PhotonNetwork.Instantiate YERÝNE normal Instantiate kullanýyoruz:
                GameObject room = Instantiate(RoomPrefab, contentParent);

                // Ýsim atamasý
                room.GetComponent<Room>().Name.text = roomList[i].Name;
            }
        }
    }
}