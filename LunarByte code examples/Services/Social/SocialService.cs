using System;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
using LunarConsole;
using UnityEngine;
using UnityEngine.SocialPlatforms;
#if UNITY_IOS
using UnityEngine.SocialPlatforms.GameCenter;
#endif
using Logger = LunarConsole.API.Logger;

/// <summary>
/// Service for handling Google Play Games (Android) and Game Center (iOS) achievements and leaderboards.
/// Requires that the user is logged on which in this case is handled by save service because save service
/// was required to be initialized earlier than the social service.
/// </summary>
[LogCategory(nameof(SocialService))]
public class SocialService : Service
{
	private readonly RuntimePlatform CurrentPlatform;

	public SocialService()
	{
		CurrentPlatform = Application.platform;
    }

    public void AddScore(long score, string leaderboardId)
	{
		if (!Social.localUser.authenticated)
		{
			Logger.LogError("Not logged in!");
			return;
		}

		Social.ReportScore(score, leaderboardId, (bool success) =>
		{
			if (success)
			{
				Logger.Log($"Posted new score to leaderboard {leaderboardId}");
			}
			else
			{
				Logger.LogError($"Unable to post new score to leaderboard {leaderboardId}");
			}
		});
    }

	public void ShowIntegratedLeaderboard()
	{
		if (!Social.localUser.authenticated)
		{
			Logger.LogError("Not logged in!");
			return;
		}
        Social.ShowLeaderboardUI();
    }

	public void ShowIntegratedAchievements()
	{
		if (!Social.localUser.authenticated)
		{
			Logger.LogError("Not logged in!");
			return;
		}
        Social.ShowAchievementsUI();
    }

	public void UnlockAchievement(string achievementId)
	{
		if (!Social.localUser.authenticated)
		{
			Logger.LogError("Not logged in!");
			return;
		}
        if (CurrentPlatform == RuntimePlatform.Android)
		{
			Social.ReportProgress(achievementId, 100.0f, (bool success) =>
			{
				if (success)
				{
					Logger.Log("Achievement unlocked");

				}
				else
				{
					Logger.LogError("Achievement failed to unlock");
				}
			});
        }
		else
		{
			Social.ReportProgress(achievementId, 100.0, (bool success) =>
			{
				if (success)
				{
					Logger.Log("Achievement unlocked");
                }
				else
				{
					Logger.LogError("Achievement failed to unlock");
				}
			});
        }
	}

	public void IncrementAchievement(string achievementId, int progress)
	{
		if (!Social.localUser.authenticated)
		{
			Logger.LogError("Not logged in!");
			return;
		}
        if (CurrentPlatform == RuntimePlatform.Android)
		{
#if UNITY_ANDROID
	    PlayGamesPlatform.Instance.IncrementAchievement(achievementId, progress, (bool success) =>
	    {
		    if (success)
		    {
			    Logger.Log("Achievement incremented");
		    }
		    else
		    {
			    Logger.LogError("Achievement failed to increment");
		    }
	    });
#endif
        }
        else
		{
            Social.LoadAchievements(achievements =>
            {
	            if (achievements.Length > 0)
	            {
		            foreach (IAchievement achievement in achievements)
		            {
			            if (achievement.id == achievementId)
			            {

				            Social.ReportProgress(achievementId, achievement.percentCompleted + (double)progress,
				                                  success =>
				                                  {
					                                  if (success)
					                                  {
						                                  Logger.Log("Achievement incremented");

					                                  }
					                                  else
					                                  {
						                                  Logger.LogError(
							                                  "Achievement failed to increment");
					                                  }
				                                  });
			            }
		            }
	            }
            });
        }
	}
}
