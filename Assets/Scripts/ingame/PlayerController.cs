using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

[OrderAfter(typeof(NetworkRigidbody))]
public class PlayerController : NetworkBehaviour
{
    public static PlayerController instance = null;
    public NetworkRunner runner;

    private void Awake()
    {
        instance = this;
        runner = GameObject.Find("NetworkRunner").GetComponent<NetworkRunner>();
    }
    int initCount = 1;
    public GameObject PlayerObjList;

    [Networked] private NRunner.NetworkInputData inputs { get; set; }
    public GameObject joystick;
    Vector2 stickInput;
    public GameObject myInfo;
    public PlayerAudio audio;
    [Networked] public float soundSpeed { get; set; }
    public List<int> itemList = new List<int>();
    string myTeam;
    string carName;
    string gunName;

    [SerializeField] NetworkPrefabRef Explosion;
    public bool explosionActive = false;

    public static float health;
    public static float maxHealth;
    public static float def;
    public static bool isReSpawnRq = false;
    public GameObject[] CheckPoints;
    public bool isroad = true;
    public static bool isStart = false;
    public static bool isbroken = false;
    public static bool isShield = false;
    public static bool isBooster = false;
    public static bool isBerserker = false;
    public static bool isHpRestore = false;
    public static bool isGetItem = false;
    public static bool isFullItem = false;
    public static bool isLapdon = false;
    public bool isPlay = false;
    public int PlayCheck = 0;
    public float haveBox = 0;
    public float AppSpeed = 0; // 현재 속도
    public float default_MaxSpeed;
    public float MaxSpeed; // 전진 최대 속도
    public float ResSpeed; // 후진 최대 속도
    public float accel = 0.8f; // 가속도
    public float decel = 0.5f; // 노 엑셀 상태 감속도
    public float repairtime = 2f;
    public float rot; // 바퀴 회전 각도
    public float rot2; // 바퀴 회전 각도
    public float thrust;    // 차량 가속
    public float thrust2;    // 차량 좌우 가속
    public float downForceValue; // 차량 위에서 아래로 가해지는 힘
    Rigidbody rb;
    // 휠 콜라이더 4개
    public GameObject[] wheels;
    // 본체 바퀴 4개
    public GameObject[] wheelMesh;

