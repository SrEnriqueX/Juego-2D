using UnityEngine;
using UnityEngine.SceneManagement;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public GameObject bulletPrefab;
    public float Speed;
    public float JumForce;
    public AudioClip soundJump;
    // public int maxLives = 3; // Vidas m�ximas
    public float deathY = -10f; // Altura m�nima para considerar "ca�da"


    private Rigidbody2D Rigidbody2D;
    private Animator Animator;
    private float Horizontal;
    private bool Grounded;
    private float LastShoot;
    private int Health = 5;
    [Header("Respawn Settings")]
    public Vector3 initialPosition; // Posición inicial de respawn
    private int currentLives = 3; // Vidas actuales

    [Header("Visual Feedback")]
    public Color damageColor = Color.red; // Color al recibir daño
    public float flashDuration = 0.3f; // Duración del efecto

    private SpriteRenderer spriteRenderer;





    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Rigidbody2D = GetComponent<Rigidbody2D>();
        Animator = GetComponent<Animator>();

        initialPosition = transform.position; // Guarda la posición inicial automáticamente
        Debug.Log("VIDAS INICIALES: " + currentLives);
        spriteRenderer = GetComponent<SpriteRenderer>();


    }


    // Update is called once per frame
    void Update()//logica de juego
    {
        Horizontal = Input.GetAxisRaw("Horizontal");//a=-1 y d=1 y si no presiona tecla es = 0

        // Flip sprite seg�n direcci�n
        if (Horizontal < 0.0f) transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
        else if (Horizontal > 0.0f) transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

        Animator.SetBool("running", Horizontal != 0.0f);

        // Detecci�n de suelo
        Debug.DrawRay(transform.position, Vector3.down * 0.1f, Color.red);
        Grounded = Physics2D.Raycast(transform.position, Vector3.down, 0.1f);

        /*if (Physics2D.Raycast(transform.position, Vector3.down, 0.1f)) //si choca con algo devuelve true
        {
            Grounded = true;
        }else
        {
            Grounded = false;
        }*/

        //Salto
        if (Input.GetKeyDown(KeyCode.W) && Grounded) //saltar con W
        {
            Jump();
            Camera.main.GetComponent<AudioSource>().PlayOneShot(soundJump);
        }

        //Disparo
        if (Input.GetKey(KeyCode.Space) && Time.time > LastShoot + 0.2f)
        {
            Shoot();
            LastShoot = Time.time;
        }
        // Detecci�n de caida
        if (transform.position.y < deathY)
        {
            LoseLife();
        }

    }
    private void Jump()
    {
        Rigidbody2D.AddForce(Vector2.up * JumForce);
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
        Health--;
        if (Health <= 0)
        {
            LoseLife();
            Health = 5; // Resetear salud para la siguiente vida
        }
    }

    private void LoseLife()
    {
        currentLives--;
        Debug.Log("VIDAS RESTANTES: " + currentLives);

        if (currentLives <= 0)
        {
            Debug.Log("GAME OVER - REINICIANDO NIVEL...");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            Respawn();
        }
    }
    private void Respawn()
    {
        // 1. Resetear posición
        transform.position = initialPosition;

        // 2. Resetear física
        Rigidbody2D.linearVelocity = Vector2.zero;

        // 3. Efecto visual
        FlashEffect();
        Debug.Log("RESPAWN en posición inicial");
    }

    private void FlashEffect()
    {
        spriteRenderer.color = damageColor;
        Invoke(nameof(ResetColor), flashDuration);
    }

    private void ResetColor()
    {
        spriteRenderer.color = Color.white;
    }

    // Detecci�n de colisi�n con enemigos
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            LoseLife();
        }
    }
}
