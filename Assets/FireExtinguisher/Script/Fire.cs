using UnityEngine;

public class Fire : MonoBehaviour
{
    [Tooltip("The maximum value of health")]
    [SerializeField] private float maxHealth = 100f;

    [Tooltip("How much health per second the fire gains when not extinguished")]
    [SerializeField] private float healthIncreasePerSec = 5f;

    [Tooltip("What scale to apply according to the health")]
    [SerializeField] private float scalePerHealth = 0.01f;

    [SerializeField] private float health;
    public float Health
    {
        get { return health; }
        set
        {
            health = Mathf.Clamp(value, 0f, maxHealth);
            UpdateHealth();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // Start at full health and apply the initial scale / activation
        Health = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        // Gain health if the fire is not fully extinguished
        // (an inactive GameObject never runs Update, so a dead fire stays dead)
        if (health > 0f && health < maxHealth)
        {
            Health += healthIncreasePerSec * Time.deltaTime;
        }
    }

    void UpdateHealth()
    {
        // Activate/deactivate gameObject depending on health
        gameObject.SetActive(health > 0f);

        // Update scale according to health
        transform.localScale = Vector3.one * (health * scalePerHealth);
    }
}