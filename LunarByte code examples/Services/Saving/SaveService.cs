using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using BayatGames.SaveGameFree;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
#if UNITY_IOS
using UnityEngine.SocialPlatforms.GameCenter;
#endif
using LunarConsole;
using Newtonsoft.Json;
using UnityEngine;
using Logger = LunarConsole.API.Logger;
using LunarByte.HyperCasualSystems;

/// <summary>
/// A service that handles local and cloud saving and loading on Android and on iOS.
/// Saves/loads any kind of MVVM model.
/// UNITY_ANDROID and UNITY_IOS preprocessor directives are used when needed to make sure the service compiles on all platforms.
/// </summary>
[LogCategory(nameof(SaveService))]
public class SaveService : Service, IResourceInitializer, ISaveService
{
	private readonly RuntimePlatform                                   CurrentPlatform;
	private readonly bool                                              Encode;
	private readonly string                                            EncodePassword;
	private readonly Encoding                                          Encoding;
	private readonly BinaryFormatter                                   Formatter;
	private readonly SaveGamePath                                      SavePath;
	private readonly Dictionary<string, string>                        Saves;
	private readonly SaveServicePropertyContractResolver               SerializerResolver;
	private readonly JsonSerializerSettings                            SerializerSettings;
	private          string                                            LoadedJson;
	public           string                                            SaveId;

    /// <summary>
    /// The constructor sets all settings and then loads all local files to Dictionary<string, string> Saves.
    /// </summary>
    /// <param name="settings"></param>
	public SaveService(SaveSettings settings)
	{
        SerializerResolver = settings.SerializerResolver;

        SerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = SerializerResolver
        };

        CurrentPlatform = Application.platform;
        Formatter = new BinaryFormatter();
        Encode = settings.Encode;
        EncodePassword = settings.EncodePassword;
        Encoding = settings.Encoding;
        SaveId = settings.SaveId;
        SavePath = settings.SavePath;

