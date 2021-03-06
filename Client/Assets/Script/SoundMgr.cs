﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum _eSoundLayer
{
    Background = 0,// 背景层，loop，同名替换，不同名替换
    LayerIgnore,
    LayerReplace,
    LayerNormal
}

public interface ISoundLayer
{
    //-------------------------------------------------------------------------
    _eSoundLayer getLayerId();

    //-------------------------------------------------------------------------
    void play(AudioSource audio_src);

    //-------------------------------------------------------------------------
    void update();

    //-------------------------------------------------------------------------
    void destroy();
}

// 背景层，loop，同名替换，不同名替换
public class CSoundLayerBackground : ISoundLayer
{
    //-------------------------------------------------------------------------
    CSoundMgr mSoundMgr = null;
    List<AudioSource> mListAudioPlaying = new List<AudioSource>();

    //-------------------------------------------------------------------------
    public CSoundLayerBackground(CSoundMgr sound_mgr)
    {
        mSoundMgr = sound_mgr;
    }

    //-------------------------------------------------------------------------
    public _eSoundLayer getLayerId()
    {
        return _eSoundLayer.Background;
    }

    //-------------------------------------------------------------------------
    public void play(AudioSource audio_src)
    {
        destroy();
        float music_value = 0f;
        if (PlayerPrefs.GetFloat("Music") == null)
        {
            music_value = 1f;
        }
        else
        {
            music_value = PlayerPrefs.GetFloat("Music");
        }
        audio_src.volume = music_value;
        audio_src.loop = true;
        audio_src.Play();
        mListAudioPlaying.Add(audio_src);
    }

    //-------------------------------------------------------------------------
    public void update()
    {
    }

    //-------------------------------------------------------------------------
    public void destroy()
    {
        foreach (var i in mListAudioPlaying)
        {
            i.Stop();
            mSoundMgr.freeAudioSource(i);
        }
        mListAudioPlaying.Clear();
    }
}

// 同名忽略，不同名也忽略
public class CSoundLayerIgnore : ISoundLayer
{
    //-------------------------------------------------------------------------
    CSoundMgr mSoundMgr = null;
    AudioSource mAudioPlaying = null;

    //-------------------------------------------------------------------------
    public CSoundLayerIgnore(CSoundMgr sound_mgr)
    {
        mSoundMgr = sound_mgr;
    }

    //-------------------------------------------------------------------------
    public _eSoundLayer getLayerId()
    {
        return _eSoundLayer.LayerIgnore;
    }

    //-------------------------------------------------------------------------
    public void play(AudioSource audio_src)
    {
        if (mAudioPlaying == null)
        {
            float music_value = 0f;
            if (PlayerPrefs.GetFloat("Sounds") == null)
            {
                music_value = 1f;
            }
            else
            {
                music_value = PlayerPrefs.GetFloat("Sounds");
            }
            audio_src.volume = music_value;
            audio_src.loop = false;
            audio_src.Play();
            mAudioPlaying = audio_src;
        }
        else
        {
            mSoundMgr.freeAudioSource(audio_src);
        }
    }

    //-------------------------------------------------------------------------
    public void update()
    {
        if (mAudioPlaying == null) return;

        if (!mAudioPlaying.isPlaying)
        {
            mAudioPlaying.Stop();
            mSoundMgr.freeAudioSource(mAudioPlaying);
            mAudioPlaying = null;
        }
    }

    //-------------------------------------------------------------------------
    public void destroy()
    {
        if (mAudioPlaying != null)
        {
            mAudioPlaying.Stop();
            mSoundMgr.freeAudioSource(mAudioPlaying);
            mAudioPlaying = null;
        }
    }
}

// 同名替换，不同名重叠
public class CSoundLayerReplace : ISoundLayer
{
    //-------------------------------------------------------------------------
    CSoundMgr mSoundMgr = null;
    Dictionary<string, AudioSource> mMapAudioPlaying = new Dictionary<string, AudioSource>();
    List<string> mListDestroy = new List<string>();

