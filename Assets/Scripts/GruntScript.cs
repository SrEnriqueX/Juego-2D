using UnityEngine;

public class GruntScript : MonoBehaviour
{
    // Configuración pública
    public GameObject bulletPrefab;
    public GameObject John;

    // Parámetros ajustables (podrían hacerse públicos)
    private float shootCooldown = 0.25f;
    private float shootRange = 1.0f;
    private int maxHealth = 3;

    // Estado interno
    private float lastShootTime;
    private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth; // Inicialización explícita
    }

    void Update()
    {
        if (John == null) return;

        UpdateFacingDirection();
        TryShoot();
    }

    private void UpdateFacingDirection()
    {
        Vector3 direction = John.transform.position - transform.position;
        transform.localScale = new Vector3(
            direction.x >= 0 ? 1f : -1f,
            1f,
            1f
        );
    }

    private void TryShoot()
    {
        float distance = Mathf.Abs(John.transform.position.x - transform.position.x);

        if (distance < shootRange && Time.time > lastShootTime + shootCooldown)
        {
            Shoot();
            lastShootTime = Time.time;
        }
    }

    private void Shoot()
    {
        Vector3 direction = transform.localScale.x > 0 ? Vector3.right : Vector3.left;
        GameObject bullet = Instantiate(
            bulletPrefab,
            transform.position + direction * 0.1f,
            Quaternion.identity
        );
        bullet.GetComponent<BulletScript>().SetDirection(direction);
    }

    public void Hit()
    {
        currentHealth--;

        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }

}