    void Start() => PvpStart();
    public override void FixedUpdateNetwork() => PvpUpdate();
    void PvpStart()
    {
        if (ENB.cpuPlay != true)
        {
            carName = GameDirector.instance.myInfoObj.GetComponent<PlayerInfo>().car;
            gunName = GameDirector.instance.myInfoObj.GetComponent<PlayerInfo>().gun;
        }
        else
        {
            carName = ENB.gameinfo.usecar;
            gunName = ENB.gameinfo.usegun;
        }
        for (int i = 0; i < ENB.carinfos.Count; i++)
        {
            if (carName.Equals(ENB.carinfos[i].cname))
            {
                MaxSpeed = ENB.carinfos[i].speed;
                default_MaxSpeed = MaxSpeed;
                ResSpeed = MaxSpeed + (MaxSpeed / 2);
                maxHealth = ENB.carinfos[i].hp;
                health = maxHealth;
                def = ENB.carinfos[i].def;
            }
        }
        for (int i = 0; i < ENB.guninfos.Count; i++)
        {
            if (gunName.Equals(ENB.guninfos[i].gname))
            {
                PlayerFire.instance.atk = ENB.guninfos[i].atk;
                PlayerFire.instance.rpm = 2 - ENB.guninfos[i].rpm;
                PlayerFire.MaxAmmo = ENB.guninfos[i].ammo;
                PlayerFire.nowAmmo = PlayerFire.MaxAmmo;
            }
        }

        initCount = 1;
        CheckPoints = GameObject.FindGameObjectsWithTag("CheckPoint");
        // joystick = GameObject.FindGameObjectWithTag("JoyStick");
        Transform[] tmp = gameObject.GetComponentsInChildren<Transform>();
        audio = tmp[tmp.Length - 12].GetComponent<PlayerAudio>();
    }
    void PvpUpdate()
    {
        // stickInput = joystick.GetComponent<PlayerInput>().actions["Move"].ReadValue<Vector2>();
        if (myInfo == null) myInfo = GameDirector.instance.myInfoObj;

        if (isBerserker == true || isBooster == true)
        {
            if (isBerserker == true && isBooster == true)
            {
                if (MaxSpeed <= (default_MaxSpeed * (ENB.itemBerserker * 0.2f) + (default_MaxSpeed * ENB.itemBooster * 0.4f)))
                {
                    MaxSpeed += (MaxSpeed * (ENB.itemBerserker * 0.01f) + (MaxSpeed * (ENB.itemBooster * 0.01f)));
                }
            }
            else if (isBerserker == true)
            {
                if (MaxSpeed <= (default_MaxSpeed * (ENB.itemBerserker * 0.4f)))
                {
                    MaxSpeed += (MaxSpeed * (ENB.itemBerserker * 0.01f));
                }
            }

            else if (isBooster == true)
            {
                if (MaxSpeed <= (default_MaxSpeed * ENB.itemBooster * 0.5f))
                {
                    MaxSpeed += (MaxSpeed * (ENB.itemBooster * 0.01f));
                }
            }
        }
        else
        {
            MaxSpeed = default_MaxSpeed;
        }

        if (initCount == 1)
        {
            wheels = GameObject.FindGameObjectsWithTag("Wheels");
            wheelMesh = GameObject.FindGameObjectsWithTag("WheelMesh");

            if (ENB.cpuPlay != true) myTeam = myInfo.GetComponent<PlayerInfo>().team;
            rb = GetComponent<Rigidbody>();

            initCount = 0;
        }

        PlayerSound();

        for (int i = 0; i < wheelMesh.Length; i++) wheels[i].transform.position = wheelMesh[i].transform.position;

        if (GameDirector.instance.loadComplate == true && isbroken == false)
        {
            // KeyBoard
            for (int i = 0; i < 2; i++)
            {
                // 앞바퀴 각도전환이 되어야하므로 for문을 앞바퀴만 해당되도록 설정한다.
                wheels[i].GetComponent<WheelCollider>().steerAngle = Input.GetAxis("Horizontal") * rot;
            }
            // JoyStick
            /*
            for (int i = 0; i < 2; i++)
            {
                // 앞바퀴 각도전환이 되어야하므로 for문을 앞바퀴만 해당되도록 설정한다.
                wheels[i].steerAngle = stickInput.x * rot;
            }
            */

            WheelPosAndAni();

            if (GetInput(out NRunner.NetworkInputData input))
            {
                inputs = input;
            }

            if (myInfo.GetComponent<PlayerInfo>().RaceGoal) AppSpeed = Mathf.Lerp(AppSpeed, 0, decel * Runner.DeltaTime);
            else if (isroad == true && GameDirector.instance.chatInputFlag == false && isStart == true)
            {
                Move(inputs);
                Drift(inputs);
                ItemOne(inputs);
                ItemTwo(inputs);
            }

            ChatEnter(inputs);
            if (myInfo.GetComponent<PlayerInfo>().RaceGoal) PlayerFire.nowAmmo = 0;
            else if (GameDirector.instance.chatInputFlag == false) PlayerReturn(inputs);
            Escape(inputs);
            var vel = (rb.rotation * Vector3.forward) * AppSpeed;
            vel.y = rb.velocity.y - 1;
            rb.velocity = vel;

            rb.AddForce(-transform.up * downForceValue * rb.velocity.magnitude); //가속받으면 아래로 눌러줌
        }

        if (CheckPoint.isReSpawn == true)
        {
            Transform Myposition = gameObject.GetComponent<Transform>();
            for (int i = 0; i < CheckPoints.Length; i++)
            {
                if (myInfo.GetComponent<PlayerInfo>().MyChackPointCount == CheckPoints[i].GetComponent<CheckPoint>().CkNum)
                {
                    Myposition.position = CheckPoints[i].GetComponent<CheckPoint>().SpawnPoint.transform.position;
                    Myposition.rotation = CheckPoints[i].GetComponent<CheckPoint>().SpawnPoint.transform.rotation;
                    break;
                }
            }
            CheckPoint.isReSpawn = false;
        }
        if (health <= 0)
        {
            if (explosionActive == false)
            {
                explosionActive = true;
                gameObject.GetComponent<GamePlayerInfo>().Explosion = true;
            }

            isbroken = true;
            GameDirector.instance.BrokenPanel.SetActive(true);
            repair();
        }
        else if (GameDirector.instance.BrokenPanel.activeSelf && health >= 1) GameDirector.instance.BrokenPanel.SetActive(false);

        if (gameObject.GetComponent<GamePlayerInfo>().Boost == true)
        {
            if (GameDirector.instance.mainCamera.GetComponent<SmoothFollow>().distance < 100)
            {
                GameDirector.instance.mainCamera.GetComponent<SmoothFollow>().distance += 1;
            }
        }
        else
        {
            if (GameDirector.instance.mainCamera.GetComponent<SmoothFollow>().distance > 60)
            {
                GameDirector.instance.mainCamera.GetComponent<SmoothFollow>().distance -= 1;
            }
        }

        if (isHpRestore == true)
        {
            HpRestore();
        }
        if (haveBox == 1)
        {
            isGetItem = true;
            haveBox = 0;
        }
    }

