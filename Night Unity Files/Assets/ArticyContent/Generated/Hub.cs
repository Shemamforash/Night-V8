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
    
    
    public class Hub : ArticyObject, IHub, IPropertyProvider, IInputPinsOwner, IOutputPinsOwner, IObjectWithColor, IObjectWithDisplayName, IObjectWithText, IObjectWithExternalId, IObjectWithShortId, IObjectWithPosition, IObjectWithZIndex, IObjectWithSize
    {
        
        [SerializeField()]
        private string mDisplayName;
        
        [SerializeField()]
        private Color mColor;
        
        [SerializeField()]
        private string mText;
        
        [SerializeField()]
        private string mExternalId;
        
        [SerializeField()]
        private Vector2 mPosition;
        
        [SerializeField()]
        private float mZIndex;
        
        [SerializeField()]
        private Vector2 mSize;
        
        [SerializeField()]
        private uint mShortId;
        
        [SerializeField()]
        private ArticyValueListInputPin mInputPins = new ArticyValueListInputPin();
        
        [SerializeField()]
        private ArticyValueListOutputPin mOutputPins = new ArticyValueListOutputPin();
        
        public string DisplayName
        {
            get
            {
                return mDisplayName;
            }
            set
            {
                mDisplayName = value;
            }
        }
        
        public Color Color
        {
            get
            {
                return mColor;
            }
            set
            {
                mColor = value;
            }
        }
        
        public string Text
        {
            get
            {
                return mText;
            }
            set
            {
                mText = value;
            }
        }
        
        public string ExternalId
        {
            get
            {
                return mExternalId;
            }
            set
            {
                mExternalId = value;
            }
        }
        
        public Vector2 Position
        {
            get
            {
                return mPosition;
            }
            set
            {
                mPosition = value;
            }
        }
        
        public float ZIndex
        {
            get
            {
                return mZIndex;
            }
            set
            {
                mZIndex = value;
            }
        }
        
        public Vector2 Size
        {
            get
            {
                return mSize;
            }
            set
            {
                mSize = value;
            }
        }
        
        public uint ShortId
        {
            get
            {
                return mShortId;
            }
            set
            {
                mShortId = value;
            }
        }
        
        public List<InputPin> InputPins
        {
            get
            {
                return mInputPins.GetValue();
            }
            set
            {
                mInputPins.SetValue(value);
            }
        }
        
        public List<OutputPin> OutputPins
        {
            get
            {
                return mOutputPins.GetValue();
            }
            set
            {
                mOutputPins.SetValue(value);
            }
        }
        
        public List<Articy.Unity.Interfaces.IInputPin> GetInputPins()
        {
            return InputPins.Cast<IInputPin>().ToList();
        }
        
        public List<Articy.Unity.Interfaces.IOutputPin> GetOutputPins()
        {
            return OutputPins.Cast<IOutputPin>().ToList();
        }
        
        protected override void CloneProperties(object aClone)
        {
            Hub newClone = ((Hub)(aClone));
            newClone.DisplayName = DisplayName;
            newClone.Color = Color;
            newClone.Text = Text;
            newClone.ExternalId = ExternalId;
            newClone.Position = Position;
            newClone.ZIndex = ZIndex;
            newClone.Size = Size;
            newClone.ShortId = ShortId;
            newClone.InputPins = new List<InputPin>();
            int i = 0;
            for (i = 0; (i < InputPins.Count); i = (i + 1))
            {
                newClone.InputPins.Add(((InputPin)(InputPins[i].CloneObject())));
            }
            newClone.OutputPins = new List<OutputPin>();
            for (i = 0; (i < OutputPins.Count); i = (i + 1))
            {
                newClone.OutputPins.Add(((OutputPin)(OutputPins[i].CloneObject())));
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
            if ((aProperty == "DisplayName"))
            {
                DisplayName = System.Convert.ToString(aValue);
                return;
            }
            if ((aProperty == "Color"))
            {
                Color = ((Color)(aValue));
                return;
            }
            if ((aProperty == "Text"))
            {
                Text = System.Convert.ToString(aValue);
                return;
            }
            if ((aProperty == "ExternalId"))
            {
                ExternalId = System.Convert.ToString(aValue);
                return;
            }
            if ((aProperty == "Position"))
            {
                Position = ((Vector2)(aValue));
                return;
            }
            if ((aProperty == "ZIndex"))
            {
                ZIndex = System.Convert.ToSingle(aValue);
                return;
            }
            if ((aProperty == "Size"))
            {
                Size = ((Vector2)(aValue));
                return;
            }
            if ((aProperty == "ShortId"))
            {
                ShortId = ((uint)(aValue));
                return;
            }
            if ((aProperty == "InputPins"))
            {
                InputPins = ((List<InputPin>)(aValue));
                return;
            }
            if ((aProperty == "OutputPins"))
            {
                OutputPins = ((List<OutputPin>)(aValue));
                return;
            }
            base.setProp(aProperty, aValue);
        }
        
        public override Articy.Unity.Interfaces.ScriptDataProxy getProp(string aProperty)
        {
            if ((aProperty == "DisplayName"))
            {
                return new Articy.Unity.Interfaces.ScriptDataProxy(DisplayName);
            }
            if ((aProperty == "Color"))
            {
                return new Articy.Unity.Interfaces.ScriptDataProxy(Color);
            }
            if ((aProperty == "Text"))
            {
                return new Articy.Unity.Interfaces.ScriptDataProxy(Text);
            }
            if ((aProperty == "ExternalId"))
            {
                return new Articy.Unity.Interfaces.ScriptDataProxy(ExternalId);
            }
            if ((aProperty == "Position"))
            {
                return new Articy.Unity.Interfaces.ScriptDataProxy(Position);
            }
            if ((aProperty == "ZIndex"))
            {
                return new Articy.Unity.Interfaces.ScriptDataProxy(ZIndex);
            }
            if ((aProperty == "Size"))
            {
                return new Articy.Unity.Interfaces.ScriptDataProxy(Size);
            }
            if ((aProperty == "ShortId"))
            {
                return new Articy.Unity.Interfaces.ScriptDataProxy(ShortId);
            }
            if ((aProperty == "InputPins"))
            {
                return new Articy.Unity.Interfaces.ScriptDataProxy(InputPins);
            }
            if ((aProperty == "OutputPins"))
            {
                return new Articy.Unity.Interfaces.ScriptDataProxy(OutputPins);
            }
            return base.getProp(aProperty);
        }
        #endregion
    }
}
