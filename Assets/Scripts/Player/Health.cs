using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] int healthPont;

    public int HealthPont { get => healthPont; set => healthPont = value; }

    public void TakeDamage(int Damage)
    {

        HealthPont-=Damage;
        Debug.Log($"the player take {Damage}");

    }


}
