using System.Collections;
using UnityEngine;

public class RenewableResourceHandler : MonoBehaviour
{
    [SerializeField] ResourceHandler ResourceToSpawn;
    [SerializeField] Transform ObjectToTurnOff;
    [SerializeField] float TimeToTurnOff = 3f;
    [SerializeField] ResourceHandler spawnedResource;
    [SerializeField] Collider collider;
    [SerializeField] Transform spawnPosition;
    public bool readyToCollect;
    private void OnEnable()
    {
        readyToCollect = true;
    }
    public void CreateResource()
    {
        spawnedResource = Instantiate(ResourceToSpawn, spawnPosition.position, Quaternion.identity);
        StartCoroutine(TurnOffAndOn());
    }
    IEnumerator TurnOffAndOn()
    {
        var mask = collider.excludeLayers;
        collider.excludeLayers = -1;
        yield return new WaitForFixedUpdate();
        ObjectToTurnOff.gameObject.SetActive(false);
        readyToCollect = false;
        yield return new WaitForSeconds(TimeToTurnOff);
        yield return new WaitUntil(() => Vector3.Distance(spawnedResource.transform.position, spawnPosition.position) > 1f);
        collider.excludeLayers = 0;
        ObjectToTurnOff.gameObject.SetActive(true);
        readyToCollect = true;
    }
}
