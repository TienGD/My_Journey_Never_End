using UnityEngine;

public class Parralax : MonoBehaviour
{
    #region parallaxRightToLeft;
    [SerializeField] float depth = 1f;
    public Player player;
    public float limitDistance = -30f;
    public float newDistance = 80f;

    private void Awake()
    {
        player = GameObject.Find("Player").GetComponent<Player>();
    }

    // Đổi từ FixedUpdate sang LateUpdate để tránh giật hình
    private void LateUpdate()
    {
        // Bây giờ player.velocity.x đã có giá trị lớn hơn 0
        float realVelocity = player.velocity.x / depth;

        Vector2 pos = transform.position;

        // Dùng Time.deltaTime thay vì fixedDeltaTime vì ta đang ở LateUpdate
        pos.x -= realVelocity * Time.deltaTime;

        if (pos.x <= limitDistance)
        {
            pos.x = newDistance;
        }

        transform.position = pos;
    }
    #endregion



}