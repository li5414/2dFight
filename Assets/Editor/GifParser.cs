using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEditor.Animations;

using UnityEngine.EventSystems;

public class GifParser : Editor
{
    [MenuItem("Tools/GifParser")]
    public static void ParseGif()
    {
        string folder = EditorUtility.OpenFolderPanel("SaveFileTo", "", "");
        foreach (var s in Selection.objects)
        {
            string path = AssetDatabase.GetAssetPath(s);
            if (Path.GetExtension(path) == ".gif")
            {
                string fileName = Path.GetFileNameWithoutExtension(path);
               
                string folderPath = Path.Combine(folder,fileName);
                if (Directory.Exists(folderPath))
                {
                    Directory.Delete(folderPath,true);
                }
                else
                {
                    Directory.CreateDirectory(folderPath);
                }
                Process proc = Process.Start(@Application.dataPath + "/Editor/ffmpeg.exe", "-i "+path+" "+folderPath+"/"+fileName+"_%02d.png");
                proc.WaitForExit();
            }
        }

        AssetDatabase.Refresh();
    }
    
    //生成出的Prefab的路径
    private static string PrefabPath = "Assets/Resources/Prefabs";
    //生成出的AnimationController的路径
    private static string AnimationControllerPath = "Assets/AnimationController";
    //生成出的Animation的路径
    private static string AnimationPath = "Assets/Animation";
    //美术给的原始图片路径
    private static string ImagePath = Application.dataPath + "/Gif";
 
    private static string pathTemp = "*.png";
    [MenuItem("Tools / BuildAnimaiton")]
    static void BuildAniamtion()
    {
        DirectoryInfo raw = new DirectoryInfo(ImagePath);
        foreach (DirectoryInfo dictorys in raw.GetDirectories())
        {
            List<AnimationClip> clips = new List<AnimationClip>();
            foreach (DirectoryInfo dictoryAnimations in dictorys.GetDirectories())
            {
                //每个文件夹就是一组帧动画，这里把每个文件夹下的所有图片生成出一个动画文件
                clips.Add(BuildAnimationClip(dictoryAnimations));
            }
            //把所有的动画文件生成在一个AnimationController里
            AnimatorController controller = BuildAnimationController(clips, dictorys.Name);
            //最后生成程序用的Prefab文件
            BuildPrefab(dictorys, controller);
        }
    }
    static AnimationClip BuildAnimationClip(DirectoryInfo dictorys)
    {
        string animationName = dictorys.Name;
        //查找所有图片，因为我找的测试动画是.jpg
        FileInfo[] images = dictorys.GetFiles(pathTemp);
        AnimationClip clip = new AnimationClip();
        EditorCurveBinding curveBinding = new EditorCurveBinding();
        curveBinding.type = typeof(SpriteRenderer);
        curveBinding.path = "";
        curveBinding.propertyName = "m_Sprite";
        ObjectReferenceKeyframe[] keyFrames = new ObjectReferenceKeyframe[images.Length];
        //动画长度是按秒为单位，1/10就表示1秒切10张图片，根据项目的情况可以自己调节
        double  frameTime = 1d / 10d;
        for (int i = 0; i < images.Length; i++)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(DataPathToAssetPath(images[i].FullName));
            keyFrames[i] = new ObjectReferenceKeyframe();
            keyFrames[i].time = (float)frameTime * i;
            keyFrames[i].value = sprite;
        }
        //动画帧率，30比较合适
        clip.frameRate = 10;
        //有些动画我希望天生它就动画循环
        if (animationName.IndexOf("idle", StringComparison.Ordinal) >= 0)
        {
            //设置idle文件为循环动画
            SerializedObject serializedClip = new SerializedObject(clip);
            AnimationClipSettings clipSettings = new AnimationClipSettings(serializedClip.FindProperty("m_AnimationClipSettings"));
            clipSettings.LoopTime = true;
            serializedClip.ApplyModifiedProperties();
        }
        string parentName = Directory.GetParent(dictorys.FullName).Name;
        Directory.CreateDirectory(AnimationPath + "/" +parentName);
        AnimationUtility.SetObjectReferenceCurve(clip, curveBinding, keyFrames);
        AssetDatabase.CreateAsset(clip, AnimationPath + "/" +parentName + "/" +animationName + ".anim");
        AssetDatabase.SaveAssets();
        return clip;
    }
    static AnimatorController BuildAnimationController(List<AnimationClip> clips, string name)
    {
        AnimatorController animatorController = AnimatorController.CreateAnimatorControllerAtPath(AnimationControllerPath + "/" +name + ".controller");
        AnimatorControllerLayer layer = animatorController.layers[0];
        AnimatorStateMachine sm = layer.stateMachine;
        foreach (AnimationClip newClip in clips)
        {
            //AnimatorStateMachine machine = sm.AddStateMachine(newClip.name);
            AnimatorState state = sm.AddState(newClip.name);
            state.motion = newClip;
            //AnimatorStateTransition trans = sm.AddAnyStateTransition(state);
            if (newClip.name == "idle") {
                sm.defaultState = state;
            }
            //sm.AddEntryTransition(machine);
            //sm.AddStateMachineExitTransition(machine);
            //trans.RemoveCondition(0);
        }
        AssetDatabase.SaveAssets();
        return animatorController;
    }
    static void BuildPrefab(DirectoryInfo dictorys, UnityEditor.Animations.AnimatorController animatorCountorller)
    {
        //生成Prefab 添加一张预览用的Sprite
        FileInfo images = dictorys.GetDirectories()[0].GetFiles(pathTemp)[0];
        GameObject go = new GameObject();
        go.name = dictorys.Name;
        SpriteRenderer spriteRender = go.AddComponent<SpriteRenderer>();
        spriteRender.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(DataPathToAssetPath(images.FullName));
        Animator animator = go.AddComponent<Animator>();
        animator.runtimeAnimatorController = animatorCountorller;
        PrefabUtility.SaveAsPrefabAsset(go,PrefabPath + "/" +go.name + ".prefab");
        DestroyImmediate(go);
    }
    public static string DataPathToAssetPath(string path)
    {
        if (Application.platform == RuntimePlatform.WindowsEditor)
            return path.Substring(path.IndexOf("Assets\\"));
        else
            return path.Substring(path.IndexOf("Assets /"));
    }
    class AnimationClipSettings
    {
        SerializedProperty m_Property;
        private SerializedProperty Get(string property)
        {
            return m_Property.FindPropertyRelative(property);
        }
        public AnimationClipSettings(SerializedProperty prop)
        {
            m_Property = prop;
        }
        public bool LoopTime { get { return Get("m_LoopTime").boolValue; } set { Get("m_LoopTime").boolValue = value; } }
    }
}
