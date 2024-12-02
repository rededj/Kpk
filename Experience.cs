using System;
using System.Windows.Controls;
using System.Windows.Shapes;

public class Experience
{
    public int CurrentExperience { get; set; } = 0;
    public int ExperienceToNextLevel { get; private set; } = 200;
    public int Level { get; set; } = 1;
    public int maxLevel { get; private set; } = 25;
    private Rectangle experienceBar;
    public event Action LevelUp;

    public Experience(Rectangle bar)
    {
        experienceBar = bar;
    }
    public void AddExperience(int amount, TextBlock levelText)
    {
        CurrentExperience += amount;
        CheckLevelUp(levelText);
        UpdateExperienceBar();
    }

    private void CheckLevelUp(TextBlock levelText)
    {
        while (CurrentExperience >= ExperienceToNextLevel)
        {
            CurrentExperience -= ExperienceToNextLevel;
            Level++;
            ExperienceToNextLevel += 150;
            levelText.Text = $"LVL {Level}";
            LevelUp?.Invoke();
        }
    }

    public void UpdateExperienceBar()
    {
        double experiencePercentage = (double)CurrentExperience / ExperienceToNextLevel;
        experienceBar.Width = 1408 * experiencePercentage;
    }

    public void Reset(TextBlock levelText)
    {
        CurrentExperience = 0;
        Level = 1;
        ExperienceToNextLevel = 200;
        UpdateExperienceBar();
        levelText.Text = "LVL 1";
    }
}