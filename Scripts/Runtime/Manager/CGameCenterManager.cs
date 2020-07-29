using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if GAME_CENTER_ENABLE
using UnityEngine.SocialPlatforms;

#if UNITY_IOS
using UnityEngine.SocialPlatforms.GameCenter;
#elif UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif			// #if UNITY_IOS

//! 게임 센터 관리자
public class CGameCenterManager : CSingleton<CGameCenterManager> {
	#region 변수
	private System.Action<CGameCenterManager, bool> m_oLoginCallback = null;
	private System.Action<CGameCenterManager, bool> m_oUpdateScoreCallback = null;
	private System.Action<CGameCenterManager, bool> m_oUpdateAchievementCallback = null;
	private System.Action<CGameCenterManager, IScore[], bool> m_oLoadScoresCallback = null;
	#endregion			// 변수

	#region 프로퍼티
	public bool IsInit { get; private set; } = false;

	public bool IsLogin {
		get {
			if(this.IsInit) {
#if UNITY_IOS
				return Social.localUser.authenticated;
#elif UNITY_ANDROID
				return PlayGamesPlatform.Instance.IsAuthenticated();
#else
				return false;
#endif			// #if UNITY_IOS
			}

			return false;
		}
	}

	public string AuthCode {
		get {
#if UNITY_ANDROID
			return this.IsLogin ? PlayGamesPlatform.Instance.GetServerAuthCode() : string.Empty;
#else
			return KCDefine.U_AUTH_CODE_GAME_CM_UNKNOWN;
#endif			// #if UNITY_ANDROID
		}
	}
	#endregion			// 프로퍼티

	#region 함수
	//! 초기화
	public virtual void Init(System.Action<CGameCenterManager, bool> a_oCallback) {
		CFunc.ShowLog("CGameCenterManager.Init", KCDefine.B_LOG_COLOR_PLUGIN);

		if(!this.IsInit && CAccess.IsMobilePlatform()) {
			this.IsInit = true;

#if UNITY_IOS
			GameCenterPlatform.ShowDefaultAchievementCompletionBanner(true);
#elif UNITY_ANDROID
			var oBuilder = new PlayGamesClientConfiguration.Builder();
			
#if SAVED_GAME_ENABLE && STORE_BUILD
			oBuilder.EnableSavedGames();
#endif			// #if SAVED_GAME_ENABLE && STORE_BUILD

			PlayGamesPlatform.InitializeInstance(oBuilder.Build());

#if ADHOC_BUILD || STORE_BUILD
			PlayGamesPlatform.DebugLogEnabled = false;
#else
			PlayGamesPlatform.DebugLogEnabled = false;
#endif			// #if ADHOC_BUILD || STORE_BUILD

			PlayGamesPlatform.Activate();
#endif			// #if UNITY_IOS
		}

		a_oCallback?.Invoke(this, this.IsInit);
	}

	//! 로그인 되었을 경우
	public void OnLogin(bool a_bIsSuccess) {
		CScheduleManager.Instance.AddCallback(KCDefine.U_KEY_GAME_CM_LOGIN_CALLBACK, () => {
			CFunc.ShowLog("CGameCenterManager.OnLogin: {0}", KCDefine.B_LOG_COLOR_PLUGIN, a_bIsSuccess);
			m_oLoginCallback?.Invoke(this, a_bIsSuccess);
		});
	}

	//! 점수를 로드했을 경우
	public void OnLoadScores(IScore[] a_oScores) {
		CScheduleManager.Instance.AddCallback(KCDefine.U_KEY_GAME_CM_LOAD_SCORES_CALLBACK, () => {
			CFunc.ShowLog("CGameCenterManager.OnLoadScores: {0}", KCDefine.B_LOG_COLOR_PLUGIN, a_oScores);
			m_oLoadScoresCallback?.Invoke(this, a_oScores, a_oScores != null);
		});	
	}

	//! 점수를 갱신했을 경우
	public void OnUpdateScore(bool a_bIsSuccess) {
		CScheduleManager.Instance.AddCallback(KCDefine.U_KEY_GAME_CM_UPDATE_SCORE_CALLBACK, () => {
			CFunc.ShowLog("CGameCenterManager.OnUpdateScore: {0}", KCDefine.B_LOG_COLOR_PLUGIN, a_bIsSuccess);
			m_oUpdateScoreCallback?.Invoke(this, a_bIsSuccess);
		});
	}

