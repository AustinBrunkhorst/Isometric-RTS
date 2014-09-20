using RTSAssetData.Buildings;

namespace RTSAssetData
{
    /// <summary>
    /// Represents the cost in resources of any entity
    /// </summary>
    public struct EntityCost
    {
        int copper, nickel, iron;

        public int Copper
        {
            get { return copper; }
            set { copper = value; }
        }

        public int Nickel
        {
            get { return nickel; }
            set { nickel = value; }
        }

        public int Iron
        {
            get { return iron; }
            set { iron = value; }
        }

        public int FromType(ResourceType resource)
        {
            switch (resource)
            {
                case ResourceType.Copper:
                    return copper;
                case ResourceType.Iron:
                    return iron;
                case ResourceType.Nickel:
                    return nickel;
            }

            return 0;
        }
    }
}
