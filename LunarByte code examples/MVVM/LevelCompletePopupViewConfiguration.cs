using LunarByte.MVVM;
using LunarByte.MVVM.Configurations;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(LevelCompletePopupViewConfiguration),
	menuName = "Window/Configurations/" + nameof(LevelCompletePopupViewConfiguration))]
public class LevelCompletePopupViewConfiguration : ViewConfiguration<ILevelCompletePopupViewModel,
	LevelCompletePopupViewModel, LevelCompletePopupView, LevelModel, PlayerModel, SawModel,
	SaveModel, SocialService, LocalizationService>
{
	protected override void Configure(LevelCompletePopupViewModel viewModel,
	                                  LevelCompletePopupView      view,
	                                  LevelModel                  lModel,
	                                  PlayerModel                 pModel,
	                                  SawModel                    sawModel,
	                                  SaveModel                   saveModel,
	                                  SocialService               socialService,
	                                  LocalizationService         localizationService)
	{
		viewModel.RestartButtonEvent.AddListener(() =>
		{
			lModel.RestartLevel();
			view.Close();
		});

		viewModel.NextLevelButtonEvent.AddListener(() =>
		{
			if (lModel.CurrentLevel == lModel.LastLevelIndex)
			{
				socialService.UnlockAchievement(GPGSIds.achievement_pro);
				lModel.PrestigeUpScrollSpeed(saveModel);
				sawModel.PrestigeUpSawAutoSpeed(saveModel, lModel.PrestigeMultiplier);
			}
			lModel.NextLevel(saveModel);
			lModel.RestartLevel();
			view.Close();
		});

		viewModel.LeaderboardButtonEvent.AddListener(socialService.ShowIntegratedLeaderboard);

		viewModel
			.Bind<LevelCompletePopupViewModel, string>(scoreText => viewModel.ScoreText = scoreText)
			.To(pModel, pm => pm.Points, nameof(PlayerModel.Points))
			.UsingConverter(score => localizationService.GetLocalization(new ScoreText(score)));

		view.Bind<LevelCompletePopupView, string>(view.SetScoreDescriptionText).ToProperty(
			viewModel, vm => vm.ScoreText, nameof(LevelCompletePopupViewModel.ScoreText));

		viewModel.LevelCompleteText =
			localizationService.GetParameterlessLocalization(
				LocalizationConstants.LevelCompleteText);

		view.Bind<LevelCompletePopupView, string>(view.SetLevelCompletionText).ToProperty(
			viewModel, vm => vm.LevelCompleteText,
			nameof(ILevelCompletePopupViewModel.LevelCompleteText));
	}
}
