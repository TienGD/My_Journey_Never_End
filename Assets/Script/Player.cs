using UnityEngine;
using UnityEngine.InputSystem;
public class Player : MonoBehaviour
{
    // vùng khai báo biến
    [SerializeField] float gravity = 400f; // trọng lực
    public Vector2 velocity; // vận tốc

    
    public float distance { get; private set; } // quãng đường

    [SerializeField] float maxXVelocity = 100f; // vận tốc theo chiêu ngang tối đa
    [SerializeField] float maxAcceleration = 10f; // gia tốc tối đa
    [SerializeField] float acceleration; // gia tốc


    [SerializeField] float jumpVelociy = 20f; // vận tốc lúc nhảy lên
    [SerializeField] float groundHeight = 10f; // chiều cao mặt đất
    [SerializeField] bool isGrounded = false; // check xem người chơi có đang ở trên mặt đất không
    [SerializeField] bool isHoldingJump = true; // check người chơi có đang giữ space hay ko;
    [SerializeField] float maxHoldJumpTime = 0.4f; // thời gian tối đa giữ phím space
    [SerializeField] float holdJumpTimer = 0;// Bộ đếm thời gian đã giữ phím nhảy
    [SerializeField] float jumpGroundThreshoud = 1;// Khoảng cách du di cho phép nhảy sớm trước khi chạm đất

    

    // Xử lý thao tác player
    void Update()
    {

        Vector2 pos = transform.position;
        // lấy vị trí hiện tại
        float groundDistance = Mathf.Abs(pos.y - groundHeight);
        //tính khoảng cách của nhân vật hiện tại so với mặt đất

        if (isGrounded || groundDistance <= jumpGroundThreshoud)
        {
            // ĐIỀU KIỆN NHẢY: Đang chạm đất HOẶC đang cách mặt đất một khoảng cực nhỏ (Threshold)
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                // Nếu người chơi VỪA BẤM phím Space trong frame này
                isGrounded = false; // Hủy trạng thái chạm đất (bắt đầu bay lên)
                velocity.y = jumpVelociy; // Truyền vận tốc nhảy hướng lên trên
                isHoldingJump = true; // Bật cờ ghi nhận đang giữ phím

            }

        }

        if (Keyboard.current.spaceKey.wasReleasedThisFrame)
        {
            // Nếu người chơi VỪA THẢ phím Space ra trong frame này
            isHoldingJump = false; // Hủy trạng thái giữ phím nhảy
        }

    }

    //Xử lý logic vật lý
    private void FixedUpdate()
    {
        Vector2 pos = transform.position;
        // lấy vị trí hiện tại

        if (!isGrounded)
        {
            // Chỉ xử lý lực rơi/nhảy khi nhân vật KHÔNG chạm đất (đang trên không)
            if (isHoldingJump)
            {
                // 1. KIỂM TRA THỜI GIAN GIỮ PHÍM NHẢY CAO
                holdJumpTimer += Time.deltaTime;
                // Tăng bộ đếm thời gian.
                if (holdJumpTimer > maxHoldJumpTime)
                {
                    // Nếu giữ phím vượt quá thời gian cho phép
                    isHoldingJump = false;
                    // Tự động ép ngắt trạng thái giữ phím
                }
            }

            pos.y += velocity.y * Time.fixedDeltaTime;
            // 2. DI CHUYỂN
            // Cộng vận tốc hiện tại vào tọa độ Y để nhân vật bay lên/rơi xuống

            if (!isHoldingJump)
            {
                // 3. ÁP DỤNG TRỌNG LỰC (RƠI)
                // Ngay khi thả phím (hoặc giữ quá lâu bị ngắt), trọng lực bắt đầu kéo nhân vật xuống
                velocity.y += gravity * Time.fixedDeltaTime;
            }


            if (pos.y < groundHeight)
            {
                // 4. XỬ LÝ CHẠM ĐẤT
                pos.y = groundHeight; // Khóa chặt tọa độ Y bằng với mặt đất (không cho lọt nền)
                isGrounded = true; // Đánh dấu là đã chạm đất an toàn
                holdJumpTimer = 0f; // Reset lại bộ đếm thời gian cho cú nhảy tiếp theo
            }
        }

        distance += velocity.x * Time.fixedDeltaTime;

        if (isGrounded)
        {
            float velocityRatio = velocity.x / maxXVelocity;
            acceleration = maxAcceleration* (1- velocityRatio);

            velocity.x += acceleration * Time.deltaTime;

            if (velocity.x >= maxXVelocity)
            {
                velocity.x= maxXVelocity;
            }

        }
        // Cập nhật tọa độ thực tế của GameObject trong Unity bằng vị trí vừa tính toán xong
        transform.position = pos;

    }
}
