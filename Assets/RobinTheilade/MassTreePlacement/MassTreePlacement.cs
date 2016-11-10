using UnityEngine;

namespace RobinTheilade.MassTreePlacement
{
    /// <summary>
    /// Values for placing random trees.
    /// <see cref="T:RobinTheilade.MassTreePlacement.MassTreePlacementEditor"/> does the work.
    /// </summary>
    [AddComponentMenu("Terrain/Mass Tree Placement")]
    [RequireComponent(typeof(Terrain))]
    public class MassTreePlacement : MonoBehaviour
    {
        /// <summary>
        /// The number of trees to place.
        /// </summary>
        [Tooltip("The number of trees to place.")]
        public int count = 70000;

        /// <summary>
        /// The lowest point to position a tree.
        /// </summary>
        [Tooltip("The lowest point to position a tree.")]
        public float minWorldY = 1.0f;

        /// <summary>
        /// The highest point to position a tree.
        /// </summary>
        [Tooltip("The highest point to position a tree.")]
        public float maxWorldY = 5200.0f;

        /// <summary>
        /// The minimum allowed slope of the ground to position a tree.
        /// </summary>
        [Tooltip("The minimum allowed slope of the ground to position a tree.")]
        public float minSlope = 0.0f;

        /// <summary>
        /// The maximum allowed slope of the ground to position a tree.
        /// </summary>
        [Tooltip("The maximum allowed slope of the ground to position a tree.")]
        public float maxSlope = 40.0f;

        /// <summary>
        /// The minimum value to scale the width of a tree.
        /// </summary>
        [Tooltip("The minimum value to scale the width of a tree.")]
        public float minWidthScale = 0.9f;

        /// <summary>
        /// The maximum value to scale the width of a tree.
        /// </summary>
        [Tooltip("The maximum value to scale the width of a tree.")]
        public float maxWidthScale = 2.0f;

        /// <summary>
        /// The minimum value to scale the height of a tree.
        /// </summary>
        [Tooltip("The minimum value to scale the height of a tree.")]
        public float minHeightScale = 0.8f;

        /// <summary>
        /// The maximum value to scale the height of a tree.
        /// </summary>
        [Tooltip("The maximum value to scale the height of a tree.")]
        public float maxHeightScale = 3.5f;

        /// <summary>
        /// The maximum number of seconds for the placement process to take.
        /// The process is aborted if it takes any longer.
        /// </summary>
        [Tooltip("The maximum number of seconds for the placement process to take. The process is aborted if it takes any longer.")]
        public double maxTime = 30.0d;


		public void GenerateTrees(Terrain terrain){
			var data = terrain.terrainData;

			var num = data.treePrototypes.Length;
			if (num == 0) {
				Debug.LogWarning ("Can't place trees because no prototypes are defined. Process aborted.");
				return;
			}

			//Undo.RegisterCompleteObjectUndo (data, "Mass Place Trees");

			float timeElapsed = 0;

			var array = new TreeInstance[count];
			var i = 0;
			while (i < array.Length) {
				//stop if process have run for over X seconds
				timeElapsed += Time.deltaTime;
				if (timeElapsed >= maxTime) {
					Debug.LogWarning ("Process was taking too much time to run");
					return;
				}

				var position = new Vector3 (Random.value, 0.0f, Random.value);

				// don't allow placement of trees below minWorldY and above maxWorldY
				var y = data.GetInterpolatedHeight (position.x, position.z);
				var worldY = y + terrain.transform.position.y;
				if (worldY < minWorldY || worldY > maxWorldY) {
					continue;
				}

				// don't allow placement of trees on surfaces flatter than minSlope and steeper than maxSlope
				var steepness = data.GetSteepness (position.x, position.z);
				if (steepness < minSlope || steepness > maxSlope) {
					continue;
				}

				var color = Color.Lerp (Color.white, Color.gray * 0.7f, Random.value);
				color.a = 1f;

				var treeInstance = default(TreeInstance);
				treeInstance.position = position;
				treeInstance.color = color;
				treeInstance.lightmapColor = Color.white;
				treeInstance.prototypeIndex = Random.Range (0, num);
				treeInstance.widthScale = Random.Range (minWidthScale, maxWidthScale);
				treeInstance.heightScale = Random.Range (minHeightScale, maxHeightScale);
				array [i] = treeInstance;
				i++;
			}
			data.treeInstances = array;
			//RecalculateTreePositions(data);
			terrain.Flush ();
		}
    }
}