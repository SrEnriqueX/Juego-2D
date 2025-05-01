using System.Collections;
using UnityEngine;

public class GruntScript : MonoBehaviour
{
    
    public GameObject bulletPrefab;
    public GameObject John;
    public Animator animator;

    private float shootCooldown = 0.25f;
    private float shootRange = 1.0f;
    private int maxHealth = 3;
    public float hurtAnimationDuration = 0.5f;


    private float lastShootTime;
    private int currentHealth;
    private bool isHurting = false;
    private SpriteRenderer spriteRenderer;



    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (animator == null) animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (John == null || isHurting) return;

        UpdateFacingDirection();
        TryShoot();
    }

    private void UpdateFacingDirection()
    {
        Vector3 direction = John.transform.position - transform.position;
        transform.localScale = new Vector3(
            direction.x >= 0 ? 1f : -1f,1f,1f
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
        if (isHurting) return;

        currentHealth--;
        StartCoroutine(PlayHurtAnimation());

        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }
    private IEnumerator PlayHurtAnimation()
    {
        isHurting = true;

     
        animator.Play("Hurt", 0, 0f);

     
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = Color.red;

     
        float originalSpeed = shootCooldown;
        shootCooldown = Mathf.Infinity; 

        
        yield return new WaitForSeconds(hurtAnimationDuration);
     
        spriteRenderer.color = originalColor;
        shootCooldown = originalSpeed;
        isHurting = false;

        animator.Play("GruntAnimator", 0, 0f);
    }
}

