using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Fusion;

public class StartCount : NetworkBehaviour
{
    public float TimeSet = 4;
    public GameObject StartCounter;
    public bool isPlay;
    public int PlayCount = 0;

    void Update()
    {
        if(GameDirector.instance.loadComplate == true)
        {
            isPlay = true;
            if(TimeSet > 0)
            {
                TimeSet -= Time.deltaTime;
            }
            else if(TimeSet <= 0)
            {
                PlayerController.isStart = true;
                MoveAI.isStart = true;
                StartCounter.GetComponent<AudioSource>().Stop();
                StartCounter.SetActive(false);
            }
            if (isPlay == true && PlayCount == 0)
            {
                StartCounter.GetComponent<AudioSource>().Play();
                PlayCount++;
            }

            StartCounter.GetComponent<Text>().text = "" + TimeSet.ToString();
        }
    }
}
