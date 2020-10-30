using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ScreenCaptureController : MonoBehaviour {
    public Button button1;
    public Button button2;
    public Button button3;
    public Camera mainCamera;
    public Camera extraCamera;
    void Start()
    {
        button1.onClick.AddListener(delegate { CaptureFunc(GetPath()); });
        button2.onClick.AddListener(delegate { StartCoroutine(Texture2DCapture(GetPath())); });
        button3.onClick.AddListener(delegate { CameraCapture(GetPath()); });

    }
    /// <summary>
    /// Unity自带的截屏功能，保存当前画面
    /// </summary>
    /// <param name="filePath"></param>
    public void CaptureFunc(string filePath)
    {
        ScreenCapture.CaptureScreenshot(filePath);
        Debug.Log("全屏截图文件路径保存在" + filePath);
        //刷新unity目录
        RefreshResource();
    }

    /// <summary>
    /// 普通截屏
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public IEnumerator Texture2DCapture(string filePath)
    {
        yield return new WaitForEndOfFrame();
        //1.起始的X坐标，Y坐标，宽、高
        var rect = new Rect(0, 0, Screen.width, Screen.height);
        Texture2D tex = new Texture2D((int)rect.width, (int)rect.height);
        //2.读取像素、后面两个参数分别是X轴的偏移量，Y轴偏移量
        tex.ReadPixels(rect, 0, 0);
        //3.保存数据
        tex.Apply();
        //4.将数据转换成PNG数据
        byte[] bytes = tex.EncodeToPNG();
        //5.将数据写成文件
        File.WriteAllBytes(filePath, bytes);
        Debug.Log("Texture2D截图文件路径保存在" + filePath);
        //6.刷新unity目录
        RefreshResource();
    }

    /// <summary>
    /// 刷新Unity的目录
    /// </summary>
    private static void RefreshResource()
    {
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }

    /// <summary>
    /// Texture2D生成图片
    /// </summary>
    /// <param name="filePath"></param>
    public void CameraCapture(string filePath)
    {
        
        var camera = mainCamera;  //设置截图相机
        var rect = new Rect(0, 0, Screen.width, Screen.height);
        RenderTexture renderTexture = new RenderTexture((int)rect.width, (int)rect.height,32);       
        camera.targetTexture = renderTexture;  //设置相机的renderTexture
        camera.Render();        //手机开启相机的渲染
        RenderTexture.active = renderTexture;        //当前活动的渲染纹理
        Texture2D tex = new Texture2D((int)rect.width, (int)rect.height);
        tex.ReadPixels(rect, 0, 0);
        tex.Apply();
        RenderTexture.active = null;        //重置当前活动的渲染纹理
        camera.targetTexture = null;        //重置相机的targetTexture


        //设置第二个截图相机
        camera = extraCamera;
        camera.targetTexture = renderTexture;
        camera.Render();
        RenderTexture.active = renderTexture;
        tex.ReadPixels(rect, 0, 0);
        tex.Apply();
        //重置当前活动的渲染纹理
        camera.targetTexture = null;
        camera = null;
        RenderTexture.active = null;
        //删除RenderTexture对象
        Destroy(renderTexture);
        //写成图片文件
        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(filePath, bytes);
        Debug.Log("多相机截图文件路径保存在" + filePath);


        //刷新unity目录
        RefreshResource();
        //回收垃圾
        Resources.UnloadUnusedAssets();
        GC.Collect();      
    }

    public string GetPath()
    {
        return Application.dataPath + "/截图" + UnityEngine.Random.Range(0, 1000) + ".Png";
    }
}
