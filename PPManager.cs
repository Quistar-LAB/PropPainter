using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.UI;
using MoveIt;
using System.Collections.Generic;
using UnityEngine;

namespace PropPainter {
    internal unsafe static class PPManager {
        private const string PAINTERBTN_NAME = @"PAPainterButton";
        private const string COLORFIELD_NAME = @"PAPainterColorField";
        private const string COLORPICKER_NAME = @"PAPaintercolorPicker";
        internal const int DEFAULT_PROP_LIMIT = 65536;

        private static readonly Color[] m_colors = new Color[DEFAULT_PROP_LIMIT];

        internal static Color[] ColorBuffer => m_colors;

        internal static Color GetColor(PropInfo info, ref Randomizer randomizer, ushort propID) {
            fixed (Color* pColor = &m_colors[0]) {
                Color* color = pColor + propID;
                if (color->r == 0 && color->g == 0 && color->b == 0 && color->a == 0) {
                    *color = info.GetColor(ref randomizer);
                }
                return *color;
            }
        }

        internal static void AddPropPainterBtn(UIToolOptionPanel optionPanel, UIButton moreTools, UIPanel mtpBackGround, UIPanel mtpContainer) {
            PropInstance[] props = Singleton<PropManager>.instance.m_props.m_buffer;
            Color[] colors = m_colors;
            UIColorField field = Object.Instantiate(UITemplateManager.Get<UIPanel>("LineTemplate").Find<UIColorField>("LineColor"));
            field.isVisible = true;
            field.name = COLORFIELD_NAME;
            UIColorPicker picker = Object.Instantiate(field.colorPicker);
            picker.eventColorUpdated += (color) => {
                HashSet<Instance> selections = Action.selection;
                if (!(selections is null) && selections.Count > 0) {
                    foreach (var selection in selections) {
                        uint propID = selection.id.Prop;
                        if (selection.isValid && !selection.id.IsEmpty && propID > 0 && propID < 65536) {
                            colors[propID] = color;
                        }
                    }
                }
            };

            optionPanel.AttachUIComponent(picker.gameObject);
            picker.color = Color.white;
            picker.name = COLORPICKER_NAME;

            UIPanel pickerPanel = picker.component as UIPanel;
            pickerPanel.eventVisibilityChanged += (c, isVisible) => {
                if (isVisible) {
                    HashSet<Instance> selections = Action.selection;
                    if (!(selections is null) && selections.Count > 0) {
                        foreach (var selection in selections) {
                            ushort propID = selection.id.Prop;
                            if (selection.isValid && !selection.id.IsEmpty && propID > 0) {
                                Randomizer randomizer = new Randomizer(propID);
                                picker.color = GetColor(props[propID].Info, ref randomizer, propID);
                                break;
                            }
                        }
                    }
                }
            };
            pickerPanel.color = Color.white;
            pickerPanel.backgroundSprite = @"InfoPanelBack";
            pickerPanel.isVisible = false;
            // re-adjust moretools panel
            Vector2 containerSize = mtpContainer.size;
            containerSize.y += 40f;
            optionPanel.m_moreToolsPanel.size = containerSize;
            mtpContainer.size = containerSize;
            Vector2 backgroundSize = mtpBackGround.size;
            backgroundSize.y += 40f;
            mtpBackGround.size = backgroundSize;
            optionPanel.m_moreToolsPanel.absolutePosition = moreTools.absolutePosition + new Vector3(0, 10 - optionPanel.m_moreToolsPanel.height);

            UIMultiStateButton painterBtn = mtpContainer.AddUIComponent<UIMultiStateButton>();
            painterBtn.name = PAINTERBTN_NAME;
            painterBtn.cachedName = PAINTERBTN_NAME;
            painterBtn.tooltip = @"Prop Painter";
            painterBtn.playAudioEvents = true;
            painterBtn.size = new Vector2(36f, 36f);
            painterBtn.atlas = optionPanel.m_picker.atlas;
            painterBtn.spritePadding = new RectOffset(2, 2, 2, 2);
            painterBtn.backgroundSprites.AddState();
            painterBtn.foregroundSprites.AddState();
            painterBtn.backgroundSprites[0].normal = "OptionBase";
            painterBtn.backgroundSprites[0].focused = "OptionBase";
            painterBtn.backgroundSprites[0].hovered = "OptionBaseHovered";
            painterBtn.backgroundSprites[0].pressed = "OptionBasePressed";
            painterBtn.backgroundSprites[0].disabled = "OptionBaseDisabled";
            painterBtn.foregroundSprites[0].normal = "EyeDropper";
            painterBtn.backgroundSprites[1].normal = "OptionBaseFocused";
            painterBtn.backgroundSprites[1].focused = "OptionBaseFocused";
            painterBtn.backgroundSprites[1].hovered = "OptionBaseHovered";
            painterBtn.backgroundSprites[1].pressed = "OptionBasePressed";
            painterBtn.backgroundSprites[1].disabled = "OptionBaseDisabled";
            painterBtn.foregroundSprites[1].normal = "EyeDropper";
            painterBtn.activeStateIndex = 0;
            Vector2 parentSize = painterBtn.parent.size;
            painterBtn.parent.parent.size = new Vector2(parentSize.x, parentSize.y + 40f);
            painterBtn.parent.size = painterBtn.parent.parent.size;
            painterBtn.eventActiveStateIndexChanged += (_, index) => {
                pickerPanel.isVisible = index == 1;
            };
            pickerPanel.absolutePosition = painterBtn.absolutePosition - new Vector3(pickerPanel.width, pickerPanel.height - 50f);
        }
    }
}
