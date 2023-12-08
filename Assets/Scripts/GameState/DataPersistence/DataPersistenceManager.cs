using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public class DataPersistenceManager : MonoBehaviour
{
    public static DataPersistenceManager Instance { get; private set; }
    
    [Header("Debugging")]
    [SerializeField] private bool disableDataPersistence = false;
    [SerializeField] private bool initializeDataIfNull = false;

    [Header("File Storage Config")]
    [SerializeField] private string fileName;

    // other fields
    private GameData gameData;
    private IDataPersistence[] dataPersistenceObjects;
    private GameStateFileIO fileHandler;

    private string selectedProfileId;
    
    private void Awake() 
    {
        if (Instance != null) 
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (disableDataPersistence) 
        {
            Debug.LogWarning("Data Persistence is currently disabled!");
        }
        
        fileHandler = new GameStateFileIO(Application.persistentDataPath, fileName);
        InitializeSelectedProfileId();
    }
    
    private void OnEnable() 
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable() 
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        dataPersistenceObjects = FindAllDataPersistenceObjects();
    }

    public void ChangeSelectedProfileId(string newProfileId) 
    {
        selectedProfileId = newProfileId;
        LoadGame();
    }

    public void NewGame(string profileId) 
    {
        gameData = new GameData(profileId);
        fileHandler.Save(gameData, selectedProfileId);
    }
    
    private void LoadGame(bool reload = true)
    {
        if (disableDataPersistence) 
            return;
        
        // if reload=true, gameData will be reloaded from disk
        if(reload)
            gameData = fileHandler.Load(selectedProfileId);

        if (gameData == null && initializeDataIfNull)
            NewGame("dev_default_profile");
        
        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects) 
            dataPersistenceObj.LoadData(gameData);
    }
    
    /// iterates through all data persistence objects, and calls save on each of them
    /// Improvements for the future: manually save data, and make gameData public. SaveGame() is too expensive to call
    public void SaveGame()
    {
        if (disableDataPersistence || gameData == null) 
            return;
        
        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects) 
            dataPersistenceObj.SaveData(gameData);

        // timestamp the data
        gameData.LastUpdated = DateTime.Now.ToBinary();

        fileHandler.Save(gameData, selectedProfileId);
    }

    public void DeleteProfileData(string profileId)
    {
        fileHandler.Delete(profileId);
        InitializeSelectedProfileId();
        LoadGame();
    }

    public string LastCheckPointScene => gameData.LastCheckPointScene;
    
    private void InitializeSelectedProfileId() 
    {
        selectedProfileId = fileHandler.GetMostRecentlyUpdatedProfileId();
        gameData = fileHandler.Load(selectedProfileId);
    }

    private IDataPersistence[] FindAllDataPersistenceObjects() 
    {
        return FindObjectsOfType<MonoBehaviour>(true)
            .OfType<IDataPersistence>()
            .ToArray();
    }

    public bool HasGameData()
    {
        if (gameData != null)
            return true;
        return GetAllProfilesGameData().Count > 0;
    }

    public Dictionary<string, GameData> GetAllProfilesGameData() 
    {
        return fileHandler.LoadAllProfiles();
    }

    public void OnDeath()
    {
        gameData.LastCheckPointScene = Loader.TargetScene;
        Player.Instance.SaveCurrencyAndInventory(gameData);
        
        // timestamp the data
        gameData.LastUpdated = DateTime.Now.ToBinary();

        fileHandler.Save(gameData, selectedProfileId);
    }
}