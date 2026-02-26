using LabApi.Features.Wrappers;
using RueI.API;
using RueI.API.Elements;
using UnityEngine;

namespace MEROptimizer.Application.Components
{
    public class PlayerDisplayHint : MonoBehaviour
    {

        public Player player;

        private float timePassed = 0;

        public void RemoveComponent()
        {
            Destroy(this);
        }
        public void Update()
        {
            timePassed += Time.deltaTime; // mrc serious
            if (timePassed > .3f)
            {
                timePassed = 0;


                int count = 0;
                int totalPrimitiveCount = 0;

                foreach (OptimizedSchematic schematic in MEROptimizer.Instance.optimizedSchematics)
                {
                    count += schematic.nonClusteredPrimitives.Count;
                    totalPrimitiveCount += (schematic.schematicServerSidePrimitiveCount + schematic.nonClusteredPrimitives.Count);
                    foreach (PrimitiveCluster cluster in schematic.primitiveClusters)
                    {
                        if (cluster.insidePlayers.Contains(player))
                        {
                            count += cluster.primitives.Count;
                        }

                        totalPrimitiveCount += cluster.primitives.Count;
                    }
                }

                if (player == null)
                {
                    Destroy(this);
                    return;
                }

                RueDisplay.Get(player).Show(
                    new Tag("Optimizer Prim Count"),
                    new BasicElement(300, $"Loaded <color=green>{count}</color> out of a total of <color=red>{totalPrimitiveCount}</color> primitives"), 
                    .5f);
                //player.SendHint($"Loaded <color=green>{count}</color> out of a total of <color=red>{totalPrimitiveCount}</color> primitives", .5f);

            }
        }
    }
}