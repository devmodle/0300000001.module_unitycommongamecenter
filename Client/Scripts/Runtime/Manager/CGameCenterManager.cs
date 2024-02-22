using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

#if GAME_CENTER_MODULE_ENABLE
#if UNITY_IOS || UNITY_ANDROID
using UnityEngine.SocialPlatforms;

#if UNITY_IOS
using UnityEngine.SocialPlatforms.GameCenter;
#elif UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif // #if UNITY_IOS
#endif // #if UNITY_IOS || UNITY_ANDROID

/** 게임 센터 관리자 */
public partial class CGameCenterManager : CSingleton<CGameCenterManager> {
	/** 콜백 */
	public enum ECallback {
		NONE = -1,
		INIT,
		[HideInInspector] MAX_VAL
	}

	/** 게임 센터 콜백 */
	private enum EGameCenterCallback {
		NONE = -1,
		LOGIN,
		UPDATE_RECORD,
		UPDATE_ACHIEVEMENT,
		[HideInInspector] MAX_VAL
	}

	/** 매개 변수 */
	public struct STParams {
		public Dictionary<ECallback, System.Action<CGameCenterManager, bool>> m_oCallbackDict;
	}

	#region 변수
	private Dictionary<EGameCenterCallback, System.Action<CGameCenterManager, bool>> m_oCallbackDict = new Dictionary<EGameCenterCallback, System.Action<CGameCenterManager, bool>>();
	#endregion // 변수

	#region 프로퍼티
	public STParams Params { get; private set; }

	public bool IsInit { get; private set; } = false;
	public string AccessToken { get; private set; } = string.Empty;

	public bool IsLogin {
		get {
#if UNITY_IOS
			return this.IsInit ? Social.localUser.authenticated : false;
#elif UNITY_ANDROID
			return this.IsInit ? PlayGamesPlatform.Instance.IsAuthenticated() : false;
#else
			return false;
#endif // #if UNITY_IOS
		}
	}

	public string UserID {
		get {
#if UNITY_IOS
			return this.IsLogin ? Social.localUser.id : string.Empty;
#elif UNITY_ANDROID
			return this.IsLogin ? PlayGamesPlatform.Instance.GetUserId() : string.Empty;
#else
			return string.Empty;
#endif // #if UNITY_IOS
		}
	}
	#endregion // 프로퍼티

	#region 함수
	/** 초기화 */
	public virtual void Init(STParams a_stParams) {
		CFunc.ShowLog("CGameCenterManager.Init", KCDefine.B_LOG_COLOR_PLUGIN);

#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
		// 초기화되었을 경우
		if(this.IsInit) {
			a_stParams.m_oCallbackDict?.GetValueOrDefault(ECallback.INIT)?.Invoke(this, this.IsInit);
		} else {
			this.Params = a_stParams;

#if UNITY_IOS
			GameCenterPlatform.ShowDefaultAchievementCompletionBanner(true);
#else
#if DEBUG || DEVELOPMENT_BUILD
			PlayGamesPlatform.DebugLogEnabled = true;
#else
			PlayGamesPlatform.DebugLogEnabled = false;
#endif // #if DEBUG || DEVELOPMENT_BUILD

			PlayGamesPlatform.Activate();
#endif // #if UNITY_IOS

			this.ExLateCallFunc((a_oSender) => this.OnInit());
		}
#else
		a_stParams.m_oCallbackDict?.GetValueOrDefault(ECallback.INIT)?.Invoke(this, false);
#endif // #if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
	}

	/** 리더보드 UI 를 출력한다 */
	public void ShowLeaderboardUIs() {
		CFunc.ShowLog("CGameCenterManager.ShowLeaderboardUIs", KCDefine.B_LOG_COLOR_PLUGIN);

#if UNITY_IOS || UNITY_ANDROID
		// 초기화되었을 경우
		if(this.IsInit) {
#if UNITY_IOS
			Social.ShowLeaderboardUI();
#else
			PlayGamesPlatform.Instance.ShowLeaderboardUI();
#endif // #if UNITY_IOS
		}
#endif // #if UNITY_IOS || UNITY_ANDROID
	}

	/** 업적 UI 를 출력한다 */
	public void ShowAchievementUIs() {
		CFunc.ShowLog("CGameCenterManager.ShowAchievementUIs", KCDefine.B_LOG_COLOR_PLUGIN);

#if UNITY_IOS || UNITY_ANDROID
		// 초기화되었을 경우
		if(this.IsInit) {
#if UNITY_IOS
			Social.ShowAchievementsUI();
#else
			PlayGamesPlatform.Instance.ShowAchievementsUI();
#endif // #if UNITY_IOS
		}
#endif // #if UNITY_IOS || UNITY_ANDROID
	}

	/** 기록을 갱신한다 */
	public void UpdateRecord(string a_oLeaderboardID, long a_nRecord, System.Action<CGameCenterManager, bool> a_oCallback) {
		CFunc.ShowLog($"CGameCenterManager.UpdateRecord: {a_oLeaderboardID}, {a_nRecord}", KCDefine.B_LOG_COLOR_PLUGIN);
		CFunc.Assert(a_nRecord >= KCDefine.B_VAL_0_INT && a_oLeaderboardID.ExIsValid());

#if UNITY_IOS || UNITY_ANDROID
		// 초기화되었을 경우
		if(this.IsInit) {
			m_oCallbackDict.ExReplaceVal(EGameCenterCallback.UPDATE_RECORD, a_oCallback);

#if UNITY_IOS
			Social.ReportScore(a_nRecord, a_oLeaderboardID, this.OnUpdateRecord);
#else
			PlayGamesPlatform.Instance.ReportScore(a_nRecord, a_oLeaderboardID, this.OnUpdateRecord);
#endif // #if UNITY_IOS
		} else {
			CFunc.Invoke(ref a_oCallback, this, false);
		}
#else
		CFunc.Invoke(ref a_oCallback, this, false);
#endif // #if UNITY_IOS || UNITY_ANDROID
	}

