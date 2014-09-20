using RTSAssetData;
using RTSAssetData.Buildings;
using System;
using System.Collections.Generic;

namespace MarsRTS.LIB.GameObjects
{
    public class ResourceBank
    {
        Dictionary<ResourceType, Resource> resources =
            new Dictionary<ResourceType, Resource>();

        public Dictionary<ResourceType, Resource> Resources
        {
            get { return resources; }
        }

        public ResourceBank()
        {
            var types = Enum.GetValues(typeof(ResourceType));

            foreach (var resource in types)
            {
                resources[(ResourceType)resource] = new Resource();
            }
        }

        public int GetTotal(ResourceType resource)
        {
            return resources[resource].Bank;
        }

        public int GetMax(ResourceType resource)
        {
            return resources[resource].MaxBank;
        }

        public void AddTo(ResourceType resource, int amount)
        {
            var type = resources[resource];

            type.Bank = Math.Min(type.MaxBank, type.Bank + amount);
        }

        public void SetTotal(ResourceType resource, int total)
        {
            resources[resource].Bank = total;
        }

        public void SetMax(ResourceType resource, int max)
        {
            resources[resource].MaxBank = max;
        }

        public void SpendFrom(ResourceType resource, int amount)
        {
            var type = resources[resource];

            type.Bank = Math.Min(0, type.Bank - amount);
        }

        public void Spend(EntityCost cost)
        {
            foreach (var resource in resources)
            {
                resource.Value.Bank = Math.Max(0,
                    resource.Value.Bank - cost.FromType(resource.Key));
            }
        }
    }
}
