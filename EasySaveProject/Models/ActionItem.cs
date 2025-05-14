namespace EasySaveProject.Models;

public class ActionItem
{
    public string name { get; set; }
    public ActionItem(string actionName) => name = actionName;
}