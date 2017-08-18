using Characters;
using SamsHelper;

namespace Game.Characters
{
    public class CharacterGenerationTester
    {
        public static void Test()
        {
            for (int i = 0; i < 10000; ++i)
            {
                
                try
                {
                    Character c = CharacterGenerator.GenerateCharacter();
                }
                catch (Exceptions.CappedValueExceededBoundsException e)
                {
                    
                }
            }
        }
    }
}