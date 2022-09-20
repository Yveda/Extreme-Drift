using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public enum Panels { MainMenu = 0, SelectVehicle = 1, SelectLevel = 2, Settings = 3 }
public class MainMenu : MonoBehaviour
{
    private int gameScore { get; set; }//金币

    public float cameraRotateSpeed = 5;//视角移动速度
    public Animator FadeBackGround; //加载背景

    public AudioSource menuMusic;    //背景音乐
    public Transform vehicleRoot;    //简单的go
    public Material[] allRestMaterials;   //所有材质

    public MenuPanels menuPanels;    //主界面：含有五个场景对象
    public MenuGUI menuGUI;    //界面gui，含有多个对象
    public VehicleSetting[] vehicleSetting;    //车辆数组，保存了汽车的各种信息
    public LevelSetting[] levelSetting;    //关卡数组，保存了关卡的信息

    //游戏的gui
    [System.Serializable]
    public class MenuGUI
    {
        public Text GameScore;
        public Text VehicleName;
        public Text VehiclePrice;

        public Slider VehicleSpeed;
        public Slider VehicleBraking;
        public Slider VehicleNitro;

        public Slider sensitivity;

        public Toggle audio;
        public Toggle music;
        public Toggle vibrateToggle;
        public Toggle ButtonMode, AccelMode;

        public Image wheelColor, smokeColor;
        public Image loadingBar;

        public GameObject loading;
        public GameObject customizeVehicle;
        public GameObject buyNewVehicle;
    }

    //五个场景的panel
    [System.Serializable]
    public class MenuPanels
    {
        public GameObject MainMenu;
        public GameObject SelectVehicle;
        public GameObject SelectLevel;
        public GameObject EnoughMoney;
        public GameObject Settings;
    }

    //车辆信息
    [System.Serializable]
    public class VehicleSetting
    {
        public string name = "Vehicle 1";

        public int price = 20000;

        public GameObject vehicle;
        public GameObject wheelSmokes;

        public Material ringMat, smokeMat;
        public Transform rearWheels;

        public VehiclePower vehiclePower;

        [HideInInspector]
        public bool Bought = false; //是否购买，隐藏

        //车辆性能参数
        [System.Serializable]
        public class VehiclePower
        {
            public float speed = 80;
            public float braking = 1000;
            public float nitro = 10;
        }
    }

    //可以使用的几张地图信息
    [System.Serializable]
    public class LevelSetting
    {
        public bool locked = true;
        public Button panel;
        public Text bestTime;
        public Image lockImage;
        public StarClass stars;

        //该地图获得了几颗星
        [System.Serializable]
        public class StarClass
        {
            public Image Star1, Star2, Star3;
        }
    }

    private Panels activePanel = Panels.MainMenu;    //默认以第一个界面开启

    private bool vertical, horizontal;

    private Vector2 startPos;

    private Vector2 touchDeltaPosition;

    private float x, y = 0;    //鼠标按下位置记录

    private VehicleSetting currentVehicle; //当前车辆以及各种信息

    private int currentVehicleNumber = 0;

    private int currentLevelNumber = 0;

    private Color mainColor;

    private bool randomColorActive = false;

    private bool startingGame = false;

    private float menuLoadTime = 0.0f;

    private AsyncOperation sceneLoadingOperation = null;


    //ControlMode： 用于设置操作模式，要么用按钮的控制左右，要么用重力控制左右//////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //用按钮控制左右
    public void ControlModeButtons(Toggle value)
    {
        if (value.isOn)
            PlayerPrefs.SetString("ControlMode", "Buttons");
    }
    //用重力控制左右
    public void ControlModeAccel(Toggle value)
    {
        if (value.isOn)
            PlayerPrefs.SetString("ControlMode", "Accel");
    }


    public void DisableVibration(Toggle toggle)
    {
        if (toggle.isOn)
            PlayerPrefs.SetInt("VibrationActive", 0);
        else
            PlayerPrefs.SetInt("VibrationActive", 1);
    }

    //Vehcile Color//////////////////////////////////////////////////////////////////////////////////////////////////////////////


