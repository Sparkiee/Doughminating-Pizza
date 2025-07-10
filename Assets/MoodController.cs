using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoodController : MonoBehaviour
{
    public SpriteRenderer moodRenderer;
    public Sprite[] moodFaces;

    public void setPatience(float patience)
    {
        int index = Mathf.Clamp(Mathf.FloorToInt(patience * (moodFaces.Length)), 0, moodFaces.Length - 1);
        moodRenderer.sprite = moodFaces[index];
    }
}
