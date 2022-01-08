using ColossalFramework;
using ColossalFramework.IO;
using ICities;
using System;
using System.IO;
using System.Threading;
using UnityEngine;

namespace PropPainter {
    public sealed class PropPainterDataContainer : IDataContainer {
        public void AfterDeserialize(DataSerializer s) { }

        public unsafe void Deserialize(DataSerializer s) {
            Color[] colors = PPManager.ColorBuffer;
            Color defColor = new Color(0, 0, 0, 1f / 255f);
            uint len = s.ReadUInt24();
            if (len == colors.Length) {
                fixed (Color* pColor = &PPManager.ColorBuffer[0]) {
                    Color* c = pColor;
                    for (int i = 0; i < colors.Length; i++, c++) {
                        byte a = (byte)s.ReadUInt8();
                        byte r = (byte)s.ReadUInt8();
                        byte g = (byte)s.ReadUInt8();
                        byte b = (byte)s.ReadUInt8();
                        if (a != 0x01 && r != 0x00 && g != 0x00 && b != 0x00) {
                            *c = new Color32(r, g, b, 255);
                        } else {
                            *c = defColor;
                        }
                    }
                }
            }
        }

        public void Serialize(DataSerializer s) {
            Color[] colors = PPManager.ColorBuffer;
            PropInstance[] props = Singleton<PropManager>.instance.m_props.m_buffer;
            s.WriteUInt24(PropManager.MAX_PROP_COUNT);
            for (int i = 0; i < props.Length; i++) {
                if (props[i].m_flags != 0) {
                    Color32 color = colors[i];
                    s.WriteUInt8(color.a);
                    s.WriteUInt8(color.r);
                    s.WriteUInt8(color.g);
                    s.WriteUInt8(color.b);
                } else {
                    s.WriteUInt32(16777216u);
                }
            }
        }
    }

    public sealed class PPSerializer : ISerializableDataExtension {
        private const string PROPPAINTERID = @"PropPainter";

        public void OnCreated(ISerializableData serializedData) { }

        private static Type PropPainterLegacyHandler(string _) => typeof(PropPainterDataContainer);

        public void OnLoadData() {
            if (ToolManager.instance.m_properties.m_mode == ItemClass.Availability.Game) {
                SimulationManager smInstance = Singleton<SimulationManager>.instance;
                if (smInstance.m_serializableDataStorage.TryGetValue(PROPPAINTERID, out byte[] data)) {
                    using (MemoryStream ms = new MemoryStream(data)) {
                        var s = DataSerializer.Deserialize<PropPainterDataContainer>(ms, DataSerializer.Mode.Memory, PropPainterLegacyHandler);
                    }
                } else {
                    Color[] colors = PPManager.ColorBuffer;
                    Color defColor = new Color(0, 0, 0, PPManager.FALSEALPHASIG);
                    for (int i = 0; i < colors.Length; i++) {
                        colors[i] = defColor;
                    }
                }
            }
        }

        public void OnReleased() { }

        public void OnSaveData() {
            try {
                byte[] data;
                using (var stream = new MemoryStream()) {
                    DataSerializer.Serialize(stream, DataSerializer.Mode.Memory, 1, new PropPainterDataContainer());
                    data = stream.ToArray();
                }
                SaveData(PROPPAINTERID, data);
            } catch (Exception e) {
                UnityEngine.Debug.LogException(e);
            }
        }

        private void SaveData(string id, byte[] data) {
            SimulationManager smInstance = Singleton<SimulationManager>.instance;
            while (!Monitor.TryEnter(smInstance.m_serializableDataStorage, SimulationManager.SYNCHRONIZE_TIMEOUT)) { }
            try {
                smInstance.m_serializableDataStorage[id] = data;
            } finally {
                Monitor.Exit(smInstance.m_serializableDataStorage);
            }
        }
    }
}