    public void ActiveCurrentColor(Image activeImage)
    {

        mainColor = activeImage.color;

        //根据传参持久化数据
        if (menuGUI.wheelColor.gameObject.activeSelf)
        {
            vehicleSetting[currentVehicleNumber].ringMat.SetColor("_Color", mainColor);
            PlayerPrefsX.SetColor("VehicleWheelsColor" + currentVehicleNumber, mainColor);
        }
        else if (menuGUI.smokeColor.gameObject.activeSelf)
        {
            vehicleSetting[currentVehicleNumber].smokeMat.SetColor("_TintColor", new Color(mainColor.r, mainColor.g, mainColor.b, 0.2f));
            PlayerPrefsX.SetColor("VehicleSmokeColor" + currentVehicleNumber, new Color(mainColor.r, mainColor.g, mainColor.b, 0.2f));
        }
    }

    //设置车轮颜色
    public void ActiveWheelColor(Image activeImage)
    {
        randomColorActive = false;

        activeImage.gameObject.SetActive(true);
        menuGUI.wheelColor = activeImage;
        menuGUI.smokeColor.gameObject.SetActive(false);
    }

    //设置烟雾颜色
    public void ActiveSmokeColor(Image activeImage)
    {
        randomColorActive = false;

        activeImage.gameObject.SetActive(true);
        menuGUI.smokeColor = activeImage;
        menuGUI.wheelColor.gameObject.SetActive(false);
    }


    public void OutCustomizeVehicle()
    {
        randomColorActive = false;
        menuGUI.wheelColor.gameObject.SetActive(false);
        menuGUI.smokeColor.gameObject.SetActive(false);
    }

    //随机颜色
    public void RandomColor()
    {

        randomColorActive = true;

        menuGUI.wheelColor.gameObject.SetActive(false);
        menuGUI.smokeColor.gameObject.SetActive(false);

        //随机数函数
        vehicleSetting[currentVehicleNumber].ringMat.SetColor("_Color", new Color(Random.Range(0.0f, 1.1f), Random.Range(0.0f, 1.1f), Random.Range(0.0f, 1.1f)));
        vehicleSetting[currentVehicleNumber].smokeMat.SetColor("_TintColor", new Color(Random.Range(0.0f, 1.1f), Random.Range(0.0f, 1.1f), Random.Range(0.0f, 1.1f), 0.2f));

        //持久化
        PlayerPrefsX.SetColor("VehicleWheelsColor" + currentVehicleNumber, vehicleSetting[currentVehicleNumber].ringMat.GetColor("_Color"));
        PlayerPrefsX.SetColor("VehicleSmokeColor" + currentVehicleNumber, vehicleSetting[currentVehicleNumber].smokeMat.GetColor("_TintColor"));
    }


    //Share//////////////////////////////////////////////////////////////////////////////////////////////////////////////

    //点击设置按钮后跳转到另一个页面
    public void SettingActive(bool activePanel)
    {
        menuPanels.Settings.gameObject.SetActive(activePanel);
    }

    //退出：绑定了Shadow、点击音效的一个按钮，以及一个MainMenu的函数：就是简单的退出
    public void ClickExitButton()
    {
        Application.Quit();
    }


    //GamePanels//////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public void CurrentPanel(int current)
    {
        //这里为什么要传一个INT呢？因为开头有定义
        //public enum Panels { MainMenu = 0, SelectVehicle = 1, SelectLevel = 2, Settings = 3 }
        //activePanel默认为0，这里传入的是1，就是选车
        activePanel = (Panels)current;

        //PlayerPrefs是数据持久化，从存档取出数据验证
        if (currentVehicleNumber != PlayerPrefs.GetInt("CurrentVehicle"))
        {
            currentVehicleNumber = PlayerPrefs.GetInt("CurrentVehicle");
            //循环所有的车辆
            foreach (VehicleSetting VSetting in vehicleSetting)
            {
                //当前车激活状态，否则不激活
                if (VSetting == vehicleSetting[currentVehicleNumber])
                {
                    VSetting.vehicle.SetActive(true);
                    currentVehicle = VSetting;
                }
                else
                {
                    VSetting.vehicle.SetActive(false);
                }
            }
        }

        //根据传入值做一些操作
        switch (activePanel)
        {

            case Panels.MainMenu:
                menuPanels.MainMenu.SetActive(true);
                menuPanels.SelectVehicle.SetActive(false);
                menuPanels.SelectLevel.SetActive(false);
                if (menuGUI.wheelColor) menuGUI.wheelColor.gameObject.SetActive(true);

                break;
            //这里传入的是1，进入选车
            case Panels.SelectVehicle:
                menuPanels.MainMenu.gameObject.SetActive(false);
                menuPanels.SelectVehicle.SetActive(true);
                menuPanels.SelectLevel.SetActive(false);
                break;
            case Panels.SelectLevel:
                menuPanels.MainMenu.SetActive(false);
                menuPanels.SelectVehicle.SetActive(false);
                menuPanels.SelectLevel.SetActive(true);
                break;
            case Panels.Settings:
                menuPanels.MainMenu.SetActive(false);
                menuPanels.SelectVehicle.SetActive(false);
                menuPanels.SelectLevel.SetActive(false);
                break;
        }
    }

