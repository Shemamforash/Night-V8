using System;
using UnityEngine;
using Steamworks;

class AchievementManager : MonoBehaviour
{
	private enum Achievement : int
	{
		ACH_CRAFTITEMS,
		ACH_COMPANIONALIVE,
		ACH_SKILLSUSED,
		ACH_ELEMENTALTRIGGERS,
		ACH_JOURNALSREAD,
		ACH_WEAPONMAX,
		ACH_ARMOURMAX,
		ACH_ACTI,
		ACH_ACTII,
		ACH_ACTIII,
		ACH_ACTIV,
		ACH_ACTV
	};

	private readonly Achievement_t[] _achievements =
	{
		new Achievement_t(Achievement.ACH_CRAFTITEMS,        "Resourceful",        "Craft 50 items"),
		new Achievement_t(Achievement.ACH_COMPANIONALIVE,    "Alone Together",     "Keep a companion alive until the end"),
		new Achievement_t(Achievement.ACH_SKILLSUSED,        "Skilled",            "Use skills 200 times"),
		new Achievement_t(Achievement.ACH_ELEMENTALTRIGGERS, "Elemental",          "Cause Shatter, Burn, or Void damage 250 times"),
		new Achievement_t(Achievement.ACH_JOURNALSREAD,      "Well Read",          "Read 50 journals"),
		new Achievement_t(Achievement.ACH_WEAPONMAX,         "Dangerous",          "Inscribe a Radiant weapon with a Bellowing inscription"),
		new Achievement_t(Achievement.ACH_ARMOURMAX,         "Indestructible",     "Fully upgrade your armour"),
		new Achievement_t(Achievement.ACH_ACTI,              "The Serpent",        "Complete Act I"),
		new Achievement_t(Achievement.ACH_ACTII,             "The Mountain Queen", "Complete Act II"),
		new Achievement_t(Achievement.ACH_ACTIII,            "The Deepdweller",    "Complete Act III"),
		new Achievement_t(Achievement.ACH_ACTIV,             "The Dark One",       "Complete Act IV"),
		new Achievement_t(Achievement.ACH_ACTV,              "The End",            "Complete Act V")
	};

	// Our GameID
	private CGameID _gameId;

	// Did we get the stats from Steam?
	private bool _statsRequested;
	private bool _areStatsValid;

	// Should we store stats this frame?
	private bool _needToStoreStats;

	private int _craftedItemCount;
	private int _companionKeptAlive;
	private int _skillsUsed;
	private int _elementalTriggers;
	private int _journalsRead;
	private int _weaponMaxed;
	private int _armourMaxed;
	private int _actIComplete;
	private int _actIIComplete;
	private int _actIIIComplete;
	private int _actIVComplete;
	private int _actVComplete;

	protected      Callback<UserStatsReceived_t>     m_UserStatsReceived;
	protected      Callback<UserStatsStored_t>       m_UserStatsStored;
	protected      Callback<UserAchievementStored_t> m_UserAchievementStored;
	private static AchievementManager                _instance;

	void OnEnable()
	{
		if (!SteamManager.Initialised)
			return;

		// Cache the GameID for use in the Callbacks
		_gameId = new CGameID(SteamUtils.GetAppID());

		m_UserStatsReceived     = Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
		m_UserStatsStored       = Callback<UserStatsStored_t>.Create(OnUserStatsStored);
		m_UserAchievementStored = Callback<UserAchievementStored_t>.Create(OnAchievementStored);

		// These need to be reset to get the stats upon an Assembly reload in the Editor.
		_statsRequested = false;
		_areStatsValid  = false;
	}

	private void Awake()
	{
		_instance = this;
	}

	private void OnDestroy()
	{
		_instance = null;
	}

