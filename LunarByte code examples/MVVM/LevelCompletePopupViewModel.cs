using LunarByte.MVVM;

public class LevelCompletePopupViewModel : ViewModel, ILevelCompletePopupViewModel
{
	public  Event<PopupType> AddPopupEvent           = new Event<PopupType>();
	public  Event            ClosePopUpEvent         = new Event();
	public  Event            DoubleRewardButtonEvent = new Event();
	public  Event            LeaderboardButtonEvent  = new Event();
	private string           LevelCompleteTextField;
	public  Event            NextLevelButtonEvent = new Event();
	public  Event            RestartButtonEvent   = new Event();
	private string           ScoreTextField;

	public string LevelCompleteText
	{
		get { return LevelCompleteTextField; }
		set
		{
			LevelCompleteTextField = value;
			OnPropertyChanged();
		}
	}

	public string ScoreText
	{
		get { return ScoreTextField; }
		set
		{
			ScoreTextField = value;
			OnPropertyChanged();
		}
	}

	public IDispatchableEvent ClosePopUp
	{
		get { return ClosePopUpEvent; }
	}

	public IDispatchableEvent<PopupType> AddPopup
	{
		get { return AddPopupEvent; }
	}

	public IDispatchableEvent OnRestartButton
	{
		get { return RestartButtonEvent; }
	}

	public IDispatchableEvent OnNextLevelButton
	{
		get { return NextLevelButtonEvent; }
	}

	public IDispatchableEvent OnLeaderboardButton
	{
		get { return LeaderboardButtonEvent; }
	}

	public IDispatchableEvent OnDoubleRewardButton
	{
		get { return DoubleRewardButtonEvent; }
	}
}

public interface ILevelCompletePopupViewModel : IPopupViewModel
{
	string                        LevelCompleteText { get; }
	string                        ScoreText         { get; }
	IDispatchableEvent            ClosePopUp        { get; }
	IDispatchableEvent<PopupType> AddPopup          { get; }

	IDispatchableEvent OnRestartButton { get; }

	IDispatchableEvent OnNextLevelButton { get; }

	IDispatchableEvent OnLeaderboardButton { get; }

	IDispatchableEvent OnDoubleRewardButton { get; }
}
