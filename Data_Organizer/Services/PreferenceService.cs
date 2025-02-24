﻿using Data_Organizer.Interfaces;

namespace Data_Organizer.Services
{
    public class PreferenceService : IPreferenceService
    {
        public string GetPreference<T>(AppEnums.Preferences key, T? defaultValue = default)
        {
            var strKey = key.ToString();
            var strDefaultValue = defaultValue?.ToString();

            var preferenceValue = Preferences.Get(strKey, strDefaultValue);

            return preferenceValue;
        }

        public void SetPreference<T>(AppEnums.Preferences key, T value)
        {
            var strKey = key.ToString();
            var strValue = value.ToString();

            Preferences.Set(strKey, strValue);
        }
    }
}
