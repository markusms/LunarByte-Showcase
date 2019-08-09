# Save Service

A service that handles local and cloud saving and loading on Android and on iOS.
Saves/loads any kind of MVVM model that implements the ISaveable interface.

For example to save a model nothing more needs to be done than to just SaveService's 
Save method with the model that wants to be saved as the parameter need to be called:
```saveService.Save(model);```
The save service then saves the model locally and then to cloud if the model's SaveToCloud 
bool is set to true.

The required interfaces for the model to be saved are:

```
public interface ISaveable : IIdentifiable
{
   //properties that aren't saved
   string[] PropertiesToIgnore { get; } 
   bool SaveToCloud { get; }
}
```
  and
```
public interface IIdentifiable
{
   string Identifier { get; }
}
```

Cloud loading happens automatically at the start of running the program by calling the method> 
```
PlatformInitialization();
```
The method initializes and logs in to Google Play Games on Android and KeyChain on iOS. 
Then it loads cloud saves and makes sure locally saved material is the same as in the cloud.

By calling the method Load(model) the given model's properties are changed to the ones that are locally saved.
For a save to be able to be be loaded a saveIdentifier needs to be created by calling the saveable model's extension method
```
public static string CreateSaveIdentifier<T>(this T saveable)
   where T : ISaveable
{
   return $"{typeof(T)}.{saveable.Identifier}";
}
```
