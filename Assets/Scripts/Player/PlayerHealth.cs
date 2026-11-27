using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    #region Fields & Properties
    [Header("Health Settings")]
    [Tooltip("Lượng máu tối đa của nhân vật")]
    [SerializeField] private int maxHealth = 3;
    [Tooltip("Lượng máu hiện tại (theo dõi trong Inspector)")]
    [SerializeField] private int currentHealth;
    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public bool IsDead => currentHealth <= 0;
    public bool IsFullHealth => currentHealth >= maxHealth;
    #endregion
    #region Unity Lifecycle
    private void Awake()
    {
        currentHealth = maxHealth;
    }
    #endregion
    #region Health Logic (Damage, Heal, Reset)
    /// Trừ máu khi va chạm. Trả về true nếu nhân vật bị trừ máu.
    public bool TakeDamage(int damageAmount = 1)
    {
        if (IsDead || damageAmount <= 0)
        {
            return false;
        }
        int previousHealth = currentHealth;
        currentHealth = Mathf.Max(currentHealth - damageAmount, 0);
        int lostHealth = previousHealth - currentHealth;
        Debug.Log($"[PlayerHealth] Mất {lostHealth} máu! Máu còn lại: {currentHealth}/{maxHealth}");
        if (IsDead)
        {
            Debug.Log("[PlayerHealth] Nhân vật đã hết máu!");
        }
        return true;
    }

    /// Hồi máu cho nhân vật. Trả về true nếu hồi máu thành công.
    //public bool Heal(int healAmount = 1)
    //{
    //    if (IsDead || IsFullHealth || healAmount <= 0)
    //    {
    //        return false;
    //    }
    //    int previousHealth = currentHealth;
    //    currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
    //    int actualHealed = currentHealth - previousHealth;
    //    Debug.Log($"[PlayerHealth] Hồi {actualHealed} máu! Máu hiện tại: {currentHealth}/{maxHealth}");
    //    return true;
    //}
    
    /// Đưa máu về 0 ngay lập tức.
    public void Kill()
    {
        if (IsDead) return;
        TakeDamage(currentHealth);
    }
    /// <summary>
    /// Khôi phục lại đầy máu.
    /// </summary>
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        Debug.Log($"[PlayerHealth] Đã hồi phục toàn bộ máu: {currentHealth}/{maxHealth}");
    }
    #endregion
}
