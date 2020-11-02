using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class Photobooth : MonoBehaviour
{
    public string outputDirectoryPath;
    public Vector2Int imageDimensions;
    public float distance;

    private new Camera camera;

    private void OnValidate()
    {
        transform.localPosition = transform.forward * -distance;
    }

    [ContextMenu("Capture")]
    public void Capture()
    {
        Capture("Capture");
    }

    public Texture2D Capture(string fileName, bool overwrite = false)
    {
        if (camera == null)
            camera = GetComponent<Camera>();
        if (imageDimensions.x <= 0 || imageDimensions.y <= 0)
            throw new System.Exception("Image dimensions <= 0");

        if (camera.targetTexture == null || camera.targetTexture.width != imageDimensions.x || camera.targetTexture.height != imageDimensions.y)
        {
            if (camera.targetTexture != null)
                Destroy(camera.targetTexture);
            camera.targetTexture = new RenderTexture(imageDimensions.x, imageDimensions.y, 0);
        }

        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = camera.targetTexture;

        camera.Render();

        Texture2D texture = new Texture2D(camera.targetTexture.width, camera.targetTexture.height);
        texture.ReadPixels(new Rect(0, 0, camera.targetTexture.width, camera.targetTexture.height), 0, 0);
        texture.Apply();
        RenderTexture.active = currentRT;

        byte[] bytes = texture.EncodeToPNG();
        DestroyImmediate(texture);

        if (!Directory.Exists(Application.dataPath + "/" + outputDirectoryPath))
            Directory.CreateDirectory(Application.dataPath + "/" + outputDirectoryPath);

        string nameAndNumber = fileName;

        if (!overwrite && File.Exists(Application.dataPath + "/" + outputDirectoryPath + "/" + fileName + ".png"))
        {
            int number = 0;
            nameAndNumber = fileName + "_" + number;
            while (true)
            {
                if (!File.Exists(Application.dataPath + "/" + outputDirectoryPath + "/" + nameAndNumber + ".png"))
                    break;
                number++;
            }
        }

        File.WriteAllBytes(Application.dataPath + "/" + outputDirectoryPath + "/" + nameAndNumber + ".png", bytes);
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        string assetPath = "Assets/" + outputDirectoryPath + "/" + nameAndNumber + ".png";

        texture = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Texture2D)) as Texture2D;

        EditorGUIUtility.PingObject(texture);

        return texture;
    }

    public GameObject[] array;

    [ContextMenu("Capture All")]
    public void CaptureAll()
    {
        foreach (GameObject o in array)
            o.SetActive(false);

        foreach(GameObject o in array)
        {
            o.SetActive(true);
            Capture(o.name, true);
            o.SetActive(false);
        }

        foreach (GameObject o in array)
            o.SetActive(true);
    }
}
