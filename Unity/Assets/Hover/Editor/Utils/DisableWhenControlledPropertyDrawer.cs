﻿using System.Collections.Generic;
using Hover.Utils;
using UnityEditor;
using UnityEngine;

namespace Hover.Editor.Utils {

	/*================================================================================================*/
	[CustomPropertyDrawer(typeof(DisableWhenControlledAttribute))]
	public class DisableWhenControlledPropertyDrawer : PropertyDrawer {

		private const string IconTextPrefix = " _  ";
		private static readonly Texture2D ControlIconTex = 
			Resources.Load<Texture2D>("Textures/ControlledPropertyIconTexture");
		

		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		public override void OnGUI(Rect pPosition, SerializedProperty pProp, GUIContent pLabel) {
			DisableWhenControlledAttribute attrib = (DisableWhenControlledAttribute)attribute;
			string mapName = attrib.ControllerMapName;
			ISettingsControllerMap map = EditorUtil.GetControllerMap(pProp.serializedObject, mapName);
			bool wasEnabled = GUI.enabled;
			Rect propRect = pPosition;
			bool hasRangeMin = (attrib.RangeMin != DisableWhenControlledAttribute.NullRangeMin);
			bool hasRangeMax = (attrib.RangeMax != DisableWhenControlledAttribute.NullRangeMax);
			bool isControlled = (map != null && map.IsControlled(pProp.name));
			string labelText = pLabel.text;

			//TODO: show displays for non-properties like "gameObject.activeSelf"
			
			if ( map != null && attrib.DisplayMessage ) {
				List<string> specialValueNames = map.GetNewListOfControlledValueNames(true);
				Rect specialRect = propRect;
				specialRect.height = EditorGUIUtility.singleLineHeight;

				foreach ( string specialValueName in specialValueNames ) {
					DrawLinkIcon(map.Get(specialValueName), specialRect);
					GUI.enabled = false;
					EditorGUI.LabelField(specialRect, IconTextPrefix+specialValueName.Substring(1));
					GUI.enabled = wasEnabled;
					specialRect.y += specialRect.height+EditorGUIUtility.standardVerticalSpacing;
				}

				propRect.y = specialRect.y;
				propRect.height = specialRect.height;
			}

			if ( isControlled ) {
				ISettingsController settingsController = map.Get(pProp.name);
				DrawLinkIcon(settingsController, propRect);
				pLabel.text = IconTextPrefix+labelText;
			}

			GUI.enabled = !isControlled;

			if ( hasRangeMin && hasRangeMax ) {
				EditorGUI.Slider(propRect, pProp, attrib.RangeMin, attrib.RangeMax, pLabel);
			}
			else {
				EditorGUI.PropertyField(propRect, pProp, pLabel, true);

				if ( hasRangeMin ) {
					pProp.floatValue = Mathf.Max(pProp.floatValue, attrib.RangeMin);
				}
				else if ( hasRangeMax ) {
					pProp.floatValue = Mathf.Min(pProp.floatValue, attrib.RangeMax);
				}
			}

			GUI.enabled = wasEnabled;
		}

		/*--------------------------------------------------------------------------------------------*/
		public override float GetPropertyHeight(SerializedProperty pProp, GUIContent pLabel) {
			DisableWhenControlledAttribute attrib = (DisableWhenControlledAttribute)attribute;
			string mapName = attrib.ControllerMapName;
			ISettingsControllerMap map = EditorUtil.GetControllerMap(pProp.serializedObject, mapName);
			float propHeight = base.GetPropertyHeight(pProp, pLabel);

			if ( map == null || !attrib.DisplayMessage ) {
				return propHeight;
			}

			float lineH = EditorGUIUtility.singleLineHeight+EditorGUIUtility.standardVerticalSpacing;
			return lineH*map.GetControlledCount(true) + propHeight;
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		private void DrawLinkIcon(ISettingsController pControl, Rect pPropertyRect) {
			Rect iconRect = pPropertyRect;
			iconRect.x -= 26;
			iconRect.y += 1;
			iconRect.width = 40;
			iconRect.height = 40;

			GUIContent labelContent = new GUIContent();
			labelContent.image = ControlIconTex;
			labelContent.tooltip = "Controlled by '"+pControl.GetType().Name+"' in "+
				"'"+pControl.name+"'";

			GUIStyle labelStyle = new GUIStyle();
			labelStyle.imagePosition = ImagePosition.ImageOnly;
			labelStyle.clipping = TextClipping.Clip;
			labelStyle.padding = new RectOffset(15, 0, 0, 0);
			labelStyle.stretchWidth = true;
			labelStyle.stretchHeight = true;

			bool shouldPing = EditorGUI.ToggleLeft(iconRect, labelContent, false, labelStyle);

			if ( shouldPing ) {
				EditorGUIUtility.PingObject((Object)pControl);
			}
		}
		
	}

}