    //-------------------------------------------------------------------------
    public CSoundLayerReplace(CSoundMgr sound_mgr)
    {
        mSoundMgr = sound_mgr;
    }

    //-------------------------------------------------------------------------
    public _eSoundLayer getLayerId()
    {
        return _eSoundLayer.LayerReplace;
    }

    //-------------------------------------------------------------------------
    public void play(AudioSource audio_src)
    {
        if (mMapAudioPlaying.ContainsKey(audio_src.name))
        {
            // 同名替换
            mMapAudioPlaying[audio_src.name].Stop();
            mMapAudioPlaying[audio_src.name].Play();

            audio_src.Stop();
            mSoundMgr.freeAudioSource(audio_src);
        }
        else
        {
            // 不同名重叠
            float music_value = 0f;
            if (PlayerPrefs.GetFloat("Sounds") == null)
            {
                music_value = 1f;
            }
            else
            {
                music_value = PlayerPrefs.GetFloat("Sounds");
            }
            audio_src.volume = music_value;
            audio_src.loop = false;
            audio_src.Play();
            mMapAudioPlaying[audio_src.name] = audio_src;
        }
    }

    //-------------------------------------------------------------------------
    public void update()
    {
        foreach (var i in mMapAudioPlaying)
        {
            if (!i.Value.isPlaying)
            {
                i.Value.Stop();
                mSoundMgr.freeAudioSource(i.Value);
                mListDestroy.Add(i.Key);
            }
        }

        foreach (var i in mListDestroy)
        {
            mMapAudioPlaying.Remove(i);
        }
        mListDestroy.Clear();
    }

    //-------------------------------------------------------------------------
    public void destroy()
    {
        foreach (var i in mMapAudioPlaying)
        {
            if (i.Value != null)
            {
                i.Value.Stop();
                mSoundMgr.freeAudioSource(i.Value);
            }
        }
        mMapAudioPlaying.Clear();
    }
}

// 常规
public class CSoundLayerNormal : ISoundLayer
{
    //-------------------------------------------------------------------------
    CSoundMgr mSoundMgr = null;
    List<AudioSource> mListAudioPlaying = new List<AudioSource>();
    List<AudioSource> mListAudioDestroy = new List<AudioSource>();

    //-------------------------------------------------------------------------
    public CSoundLayerNormal(CSoundMgr sound_mgr)
    {
        mSoundMgr = sound_mgr;
    }

    //-------------------------------------------------------------------------
    public _eSoundLayer getLayerId()
    {
        return _eSoundLayer.LayerNormal;
    }

    //-------------------------------------------------------------------------
    public void play(AudioSource audio_src)
    {
        float music_value = 0f;
        if (PlayerPrefs.GetFloat("Sounds") == null)
        {
            music_value = 1f;
        }
        else
        {
            music_value = PlayerPrefs.GetFloat("Sounds");
        }
        audio_src.volume = music_value;
        audio_src.loop = false;
        audio_src.Play();
        mListAudioPlaying.Add(audio_src);
    }

    //-------------------------------------------------------------------------
    public void update()
    {
        foreach (var i in mListAudioPlaying)
        {
            if (!i.isPlaying)
            {
                mListAudioDestroy.Add(i);
            }
        }

        foreach (var i in mListAudioDestroy)
        {
            i.Stop();
            mSoundMgr.freeAudioSource(i);
            mListAudioPlaying.Remove(i);
        }
        mListAudioDestroy.Clear();
    }

    //-------------------------------------------------------------------------
    public void destroy()
    {
        foreach (var i in mListAudioPlaying)
        {
            i.Stop();
            mSoundMgr.freeAudioSource(i);
        }
        mListAudioPlaying.Clear();
        mListAudioDestroy.Clear();
    }
}

