using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.Tilemaps {
    [Serializable]
	[CreateAssetMenu]
    public class GameObjectTile : Tile {
        [SerializeField]
        public GameObject spawnedObject;

        [SerializeField]
        public bool disableCollider = false;

		[SerializeField]
		new public Sprite sprite;

        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData) {
            base.GetTileData(position, tilemap, ref tileData);
            tileData.gameObject = spawnedObject;
			tileData.sprite = sprite;
			tileData.transform = transform;
            tileData.flags = TileFlags.None;
            if (disableCollider) tileData.colliderType = ColliderType.None;
		}

        public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject instantiatedGameObject) {
            if (instantiatedGameObject == null) {
                return false;
            }
            instantiatedGameObject.transform.localRotation = tilemap.GetTransformMatrix(position).rotation;
            return true;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(GameObjectTile))]
    public class GameObjectTileEditor : Editor {
        private SerializedProperty spawnedObject;
		private SerializedProperty sprite;
        private SerializedProperty disableCollider;

        private GameObjectTile tile { get { return (target as GameObjectTile); } }

        public void OnEnable() {
            spawnedObject = serializedObject.FindProperty("spawnedObject");
			sprite = serializedObject.FindProperty("sprite");
			disableCollider = serializedObject.FindProperty("disableCollider");
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();

			EditorGUILayout.PropertyField(sprite);
            EditorGUILayout.PropertyField(spawnedObject);
            EditorGUILayout.PropertyField(disableCollider);

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(tile);
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
#endif
}
