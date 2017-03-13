namespace Game
{
    public static class MainVariables 
    {
        static bool oneActiveMortire;

        public static bool isOneActiveMortirTurrelOnTheField()
        {
            return oneActiveMortire;
        }

        public static void setOneActiveMortirTurrelOnTheField(bool _newBool)
        {
            oneActiveMortire = _newBool;
        }
    }
}