    void PlayerSound()
    {
        soundSpeed = rb.transform.InverseTransformVector(rb.velocity / MaxSpeed).z;
        audio.HandleDriveAudio(soundSpeed);

        audio.IdleSound.volume = Mathf.Lerp(audio.IdleSoundMaxVolume, 0.0f, soundSpeed * 4);
    }
    void Move(NRunner.NetworkInputData input)
    {
        // KeyBoard
        if (input.IsAccelerate && (input.IsLeft || input.IsRight))
        {
            AppSpeed = Mathf.Lerp(AppSpeed, (MaxSpeed - (MaxSpeed * 0.15f)), accel * Runner.DeltaTime);
        }
        else if (input.IsAccelerate)
        {
            AppSpeed = Mathf.Lerp(AppSpeed, MaxSpeed, accel * Runner.DeltaTime);
        }
        else if (input.IsReverse)
        {
            AppSpeed = Mathf.Lerp(AppSpeed, -ResSpeed, accel * Runner.DeltaTime);
        }
        else
        {
            AppSpeed = Mathf.Lerp(AppSpeed, 0, decel * Runner.DeltaTime);
        }

        // JoyStick (모바일의 경우 위 else 지우기)
        /*
        if (stickInput.y > 0)
        {
            AppSpeed = Mathf.Lerp(AppSpeed, MaxSpeed, accel * Runner.DeltaTime);
        }
        else if (stickInput.y < 0)
        {
            AppSpeed = Mathf.Lerp(AppSpeed, -ResSpeed, accel * Runner.DeltaTime);
        }
        else
        {
            AppSpeed = Mathf.Lerp(AppSpeed, 0, decel * Runner.DeltaTime);
        }
        */
    }
    void Drift(NRunner.NetworkInputData input)
    {
        var startDrift = input.IsDriftPressedThisFrame;

        if (startDrift)
        {
            StartDrifting(input);
        }

        if (!input.IsDriftPressed)
        {
            soundSpeed = rb.transform.InverseTransformVector(rb.velocity / 0).z;
            audio.HandleDriftAudio(soundSpeed);
            StopDrifting();
        }
    }
    private void StartDrifting(NRunner.NetworkInputData input)
    {
        // KeyBoard
        for (int i = 2; i < 4; i++)
        {
            // 뒷바퀴 각도전환이 되어야하므로 for문을 뒷바퀴만 해당되도록 설정한다.
            wheels[i].GetComponent<WheelCollider>().steerAngle = Input.GetAxis("Horizontal") * rot2;
        }
        // JoyStick
        /*
        for (int i = 2; i < 4; i++)
        {
            // 뒷바퀴 각도전환이 되어야하므로 for문을 뒷바퀴만 해당되도록 설정한다.
            wheels[i].steerAngle = stickInput.x * rot2;
        }
        */

        // KeyBoard
        if (input.IsRight)
        {
            rb.AddRelativeForce(Vector3.right * thrust2);  // 차량을 오른쪽으로 밀어주는 힘
        }
        if (input.IsLeft)
        {
            rb.AddRelativeForce(Vector3.left * thrust2);  // 차량을 왼쪽으로 밀어주는 힘
        }

        // JoyStick (모바일의 경우 위 KeyBoard 지우고 아래 사용)
        /*
        if (input.IsRight || stickInput.x > 0)
        {
            rb.AddRelativeForce(Vector3.right * thrust2);  // 차량을 오른쪽으로 밀어주는 힘
        }
        if (input.IsLeft || stickInput.x < 0)
        {
            rb.AddRelativeForce(Vector3.left * thrust2);  // 차량을 왼쪽으로 밀어주는 힘
        }
        */

        soundSpeed = rb.transform.InverseTransformVector(rb.velocity / MaxSpeed).z;
        audio.HandleDriftAudio(soundSpeed * audio.DriftMaxVolume);
    }
    private void HpRestore()
    {
        if (health < maxHealth)
        {
            health += ENB.itemHpRestore;
            if (health >= maxHealth)
            {
                health = maxHealth;
            }
        }
    }
    private void StopDrifting()
    {
        // KeyBoard
        for (int i = 2; i < 4; i++)
        {
            // 쉬프트를 풀었을 경우 뒷바퀴의 각도를 원상태로 돌려준다
            wheels[i].GetComponent<WheelCollider>().steerAngle = Input.GetAxis("Horizontal") * 0;
        }
        // JoyStick
        /*
        for (int i = 2; i < 4; i++)
        {
            // 뒷바퀴 각도전환이 되어야하므로 for문을 뒷바퀴만 해당되도록 설정한다.
            wheels[i].steerAngle = stickInput.x * 0;
        }
        */
    }
    void ItemOne(NRunner.NetworkInputData input)
    {
        if (input.IsDownThisFrame(NRunner.NetworkInputData.UseItemOne))
        {
            if (itemList.Count > 0)
            {
                switch (itemList[0])
                {
                    case 1: audio.PlayHpRestore(); ItemController.instance.ItemHpRestore(); break;
                    case 2: audio.PlayBerserker(); Berserker.instance.OnClick(); break;
                    case 3: audio.PlayBooster(); Booster.instance.OnClick();
                        gameObject.GetComponent<GamePlayerInfo>().Boost = true;
                        break;
                    case 4: audio.PlayAmmoRestore(); AmmoRestore.instance.OnClick(); break;
                    case 5: audio.PlayShield(); shield.instance.OnClick(); break;
                }
                itemList.RemoveAt(0);
            }
        }
        ItemController.instance.ItemSlotPrint();
    }
    void ItemTwo(NRunner.NetworkInputData input)
    {
        if (input.IsDownThisFrame(NRunner.NetworkInputData.UseItemTwo))
        {
            if (itemList.Count > 0)
            {
                switch (itemList[1])
                {
                    case 1: audio.PlayHpRestore(); ItemController.instance.ItemHpRestore(); break;
                    case 2: audio.PlayBerserker(); Berserker.instance.OnClick(); break;
                    case 3: audio.PlayBooster(); Booster.instance.OnClick();
                        gameObject.GetComponent<GamePlayerInfo>().Boost = true;
                        break;
                    case 4: audio.PlayAmmoRestore(); AmmoRestore.instance.OnClick(); break;
                    case 5: audio.PlayShield(); shield.instance.OnClick(); break;
                }
                itemList.RemoveAt(1);
            }
        }
        ItemController.instance.ItemSlotPrint();
    }
    void PlayerReturn(NRunner.NetworkInputData input)
    {
        if (input.IsDownThisFrame(NRunner.NetworkInputData.ButtonReturn))
        {
            isReSpawnRq = true;
            if (isReSpawnRq == true)
            {
                CheckPoint.isReSpawn = true;
            }
        }
    }
    void Escape(NRunner.NetworkInputData input)
    {
        if (input.IsDownThisFrame(NRunner.NetworkInputData.ButtonPause))
        {
            GameDirector.instance.ExitPanel.SetActive(true);
        }
    }
    void ChatEnter(NRunner.NetworkInputData input)
    {
        if (input.IsDownThisFrame(NRunner.NetworkInputData.ButtonEnter))
        {
            if (GameDirector.instance.chatInputFlag == false)
            {
                GameDirector.instance.chatInputFlag = true;
                GameDirector.instance.ChatInput.SetActive(true);
                GameDirector.instance.chatTime = 5;
                GameDirector.instance.ChatInput.GetComponent<InputField>().Select();
            }
            else
            {
                GameDirector.instance.chatInputFlag = false;
                GameDirector.instance.ChatInput.SetActive(false);
            }
        }
    }

