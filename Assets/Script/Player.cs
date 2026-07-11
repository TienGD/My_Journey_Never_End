using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] float speed = 4.5f;
    [SerializeField] float acceleration = 1.2f;
    public Vector2 velocity;
    [SerializeField] Rigidbody2D rb;
    public delegate void PowerUpCollectedEventHandler();
    public event PowerUpCollectedEventHandler PowerUpCollected;
    [SerializeField] Sprite classicForm;
    [SerializeField] Sprite superForm;
    [SerializeField] SpriteRenderer render;



    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        render = GetComponent<SpriteRenderer>();
        
    }

    private void Start()
    {
        PowerUpCollected += PowerUpTransform;
    }

    private void Update()
    {
        // Tăng tốc độ theo thời gian
        speed += acceleration * Time.deltaTime;

        // CẬP NHẬT VẬN TỐC Ở ĐÂY: Vận tốc = Hướng đi * Tốc độ
        velocity = Vector2.right * speed;

        //// Di chuyển nhân vật dựa trên velocity đã tính toán
        //transform.Translate(velocity * Time.deltaTime);       
    }

    private void OnTriggerEnter2D(Collider2D upgradeItems)
    {
        if (upgradeItems.CompareTag("PowerUp"))
        {
            OnPowerUpCollected();
            Destroy(upgradeItems);
        }
    }

    protected virtual void OnPowerUpCollected()
    {
        PowerUpCollected?.Invoke();
    }

    void PowerUpTransform()
    {
        
        if(render != null)
        {
            
        }
    }
    



}