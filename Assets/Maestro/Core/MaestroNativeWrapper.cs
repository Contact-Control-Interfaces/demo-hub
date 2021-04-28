using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

static class MaestroNativeWrapper
{
    /**
     * Force-feedback endpoints
     */
    [DllImport("MaestroAPI")]
    public static extern void set_thumb_motor_amplitude(IntPtr maestroPtr, byte amplitude);

    [DllImport("MaestroAPI")]
    public static extern void set_index_motor_amplitude(IntPtr maestroPtr, byte amplitude);

    [DllImport("MaestroAPI")]
    public static extern void set_middle_motor_amplitude(IntPtr maestroPtr, byte amplitude);

    [DllImport("MaestroAPI")]
    public static extern void set_ring_motor_amplitude(IntPtr maestroPtr, byte amplitude);

    [DllImport("MaestroAPI")]
    public static extern void set_little_motor_amplitude(IntPtr maestroPtr, byte amplitude);


    /**
     * Vibration endpoints
     */
    [DllImport("MaestroAPI")]
    public static extern void set_thumb_vibration_effect(IntPtr maestroPtr, byte amplitude);

    [DllImport("MaestroAPI")]
    public static extern void set_index_vibration_effect(IntPtr maestroPtr, byte amplitude);

    [DllImport("MaestroAPI")]
    public static extern void set_middle_vibration_effect(IntPtr maestroPtr, byte amplitude);

    [DllImport("MaestroAPI")]
    public static extern void set_ring_vibration_effect(IntPtr maestroPtr, byte amplitude);

    [DllImport("MaestroAPI")]
    public static extern void set_little_vibration_effect(IntPtr maestroPtr, byte amplitude);

    public delegate void MaestroMutator(IntPtr maestroPtr, byte amplitude);

    public static void SetAllAmplitudes(IntPtr maestroPtr, byte amplitude)
    {
        set_thumb_motor_amplitude(maestroPtr, amplitude);
        set_index_motor_amplitude(maestroPtr, amplitude);
        set_middle_motor_amplitude(maestroPtr, amplitude);
        set_ring_motor_amplitude(maestroPtr, amplitude);
        set_little_motor_amplitude(maestroPtr, amplitude);
    }

    public static void SetAllVibrationEffects(IntPtr maestroPtr, byte effect)
    {
        set_thumb_vibration_effect(maestroPtr, effect);
        set_index_vibration_effect(maestroPtr, effect);
        set_middle_vibration_effect(maestroPtr, effect);
        set_ring_vibration_effect(maestroPtr, effect);
        set_little_vibration_effect(maestroPtr, effect);
    }

    public static void SetAllHaptics(IntPtr maestroPtr, byte amplitude, byte vibrationEffect)
    {
        SetAllAmplitudes(maestroPtr, amplitude);
        SetAllVibrationEffects(maestroPtr, vibrationEffect);
    }

    public static void SetHapticsFromContexts(IntPtr maestroPtr, MaestroHapticContext context, MaestroHapticContext lastContext)
    {
        // All amplitudes
        TrySetMutator(maestroPtr, set_thumb_motor_amplitude, context.ThumbAmplitude, lastContext.ThumbAmplitude);
        TrySetMutator(maestroPtr, set_index_motor_amplitude, context.IndexAmplitude, lastContext.IndexAmplitude);
        TrySetMutator(maestroPtr, set_middle_motor_amplitude, context.MiddleAmplitude, lastContext.MiddleAmplitude);
        TrySetMutator(maestroPtr, set_ring_motor_amplitude, context.RingAmplitude, lastContext.RingAmplitude);
        TrySetMutator(maestroPtr, set_little_motor_amplitude, context.LittleAmplitude, lastContext.LittleAmplitude);

        // All vibration effects
        TrySetMutator(maestroPtr, set_thumb_vibration_effect, context.ThumbVibrationEffect, lastContext.ThumbVibrationEffect);
        TrySetMutator(maestroPtr, set_index_vibration_effect, context.IndexVibrationEffect, lastContext.IndexVibrationEffect);
        TrySetMutator(maestroPtr, set_middle_vibration_effect, context.MiddleVibrationEffect, lastContext.MiddleVibrationEffect);
        TrySetMutator(maestroPtr, set_ring_vibration_effect, context.RingVibrationEffect, lastContext.RingVibrationEffect);
        TrySetMutator(maestroPtr, set_little_vibration_effect, context.LittleVibrationEffect, lastContext.LittleVibrationEffect);
    }

    private static void TrySetMutator(IntPtr maestroPtr, MaestroMutator mutator, byte? value, byte? previousValue)
    {
        if (value.HasValue) {
            // Send value if there's one to send
            mutator(maestroPtr, value.Value);
        } else if (previousValue.HasValue) {
            // Turn off haptics if there's no longer a value
            mutator(maestroPtr, 0);
        }
    }
}