	/** 업적을 갱신한다 */
	public void UpdateAchievement(string a_oAchievementID, double a_dblPercent, System.Action<CGameCenterManager, bool> a_oCallback) {
		CFunc.ShowLog($"CGameCenterManager.UpdateAchievement: {a_oAchievementID}, {a_dblPercent}", KCDefine.B_LOG_COLOR_PLUGIN);
		CFunc.Assert(a_oAchievementID.ExIsValid() && a_dblPercent.ExIsGreatEquals(KCDefine.B_VAL_0_REAL));

#if UNITY_IOS || UNITY_ANDROID
		// 초기화되었을 경우
		if(this.IsInit) {
			m_oCallbackDict.ExReplaceVal(EGameCenterCallback.UPDATE_ACHIEVEMENT, a_oCallback);

#if UNITY_IOS
			Social.ReportProgress(a_oAchievementID, a_dblPercent, this.OnUpdateAchievement);
#else
			PlayGamesPlatform.Instance.ReportProgress(a_oAchievementID, a_dblPercent, this.OnUpdateAchievement);
#endif // #if UNITY_IOS
		} else {
			CFunc.Invoke(ref a_oCallback, this, false);
		}
#else
		CFunc.Invoke(ref a_oCallback, this, false);
#endif // #if UNITY_IOS || UNITY_ANDROID
	}
	#endregion // 함수

	#region 조건부 함수
#if UNITY_IOS || UNITY_ANDROID
	/** 초기화되었을 경우 */
	private void OnInit() {
		CFunc.ShowLog("CGameCenterManager.OnInit", KCDefine.B_LOG_COLOR_PLUGIN);

		CScheduleManager.Inst.AddCallback(KCDefine.U_KEY_GAME_CM_INIT_CALLBACK, () => {
			this.IsInit = true;
			this.Params.m_oCallbackDict?.GetValueOrDefault(ECallback.INIT)?.Invoke(this, this.IsInit);
		});
	}
	
	/** 기록이 갱신되었을 경우 */
	private void OnUpdateRecord(bool a_bIsSuccess) {
		CFunc.ShowLog($"CGameCenterManager.OnUpdateRecord: {a_bIsSuccess}", KCDefine.B_LOG_COLOR_PLUGIN);
		CScheduleManager.Inst.AddCallback(KCDefine.U_KEY_GAME_CM_UPDATE_RECORD_CALLBACK, () => m_oCallbackDict.GetValueOrDefault(EGameCenterCallback.UPDATE_RECORD)?.Invoke(this, a_bIsSuccess));
	}

	/** 업적이 갱신되었을 경우 */
	private void OnUpdateAchievement(bool a_bIsSuccess) {
		CFunc.ShowLog($"CGameCenterManager.OnUpdateAchievement: {a_bIsSuccess}", KCDefine.B_LOG_COLOR_PLUGIN);
		CScheduleManager.Inst.AddCallback(KCDefine.U_KEY_GAME_CM_UPDATE_ACHIEVEMENT_CALLBACK, () => m_oCallbackDict.GetValueOrDefault(EGameCenterCallback.UPDATE_ACHIEVEMENT)?.Invoke(this, a_bIsSuccess));
	}

#if UNITY_ANDROID
	/** 서버 접근 결과를 수신했을 경우 */
	private void OnReceiveServerSideAccessResult(string a_oAccessToken) {
		CFunc.ShowLog($"CGameCenterManager.OnReceiveServerSideAccessResult: {a_oAccessToken}", KCDefine.B_LOG_COLOR_PLUGIN);

		CScheduleManager.Inst.AddCallback(KCDefine.U_KEY_GAME_CM_RECEIVE_SERVER_SIDE_ACCESS_RESULT_CALLBACK, () => {
			this.AccessToken = a_oAccessToken.ExIsValid() ? a_oAccessToken : string.Empty;
			m_oCallbackDict.GetValueOrDefault(EGameCenterCallback.LOGIN)?.Invoke(this, this.IsLogin);
		});
	}
#endif // #if UNITY_ANDROID
#endif // #if UNITY_IOS || UNITY_ANDROID
	#endregion // 조건부 함수
}

/** 게임 센터 관리자 - 팩토리 */
public partial class CGameCenterManager : CSingleton<CGameCenterManager> {
	#region 클래스 함수
	/** 매개 변수를 생성한다 */
	public static STParams MakeParams(Dictionary<ECallback, System.Action<CGameCenterManager, bool>> a_oCallbackDict = null) {
		return new STParams() {
			m_oCallbackDict = a_oCallbackDict ?? new Dictionary<ECallback, System.Action<CGameCenterManager, bool>>()
		};
	}
	#endregion // 클래스 함수
}
#endif // #if GAME_CENTER_MODULE_ENABLE

