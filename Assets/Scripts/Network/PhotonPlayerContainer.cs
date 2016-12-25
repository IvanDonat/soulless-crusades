using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotonPlayerContainer : Photon.PunBehaviour {
    private PhotonPlayer pp;

    public void Set(PhotonPlayer photonPlayer)
    {
        pp = photonPlayer;
    }

    public PhotonPlayer Get()
    {
        return pp;
    }
}
