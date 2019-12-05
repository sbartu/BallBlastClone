using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class InputData 
{
    public float gravity;
    public int bullet_count_increase;
    public int bullet_damage_increase;
	public List<Level> levels;

    public static InputData CreateFromJSON()
    {
		StreamReader reader = new StreamReader("level.json");
        string json = reader.ReadToEnd();
        return JsonUtility.FromJson<InputData>(json);
    }
}

[System.Serializable]
public class Level {
	public List<Ball> balls;
    
    public Level(List<Ball> ballsIn) 
    {
        balls = ballsIn;
    }
}

[System.Serializable]
public class Ball {
	public int hp;
	public List<int> splits;
	public float delay;

    public Ball(int hpIn, List<int> splitsIn, float delayIn) 
    {
        hp = hpIn;
        splits = splitsIn;
        delay = delayIn;
    }
}