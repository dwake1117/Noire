using System.Collections.Generic;
using MessagePack;
using UnityEngine;

[MessagePackObject]
public class GameData
{
    // profile fields
    [Key(0)] public string ProfileName;
    [Key(1)] public long LastUpdated;
    [Key(2)] public float PercentageComplete;
    
    // player fields
    [Key(3)] public float DreamShards;
    [Key(4)] public float DreamThreads;
    [Key(5)] public Vector3 Position;
    [Key(6)] public Dictionary<string, int> Inventory;
    
    // game states
    [Key(7)] public string CurrentScene;
    
    // BedrockPlains
    [Key(8)] public Dictionary<string, InteractableProgress> InteractableProgress;
    [Key(9)] public bool LightsOpen;
    
    [SerializationConstructor]
    public GameData(string profileId)
    {
        DreamShards = 0;
        DreamThreads = 0;
        Position = new Vector3();

        ProfileName = profileId;
        PercentageComplete = 0;

        CurrentScene = Loader.FirstScene.ToString();

        Inventory = new();
        
        // maps number of times each instance of interactable object has been interacted
        InteractableProgress = new();
    }
}