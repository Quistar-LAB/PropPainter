using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.UI;
using MoveIt;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections;

namespace PropPainter {
    internal unsafe static class PPManager {
        private const string PAINTERBTN_NAME = @"PAPainterButton";
        private const string COLORFIELD_NAME = @"PAPainterColorField";
        private const string COLORPICKER_NAME = @"PAPaintercolorPicker";
        internal const int DEFAULT_PROP_LIMIT = 65536;
        internal const float FALSEALPHASIG = 1f / 255f;
        internal delegate void ActionHandler(HashSet<Instance> selection);
        internal delegate void CloneHandler(Dictionary<Instance, Instance> clonedOrigin);
        private static System.Action<UIColorPicker, Color> SetPickerColorField;
        private static readonly Color[] m_colors = new Color[DEFAULT_PROP_LIMIT];
        internal static ActionHandler ActionAddHandler;
        internal static CloneHandler ActionCloneHandler;

        internal static Color[] ColorBuffer => m_colors;

        internal static void Initialize() {
            FieldInfo field = typeof(UIColorPicker).GetField("m_Color", BindingFlags.Instance | BindingFlags.NonPublic);
            string methodName = field.ReflectedType.FullName + ".set_" + field.Name;
            DynamicMethod setterMethod = new DynamicMethod(methodName, null, new System.Type[2] { typeof(UIColorPicker), typeof(Color) }, true);
            ILGenerator gen = setterMethod.GetILGenerator();
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Stfld, field);
            gen.Emit(OpCodes.Ret);
            SetPickerColorField = (System.Action<UIColorPicker, Color>)setterMethod.CreateDelegate(typeof(System.Action<UIColorPicker, Color>));
        }

        internal static Color GetPropColor(PropInfo info, ref Randomizer randomizer, ushort propID) {
            fixed (Color* pColor = &m_colors[0]) {
                Color* color = pColor + propID;
                if (color->r == 0f && color->g == 0f && color->b == 0f && color->a == FALSEALPHASIG) {
                    *color = info.GetColor(ref randomizer);
                }
                return *color;
            }
        }

        private static Color GetColor(float x, float y, float width, float height, Color hue) {
            float num = x / width;
            float num2 = y / height;
            num = num < 0f ? 0f : (num > 1f ? 1f : num);
            num2 = num2 < 0f ? 0f : (num2 > 1f ? 1f : num2);
            Color result = Color.Lerp(Color.white, hue, num) * (1f - num2);
            result.a = 1f;
            return result;
        }

        private static void SetPickerColor(UIColorPicker picker, Color color) {
            UISprite indicator = picker.m_Indicator;
            UITextureSprite HSBField = picker.m_HSBField;
            picker.hue = HSBColor.GetHue(color);
            SetPickerColorField(picker, color);
            HSBColor hSBColor = HSBColor.FromColor(color);
            Vector2 a = new Vector2(hSBColor.s * HSBField.width, (1f - hSBColor.b) * HSBField.height);
            indicator.relativePosition = a - indicator.size * 0.5f;
            if (!(HSBField.renderMaterial is null)) {
                HSBField.renderMaterial.color = picker.hue.gamma;
            }
            Vector2 vector = new Vector2(indicator.relativePosition.x + indicator.size.x * 0.5f, indicator.relativePosition.y + indicator.size.y * 0.5f);
            SetPickerColorField(picker, GetColor(vector.x, vector.y, HSBField.width, HSBField.height, picker.hue));
        }

        internal static void AddPropPainterBtn(UIToolOptionPanel optionPanel, UIButton moreTools, UIPanel mtpBackGround, UIPanel mtpContainer) {
            SimulationManager smInstance = Singleton<SimulationManager>.instance;
            PropInstance[] props = Singleton<PropManager>.instance.m_props.m_buffer;
            Color[] colors = m_colors;
            UIColorField field = Object.Instantiate(UITemplateManager.Get<UIPanel>("LineTemplate").Find<UIColorField>("LineColor"));
            field.isVisible = true;
            field.name = COLORFIELD_NAME;
            UIColorPicker picker = Object.Instantiate(field.colorPicker);

            optionPanel.AttachUIComponent(picker.gameObject);
            picker.color = Color.white;
            picker.name = COLORPICKER_NAME;

            UIPanel pickerPanel = picker.component as UIPanel;
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

            /* Finally attach all delegates */
            IEnumerator ProcessEvent(HashSet<Instance> selections) {
                yield return null;
                if (!(selections is null) && selections.Count > 0) {
                    foreach (var selection in selections) {
                        uint propID = selection.id.Prop;
                        if (selection.isValid && !selection.id.IsEmpty && propID > 0) {
                            colors[propID] = picker.color;
                        }
                    }
                }
            }

            picker.eventColorUpdated += (color) => {
                smInstance.AddAction(ProcessEvent(Action.selection));
            };

            picker.m_HSBField.eventClicked += (c, p) => {
                HashSet<Instance> selections = Action.selection;
                if (!(selections is null) && selections.Count > 0) {
                    foreach (var selection in selections) {
                        uint propID = selection.id.Prop;
                        if (selection.isValid && !selection.id.IsEmpty && propID > 0) {
                            colors[propID] = picker.color;
                        }
                    }
                }
            };

            ActionAddHandler = (selections) => {
                smInstance.AddAction(() => {
                    foreach (var selection in selections) {
                        uint propID = selection.id.Prop;
                        if (selection.isValid && !selection.id.IsEmpty && propID > 0) {
                            Color color = colors[propID];
                            if (color.r == 0f && color.g == 0f && color.b == 0f && color.a == FALSEALPHASIG) {
                                Randomizer randomizer = new Randomizer(propID);
                                SetPickerColor(picker, props[propID].Info.GetColor(ref randomizer));
                            } else {
                                SetPickerColor(picker, color);
                            }
                            break;
                        }
                    }
                });
            };

            ActionCloneHandler = (clonedOrigin) => {
                smInstance.AddAction(() => {
                    foreach (KeyValuePair<Instance, Instance> x in clonedOrigin) {
                        if (x.Key.id.Type != InstanceType.Prop) return;
                        Color originColor = colors[x.Key.id.Prop];
                        if (originColor.r != 0f && originColor.g != 0f && originColor.b != 0f && originColor.a != FALSEALPHASIG) {
                            colors[x.Value.id.Prop] = originColor;
                        }
                    }
                });
            };
        }
    }
}
