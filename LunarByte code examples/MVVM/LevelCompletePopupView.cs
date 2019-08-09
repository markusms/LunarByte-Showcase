using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class
	LevelCompletePopupView : PopupView<ILevelCompletePopupViewModel>
{
	private Animator Animator;

	[SerializeField] private Button          DoubleRewardButton;
	[SerializeField] private Button          LeaderboardButton;
	[SerializeField] private TextMeshProUGUI LevelCompletionText;
	[SerializeField] private Button          NextLevelButton;
	[SerializeField] private Button          RestartButton;
	[SerializeField] private TextMeshProUGUI RewardText;
	[SerializeField] private TextMeshProUGUI ScoreDescriptionText;

	private void Awake()
	{
		Animator = GetComponent<Animator>();
		RestartButton.onClick.AddListener(OnRestartButtonClicked);
		NextLevelButton.onClick.AddListener(OnNextLevelButtonClicked);
		LeaderboardButton.onClick.AddListener(OnLeaderboardButtonClicked);
		DoubleRewardButton.onClick.AddListener(OnDoubleRewardButtonClicked);
	}

	public void SetScoreDescriptionText(string text)
	{
		ScoreDescriptionText.text = text;
	}

	public void SetRewardText(string text)
	{
		RewardText.text = text;
	}

	public void SetLevelCompletionText(string text)
	{
		LevelCompletionText.text = text;
	}

	public void OnRestartButtonClicked()
	{
		Animator.SetBool("Exit", true);
		ViewModel.OnRestartButton.Dispatch();
	}

	public void OnNextLevelButtonClicked()
	{
		ViewModel.OnNextLevelButton.Dispatch();
	}

	public void OnDoubleRewardButtonClicked()
	{
		ViewModel.OnDoubleRewardButton.Dispatch();
	}

	public void OnLeaderboardButtonClicked()
	{
		ViewModel.OnLeaderboardButton.Dispatch();
	}
}
