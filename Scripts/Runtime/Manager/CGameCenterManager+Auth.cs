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
#endif         // #if UNITY_IOS                          
#endif         // #if UNITY_IOS || UNITY_ANDROID                                           

/** 게임 센터 관리자 - 인증 */
public partial class CGameCenterManager : CSingleton<CGameCenterManager> {
#region 함수
	/** 로그인을 처리한다 */
	public void Login(System.Action<CGameCenterManager, bool> a_oCallback) {
		CFunc.ShowLog("CGameCenterManager.Login", KCDefine.B_LOG_COLOR_PLUGIN);

#if UNITY_IOS || UNITY_ANDROID
		// 초기화 되었을 경우
		if(!m_oBoolDict.GetValueOrDefault(EKey.IS_INIT) || this.IsLogin) {
			CFunc.Invoke(ref a_oCallback, this, this.IsLogin);
		} else {
			m_oCallbackDict.ExReplaceVal(EGameCenterCallback.LOGIN, a_oCallback);

#if UNITY_IOS
			Social.localUser.Authenticate(this.OnLogin);
#else
			PlayGamesPlatform.Instance.Authenticate((a_eLoginState) => this.OnLogin(a_eLoginState == SignInStatus.Success));
#endif         // #if UNITY_IOS                          
		}
#else
		CFunc.Invoke(ref a_oCallback, this, false);
#endif         // #if UNITY_IOS || UNITY_ANDROID                                           
	}

	/** 로그아웃을 처리한다 */
	public void Logout(System.Action<CGameCenterManager> a_oCallback) {
		CFunc.ShowLog("CGameCenterManager.Logout", KCDefine.B_LOG_COLOR_PLUGIN);

		try {
#if UNITY_IOS || UNITY_ANDROID
			// 로그인 되었을 경우
			if(m_oBoolDict.GetValueOrDefault(EKey.IS_INIT) && this.IsLogin) {
				// Do Something
			}
#endif         // #if UNITY_IOS || UNITY_ANDROID                                           
		} finally {
			CScheduleManager.Inst.AddCallback(KCDefine.U_KEY_GAME_CM_LOGOUT_CALLBACK, () => CFunc.Invoke(ref a_oCallback, this));
		}
	}
#endregion         // 함수               

#region 조건부 함수
#if UNITY_IOS || UNITY_ANDROID
	/** 로그인 되었을 경우 */
	private void OnLogin(bool a_bIsSuccess) {
		CFunc.ShowLog($"CGameCenterManager.OnLogin: {a_bIsSuccess}", KCDefine.B_LOG_COLOR_PLUGIN);

		CScheduleManager.Inst.AddCallback(KCDefine.U_KEY_GAME_CM_LOGIN_CALLBACK, () => {
#if UNITY_IOS
			m_oCallbackDict.GetValueOrDefault(EGameCenterCallback.LOGIN)?.Invoke(this, a_bIsSuccess);
#else
			// 로그인 되었을 경우
			if(a_bIsSuccess) {
				PlayGamesPlatform.Instance.RequestServerSideAccess(true, this.OnReceiveServerSideAccessResult);
			} else {
				m_oCallbackDict.GetValueOrDefault(EGameCenterCallback.LOGIN)?.Invoke(this, a_bIsSuccess);
			}
#endif         // #if UNITY_IOS                          
		});
	}
#endif         // #if UNITY_IOS || UNITY_ANDROID                                           
#endregion         // 조건부 함수                   
}
#endif         // #if GAME_CENTER_MODULE_ENABLE                                          
