using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Rigidbody middleSpine;
    [SerializeField] private Animator animator;
    
    [SerializeField] private Joystick joystick;

    [SerializeField] private float speed;

    [SerializeField] private float speedHit;

    [SerializeField] private TextMeshProUGUI hpText;

    [SerializeField] private Slider slider;

    [SerializeField] private TextMeshProUGUI levelText;

    [SerializeField] private GameObject losePanel;

    [SerializeField] private GameObject winPanel;
    [SerializeField] private Transform pointCameraWin;
    private int hp;

    private float _timerHit;

    private Rigidbody rb;

    private Transform _enemy;

    private int _hand;

    private bool _inDamageWave;

    private int _level;
    private bool isDown;
    [HideInInspector] public bool Play = false;

    [HideInInspector] public bool Magnet;

    public CameraController CameraController;
    public bool isWin;
    public static PlayerController singleton { get; private set; }

    private void Awake()
    {
        singleton = this;
    }
    
    private void Start()
    {
        Time.timeScale = 1f;
        rb = GetComponent<Rigidbody>();
        ActivateRagdoll(false);

        _level = PlayerPrefs.GetInt("Level");
        _level += 1;
        hp = 180 * _level / 4;
        levelText.text = "LEVEL " + _level;
        _enemy = GameObject.Find("Enemy").transform;
        _timerHit = speedHit;
        
        slider.maxValue = hp;
        slider.value = hp;
        hpText.text = hp.ToString();



    }

    private void Update()
    {
        if(!Play)
            return;
        
        ChangeRunAnim();
        
        if(!isDown && !isWin)
        {
            Vector3 move = new Vector3(joystick.Horizontal * speed, rb.velocity.y, joystick.Vertical * speed);
            transform.Translate(move * speed * Time.deltaTime);
            
            transform.LookAt(_enemy);
                        
        }
        if(isWin) { animator.SetBool("isWin", true); }
        EnemyController.singleton.Slider.transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);

        if (Magnet)
        {
            transform.position = Vector3.MoveTowards(transform.position, _enemy.position, 8.5f * Time.deltaTime);
        }
    }

    private void ChangeRunAnim()
    {
        if(joystick.Horizontal >= 0.1f) 
        { 
            animator.SetBool("isRunR", false);
            animator.SetBool("isRunL", true);
        }

        if (joystick.Horizontal <= -0.1f)
        {
            animator.SetBool("isRunR", true);
            animator.SetBool("isRunL", false);
        }

        if(joystick.Vertical <= -0.25f) 
        {   
            animator.SetBool("isRunB", true);
            animator.SetBool("isRun", false);
            animator.SetBool("isRunR", false);
            animator.SetBool("isRunL", false);
        }

        if(joystick.Vertical >= 0.25f) 
        {   
            animator.SetBool("isRunB", false);
            animator.SetBool("isRun", true);
            animator.SetBool("isRunR", false);
            animator.SetBool("isRunL", false);
        }

        if(joystick.Vertical == 0 && joystick.Horizontal == 0) 
        {
            animator.SetBool("isRunB", false);
            animator.SetBool("isRun", false);
            animator.SetBool("isRunR", false);
            animator.SetBool("isRunL", false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            if(Random.Range(0, 2) == 0)
            {
                EnemyController.singleton.Stun();
            }
            else
            {
                EnemyController.singleton.Attack = true;
            }
        }
        
        if (other.gameObject.CompareTag("DamageWave"))
        {
            _inDamageWave = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {   
            if (_timerHit <= 0)
            {
                if(EnemyController.singleton.Live)
                {
                    animator.SetBool("isHit1", true);

                    animator.SetBool("isRunB", false);
                    animator.SetBool("isRun", false);
                    animator.SetBool("isRunR", false);
                    animator.SetBool("isRunL", false);

                    _hand += 1;
                    EnemyController.singleton.Hit(10, _hand);
                    _timerHit = speedHit;
                }
            }
            else
            {
                _timerHit -= Time.deltaTime;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.CompareTag("Enemy"))
        {
            animator.SetBool("isHit1", false);
            EnemyController.singleton.Attack = false;
            StopAllCoroutines();
        }
        
        if (other.gameObject.CompareTag("DamageWave"))
        {
            _inDamageWave = false;
        }
    }

    private void Die()
    {
        CameraController.OffIU();
        slider.gameObject.SetActive(false);
        EnemyController.singleton.Slider.gameObject.SetActive(false);
        levelText.gameObject.SetActive(false);
        losePanel.SetActive(true);
        this.enabled = false;
    }

    public void Force()
    {
        if (_inDamageWave)
        {
            hp -= 45;
            slider.value = hp;
            hpText.text = hp.ToString();
            
            ActivateRagdoll(true);
            isDown = true;
            middleSpine.AddForce(Vector3.back * 80f, ForceMode.Impulse);
            middleSpine.AddForce(Vector3.up * 60f, ForceMode.Impulse);
            StartCoroutine(standUp(4f));
        }
    }
    IEnumerator standUp(float wait)
    {
        yield return new WaitForSeconds(wait);
        ActivateRagdoll(false);
        isDown = false;
    }
    
    private void ActivateRagdoll(bool ragdollActive)
    {
        Rigidbody[] allRb = GetComponentsInChildren<Rigidbody>();
        Collider[] colliders = GetComponentsInChildren<Collider>();

        animator.enabled = !ragdollActive;

        foreach(Rigidbody rb in allRb)
        {
            rb.isKinematic = !ragdollActive;
        }
        foreach(var coll in colliders)
        {
            coll.enabled = ragdollActive;
        }

        rb.isKinematic = ragdollActive;
        GetComponent<BoxCollider>().enabled = !ragdollActive;
    }

    public void WinAnim()
    {
        animator.SetBool("isWin", true);
        isWin = true;
        CameraController.OffIU();
        levelText.gameObject.SetActive(false);
        Invoke("WinPanelOn", 1);
    }

    private void WinPanelOn()
    {
        winPanel.SetActive(true);
    }

    public void Hit(int hit)
    {
        hp -= hit;
        hpText.text = hp.ToString();
        slider.value = hp;
        
        if (hp <= 0)
        {
            Die();
        }
    }
}