    //Vehicles Switch//////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //车辆购买
    public void BuyVehicle()
    {
        //金币足够或者车辆未买才会执行
        if ((gameScore >= vehicleSetting[currentVehicleNumber].price) && !vehicleSetting[currentVehicleNumber].Bought)
        {
            //数据持久化
            PlayerPrefs.SetInt("BoughtVehicle" + currentVehicleNumber.ToString(), 1);
            //减钱
            gameScore -= vehicleSetting[currentVehicleNumber].price;
            //防止负数
            if (gameScore <= 0) { gameScore = 1; }
            //保存金币数量
            PlayerPrefs.SetInt("GameScore", gameScore);
            //保存车辆购买状态
            vehicleSetting[currentVehicleNumber].Bought = true;
        }
        else
        {
            // 当买车后金币不足弹出的页面
            menuPanels.EnoughMoney.SetActive(true);
        }
    }

    //在车库里面点击后出现下一个车辆
    public void NextVehicle()
    {
        if (menuGUI.wheelColor) { menuGUI.wheelColor.gameObject.SetActive(false); }

        currentVehicleNumber++;
        //取模运算，防止越界
        currentVehicleNumber = (int)Mathf.Repeat(currentVehicleNumber, vehicleSetting.Length);

        foreach (VehicleSetting VSetting in vehicleSetting)
        {

            if (VSetting == vehicleSetting[currentVehicleNumber])
            {
                VSetting.vehicle.SetActive(true);
                //循环到下一辆车赋值给当前
                currentVehicle = VSetting;
            }
            else
            {
                VSetting.vehicle.SetActive(false);

            }
        }
    }

    // 在车库里面点击后出现上一个车辆
    public void PreviousVehicle()
    {
        if (menuGUI.wheelColor) { menuGUI.wheelColor.gameObject.SetActive(false); }

        currentVehicleNumber--;
        currentVehicleNumber = (int)Mathf.Repeat(currentVehicleNumber, vehicleSetting.Length);

        foreach (VehicleSetting VSetting in vehicleSetting)
        {
            if (VSetting == vehicleSetting[currentVehicleNumber])
            {
                VSetting.vehicle.SetActive(true);
                currentVehicle = VSetting;
            }
            else
            {
                VSetting.vehicle.SetActive(false);
            }
        }
    }

    //GameSettings//////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //设置画质
    public void QualitySetting(int quality)
    {
        //直接调用系统画质api
        QualitySettings.SetQualityLevel(quality - 1, true);
        //然后持久化保存数据
        PlayerPrefs.SetInt("QualitySettings", quality);
    }

    //保存敏感系数函数：
    public void EditSensitivity()
    {
        PlayerPrefs.SetFloat("Sensitivity", menuGUI.sensitivity.value);
    }

    //sound音量设置下是两个toggle：选项框绑定函数：设置后保存
    //是否开启音效
    public void DisableAudioButton(Toggle toggle)
    {
        if (toggle.isOn)
        {
            AudioListener.volume = 1;
            PlayerPrefs.SetInt("AudioActive", 0);
        }
        else
        {
            AudioListener.volume = 0;
            PlayerPrefs.SetInt("AudioActive", 1);
        }
    }

    //是否开启音乐
    public void DisableMusicButton(Toggle toggle)
    {
        if (toggle.isOn)
        {
            menuMusic.GetComponent<AudioSource>().mute = false;
            PlayerPrefs.SetInt("MusicActive", 0);
        }
        else
        {
            menuMusic.GetComponent<AudioSource>().mute = true;
            PlayerPrefs.SetInt("MusicActive", 1);
        }
    }

