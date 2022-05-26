using System.Collections;
using System.ComponentModel.Design;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private GameObject flash;
    [SerializeField] private GameObject particlesStan;
    [SerializeField] private Transform rHand;

    [SerializeField] private Transform lHand;

    [SerializeField] private GameObject damageWave;

    [SerializeField] private Transform spawnPosDamageWave;
    
    [SerializeField] private float speedHit;

    [SerializeField] private GameObject confetti;

    [Header("UI")] 
    
    [SerializeField] private TextMeshProUGUI hpText;
    
    [SerializeField] private TextMeshProUGUI damageText;

    [SerializeField] private Transform canvas;

    [SerializeField] private GameObject pauseButton;

    [SerializeField] private GameObject pausePannel;

    private bool _pause;
    
    private int hp;

    private Transform _player;

    private Vector3 scale = new Vector3(25.5f, 0.01f, 25.5f);
    
    private bool _stunned;
    private bool _magnet;

    
    private float _timerHit;

    public Slider Slider;
    
    private int _level;
    
    [HideInInspector] public bool Attack;

    [HideInInspector] public bool Live = true;

    public static EnemyController singleton { get; private set; }

    private void Awake()
    {
        singleton = this;
    }
    
    private void Start()
    {   
        _level = PlayerPrefs.GetInt("Level");
        _level += 1;
        hp = 180 * _level / 2;
        _player = GameObject.Find("Player").transform;
        Slider.maxValue = hp;
        Slider.value = hp;
        hpText.text = hp.ToString();
        _timerHit = speedHit;
        StartCoroutine(PassivAttack());
    }

    private void Update()
    {
        if(_stunned || _magnet)
            return;
        
        transform.LookAt(_player);

        if (Attack)
        {
            if (_timerHit <= 0)
            {
                anim.SetBool("isAttack", true);
                PlayerController.singleton.Hit(9);
                _timerHit = speedHit;
            }
            else
            {
                _timerHit -= Time.deltaTime;
            }
        }
        else
        {
            anim.SetBool("isAttack", false);
        }
    }

    private IEnumerator StopStunned()
    {
        yield return new WaitForSeconds(3);
        
        _stunned = false;
    }

    private IEnumerator PassivAttack()
    {
        yield return new WaitForSeconds(14);
        
        int rand = Random.Range(0, 2);
        
        if (rand == 0)
        {
            Stun();
        }
        else
        {
            Magnet();
        }

        StartCoroutine(PassivAttack());
    }

    private IEnumerator StopMagnet()
    {
        yield return new WaitForSeconds(4.5f);
        anim.SetBool("super6", false);
        _magnet = false;
        PlayerController.singleton.Magnet = false;
    }

    private void Magnet()
    {
        PlayerController.singleton.Magnet = true;
        anim.SetBool("super6", true);
        _magnet = true;

        StartCoroutine(StopMagnet());
    }

    private void Win()
    {
        Time.timeScale = 0.3f;
        _level += 1;
        PlayerPrefs.SetInt("Level", _level);
        PlayerController.singleton.WinAnim();
        confetti.SetActive(true);
        Live = false;
        Slider.gameObject.SetActive(false);
        pauseButton.SetActive(false);
        StopAllCoroutines();
        this.enabled = false;
    }

    public void Pause()
    {
        if (!_pause)
        {
            pausePannel.SetActive(true);
            Time.timeScale = 0f;
            _pause = true;
        }
        else
        {
            Time.timeScale = 1f;
            pausePannel.SetActive(false);
            _pause = false;
        }
    }

    public void Stun()
    {
        anim.SetTrigger("super5");
        GameObject wave = Instantiate(damageWave, spawnPosDamageWave.position, Quaternion.identity);
        wave.GetComponent<Transform>().DOScale(scale, 3.55f).OnComplete(() => Destroy(wave, 0.6f));
        _stunned = true;
        
        StartCoroutine(StopStunned());
    }
    
    public void DamageSuper5()
    {
        Instantiate(particlesStan, transform.position, Quaternion.identity, null);
        PlayerController.singleton.Force();
    }
    
    public void Hit(int hit, int hand)
    {
        hp -= hit;
        Slider.value = hp;
        hpText.text = hp.ToString();
        TextMeshProUGUI damage = Instantiate(damageText, Slider.transform.position, Slider.transform.rotation, canvas);
        
        if (Random.Range(0, 2) == 0)
        {
            damage.GetComponent<Rigidbody2D>().AddForce(Vector2.right * Random.Range(60, 90));
        }
        else
        {
            damage.GetComponent<Rigidbody2D>().AddForce(Vector2.left * Random.Range(60, 90));
        }
        
        damage.GetComponent<Rigidbody2D>().AddForce(Vector2.up * Random.Range(100, 200));

        damage.text = hit.ToString();

        Color color = new Color(1, 1, 1, 0);
        damage.DOColor(color, Random.Range(2f, 2.7f)).OnComplete(() => Destroy(damage));

        Vector2 scale = new Vector2(2.2f, 2.2f);
        
        if(hand % 2 == 0)
        {
            GameObject flashGm = Instantiate(flash, rHand.position, transform.rotation);
            flashGm.transform.rotation = Quaternion.Euler(20, _player.rotation.eulerAngles.y, 0);
            flashGm.transform.DOScale(scale, 0.085f).OnComplete(() => Destroy(flashGm));
        }
        else
        {
            GameObject flashGm = Instantiate(flash, lHand.position, transform.rotation);
            flashGm.transform.rotation = Quaternion.Euler(20, _player.rotation.eulerAngles.y, 0);
            flashGm.transform.DOScale(scale, 0.085f).OnComplete(() => Destroy(flashGm));
        }

        if (hp <= 0)
        {
            Win();
        }
    }
    
    public IEnumerator FreezeAnim(float time)
    {
        anim.speed = 0f;
        yield return new WaitForSeconds(time);
        anim.speed = 1f;
    }
}
