using UnityEngine;
using System.Collections;
using System.Linq;
using System;

[CreateAssetMenu(fileName = "Character",menuName = "City Builder Data/Character")]
public class CharacterData : ScriptableObject
{
    [SerializeField] private CharacterEmotion[] emotions;

    public CharacterEmotion GetEmotion(Emotion e)
    {
        return emotions.First(val => val.emotion == e);
    }
}
[Serializable]
public class CharacterEmotion
{
    [SerializeField] public Emotion emotion;
    [SerializeField] public Sprite sprite;
    [SerializeField] public AudioClip[] sounds;
}

public enum Emotion
{
    HAPPY,
    SAD,
    ANGRY,
    THINKING,
    SURPRISED
}
