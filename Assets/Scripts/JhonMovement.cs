
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;


public class NewMonoBehaviourScript : MonoBehaviour
{
    public GameObject bulletPrefab;
    public float Speed;
    public float JumForce;
    public AudioClip soundJump;
    public AudioClip doubleJumpSound;
    public float deathY = -10f;
    public Vector3 initialPosition;
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

    private int availableJumps = 2;
    private bool canDoubleJump = false;


    public float maxJumpTime = 0.3f;    
    public float jumpMultiplier = 1.5f; 
    private bool isJumping = false;     
    private float jumpTimeCounter;      




    void Start()
    {
        Rigidbody2D = GetComponent<Rigidbody2D>();
        Animator = GetComponent<Animator>();
        initialPosition = transform.position;
        Debug.Log("VIDAS INICIALES: " + currentLives);
    }

    void Update()
    {
        Horizontal = Input.GetAxisRaw("Horizontal");

        if (Horizontal < 0.0f) transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
        else if (Horizontal > 0.0f) transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

        Animator.SetBool("running", Horizontal != 0.0f);

        Debug.DrawRay(transform.position, Vector3.down * 0.1f, Color.red);
        Grounded = Physics2D.Raycast(transform.position, Vector3.down, 0.1f);

        if (Input.GetKeyDown(KeyCode.W))
        {
            if (Grounded)
            {
                StartJump();
                availableJumps = 1;
                canDoubleJump = true;
            }
            else if (canDoubleJump && availableJumps > 0)
            {
                StartJump();
                availableJumps--;
                canDoubleJump = false;
            }
        }

        if (Input.GetKey(KeyCode.W) && isJumping)
        {
            if (jumpTimeCounter > 0)
            {
                Rigidbody2D.AddForce(Vector2.up * JumForce * (jumpMultiplier * Time.deltaTime));
                jumpTimeCounter -= Time.deltaTime;
            }
            else
            {
                isJumping = false;
            }
        }

        if (Input.GetKeyUp(KeyCode.W))
        {
            isJumping = false;
        }

        if (Input.GetKey(KeyCode.Space) && Time.time > LastShoot + 0.2f)
        {
            Shoot();
            LastShoot = Time.time;
        }

        if (transform.position.y < deathY)
        {
            LoseLife();
        }


        Grounded = Physics2D.Raycast(transform.position, Vector3.down, 0.1f);


        bool wasFalling = Animator.GetBool("isFalling");
        bool isNowFalling = !Grounded && !wasGrounded;

        Animator.SetBool("isFalling", isNowFalling);


        if (Grounded && wasFalling)
        {
            Animator.Play("Idle", 0, 0f);
        }

        wasGrounded = Grounded;
    }

    private void StartJump()
    {
        Rigidbody2D.linearVelocity = new Vector2(Rigidbody2D.linearVelocity.x, 0);
        Rigidbody2D.AddForce(Vector2.up * JumForce);

       
        if (Grounded)
        {
            Camera.main.GetComponent<AudioSource>().PlayOneShot(soundJump); 
        }
        else
        {
            Camera.main.GetComponent<AudioSource>().PlayOneShot(doubleJumpSound); 
        }

        isJumping = true;
        jumpTimeCounter = maxJumpTime;
    }
    private void Shoot()
    {
        Vector3 direction = transform.localScale.x == 1.0f ? Vector3.right : Vector3.left;
        GameObject bullet = Instantiate(bulletPrefab, transform.position + direction * 0.1f, Quaternion.identity);
        bullet.GetComponent<BulletScript>().SetDirection(direction);
    }

    private void FixedUpdate()
    {
        Rigidbody2D.linearVelocity = new Vector2(Horizontal * Speed, Rigidbody2D.linearVelocity.y);
    }

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


        float hurtDuration = 0.3f;
        yield return new WaitForSeconds(hurtDuration);

        isHurting = false;
        UpdateAnimationState();
    }

    private void UpdateAnimationState()
    {
        Animator.SetBool("running", Horizontal != 0.0f);
    }
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
        float deathAnimationLength = 0.6f;
        yield return new WaitForSeconds(deathAnimationLength);
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


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            LoseLife();
        }
    }
}


