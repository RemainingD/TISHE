using UnityEngine;
namespace Tishe {
public class NoiseImpact : MonoBehaviour {
    public bool armed=true;
    void OnCollisionEnter(Collision c){if(armed && c.relativeVelocity.magnitude>.7f){armed=false;GameController.Instance.Noise(transform.position,16,"drop");}}
}
}
