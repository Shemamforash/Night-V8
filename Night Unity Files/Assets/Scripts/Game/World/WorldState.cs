using System;
using Facilitating.Persistence;
using Game.Characters;
using Game.Gear.Weapons;
using Game.World.Environment_and_Weather;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.CooldownSystem;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.ReactiveUI.InventoryUI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.World
{
    public class WorldState : MonoBehaviour
    {
        public static int StormDistanceMax, StormDistanceActual;
        public static int DaysSpentHere;
        private static GameObject _inventoryButton;
        public static EnvironmentManager EnvironmentManager = new EnvironmentManager();
        public static DesolationCharacterManager HomeInventory;
        private WeatherManager Weather = new WeatherManager();
        public event Action MinuteEvent;
        public event Action HourEvent;
        public event Action DayEvent;
        public event Action TravelEvent;
        public event Action<bool> PauseEvent;

        private static float _currentTime;
        public static int Days, Hours = 6, Minutes;
        public const int MinutesPerHour = 12;
        private static bool _isNight, _isPaused;
        private TextMeshProUGUI _timeText, _dayText;
        private static WorldState _instance;
        public static float MinuteInSeconds = .2f;

        public void Awake()
        {
            HomeInventory = new DesolationCharacterManager();
            _timeText = Helper.FindChildWithName(gameObject, "Time").GetComponent<TextMeshProUGUI>();
            _dayText = Helper.FindChildWithName(gameObject, "Day").GetComponent<TextMeshProUGUI>();
            _inventoryButton = Helper.FindChildWithName(gameObject, "Inventory");
            _inventoryButton.GetComponent<Button>().onClick.AddListener(() => InventoryTransferManager.Instance().ShowSingleInventory(Home()));
            _instance = this;
        }

        public void Start()
        {
            HomeInventory.Start();
            EnvironmentManager.Start();
            Weather.Start();

            SetResourceSuffix("Water", "sips");
            SetResourceSuffix("Food", "meals");
            SetResourceSuffix("Fuel", "dregs");
            SetResourceSuffix("Ammo", "rounds");
            SetResourceSuffix("Scrap", "bits");

            SaveController.LoadSettings();
            SaveController.LoadGame();

            DayEvent += IncrementDaysSpentHere;
            StormDistanceMax = 10;
            StormDistanceActual = 10;

#if UNITY_EDITOR
            HomeInventory.IncrementResource("Ammo", 100);
            for (int i = 0; i < 10; ++i)
            {
                HomeInventory.AddItem(WeaponGenerator.GenerateWeapon());
            }
#endif
        }

        public static GameObject GetInventoryButton() => _inventoryButton;

        private void IncrementDaysSpentHere()
        {
            ++DaysSpentHere;
            --StormDistanceActual;
            if (StormDistanceActual == 10)
            {
                FindNewLocation();
            }
            if (DaysSpentHere == 7)
            {
                //TODO gameover
            }
        }

        private void FindNewLocation()
        {
            StormDistanceMax--;
            StormDistanceActual = StormDistanceMax;
            if (StormDistanceMax == 0)
            {
                InitiateFinalEncounter();
            }
        }

        private void InitiateFinalEncounter()
        {
            //TODO
        }

        public static Inventory Home()
        {
            return HomeInventory;
        }

        private static void SetResourceSuffix(string name, string convention)
        {
            TextMeshProUGUI resourceText = GameObject.Find(name).GetComponent<TextMeshProUGUI>();
            HomeInventory.GetResource(name).AddOnUpdate(f => { resourceText.text = "<sprite name=\"" + name + "\">" + Mathf.Round(f) + " " + convention; });
        }

        public static WorldState Instance()
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType(typeof(WorldState)) as WorldState;
            }
            return _instance;
        }

        public void Pause()
        {
            _isPaused = true;
            if (PauseEvent != null) PauseEvent(true);
        }

        public void UnPause()
        {
            _isPaused = false;
            if (PauseEvent != null) PauseEvent(false);
        }

        private void IncrementWorld()
        {
            _currentTime += UnityEngine.Time.deltaTime;
            if (_currentTime >= MinuteInSeconds)
            {
                _currentTime = 0;
                Minutes += 60 / MinutesPerHour;
                MinuteEvent?.Invoke();
                if (Minutes == 60)
                {
                    Minutes = 0;
                    ++Hours;
                    HourEvent?.Invoke();
                    if (Hours == 24)
                    {
                        ++Days;
                        DayEvent?.Invoke();
                        Hours = 0;
                    }
                    //TODO make me make sense
                    if (Hours >= 6 && Hours < 20 && _isNight)
                    {
                        _isNight = false;
                    }
                    else if ((Hours < 6 || Hours >= 20) && !_isNight)
                    {
                        _isNight = true;
                    }
                }
            }
            if (Minutes < 10)
            {
                _timeText.text = Hours + ":0" + Minutes;
            }
            else
            {
                _timeText.text = Hours + ":" + Minutes;
            }
            _dayText.text = "Day " + Days;
        }

        void Update()
        {
            if (!_isPaused)
            {
                IncrementWorld();
            }
            CooldownManager.UpdateCooldowns();
        }
    }
}