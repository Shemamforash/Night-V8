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
using Articy.Unity;
using Articy.Unity.Interfaces;
using Articy.Night.Features;

namespace Articy.Night
{
    
    
    public class EnvironmentTemplate : Entity, IEntity, IPropertyProvider, IObjectWithFeatureJournalEntry, IObjectWithFeatureEnvironment
    {
        
        [SerializeField()]
        private ArticyValueEnvironmentTemplateTemplate mTemplate = new ArticyValueEnvironmentTemplateTemplate();
        
        private static Articy.Night.Templates.EnvironmentTemplateTemplateConstraint mConstraints = new Articy.Night.Templates.EnvironmentTemplateTemplateConstraint();
        
        public Articy.Night.Templates.EnvironmentTemplateTemplate Template
        {
            get
            {
                return mTemplate.GetValue();
            }
            set
            {
                mTemplate.SetValue(value);
            }
        }
        
        public static Articy.Night.Templates.EnvironmentTemplateTemplateConstraint Constraints
        {
            get
            {
                return mConstraints;
            }
        }
        
        public JournalEntryFeature GetFeatureJournalEntry()
        {
            return Template.JournalEntry;
        }
        
        public EnvironmentFeature GetFeatureEnvironment()
        {
            return Template.Environment;
        }
        
        protected override void CloneProperties(object aClone)
        {
            EnvironmentTemplate newClone = ((EnvironmentTemplate)(aClone));
            if ((Template != null))
            {
                newClone.Template = ((Articy.Night.Templates.EnvironmentTemplateTemplate)(Template.CloneObject()));
            }
            base.CloneProperties(newClone);
        }
        
        public override bool IsLocalizedPropertyOverwritten(string aProperty)
        {
            return base.IsLocalizedPropertyOverwritten(aProperty);
        }
        
        #region property provider interface
        public override void setProp(string aProperty, object aValue)
        {
            if (aProperty.Contains("."))
            {
                Template.setProp(aProperty, aValue);
                return;
            }
            base.setProp(aProperty, aValue);
        }
        
        public override Articy.Unity.Interfaces.ScriptDataProxy getProp(string aProperty)
        {
            if (aProperty.Contains("."))
            {
                return Template.getProp(aProperty);
            }
            return base.getProp(aProperty);
        }
        #endregion
    }
}
