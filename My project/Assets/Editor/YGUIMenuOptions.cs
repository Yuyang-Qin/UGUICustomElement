using System.ComponentModel.Design;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using MenuCommand = UnityEditor.MenuCommand;

public static class YGUIMenuOptions
{
    [MenuItem("GameObject/YGUI/TouchList", false, 2027)]
    public static void AddTouchList(MenuCommand menuCommand)
    {
        GameObject go = new GameObject("TouchList", typeof(RectTransform));
        go.AddComponent<YTouchList>();

        go.transform.SetParent(FindFirstCanvas(menuCommand).transform, false);

        Selection.activeGameObject = go;
    }

    private static GameObject CreateCanvas()
    {
        GameObject canvasGO = ObjectFactory.CreateGameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        if (Object.FindAnyObjectByType<EventSystem>(FindObjectsInactive.Include) == null)
        {
            GameObject esGO = ObjectFactory.CreateGameObject("EventSystem");
            ObjectFactory.AddComponent<EventSystem>(esGO);
            ObjectFactory.AddComponent<StandaloneInputModule>(esGO);
        }

        return canvasGO;
    }

    private static GameObject FindFirstCanvas(MenuCommand menuCommand)
    {
        GameObject go = menuCommand.context as GameObject;
        if (go == null || go.GetComponentInParent<Canvas>() == null)
        {
            go = GameObject.FindFirstObjectByType<Canvas>()?.gameObject ?? CreateCanvas();
        }
        return go;
    }
}
