using UnityEngine;
namespace CubeInvasion.Prototype
{
    public static class MobileHaptics
    {
        static float last;
        public static void Play(CombatFeedback.Cue cue)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if(PlayerPrefs.GetInt("CI.Haptics",0)!=1||Time.unscaledTime-last<.08f)return;
            bool hit=cue==CombatFeedback.Cue.Hit;bool strong=cue==CombatFeedback.Cue.Fail;bool medium=cue==CombatFeedback.Cue.Hurt||cue==CombatFeedback.Cue.Explosion||cue==CombatFeedback.Cue.BossSlam||cue==CombatFeedback.Cue.Unlock||cue==CombatFeedback.Cue.Upgrade;
            if(!hit&&!strong&&!medium)return;last=Time.unscaledTime;
            try
            {
                using(var unity=new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using(var activity=unity.GetStatic<AndroidJavaObject>("currentActivity"))
                using(var vibrator=activity.Call<AndroidJavaObject>("getSystemService","vibrator"))
                {
                    if(!vibrator.Call<bool>("hasVibrator"))return;
                    using(var version=new AndroidJavaClass("android.os.Build$VERSION"))
                    {
                        long[] pattern=strong?new long[]{0,60,50,90}:new long[]{0,hit?8:28};
                        if(version.GetStatic<int>("SDK_INT")>=26)using(var effects=new AndroidJavaClass("android.os.VibrationEffect"))using(var effect=effects.CallStatic<AndroidJavaObject>("createWaveform",pattern,-1))vibrator.Call("vibrate",effect);
                        else vibrator.Call("vibrate",pattern,-1);
                    }
                }
            }
            catch(System.Exception) {Handheld.Vibrate();}
#endif
        }
    }
}