    //resetPanel是一个红色背景和选择yes或者no
    public void EraseSave()
    {
        //清除所有数据后重新加载
        PlayerPrefs.DeleteAll();
        currentVehicleNumber = 0;
        Application.LoadLevel(0);

        foreach (Material mat in allRestMaterials)
            mat.SetColor("_Color", new Color(0.7f, 0.7f, 0.7f));
    }


    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public void StartGame()
    {
        //防止出错
        if (startingGame) return;
        //设置背景
        FadeBackGround.SetBool("FadeOut", true);
        //调用协程加载（由于不能直接调用c#的多线程，这个函数实现多线程）
        StartCoroutine(LoadLevelGame(1.5f));
        startingGame = true;
    }


    IEnumerator LoadLevelGame(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        //激活加载页面
        menuGUI.loading.SetActive(true);
        //加载
        StartCoroutine(LoadLevelAsync());

    }

    IEnumerator LoadLevelAsync()
    {

        yield return new WaitForSeconds(0.4f);

        sceneLoadingOperation = Application.LoadLevelAsync(currentLevelNumber + 1);
        sceneLoadingOperation.allowSceneActivation = false;

        while (!sceneLoadingOperation.isDone || sceneLoadingOperation.progress < 0.9f)
        {
            menuLoadTime += Time.deltaTime;

            yield return 0;
        }
    }

