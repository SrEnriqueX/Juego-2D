using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NewMonoBehaviourScript : MonoBehaviour
{
    // ===== Variables públicas =====
    public GameObject bulletPrefab;
    public float Speed;
    public float JumForce;
    public AudioClip soundJump;
    public float deathY = -10f;
    public Vector3 initialPosition;

    // ===== Variables privadas =====
    private Rigidbody2D Rigidbody2D;
    private Animator Animator;
    private float Horizontal;
    private bool Grounded;
    private float LastShoot;
    private int Health = 5;
    private int currentLives = 3;
    private bool isDead = false;
    private bool isHurting = false;
    private bool wasGrounded = true;

    // ===== Métodos de Unity =====
    void Start()
    {
        Rigidbody2D = GetComponent<Rigidbody2D>();
        Animator = GetComponent<Animator>();
        initialPosition = transform.position;
        Debug.Log("VIDAS INICIALES: " + currentLives);
    }

    void Update()
    {
        HandleMovementInput();
        HandleShootingInput();
        HandleFallDetection();
        HandleFallingAnimation();
    }

    private void FixedUpdate()
    {
        Rigidbody2D.linearVelocity = new Vector2(Horizontal * Speed, Rigidbody2D.linearVelocity.y);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            LoseLife();
        }
    }

    // ===== Lógica de movimiento =====
    private void HandleMovementInput()
    {
        Horizontal = Input.GetAxisRaw("Horizontal");

        if (Horizontal < 0.0f) transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
        else if (Horizontal > 0.0f) transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

        Animator.SetBool("running", Horizontal != 0.0f);

        Debug.DrawRay(transform.position, Vector3.down * 0.1f, Color.red);
        Grounded = Physics2D.Raycast(transform.position, Vector3.down, 0.1f);

        if (Input.GetKeyDown(KeyCode.W) && Grounded)
        {
            Jump();
            Camera.main.GetComponent<AudioSource>().PlayOneShot(soundJump);
        }
    }

    private void Jump()
    {
        Rigidbody2D.AddForce(Vector2.up * JumForce);
    }

    // ===== Lógica de disparo =====
    private void HandleShootingInput()
    {
        if (Input.GetKey(KeyCode.Space) && Time.time > LastShoot + 0.2f)
        {
            Shoot();
            LastShoot = Time.time;
        }
    }

    private void Shoot()
    {
        Vector3 direction = transform.localScale.x == 1.0f ? Vector3.right : Vector3.left;
        GameObject bullet = Instantiate(bulletPrefab, transform.position + direction * 0.1f, Quaternion.identity);
        bullet.GetComponent<BulletScript>().SetDirection(direction);
    }

    // ===== Lógica de animación de caída =====
    private void HandleFallingAnimation()
    {
        bool wasFalling = Animator.GetBool("isFalling");
        bool isNowFalling = !Grounded && !wasGrounded;

        Animator.SetBool("isFalling", isNowFalling);

        if (Grounded && wasFalling)
        {
            Animator.Play("Idle", 0, 0f);
        }

        wasGrounded = Grounded;
    }

    // ===== Lógica de daño y muerte =====
    public void Hit()
    {
        if (isHurting || isDead) return;

        Health--;
        StartCoroutine(PlayHurtAnimation());

        if (Health <= 0)
        {
            LoseLife();
            Health = 5;
        }
    }

    private IEnumerator PlayHurtAnimation()
    {
        isHurting = true;
        Animator.Play("Hurt", 0, 0f);
        yield return new WaitForSeconds(0.3f);
        isHurting = false;
        UpdateAnimationState();
    }

    private void UpdateAnimationState()
    {
        Animator.SetBool("running", Horizontal != 0.0f);
    }

    // ===== Lógica de caída al vacío =====
    private void HandleFallDetection()
    {
        if (transform.position.y < deathY)
        {
            LoseLife();
        }
    }

    // ===== Sistema de vidas =====
    private void LoseLife()
    {
        currentLives--;
        Debug.Log("VIDAS RESTANTES: " + currentLives);

        if (currentLives <= 0)
        {
            DiePermanently();
        }
        else
        {
            StartCoroutine(PlayDeathAnimation());
        }
    }

    private IEnumerator PlayDeathAnimation()
    {
        isDead = true;
        Animator.SetBool("isDead", true);
        Rigidbody2D.simulated = false;
        GetComponent<Collider2D>().enabled = false;
        enabled = false;

        yield return new WaitForSeconds(0.6f);

        Animator.Play("Idle", 0, 0f);
        Respawn();
        enabled = true;
    }

    private void Respawn()
    {
        Animator.SetBool("isDead", false);
        isDead = false;
        transform.position = initialPosition;
        Rigidbody2D.linearVelocity = Vector2.zero;
        Rigidbody2D.simulated = true;
        GetComponent<Collider2D>().enabled = true;
    }

    private void DiePermanently()
    {
        Animator.SetBool("isDead", true);
        Debug.Log("GAME OVER - REINICIANDO NIVEL...");

        Rigidbody2D.simulated = false;
        GetComponent<Collider2D>().enabled = false;
        enabled = false;

        StartCoroutine(ReloadSceneAfterDelay(2f));
    }

    private IEnumerator ReloadSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
