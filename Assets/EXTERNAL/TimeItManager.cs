using System;
using System.Collections.Generic;
using UnityEngine;

namespace XephTools
{
    public class TimeIt
    {
        float _time = 0;
        bool _isUnscaled;

        Action onComplete;

        public TimeIt OnComplete(Action action)
        {
            onComplete = action;
            return this;
        }

        public TimeIt SetDuration(float time)
        {
            _time = time;
            return this;
        }

        public TimeIt SetIsUnscaled(bool isUnscaled)
        {
            _isUnscaled = isUnscaled;
            return this;
        }

        public void Abort(bool callOnComplete = false)
        {
            _time = 0f;
            TimeItManager.RemoveTimer(this);
            if (callOnComplete)
            {
                try
                {
                    onComplete?.Invoke();
                }
                catch (Exception) { } // GameObject no longer available
            }
        }

        public bool isExpired { get { return _time <= 0f; } }
        public float timeLeft { get { return _time; } }

        public void Start()
        {
            TimeItManager.AddTimer(this);
        }

        // To be called by TimeItManager
        internal void Update(float dt, float udt)
        {
            float delta = (_isUnscaled) ? udt : dt;
            _time -= delta;
            if (_time <= 0f)
            {
                Abort(true);
            }
        }
    }

    public class TimeItManager : MonoBehaviour
    {
        static TimeItManager instance = null;
        List<TimeIt> _timers = new();
        List<TimeIt> _removeBuffer = new();

        internal static void AddTimer(TimeIt timer)
        {
            if (instance == null)
            {
                GameObject go = Instantiate(new GameObject());
                instance = go.AddComponent<TimeItManager>();
                DontDestroyOnLoad(go);
            }

            if (instance._timers.Contains(timer))
            {
                Debug.LogWarning("TimeIt already added");
                return;
            }
            instance._timers.Add(timer);
        }
        internal static void RemoveTimer(TimeIt timer)
        {
            if (instance == null)
            {
                Debug.LogError("TimeItManager not in scene. Add a TimeIt Object");
            }
            if (!instance._timers.Contains(timer))
            {
                Debug.LogWarning("TimeIt not found to remove");
                return;
            }
            instance._removeBuffer.Add(timer);
        }

        private void Update()
        {
            foreach (TimeIt timer in _timers)
            {
                timer.Update(Time.deltaTime, Time.unscaledDeltaTime);
            }

            foreach (TimeIt timer in _removeBuffer)
            {
                _timers.Remove(timer);
            }
            _removeBuffer.Clear();
        }
    }
}