    // levels是一个带有scroll rect的image，内含group，下有五张地图对象
    // 每一个地图下有star1，star2，star3，简单的image表实获取到的成绩，以及一个locked表示是否解锁，本质是一个简单的image
    // 还有一个currentlevel对象，简单的image背景，里面有两个简单的text表示曾经获取的成绩
    // 每一张地图对象只是一个图片，并不实际含有地图，并且绑定一个函数，根据传参进行下一步：
    public void currentLevel(int current)
    {

        for (int i = 0; i < levelSetting.Length; i++)
        {
            if (i == current)
            {
                currentLevelNumber = current;
                levelSetting[i].panel.image.color = Color.white;
                levelSetting[i].panel.enabled = true;
                levelSetting[i].lockImage.gameObject.SetActive(false);
                PlayerPrefs.SetInt("CurrentLevelNumber", currentLevelNumber);
            }
            else if (levelSetting[i].locked == false)
            {
                levelSetting[i].panel.image.color = new Color(0.3f, 0.3f, 0.3f);
                levelSetting[i].panel.enabled = true;
                levelSetting[i].lockImage.gameObject.SetActive(false);
            }
            else
            {
                levelSetting[i].panel.image.color = new Color(1.0f, 0.5f, 0.5f);
                levelSetting[i].panel.enabled = false;
                levelSetting[i].lockImage.gameObject.SetActive(true);
            }

            //根据保存过的信息进行显示
            if (levelSetting[i].bestTime)
            {
                if (PlayerPrefs.GetFloat("BestTime" + (i + 1).ToString()) != 0.0f)
                {
                    if (PlayerPrefs.GetInt("LevelStar" + (i + 1)) == 1)
                    {
                        levelSetting[i].stars.Star1.color = Color.white;
                    }
                    else if (PlayerPrefs.GetInt("LevelStar" + (i + 1)) == 2)
                    {
                        levelSetting[i].stars.Star1.color = Color.white;
                        levelSetting[i].stars.Star2.color = Color.white;
                    }
                    else if (PlayerPrefs.GetInt("LevelStar" + (i + 1)) == 3)
                    {
                        levelSetting[i].stars.Star1.color = Color.white;
                        levelSetting[i].stars.Star2.color = Color.white;
                        levelSetting[i].stars.Star3.color = Color.white;
                    }

                    levelSetting[i].bestTime.text = "BEST : " + GetComponent<FormatSecondsScript>().FormatSeconds(PlayerPrefs.GetFloat("BestTime" + (i + 1))).ToString();
                }
            }
        }
    }




    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //初始化
    void Awake()
    {

        AudioListener.pause = false;
        Time.timeScale = 1.0f;

        //震动选项，这里没用
        menuGUI.vibrateToggle.isOn = (PlayerPrefs.GetInt("VibrationActive") == 0) ? true : false;

        //默认金币999999
        gameScore = 999999;

        //加载第一个场景
        CurrentPanel(0);

        //设置画质
        if (PlayerPrefs.GetInt("QualitySettings") == 0)
        {
            PlayerPrefs.SetInt("QualitySettings", 4);
            QualitySettings.SetQualityLevel(3, true);
        }
        else
        {
            QualitySettings.SetQualityLevel(PlayerPrefs.GetInt("QualitySettings") - 1, true);
        }

        //设置车辆敏感度
        if (PlayerPrefs.GetFloat("Sensitivity") == 0.0f)
        {
            menuGUI.sensitivity.value = 1.0f;
            PlayerPrefs.SetFloat("Sensitivity", 1.0f);
        }
        else
        {
            menuGUI.sensitivity.value = PlayerPrefs.GetFloat("Sensitivity");
        }

        //设置操作模式
        switch (PlayerPrefs.GetString("ControlMode"))
        {
            case "":
                menuGUI.ButtonMode.isOn = true;
                break;
            case "Buttons":
                menuGUI.ButtonMode.isOn = true;
                break;
            case "Accel":
                menuGUI.AccelMode.isOn = true;
                break;
        }

        //当前车辆
        currentLevelNumber = PlayerPrefs.GetInt("CurrentLevelNumber");

        //地图解锁情况处理
        for (int lvls = 0; lvls < levelSetting.Length; lvls++)
        {
            if (lvls <= PlayerPrefs.GetInt("CurrentLevelUnlocked"))
                levelSetting[lvls].locked = false;

        }

        //设置地图
        currentLevel(currentLevelNumber);

        //读取默认控制信息设置
        switch (PlayerPrefs.GetString("ControlMode"))
        {
            case "":
                PlayerPrefs.SetString("ControlMode", "Buttons");
                menuGUI.ButtonMode.isOn = true;
                break;
            case "Buttons":
                menuGUI.ButtonMode.isOn = true;
                break;
            case "Accel":
                menuGUI.AccelMode.isOn = true;
                break;
        }

        //默认只买了第一辆车
        PlayerPrefs.SetInt("BoughtVehicle0", 1);

        //audio and music Toggle
        //背景音乐和音量设置
        menuGUI.audio.isOn = (PlayerPrefs.GetInt("AudioActive") == 0) ? true : false;
        AudioListener.volume = (PlayerPrefs.GetInt("AudioActive") == 0) ? 1.0f : 0.0f;

        menuGUI.music.isOn = (PlayerPrefs.GetInt("MusicActive") == 0) ? true : false;
        menuMusic.mute = (PlayerPrefs.GetInt("MusicActive") == 0) ? false : true;

        currentVehicleNumber = PlayerPrefs.GetInt("CurrentVehicle");
        currentVehicle = vehicleSetting[currentVehicleNumber];

        int i = 0;

        //遍历所有车辆，进行轮胎颜色烟雾设置
        foreach (VehicleSetting VSetting in vehicleSetting)
        {

            if (PlayerPrefsX.GetColor("VehicleWheelsColor" + i) == Color.clear)
            {
                vehicleSetting[i].ringMat.SetColor("_DiffuseColor", Color.white);
            }
            else
            {
                vehicleSetting[i].ringMat.SetColor("_DiffuseColor", PlayerPrefsX.GetColor("VehicleWheelsColor" + i));
            }



            if (PlayerPrefsX.GetColor("VehicleSmokeColor" + i) == Color.clear)
            {
                vehicleSetting[i].smokeMat.SetColor("_TintColor", new Color(0.8f, 0.8f, 0.8f, 0.2f));
            }
            else
            {
                vehicleSetting[i].smokeMat.SetColor("_TintColor", PlayerPrefsX.GetColor("VehicleSmokeColor" + i));
            }

            //bought=true说明车没有买
            if (PlayerPrefs.GetInt("BoughtVehicle" + i.ToString()) == 1)
            {
                VSetting.Bought = true;

                if (PlayerPrefs.GetInt("GameScore") == 0)
                {
                    PlayerPrefs.SetInt("GameScore", gameScore);
                }
                else
                {
                    gameScore = PlayerPrefs.GetInt("GameScore");
                }
            }

            //显示当前车辆
            if (VSetting == vehicleSetting[currentVehicleNumber])
            {
                VSetting.vehicle.SetActive(true);
                currentVehicle = VSetting;
            }
            else
            {
                VSetting.vehicle.SetActive(false);
            }

            i++;
        }
    }


    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    void Update()
    {
        //如果加载了下一个scene
        if (sceneLoadingOperation != null)
        {
            //根据时间显示加载轮胎动画（圆形逐渐圆满）
            /*public static vector3 movetowards(vector3 current, vector3 target, float maxdistancedelta);
            作用是将当前值current移向目标target。（对vector3是沿两点间直线）
            maxdistancedelta就是每次移动的最大长度。
            返回值是当current值加上maxdistancedelta的值，如果这个值超过了target，返回的就是target的值*/
            menuGUI.loadingBar.fillAmount = Mathf.MoveTowards(menuGUI.loadingBar.fillAmount, sceneLoadingOperation.progress + 0.2f, Time.deltaTime * 0.5f);

            //加载完成
            if (menuGUI.loadingBar.fillAmount > sceneLoadingOperation.progress)
                sceneLoadingOperation.allowSceneActivation = true;
        }


        if (menuGUI.smokeColor.gameObject.activeSelf || randomColorActive)
        {
            //车辆的后轮旋转
            vehicleSetting[currentVehicleNumber].rearWheels.Rotate(1000 * Time.deltaTime, 0, 0);
            //开启车辆轮胎烟雾
            vehicleSetting[currentVehicleNumber].wheelSmokes.SetActive(true);
        }
        else
        {
            vehicleSetting[currentVehicleNumber].wheelSmokes.SetActive(false);
        }

        //车辆参数的显示
        menuGUI.VehicleSpeed.value = vehicleSetting[currentVehicleNumber].vehiclePower.speed / 100.0f;
        menuGUI.VehicleBraking.value = vehicleSetting[currentVehicleNumber].vehiclePower.braking / 100.0f;
        menuGUI.VehicleNitro.value = vehicleSetting[currentVehicleNumber].vehiclePower.nitro / 100.0f;
        menuGUI.GameScore.text = gameScore.ToString();

        //如果车已购买
        if (vehicleSetting[currentVehicleNumber].Bought)
        {
            //一些简单的设置
            menuGUI.customizeVehicle.SetActive(true);
            menuGUI.buyNewVehicle.SetActive(false);

            menuGUI.VehicleName.text = vehicleSetting[currentVehicleNumber].name;
            menuGUI.VehiclePrice.text = "BOUGHT";
            PlayerPrefs.SetInt("CurrentVehicle", currentVehicleNumber);
        }
        else
        {
            //车没买的话一些简单的设置
            menuGUI.customizeVehicle.SetActive(false);
            menuGUI.buyNewVehicle.SetActive(true);

            menuGUI.VehicleName.text = vehicleSetting[currentVehicleNumber].name;
            menuGUI.VehiclePrice.text = "COST: " + vehicleSetting[currentVehicleNumber].price.ToString();
        }



#if UNITY_STANDALONE || UNITY_WEBGL || UNITY_EDITOR

        if (Input.GetMouseButton(0) && activePanel != Panels.SelectLevel)
        {
            // 鼠标点击屏幕出现是视觉效果和旋转效果
            x = Mathf.Lerp(x, Mathf.Clamp(Input.GetAxis("Mouse X"), -2, 2) * cameraRotateSpeed, Time.deltaTime * 5.0f);
            Camera.main.fieldOfView = Mathf.Clamp(Camera.main.fieldOfView, 50, 60);
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, 50, Time.deltaTime);
        }
        else
        {
            x = Mathf.Lerp(x, cameraRotateSpeed * 0.01f, Time.deltaTime * 5.0f);
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, 60, Time.deltaTime);
        }


#elif UNITY_ANDROID || UNITY_IOS



        if (Input.touchCount == 1&& activePanel!=Panels.SelectLevel)
        {
            switch (Input.GetTouch(0).phase)
            {
                case TouchPhase.Moved:
                    x = Mathf.Lerp(x, Mathf.Clamp(Input.GetTouch(0).deltaPosition.x, -2, 2) * cameraRotateSpeed, Time.deltaTime*3.0f);
                    Camera.main.fieldOfView = Mathf.Clamp(Camera.main.fieldOfView, 50, 60);
                    Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, 50, Time.deltaTime);
                    break;
            }

        }
        else {
            x = Mathf.Lerp(x, cameraRotateSpeed * 0.02f, Time.deltaTime*3.0f);
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, 60, Time.deltaTime);
        }

#endif
        //慢慢旋转视角
        transform.RotateAround(vehicleRoot.position, Vector3.up, x);
    }

}
