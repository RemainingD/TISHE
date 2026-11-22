using UnityEngine;
namespace Tishe {
public class GameItem : MonoBehaviour {
    public string kind, title, id;
    public int taskIndex=-1;
    public void Use() { GameController.Instance.Interact(this); }
}
}