	//! 업적을 갱신했을 경우
	public void OnUpdateAchievement(bool a_bIsSuccess) {
		CScheduleManager.Instance.AddCallback(KCDefine.U_KEY_GAME_CM_UPDATE_ACHIEVEMENT_CALLBACK, () => {
			CFunc.ShowLog("CGameCenterManager.OnUpdateAchievement: {0}", KCDefine.B_LOG_COLOR_PLUGIN, a_bIsSuccess);
			m_oUpdateAchievementCallback?.Invoke(this, a_bIsSuccess);
		});
	}

	//! 로그인을 처리한다
	public void Login(System.Action<CGameCenterManager, bool> a_oCallback) {
		CFunc.ShowLog("CGameCenterManager.Login", KCDefine.B_LOG_COLOR_PLUGIN);

		if(!this.IsInit || this.IsLogin) {
			a_oCallback?.Invoke(this, this.IsLogin);
		} else {
			m_oLoginCallback = a_oCallback;

#if UNITY_IOS
			Social.localUser.Authenticate(this.OnLogin);
#elif UNITY_ANDROID
			PlayGamesPlatform.Instance.Authenticate(this.OnLogin);
#endif			// #if UNITY_IOS
		}
	}

	//! 로그아웃을 처리한다
	public void Logout(System.Action<CGameCenterManager> a_oCallback) {
		CFunc.ShowLog("CGameCenterManager.Logout", KCDefine.B_LOG_COLOR_PLUGIN);

		if(this.IsInit) {
#if UNITY_ANDROID
			PlayGamesPlatform.Instance.SignOut();
#endif			// #if UNITY_ANDROID
		}

		a_oCallback?.Invoke(this);
	}
	
	//! 리더보드 UI 를 출력한다
	public void ShowLeaderboardUI() {
		CFunc.ShowLog("CGameCenterManager.ShowLeaderboardUI", KCDefine.B_LOG_COLOR_PLUGIN);

		if(this.IsInit) {
#if UNITY_IOS
			Social.ShowLeaderboardUI();
#elif UNITY_ANDROID
			PlayGamesPlatform.Instance.ShowLeaderboardUI();
#endif			// #if UNITY_IOS
		}
	}

	//! 업적 UI 를 출력한다
	public void ShowAchievementUI() {
		CFunc.ShowLog("CGameCenterManager.ShowAchievementUI", KCDefine.B_LOG_COLOR_PLUGIN);

		if(this.IsInit) {
#if UNITY_IOS
			Social.ShowAchievementsUI();
#elif UNITY_ANDROID
			PlayGamesPlatform.Instance.ShowAchievementsUI();
#endif			// #if UNITY_IOS
		}
	}

	//! 점수를 갱신한다
	public void UpdateScore(string a_oLeaderboardID, long a_nScore, System.Action<CGameCenterManager, bool> a_oCallback) {
		CAccess.Assert(a_oLeaderboardID.ExIsValid());
		CFunc.ShowLog("CGameCenterManager.UpdateScore: {0}, {1}", KCDefine.B_LOG_COLOR_PLUGIN, a_oLeaderboardID, a_nScore);

		if(!this.IsInit) {
			a_oCallback?.Invoke(this, false);
		} else {
			m_oUpdateScoreCallback = a_oCallback;

#if UNITY_IOS
			Social.ReportScore(a_nScore, a_oLeaderboardID, this.OnUpdateScore);
#elif UNITY_ANDROID
			PlayGamesPlatform.Instance.ReportScore(a_nScore, a_oLeaderboardID, this.OnUpdateScore);
#endif			// #if UNITY_IOS
		}
	}

	//! 업적을 갱신한다
	public void UpdateAchievement(string a_oAchievementID, double a_dblPercent, System.Action<CGameCenterManager, bool> a_oCallback) {
		CAccess.Assert(a_oAchievementID.ExIsValid());
		CFunc.ShowLog("CGameCenterManager.UpdateAchievement: {0}, {1}", KCDefine.B_LOG_COLOR_PLUGIN, a_oAchievementID, a_dblPercent);

		if(!this.IsInit) {
			a_oCallback?.Invoke(this, false);
		} else {
			m_oUpdateAchievementCallback = a_oCallback;

#if UNITY_IOS
			Social.ReportProgress(a_oAchievementID, a_dblPercent, this.OnUpdateAchievement);
#elif UNITY_ANDROID
			PlayGamesPlatform.Instance.ReportProgress(a_oAchievementID, a_dblPercent, this.OnUpdateAchievement);
#endif			// #if UNITY_IOS
		}
	}
	#endregion			// 함수
}
#endif			// #if GAME_CENTER_ENABLE