	private void Update()
	{
		if (!SteamManager.Initialised)
			return;

		if (!_statsRequested)
		{
			// Is Steam Loaded? if no, can't get stats, done
			if (!SteamManager.Initialised)
			{
				_statsRequested = true;
				return;
			}

			// If yes, request our stats
			bool bSuccess = SteamUserStats.RequestCurrentStats();

			// This function should only return false if we weren't logged in, and we already checked that.
			// But handle it being false again anyway, just ask again later.
			_statsRequested = bSuccess;
		}

		if (!_areStatsValid)
			return;

		// Get info from sources

		// Evaluate achievements
		UpdateAchievements();

		//Store stats in the Steam database if necessary
		if (!_needToStoreStats) return;
		StoreStats();
	}

	private void UpdateAchievements()
	{
		foreach (Achievement_t achievement in _achievements)
		{
			if (achievement.m_bAchieved)
				continue;

			switch (achievement.m_eAchievementID)
			{
				case Achievement.ACH_CRAFTITEMS:
					if (_craftedItemCount >= 50)
						UnlockAchievement(achievement);
					break;
				case Achievement.ACH_COMPANIONALIVE:
					if (_companionKeptAlive != 0)
						UnlockAchievement(achievement);
					break;
				case Achievement.ACH_SKILLSUSED:
					if (_skillsUsed >= 200)
						UnlockAchievement(achievement);
					break;
				case Achievement.ACH_ELEMENTALTRIGGERS:
					if (_elementalTriggers >= 250)
						UnlockAchievement(achievement);
					break;
				case Achievement.ACH_JOURNALSREAD:
					if (_journalsRead >= 50)
						UnlockAchievement(achievement);
					break;
				case Achievement.ACH_WEAPONMAX:
					if (_weaponMaxed != 0)
						UnlockAchievement(achievement);
					break;
				case Achievement.ACH_ARMOURMAX:
					if (_armourMaxed != 0)
						UnlockAchievement(achievement);
					break;
				case Achievement.ACH_ACTI:
					if (_actIComplete != 0)
						UnlockAchievement(achievement);
					break;
				case Achievement.ACH_ACTII:
					if (_actIIComplete != 0)
						UnlockAchievement(achievement);
					break;
				case Achievement.ACH_ACTIII:
					if (_actIIIComplete != 0)
						UnlockAchievement(achievement);
					break;
				case Achievement.ACH_ACTIV:
					if (_actIVComplete != 0)
						UnlockAchievement(achievement);
					break;
				case Achievement.ACH_ACTV:
					if (_actVComplete != 0)
						UnlockAchievement(achievement);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}

	private void StoreStats()
	{
		// already set any achievements in UnlockAchievement
		SteamUserStats.SetStat("CraftedItems",   _craftedItemCount);
		SteamUserStats.SetStat("CompanionAlive", _companionKeptAlive);
		SteamUserStats.SetStat("SkillsUsed",     _skillsUsed);
		Debug.Log(_skillsUsed);
		SteamUserStats.SetStat("ElementsTriggered", _elementalTriggers);
		SteamUserStats.SetStat("JournalsRead",      _journalsRead);
		SteamUserStats.SetStat("WeaponMaxed",       _weaponMaxed);
		SteamUserStats.SetStat("ArmourMaxed",       _armourMaxed);
		SteamUserStats.SetStat("ActIComplete",      _actIComplete);
		SteamUserStats.SetStat("ActIIComplete",     _actIIComplete);
		SteamUserStats.SetStat("ActIIIComplete",    _actIIIComplete);
		SteamUserStats.SetStat("ActIVComplete",     _actIVComplete);
		SteamUserStats.SetStat("ActVComplete",      _actVComplete);

		bool bSuccess = SteamUserStats.StoreStats();
		// If this failed, we never sent anything to the server, try
		// again later.
		_needToStoreStats = !bSuccess;
	}


	public void IncreaseItemsCrafted()
	{
		if (_craftedItemCount >= 50) return;
		++_craftedItemCount;
		SetStatsNeedUpdate();
	}

	public void KeepCompanionAlive()
	{
		if (_companionKeptAlive != 0) return;
		_companionKeptAlive = 1;
		SetStatsNeedUpdate();
	}

	public void IncreaseSkillsUsed()
	{
		if (_skillsUsed >= 200) return;
		++_skillsUsed;
		SetStatsNeedUpdate();
	}

	public void IncreaseElementTriggers()
	{
		if (_elementalTriggers > 250) return;
		++_elementalTriggers;
		SetStatsNeedUpdate();
	}

	public void IncreaseJournalsRead()
	{
		if (_journalsRead > 50) return;
		++_journalsRead;
		SetStatsNeedUpdate();
	}

	public void MaxOutWeapon()
	{
		if (_weaponMaxed != 0) return;
		_weaponMaxed = 1;
		SetStatsNeedUpdate();
	}

	public void MaxOutArmour()
	{
		if (_armourMaxed != 0) return;
		_armourMaxed = 1;
		SetStatsNeedUpdate();
	}

	public void CompleteActI()
	{
		if (_actIComplete != 0) return;
		_actIComplete = 1;
		SetStatsNeedUpdate();
	}

	public void CompleteActII()
	{
		if (_actIIComplete != 0) return;
		_actIIComplete = 1;
		SetStatsNeedUpdate();
	}

	public void CompleteActIII()
	{
		if (_actIIIComplete != 0) return;
		_actIIIComplete = 1;
		SetStatsNeedUpdate();
	}

	public void CompleteActIV()
	{
		if (_actIVComplete != 0) return;
		_actIVComplete = 1;
		SetStatsNeedUpdate();
	}

	public void CompleteActV()
	{
		if (_actVComplete != 0) return;
		_actVComplete = 1;
		SetStatsNeedUpdate();
	}

	//-----------------------------------------------------------------------------
	// Purpose: Game state has changed
	//-----------------------------------------------------------------------------
	private void SetStatsNeedUpdate()
	{
		if (!_areStatsValid)
			return;

		_needToStoreStats = true;
	}

	//-----------------------------------------------------------------------------
	// Purpose: Unlock this achievement
	//-----------------------------------------------------------------------------
	private void UnlockAchievement(Achievement_t achievement)
	{
		achievement.m_bAchieved = true;

		// the icon may change once it's unlocked
		achievement.m_iIconImage = 0;

		// mark it down
		SteamUserStats.SetAchievement(achievement.m_eAchievementID.ToString());

		// Store stats end of frame
		_needToStoreStats = true;
	}

	//-----------------------------------------------------------------------------
	// Purpose: We have stats data from Steam. It is authoritative, so update
	//			our data with those results now.
	//-----------------------------------------------------------------------------
	private void OnUserStatsReceived(UserStatsReceived_t pCallback)
	{
		if (!SteamManager.Initialised)
			return;

		// we may get callbacks for other games' stats arriving, ignore them
		if ((ulong) _gameId != pCallback.m_nGameID) return;
		if (EResult.k_EResultOK == pCallback.m_eResult)
		{
			Debug.Log("Received stats and achievements from Steam\n");

			_areStatsValid = true;

			// load achievements
			foreach (Achievement_t ach in _achievements)
			{
				bool ret = SteamUserStats.GetAchievement(ach.m_eAchievementID.ToString(), out ach.m_bAchieved);
				if (ret)
				{
					ach.m_strName        = SteamUserStats.GetAchievementDisplayAttribute(ach.m_eAchievementID.ToString(), "name");
					ach.m_strDescription = SteamUserStats.GetAchievementDisplayAttribute(ach.m_eAchievementID.ToString(), "desc");
				}
				else
				{
					Debug.LogWarning("SteamUserStats.GetAchievement failed for Achievement " + ach.m_eAchievementID + "\nIs it registered in the Steam Partner site?");
				}
			}

			SteamUserStats.GetStat("CraftedItems",      out _craftedItemCount);
			SteamUserStats.GetStat("CompanionAlive",    out _companionKeptAlive);
			SteamUserStats.GetStat("SkillsUsed",        out _skillsUsed);
			SteamUserStats.GetStat("ElementsTriggered", out _elementalTriggers);
			SteamUserStats.GetStat("JournalsRead",      out _journalsRead);
			SteamUserStats.GetStat("WeaponMaxed",       out _weaponMaxed);
			SteamUserStats.GetStat("ArmourMaxed",       out _armourMaxed);
			SteamUserStats.GetStat("ActIComplete",      out _actIComplete);
			SteamUserStats.GetStat("ActIIComplete",     out _actIIComplete);
			SteamUserStats.GetStat("ActIIIComplete",    out _actIIIComplete);
			SteamUserStats.GetStat("ActIVComplete",     out _actIVComplete);
			SteamUserStats.GetStat("ActVComplete",      out _actVComplete);
			return;
		}

		Debug.Log("RequestStats - failed, " + pCallback.m_eResult);
	}

	//-----------------------------------------------------------------------------
	// Purpose: Our stats data was stored!
	//-----------------------------------------------------------------------------
	private void OnUserStatsStored(UserStatsStored_t pCallback)
	{
		// we may get callbacks for other games' stats arriving, ignore them
		if ((ulong) _gameId != pCallback.m_nGameID) return;
		switch (pCallback.m_eResult)
		{
			case EResult.k_EResultOK:
				Debug.Log("StoreStats - success");
				break;
			case EResult.k_EResultInvalidParam:
			{
				// One or more stats we set broke a constraint. They've been reverted,
				// and we should re-iterate the values now to keep in sync.
				Debug.Log("StoreStats - some failed to validate");
				// Fake up a callback here so that we re-load the values.
				UserStatsReceived_t callback = new UserStatsReceived_t();
				callback.m_eResult = EResult.k_EResultOK;
				callback.m_nGameID = (ulong) _gameId;
				OnUserStatsReceived(callback);
				break;
			}

			default:
				Debug.Log("StoreStats - failed, " + pCallback.m_eResult);
				break;
		}
	}

	//-----------------------------------------------------------------------------
	// Purpose: An achievement was stored
	//-----------------------------------------------------------------------------
	private void OnAchievementStored(UserAchievementStored_t pCallback)
	{
		// We may get callbacks for other games' stats arriving, ignore them
		if ((ulong) _gameId != pCallback.m_nGameID) return;
		if (0 == pCallback.m_nMaxProgress)
		{
			Debug.Log("Achievement '" + pCallback.m_rgchAchievementName + "' unlocked!");
		}
		else
		{
			Debug.Log("Achievement '" + pCallback.m_rgchAchievementName + "' progress callback, (" + pCallback.m_nCurProgress + "," + pCallback.m_nMaxProgress + ")");
		}
	}

	private class Achievement_t
	{
		public Achievement m_eAchievementID;
		public string      m_strName;
		public string      m_strDescription;
		public bool        m_bAchieved;
		public int         m_iIconImage;

		/// <summary>
		/// Creates an Achievement. You must also mirror the data provided here in https://partner.steamgames.com/apps/achievements/yourappid
		/// </summary>
		/// <param name="achievement">The "API Name Progress Stat" used to uniquely identify the achievement.</param>
		/// <param name="name">The "Display Name" that will be shown to players in game and on the Steam Community.</param>
		/// <param name="desc">The "Description" that will be shown to players in game and on the Steam Community.</param>
		public Achievement_t(Achievement achievementID, string name, string desc)
		{
			m_eAchievementID = achievementID;
			m_strName        = name;
			m_strDescription = desc;
			m_bAchieved      = false;
		}
	}

	public static AchievementManager Instance()
	{
		return _instance;
	}
}