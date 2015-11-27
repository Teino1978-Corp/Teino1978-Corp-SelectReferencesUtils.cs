using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityObject = UnityEngine.Object;

public static class SelectReferencesUtils {
	[MenuItem("Assets/Select Intances On Scene", false, 30)]
	static void SelectInstances() {
		Selection.objects = GetReferencesOfAsset(Selection.objects);
	}
	
	[MenuItem("Assets/Select References On Scene", false, 30)]
	static void SelectReferences() {
		Selection.objects = GetReferencesOfAsset(Selection.objects, false);
	}
	
	public static UnityObject[] GetReferencesOfAsset(UnityObject asset, bool prefabOnly = true) {
		return GetReferencesOfAsset(new [] { asset }, prefabOnly);
	}
	
	public static UnityObject[] GetReferencesOfAsset(UnityObject[] assets, bool prefabOnly = true) {
		if(assets == null || assets.Length == 0)
			return new GameObject[0];
		var result = new List<UnityObject>();
		var allObjects = UnityObject.FindObjectsOfType(typeof(UnityObject)) as UnityObject[];
		var allGameObjects = !prefabOnly ? GetChildrenWithInactive(allObjects) : new GameObject[0];
		foreach(var target in assets) {
			if(target == null)
				continue;
			if(prefabOnly) {
				foreach(var obj in allObjects)
					if(PrefabUtility.GetPrefabType(obj) == PrefabType.PrefabInstance && PrefabUtility.GetPrefabParent(obj) == target)
						result.Add(obj);
				continue;
			}
			foreach(var gameObject in allGameObjects) {
				var added = false;
				if(result.Contains(gameObject))
					continue;
				if(PrefabUtility.GetPrefabType(gameObject) == PrefabType.PrefabInstance && PrefabUtility.GetPrefabParent(gameObject) == target)
					result.Add(gameObject);
				foreach(var component in gameObject.GetComponents<Component>()) {
					if(target == component) {
						result.Add(gameObject);
						break;
					}
					var property = new SerializedObject(component).GetIterator();
					while(property != null && property.Next(true))
						if(property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue == target) {
							result.Add(gameObject);
							added = true;
							break;
						}
					if(added)
						break;
				}
			}
		}
		return result.ToArray();
	}
	
	public static GameObject[] GetChildrenWithInactive(UnityObject parent) {
		return GetChildrenWithInactive(new [] { parent });
	}
	
	public static GameObject[] GetChildrenWithInactive(IEnumerable<UnityObject> parents) {
		if(parents == null)
			return new GameObject[0];
		var result = new List<GameObject>();
		var checkList = new List<UnityObject>(parents);
		while(checkList.Count > 0) {
			var uncheckedList = new List<UnityObject>();
			foreach(var item in checkList) {
				var parentGameObject = item as GameObject;
				if(parentGameObject == null)
					continue;
				if(!result.Contains(parentGameObject))
					result.Add(parentGameObject);
				var childrenComponents = parentGameObject.GetComponentsInChildren(typeof(Component), true);
				foreach(var component in childrenComponents) {
					var childGameObject = component.gameObject;
					if(childGameObject != null && !result.Contains(childGameObject)) {
						result.Add(childGameObject);
						uncheckedList.Add(childGameObject);
					}
				}
			}
			checkList = uncheckedList;
		}
		return result.ToArray();
	}
}