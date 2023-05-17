using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class BGM : MonoBehaviour
{
    public bool isPlay = false;
    // Start is called before the first frame update
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if(SceneManager.GetActiveScene().name == "Shop")
        {
            Destroy(gameObject);
        }
        else if (SceneManager.GetActiveScene().name == "RoomPVP")
        {
            Destroy(gameObject);
        }
        else if (SceneManager.GetActiveScene().name == "Garage")
        {
            Destroy(gameObject);
        }
        else if (SceneManager.GetActiveScene().name == "RoomCPU")
        {
            Destroy(gameObject);
        }
        else if (SceneManager.GetActiveScene().name == "Loading")
        {
            Destroy(gameObject);
        }
        else if (SceneManager.GetActiveScene().name == "Login")
        {
            Destroy(gameObject);
        }
        else if (SceneManager.GetActiveScene().name == "Friends")
        {
            isPlay = true;
        }
        if (isPlay == true && SceneManager.GetActiveScene().name == "Lobby")
        {
            Destroy(gameObject);
        }
    }
}