    // 바퀴위치 조절
    void WheelPosAndAni()
    {
        Vector3 wheelPosition = Vector3.zero;
        Quaternion wheelRotation = Quaternion.identity;

        for (int i = 0; i < 4; i++)
        {
            wheels[i].GetComponent<WheelCollider>().GetWorldPose(out wheelPosition, out wheelRotation);
            wheelMesh[i].transform.position = wheelPosition;
            wheelMesh[i].transform.rotation = wheelRotation;
        }
    }
    void OnDamage(float value)
    {
        if (gameObject.GetComponent<NetworkObject>().HasStateAuthority == true)
        {
            health = health - (value - def);
            if (health <= 0)
            {
                isbroken = true;
                repair();
            }
        }
    }
    public void repair()
    { 
        if(repairtime > 0)
        {
            repairtime -= Time.deltaTime;
        }
        else
        {
            explosionActive = false;
            isbroken = false;
            GameDirector.instance.BrokenPanel.SetActive(false);
            health = maxHealth;
            repairtime = 2f;
        }
    }
    public void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "Bullet" && !collision.gameObject.GetComponent<NetworkObject>().HasStateAuthority)
        {
            Bullet bullet = collision.GetComponent<Bullet>();
            string enemy = bullet.team;

            if (myTeam.Equals("i") || !enemy.Equals(myTeam)) OnDamage(bullet.damage);
        }
        if (collision.gameObject.tag == "AIBullet")
        {
            Bullet bullet = collision.GetComponent<Bullet>();
            string enemy = bullet.team;
            OnDamage(bullet.damage);
        }
        if (collision.gameObject.CompareTag("ItemBox") && isFullItem == false)
        {
            ItemBox.isBoxOff = true;
        }

        if (collision.gameObject.CompareTag("Wall"))
        {
            audio.PlayCrash();
            AppSpeed = 0;
        }
    }

    public void OnTriggerStay(Collider col)
    {
        if (col.gameObject.CompareTag("Road"))
        {
            isroad = true;
        }
    }
    public void OnTriggerExit(Collider col)
    {
        if (col.gameObject.CompareTag("Road"))
        {
            isroad = false;
        }
    }
}
