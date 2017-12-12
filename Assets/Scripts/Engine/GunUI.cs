using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GunUI : MonoBehaviour 
{
    public UnityEngine.UI.Text Name;
    public UnityEngine.UI.Text Ammo;
    private Gun GunComponent;

    void Awake() {
        GunComponent = GetComponent<Gun>();
    }

	void Update ()
    {
        string GunName = transform.gameObject.name;
        string AmmoText = (GunComponent.CurrentMagSize + "/" + GunComponent.Ammo);
        Name.text = GunName;
        Ammo.text = AmmoText;
	}
}
