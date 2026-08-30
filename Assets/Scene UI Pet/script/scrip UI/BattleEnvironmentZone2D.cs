using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BattleEnvironmentZone2D : MonoBehaviour
{
    [Header("Loại môi trường của khu vực")]
    [SerializeField]
    private BattleEnvironmentType environmentType =
        BattleEnvironmentType.Fire;

    private void Reset()
    {
        Collider2D zoneCollider =
            GetComponent<Collider2D>();

        if (zoneCollider != null)
        {
            zoneCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        SetEnvironment(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        SetEnvironment(other);
    }

    private void SetEnvironment(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        BattleEnvironmentState.SetEnvironment(
            environmentType
        );
    }
}