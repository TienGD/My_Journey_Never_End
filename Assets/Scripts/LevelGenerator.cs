using UnityEngine;
using UnityEngine.InputSystem;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] private Transform[] levelPart;
    [SerializeField] private Vector3 nextPartPosititon;

    [SerializeField] float distanceToSpawn;
    [SerializeField] float distanceToDelete;
    [SerializeField] Transform player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    // Update is called once per frame
    void Update()
    {
        DeletePlatform();
        GeneratePlatform();
    }

    private void GeneratePlatform()
    {
        while (Vector2.Distance(player.transform.position,nextPartPosititon)<distanceToSpawn)
        {
            Transform part = levelPart[Random.Range(0, levelPart.Length)];

            Vector2 newPositon = new Vector2(nextPartPosititon.x - part.Find("StartPoint").position.x, 0);

            Transform newPart = Instantiate(part, newPositon, transform.rotation, transform);
            nextPartPosititon = newPart.Find("EndPoint").position;
        }
    }
    private void DeletePlatform()
    {
        if (transform.childCount > 0)
        {
            Transform partToDelete = transform.GetChild(0);
            if (Vector2.Distance(player.transform.position, partToDelete.transform.position) > distanceToDelete)
            {
                Destroy(partToDelete.gameObject);
            }
        }
    }
}