public class CSoundMgr : IDisposable
{
    //-------------------------------------------------------------------------
    GameObject mObjAudio = null;
    Dictionary<_eSoundLayer, ISoundLayer> mMapSoundLayer = new Dictionary<_eSoundLayer, ISoundLayer>();
    Queue<AudioSource> mQueAudioFree = new Queue<AudioSource>();
    List<AudioSource> mListAudioFree = new List<AudioSource>();
    Dictionary<string, AudioClip> mMapAudioClip = new Dictionary<string, AudioClip>();

    //-------------------------------------------------------------------------
    public CSoundMgr()
    {
        mObjAudio = new GameObject("Audio Object");

        if (!PlayerPrefs.HasKey("Music"))
        {
            PlayerPrefs.SetFloat("Music", 0.5f);
        }
        if (!PlayerPrefs.HasKey("Sounds"))
        {
            PlayerPrefs.SetFloat("Sounds", 0.5f);
        }

        mListAudioFree.Clear();
        for (int i = 0; i < 8; i++)
        {
            AudioSource obj_audio = mObjAudio.AddComponent<AudioSource>();
            Debug.Log("CSoundMgr调用");
            mListAudioFree.Add(obj_audio);           
            mQueAudioFree.Enqueue(obj_audio);
        }

        {
            CSoundLayerBackground s = new CSoundLayerBackground(this);
            mMapSoundLayer[s.getLayerId()] = s;
        }

        {
            CSoundLayerIgnore s = new CSoundLayerIgnore(this);
            mMapSoundLayer[s.getLayerId()] = s;
        }

        {
            CSoundLayerReplace s = new CSoundLayerReplace(this);
            mMapSoundLayer[s.getLayerId()] = s;
        }

        {
            CSoundLayerNormal s = new CSoundLayerNormal(this);
            mMapSoundLayer[s.getLayerId()] = s;
        }
    }

    //-------------------------------------------------------------------------
    ~CSoundMgr()
    {
        this.Dispose(false);
    }

    //-------------------------------------------------------------------------
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    //-------------------------------------------------------------------------
    protected virtual void Dispose(bool disposing)
    {
        destroy();
    }

    //-------------------------------------------------------------------------
    public void play(string file_name, _eSoundLayer sound_layer)
    {
        AudioSource audio_src = _genAudioSource();
        audio_src.clip = _loadAudioClip("Audio/" + file_name);
        mMapSoundLayer[sound_layer].play(audio_src);
    }

    //-------------------------------------------------------------------------
    public List<AudioSource> getAudioSource()
    {
        return mListAudioFree;
    }

    //-------------------------------------------------------------------------
    public void update()
    {
        foreach (var i in mMapSoundLayer)
        {
            i.Value.update();
        }
    }

    //-------------------------------------------------------------------------
    public void destroy()
    {
        mMapAudioClip.Clear();

        foreach (var i in mMapSoundLayer)
        {
            i.Value.destroy();
        }
        mMapSoundLayer.Clear();

        mQueAudioFree.Clear();

        mListAudioFree.Clear();
        if (mObjAudio != null)
        {
            GameObject.Destroy(mObjAudio);
            mObjAudio = null;
        }
    }

    //-------------------------------------------------------------------------
    public void destroyAllSceneSound()
    {
    }

    //-------------------------------------------------------------------------
    public void freeAudioSource(AudioSource audio_src)
    {
        mQueAudioFree.Enqueue(audio_src);
    }

    //-------------------------------------------------------------------------
    AudioSource _genAudioSource()
    {
        if (mQueAudioFree.Count == 0)
        {            
            AudioSource audio_src = mObjAudio.AddComponent<AudioSource>();
            mListAudioFree.Add(audio_src);
            return audio_src;
        }
        else
        {
            return mQueAudioFree.Dequeue();
        }
    }

    //-------------------------------------------------------------------------
    AudioClip _loadAudioClip(string audio_name)
    {
        if (!mMapAudioClip.ContainsKey(audio_name))
        {
            mMapAudioClip.Add(audio_name, Resources.Load(audio_name) as AudioClip);
        }

        return mMapAudioClip[audio_name];
    }
}
