using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if GAME_CENTER_MODULE_ENABLE
#if UNITY_IOS || UNITY_ANDROID
using UnityEngine.SocialPlatforms;

#if UNITY_IOS
using UnityEngine.SocialPlatforms.GameCenter;
#elif UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif			// #if UNITY_IOS
#endif			// #if UNITY_IOS || UNITY_ANDROID

//! 게임 센터 관리자
public class CGameCenterManager : CSingleton<CGameCenterManager> {
	#region 변수
	private System.Action<CGameCenterManager, bool> m_oInitCallback = null;
	private System.Action<CGameCenterManager, bool> m_oLoginCallback = null;
	private System.Action<CGameCenterManager, bool> m_oUpdateScoreCallback = null;
	private System.Action<CGameCenterManager, bool> m_oUpdateAchievementCallback = null;
	#endregion			// 변수

	#region 프로퍼티
	public bool IsInit { get; private set; } = false;

	public bool IsLogin {
		get {
#if UNITY_IOS || UNITY_ANDROID
			// 초기화 되었을 경우
			if(this.IsInit) {
#if UNITY_IOS
				return Social.localUser.authenticated;
#else
				return PlayGamesPlatform.Instance.IsAuthenticated();
#endif			// #if UNITY_IOS
			} else {
				return false;
			}
#else
			return false;
#endif			// #if UNITY_IOS || UNITY_ANDROID
		}
	}

	public string AuthCode {
		get {
#if UNITY_ANDROID
			return this.IsLogin ? PlayGamesPlatform.Instance.GetServerAuthCode() : string.Empty;
#else
			return string.Empty;
#endif			// #if UNITY_ANDROID
		}
	}
	#endregion			// 프로퍼티

	#region 함수
	//! 초기화
	public virtual void Init(System.Action<CGameCenterManager, bool> a_oCallback) {
		CFunc.ShowLog("CGameCenterManager.Init", KCDefine.B_LOG_COLOR_PLUGIN);

#if UNITY_IOS || UNITY_ANDROID
		// 초기화 되었을 경우
		if(this.IsInit) {
			a_oCallback?.Invoke(this, true);
		} else {
#if UNITY_IOS
			GameCenterPlatform.ShowDefaultAchievementCompletionBanner(true);
#else
			var oBuilder = new PlayGamesClientConfiguration.Builder();
			
#if STORE_BUILD && GAME_CENTER_SAVE_ENABLE
			oBuilder.EnableSavedGames();
#endif			// #if STORE_BUILD && GAME_CENTER_SAVE_ENABLE

			PlayGamesPlatform.InitializeInstance(oBuilder.Build());

#if ADHOC_BUILD || STORE_BUILD
			PlayGamesPlatform.DebugLogEnabled = false;
#else
			PlayGamesPlatform.DebugLogEnabled = true;
#endif			// #if ADHOC_BUILD || STORE_BUILD

			PlayGamesPlatform.Activate();
#endif			// #if UNITY_IOS

			this.ExLateCallFunc((a_oSender, a_oParams) => this.OnInit());
		}
#else
		a_oCallback?.Invoke(this, false);
#endif			// #if UNITY_IOS || UNITY_ANDROID
	}

	//! 로그인을 처리한다
	public void Login(System.Action<CGameCenterManager, bool> a_oCallback) {
		CFunc.ShowLog("CGameCenterManager.Login", KCDefine.B_LOG_COLOR_PLUGIN);

#if UNITY_IOS || UNITY_ANDROID
		// 로그인 되었을 경우
		if(!this.IsInit || this.IsLogin) {
			a_oCallback?.Invoke(this, this.IsLogin);
		} else {
			m_oLoginCallback = a_oCallback;

#if UNITY_IOS
			Social.localUser.Authenticate(this.OnLogin);
#else
			PlayGamesPlatform.Instance.Authenticate(this.OnLogin);
#endif			// #if UNITY_IOS
		}
#else
		a_oCallback?.Invoke(this, false);
#endif			// #if UNITY_IOS || UNITY_ANDROID
	}

	//! 로그아웃을 처리한다
	public void Logout(System.Action<CGameCenterManager> a_oCallback) {
		CFunc.ShowLog("CGameCenterManager.Logout", KCDefine.B_LOG_COLOR_PLUGIN);

#if UNITY_ANDROID
		// 초기화 되었을 경우
		if(this.IsInit) {
			PlayGamesPlatform.Instance.SignOut();
		}
#endif			// #if UNITY_ANDROID

		a_oCallback?.Invoke(this);
	}
	
	//! 리더보드 UI 를 출력한다
	public void ShowLeaderboardUIs() {
		CFunc.ShowLog("CGameCenterManager.ShowLeaderboardUIs", KCDefine.B_LOG_COLOR_PLUGIN);

#if UNITY_IOS || UNITY_ANDROID
		// 초기화 되었을 경우
		if(this.IsInit) {
#if UNITY_IOS
			Social.ShowLeaderboardUI();
#else
			PlayGamesPlatform.Instance.ShowLeaderboardUI();
#endif			// #if UNITY_IOS
		}
#endif			// #if UNITY_IOS || UNITY_ANDROID
	}

