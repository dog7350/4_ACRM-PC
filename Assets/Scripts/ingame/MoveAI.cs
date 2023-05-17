using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public class MoveAI : MonoBehaviour
{
    NavMeshAgent agent;

    [SerializeField]
    Transform target1;
    [SerializeField]
    Transform target2;
    [SerializeField]
    Transform target3;

    public GameObject RoomInfoObj;
    public GameObject Explosion;

    public float TargetCount = 1;
    public static bool isStart = false;
    public static bool isbroken = false;
    public float health;
    public float maxHealth;
    public float def = 0;
    public float repairtime = 3f;
    public float AISpeed;

    void Start()
    {
        RoomInfoObj = GameObject.FindGameObjectWithTag("RoomInfo");
        Explosion = gameObject.transform.GetChild(5).transform.GetChild(0).gameObject;
        gameObject.transform.GetChild(4).GetComponent<TextMesh>().text = "[AI] " + gameObject.transform.parent.name.Replace(" AI(Clone)", "");
        gameObject.transform.GetChild(4).GetComponent<TextMesh>().fontSize = 25;
        gameObject.transform.GetChild(4).GetComponent<TextMesh>().color = new Color(0 / 255f, 255 / 255f, 0 / 255f);

        target1 = GameObject.Find("target1").transform;
        target2 = GameObject.Find("target2").transform;
        target3 = GameObject.Find("target3").transform;

        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;

        maxHealth = RoomInfoObj.GetComponent<RoomInfo>().CpuHp;
        def = RoomInfoObj.GetComponent<RoomInfo>().CpuDef;
        health = maxHealth;
        AISpeed = RoomInfoObj.GetComponent<RoomInfo>().CpuSpeed;

        gameObject.GetComponent<NavMeshAgent>().speed = AISpeed;
        gameObject.GetComponent<NavMeshAgent>().angularSpeed = AISpeed;
        gameObject.GetComponent<NavMeshAgent>().acceleration = AISpeed;
    }

    void Update()
    {
        if(isStart == true && isbroken == false)
        {
            targetSwitch();
            transform.rotation = Quaternion.LookRotation(agent.desiredVelocity);
        }

        if (health <= 0)
        {
            isbroken = true;
            gameObject.GetComponent<NavMeshAgent>().speed = 0;
            repair();
        }
    }
    public void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Bullet")
        {
            Bullet bullet = col.GetComponent<Bullet>();
            OnDamage(bullet.damage);
        }
    }

    void targetSwitch()
    {
        switch (TargetCount)
        {
            case 1:
                agent.SetDestination(target1.position);
                break;
            case 2:
                agent.SetDestination(target2.position);
                break;
            case 3:
                agent.SetDestination(target3.position);
                break;
            case 4:
                agent.isStopped = true;
                break;
        }
    }

    public void repair()
    {
        if (repairtime > 0)
        {
            repairtime -= Time.deltaTime;
        }
        else
        {
            isbroken = false;
            health = maxHealth;
            repairtime = 2f;
            gameObject.GetComponent<NavMeshAgent>().speed = AISpeed;
        }
    }

    void OnDamage(float value)
    {
        if (def > value) health = health - (def - value);
        else health = health - (value - def);

        if (health <= 0)
        {
            Explosion.transform.GetChild(0).gameObject.SetActive(true);
            isbroken = true;
            repair();
        }
    }
}