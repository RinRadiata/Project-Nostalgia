using UnityEngine;
using UnityEngine.Video;
using System.Collections.Generic;

public class GraphicLayer
{
    public const string LAYER_OBJECT_NAME_FORMAT = "Layer {0}";
    public int layerDepth = 0; // The depth of the layer, used for sorting and rendering order
    public Transform panel; // The parent panel this layer belongs to

    public GraphicObject currentGraphic = null;
    public List<GraphicObject> oldGraphics = new List<GraphicObject>();

    // set texture from file path
    public Coroutine SetTexture(string filePath, float transitionSpeed = 1f, Texture blendingTexture = null, bool immediate = false)
    {
        Texture texture = Resources.Load<Texture>(filePath);

        if (texture == null)
        {
            Debug.LogError($"Texture could not found from path: {filePath}, please ensure it exist within Resources!");
            return null;
        }

        return SetTexture(texture, transitionSpeed, blendingTexture, filePath, immediate);
    }

    // set texture from Texture object
    public Coroutine SetTexture(Texture tex, float transitionSpeed = 1f, Texture blendingTexture = null, string filePath = "", bool immediate = false)
    {
        return CreateGraphic(tex, transitionSpeed, filePath, blendingTexture: blendingTexture, immediate: immediate); // create a new graphic object with the texture
    }

    public Coroutine SetVideo(string filePath, float transitionSpeed = 1f, bool useAudio = true, Texture blendingTexture = null, bool immediate = false)
    {
        VideoClip clip = Resources.Load<VideoClip>(filePath);

        if (clip == null)
        {
            Debug.LogError($"Video could not found from path: {filePath}, please ensure it exist within Resources!");
            return null;
        }

        return SetVideo(clip, transitionSpeed, useAudio, blendingTexture, filePath);
    }
    public Coroutine SetVideo(VideoClip video, float transitionSpeed = 1f, bool useAudio = true, Texture blendingTexture = null, string filePath = "", bool immediate = false)
    {
        return CreateGraphic(video, transitionSpeed, filePath, useAudio, blendingTexture, immediate);
    }

    private Coroutine CreateGraphic<T>(T graphicData, float transitionSpeed, string filePath, bool useAudioForVideo = true, Texture blendingTexture = null, bool immediate = false)
    {
        GraphicObject newGraphic = null;

        if (graphicData is Texture)
            newGraphic = new GraphicObject(this, filePath, graphicData as Texture, immediate);
        else if (graphicData is VideoClip)
            newGraphic = new GraphicObject(this, filePath, graphicData as VideoClip, useAudioForVideo, immediate);

        if (currentGraphic != null && !oldGraphics.Contains(currentGraphic))
            oldGraphics.Add(currentGraphic);

        currentGraphic = newGraphic;

        if (!immediate)
            return currentGraphic.FadeIn(transitionSpeed, blendingTexture);

        //otherwise this is an immediate effect, destroy old graphics and return null
        DestroyOldGraphics();
        return null;
    }

    //func to remove all of the old graphics
    public void DestroyOldGraphics()
    {
        foreach (var g in oldGraphics)
            Object.Destroy(g.renderer.gameObject);

        oldGraphics.Clear();
    }

    //clear layer
    public void Clear(float transitionSpeed = 1, Texture blendTexture = null, bool immediate = false)
    {
        if (currentGraphic != null)
        {
            if (!immediate)
                currentGraphic.FadeOut(transitionSpeed, blendTexture);
            else
                currentGraphic.Destroy();
        }
            

        foreach (var g in oldGraphics)
        {
            if (!immediate)
                g.FadeOut(transitionSpeed, blendTexture);
            else
                g.Destroy();
        }
    }
}
