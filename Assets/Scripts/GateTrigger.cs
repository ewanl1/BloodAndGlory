using UnityEngine;

public class GateTrigger : MonoBehaviour
{
    public GateOpen door;

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == door.playerTag) door.Open();
    }
    void OnTriggerExit(Collider other)
    {
        if (other.tag == door.playerTag) door.Close();
    }
}
