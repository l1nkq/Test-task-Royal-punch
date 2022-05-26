using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraController : MonoBehaviour
{
    [SerializeField] private GameObject[] ui;

    [Space] 
    [SerializeField] private Transform positionWin;
    [SerializeField] private GameObject startUI;

    [SerializeField] private Camera camera;

    private Transform _player;

    private void Start()
    {
        _player = GameObject.Find("Player").transform;
        camera.GetComponent<Animator>().enabled = false;
    }

    private void Update()
    {
        if(!PlayerController.singleton.Play)
            return;
        
        if(PlayerController.singleton.isWin)
        {
            transform.position = Vector3.Lerp(transform.position, positionWin.position, 2f * Time.deltaTime);
            transform.LookAt(PlayerController.singleton.transform);
        }
        else
        {
            transform.rotation = Quaternion.Euler(35, _player.rotation.eulerAngles.y, 0);
        }
    }
    
    private void WaitPlay()
    {
        PlayerController.singleton.Play = true;
        
        startUI.SetActive(false);

        OffIU();

        camera.targetDisplay = 1;
    }

    public void Play()
    {
        camera.GetComponent<Animator>().enabled = true;
        Invoke("WaitPlay", 1.55f);
    }

    public void OffIU()
    {
        for (int i = 0; i < ui.Length; i++)
        {
            ui[i].SetActive(true);
        }
    }

    public void Restart()
    {
        SceneManager.LoadScene(0);
    }
}