	//! 업적 UI 를 출력한다
	public void ShowAchievementUIs() {
		CFunc.ShowLog("CGameCenterManager.ShowAchievementUIs", KCDefine.B_LOG_COLOR_PLUGIN);

#if UNITY_IOS || UNITY_ANDROID
		// 초기화 되었을 경우
		if(this.IsInit) {
#if UNITY_IOS
			Social.ShowAchievementsUI();
#else
			PlayGamesPlatform.Instance.ShowAchievementsUI();
#endif			// #if UNITY_IOS
		}
#endif			// #if UNITY_IOS || UNITY_ANDROID
	}

	//! 점수를 갱신한다
	public void UpdateScore(string a_oLeaderboardID, long a_nScore, System.Action<CGameCenterManager, bool> a_oCallback) {
		CAccess.Assert(a_nScore >= KCDefine.B_VAL_0_LONG && a_oLeaderboardID.ExIsValid());
		CFunc.ShowLog($"CGameCenterManager.UpdateScore: {a_oLeaderboardID}, {a_nScore}", KCDefine.B_LOG_COLOR_PLUGIN);

#if UNITY_IOS || UNITY_ANDROID
		// 초기화 되었을 경우
		if(this.IsInit) {
			m_oUpdateScoreCallback = a_oCallback;

#if UNITY_IOS
			Social.ReportScore(a_nScore, a_oLeaderboardID, this.OnUpdateScore);
#else
			PlayGamesPlatform.Instance.ReportScore(a_nScore, a_oLeaderboardID, this.OnUpdateScore);
#endif			// #if UNITY_IOS
		} else {
			a_oCallback?.Invoke(this, false);
		}
#else
		a_oCallback?.Invoke(this, false);
#endif			// #if UNITY_IOS || UNITY_ANDROID
	}

	//! 업적을 갱신한다
	public void UpdateAchievement(string a_oAchievementID, double a_dblPercent, System.Action<CGameCenterManager, bool> a_oCallback) {
		CAccess.Assert(a_oAchievementID.ExIsValid());
		CAccess.Assert(a_dblPercent.ExIsGreateEquals(KCDefine.B_VAL_0_DBL));

		CFunc.ShowLog($"CGameCenterManager.UpdateAchievement: {a_oAchievementID}, {a_dblPercent}", KCDefine.B_LOG_COLOR_PLUGIN);

#if UNITY_IOS || UNITY_ANDROID
		// 초기화 되었을 경우
		if(this.IsInit) {
			m_oUpdateAchievementCallback = a_oCallback;

#if UNITY_IOS
			Social.ReportProgress(a_oAchievementID, a_dblPercent, this.OnUpdateAchievement);
#else
			PlayGamesPlatform.Instance.ReportProgress(a_oAchievementID, a_dblPercent, this.OnUpdateAchievement);
#endif			// #if UNITY_IOS
		} else {
			a_oCallback?.Invoke(this, false);
		}
#else
		a_oCallback?.Invoke(this, false);
#endif			// #if UNITY_IOS || UNITY_ANDROID
	}
	#endregion			// 함수

	#region 조건부 함수
#if UNITY_IOS || UNITY_ANDROID
	//! 초기화 되었을 경우
	private void OnInit() {
		CScheduleManager.Inst.AddCallback(KCDefine.U_KEY_GAME_CM_INIT_CALLBACK, () => {
			CFunc.ShowLog("CGameCenterManager.OnInit");
			this.IsInit = true;
			
			CFunc.Invoke(ref m_oInitCallback, this, this.IsInit);
		});
	}

	//! 로그인 되었을 경우
	private void OnLogin(bool a_bIsSuccess) {
		CScheduleManager.Inst.AddCallback(KCDefine.U_KEY_GAME_CM_LOGIN_CALLBACK, () => {
			CFunc.ShowLog($"CGameCenterManager.OnLogin: {a_bIsSuccess}", KCDefine.B_LOG_COLOR_PLUGIN);
			CFunc.Invoke(ref m_oLoginCallback, this, a_bIsSuccess);
		});
	}

	//! 점수가 갱신 되었을 경우
	private void OnUpdateScore(bool a_bIsSuccess) {
		CScheduleManager.Inst.AddCallback(KCDefine.U_KEY_GAME_CM_UPDATE_SCORE_CALLBACK, () => {
			CFunc.ShowLog($"CGameCenterManager.OnUpdateScore: {a_bIsSuccess}", KCDefine.B_LOG_COLOR_PLUGIN);
			CFunc.Invoke(ref m_oUpdateScoreCallback, this, a_bIsSuccess);
		});
	}

	//! 업적이 갱신 되었을 경우
	private void OnUpdateAchievement(bool a_bIsSuccess) {
		CScheduleManager.Inst.AddCallback(KCDefine.U_KEY_GAME_CM_UPDATE_ACHIEVEMENT_CALLBACK, () => {
			CFunc.ShowLog($"CGameCenterManager.OnUpdateAchievement: {a_bIsSuccess}", KCDefine.B_LOG_COLOR_PLUGIN);
			CFunc.Invoke(ref m_oUpdateAchievementCallback, this, a_bIsSuccess);
		});
	}
#endif			// #if UNITY_IOS || UNITY_ANDROID
	#endregion			// 조건부 함수
}
#endif			// #if GAME_CENTER_MODULE_ENABLE
