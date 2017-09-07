namespace SamsHelper.ReactiveUI.CustomTypes
{
    public class MyString : MyValue<string>
    {
        
        public MyString() : this("")
        {
        }
        
        public MyString(string initialText) : base(initialText)
        {
        }

        public string Text
        {
            set
            {
                _currentValue = value;
                BroadcastChange();
            }
            get { return _currentValue; }
        }
    }
}