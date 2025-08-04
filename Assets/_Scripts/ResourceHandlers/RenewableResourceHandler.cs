using System.Collections;
using UnityEngine;

public class RenewableResourceHandler : MonoBehaviour
{
    [SerializeField] ResourceHandler ResourceToSpawn;
    [SerializeField] Transform ObjectToTurnOff;
    [SerializeField] float TimeToTurnOff = 3f;
    [SerializeField] ResourceHandler spawnedResource;
    public bool readyToCollect;
    private void OnEnable()
    {
        readyToCollect = true;
    }
    public void CreateResource()
    {
        spawnedResource = Instantiate(ResourceToSpawn, transform.position, Quaternion.identity);
        StartCoroutine(TurnOffAndOn());
    }
    IEnumerator TurnOffAndOn()
    {
        ObjectToTurnOff.gameObject.SetActive(false);
        readyToCollect = false;
        yield return new WaitForSeconds(TimeToTurnOff);
        yield return new WaitUntil(() => Vector3.Distance(spawnedResource.transform.position, transform.position) > 0.1f);
        ObjectToTurnOff.gameObject.SetActive(true);
        readyToCollect = true;
    }
}
