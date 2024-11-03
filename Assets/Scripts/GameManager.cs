using System.Collections.Generic;
using UnityEngine;
using System.Collections;  //协程调用
using UnityEngine.UI;   //UI刷新
using UnityEngine.SceneManagement;  //重载场景

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    public Sprite Spritebangbangtang;//棒棒糖图标
    public Sprite Spriteshalou;//沙漏图标
    public bool Openshalou;//开启刷新沙漏道具

    public static GameManager Instance
    {
        get
        {
            return _instance;
        }
    }

    public GameObject Pacman;
    public GameObject Blinky;
    public GameObject Cycle;
    public GameObject Inky;
    public GameObject Pinky;

    //持有UI面板
    public GameObject startPanel;
    public GameObject gamePanel;
    public GameObject SettingPanel;
    public GameObject RankPanel;//排行榜面板
    public GameObject SwitchLevelPanel;//选关面板
    public GameObject FailPanel;//失败关卡面板

    //持有开始动画
    public GameObject startCountDownPrefab;
    public GameObject gameOverPrefab;
    public GameObject winPrefab;

    //持有音乐
    public Slider Slider;//音量条
    public float CurrentVolume;//当前音量
    public AudioClip startClip;
    public AudioSource AudioSource;//背景音乐音频组件

    //超级吃豆人状态
    public bool isSuperPacman = false;
    public int Health;//角色血量

    public List<int> usingIndex = new List<int>();
    public List<int> rawIndex = new List<int> { 0, 1, 2, 3 };
    private List<GameObject> pacdotGos = new List<GameObject>();

    //当前地图中剩余豆子，已吃多少个豆子，目前分数(要在EnemyMove脚本调用)
    private int pacdotNumber = 0;
    private int nowEat = 0;
    public int score = 0;
    private float GameTimer;//游戏进行时长计时器
    private int CurrentGameTime;    //当前游戏进行时长

    //数组组件，用来更新UI
    public Text remainText;
    public Text nowText;
    public Text scoreText;
    public Text RankText;
    public GameObject HealthImage1;//血量显示Image
    public GameObject HealthImage2;
    public GameObject HealthImage3;
    public Text GameTime;//游戏计时
    public bool IsMap1;//关卡1，用于在游戏结束后切换到关卡2


    private void Awake()
    {
        _instance = this;
        int tempCount = rawIndex.Count;

        for (int i = 0; i < tempCount; i++)
        {
            int tempIndex = Random.Range(0, rawIndex.Count);
            usingIndex.Add(rawIndex[tempIndex]);
            rawIndex.RemoveAt(tempIndex);
        }

        //取得所有豆子
        foreach (Transform t in GameObject.Find("Maze").transform)
        {
            pacdotGos.Add(t.gameObject);
        }

        //豆子是迷宫Maze的子物体，从迷宫内取得豆子数赋给pacdotNumber
        pacdotNumber = GameObject.Find("Maze").transform.childCount;
    }

    //生成超级豆点
    private void CreateSuperPacdot()
    {
        //当剩余豆子过少，则不再产生超级豆
        if (pacdotGos.Count < 7)
            return;

        //随机取得豆子
        int tempIndex = Random.Range(0, pacdotGos.Count);

        //变大豆子为原来2倍
        pacdotGos[tempIndex].transform.localScale = new Vector3(2, 2, 2);

        //设置豆子外观为棒棒糖
        pacdotGos[tempIndex].GetComponent<SpriteRenderer>().sprite = Spritebangbangtang;

        //设置该豆子为超级豆子属性
        pacdotGos[tempIndex].GetComponent<Pacdot>().isSupperPacdot = true;
    }

    //生成暂停AI的道具
    public void CreatePausePacdot()
    {
        //当剩余豆子过少，则不再产生超级豆
        if (pacdotGos.Count < 7)
            return;

        //随机取得豆子
        int tempIndex = Random.Range(0, pacdotGos.Count);

        //变大豆子为原来2倍
        pacdotGos[tempIndex].transform.localScale = new Vector3(2, 2, 2);

        //设置豆子外观为沙漏
        pacdotGos[tempIndex].GetComponent<SpriteRenderer>().sprite = Spriteshalou;

        //设置该豆子为暂停道具属性
        pacdotGos[tempIndex].GetComponent<Pacdot>().IsPausePacdot = true;
    }

    //初始化
    private void Start()
    {
        //开局停止状态，等待点击Start
        SetGameState(false);
        if (IsMap1 == false)
        {
            OnStartButton();//如果是第二关则直接开始运行
        }
    }

    //当点击开始按钮(对UI图标添加On Click->GameManager->OnStartButton启用该函数)
    public void OnStartButton()
    {
        //与点击开始按钮后同步进行的函数
        StartCoroutine(PlayStartCountDown());

        //Start声音，声音源在原点位置
        //AudioSource.PlayClipAtPoint(startClip, Vector3.zero);

        //隐藏开始按钮的页面
        startPanel.SetActive(false);

        //隐藏关卡选择页面
        if (IsMap1)
        {
            OpenOrCloseSwitchLevel();
        }


        //测试读取得分
        //PlayerPrefs.DeleteKey("Score"); //删除Name的键和值
        //PlayerPrefs.DeleteKey("1Score"); //删除Name的键和值
        //PlayerPrefs.DeleteKey("2Score"); //删除Name的键和值
        //PlayerPrefs.DeleteKey("3Score"); //删除Name的键和值
        //int L_Score = ReadScore();
        //print("最高得分：" + L_Score);//打印
    }

    //点击Exit后退出游戏
    public void OnExitButton()
    {
        UnityEditor.EditorApplication.isPlaying = false;//编辑器测试
        Application.Quit();
    }

    //开启/关闭失败页面
    public void OpenOrCloseFailPanel()
    {
        bool Active = FailPanel.activeSelf;//获取面板的显示状态
        if (Active)
        {
            FailPanel.SetActive(false);
        }
        else
        {
            FailPanel.SetActive(true);
        }//取反
    }

    //开启/关闭关卡选择页面
    public void OpenOrCloseSwitchLevel()
    {
        bool Active = SwitchLevelPanel.activeSelf;//获取面板的显示状态
        if (Active)
        {
            SwitchLevelPanel.SetActive(false);
        }
        else
        {
            SwitchLevelPanel.SetActive(true);
        }//取反
    }

    //重新打开关卡1
    public void OpenLevel1()
    {
        SceneManager.LoadScene("level1");//level1为我们要切换到的场景
    }
    public void OpenLevel2()
    {
        SceneManager.LoadScene("level2");//level1为我们要切换到的场景
    }

    //选择第一关则直接调用开始游戏
    public void SwitchLevel_1()
    {
        OnStartButton();
    }

    //选择第二关，跳转关卡
    public void SwitchLevel_2()
    {

    }



    //点击开启设置面板
    public void OpenOrCloseSettingPanel()
    {
        bool Active = SettingPanel.activeSelf;//获取面板的显示状态
        if (Active)
        {
            SettingPanel.SetActive(false);
            SaveVolume();//保存音量设置
        }
        else
        {
            SettingPanel.SetActive(true);
            ReadAndSetVolume();//读取并设置音量条
        }//取反
    }


    //点击开启排行榜面板
    public void OpenOrCloseRankPanel()
    {
        
        bool Active = RankPanel.activeSelf;//获取面板的显示状态
        if (Active)
        {
            RankPanel.SetActive(false);

        }
        else
        {
            RankPanel.SetActive(true);
            int L_Score = ReadScore();//更新排行榜显示
        }//界面显示取反
    }

    //协程函数：与点下开始按钮同步进行
    IEnumerator PlayStartCountDown()
    {
        //弃用倒计时
        //在方法执行过程中延时，从动作播放开始计时，计时4s后销毁倒计时动作
        //GameObject go = Instantiate(startCountDownPrefab);
        yield return new WaitForSeconds(0f);
        //Destroy(go);

        //设置开始游戏状态
        SetGameState(true);




        if (Openshalou)
        {
            //开始后，每10s产生一个暂停道具
            Invoke("CreatePausePacdot", 10f);
            //开始后，每10s产生一个超级豆子
            Invoke("CreateSuperPacdot", 10f);
        }


        //显示积分面板
        gamePanel.SetActive(true);

        //播放bgm
        GetComponent<AudioSource>().Play();
    }

    //更新玩家当前血量
    public void UpdatePlayerHealth(int UpdateDate)
    {
        Health += UpdateDate;//更新当前血量
        if (Health >= 3)//更新血量显示
        {
            HealthImage1.SetActive(true);
            HealthImage2.SetActive(true);
            HealthImage3.SetActive(true);
        }
        else if (Health >= 2)
        {
            HealthImage1.SetActive(true);
            HealthImage2.SetActive(true);
            HealthImage3.SetActive(false);
        }
        else if (Health >= 1)
        {
            HealthImage1.SetActive(true);
            HealthImage2.SetActive(false);
            HealthImage3.SetActive(false);
        }
        else 
        {
            HealthImage1.SetActive(false);
            HealthImage2.SetActive(false);
            HealthImage3.SetActive(false);
        }


        //角色闪烁
        Pacman.GetComponent<Animator>().SetBool("Hit", true);//设置为受击状态                                 
        Invoke("SetClosePlayerHit", 1f);//延迟1秒后，关闭受击状态

        if (Health <= 0)
        {
            //隐藏 Pacman
            Pacman.gameObject.SetActive(false);
            //游戏结束，先隐藏积分面板
            gamePanel.SetActive(false);
            //显示失败页面
            OpenOrCloseFailPanel();
            //再实例化结束面板
            //Instantiate(gameOverPrefab);
            //延迟3秒后，重载场景
            //Invoke("ReStart", 3f);
            //保存游戏得分
            SaveScore();
        }//当前血量归零
    }

    public void SetClosePlayerHit()
    {
        Pacman.GetComponent<Animator>().SetBool("Hit", false);//设置为非受击状态
    }

    //开局设置：吃豆人及怪物都不动，等待start()内传入布尔值启用
    private void SetGameState(bool state)
    {
        Pacman.GetComponent<PacmanMove>().enabled = state;
        Blinky.GetComponent<EnemyMove>().enabled = state;
        Cycle.GetComponent<EnemyMove>().enabled = state;
        Inky.GetComponent<EnemyMove>().enabled = state;
        Pinky.GetComponent<EnemyMove>().enabled = state;
    }

    //吃到豆子后
    public void OnEatPacdot(GameObject go)
    {
        //吃到普通豆子，从表内移除
        pacdotGos.Remove(go);

        //吃到的豆子数及得分增加
        nowEat++;
        score += 10;
    }

    //吃到暂停道具后
    public void OnEatPausePacdot()
    {
        //得分增加
        score += 20;

        if (Openshalou)
        {
            //延迟调用：延迟10s后调用创建暂停道具函数
            Invoke("CreatePausePacdot", 10f);
        }


        PauseEnemy(true);//暂停怪物


        //普通延时方法：Invoke("Recover", 10f);
        //协程延时方法：
        StartCoroutine(PauseRecover());
    }



    //吃到超级豆子后
    public void OnEatSuperPacdot()
    {
        //得分增加
        score += 20;

        if (Openshalou)
        {
            //延迟调用：延迟10s后调用创建超级豆子函数
            Invoke("CreateSuperPacdot", 10f);
        }


        //变更为超级状态，冻结敌人
        isSuperPacman = true;
        FreezeEnemy();


        //普通延时方法：Invoke("Recover", 10f);
        //协程延时方法：
        StartCoroutine(Recover());
    }

    //恢复状态，协程延时
    IEnumerator PauseRecover()
    {
        //等待4s：相当于持续超级吃豆人状态4s
        yield return new WaitForSeconds(4f);

        PauseEnemy(false);

    }

    //恢复状态，协程延时
    IEnumerator Recover()
    {
        //等待4s：相当于持续超级吃豆人状态4s
        yield return new WaitForSeconds(4f);

        Dis_FreezeEnemy();
        isSuperPacman = false;
    }

    //暂停AI怪物
    private void PauseEnemy(bool PausePacdot)
    {
        if (PausePacdot)
        {
            //冻结无法移动：禁用update方法
            Blinky.GetComponent<EnemyMove>().enabled = false;
            Cycle.GetComponent<EnemyMove>().enabled = false;
            Inky.GetComponent<EnemyMove>().enabled = false;
            Pinky.GetComponent<EnemyMove>().enabled = false;

            //变色：敌人图标变暗淡
            Blinky.GetComponent<SpriteRenderer>().color = new Color(0.7f, 0.7f, 0.7f, 0.7f);
            Cycle.GetComponent<SpriteRenderer>().color = new Color(0.7f, 0.7f, 0.7f, 0.7f);
            Inky.GetComponent<SpriteRenderer>().color = new Color(0.7f, 0.7f, 0.7f, 0.7f);
            Pinky.GetComponent<SpriteRenderer>().color = new Color(0.7f, 0.7f, 0.7f, 0.7f);

        }else
        {
            //解冻
            Blinky.GetComponent<EnemyMove>().enabled = true;
            Cycle.GetComponent<EnemyMove>().enabled = true;
            Inky.GetComponent<EnemyMove>().enabled = true;
            Pinky.GetComponent<EnemyMove>().enabled = true;
            
            //恢复原色
            Blinky.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
            Cycle.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
            Inky.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
            Pinky.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
        }
    }

    private void FreezeEnemy()
    {
            /*弃用冻结无法移动
            //冻结无法移动：禁用update方法
            Blinky.GetComponent<EnemyMove>().enabled = false;
            Cycle.GetComponent<EnemyMove>().enabled = false;
            Inky.GetComponent<EnemyMove>().enabled = false;
            Pinky.GetComponent<EnemyMove>().enabled = false;
            */

            /*弃用图标变暗淡
            //变色：敌人图标变暗淡
            Blinky.GetComponent<SpriteRenderer>().color = new Color(0.7f, 0.7f, 0.7f, 0.7f);
            Cycle.GetComponent<SpriteRenderer>().color = new Color(0.7f, 0.7f, 0.7f, 0.7f);
            Inky.GetComponent<SpriteRenderer>().color = new Color(0.7f, 0.7f, 0.7f, 0.7f);
            Pinky.GetComponent<SpriteRenderer>().color = new Color(0.7f, 0.7f, 0.7f, 0.7f);
            */

            //设置敌人为逃跑状态
            Blinky.GetComponent<Animator>().SetBool("Shy",true);
        Cycle.GetComponent<Animator>().SetBool("Shy", true);
        Inky.GetComponent<Animator>().SetBool("Shy", true);
        Pinky.GetComponent<Animator>().SetBool("Shy", true);
    }

    private void Dis_FreezeEnemy()
    {
        /*
        //解冻
        Blinky.GetComponent<EnemyMove>().enabled = true;
        Cycle.GetComponent<EnemyMove>().enabled = true;
        Inky.GetComponent<EnemyMove>().enabled = true;
        Pinky.GetComponent<EnemyMove>().enabled = true;
        */
        /*
        //恢复原色
        Blinky.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
        Cycle.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
        Inky.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
        Pinky.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
        */
        //解除逃跑状态
        //设置敌人为逃跑状态
        Blinky.GetComponent<Animator>().SetBool("Shy", false);
        Cycle.GetComponent<Animator>().SetBool("Shy", false);
        Inky.GetComponent<Animator>().SetBool("Shy", false);
        Pinky.GetComponent<Animator>().SetBool("Shy", false);
    }

    //按下 Win 图标，重载游戏
    public void OnWinButton()
    {
        SceneManager.LoadScene(0);
    }

    //按下 Persona 图标，跳转网页
    public void OnPersonaButton()
    {
        //Application.OpenURL("https://www.cnblogs.com/SouthBegonia");
    }

    //保存得分
    public void SaveScore()//保存前三
    {
        int L_1Score = PlayerPrefs.GetInt("1Score"); //读取得分(第一名)
        int L_2Score = PlayerPrefs.GetInt("2Score"); //读取得分(第二名)
        int L_3Score = PlayerPrefs.GetInt("3Score"); //读取得分(第三名)

        if (score >= L_1Score)//如果超过最高得分记录
        {
            PlayerPrefs.SetInt("1Score", score); //储存第一名新的最高记录得分
            PlayerPrefs.SetInt("2Score", L_1Score); //储存第二名新的最高记录得分
            PlayerPrefs.SetInt("3Score", L_2Score); //储存第三名新的最高记录得分
        }
        else if(score >= L_2Score)
        {
            PlayerPrefs.SetInt("2Score", score); //储存第二名新的最高记录得分
            PlayerPrefs.SetInt("3Score", L_2Score); //储存第三名新的最高记录得分
        }
        else if (score >= L_3Score)
        {
            PlayerPrefs.SetInt("3Score", score); //储存第三名新的最高记录得分
        }

    }
    //读取最高得分
    public int ReadScore()
    {
        int L_1Score = PlayerPrefs.GetInt("1Score"); //读取得分(第一名)
        int L_2Score = PlayerPrefs.GetInt("2Score"); //读取得分(第二名)
        int L_3Score = PlayerPrefs.GetInt("3Score"); //读取得分(第三名)
        string Ranktext = "";
        if (L_1Score > 0)//第一名得分有效
        {
            Ranktext += Ranktext + "1. " + L_1Score;
            if (L_2Score > 0)//第二名得分有效
            {
                Ranktext = Ranktext + "\n 2. " + L_2Score;
                if (L_3Score > 0)//第二名得分有效
                {
                    Ranktext = Ranktext + "\n 3. " + L_3Score;
                }
            }
        }
        RankText.text = Ranktext;//更新Text显示
        print(Ranktext);//打印测试
        return (L_1Score);
    }

    //保存音量设置
    public void SaveVolume()
    {
        CurrentVolume = Slider.value;//读取当前音量
        PlayerPrefs.SetFloat("Volume", CurrentVolume); //储存当前音量
        AudioSource.volume = CurrentVolume;//将音量设置同步到背景音乐组件中
    }

    //读取并设置音量设置
    public void ReadAndSetVolume()
    {
        float L_Volume = PlayerPrefs.GetFloat("Volume"); //读取音量
        Slider.value = L_Volume;//设置音量调
    }

    //时时更新UI
    private void Update()
    {
        //当吃完所有豆子
        if (pacdotNumber == nowEat && Pacman.GetComponent<PacmanMove>().enabled != false)
        {
            //隐藏积分面板和背景面板，显示胜利面板
            gamePanel.SetActive(false);

            winPrefab.SetActive(true);

            //保存得分
            SaveScore();

            //我需要的是实例化Win面板，且面板上的UI图标带有按键功能；
            //但下列代码实例化只能用于物体，且无法显示再面板，也无法进行按键赋值
            //Instantiate(winPrefab);


            //取消其他所有协程
            StopAllCoroutines();

            SetGameState(false);
        }

        if (pacdotNumber == nowEat)
        {
            //
            if (Input.GetKey(KeyCode.Z))
            {
                OnPersonaButton();

                //Invoke("", 5f);
                //SceneManager.LoadScene(0);
                //Application.OpenURL("https://www.cnblogs.com/SouthBegonia");
            }

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            {
                OnWinButton();
            }

        }

        //如果显示得分的UI gamePanel 为显示状态
        if (gamePanel.activeInHierarchy)
        {
            //修改UI文本
            remainText.text = "Remain:\n\n" + (pacdotNumber - nowEat);
            nowText.text = "Eaten:\n\n" + nowEat;
            scoreText.text = "Score:\n\n" + score;

            //游戏计时器
            GameTimer += Time.deltaTime;
            if (GameTimer >= 1)
            {
                CurrentGameTime++;
                GameTimer = 0F;
            }
            //修改UI文本
            GameTime.text = "GameTime:\n\n" + CurrentGameTime + " S";

        }
    }

}
