using System;
using System.Collections.Generic;
using System.Linq;

namespace XephTools
{
    public sealed class OverTime
    {
        private static OverTime instance = null;

        internal static OverTime Instance
        {
            get
            {
                if (instance == null)
                    instance = new OverTime();
                return instance;
            }
        }

        public abstract class ModuleBase
        {
            public float Progress { get; set; }

            private Action competeAction;
            private DateTime startTime;
            private float invLength = 0f;

            internal abstract void Update();

            public void OnComplete(Action action)
            {
                competeAction = action;
            }

            public void End(float endProgress = 1f, bool callOnComplete = false)
            {
                Progress = endProgress;
                if (!callOnComplete)
                    competeAction = null;
            }

            internal void Tick(DateTime currTime)
            {
                if (IsExpired())
                    return;

                float elapsed = (float)(currTime - startTime).TotalSeconds;
                Progress = elapsed * invLength;
                if (Progress > 1f)
                    Progress = 1f;
            }

            internal bool IsExpired()
            {
                return Progress >= 1f;
            }

            internal void InvokeCompleteAction()
            {
                try
                {
                    competeAction?.Invoke();
                }
                catch(Exception) { } // GameObject has been destroyed
            }

            protected void Init(float length)
            {
                invLength = 1f / length;
            }

            public void Start(DateTime startTime)
            {
                this.startTime = startTime;
            }
        }

        public class LerpModule : ModuleBase
        {
            private readonly float start;
            private readonly float end;
            private readonly Action<float> setter;

            public LerpModule(float start, float end, float length, Action<float> setter)
            {
                this.start = start;
                this.end = end;
                this.setter = setter;
                Init(length);
            }

            internal override void Update()
            {
                 setter(start + Progress * (end - start));
            }
        }

        public class SquareLerpModule : ModuleBase
        {
            private readonly float start;
            private readonly float end;
            private readonly float weight;
            private readonly Action<float> setter;

            public SquareLerpModule(float start, float end, float weight, float length, Action<float> setter)
            {
                this.start = start;
                this.end = end;
                this.weight = weight;
                this.setter = setter;
                Init(length);
            }

            internal override void Update()
            {
                float a = start + Progress * (weight - start);
                float b = weight + Progress * (end - weight);
                setter(a + Progress * (b - a));
            }
        }

        public class CubicLerpModule : ModuleBase
        {
            private readonly float start;
            private readonly float end;
            private readonly float weightStart;
            private readonly float weightEnd;
            private readonly Action<float> setter;

            public CubicLerpModule(float start, float end, float weightStart, float weightEnd, float length, Action<float> setter)
            {
                this.start = start;
                this.end = end;
                this.weightStart = weightStart;
                this.weightEnd = weightEnd;
                this.setter = setter;
                Init(length);
            }

            internal override void Update()
            {
                float a = start + Progress * (weightStart - start);
                float b = weightStart + Progress * (weightEnd - weightStart);
                float c = weightEnd + Progress * (end - weightEnd);

                float d = a + Progress * (b - a);
                float e = b + Progress * (c - b);

                setter(d + Progress * (e - d));
            }
        }

        public class ModuleReference<T> where T : ModuleBase
        {
            private readonly uint id;

            public ModuleReference(uint id)
            {
                this.id = id;
            }

            public bool IsExpired()
            {
                return !OverTime.Instance.modules.ContainsKey(id);
            }

            public T Get()
            {
                if (IsExpired())
                    throw new Exception($"OverTime Module with id {id} expired.");

                return (T)OverTime.Instance.modules[id];
            }
        }

        private readonly Dictionary<uint, ModuleBase> modules = new Dictionary<uint, ModuleBase>();
        private readonly List<uint> activeProcesses = new List<uint>();
        private readonly List<DelayedModule> delayedProcesses = new List<DelayedModule>();

        private struct DelayedModule
        {
            public DateTime expiryTime;
            public uint id;
            public Action startAction;
        }

        public static void Update()
        {
            DateTime now = DateTime.UtcNow;
            OverTime instance = Instance;

            foreach (DelayedModule delayed in instance.delayedProcesses.Where(d => now >= d.expiryTime).ToList())
            {
                delayed.startAction?.Invoke();
                instance.activeProcesses.Add(delayed.id);
                instance.delayedProcesses.Remove(delayed);
            }

            foreach (uint id in instance.activeProcesses.ToList())
            {
                ModuleBase module = instance.modules[id];
                module.Tick(now);
                try
                {
                    module.Update();
                }
                catch (Exception) // Accessing Destroyed Object
                {
                    instance.modules.Remove(id);
                    instance.activeProcesses.Remove(id);
                }

                if (module.IsExpired())
                {
                    module.InvokeCompleteAction();
                    instance.modules.Remove(id);
                    instance.activeProcesses.Remove(id);
                }
            }
        }

        public static ModuleReference<T> Add<T>(T mod, float delay = 0f, Action startAction = null) where T : ModuleBase
        {
            Random rand = new Random();
            uint id = (uint)rand.Next(int.MinValue, int.MaxValue);

            OverTime instance = Instance;
            instance.modules[id] = mod;
            mod.Start(DateTime.UtcNow);

            if (delay <= 0f)
            {
                instance.activeProcesses.Add(id);
            }
            else
            {
                DelayedModule delayedModule = new DelayedModule
                {
                    expiryTime = DateTime.UtcNow.AddSeconds(delay),
                    id = id,
                    startAction = startAction
                };

                instance.delayedProcesses.Add(delayedModule);
            }

            return new ModuleReference<T>(id);
        }
    }
}
