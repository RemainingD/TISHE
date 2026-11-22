using UnityEngine;
namespace Tishe {
public sealed class NurseryMobile : MonoBehaviour {
    void Update(){
        var game=GameController.Instance;
        if(game==null || game.Paused || (game.Player.transform.position-transform.position).sqrMagnitude>81)return;
        float t=Time.time;
        transform.localRotation=Quaternion.Euler(1.8f*Mathf.Sin(t*.58f),24*Mathf.Sin(t*.31f),2.3f*Mathf.Sin(t*.42f));
    }
}
}
