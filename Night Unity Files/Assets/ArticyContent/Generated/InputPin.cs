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
using System.Linq;

namespace Articy.Night
{
    
    
    [Serializable()]
    [Articy.Unity.ArticyCodeGenerationHashAttribute(636391813431813585)]
    public class InputPin : ArticyPrimitive, IInputPin
    {
        
        [SerializeField()]
        private ArticyValueArticyScriptCondition mText = new ArticyValueArticyScriptCondition();
        
        [SerializeField()]
        private ulong mOwner;
        
        [SerializeField()]
        private ArticyValueListOutgoingConnection mConnections = new ArticyValueListOutgoingConnection();
        
        public ArticyScriptCondition Text
        {
            get
            {
                return mText.GetValue();
            }
            set
            {
                mText.SetValue(value);
            }
        }
        
        public ulong Owner
        {
            get
            {
                return mOwner;
            }
            set
            {
                mOwner = value;
            }
        }
        
        public List<OutgoingConnection> Connections
        {
            get
            {
                return mConnections.GetValue();
            }
            set
            {
                mConnections.SetValue(value);
            }
        }
        
        public List<Articy.Unity.Interfaces.IOutgoingConnection> GetOutgoingConnections()
        {
            return Connections.Cast<IOutgoingConnection>().ToList();
        }
        
        public bool Evaluate([System.Runtime.InteropServices.OptionalAttribute()] Articy.Unity.IBaseScriptMethodProvider aMethodProvider, [System.Runtime.InteropServices.OptionalAttribute()] Articy.Unity.Interfaces.IGlobalVariables aGlobalVariables)
        {
            return Text.CallScript(aMethodProvider, aGlobalVariables);
        }
        
        protected void CloneProperties(object aClone)
        {
            InputPin newClone = ((InputPin)(aClone));
            if ((Text != null))
            {
                newClone.Text = ((ArticyScriptCondition)(Text.CloneObject()));
            }
            newClone.Owner = Owner;
            newClone.Connections = new List<OutgoingConnection>();
            int i = 0;
            for (i = 0; (i < Connections.Count); i = (i + 1))
            {
                newClone.Connections.Add(((OutgoingConnection)(Connections[i].CloneObject())));
            }
            newClone.Id = Id;
        }
        
        public override object CloneObject()
        {
            InputPin clone = new InputPin();
            CloneProperties(clone);
            return clone;
        }
        
        public override bool IsLocalizedPropertyOverwritten(string aProperty)
        {
            return base.IsLocalizedPropertyOverwritten(aProperty);
        }
    }
}
