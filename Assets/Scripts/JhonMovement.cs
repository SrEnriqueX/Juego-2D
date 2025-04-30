using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public GameObject bulletPrefab;
    public float Speed;
    public float JumForce;
    public AudioClip soundJump;


    private Rigidbody2D Rigidbody2D;
    private Animator Animator;
    private float Horizontal;
    private bool Grounded;
    private float LastShoot;
    private int Health=5;
    


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Rigidbody2D = GetComponent<Rigidbody2D>();
        Animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()//logica de juego
    {
        Horizontal = Input.GetAxisRaw("Horizontal");//a=-1 y d=1 y si no presiona tecla es = 0

        if (Horizontal < 0.0f) transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
        else if(Horizontal > 0.0f) transform.localScale= new Vector3(1.0f, 1.0f, 1.0f);

        Animator.SetBool("running", Horizontal != 0.0f);

        Debug.DrawRay(transform.position,Vector3.down*0.1f, Color.red);
        if (Physics2D.Raycast(transform.position, Vector3.down, 0.1f)) //si choca con algo devuelve true
        {
            Grounded = true;
        }else
        {
            Grounded = false;
        }

        if (Input.GetKeyDown(KeyCode.W) && Grounded) //saltar con W
        {
            Jump();
            Camera.main.GetComponent<AudioSource>().PlayOneShot(soundJump);
        }

        if (Input.GetKey(KeyCode.Space) && Time.time > LastShoot + 0.2f)
        {
            Shoot();
            LastShoot = Time.time;
        }
    }
    private void Jump()
    {
        Rigidbody2D.AddForce(Vector2.up * JumForce); //x= 0 y=1
    }
    private void Shoot()
    {
        Vector3 direction;
        if (transform.localScale.x == 1.0f) direction = Vector3.right;
        else direction = Vector3.left;

        GameObject bullet = Instantiate(bulletPrefab, transform.position + direction * 0.1f, Quaternion.identity);
        bullet.GetComponent<BulletScript>().SetDirection(direction);
    }

    private void FixedUpdate() //FISCIAS
    {
        Rigidbody2D.linearVelocity = new Vector2(Horizontal, Rigidbody2D.linearVelocity.y);
    }

    public void Hit()
    {
        Health = Health - 1;
        if(Health == 0) Destroy(gameObject);
    }
}
