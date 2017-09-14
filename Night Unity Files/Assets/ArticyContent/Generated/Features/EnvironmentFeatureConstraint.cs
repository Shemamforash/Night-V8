// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      Mono Runtime Version: 2.0.50727.1433
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------

using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using Articy.Unity.Interfaces;
using Articy.Unity;
using Articy.Unity.Constraints;
using Articy.Night;

namespace Articy.Night.Features
{
    
    
    public class EnvironmentFeatureConstraint
    {
        
        private bool mLoadedConstraints;
        
        private NumberConstraint mMinTemperature;
        
        private NumberConstraint mMaxTemperature;
        
        private NumberConstraint mWaterLevel;
        
        private NumberConstraint mFoodLevel;
        
        private NumberConstraint mFuelLevel;
        
        private NumberConstraint mScrapLevel;
        
        private NumberConstraint mBanditDanger;
        
        private NumberConstraint mTerrainAccessibility;
        
        private NumberConstraint mCampFrequency;
        
        private NumberConstraint mNormalisedDanger;
        
        public NumberConstraint MinTemperature
        {
            get
            {
                EnsureConstraints();
                return mMinTemperature;
            }
        }
        
        public NumberConstraint MaxTemperature
        {
            get
            {
                EnsureConstraints();
                return mMaxTemperature;
            }
        }
        
        public NumberConstraint WaterLevel
        {
            get
            {
                EnsureConstraints();
                return mWaterLevel;
            }
        }
        
        public NumberConstraint FoodLevel
        {
            get
            {
                EnsureConstraints();
                return mFoodLevel;
            }
        }
        
        public NumberConstraint FuelLevel
        {
            get
            {
                EnsureConstraints();
                return mFuelLevel;
            }
        }
        
        public NumberConstraint ScrapLevel
        {
            get
            {
                EnsureConstraints();
                return mScrapLevel;
            }
        }
        
        public NumberConstraint BanditDanger
        {
            get
            {
                EnsureConstraints();
                return mBanditDanger;
            }
        }
        
        public NumberConstraint TerrainAccessibility
        {
            get
            {
                EnsureConstraints();
                return mTerrainAccessibility;
            }
        }
        
        public NumberConstraint CampFrequency
        {
            get
            {
                EnsureConstraints();
                return mCampFrequency;
            }
        }
        
        public NumberConstraint NormalisedDanger
        {
            get
            {
                EnsureConstraints();
                return mNormalisedDanger;
            }
        }
        
        public virtual void EnsureConstraints()
        {
            if ((mLoadedConstraints == true))
            {
                return;
            }
            mLoadedConstraints = true;
            mMinTemperature = new Articy.Unity.Constraints.NumberConstraint(-3.40282346638529E+38, 3.40282346638529E+38, 0, 0, 0, null);
            mMaxTemperature = new Articy.Unity.Constraints.NumberConstraint(-3.40282346638529E+38, 3.40282346638529E+38, 0, 0, 0, null);
            mWaterLevel = new Articy.Unity.Constraints.NumberConstraint(0, 10, 0, 0, 0, null);
            mFoodLevel = new Articy.Unity.Constraints.NumberConstraint(0, 10, 0, 0, 0, null);
            mFuelLevel = new Articy.Unity.Constraints.NumberConstraint(0, 10, 0, 0, 0, null);
            mScrapLevel = new Articy.Unity.Constraints.NumberConstraint(0, 10, 0, 0, 0, null);
            mBanditDanger = new Articy.Unity.Constraints.NumberConstraint(0, 10, 0, 0, 0, null);
            mTerrainAccessibility = new Articy.Unity.Constraints.NumberConstraint(0, 10, 0, 0, 0, null);
            mCampFrequency = new Articy.Unity.Constraints.NumberConstraint(0, 1, 0, 0, 0, null);
            mNormalisedDanger = new Articy.Unity.Constraints.NumberConstraint(0, 8, 0, 0, 0, null);
        }
    }
}
