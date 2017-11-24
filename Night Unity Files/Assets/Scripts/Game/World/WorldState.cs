using System;
using System.Collections.Generic;
using Facilitating.MenuNavigation;
using Facilitating.Persistence;
using Game.Characters;
using Game.Gear.Armour;
using Game.Gear.Weapons;
using Game.World.Environment_and_Weather;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.Characters;
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
        public static CooldownManager WorldCooldownManager = new CooldownManager();
        public static int StormDistanceMax, StormDistanceActual;
        public static int DaysSpentHere;
        private static Button _inventoryButton;
        public static readonly EnvironmentManager EnvironmentManager = new EnvironmentManager();
        private static CharacterManager _homeInventory = new CharacterManager();
        private readonly WeatherManager Weather = new WeatherManager();
        private event Action MinuteEvent, HourEvent, DayEvent, TravelEvent;

        private static float _currentTime;
        public static int Days, Hours = 6, Minutes;
        public const int MinutesPerHour = 12;
        private static bool _isNight, _isPaused;
        private TextMeshProUGUI _timeText, _dayText;
        private static WorldState _instance;
        public static float MinuteInSeconds = .2f;

        public void Awake()
        {
            _timeText = Helper.FindChildWithName(gameObject, "Time").GetComponent<TextMeshProUGUI>();
            _dayText = Helper.FindChildWithName(gameObject, "Day").GetComponent<TextMeshProUGUI>();
            _inventoryButton = Helper.FindChildWithName<Button>(gameObject, "Inventory");
            _inventoryButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                Popup popup = new Popup("Vehicle Inventory");
                List<MyGameObject> visibleContents = _homeInventory.SortByType();
                popup.AddList(visibleContents, g =>
                {
                    GearItem item = g as GearItem;
                    if (item != null)
                    {
                        ShowCharacterPopup(item.Name, item);
                    }
                }, true);
                popup.AddBackButton();
            });
            _instance = this;
        }

        private void ShowCharacterPopup(string name, GearItem gearItem)
        {
            Popup popupWithList = new Popup("Equip " + name);
            List<MyGameObject> characterGear = new List<MyGameObject>();
            CharacterManager.Characters().ForEach(c => characterGear.Add(new CharacterGearComparison(c, gearItem)));
            popupWithList.AddList(characterGear, null, true, true, true);
            popupWithList.AddBackButton();
        }

        public void Start()
        {
            _homeInventory.Start();
            EnvironmentManager.Start();
            Weather.Start();

            SetResourceSuffix(InventoryResourceType.Water, "sips");
            SetResourceSuffix(InventoryResourceType.Food, "meals");
            SetResourceSuffix(InventoryResourceType.Fuel, "dregs");
            SetResourceSuffix(InventoryResourceType.Ammo, "rounds");
            SetResourceSuffix(InventoryResourceType.Scrap, "bits");

            SaveController.LoadSettings();
            SaveController.LoadGame();

            DayEvent += IncrementDaysSpentHere;
            StormDistanceMax = 10;
            StormDistanceActual = 10;

#if UNITY_EDITOR
            _homeInventory.AddTestingResources();
#endif
        }

        public static Button GetInventoryButton() => _inventoryButton;

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

        public static Inventory HomeInventory()
        {
            return _homeInventory;
        }

        private static void SetResourceSuffix(InventoryResourceType name, string convention)
        {
            TextMeshProUGUI resourceText = GameObject.Find(name.ToString()).GetComponent<TextMeshProUGUI>();
            _homeInventory.GetResource(name).AddOnUpdate(f => { resourceText.text = "<sprite name=\"" + name + "\">" + Mathf.Round(f.CurrentValue()) + " " + convention; });
        }

        public static WorldState Instance()
        {
            return _instance ?? (_instance = (WorldState) FindObjectOfType(typeof(WorldState)));
        }

        public static void Pause()
        {
            _isPaused = true;
        }

        public static void UnPause()
        {
            _isPaused = false;
        }

        private void IncrementWorldTime()
        {
            _currentTime += Time.deltaTime;
            if (_currentTime >= MinuteInSeconds)
            {
                _currentTime = _currentTime - MinuteInSeconds;
                IncrementMinutes();
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
        
        private void IncrementMinutes()
        {
            Minutes += 60 / MinutesPerHour;
            MinuteEvent?.Invoke();
            if (Minutes != 60) return;
            Minutes = 0;
            IncrementHours();
        }

        private void IncrementHours()
        {
            ++Hours;
            HourEvent?.Invoke();
            if (Hours == 24)
            {
                IncrementDays();
                Hours = 0;
            }
            if (Hours >= 6 && Hours < 20 && _isNight)
            {
                _isNight = false;
            }
            else if ((Hours < 6 || Hours >= 20) && !_isNight)
            {
                _isNight = true;
            }
        }

        private void IncrementDays()
        {
            ++Days;
            DayEvent?.Invoke();
        }

        public void Update()
        {
            if (!_isPaused)
            {
                IncrementWorldTime();
            }
            WorldCooldownManager?.UpdateCooldowns();
        }


        public static void RegisterMinuteEvent(Action a)
        {
            if (Instance() != null) Instance().MinuteEvent += a;
        }

        public static void UnregisterMinuteEvent(Action a)
        {
            if (Instance() != null) Instance().MinuteEvent -= a;
        }

        public static void RegisterHourEvent(Action a)
        {
            if (Instance() != null) Instance().HourEvent += a;
        }

        public static void UnregisterHourEvent(Action a)
        {
            if (Instance() != null) Instance().HourEvent -= a;
        }

        public static void RegisterDayEvent(Action a)
        {
            if (Instance() != null) Instance().DayEvent += a;
        }

        public static void UnregisterDayEvent(Action a)
        {
            if (Instance() != null) Instance().DayEvent -= a;
        }

        public static void RegisterTravelEvent(Action a)
        {
            if (Instance() != null) Instance().TravelEvent += a;
        }

        public static void UnregisterTravelEvent(Action a)
        {
            if (Instance() != null) Instance().TravelEvent -= a;
        }
    }
}