        Saves = LoadLocal();
    }

    /// <summary>
    /// A method used for cloud saving. 
    /// Initializes and logs in to Google Play Games on Android and KeyChain on iOS.
    /// Loads cloud saves after logging in and makes sure locally saved material is the same as in the cloud.
    /// This method is called by a service handler via the LoadResources method.
    /// </summary>
    /// <param name="onSuccess">Callback for success.</param>
    /// <param name="onFail">Callback for failure.</param>
	private void PlatformInitialization(Action onSuccess, Action onFail)
	{
#if UNITY_ANDROID
		PlayGamesClientConfiguration.Builder builder = new PlayGamesClientConfiguration.Builder();
		builder.EnableSavedGames();
		PlayGamesPlatform.InitializeInstance(builder.Build());
		PlayGamesPlatform.Activate();
#endif

		Social.localUser.Authenticate((bool success) =>
		{
			if (success)
			{
				Logger.Log("Logged in");
#if UNITY_ANDROID
				//load Android cloud saves
				var platform = (PlayGamesPlatform)Social.Active;
				//fetches all Android cloud saves
				platform.SavedGame.FetchAllSavedGames(DataSource.ReadCacheOrNetwork,
					                                    OnPlayServicesSavesFetched);
#endif
#if UNITY_IOS
				//load iOS cloud saves
				KeyChain.InitializeKeychain();
			    foreach (var saveName in Saves)
			    {
				    if (!Saves[saveName.Key].Equals(KeyChain.GetString(saveName.Key)))
				    {
					    Saves[saveName.Key] = KeyChain.GetString(saveName.Key);
				    }
			    }
#endif
                onSuccess();
            }
            else
            {
                Logger.LogError("Failed to login");
                onFail();
			}
		});
    }

    /// <summary>
    /// AFter all cloud saves are loaded make sure local saves are the same as cloud saves.
    /// If differences are found change local saves to cloud saves.
    /// </summary>
    /// <param name="saveData">key = identifier, value = json</param>
    private void OnAllSavesLoaded(List<KeyValuePair<string, string>> saveData)
	{
        foreach (var loadedData in saveData)
		{
			if (!Saves[loadedData.Key].Equals(loadedData.Value))
			{
				Saves[loadedData.Key] = loadedData.Value;
			}
        }
	}

    /// <summary>
    /// After all Android cloud saves are fetched this method opens each save file.
    /// Google Play requires that each cloud save is "opened" before actually loading or saving it.
    /// </summary>
    /// <param name="status">Saved game request status</param>
    /// <param name="savesList">All fetched saves</param>
    private void OnPlayServicesSavesFetched(SavedGameRequestStatus status, List<ISavedGameMetadata> savesList)
    {
        var cloudLoadStatus = new CloudLoadStatus(savesList, OnAllSavesLoaded);

		if (status == SavedGameRequestStatus.Success)
		{
            foreach (var save in savesList)
			{
				var fileName = save.Filename;
				OpenAndroidCloudSave(save.Filename, 
				                     (requestStatus, metadata) => OnPlayServicesLoadResponse(requestStatus, metadata, cloudLoadStatus), 
				                     error => OnPlayServicesError(error, fileName, cloudLoadStatus));
            }
	    }
	    else
	    {
		    Logger.LogError($"Failed to fetch all cloud saved games {status}");
	    }
    }

    /// <summary>
    /// Saves locally and then to cloud if cloud saving for the model is true.
    /// </summary>
    /// <typeparam name="TSaveable">Saves models that implement ISaveable.</typeparam>
    /// <param name="saveable"></param>
    public void Save<TSaveable>(TSaveable saveable)
		where TSaveable : ISaveable
	{
		Saves[saveable.CreateSaveIdentifier()] = Serialize(saveable);

		SaveGame.Save(SaveId, Saves, Encode, EncodePassword,
		              SaveGame.Serializer, SaveGame.Encoder, Encoding, SavePath);
		
		if (saveable.SaveToCloud && Social.localUser.authenticated)
		{
			CloudSave(saveable.CreateSaveIdentifier(), Serialize(saveable));
		}
    }

    /// <summary>
    /// Serializes a model and ignores the values that are not needed to be saved.
    /// </summary>
    /// <typeparam name="TSaveable"></typeparam>
    /// <param name="saveable"></param>
    /// <returns></returns>
	private string Serialize<TSaveable>(TSaveable saveable)
		where TSaveable : ISaveable
    {
	    SerializerResolver.IgnoreProperty(typeof(TSaveable), saveable.PropertiesToIgnore);
	    return JsonConvert.SerializeObject(saveable, SerializerSettings);
    }

    /// <summary>
    /// Loads a model and throws an exception if it doesn't exist.
    /// </summary>
    /// <typeparam name="TSaveable">Loads models that implement ISaveable.</typeparam>
    /// <param name="saveable"></param>
	public void Load<TSaveable>(TSaveable saveable)
		where TSaveable : ISaveable
	{
        string saveIdentifier = saveable.CreateSaveIdentifier();

        if (!Saves.ContainsKey(saveIdentifier))
        {
            throw new Exception($"No save with identifier {saveIdentifier} found when loading");
        }
        JsonConvert.PopulateObject(Saves[saveIdentifier], saveable);
    }

    /// <summary>
    /// Trys to load model but doesn't throw an exception if it doesn't exit unlike the method Load.
    /// </summary>
    /// <typeparam name="TSaveable"></typeparam>
    /// <param name="saveable"></param>
    /// <returns></returns>
    public bool TryLoad<TSaveable>(TSaveable saveable)
		where TSaveable : ISaveable
	{
		string saveIdentifier = saveable.CreateSaveIdentifier();
		bool saveExists = Saves.ContainsKey(saveIdentifier);

		if (saveExists)
		{
			JsonConvert.PopulateObject(Saves[saveIdentifier], saveable);
		}

		return saveExists;
	}

    /// <summary>
    /// Loads local saves.
    /// </summary>
    /// <returns></returns>
	private Dictionary<string, string> LoadLocal()
	{
		if (!SaveGame.Exists(SaveId))
		{
			Logger.Log("No saves found");

			return new Dictionary<string, string>();
		}

		Dictionary<string, string> saves = null;
		Logger.Log("Loading save");

		saves = SaveGame.Load(SaveId, Saves, Encode,
		                      EncodePassword, SaveGame.Serializer,
		                      SaveGame.Encoder, Encoding, SavePath);

		var saveMessageBuilder = new StringBuilder($"Loaded values: {Environment.NewLine}");

		foreach (KeyValuePair<string, string> save in saves)
		{
			saveMessageBuilder.Append($"{save.Key}: {save.Value}");
		}

		Logger.Log(saveMessageBuilder.ToString());

		return saves;
	}

    /// <summary>
    /// Cloud saves a json serialized model.
    /// </summary>
    /// <param name="name">Save name</param>
    /// <param name="json">Serialized model</param>
	public void CloudSave(string name, string json)
	{
		if (CurrentPlatform == RuntimePlatform.Android)
		{
			OpenAndroidCloudSave(name, (status, metadata) => OnPlayServicesSaveSuccess(status, metadata, json), OnPlayServicesError);
		}
		else
		{
			KeyChain.SetString(name, json);
		}
	}

    /// <summary>
    /// Opens Android cloud save. 
    /// Google Play requires that each cloud save is "opened" before actually loading or saving it.
    /// After opening is successful then the save can be saved/loaded.
    /// DataSource can be "only network" or "cache and network", which saves files locally before uploading them to Google's servers.
    /// Cache and network works even when the player loses connection because the save file is uploaded after the player is connected again.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="onSuccess"></param>
    /// <param name="onError"></param>
	public void OpenAndroidCloudSave(string                                             name,
	                                 Action<SavedGameRequestStatus, ISavedGameMetadata> onSuccess,
	                                 Action<PlayServiceError>
		                                 onError = null)
	{
#if UNITY_ANDROID
		var error = PlayServiceError.None;

		if (!Social.localUser.authenticated)
		{
			error |= PlayServiceError.NotAuthenticated;
		}

		if (PlayGamesClientConfiguration.DefaultConfiguration.EnableSavedGames)
		{
			error |= PlayServiceError.SaveGameNotEnabled;
		}

		if (string.IsNullOrWhiteSpace(name))
		{
			error |= PlayServiceError.CloudSaveNameNotSet;
		}

		if ((error |= PlayServiceError.None) != PlayServiceError.None)
		{
            onError?.Invoke(error);
		}

        var platform = (PlayGamesPlatform) Social.Active;

		platform.SavedGame.OpenWithAutomaticConflictResolution(
			name, DataSource.ReadCacheOrNetwork,
			ConflictResolutionStrategy.UseLongestPlaytime, onSuccess);
#endif
	}

    /// <summary>
    /// Failed opening an Android cloud save.
    /// </summary>
    /// <param name="err"></param>
    /// <param name="fileName"></param>
    /// <param name="cloudLoadStatus"></param>
	private void OnPlayServicesError(PlayServiceError err, string fileName, CloudLoadStatus cloudLoadStatus)
	{
		Logger.Log(err);
		cloudLoadStatus.Loaded(fileName);
		throw new Exception($"Error opening Android cloud saves. {err}");
	}

    private void OnPlayServicesError(PlayServiceError err)
	{
		Logger.Log(err);
		throw new Exception($"Error opening Android cloud saves. {err}");
	}

    /// <summary>
    /// Successfully opened an Android cloud save. A cloud save can now be done.
    /// Saving to Google servers must be done in type byte[] so the json is serialized to this.
    /// Then the meta information is updated and the save file is uploaded to the server.
    /// </summary>
    /// <param name="status"></param>
    /// <param name="meta"></param>
    /// <param name="json"></param>
	private void OnPlayServicesSaveSuccess(SavedGameRequestStatus status, ISavedGameMetadata meta, string json)
	{
#if UNITY_ANDROID
		if (status == SavedGameRequestStatus.Success)
		{
			byte[] data = SerializeToByte(json);

			SavedGameMetadataUpdate update = new SavedGameMetadataUpdate.Builder()
			                                 .WithUpdatedDescription(
				                                 $"Last save : {DateTime.Now.ToString()}").Build();
			var platform = (PlayGamesPlatform) Social.Active;
			platform.SavedGame.CommitUpdate(meta, update, data, OnPlayServicesSaved);
		}
		else
		{
			OnPlayServicesSaved(status, null);
		}
#endif
	}

    /// <summary>
    /// Checks if cloud saving was successful or not.
    /// </summary>
    /// <param name="status"></param>
    /// <param name="meta"></param>
	private void OnPlayServicesSaved(SavedGameRequestStatus status, ISavedGameMetadata meta)
	{
        switch (status)
		{
			case SavedGameRequestStatus.Success:
				Logger.Log("Saved to cloud");

                break;

			default: //error
				Logger.LogError($"Failed to save {status}");
                throw new Exception($"Failed to save. {status}");
		}
	}

    /// <summary>
    /// Opening a cloud save was successful. It can now be loaded.
    /// </summary>
    /// <param name="status"></param>
    /// <param name="meta"></param>
    /// <param name="cloudLoadStatus"></param>
	private void OnPlayServicesLoadResponse(SavedGameRequestStatus status, ISavedGameMetadata meta, CloudLoadStatus cloudLoadStatus)
	{
#if UNITY_ANDROID
		if (status == SavedGameRequestStatus.Success)
		{
			var platform = (PlayGamesPlatform)Social.Active;
			platform.SavedGame.ReadBinaryData(meta, (dataReadStatus, binaryData) => OnPlayServicesLoaded(dataReadStatus, binaryData, cloudLoadStatus, meta));
        }
		else
		{
			OnPlayServicesLoaded(status, null, cloudLoadStatus);
		}
#endif
	}

    /// <summary>
    /// Check if cloud save was successfully loaded. If true then cloud save file is deserialized.
    /// CloudLoadStatus is also updated to inform that a file is loaded.
    /// </summary>
    /// <param name="status"></param>
    /// <param name="data"></param>
    /// <param name="cloudLoadStatus"></param>
    /// <param name="meta"></param>
	private void OnPlayServicesLoaded(SavedGameRequestStatus status, byte[] data, CloudLoadStatus cloudLoadStatus, ISavedGameMetadata meta = null)
	{
        switch (status)
		{
			case SavedGameRequestStatus.Success:
                Logger.Log("Loaded from cloud.");
				if(meta != null)
				{
                    string json = DeserializeByte(data);
                    cloudLoadStatus.Loaded(meta.Filename, meta.Filename, json);
                }

                break;

			default: //error
				Logger.LogError($"Failed to load. {status}");

                throw new Exception($"Failed to load. {status}");
		}
	}

    /// <summary>
    /// Serializes to byte[].
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
	public byte[] SerializeToByte(string json)
	{
		using (var ms = new MemoryStream())
		{
			Formatter.Serialize(ms, json);

			return ms.GetBuffer();
		}
	}

    /// <summary>
    /// Deserializes from byte[].
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
	public string DeserializeByte(byte[] data)
	{
        using (var ms = new MemoryStream(data))
		{
            return (string) Formatter.Deserialize(ms);
		}
	}

    /// <summary>
    /// A class that handles the status of loading every cloud save file.
    /// In constructor every single Android cloud save file's name is added to a NotYetLoaded list.
    /// After a cloud save file is loaded the method Loaded is called and the file is removed from NotYetLoaded.
    /// After NotYetLoaded is empty the callback onAllLoaded is called and a KeyValuePair is sent which contains
    /// key = name, value = json
    /// </summary>
	private class CloudLoadStatus : IDisposable
	{
		public CloudLoadStatus(List<ISavedGameMetadata> saveGameMetaDatas, Action<List<KeyValuePair<string, string>>> onAllLoaded)
		{
			OnAllLoaded = onAllLoaded;
			NotYetLoaded = new HashSet<string>();
			LoadedValues = new List<KeyValuePair<string, string>>();

            for (int i = 0; i < saveGameMetaDatas.Count; i++)
			{
				NotYetLoaded.Add(saveGameMetaDatas[i].Filename);
			}
		}

		private readonly HashSet<string> NotYetLoaded;
		private readonly Action<List<KeyValuePair<string, string>>> OnAllLoaded;
		private readonly List<KeyValuePair<string, string>>         LoadedValues;

        /// <summary>
        /// After a cloud save file is loaded this method is called and the loaded file is removed from NotYetLoaded.
        /// After everything is loaded and the callback is called. This object is disposed. 
        /// </summary>
        /// <param name="loadedFile">The name of the file</param>
        /// <param name="identifier">The identifier of the file</param>
        /// <param name="json">The data of the file</param>
        /// <returns></returns>
		public bool Loaded(string loadedFile, string identifier, string json)
		{
            bool shouldDispose = false;
			NotYetLoaded.Remove(loadedFile);
            LoadedValues.Add(new KeyValuePair<string, string>(identifier, json));

            if (NotYetLoaded.Count == 0)
			{
				OnAllLoaded(LoadedValues);
				shouldDispose = true;
			}

            return shouldDispose;
		}

        /// <summary>
        /// If a file load is a failure the file still needs to be removed from the list to make sure the actually 
        /// successfully loaded files are loaded to a local dictionary.
        /// </summary>
        /// <param name="loadedFile"></param>
        /// <returns></returns>
		public bool Loaded(string loadedFile)
		{
			bool shouldDispose = false;
			NotYetLoaded.Remove(loadedFile);

			if (NotYetLoaded.Count == 0)
			{
				OnAllLoaded(LoadedValues);
				shouldDispose = true;
			}

			return shouldDispose;
		}

        /// <summary>
        /// Dispose the object when it is not needed anymore.
        /// </summary>
        public void Dispose()
		{
			GC.SuppressFinalize(this);
		}
	}

    /// <summary>
    /// This method is called by a service handler that initializes all the services in order.
    /// This method starts cloud loading at the start of the program.
    /// </summary>
    /// <param name="onSuccess"></param>
    /// <param name="onFail"></param>
	public void LoadResources(Action onSuccess, Action onFail)
	{
		PlatformInitialization(onSuccess, onFail);
    }
    public string Identifier { get; } = nameof(SaveService);
}
