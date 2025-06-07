namespace Vizualizr.Backend.Registry
{
    public class Registry : Dictionary<Enum, object>, IRegistry
    {
        public static readonly Registry Shared = CreateNewRegistry();
        
        /**
         * Instantiates a new registry with default values.
         */
        public static Registry CreateNewRegistry()
        {
            return new Registry()
            {
                { RegistryKey.I_Waveform_SamplesPerPixel, 2048 },
                { RegistryKey.I_Waveform_RenderType, 0 },
            };
        }


        public int ReadInt(RegistryKey key)
        {
            if (TryGetValue(key, out object value))
            {
                return (int)value;
            }
            else
            {
                return 0;
            }
        }

        public float ReadFloat(RegistryKey key)
        {
            if (TryGetValue(key, out object value))
            {
                return (float)value;
            }
            else
            {
                return 0f;
            }
        }

        public bool ReadBool(RegistryKey key)
        {
            if (TryGetValue(key, out object value))
            {
                return (bool)value;
            }
            else
            {
                return false;
            }
        }

        public string ReadString(RegistryKey key)
        {
            if (TryGetValue(key, out object value))
            {
                return (string)value;
            }
            else
            {
                return string.Empty;
            }
        }
    }
}