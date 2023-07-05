using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TendonLength : MonoBehaviour
{
    //public float TotalLength => BoneLength + JointLength;

    // Length of the hand bones themselves, this should be constant
    private float BoneLength;

    /// <summary>
    /// Calculates the length of the tendon running along each joint.
    /// This method takes a variable number of rotations to account for the thumb.
    /// </summary>
    /// <param name="radius">The radius, i.e. half the hand thickness</param>
    /// <param name="rotations">The angles for each joint in radians</param>
    /// <returns>The total length of the tendon along each joint</returns>
    public float JointLength(float radius, params float[] rotations)
    {
        /*float sum = 0f;
        foreach (float rotation in rotations) {
            sum += ArcLength(radius, rotation);
        }
        return sum;*/

        return rotations.Aggregate(0f, (acc, next) => acc + ArcLength(radius, next));
    }
    
    /// <summary>
    /// Computes arc length given a radius and an angle in radians.
    /// </summary>
    /// <param name="radius">The radius of the circle</param>
    /// <param name="angleInRads">The angle of the arc in radians</param>
    /// <returns></returns>
    private float ArcLength(float radius, float angleInRads) => radius * angleInRads;
}
