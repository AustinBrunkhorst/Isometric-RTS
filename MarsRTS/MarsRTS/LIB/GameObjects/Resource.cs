namespace MarsRTS.LIB.GameObjects
{
    public class Resource
    {
        int bank, maxBank;

        public int Bank
        {
            get { return bank; }
            set { bank = value; }
        }

        public int MaxBank
        {
            get { return maxBank; }
            set { maxBank = value; }
        }
    }
}
