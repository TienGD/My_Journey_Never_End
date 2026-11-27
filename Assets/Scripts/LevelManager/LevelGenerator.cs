using UnityEngine;
using UnityEngine.Serialization;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] private Transform[] levelPart;

    [FormerlySerializedAs("nextPartPosititon")]
    [SerializeField] private Vector3 nextPartPosition;

    [SerializeField] private float distanceToSpawn;
    [SerializeField] private float distanceToDelete;
    [SerializeField] private Transform player;

    private void Update()
    {
        if (player == null) return;

        DeletePlatform();
        GeneratePlatform();
    }

    private void GeneratePlatform()
    {
        if (levelPart == null || levelPart.Length == 0) return;

        if (Vector2.Distance(player.position, nextPartPosition) < distanceToSpawn)
        {
            Transform part = levelPart[Random.Range(0, levelPart.Length)];
            if (part == null) return;

            Transform startPoint = part.Find("StartPoint");
            float startOffsetX = startPoint != null ? startPoint.position.x : 0f;

            Vector2 newPosition = new Vector2(nextPartPosition.x - startOffsetX, 0f);
            Transform newPart = Instantiate(part, newPosition, transform.rotation, transform);

            Transform endPoint = newPart.Find("EndPoint");
            if (endPoint != null)
            {
                nextPartPosition = endPoint.position;
            }
        }
    }

    private void DeletePlatform()
    {
        if (transform.childCount == 0) return;

        Transform partToDelete = transform.GetChild(0);
        if (Vector2.Distance(player.position, partToDelete.position) > distanceToDelete)
        {
            Destroy(partToDelete.gameObject);
        }
    }
}
