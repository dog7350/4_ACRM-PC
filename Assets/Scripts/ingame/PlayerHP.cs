using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Fusion;

public class PlayerHP : NetworkBehaviour
{
    public Image HealthBar;

    void Update()
    {
            HealthBar.fillAmount = PlayerController.health / PlayerController.maxHealth;
    }
}
