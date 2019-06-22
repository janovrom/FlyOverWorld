using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using Assets.Scripts.Cameras;
using Assets.Scripts.Gui;
using System.Collections.Generic;


[ExecuteInEditMode]
public class Helper : MonoBehaviour
{

    public bool update = false;
    public Font font;

    // Use this for initialization
    void Start()
    {
    }

#if UNITY_EDITOR
    // Add a menu item named "Do Something with a Shortcut Key" to MyMenu in the menu bar
    // and give it a shortcut (ctrl-g on Windows, cmd-g on macOS).
    //[MenuItem("MyMenu/Add ColorChangeableImage %g")]
    static void AddColorChangeableImage()
    {
        foreach (GameObject o in UnityEditor.Selection.gameObjects)
        {
            ColorChangableImage cci = o.GetComponent<ColorChangableImage>();
            if (cci == null)
                cci = o.AddComponent<ColorChangableImage>();
            cci.ColorImage = o.GetComponent<Image>();
            cci.UseTextColor = false;

            cci.ChangeColor();
        }
    }

    //[MenuItem("MyMenu/UseDarkening #g")]
    static void UseDarkeningChangableImage()
    {
        foreach (GameObject o in UnityEditor.Selection.gameObjects)
        {
            ColorChangableImage cci = o.GetComponent<ColorChangableImage>();
            if (cci == null)
                cci = o.AddComponent<ColorChangableImage>();
            cci.UseDarkening = true;
            cci.ChangeColor();
        }
    }

    //[MenuItem("MyMenu/DisableDarkening #h")]
    static void DisableDarkeningChangableImage()
    {
        foreach (GameObject o in UnityEditor.Selection.gameObjects)
        {
            ColorChangableImage cci = o.GetComponent<ColorChangableImage>();
            if (cci == null)
                cci = o.AddComponent<ColorChangableImage>();
            cci.UseDarkening = false;
            cci.ChangeColor();
        }
    }

    //[MenuItem("MyMenu/Add ColorChangeableImage %h")]
    static void AddColorChangeableImageInverted()
    {
        foreach (GameObject o in UnityEditor.Selection.gameObjects)
        {
            ColorChangableImage cci = o.GetComponent<ColorChangableImage>();
            if (cci == null)
                cci = o.AddComponent<ColorChangableImage>();
            cci.ColorImage = o.GetComponent<Image>();
            cci.UseTextColor = true;
            cci.ChangeColor();
        }
    }

    //[MenuItem("MyMenu/Add ColorChangeableText %j")]
    static void AddColorChangeableText()
    {
        foreach (GameObject o in UnityEditor.Selection.gameObjects)
        {
            ColorChangableText cct = o.GetComponent<ColorChangableText>();
            if (cct == null)
                cct = o.AddComponent<ColorChangableText>();

            cct.ColorText = o.GetComponent<Text>();
            cct.ChangeColor();
        }
    }

    [UnityEditor.MenuItem("MyMenu/Screenshot")]
    static void Screenshot()
    {
        ScreenCapture.CaptureScreenshot("../../screenshots/Screenshot" + System.DateTime.Now.ToString("_HH_mm_ss") + ".png", 4);
        //Application.CaptureScreenshot("C:/Users/roman/OneDrive/DP/screenshots/Screenshot" + System.DateTime.Now.ToString("_HH_mm_ss") + ".png", 4);
    }
#endif

    void Help()
    {
        foreach (Text i in FindObjectsOfType<Text>())
        {
            i.font = font;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //if (update)
        //{
        //    Help();
        //    Debug.Log("Updated");
        //    update = false;
        //}
    }
}
