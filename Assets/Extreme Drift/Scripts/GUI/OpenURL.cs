using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenURL : MonoBehaviour
{

    //资源和油管都是链接，本质一个按钮带Shadow和Audio，绑定OpenURL脚本的函数：传入URL，打开，简单
    public void OpenTab(string URL)
    {
        Application.OpenURL(URL);
    }

}
