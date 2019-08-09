using LunarByte.MVVM;

public class LevelModel : Model, ILevelModel
{
	private int               CurrentLevelField;
	private int               GridWidthField;
	private string            LevelCompleteTextField;
	private string            LevelFailTextField;
	private LevelObjectType[] LevelObjectTypeContainerField;
	private LevelState        LevelStateField;
	private int               PointsField;
	private float             ScrollSpeedField;
	private float BaseScrollSpeedField;
	private string            SuperSawTextField;
	private int LastLevelIndexField;
	private float PrestigeMultiplierField;
	private int CurrentPrestigeField;
	private int ColorSchemeField;
	private int SuperColorSchemeField;

	public LevelState LevelState
	{
		get { return LevelStateField; }
		set
		{
			LevelStateField = value;
			OnPropertyChanged();
		}
	}

	public int GridWidth
	{
		get { return GridWidthField; }
		set
		{
			GridWidthField = value;
			OnPropertyChanged();
		}
	}

	public float ScrollSpeed
	{
		get { return ScrollSpeedField; }
		set
		{
			ScrollSpeedField = value;
			OnPropertyChanged();
		}
	}

	public float BaseScrollSpeed
	{
		get { return BaseScrollSpeedField; }
		set
		{
			BaseScrollSpeedField = value;
			OnPropertyChanged();
		}
	}

	public int CurrentLevel
	{
		get { return CurrentLevelField; }
		set
		{
			CurrentLevelField = value;
			OnPropertyChanged();
		}
	}

	public LevelObjectType[] LevelObjectTypeContainer
	{
		get { return LevelObjectTypeContainerField; }
		set
		{
			LevelObjectTypeContainerField = value;
			OnPropertyChanged();
		}
	}

	public int Points
	{
		get { return PointsField; }
		set
		{
			PointsField = value;
			OnPropertyChanged();
		}
	}

	public string CurrentLevelName { get; set; }

	public int TapCount { get; set; }

	public string SuperSawText
	{
		get { return SuperSawTextField; }
		set
		{
			SuperSawTextField = value;
			OnPropertyChanged();
		}
	}

	public string LevelFailText
	{
		get { return LevelFailTextField; }
		set
		{
			LevelFailTextField = value;
			OnPropertyChanged();
		}
	}

	public string LevelCompleteText
	{
		get { return LevelCompleteTextField; }
		set
		{
			LevelCompleteTextField = value;
			OnPropertyChanged();
		}
	}

	public int LastLevelIndex
	{
		get { return LastLevelIndexField; }
		set
		{
			LastLevelIndexField = value;
			OnPropertyChanged();
		}
	}

	public float PrestigeMultiplier
	{
		get { return PrestigeMultiplierField; }
		set
		{
			PrestigeMultiplierField = value;
			OnPropertyChanged();
		}
	}

	public int CurrentPrestige
	{
		get { return CurrentPrestigeField; }
		set
		{
			CurrentPrestigeField = value;
			OnPropertyChanged();
		}
	}

	public int ColorScheme
	{
		get { return ColorSchemeField; }
		set
		{
			ColorSchemeField = value;
			OnPropertyChanged();
		}
	}

	public int SuperColorScheme
	{
		get { return SuperColorSchemeField; }
		set
		{
			SuperColorSchemeField = value;
			OnPropertyChanged();
		}
	}
}

public interface ILevelModel : IObservableProperties
{
	LevelState        LevelState               { get; }
	int               GridWidth                { get; }
	float             ScrollSpeed              { get; }
	float BaseScrollSpeed { get; }
	int               CurrentLevel             { get; }
	LevelObjectType[] LevelObjectTypeContainer { get; }
	int               Points                   { get; }
	string            CurrentLevelName         { get; }
	int               TapCount                 { get; }
	string            SuperSawText             { get; }
	string            LevelFailText            { get; }
	string            LevelCompleteText        { get; }
	int LastLevelIndex { get; }
	float PrestigeMultiplier { get; }
	int CurrentPrestige { get; }
	int ColorScheme { get; }
	int SuperColorScheme { get; }
}
