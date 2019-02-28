using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using Game.Exploration.Environment;
using SamsHelper.BaseGameFunctionality.StateMachines;
using SamsHelper.Libraries;
using Random = UnityEngine.Random;

namespace Game.Exploration.Weather
{
    public static class WeatherManager
    {
        private static readonly StateMachine _weatherStates = new StateMachine();
        private static readonly Dictionary<EnvironmentType, WeatherProbabilities> _regionWeatherProbabilities = new Dictionary<EnvironmentType, WeatherProbabilities>();

        private class WeatherProbabilities
        {
            private readonly List<string> _types;
            private readonly Dictionary<string, List<float>> _probabilities = new Dictionary<string, List<float>>();

            public WeatherProbabilities(XmlNode node)
            {
                string typeString = node.StringFromNode("Types");
                _types = typeString.Split(',').ToList();
                foreach (XmlNode subNode in node.ChildNodes)
                {
                    if (subNode.Name == "Types") continue;
                    List<float> probabilityValues = subNode.InnerText.Split(',').Select(f => float.Parse(f, CultureInfo.InvariantCulture.NumberFormat)).ToList();
                    _probabilities.Add(subNode.Name, probabilityValues);
                }
            }

            public string NextWeather(string currentWeather)
            {
                if (currentWeather == "" ||
                    !_probabilities.ContainsKey(currentWeather)) return _types.RandomElement();
                List<float> pValues = _probabilities[currentWeather];
                float rand = Random.Range(0f, 1f);
                float count = 0;
                for (int i = 0; i < pValues.Count; i++)
                {
                    count += pValues[i];
                    if (count <= rand) continue;
                    return _types[i];
                }

                pValues.ToArray().Print();
                _types.ToArray().Print();
                throw new ArgumentOutOfRangeException();
            }
        }

        private static bool _loaded;

        public static void Start()
        {
            LoadWeather();
        }

        public static void GoToWeather()
        {
            EnvironmentType currentEnvironment = EnvironmentManager.CurrentEnvironmentType();
            string currentWeather = _weatherStates.GetCurrentState()?.Name ?? "";
            string nextWeatherName = _regionWeatherProbabilities[currentEnvironment].NextWeather(currentWeather);
            _weatherStates.GetState(nextWeatherName).Enter();
        }

        public static Weather CurrentWeather()
        {
            return (Weather) _weatherStates.GetCurrentState();
        }

        private static void LoadWeather()
        {
            if (_loaded) return;
            XmlNode root = Helper.OpenRootNode("Weather", "WeatherTypes");
            foreach (XmlNode weatherNode in root.GetNodesWithName("Weather"))
                new Weather(_weatherStates, weatherNode);

            root = Helper.OpenRootNode("WeatherProbabilities", "Regions");
            Array environmentTypes = Enum.GetValues(typeof(EnvironmentType));

            foreach (XmlNode regionNode in root.ChildNodes)
            {
                foreach (EnvironmentType environmentType in environmentTypes)
                {
                    if (environmentType.ToString() != regionNode.Name) continue;
                    WeatherProbabilities probabilities = new WeatherProbabilities(regionNode);
                    _regionWeatherProbabilities.Add(environmentType, probabilities);
                    break;
                }
            }

//            GenerateWeatherString(1000);
            _loaded = true;
        }

        private static void GenerateWeatherString(int stringLength)
        {
            float max = stringLength;
            for (int i = 0; i < 5; ++i)
            {
                stringLength = (int) max;
                GoToWeather();
                Dictionary<string, int> _weatherOccurrences = new Dictionary<string, int>();
                string weatherString = "";
                while (stringLength > 0)
                {
                    string currentWeatherName = CurrentWeather().Name;
                    if (_weatherOccurrences.ContainsKey(currentWeatherName))
                        _weatherOccurrences[currentWeatherName]++;
                    else
                        _weatherOccurrences[currentWeatherName] = 1;
                    weatherString += currentWeatherName;
                    GoToWeather();
                    --stringLength;
                }

                foreach (string key in _weatherOccurrences.Keys)
                {
                    int count = _weatherOccurrences[key];
                    float proportion = count / max;
                    proportion *= 100;
                    proportion = Helper.Round(proportion);
                    string occurences = key + " occured " + _weatherOccurrences[key] + " times. -- (" + proportion + "%)";
                    weatherString = occurences + "\n" + weatherString;
                }

                EnvironmentManager.NextLevel(false, false);
            }
        }

        public static void Reset()
        {
            LoadWeather();
            _weatherStates.GetState("Clear").Enter();
        }

        public static void Save(XmlNode doc)
        {
            CurrentWeather().Save(doc);
        }

        public static void Load(XmlNode doc)
        {
            XmlNode weatherNode = doc.SelectSingleNode("Weather");
            string weatherName = weatherNode.StringFromNode("Name");
            Weather weather = (Weather) _weatherStates.StatesAsList().First(w => w.Name == weatherName);
            _weatherStates.SetCurrentState(weather);
            weather.SetTimeRemaining(weatherNode.IntFromNode("TimeRemaining"));
        }
    }
}